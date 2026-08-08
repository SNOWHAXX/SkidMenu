using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace SkidMenu;

public static class SkipRoleReveal
{
    public static bool Enabled { get; set; } = false;
    private static bool _done       = false;
    private static bool _shouldSkip = false;

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    static class ResetOnEnd { static void Postfix() { _done = false; _shouldSkip = false; Patch.Reset(); } }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    static class ResetOnLobby { static void Postfix() { _done = false; _shouldSkip = false; Patch.Reset(); } }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class MeetingHudStartFix
    {
        static void Prefix()
        {
            if (!Enabled) return;
            try
            {
                if (HudManager.Instance?.FullScreen != null)
                    HudManager.Instance.FullScreen.color = Color.clear;
                var local = PlayerControl.LocalPlayer;
                if (HudManager.Instance != null && local?.Data?.Role != null)
                    HudManager.Instance.SetHudActive(local, local.Data.Role, true);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    static class UpdateButtonsFix
    {
        static bool Prefix(MeetingHud __instance)
        {
            if (!Enabled) return true;
            try { return __instance.state < MeetingHud.VoteStates.Results; }
            catch { return false; }
        }
    }

    private static IEnumerator DelayedSkip(IntroCutscene intro)
    {
        yield return new WaitForSeconds(0.1f);
        try
        {
            if (HudManager.Instance?.FullScreen != null)
                HudManager.Instance.FullScreen.color = Color.clear;
            if (intro != null)
                foreach (var sr in intro.GetComponentsInChildren<SpriteRenderer>(true))
                    { var c = sr.color; c.a = 0f; sr.color = c; }
        }
        catch { }

        yield return new WaitForSeconds(0.1f);
        try
        {
            if (intro != null)
            {
                intro.StopAllCoroutines();
                intro.gameObject.SetActive(false);
                IntroCutscene.Instance = null;
            }
            if (HudManager.Instance != null)
                HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);
        }
        catch { }
        _shouldSkip = true;
    }

    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__35), nameof(IntroCutscene._CoBegin_d__35.MoveNext))]
    static class Patch
    {
        private static int _callCount = 0;
        public static void Reset() => _callCount = 0;

        static void Prefix(IntroCutscene._CoBegin_d__35 __instance)
        {
            if (!Enabled) return;
            if (_callCount == 0) _callCount++;
        }

        static void Postfix(IntroCutscene._CoBegin_d__35 __instance, ref bool __result)
        {
            if (!Enabled) return;

            if (_callCount == 1)
            {
                _callCount++;
                if (!_done)
                {
                    _done = true;
                    try
                    {
                        AmongUsClient.Instance.StartCoroutine(DelayedSkip(__instance.__4__this));
                    }
                    catch { }
                }
            }

            // stage 2: once timer fired, stop the coroutine so movement unlocks
            if (_shouldSkip)
                __result = false;
        }
    }
}