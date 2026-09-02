using UnityEngine;
using System.Collections.Generic;

namespace SkidMenu;

public class ConsoleTab : ITab
{
    public string name => "Console";

    private string _conScaleHInput = "100", _conScaleVInput = "100";
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
        if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (!_cursorPositions.ContainsKey(fieldKey)) _cursorPositions[fieldKey] = content.Length;
            int cp = System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length);
            if (Event.current.keyCode == KeyCode.Backspace && cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; Event.current.Use(); }
            else if (char.IsDigit(Event.current.character) && content.Length < 3) { content = content.Substring(0, cp) + Event.current.character + content.Substring(cp); cp++; Event.current.Use(); }
            _cursorPositions[fieldKey] = System.Math.Clamp(cp, 0, content.Length);
        }
        if (_fieldRects.ContainsKey(fieldKey))
        {
            GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content);
            if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey])
            {
                if (!_lastBlinkTime.ContainsKey(fieldKey)) _lastBlinkTime[fieldKey] = Time.time;
                if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime) { _cursorVisible[fieldKey] = !_cursorVisible[fieldKey]; _lastBlinkTime[fieldKey] = Time.time; }
                if (_cursorVisible.ContainsKey(fieldKey) && _cursorVisible[fieldKey]) { int cp2 = _cursorPositions.ContainsKey(fieldKey) ? System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length) : content.Length; Vector2 ts = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp2))); GUI.Label(new Rect(_fieldRects[fieldKey].x + ts.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|"); }
            }
        }
    }

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        CheatToggles.showConsole = GUIStylePreset.CustomToggle(CheatToggles.showConsole, " Show Console");

        GUILayout.Space(5);
        GUILayout.Label("-- Players --");
        CheatToggles.logDeaths           = GUIStylePreset.CustomToggle(CheatToggles.logDeaths, " Log Kills / Deaths");
        CheatToggles.logVentIn            = GUIStylePreset.CustomToggle(CheatToggles.logVentIn, " Log Vent In");
        CheatToggles.logVentOut           = GUIStylePreset.CustomToggle(CheatToggles.logVentOut, " Log Vent Out");
        CheatToggles.logShapeshiftInto    = GUIStylePreset.CustomToggle(CheatToggles.logShapeshiftInto, " Log Shapeshift Into");
        CheatToggles.logShapeshiftRevert  = GUIStylePreset.CustomToggle(CheatToggles.logShapeshiftRevert, " Log Shapeshift Revert");
        CheatToggles.logPhantomVanish     = GUIStylePreset.CustomToggle(CheatToggles.logPhantomVanish, " Log Phantom Vanish");
        CheatToggles.logPhantomReappear   = GUIStylePreset.CustomToggle(CheatToggles.logPhantomReappear, " Log Phantom Reappear");
        CheatToggles.logDisconnects   = GUIStylePreset.CustomToggle(CheatToggles.logDisconnects, " Log Disconnects");
        CheatToggles.logJoins         = GUIStylePreset.CustomToggle(CheatToggles.logJoins, " Log Joins");
        CheatToggles.logTaskCompleted = GUIStylePreset.CustomToggle(CheatToggles.logTaskCompleted, " Log Task Completed");
        CheatToggles.logGuardianProtect = GUIStylePreset.CustomToggle(CheatToggles.logGuardianProtect, " Log Guardian Protect");
        CheatToggles.logKillAttempt     = GUIStylePreset.CustomToggle(CheatToggles.logKillAttempt, " Log Kill Attempts");

        GUILayout.Space(5);
        GUILayout.Label("-- Meetings --");
        CheatToggles.logMeetingCalled = GUIStylePreset.CustomToggle(CheatToggles.logMeetingCalled, " Log Meetings Called");
        CheatToggles.logBodyReport    = GUIStylePreset.CustomToggle(CheatToggles.logBodyReport, " Log Body Reports");
        CheatToggles.logEjections = GUIStylePreset.CustomToggle(CheatToggles.logEjections, " Log Ejections");
        CheatToggles.logVotes     = GUIStylePreset.CustomToggle(CheatToggles.logVotes, " Log Votes");
        CheatToggles.logVotekicks = GUIStylePreset.CustomToggle(CheatToggles.logVotekicks, " Log Votekicks");
        CheatToggles.logVerdict   = GUIStylePreset.CustomToggle(CheatToggles.logVerdict, " Log Judge Verdicts");
        CheatToggles.logVerdictLive = GUIStylePreset.CustomToggle(CheatToggles.logVerdictLive, " Log Judge Gavel");

        GUILayout.Space(5);
        GUILayout.Label("-- Game --");
        CheatToggles.logSabotages = GUIStylePreset.CustomToggle(CheatToggles.logSabotages, " Log Sabotages");
        CheatToggles.logSabotageFix = GUIStylePreset.CustomToggle(CheatToggles.logSabotageFix, " Log Sabotage Fixes");
        CheatToggles.logCameras = GUIStylePreset.CustomToggle(CheatToggles.logCameras, " Log Cameras / Vitals");
        CheatToggles.logRoomEntry = GUIStylePreset.CustomToggle(CheatToggles.logRoomEntry, " Log Room Entries");
        CheatToggles.logGameOver = GUIStylePreset.CustomToggle(CheatToggles.logGameOver, " Log Round Start / Game Over");
        CheatToggles.logChat      = GUIStylePreset.CustomToggle(CheatToggles.logChat, " Log Chat Messages");
        CheatToggles.logZipline   = GUIStylePreset.CustomToggle(CheatToggles.logZipline, " Log Zipline");
        CheatToggles.logPlatform  = GUIStylePreset.CustomToggle(CheatToggles.logPlatform, " Log Platform");
        CheatToggles.logLadder    = GUIStylePreset.CustomToggle(CheatToggles.logLadder, " Log Ladder");

        GUILayout.Space(5);
        GUILayout.Label("-- Settings --");
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleCustomTextField(ref _conScaleHInput, "conScaleH");
        GUILayout.Label("%  Vertical:", GUILayout.Width(70));
        HandleCustomTextField(ref _conScaleVInput, "conScaleV");
        GUILayout.Label("%", GUILayout.Width(20));
        if (GUILayout.Button("Apply", GUILayout.Width(60))) { if (float.TryParse(_conScaleHInput, out var h)) CheatToggles.consoleScaleH = System.Math.Clamp(h, 50f, 300f); if (float.TryParse(_conScaleVInput, out var v)) CheatToggles.consoleScaleV = System.Math.Clamp(v, 50f, 300f); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Max Log Entries: {CheatToggles.maxLogEntries}", GUILayout.Width(220));
        int newMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(CheatToggles.maxLogEntries, 50, 2000, GUILayout.Width(150)));
        if (newMax != CheatToggles.maxLogEntries) CheatToggles.maxLogEntries = newMax;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}
