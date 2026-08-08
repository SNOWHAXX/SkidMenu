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
    }

    private static IEnumerator StaggerVotes(List<int> targets, bool multi)
    {
        foreach (int clientId in targets)
        {
            if (VoteBanSystem.Instance == null) break;
            int count = multi ? VoteCount : 1;
            for (int i = 0; i < count; i++)
            {
                VoteBanSystem.Instance.CmdAddVote(clientId);
                yield return new WaitForSeconds(0.18f);
            }
            yield return new WaitForSeconds(0.2f);
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
                if (!IgnoreOwnVotekicks) ShowInfo(myName, player.Data.DefaultOutfit.PlayerName);
            }
        }
        catch { }
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
        if (_isRejoining || string.IsNullOrEmpty(LastGameCode)) return;
        if (AmongUsClient.Instance == null) return;
        _isRejoining = true;
        AmongUsClient.Instance.StartCoroutine(RejoinCoroutine(LastGameCode));
    }

    private static IEnumerator RejoinCoroutine(string code)
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.IsGameStarted)
            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        yield return new WaitForSeconds(2f);
        yield return AmongUsClient.Instance.CoFindGameInfoFromCodeAndJoin(GameCode.GameNameToIntV2(code));
        RejoinCount++;
        _isRejoining = false;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    public static class VotekickReset_OnJoin
    {
        public static void Postfix(string gameIdString)
        {
            LastGameCode = gameIdString ?? LastGameCode;
            _votekickedIds.Clear();
            _knownPlayerIds.Clear();
            UniqueVoters.Clear();
            _autoKickTimer = 0f;
            _pendingFastKick = true;
            _isRejoining = false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
    public static class AutoRejoin_OnDisconnect
    {
        public static void Postfix()
        {
            if (!AutoRejoinEnabled || string.IsNullOrEmpty(LastGameCode) || _isRejoining) return;
            if (AmongUsClient.Instance == null) return;
            _isRejoining = true;
            AmongUsClient.Instance.StartCoroutine(RejoinCoroutine(LastGameCode));
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





