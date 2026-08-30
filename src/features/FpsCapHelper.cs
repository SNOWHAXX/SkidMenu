using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class FpsCapHelper
{
    public static void Apply()
    {
        if (CheatToggles.maxFpsEnabled)
        {
            QualitySettings.vSyncCount  = 0;
            Application.targetFrameRate = CheatToggles.maxFpsValue;
        }
        else
        {
            QualitySettings.vSyncCount  = 1;
            Application.targetFrameRate = -1;
        }
    }

    // existing hooks
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    static class OnGameEnd     { static void Postfix() => Apply(); }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    static class OnGameStart   { static void Postfix() => Apply(); }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class OnMeetingStart { static void Postfix() => Apply(); }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    static class OnMeetingEnd  { static void Postfix() => Apply(); }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    static class OnExitGame    { static void Postfix() => Apply(); }

    // new: lobby join
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    static class OnJoinLobby   { static void Postfix() => Apply(); }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    static class OnJoinGame    { static void Postfix() => Apply(); }

    // new: sabotage start + fix on every sabotage system
    [HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.UpdateSystem))]
    static class OnSwitchUpdate
    {
        static bool _prev;
        static void Postfix(SwitchSystem __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }

    [HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.UpdateSystem))]
    static class OnLifeSuppUpdate
    {
        static bool _prev;
        static void Postfix(LifeSuppSystemType __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }

    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.UpdateSystem))]
    static class OnReactorUpdate
    {
        static bool _prev;
        static void Postfix(ReactorSystemType __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }

    [HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
    static class OnHudOverrideUpdate
    {
        static bool _prev;
        static void Postfix(HudOverrideSystemType __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }

    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.UpdateSystem))]
    static class OnHeliUpdate
    {
        static bool _prev;
        static void Postfix(HeliSabotageSystem __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }

    [HarmonyPatch(typeof(HqHudSystemType), nameof(HqHudSystemType.UpdateSystem))]
    static class OnHqHudUpdate
    {
        static bool _prev;
        static void Postfix(HqHudSystemType __instance)
        {
            bool now = __instance.IsActive;
            if (_prev != now) Apply();
            _prev = now;
        }
    }
}
