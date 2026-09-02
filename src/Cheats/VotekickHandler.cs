using Hazel;
using InnerNet;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SkidMenu;

public static class VotekickHandler
{
    public static bool VotekickAllEnabled = false;
    public static bool AutoPurgeImpostors = false;
    public static bool AutoPurgeCrew = false;
    public static bool AutoPurgeHost = false;
    public static bool AutoRetaliate = false;
    public static bool FinishTheKick = false;
    public static bool ShowVotekickInfo = false;
    public static bool IgnoreOwnVotekicks = false;
    public static bool NotifyVotekickInfo = false;
    public static bool AutoRejoinEnabled = false;
    public static bool AutoRejoinVotekickAll = false;
    public static bool AutoRejoinVotekickHost = false;
    public static float RejoinDelay = 1f;
    public static int SelectedTargetId = -1;
    public static int VoteCount = 3;
    public static float AutoKickInterval = 3f;

    public static int RejoinCount = 0;
    public static string LastGameCode = "";

    public static readonly Dictionary<int, HashSet<int>> UniqueVoters = new();

    private static readonly HashSet<int> _votekickedIds = new();
    private static readonly HashSet<int> _perPlayerKickedIds = new();
    private static readonly HashSet<int> _knownPlayerIds = new();
    private static readonly HashSet<int> _perPlayerAutoKick = new();
    private static float _autoKickTimer = 0f;
    private static float _perPlayerKickTimer = 0f;
    private const float JoinKickDelay = 0.25f;
    private static bool _pendingFastKick = false;
    private static bool _isRejoining = false;

    public static int VotekickedCount => _votekickedIds.Count;

    private static void ShowInfo(string src, string tgt)
    {
        string msg = $"<color=orange>[Votekick Info]</color> <color=blue>{src}</color> voted to kick <color=red>{tgt}</color>";
        if (ShowVotekickInfo && PlayerControl.LocalPlayer != null)
            DestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, msg, true);
        if (NotifyVotekickInfo)
            SkidMenu.notifications.Send("Votekick Info", $"{src} voted to kick {tgt}", 5f);
    }

    public static void VotekickTarget()
    {
        if (SelectedTargetId == -1) return;
        try
        {
            string myName = PlayerControl.LocalPlayer?.Data?.DefaultOutfit?.PlayerName ?? "Me";
            string tgtName = AmongUsClient.Instance?.GetClient(SelectedTargetId)?.PlayerName ?? $"Client {SelectedTargetId}";
            if (VoteBanSystem.Instance == null) return;
            for (int i = 0; i < VoteCount; i++)
                VoteBanSystem.Instance.CmdAddVote(SelectedTargetId);
            if (!IgnoreOwnVotekicks) ShowInfo(myName, tgtName);
            else SkidMenu.notifications.Send("Votekick", $"Sent {VoteCount} vote(s) against {tgtName}.", 4f);
        }
        catch { }
    }

    public static void VotekickPlayer(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (VoteBanSystem.Instance == null) return;
        try
        {
            string myName = PlayerControl.LocalPlayer?.Data?.DefaultOutfit?.PlayerName ?? "Me";
            string tgtName = player.Data.DefaultOutfit.PlayerName;
            for (int i = 0; i < VoteCount; i++)
                VoteBanSystem.Instance.CmdAddVote(player.Data.ClientId);
        }
        catch { }
    }

    public static void TogglePerPlayerAutoKick(int clientId)
    {
        if (!_perPlayerAutoKick.Remove(clientId))
            _perPlayerAutoKick.Add(clientId);
    }

    public static bool IsPerPlayerAutoKickEnabled(int clientId) => _perPlayerAutoKick.Contains(clientId);

    private static bool _isProcessingVote = false;
    private static Coroutine _staggerCoroutine = null;

    public static void VotekickAllNow()
    {
        if (VoteBanSystem.Instance == null) return;
        if (_staggerCoroutine != null) return;
        var targets = new List<int>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.AmOwner || player.Data == null) continue;
            int clientId = player.Data.ClientId;
            if (!VoteBanSystem.Instance.HasMyVote(clientId)) targets.Add(clientId);
        }
        if (targets.Count == 0) return;
        _staggerCoroutine = AmongUsClient.Instance.StartCoroutine(StaggerVotes(targets, false).WrapToIl2Cpp());
    }

    // One-shot "vote everyone, then leave and rejoin". The stagger MUST finish before we
    // exit, otherwise ExitGame nukes VoteBanSystem mid-vote and only the first target
    // ever gets a vote.
    public static void VotekickAllAndRejoin()
    {
        if (VoteBanSystem.Instance == null) return;
        ResetTracking();
        VotekickAllNow();
        if (_staggerCoroutine != null && AmongUsClient.Instance != null)
            AmongUsClient.Instance.StartCoroutine(RejoinAfterStagger().WrapToIl2Cpp());
    }

    public static void VotekickAll()
    {
        if (!VotekickAllEnabled) return;
        _autoKickTimer += Time.deltaTime;
        float threshold = _pendingFastKick ? JoinKickDelay : AutoKickInterval;
        if (_autoKickTimer < threshold) return;
        _autoKickTimer = 0f;
        _pendingFastKick = false;
        _votekickedIds.Clear();
        if (_staggerCoroutine != null) return;
        if (VoteBanSystem.Instance == null) return;
        var targets = new List<int>();
        string myName = PlayerControl.LocalPlayer?.Data?.DefaultOutfit?.PlayerName ?? "Me";
        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.AmOwner || player.Data == null) continue;
            int clientId = player.Data.ClientId;
            if (_votekickedIds.Contains(clientId)) continue;
            if (VoteBanSystem.Instance.HasMyVote(clientId)) continue;
            _votekickedIds.Add(clientId);
            targets.Add(clientId);
            if (!IgnoreOwnVotekicks) ShowInfo(myName, player.Data.DefaultOutfit?.PlayerName ?? "?");
        }
        if (targets.Count == 0) return;
        _staggerCoroutine = AmongUsClient.Instance.StartCoroutine(StaggerVotes(targets, true).WrapToIl2Cpp());
        if (AutoRejoinVotekickAll)
            AmongUsClient.Instance.StartCoroutine(RejoinAfterStagger().WrapToIl2Cpp());
    }

    private static IEnumerator RejoinAfterStagger()
    {
        while (_staggerCoroutine != null) yield return null;
        // Tiny fixed buffer so the last votes register before we leave.
        yield return new WaitForSeconds(0.25f);
        RejoinGame();
    }

    private static IEnumerator StaggerVotes(List<int> targets, bool multi)
    {
        foreach (int clientId in targets)
        {
            if (VoteBanSystem.Instance == null) break;
            int count = multi ? VoteCount : 1;
            for (int i = 0; i < count; i++)
                VoteBanSystem.Instance.CmdAddVote(clientId);
            yield return null;
        }
        _staggerCoroutine = null;
    }

    public static void TickPerPlayerAutoKick()
    {
        if (_perPlayerAutoKick.Count == 0) return;
        if (VoteBanSystem.Instance == null) return;
        _perPlayerKickTimer += Time.deltaTime;
        if (_perPlayerKickTimer < AutoKickInterval) return;
        _perPlayerKickTimer = 0f;
        _perPlayerKickedIds.Clear();
        try
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (player == null || player.AmOwner || player.Data == null) continue;
                int clientId = player.Data.ClientId;
                if (!_perPlayerAutoKick.Contains(clientId)) continue;
                if (_perPlayerKickedIds.Contains(clientId)) continue;
                if (VoteBanSystem.Instance.HasMyVote(clientId)) continue;
                string myName = PlayerControl.LocalPlayer?.Data?.DefaultOutfit?.PlayerName ?? "Me";
                VoteBanSystem.Instance.CmdAddVote(clientId);
                _perPlayerKickedIds.Add(clientId);
                if (!IgnoreOwnVotekicks) ShowInfo(myName, player.Data.DefaultOutfit.PlayerName);
            }
        }
        catch { }
    }

    private static float _conditionKickTimer = 0f;
    public static void TickConditionKicks()
    {
        bool anyEnabled = AutoPurgeImpostors || AutoPurgeCrew || AutoPurgeHost || AutoRetaliate;
        if (!anyEnabled || VoteBanSystem.Instance == null) return;
        _conditionKickTimer += Time.deltaTime;
        if (_conditionKickTimer < AutoKickInterval) return;
        _conditionKickTimer = 0f;

        string myName = PlayerControl.LocalPlayer?.Data?.DefaultOutfit?.PlayerName ?? "Me";
        int myClientId = AmongUsClient.Instance.ClientId;
        bool hostVoted = false;

        try
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (player == null || player.AmOwner || player.Data == null) continue;
                int clientId = player.Data.ClientId;
                if (VoteBanSystem.Instance.HasMyVote(clientId)) continue;
                bool shouldKick = false;

                if (AutoPurgeImpostors && player.Data.Role != null && player.Data.Role.IsImpostor)
                    shouldKick = true;
                if (AutoPurgeCrew && player.Data.Role != null && !player.Data.Role.IsImpostor)
                    shouldKick = true;
                if (AutoPurgeHost && player.OwnerId == AmongUsClient.Instance.HostId)
                    shouldKick = true;
                if (AutoRetaliate && UniqueVoters.TryGetValue(myClientId, out var voters) && voters != null && voters.Contains(clientId))
                    shouldKick = true;

                if (!shouldKick) continue;
                VoteBanSystem.Instance.CmdAddVote(clientId);
                if (player.OwnerId == AmongUsClient.Instance.HostId) hostVoted = true;
                if (!IgnoreOwnVotekicks) ShowInfo(myName, player.Data.DefaultOutfit.PlayerName);
            }
        }
        catch { }

        if (AutoRejoinVotekickHost && hostVoted)
            AmongUsClient.Instance?.StartCoroutine(RejoinAfterHostVote().WrapToIl2Cpp());
    }

    private static IEnumerator RejoinAfterHostVote()
    {
        // Small buffer so the host vote registers client-side before we leave.
        yield return new WaitForSeconds(0.25f);
        RejoinGame();
    }

    public static void CheckForNewPlayers()
    {
        if (!VotekickAllEnabled) return;
        try
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (player == null || player.AmOwner || player.Data == null) continue;
                int clientId = player.Data.ClientId;
                if (_knownPlayerIds.Contains(clientId)) continue;
                _knownPlayerIds.Add(clientId);
                _pendingFastKick = true;
                _autoKickTimer = 0f;
            }
        }
        catch { }
    }

    public static void ResetTracking()
    {
        _votekickedIds.Clear();
        _perPlayerKickedIds.Clear();
        _knownPlayerIds.Clear();
        _perPlayerAutoKick.Clear();
        UniqueVoters.Clear();
        _pendingFastKick = false;
    }

    public static void RejoinGame()
    {
        if (_isRejoining) return;
        if (AmongUsClient.Instance == null) return;
        if (SkidMenu.menuUI == null) return;

        // Always resolve the code from the live game id so this works whether
        // we host or joined. GameId is a public int field on InnerNetClient and
        // is populated the moment a room exists - no string callback needed.
        int liveGameId = AmongUsClient.Instance.GameId;
        string code = LastGameCode;
        if (liveGameId != 0)
        {
            try { code = GameCode.IntToGameName(liveGameId); }
            catch { }
        }
        if (string.IsNullOrEmpty(code))
        {
            SkidMenu.Log.LogMessage("[Rejoin] no game code available, cannot rejoin");
            SkidMenu.notifications?.Send("Rejoin", "<color=#ff4444>No game code - cannot rejoin</color>", 4f);
            return;
        }
        LastGameCode = code;
        _isRejoining = true;
        // Host the coroutine on the persistent menu component, NOT on
        // AmongUsClient. ExitGame destroys AmongUsClient and every coroutine it
        // is running, which killed the rejoin at its first yield after we left.
        // menuUI survives the exit, so the coroutine runs to the join.
        SkidMenu.menuUI.StartCoroutine(RejoinCoroutine(code).WrapToIl2Cpp());
    }

    private static IEnumerator RejoinCoroutine(string code)
    {
        try
        {
            if (AmongUsClient.Instance == null || string.IsNullOrEmpty(code)) yield break;

            SkidMenu.notifications?.Send("Rejoin", $"Rejoining {code}...", 3f);

            // 1) Leave the current game/lobby first if we are in one. The game
            //    needs to fully drop the OnlineGame state before a new join.
            //    We do NOT gate the join on the scene reaching a menu - after a
            //    forced host exit the scene often stays on "OnlineGame" for a
            //    long while, so we just wait a short fixed time and join anyway.
            if (IsInGameOrLobby())
            {
                SkidMenu.Log.LogMessage($"[Rejoin] leaving current lobby to rejoin {code}");
                AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                // Just enough for the client to tear down OnlineGame state and
                // start spawning the menu scene. Any longer is pure wasted time;
                // CoJoinOnlineGameFromCode accumulates the join correctly even if
                // the scene is still mid-transition.
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                SkidMenu.Log.LogMessage($"[Rejoin] not in a game, joining {code} directly");
                yield return null;
            }

            // 2) Join by the remembered code. Use the same coroutine the game's
            //    own join-from-code button drives (CoJoinOnlineGameFromCode) and
            //    poll the game state instead of trusting the nested enumerator.
            int gameId;
            try { gameId = GameCode.GameNameToIntV2(code); }
            catch { gameId = 0; }
            if (gameId == 0)
            {
                SkidMenu.Log.LogError($"[Rejoin] could not decode code {code}");
                SkidMenu.notifications?.Send("Rejoin", "<color=#ff4444>Invalid code - cannot rejoin</color>", 4f);
                _isRejoining = false;
                yield break;
            }

            // Hand the join to the game's own coroutine engine exactly like the
            // join-from-code button does. Host it on the persistent plugin
            // component so it isn't killed if AmongUsClient gets recreated while
            // the menu scene loads. OnGameJoined / OnDisconnected patches flip
            // _isRejoining to end this routine.
            SkidMenu.Log.LogMessage($"[Rejoin] firing join for {code}");
            AmongUsClient.Instance.StartCoroutine(
                AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameId, true));

            float settle = 0f;
            while (_isRejoining && settle < 15f)
            {
                yield return null;
                settle += Time.deltaTime;
            }
        }
        finally
        {
            _isRejoining = false;
        }
    }

    private static bool IsInGameOrLobby()
    {
        try
        {
            if (AmongUsClient.Instance == null) return false;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.NotJoined)
                return true;
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return scene != "MainMenu" && scene != "MatchMaking";
        }
        catch { return false; }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    public static class VotekickReset_OnJoin
    {
        public static void Postfix(string gameIdString)
        {
            if (!string.IsNullOrEmpty(gameIdString)) LastGameCode = gameIdString;
            _votekickedIds.Clear();
            _knownPlayerIds.Clear();
            UniqueVoters.Clear();
            _autoKickTimer = 0f;
            _pendingFastKick = true;
            if (_isRejoining)
            {
                RejoinCount++;
                SkidMenu.notifications?.Send("Rejoin", "<color=#88ff88>Rejoined</color>", 3f);
            }
            _isRejoining = false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
    public static class AutoRejoin_OnDisconnect
    {
        public static void Postfix()
        {
            if (!AutoRejoinEnabled || _isRejoining) return;
            if (AmongUsClient.Instance == null) return;

            int liveGameId = AmongUsClient.Instance.GameId;
            string code = liveGameId != 0 ? GameCode.IntToGameName(liveGameId) : LastGameCode;
            if (string.IsNullOrEmpty(code)) return;
            LastGameCode = code;
            _isRejoining = true;
            if (SkidMenu.menuUI != null)
                SkidMenu.menuUI.StartCoroutine(RejoinCoroutine(code).WrapToIl2Cpp());
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), "AddVote")]
    public static class VotekickInfo_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(int srcClient, int clientId)
        {
            if (_isProcessingVote) return;
            if (AmongUsClient.Instance == null) return;
            _isProcessingVote = true;
            try
            {
                if (!UniqueVoters.ContainsKey(clientId)) UniqueVoters[clientId] = new HashSet<int>();
                UniqueVoters[clientId].Add(srcClient);
                if (srcClient == AmongUsClient.Instance.ClientId) return;
                if (FinishTheKick && UniqueVoters[clientId].Count >= 2 && clientId != AmongUsClient.Instance.ClientId)
                    VoteBanSystem.Instance?.CmdAddVote(clientId);
                string src = AmongUsClient.Instance.GetClient(srcClient)?.PlayerName ?? $"Client {srcClient}";
                string tgt = AmongUsClient.Instance.GetClient(clientId)?.PlayerName ?? $"Client {clientId}";
                ShowInfo(src, tgt);
            }
            catch { }
            finally { _isProcessingVote = false; }
        }
    }
}





