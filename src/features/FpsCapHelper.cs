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

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    static class ReapplyOnGameEnd
    {
        static void Postfix() => Apply();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    static class ReapplyOnGameStart
    {
        static void Postfix() => Apply();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class ReapplyOnMeetingStart
    {
        static void Postfix() => Apply();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    static class ReapplyOnMeetingEnd
    {
        static void Postfix() => Apply();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    static class ReapplyOnExitGame
    {
        static void Postfix() => Apply();
    }
}
