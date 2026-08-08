using UnityEngine;
using System;
using System.Collections.Generic;

namespace SkidMenu;

public class MovementTab : ITab
{
    public string name => "Movement";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        bool inGame = PlayerControl.LocalPlayer != null;

        if (inGame)
        {
            Vector2 position = PlayerControl.LocalPlayer.transform.position;
            GUILayout.Label($"Current Map: {Utilities.GetCurrentMap()}\nCurrent Position:\nX: {position.x:F2}\nY: {position.y:F2}");
        }
        else
        {
            GUILayout.Label("Not currently in a game, some options will not take effect until you are.");
        }

        GUILayout.Space(15);
        DrawGeneral(inGame);
        GUILayout.Space(15);
        DrawTeleport(inGame);

        GUILayout.EndVertical();
    }

    private void DrawGeneral(bool inGame)
    {
        CheatToggles.noClip = GUIStylePreset.CustomToggle(CheatToggles.noClip, " NoClip");
        CheatToggles.invertControls = GUIStylePreset.CustomToggle(CheatToggles.invertControls, " Invert Controls");

        GUILayout.Space(8);
        features.Self.PlayerSpeedModifier.Enabled = GUIStylePreset.CustomToggle(features.Self.PlayerSpeedModifier.Enabled, " Speed Modifier");

        float mult = features.Self.PlayerSpeedModifier.Multiplier;
        GUILayout.Label($"Speed Multiplier: {mult:F2}x");
        mult = GUILayout.HorizontalSlider(mult, 0.01f, 10f, GUILayout.Width(400f));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1", GUILayout.Width(40))) mult -= 1f;
        if (GUILayout.Button("-0.1", GUILayout.Width(45))) mult -= 0.1f;
        if (GUILayout.Button("-0.01", GUILayout.Width(50))) mult -= 0.01f;
        if (GUILayout.Button("Reset", GUILayout.Width(55))) mult = 1f;
        if (GUILayout.Button("+0.01", GUILayout.Width(50))) mult += 0.01f;
        if (GUILayout.Button("+0.1", GUILayout.Width(45))) mult += 0.1f;
        if (GUILayout.Button("+1", GUILayout.Width(40))) mult += 1f;
        GUILayout.EndHorizontal();

        features.Self.PlayerSpeedModifier.Multiplier = Mathf.Clamp(mult, 0.01f, 10f);

        GUILayout.Space(8);
        features.Self.CurrentSpeedChanger.Enabled = GUIStylePreset.CustomToggle(features.Self.CurrentSpeedChanger.Enabled, " Current Speed Changer");
        GUILayout.Label($"Target Speed: {features.Self.CurrentSpeedChanger.Speed:F2}");
        features.Self.CurrentSpeedChanger.Speed = GUILayout.HorizontalSlider(features.Self.CurrentSpeedChanger.Speed, 0f, 10f, GUILayout.Width(550f));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1", GUILayout.Width(40)))    features.Self.CurrentSpeedChanger.Speed -= 1f;
        if (GUILayout.Button("-0.1", GUILayout.Width(45)))  features.Self.CurrentSpeedChanger.Speed -= 0.1f;
        if (GUILayout.Button("-0.01", GUILayout.Width(50))) features.Self.CurrentSpeedChanger.Speed -= 0.01f;
        if (GUILayout.Button("Reset", GUILayout.Width(55))) features.Self.CurrentSpeedChanger.Speed = 2.5f;
        if (GUILayout.Button("+0.01", GUILayout.Width(50))) features.Self.CurrentSpeedChanger.Speed += 0.01f;
        if (GUILayout.Button("+0.1", GUILayout.Width(45)))  features.Self.CurrentSpeedChanger.Speed += 0.1f;
        if (GUILayout.Button("+1", GUILayout.Width(40)))    features.Self.CurrentSpeedChanger.Speed += 1f;
        GUILayout.EndHorizontal();
        features.Self.CurrentSpeedChanger.Speed = Mathf.Clamp(features.Self.CurrentSpeedChanger.Speed, 0f, 10f);

        if (!inGame)
        {
            GUILayout.Label("Current Speed: N/A");
            return;
        }

        try
        {
            float baseSpeed = PlayerControl.LocalPlayer.Data.IsDead
                ? PlayerControl.LocalPlayer.MyPhysics.GhostSpeed
                : PlayerControl.LocalPlayer.MyPhysics.Speed * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod;
            float effective = features.Self.PlayerSpeedModifier.Enabled ? baseSpeed * features.Self.PlayerSpeedModifier.Multiplier : baseSpeed;
            GUILayout.Label($"Current Speed: {effective:F2}");
        }
        catch (NullReferenceException) { SkidMenu.Log.LogWarning("Failed to draw general movement tab."); }

        GUILayout.Space(15);
        DrawLagCompensation();
    }

    private void DrawLagCompensation()
    {
        GUILayout.Label("Lag Compensation", GUIStylePreset.TabSubtitle);
        GUILayout.Space(3);

        bool newEnabled = GUIStylePreset.CustomToggle(features.LagCompensation.Enabled, " Enable Lag Compensation");
        if (newEnabled != features.LagCompensation.Enabled)
        {
            features.LagCompensation.Enabled = newEnabled;
            features.LagCompensation.Reset();
        }

        if (!features.LagCompensation.Enabled) return;

        GUILayout.Space(4);

        bool newFreeze = GUIStylePreset.CustomToggle(features.LagCompensation.FreezePosition, " Freeze Position (appear stationary to others)");
        if (newFreeze && !features.LagCompensation.FreezePosition) features.LagCompensation.Jitter = false;
        features.LagCompensation.FreezePosition = newFreeze;
        bool newJitter = GUIStylePreset.CustomToggle(features.LagCompensation.Jitter, " Jitter Mode (random desync intervals)");
        if (newJitter && !features.LagCompensation.Jitter) features.LagCompensation.FreezePosition = false;
        features.LagCompensation.Jitter = newJitter;

        if (!features.LagCompensation.FreezePosition && !features.LagCompensation.Jitter)
        {
            int skip = features.LagCompensation.SkipTicks;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  Delay: {skip} tick(s) skipped", GUILayout.Width(200));
            int newSkip = Mathf.RoundToInt(GUILayout.HorizontalSlider(skip, 0, 10, GUILayout.Width(300)));
            if (newSkip != skip) features.LagCompensation.SkipTicks = newSkip;
            GUILayout.EndHorizontal();
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label("  0 = normal, 2 = noticeable lag, 5 = heavy rubber-band, 10 = max");
            GUI.color = Color.white;
        }

        if (features.LagCompensation.Jitter)
        {
            GUILayout.Space(3);
            float jMin = features.LagCompensation.JitterMin;
            float jMax = features.LagCompensation.JitterMax;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  Jitter Min: {jMin:F0} frames", GUILayout.Width(160));
            float newMin = Mathf.Round(GUILayout.HorizontalSlider(jMin, 1f, 20f, GUILayout.Width(300)));
            if (System.Math.Abs(newMin - jMin) > 0.5f) features.LagCompensation.JitterMin = newMin;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  Jitter Max: {jMax:F0} frames", GUILayout.Width(160));
            float newMax = Mathf.Round(GUILayout.HorizontalSlider(jMax, 1f, 40f, GUILayout.Width(300)));
            if (System.Math.Abs(newMax - jMax) > 0.5f) features.LagCompensation.JitterMax = newMax;
            GUILayout.EndHorizontal();
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label("  1 frame = ~16ms. Min 1, Max 1 = random every frame.");
            GUI.color = Color.white;
        }

        GUILayout.Space(3);
        GUI.color = new Color(1f, 0.75f, 0.2f);
        GUILayout.Label("  Only affects what others see. You move normally on your screen.");
        GUI.color = Color.white;

        if (GUILayout.Button("Reset", GUILayout.Width(80)))
            features.LagCompensation.Reset();
    }

    private void DrawTeleport(bool inGame)
    {
        GUILayout.Label("Teleport", GUIStylePreset.TabSubtitle);

        CheatToggles.teleportCursor = GUIStylePreset.CustomToggle(CheatToggles.teleportCursor, " to Cursor");
        CheatToggles.teleportPlayer = GUIStylePreset.CustomToggle(CheatToggles.teleportPlayer, " to Player");
        Teleporter.UseSnapToRPC = GUIStylePreset.CustomToggle(Teleporter.UseSnapToRPC, "Use SnapTo RPC For Teleports");

        GUILayout.Label("Teleport To Location:");

        Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();

        byte i = 0;
        foreach (var (key, value) in teleportLocations)
        {
            if (i % 2 == 0) GUILayout.BeginHorizontal();

            GUI.enabled = inGame;
            if (GUILayout.Button(key)) Teleporter.TeleportTo(value);
            GUI.enabled = true;

            if (i % 2 != 0) GUILayout.EndHorizontal();
            i++;
        }

        if (i % 2 != 0) GUILayout.EndHorizontal();
    }
}
