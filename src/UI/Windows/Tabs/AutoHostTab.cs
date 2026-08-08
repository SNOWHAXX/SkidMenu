using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

internal class AutoHostTab : ITab
{
    public string name => "AutoHost";

    private Vector2 _scroll = Vector2.zero;

    public void Draw()
    {
        _scroll = GUILayout.BeginScrollView(_scroll);

        DrawAutoHost();
        GUILayout.Space(10);
        DrawStatus();

        GUILayout.EndScrollView();
    }

    private void DrawAutoHost()
    {
        GUILayout.Label("AutoHost", GUIStylePreset.TabSubtitle);

        Toggle(ref SkidMenu.autoHostEnabled,              "Lobby autostart");
        Toggle(ref SkidMenu.autoHostInstantStart,         "Instant start");
        Toggle(ref SkidMenu.autoHostCancelBelowMin,       "Cancel if players leave");
        Toggle(ref SkidMenu.autoHostWaitLoadedPlayers,    "Wait for loaded players");
        Toggle(ref SkidMenu.autoHostReturnAfterMatch,     "Return after match");
        Toggle(ref SkidMenu.autoHostForceLastMinute,      "Last-minute start");

        GUILayout.Space(6);

        Slider(ref SkidMenu.autoHostMinPlayers,           1,   15,  "Min players",       v => $"{v}");
        Slider(ref SkidMenu.autoHostForceMinPlayers,      1,   15,  "Force min players", v => $"{v}");
        Slider(ref SkidMenu.autoHostWarmupSeconds,        0,  120,  "Lobby warmup",      v => v == 0 ? "Off" : $"{v}s");
        Slider(ref SkidMenu.autoHostStartDelaySeconds,    0,  180,  "Start delay",       v => $"{v}s");
        Slider(ref SkidMenu.autoHostFastStartPlayers,     0,   15,  "Fast start at",     v => v == 0 ? "Off" : $"{v}p");
        Slider(ref SkidMenu.autoHostFastStartDelaySeconds,0,   60,  "Fast delay",        v => $"{v}s");
        Slider(ref SkidMenu.autoHostLoadGraceSeconds,     0,   90,  "Load grace",        v => v == 0 ? "Forever" : $"{v}s");
        Slider(ref SkidMenu.autoHostForceAfterMinutes,    0,   10,  "Force after",       v => v == 0 ? "Off" : $"{v}m");
        Slider(ref SkidMenu.autoHostBackoffSeconds,       2,   60,  "Retry cooldown",    v => $"{v}s");
    }

    private void DrawStatus()
    {
        GUILayout.Label("Status", GUIStylePreset.TabSubtitle);

        InfoRow("State",      AutoHostService.StatusText);
        InfoRow("Players",    $"{AutoHostService.ConnectedPlayers} / {SkidMenu.autoHostMinPlayers}");

        if (AutoHostService.WarmupRemaining    > 0.05f) InfoRow("Warmup",    FormatTime(AutoHostService.WarmupRemaining));
        if (AutoHostService.CountdownRemaining > 0.05f) InfoRow("Countdown", FormatTime(AutoHostService.CountdownRemaining));
        if (AutoHostService.LoadGraceRemaining > 0.05f) InfoRow("Load wait", FormatTime(AutoHostService.LoadGraceRemaining));
        if (AutoHostService.BackoffRemaining   > 0.05f) InfoRow("Backoff",   FormatTime(AutoHostService.BackoffRemaining));

        float age = AutoHostService.LobbyAgeSeconds;
        if (age > 0f) InfoRow("Lobby age", FormatClock(age));

        float remaining = 600f - age;
        if (remaining > 0f && age > 0f) InfoRow("Until close", FormatClock(Mathf.Max(0f, remaining)));
    }

    private static void Toggle(ref bool value, string label)
    {
        value = GUIStylePreset.CustomToggle(value, " " + label);
    }

    private static void Slider(ref int value, int min, int max, string label, System.Func<int, string> fmt)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {fmt(value)}", GUILayout.Width(200));
        value = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max));
        GUILayout.EndHorizontal();
    }

    private static void InfoRow(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(90));
        GUILayout.Label(value);
        GUILayout.EndHorizontal();
    }

    private static string FormatTime(float seconds)
    {
        int s = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        return s >= 60 ? $"{s / 60}m {s % 60}s" : $"{s}s";
    }

    private static string FormatClock(float seconds)
    {
        int s = Mathf.RoundToInt(Mathf.Max(0f, seconds));
        return $"{s / 60}:{(s % 60 < 10 ? "0" : "")}{s % 60}";
    }
}
