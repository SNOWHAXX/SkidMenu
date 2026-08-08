using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;

public class ShipTab : ITab
{
    public string name => "Ship";

    private string _doorsScaleHInput = CheatToggles.doorsScaleH.ToString("F0");
    private string _doorsScaleVInput = CheatToggles.doorsScaleV.ToString("F0");
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

        DrawSabotage();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawVents();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        // Will implement this later, currently gets user kicked by AC. -ADHyperActive
        // CheatToggles.completeAllTasks = GUIStylePreset.CustomToggle(CheatToggles.completeAllTasks, " Allow All Tasks");
        
        CheatToggles.fakeTasks = GUIStylePreset.CustomToggle(CheatToggles.fakeTasks, " Fake Tasks");

        CheatToggles.doAnyTask = GUIStylePreset.CustomToggle(CheatToggles.doAnyTask, " Do Any Task");

        CheatToggles.unfixableLights = GUIStylePreset.CustomToggle(CheatToggles.unfixableLights, " Unfixable Lights");

        CheatToggles.callMeeting = GUIStylePreset.CustomToggle(CheatToggles.callMeeting, " Call Meeting");

        CheatToggles.reportBody = GUIStylePreset.CustomToggle(CheatToggles.reportBody, " Report Body");

        CheatToggles.closeMeeting = GUIStylePreset.CustomToggle(CheatToggles.closeMeeting, " Close Meeting");

        CheatToggles.autoOpenDoorsOnUse = GUIStylePreset.CustomToggle(CheatToggles.autoOpenDoorsOnUse, " Auto-Open Doors On Use");

        CheatToggles.kickOffensiveNames = GUIStylePreset.CustomToggle(CheatToggles.kickOffensiveNames, " Kick Offensive Names");
    }

    private void DrawSabotage()
    {
        GUILayout.Label("Sabotage", GUIStylePreset.TabSubtitle);

        CheatToggles.reactorSab = GUIStylePreset.CustomToggle(CheatToggles.reactorSab, " Reactor");

        CheatToggles.oxygenSab = GUIStylePreset.CustomToggle(CheatToggles.oxygenSab, " Oxygen");

        CheatToggles.elecSab = GUIStylePreset.CustomToggle(CheatToggles.elecSab, " Lights");

        CheatToggles.commsSab = GUIStylePreset.CustomToggle(CheatToggles.commsSab, " Comms");

        CheatToggles.showDoorsMenu = GUIStylePreset.CustomToggle(CheatToggles.showDoorsMenu, " Show Doors Menu");
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleCustomTextField(ref _doorsScaleHInput, "doorsScaleH");
        GUILayout.Label("%  Vertical:", GUILayout.Width(70));
        HandleCustomTextField(ref _doorsScaleVInput, "doorsScaleV");
        if (GUILayout.Button("Apply", GUILayout.Width(60))) { if (float.TryParse(_doorsScaleHInput, out var ph)) CheatToggles.doorsScaleH = System.Math.Clamp(ph, 50f, 300f); if (float.TryParse(_doorsScaleVInput, out var pv)) CheatToggles.doorsScaleV = System.Math.Clamp(pv, 50f, 300f); }
        GUILayout.EndHorizontal();

        CheatToggles.mushSab = GUIStylePreset.CustomToggle(CheatToggles.mushSab, " Mushroom Mixup");

        CheatToggles.mushSpore = GUIStylePreset.CustomToggle(CheatToggles.mushSpore, " Trigger Spores");

        CheatToggles.sabotageMap = GUIStylePreset.CustomToggle(CheatToggles.sabotageMap, " Open Sabotage Map");
    }

    private void DrawVents()
    {
        GUILayout.Label("Vents", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockVents = GUIStylePreset.CustomToggle(CheatToggles.unlockVents, " Unlock Vents");

        CheatToggles.kickVents = GUIStylePreset.CustomToggle(CheatToggles.kickVents, " Kick All From Vents");

        CheatToggles.walkInVents = GUIStylePreset.CustomToggle(CheatToggles.walkInVents, " Walk In Vents");
    }
}
