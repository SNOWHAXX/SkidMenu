using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace SkidMenu.features;

public static class JudgeCheats
{
    public static bool InstantUnlock = false;
    public static bool InfiniteGavels = false;

    private static byte _forgedTargetId = byte.MaxValue;
    private static byte _forgedJudgeId = byte.MaxValue;
    private static float _forgedTime = -999f;

    public static void MarkForgedVerdict(byte targetId) => MarkForgedVerdict(targetId, null);

    public static void MarkForgedVerdict(byte targetId, byte? judgeId)
    {
        _forgedTargetId = targetId;
        _forgedJudgeId = judgeId ?? PlayerControl.LocalPlayer?.PlayerId ?? byte.MaxValue;
        _forgedTime = Time.time;
    }

    public static PlayerControl GetAttributedJudge(byte targetId)
    {
        var j = FindOverrulingJudge(targetId);
        if (j != null) return j;
        try
        {
            if (Time.time - _forgedTime < 2f && _forgedTargetId == targetId)
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p != null && p.PlayerId == _forgedJudgeId) return p;
            }
        }
        catch { }
        return null;
    }

    public static PlayerControl FindOverrulingJudge(byte targetId)
    {
        try
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data?.Role == null) continue;
                var role = p.Data.Role;
                if (role.GetIl2CppType().Name != "JudgeRole") continue;
                var jr = role.TryCast<JudgeRole>();
                if (jr == null) continue;
                if ((byte)jr.OverruledPlayerId == targetId) return p;
            }
        }
        catch { }
        return null;
    }

    public static void Overrule(PlayerControl target)
    {
        if (target?.Data == null) return;
        if (MeetingHud.Instance == null)
        {
            SkidMenu.notifications.Send("Judge", "No active meeting.");
            return;
        }
        try
        {
            var role = PlayerControl.LocalPlayer?.Data?.Role;
            var jr = role != null && role.GetIl2CppType().Name == "JudgeRole" ? role.TryCast<JudgeRole>() : null;
            if (jr != null)
            {
                if (jr.TryOverrule(target.PlayerId))
                {
                    SkidMenu.notifications.Send("Judge", $"Gavel dropped on {target.Data.PlayerName}");
                    return;
                }
            }

            if (AmongUsClient.Instance.AmHost)
            {
                MarkForgedVerdict(target.PlayerId);
                MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), target.Data, false, true, 0);
                SkidMenu.notifications.Send("Judge", $"Overruled vote: {target.Data.PlayerName} ejected");
                return;
            }

            SkidMenu.notifications.Send("Judge", "You are not the Judge (host-only fallback).");
        }
        catch (System.Exception e)
        {
            SkidMenu.notifications.Send("Judge", $"Overrule failed: {e.Message}");
        }
    }

    [HarmonyPatch(typeof(JudgeRole), nameof(JudgeRole.IsBlockedByTasks))]
    static class JudgeRole_IsBlockedByTasks
    {
        static bool Prefix(ref bool __result)
        {
            if (!InstantUnlock) return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(JudgeRole), nameof(JudgeRole.ConsumeOverruleVotesUsage))]
    static class JudgeRole_ConsumeOverrule
    {
        static bool Prefix()
        {
            return !InfiniteGavels;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    static class JudgeRole_Refill
    {
        static void Postfix()
        {
            if (!InfiniteGavels) return;
            try
            {
                var role = PlayerControl.LocalPlayer?.Data?.Role;
                if (role == null || role.GetIl2CppType().Name != "JudgeRole") return;
                var jr = role.TryCast<JudgeRole>();
                if (jr == null) return;
                jr.HasAnOverruleUse = true;
                jr.HasAlreadyOverruledThisMeeting = false;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    static class VotingComplete_JudgeImmune
    {
        static void Prefix(ref NetworkedPlayerInfo exiled, ref bool wasOverruled)
        {
            if (!CheatToggles.judgeImmune || !wasOverruled) return;
            if (exiled == null || PlayerControl.LocalPlayer == null) return;
            if (exiled.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
            exiled = null;
            wasOverruled = false;
        }
    }
}
