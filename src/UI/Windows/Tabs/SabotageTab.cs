using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;

public class SabotageTab : ITab
{
    public string name => "Ship";

    public static Dictionary<SystemTypes, bool> SpamIndividual = new();

    private static Dictionary<string, SystemTypes> _cachedSabotages;
    private static Dictionary<string, SystemTypes> _cachedDoors;
    private static ShipStatus _lastShip;

    private string _doorsScaleHInput = CheatToggles.doorsScaleH.ToString("F0");
    private string _doorsScaleVInput = CheatToggles.doorsScaleV.ToString("F0");
    private Dictionary<string, bool>  _focusedFields   = new();
    private Dictionary<string, float> _lastBlinkTime   = new();
    private Dictionary<string, bool>  _cursorVisible   = new();
    private Dictionary<string, Rect>  _fieldRects      = new();
    private Dictionary<string, int>   _cursorPositions = new();
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

    private static void RefreshCache()
    {
        if (ShipStatus.Instance == _lastShip && _cachedSabotages != null) return;
        _lastShip        = ShipStatus.Instance;
        _cachedSabotages = ShipStatus.Instance != null ? Sabotage.GetSabotages() : new();
        _cachedDoors     = ShipStatus.Instance != null ? Sabotage.GetDoors()     : new();
    }

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        // ── LEFT COLUMN ───────────────────────────────────────────────
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.38f));

        GUILayout.Label("General", GUIStylePreset.TabSubtitle);
        CheatToggles.fakeTasks           = GUIStylePreset.CustomToggle(CheatToggles.fakeTasks,           " Fake Tasks");
        CheatToggles.doAnyTask           = GUIStylePreset.CustomToggle(CheatToggles.doAnyTask,           " Do Any Task");
        CheatToggles.unfixableLights     = GUIStylePreset.CustomToggle(CheatToggles.unfixableLights,     " Unfixable Lights");
        CheatToggles.callMeeting         = GUIStylePreset.CustomToggle(CheatToggles.callMeeting,         " Call Meeting");
        CheatToggles.reportBody          = GUIStylePreset.CustomToggle(CheatToggles.reportBody,          " Report Body");
        CheatToggles.closeMeeting        = GUIStylePreset.CustomToggle(CheatToggles.closeMeeting,        " Close Meeting");
        CheatToggles.autoOpenDoorsOnUse  = GUIStylePreset.CustomToggle(CheatToggles.autoOpenDoorsOnUse,  " Auto-Open Doors On Use");
        CheatToggles.kickOffensiveNames  = GUIStylePreset.CustomToggle(CheatToggles.kickOffensiveNames,  " Kick Offensive Names");
        CheatToggles.sabotageMap         = GUIStylePreset.CustomToggle(CheatToggles.sabotageMap,         " Open Sabotage Map");

        GUILayout.Space(8);
        GUILayout.Label("Vents", GUIStylePreset.TabSubtitle);
        CheatToggles.unlockVents = GUIStylePreset.CustomToggle(CheatToggles.unlockVents, " Unlock Vents");
        CheatToggles.kickVents   = GUIStylePreset.CustomToggle(CheatToggles.kickVents,   " Kick All From Vents");
        CheatToggles.walkInVents = GUIStylePreset.CustomToggle(CheatToggles.walkInVents, " Walk In Vents");

        GUILayout.Space(8);
        GUILayout.Label("Fungus", GUIStylePreset.TabSubtitle);
        CheatToggles.mushSab   = GUIStylePreset.CustomToggle(CheatToggles.mushSab,   " Mushroom Mixup");
        CheatToggles.mushSpore = GUIStylePreset.CustomToggle(CheatToggles.mushSpore, " Trigger Spores");

        GUILayout.EndVertical();

        GUILayout.Space(8);

        // ── RIGHT COLUMN ──────────────────────────────────────────────
        GUILayout.BeginVertical();

        Sabotage.UpdateSystemsDirectly = GUIStylePreset.CustomToggle(Sabotage.UpdateSystemsDirectly, " Update Sabotage Systems Directly");

        GUILayout.Space(6);
        GUILayout.Label("Sabotage All", GUIStylePreset.TabSubtitle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sabotage All"))     { Sabotage.SabotageAll();     SkidMenu.notifications.Send("Sabotage", "All sabotages have been enabled.", 5); }
        if (GUILayout.Button("Fix All Sabotages")){ Sabotage.FixAllSabotages(); SkidMenu.notifications.Send("Sabotage", "All sabotages have been repaired.", 5); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        CheatToggles.spamSabotageAll = GUIStylePreset.CustomToggle(CheatToggles.spamSabotageAll, " Spam Sabotage All");
        CheatToggles.spamFixAll      = GUIStylePreset.CustomToggle(CheatToggles.spamFixAll,      " Spam Fix All");
        GUILayout.EndHorizontal();

        if (ShipStatus.Instance != null)
        {
            RefreshCache();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random Sabotage"))
            {
                var sabotages = Sabotage.GetSabotages();
                if (sabotages.Count > 0)
                {
                    int pick = UnityEngine.Random.Range(0, sabotages.Count);
                    int idx = 0;
                    string picked = "";
                    SystemTypes pickedType = default;
                    foreach (var (key, value) in sabotages) { if (idx == pick) { picked = key; pickedType = value; break; } idx++; }
                    Sabotage.SabotageSystem(pickedType);
                    SkidMenu.notifications.Send("Sabotage", $"Random sabotage: {picked}", 5);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Individual Sabotages", GUIStylePreset.TabSubtitle);
            foreach (var (key, value) in _cachedSabotages)
            {
                if (!SpamIndividual.ContainsKey(value)) SpamIndividual[value] = false;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(key)) { Sabotage.SabotageSystem(value); SkidMenu.notifications.Send("Sabotage", $"{key} has been sabotaged.", 5); }
                SpamIndividual[value] = GUIStylePreset.CustomToggle(SpamIndividual[value], " Spam", GUILayout.Width(65));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            GUILayout.Label("Doors", GUIStylePreset.TabSubtitle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close All Doors"))
            {
                Sabotage.LockAll();
                SkidMenu.notifications.Send("Sabotage", "All doors have been closed.", 5);
            }
            if (GUILayout.Button("Unlock All Doors"))
            {
                if (Sabotage.CanUnlockDoors()) { Sabotage.UnlockAll(); SkidMenu.notifications.Send("Sabotage", "All doors have been unlocked.", 5); }
                else SkidMenu.notifications.Send("Sabotage", "This map does not support unlocking doors.", 10);
            }
            GUILayout.EndHorizontal();

            CheatToggles.showDoorsMenu = GUIStylePreset.CustomToggle(CheatToggles.showDoorsMenu, " Show Doors Menu");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scale H:", GUILayout.Width(60));
            HandleCustomTextField(ref _doorsScaleHInput, "doorsScaleH");
            GUILayout.Label("%  V:", GUILayout.Width(35));
            HandleCustomTextField(ref _doorsScaleVInput, "doorsScaleV");
            GUILayout.Label("%", GUILayout.Width(15));
            if (GUILayout.Button("Apply", GUILayout.Width(55)))
            {
                if (float.TryParse(_doorsScaleHInput, out var ph)) CheatToggles.doorsScaleH = System.Math.Clamp(ph, 50f, 300f);
                if (float.TryParse(_doorsScaleVInput, out var pv)) CheatToggles.doorsScaleV = System.Math.Clamp(pv, 50f, 300f);
            }
            GUILayout.EndHorizontal();

            if (_cachedDoors.Count > 0)
            {
                GUILayout.Space(4);
                GUILayout.Label("Close Individual Doors", GUIStylePreset.TabSubtitle);
                byte i = 0;
                foreach (var (key, value) in _cachedDoors)
                {
                    if (i % 2 == 0) GUILayout.BeginHorizontal();
                    if (GUILayout.Button(key)) Sabotage.LockDoor(value);
                    if (i % 2 != 0) GUILayout.EndHorizontal();
                    i++;
                }
                if (i % 2 != 0) GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Space(6);
            GUILayout.Label("<color=#888888>Join a game to use sabotage actions.</color>", GUIStylePreset.ModernLabel);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}

