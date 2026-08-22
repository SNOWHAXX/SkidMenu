using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkidMenu;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    public static HashSet<byte> votedPlayers = new HashSet<byte>();
    internal static Dictionary<byte, byte> _lastVotes = new Dictionary<byte, byte>();
    internal static Dictionary<byte, byte> _voteCache = new Dictionary<byte, byte>();
    internal static HashSet<byte> _handledDisconnects = new HashSet<byte>();

    public static void ClearAll()
    {
        votedPlayers.Clear();
        _lastVotes.Clear();
        _voteCache.Clear();
        _handledDisconnects.Clear();
    }

    private static void PlaceSilent(MeetingHud hud, NetworkedPlayerInfo voter, int idx, Transform parent)
    {
        hud.BloopAVoteIcon(voter, idx, parent);
        var vs = parent.GetComponent<VoteSpreader>();
        if (vs != null && vs.Votes.Count > 0)
        {
            var icon = vs.Votes[vs.Votes.Count - 1]?.gameObject;
            if (icon != null)
            {
                var anim = icon.GetComponentInParent<Animator>() ?? icon.GetComponent<Animator>();
                if (anim != null)
                    anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
            }
        }
    }

    public static void Prefix(MeetingHud __instance)
    {
        try
        {
        if (CheatToggles.revealVotes && __instance.CurrentState < MeetingHud.MeetingStates.Results)
        {
            foreach (var area in __instance.playerStates)
            {
                if (area == null) continue;
                if (area.VotedForId != PlayerVoteArea.HasNotVoted &&
                    area.VotedForId != PlayerVoteArea.MissedVote &&
                    area.VotedForId != PlayerVoteArea.DeadVote)
                    _voteCache[area.PlayerId] = (byte)area.VotedForId;
            }

            bool anyReset = false;
            var connectedIds = new HashSet<byte>();
            if (PlayerControl.AllPlayerControls != null)
                foreach (var p in PlayerControl.AllPlayerControls) if (p != null && p.Data != null) connectedIds.Add(p.Data.PlayerId);
            foreach (var area in __instance.playerStates)
            {
                if (area == null) continue;
                if (!connectedIds.Contains(area.PlayerId) && votedPlayers.Contains(area.PlayerId) && !_handledDisconnects.Contains(area.PlayerId)) { _handledDisconnects.Add(area.PlayerId); anyReset = true; break; }
            }
            foreach (var area in __instance.playerStates)
            {
                if (area == null) continue;
                _lastVotes.TryGetValue(area.PlayerId, out byte prev);
                byte cur = (byte)area.VotedForId;
                var voter = GameData.Instance?.GetPlayerById(area.PlayerId);
                bool stillConnected = voter != null && !voter.Disconnected;
                if (prev != 0 && cur != prev && stillConnected &&
                    (area.VotedForId == PlayerVoteArea.HasNotVoted ||
                     area.VotedForId == PlayerVoteArea.MissedVote  ||
                     area.VotedForId == PlayerVoteArea.DeadVote)) anyReset = true;
                _lastVotes[area.PlayerId] = cur;
            }
            if (anyReset)
            {
                votedPlayers.Clear();
                foreach (var area in __instance.playerStates)
                {
                    if (!area) continue;
                    var vs = area.transform.GetComponent<VoteSpreader>();
                    if (vs == null) continue;
                    foreach (var sr in vs.Votes.ToArray()) if (sr) Object.Destroy(sr.gameObject);
                    vs.Votes.Clear();
                }
                if (__instance.SkippedVoting)
                {
                    var svs = __instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
                    if (svs != null) { foreach (var sr in svs.Votes.ToArray()) if (sr) Object.Destroy(sr.gameObject); svs.Votes.Clear(); }
                }
                if (GameData.Instance != null)
                {
                    var disconnected = new HashSet<byte>();
                    foreach (var pd in GameData.Instance.AllPlayers)
                        if (pd != null && pd.Disconnected) disconnected.Add(pd.PlayerId);
                    foreach (var kv in _voteCache)
                    {
                        byte voterId = kv.Key; byte targetId = kv.Value;
                        if (disconnected.Contains(voterId)) { votedPlayers.Add(voterId); continue; }
                        if (disconnected.Contains(targetId)) { votedPlayers.Add(voterId); continue; }
                        var voterData = GameData.Instance.GetPlayerById(voterId);
                        if (voterData == null) continue;
                        try {
                        votedPlayers.Add(voterId);
                        if (targetId == PlayerVoteArea.SkippedVote && __instance.SkippedVoting)
                        {
                            var ss = __instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
                            int si = ss != null ? ss.Votes.Count : 0;
                            __instance.BloopAVoteIcon(voterData, si, __instance.SkippedVoting.transform);
                        }
                        else
                        {
                            foreach (var area in __instance.playerStates)
                            {
                                if (area == null || area.transform == null) continue;
                                if (area.PlayerId != targetId) continue;
                                var vs2 = area.transform.GetComponent<VoteSpreader>();
                                int vi = vs2 != null ? vs2.Votes.Count : 0;
                                __instance.BloopAVoteIcon(voterData, vi, area.transform);
                                break;
                            }
                        }
                        } catch { }
                    }
                }
            }
            foreach (var playerVoteArea in __instance.playerStates)
            {
                if (!playerVoteArea) continue;
                if (playerVoteArea.VotedForId == PlayerVoteArea.HasNotVoted) continue;
                if (playerVoteArea.VotedForId == PlayerVoteArea.MissedVote)  continue;
                if (playerVoteArea.VotedForId == PlayerVoteArea.DeadVote)    continue;
                if (votedPlayers.Contains(playerVoteArea.PlayerId))  continue;

                if (GameData.Instance == null) continue;
                var playerData = GameData.Instance.GetPlayerById(playerVoteArea.PlayerId);
                if (playerData == null) continue;

                votedPlayers.Add(playerVoteArea.PlayerId);

                if (playerVoteArea.VotedForId != PlayerVoteArea.SkippedVote)
                {
                    foreach (var votedForArea in __instance.playerStates)
                    {
                        if (votedForArea.PlayerId != (byte)playerVoteArea.VotedForId) continue;
                        var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
                        int voteIdx = voteSpreader != null ? voteSpreader.Votes.Count : 0;
                        __instance.BloopAVoteIcon(playerData, voteIdx, votedForArea.transform);
                        break;
                    }
                }
                else if (__instance.SkippedVoting)
                {
                    var skipSpreader = __instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
                    int voteIdx = skipSpreader != null ? skipSpreader.Votes.Count : 0;
                    __instance.BloopAVoteIcon(playerData, voteIdx, __instance.SkippedVoting.transform);
                }
            }
        }

        foreach (var votedForArea in __instance.playerStates)
        {
            if (!votedForArea) continue;
            var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
            if (!voteSpreader) continue;
            foreach (var spriteRenderer in voteSpreader.Votes)
                if (spriteRenderer) spriteRenderer.gameObject.SetActive(CheatToggles.revealVotes);
        }

        if (__instance.SkippedVoting)
            __instance.SkippedVoting.SetActive(CheatToggles.revealVotes);
        }
        catch { }
    }

    public static void Postfix(MeetingHud __instance)
    {
        try { MalumESP.MeetingNametags(__instance); } catch { }
        if (PlayerControl.LocalPlayer != null)
            PlayerControl.LocalPlayer.onLadder = false;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHud_PopulateResults
{
    public static void Prefix(MeetingHud __instance)
    {
        foreach (var votedForArea in __instance.playerStates)
        {
            if (!votedForArea) continue;
            var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
            if (!voteSpreader) continue;
            if (voteSpreader.Votes.Count == 0) continue;
            foreach (var spriteRenderer in voteSpreader.Votes.ToArray())
                Object.DestroyImmediate(spriteRenderer.gameObject);
            voteSpreader.Votes.Clear();
        }

        if (__instance.SkippedVoting)
        {
            var voteSpreader = __instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
            if (voteSpreader != null)
            {
                foreach (var spriteRenderer in voteSpreader.Votes.ToArray())
                    Object.DestroyImmediate(spriteRenderer.gameObject);
                voteSpreader.Votes.Clear();
            }
        }

        MeetingHud_Update.ClearAll();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHud_Close
{
    public static void Prefix()
    {
        MeetingHud_Update.ClearAll();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class MeetingHud_CheckForEndVoting
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!CheatToggles.voteImmune) return true;

        if (!__instance.playerStates.All(ps => { var pd = GameData.Instance.GetPlayerById(ps.PlayerId); return ps.AmDead || ps.DidVote || (pd != null && pd.Disconnected); })) return true;

        var max = __instance.CalculateVotes().MaxPair(out var tie);
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.Disconnected);

        if (exiled != null && exiled == PlayerControl.LocalPlayer.Data)
            exiled = null;

        var states = new MeetingHud.VoterState[__instance.playerStates.Length];

        for (var index = 0; index < __instance.playerStates.Length; ++index)
        {
            var playerState = __instance.playerStates[index];
            states[index] = new MeetingHud.VoterState
            {
                VoterId = playerState.PlayerId,
                VotedForId = playerState.VotedForId
            };
        }

        __instance.RpcVotingComplete(states, exiled, tie, false, 0);
        return false;
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class MeetingCleanup_OnLobby
{
    public static void Postfix()
    {
        MeetingHud_Update.ClearAll();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
public static class MeetingCleanup_OnGameStart
{
    public static void Postfix()
    {
        MeetingHud_Update.ClearAll();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHud_Start
{
    public static void Prefix(MeetingHud __instance)
    {
        try
        {
            var cache = ShipStatus.Instance?.CosmeticsCache;
            if (cache != null) __instance.StartCoroutine(cache.PopulateFromPlayers());
        }
        catch { }
    }
}



