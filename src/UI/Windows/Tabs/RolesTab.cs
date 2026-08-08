using System.Collections.Generic;
using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class RolesTab : ITab
{
    public string name => "Roles";

    private string _scaleHInput = CheatToggles.tasksScaleH.ToString("F0");
    private string _scaleVInput = CheatToggles.tasksScaleV.ToString("F0");
    private Dictionary<string, bool> _focusedFields = new();
    private Dictionary<string, float> _lastBlinkTime = new();
    private Dictionary<string, bool> _cursorVisible = new();
    private Dictionary<string, Rect> _fieldRects = new();
    private Dictionary<string, int> _cursorPositions = new();
    private float _cursorBlinkTime = 0.5f;

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 50, int height = 20)
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(width), GUILayout.Height(height));
        if (Event.current.type == EventType.Repaint) _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();
        if (!_focusedFields.ContainsKey(fieldKey)) _focusedFields[fieldKey] = false;
        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey)) { if (_fieldRects[fieldKey].Contains(Event.current.mousePosition)) { _focusedFields[fieldKey] = true; _lastBlinkTime[fieldKey] = Time.time; _cursorVisible[fieldKey] = true; Event.current.Use(); } else _focusedFields[fieldKey] = false; }
        if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey] && Event.current.type == EventType.KeyDown) { if (!_cursorPositions.ContainsKey(fieldKey)) _cursorPositions[fieldKey] = content.Length; int cp = System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length); if (Event.current.keyCode == KeyCode.Backspace && cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; Event.current.Use(); } else if (char.IsDigit(Event.current.character) && content.Length < 3) { content = content.Substring(0, cp) + Event.current.character + content.Substring(cp); cp++; Event.current.Use(); } _cursorPositions[fieldKey] = System.Math.Clamp(cp, 0, content.Length); }
        if (_fieldRects.ContainsKey(fieldKey)) { GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content); if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey]) { if (!_lastBlinkTime.ContainsKey(fieldKey)) _lastBlinkTime[fieldKey] = Time.time; if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime) { _cursorVisible[fieldKey] = !_cursorVisible[fieldKey]; _lastBlinkTime[fieldKey] = Time.time; } if (_cursorVisible.ContainsKey(fieldKey) && _cursorVisible[fieldKey]) { int cp2 = _cursorPositions.ContainsKey(fieldKey) ? System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length) : content.Length; Vector2 ts = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp2))); GUI.Label(new Rect(_fieldRects[fieldKey].x + ts.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|"); } } }
    }

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawImpostor();

        GUILayout.Space(15);

        DrawShapeshifter();

        GUILayout.Space(15);

        DrawCrewmate();

        GUILayout.Space(15);

        DrawTracker();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawEngineer();

        GUILayout.Space(15);

        DrawScientist();

        GUILayout.Space(15);

        DrawDetective();
        DrawPhantom();

        GUILayout.Space(15);

        DrawGuardianAngel();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.setFakeRole = GUIStylePreset.CustomToggle(CheatToggles.setFakeRole, " Set Fake Role");

        CheatToggles.setFakeAlive = GUIStylePreset.CustomToggle(CheatToggles.setFakeAlive, " Set Fake Alive");
    }

    private void DrawImpostor()
    {
        GUILayout.Label("Impostor", GUIStylePreset.TabSubtitle);

        CheatToggles.killReach = GUIStylePreset.CustomToggle(CheatToggles.killReach, " Kill Reach");
        if (CheatToggles.killReach)
        {
            CheatToggles.killReachInfinite = GUIStylePreset.CustomToggle(CheatToggles.killReachInfinite, "   Infinite Reach");
            if (!CheatToggles.killReachInfinite)
            {
                GUILayout.Label($"   Range: {CheatToggles.killReachRange:F1}");
                CheatToggles.killReachRange = GUILayout.HorizontalSlider(CheatToggles.killReachRange, 0.5f, 20f);
            }
        }

        features.KillAura.Enabled = GUIStylePreset.CustomToggle(features.KillAura.Enabled, " Kill Aura");
        if (features.KillAura.Enabled)
        {
            features.KillAura.Telemurder = GUIStylePreset.CustomToggle(features.KillAura.Telemurder, "   Telemurder");
        features.KillAura.IgnoreCooldownAsHost = GUIStylePreset.CustomToggle(features.KillAura.IgnoreCooldownAsHost, "   Ignore Cooldown As Host");
            features.KillAura.InfiniteRange = GUIStylePreset.CustomToggle(features.KillAura.InfiniteRange, "   Infinite Range");
            if (!features.KillAura.InfiniteRange)
            {
                GUILayout.Label($"   Range: {features.KillAura.Range:F1}");
                features.KillAura.Range = GUILayout.HorizontalSlider(features.KillAura.Range, 0.5f, 20f);
            }
            GUILayout.Label($"   Fire Rate: {features.KillAura.FireRate:F2}s");
            features.KillAura.FireRate = GUILayout.HorizontalSlider(features.KillAura.FireRate, 0.01f, 2f);
            features.KillAura.RespectMeeting = GUIStylePreset.CustomToggle(features.KillAura.RespectMeeting, "   Disable In Meetings");
            features.KillAura.RespectVent = GUIStylePreset.CustomToggle(features.KillAura.RespectVent, "   Disable In Vents");
            features.KillAura.WaitAfterStart = GUIStylePreset.CustomToggle(features.KillAura.WaitAfterStart, "   Wait After Game Start");
            if (features.KillAura.WaitAfterStart)
            {
                GUILayout.Label($"   Start Delay: {features.KillAura.StartDelay:F0}s");
                features.KillAura.StartDelay = GUILayout.HorizontalSlider(features.KillAura.StartDelay, 1f, 30f);
            }
            CheatToggles.killOtherImpostors = GUIStylePreset.CustomToggle(CheatToggles.killOtherImpostors, "   Kill Other Impostors");
        }

        Roles.SkipSabotageChecks.SabotageInVents = GUIStylePreset.CustomToggle(Roles.SkipSabotageChecks.SabotageInVents, " Allow Sabotaging In Vents As Imposter");

        CheatToggles.impostorTasks = GUIStylePreset.CustomToggle(CheatToggles.impostorTasks, " Allow Tasks");
        KillImpostors.Enabled      = GUIStylePreset.CustomToggle(KillImpostors.Enabled,      " Kill Other Impostors");
    }

    private void DrawShapeshifter()
    {
        GUILayout.Label("Shapeshifter", GUIStylePreset.TabSubtitle);

        CheatToggles.noShapeshiftAnim = GUIStylePreset.CustomToggle(CheatToggles.noShapeshiftAnim, " No Ss Animation");

        CheatToggles.endlessSsDuration = GUIStylePreset.CustomToggle(CheatToggles.endlessSsDuration, " Endless Ss Duration");
        CheatToggles.noShapeshiftCooldown = GUIStylePreset.CustomToggle(CheatToggles.noShapeshiftCooldown, " No Shift Cooldown");
    }

    private void DrawCrewmate()
    {
        GUILayout.Label("Crewmate", GUIStylePreset.TabSubtitle);

        Roles.SkipSabotageChecks.SabotageAsCrewmate = GUIStylePreset.CustomToggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, " Sabotage As Crewmate");

        CheatToggles.showTasksMenu = GUIStylePreset.CustomToggle(CheatToggles.showTasksMenu, " Show Tasks Menu");
        CheatToggles.instantPet = GUIStylePreset.CustomToggle(CheatToggles.instantPet, " Instant Pet");
        CheatToggles.spamPet = GUIStylePreset.CustomToggle(CheatToggles.spamPet, " Spam Pet");
        if (CheatToggles.spamPet)
        {
            GUILayout.Label($"   Delay: {CheatToggles.spamPetDelay:F2}s");
            CheatToggles.spamPetDelay = Mathf.Round(GUILayout.HorizontalSlider(CheatToggles.spamPetDelay, 0.01f, 1f) * 100f) / 100f;
        }
        if (GUILayout.Button("Break Pet"))
        {
            MalumCheats.BreakPetCheat();
            SkidMenu.notifications.Send("Pet", "Pet broken.", 5);
        }
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleCustomTextField(ref _scaleHInput, "tasksScaleH");
        GUILayout.Label("%  Vertical:", GUILayout.Width(70));
        HandleCustomTextField(ref _scaleVInput, "tasksScaleV");
        GUILayout.Label("%", GUILayout.Width(20));
        if (GUILayout.Button("Apply", GUILayout.Width(60))) { if (float.TryParse(_scaleHInput, out var ph)) CheatToggles.tasksScaleH = System.Math.Clamp(ph, 50f, 300f); if (float.TryParse(_scaleVInput, out var pv)) CheatToggles.tasksScaleV = System.Math.Clamp(pv, 50f, 300f); }
        GUILayout.EndHorizontal();
    }

    private void DrawTracker()
    {
        GUILayout.Label("Tracker", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessTracking = GUIStylePreset.CustomToggle(CheatToggles.endlessTracking, " Endless Tracking");

        CheatToggles.noTrackingDelay = GUIStylePreset.CustomToggle(CheatToggles.noTrackingDelay, " No Track Delay");

        CheatToggles.noTrackingCooldown = GUIStylePreset.CustomToggle(CheatToggles.noTrackingCooldown, " No Track Cooldown");

        CheatToggles.trackReach = GUIStylePreset.CustomToggle(CheatToggles.trackReach, " Track Reach");
    }

    private void DrawEngineer()
    {
        GUILayout.Label("Engineer", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessVentTime = GUIStylePreset.CustomToggle(CheatToggles.endlessVentTime, " Endless Vent Time");

        CheatToggles.noVentCooldown = GUIStylePreset.CustomToggle(CheatToggles.noVentCooldown, " No Vent Cooldown");
    }

    private void DrawScientist()
    {
        GUILayout.Label("Scientist", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessBattery = GUIStylePreset.CustomToggle(CheatToggles.endlessBattery, " Endless Battery");

        CheatToggles.noVitalsCooldown = GUIStylePreset.CustomToggle(CheatToggles.noVitalsCooldown, " No Vitals Cooldown");
    }

    private void DrawPhantom()
    {
        GUILayout.Label("Phantom", GUIStylePreset.TabSubtitle);
        CheatToggles.noVanishCooldown = GUIStylePreset.CustomToggle(CheatToggles.noVanishCooldown, " No Vanish Cooldown");
        CheatToggles.endlessVanishDuration = GUIStylePreset.CustomToggle(CheatToggles.endlessVanishDuration, " Endless Vanish Duration");
    }

    private void DrawDetective()
    {
        GUILayout.Label("Detective", GUIStylePreset.TabSubtitle);

        CheatToggles.interrogateReach = GUIStylePreset.CustomToggle(CheatToggles.interrogateReach, " Interrogate Reach");
    }

    private void DrawGuardianAngel()
    {
        GUILayout.Label("Guardian Angel", GUIStylePreset.TabSubtitle);

        CheatToggles.gaInfiniteRange = GUIStylePreset.CustomToggle(CheatToggles.gaInfiniteRange, " Infinite Protect Range");

        CheatToggles.gaIgnoreImpostors = GUIStylePreset.CustomToggle(CheatToggles.gaIgnoreImpostors, " Ignore Impostors");
    }
}






