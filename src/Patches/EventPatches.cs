using System;
using HarmonyLib;

namespace SkidMenu;

internal static class SabotageFixHelper
{
    public static void CheckFix(PlayerControl fixer, string display)
    {
        if (fixer == null) return;
        if (CheatToggles.logSabotageFix)
        {
            try { ConsoleUI.Log($"{ConsoleHelper.Fmt(fixer)} fixed <color=#88ff88>{display}</color>", "44FF88"); } catch { }
        }
        if (CheatToggles.notifSabotageFix && !NotifHelper.Skip(fixer, 20))
        {
            try
            {
                string who = fixer.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(fixer);
                SkidMenu.notifications.Send("<color=#88ff88>🔧 Fix</color>", $"{who} fixed {display}{NotifHelper.Room(fixer)}", 3.5f);
            }
            catch { }
        }
    }

    public static void Reset()
    {
        Event_SwitchFix.Reset();
        Event_LifeSuppFix.Reset();
        Event_ReactorFix.Reset();
        Event_HudFix.Reset();
        Event_HeliFix.Reset();
        Event_HqHudFix.Reset();
    }
}

[HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.UpdateSystem))]
public static class Event_SwitchFix
{
    private static bool _wasActive;
    public static void Postfix(SwitchSystem __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Lights");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.UpdateSystem))]
public static class Event_LifeSuppFix
{
    private static bool _wasActive;
    public static void Postfix(LifeSuppSystemType __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Life Support");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.UpdateSystem))]
public static class Event_ReactorFix
{
    private static bool _wasActive;
    public static void Postfix(ReactorSystemType __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Reactor");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

[HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
public static class Event_HudFix
{
    private static bool _wasActive;
    public static void Postfix(HudOverrideSystemType __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Comms");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.UpdateSystem))]
public static class Event_HeliFix
{
    private static bool _wasActive;
    public static void Postfix(HeliSabotageSystem __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Comms");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

[HarmonyPatch(typeof(HqHudSystemType), nameof(HqHudSystemType.UpdateSystem))]
public static class Event_HqHudFix
{
    private static bool _wasActive;
    public static void Postfix(HqHudSystemType __instance, PlayerControl player)
    {
        try
        {
            bool now = __instance.IsActive;
            if (_wasActive && !now) SabotageFixHelper.CheckFix(player, "Comms");
            _wasActive = now;
        }
        catch { }
    }
    public static void Reset() => _wasActive = false;
}

internal static class CamerasHelper
{
    public static void Report(string what, bool closed)
    {
        if (!CheatToggles.logCameras && !CheatToggles.notifCameras) return;
        try
        {
            string action = closed ? "closed" : "opened";
            string title = what switch
            {
                "Vitals" => "<color=#aaddff>🩺 Vitals</color>",
                "Binoculars" => "<color=#aaddff>🔭 Binoculars</color>",
                _ => "<color=#aaddff>📷 Cameras</color>"
            };
            if (CheatToggles.logCameras) ConsoleUI.Log($"<color=#aaddff>You {action} {what}</color>", "AACCFF");
            if (CheatToggles.notifCameras)
                SkidMenu.notifications.Send(title, $"<color=#00ff88>You</color> {action} {what}", 3f);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
public static class Event_Cameras
{
    public static void Postfix() => CamerasHelper.Report("Cameras", false);
}

[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
public static class Event_PolusCams
{
    public static void Postfix() => CamerasHelper.Report("Cameras", false);
}

[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
public static class Event_Vitals
{
    public static void Postfix() => CamerasHelper.Report("Vitals", false);
}

[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
public static class Event_FungleCams
{
    public static void Postfix() => CamerasHelper.Report("Binoculars", false);
}

[HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[0])]
public static class Event_MinigameClose
{
    public static void Postfix(Minigame __instance)
    {
        if (!CheatToggles.logCameras && !CheatToggles.notifCameras) return;
        try
        {
            if (__instance is VitalsMinigame)
                CamerasHelper.Report("Vitals", true);
            else if (__instance is FungleSurveillanceMinigame)
                CamerasHelper.Report("Binoculars", true);
            else if (__instance is SurveillanceMinigame || __instance is PlanetSurveillanceMinigame)
                CamerasHelper.Report("Cameras", true);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class Event_RoundStart
{
    public static void Postfix()
    {
        SabotageFixHelper.Reset();
        if (!CheatToggles.logGameOver && !CheatToggles.notifGameOver) return;
        try
        {
            string map = "?";
            try
            {
                if (ShipStatus.Instance != null) map = ShipStatus.Instance.Type.ToString();
            }
            catch { }
            if (CheatToggles.logGameOver)
                ConsoleUI.Log($"<color=#ffd700>Round started</color> on <color=#aaddff>{map}</color>", "FFD700");
            if (CheatToggles.notifGameOver)
                SkidMenu.notifications.Send("<color=#ffd700>▶ Round Start</color>", $"<color=#aaddff>{map}</color>", 3f);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class Event_GameOver
{
    public static void Postfix()
    {
        if (!CheatToggles.logGameOver && !CheatToggles.notifGameOver) return;
        try
        {
            bool humansWon = false, impsWon = false;
            try
            {
                var reason = EndGameResult.CachedGameOverReason;
                humansWon = GameManager.Instance.DidHumansWin(reason);
                impsWon = GameManager.Instance.DidImpostorsWin(reason);
            }
            catch { }

            if (!humansWon && !impsWon)
            {
                int aliveImps = 0;
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p != null && p.Data != null && RoleManager.IsImpostorRole(p.Data.RoleType) && !p.Data.IsDead) aliveImps++;
                impsWon = aliveImps > 0;
                humansWon = !impsWon;
            }

            string winner = humansWon && !impsWon ? "Crewmates" : impsWon && !humansWon ? "Impostors" : "Nobody";
            string col = impsWon ? "FF4444" : "44FF88";
            if (CheatToggles.logGameOver)
                ConsoleUI.Log($"<color=#{col}>{winner}</color> win!", col);
            if (CheatToggles.notifGameOver)
                SkidMenu.notifications.Send("<color=#ffd700>✓ Game Over</color>", $"<color=#{col}>{winner}</color> win!", 5f);
        }
        catch { }
    }
}
