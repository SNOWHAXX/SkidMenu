using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

namespace SkidMenu;

public static class KillCooldownTracker
{
    private static readonly Dictionary<byte, (float startTime, float duration)> _timers = new();

    public static float GetRemainingCooldown(byte playerId)
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == playerId)
            return Mathf.Max(0f, PlayerControl.LocalPlayer.killTimer);

        if (_timers.TryGetValue(playerId, out var e))
        {
            float remaining = e.duration - (Time.time - e.startTime);
            if (remaining > 0f) return remaining;
        }

        return 0f;
    }
    private static void Set(byte playerId, float duration) =>
        _timers[playerId] = (Time.time, duration);

    private static float ConfiguredCooldown()
    {
        try { return GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown); }
        catch { return 25f; }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    static class IntroPatch
    {
        static void Postfix()
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc?.Data?.Role == null) continue;
                if (pc.Data.Role.IsImpostor)
                    Set(pc.PlayerId, 10f);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class SetKillCooldownPatch
    {
        static void Postfix(PlayerControl __instance, float time)
        {
            if (__instance?.Data?.Role == null) return;
            Set(__instance.PlayerId, time);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class MeetingStartPatch
    {
        static void Postfix() => _timers.Clear();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    static class MurderPatch
    {
        static void Postfix(PlayerControl __instance)
        {
            Set(__instance.PlayerId, ConfiguredCooldown());
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    static class MeetingEndPatch
    {
        static void Postfix()
        {
            float cd = ConfiguredCooldown();
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc?.Data?.Role == null || pc.Data.IsDead) continue;
                if (pc.Data.Role.IsImpostor)
                    Set(pc.PlayerId, cd);
            }
        }
    }
}
