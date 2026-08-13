using UnityEngine;
using System.Globalization;
using System.Collections.Generic;

namespace SkidMenu;

public class SpoofingTab : ITab
{
    public string name => "Spoofing";

    private static string _spoofLevelInput = "";
    private static string _spoofPlatformInput = "";
    private static string _spoofXboxIdInput = "";
    private static string _spoofPsnIdInput = "";
    private static string _customNameInput = "";
    private static int _nameModeIndex = 0;
    private static int _nameLength = 10;
    private string _randomMinInput = "1";
    private string _randomMaxInput = "100001";
    public static float frRpcDelayTemp = 0.10f;
    public static bool  frSpamEnabled  = false;
    public static float frSpamDelay    = 1.0f;

    private bool _initialized = false;
    private bool _platformExcludeOpen = false;

    private static readonly string[] AllPlatforms = {
        "Unknown", "StandaloneEpicPC", "StandaloneSteamPC", "StandaloneMac",
        "StandaloneWin10", "StandaloneItch", "IPhone", "Android", "Switch", "Xbox", "Playstation"
    };

    private static HashSet<string> _excludedPlatforms = new();

    private Dictionary<string, bool> _focusedFields = new();
    private Dictionary<string, float> _lastBlinkTime = new();
    private Dictionary<string, bool> _cursorVisible = new();
    private Dictionary<string, Rect> _fieldRects = new();
    private Dictionary<string, int> _cursorPositions = new();
    private float _cursorBlinkTime = 0.5f;

    // Cached per-frame allocations
    private GUIStyle         _fullRandStyle;
    private GUILayoutOption  _w100;
    private GUILayoutOption  _w155;
    private GUILayoutOption  _h36;
    private GUIContent       _cursorContent = new GUIContent(string.Empty);
    private bool             _layoutOptsCached;

    private void EnsureLayoutOpts()
    {
        if (_layoutOptsCached) return;
        _w100 = GUILayout.Width(100);
        _w155 = GUILayout.Width(155);
        _h36  = GUILayout.Height(36);
        _layoutOptsCached = true;
    }

    private GUIStyle GetFullRandStyle()
    {
        if (_fullRandStyle != null) return _fullRandStyle;
        var bg = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
        _fullRandStyle = new GUIStyle(GUI.skin.button);
        _fullRandStyle.normal.background = bg;
        _fullRandStyle.hover.background  = bg;
        _fullRandStyle.active.background = bg;
        return _fullRandStyle;
    }

    public void Draw()
    {
        if (!_initialized)
        {
            InitializeInputFields();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawSpoofingSettings();
        GUILayout.Space(15);
        DrawNameSpooferSettings();
        GUILayout.Space(15);
        DrawFullyRandomize();

        GUILayout.EndVertical();
    }

    private void InitializeInputFields()
    {
        _spoofLevelInput = SkidMenu.spoofLevel;
        _spoofPlatformInput = SkidMenu.spoofPlatform;
        _spoofXboxIdInput = features.Spoofer.spoofedXboxId.ToString();
        _spoofPsnIdInput = features.Spoofer.spoofedPsnId.ToString();
        _randomMinInput = SkidMenu.spoofLevelRandomMin.ToString();
        _randomMaxInput = SkidMenu.spoofLevelRandomMax.ToString();
        _nameModeIndex = (int)features.NameSpoofer.Mode;
        _nameLength = features.NameSpoofer.RandomLength;
        frRpcDelayTemp = SkidMenu.frRpcDelay;

        _excludedPlatforms.Clear();
        var saved = SkidMenu.spoofPlatformExclusions;
        if (!string.IsNullOrWhiteSpace(saved))
        {
            foreach (var entry in saved.Split(','))
            {
                var trimmed = entry.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _excludedPlatforms.Add(trimmed);
            }
        }

        foreach (var k in new[] { "spoofLevel", "spoofPlatform", "randomMin", "randomMax", "customName", "xboxId", "psnId" })
        {
            _focusedFields[k] = false;
            _cursorVisible[k] = true;
            _lastBlinkTime[k] = 0f;
            _cursorPositions[k] = 0;
        }

        _cursorPositions["spoofLevel"]    = _spoofLevelInput.Length;
        _cursorPositions["spoofPlatform"] = _spoofPlatformInput.Length;
        _cursorPositions["randomMin"]     = _randomMinInput.Length;
        _cursorPositions["randomMax"]     = _randomMaxInput.Length;
        _cursorPositions["customName"]    = _customNameInput.Length;
        _cursorPositions["xboxId"]        = _spoofXboxIdInput.Length;
        _cursorPositions["psnId"]         = _spoofPsnIdInput.Length;
    }

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 200, int height = 20)
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(width), GUILayout.Height(height));

        if (Event.current.type == EventType.Repaint)
            _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();

        if (!_focusedFields.ContainsKey(fieldKey))
            _focusedFields[fieldKey] = false;

        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey))
        {
            if (_fieldRects[fieldKey].Contains(Event.current.mousePosition))
            {
                _focusedFields[fieldKey] = true;
                _lastBlinkTime[fieldKey] = Time.time;
                _cursorVisible[fieldKey] = true;
                Event.current.Use();
            }
            else
            {
                _focusedFields[fieldKey] = false;
            }
        }

        if (_focusedFields[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (!_cursorPositions.ContainsKey(fieldKey)) _cursorPositions[fieldKey] = content.Length;
            int cp = _cursorPositions[fieldKey];
            cp = System.Math.Clamp(cp, 0, content.Length);

            bool ctrl = Event.current.control || Event.current.command;

            if (ctrl && Event.current.keyCode == KeyCode.C)
            {
                GUIUtility.systemCopyBuffer = content;
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.X)
            {
                GUIUtility.systemCopyBuffer = content;
                content = "";
                cp = 0;
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.V)
            {
                string clip = GUIUtility.systemCopyBuffer ?? "";
                var sb = new System.Text.StringBuilder();
                foreach (char c in clip) if (!char.IsControl(c)) sb.Append(c);
                clip = sb.ToString();
                content = content.Substring(0, cp) + clip + content.Substring(cp);
                cp = System.Math.Clamp(cp + clip.Length, 0, content.Length);
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.A)
            {
                GUIUtility.systemCopyBuffer = content;
                cp = content.Length;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; }
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Delete)
            {
                if (cp < content.Length) content = content.Substring(0, cp) + content.Substring(cp + 1);
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.LeftArrow)
            {
                if (cp > 0) cp--;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.RightArrow)
            {
                if (cp < content.Length) cp++;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Home)
            {
                cp = 0;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.End)
            {
                cp = content.Length;
                Event.current.Use();
            }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            {
                content = content.Substring(0, cp) + Event.current.character + content.Substring(cp);
                cp++;
                Event.current.Use();
            }

            _cursorPositions[fieldKey] = System.Math.Clamp(cp, 0, content.Length);
        }

        if (_fieldRects.ContainsKey(fieldKey))
        {
            GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content);

            if (_focusedFields[fieldKey])
            {
                if (!_lastBlinkTime.ContainsKey(fieldKey)) _lastBlinkTime[fieldKey] = Time.time;
                if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime)
                {
                    _cursorVisible[fieldKey] = !_cursorVisible[fieldKey];
                    _lastBlinkTime[fieldKey] = Time.time;
                }
                if (_cursorVisible[fieldKey])
                {
                    int cp2 = _cursorPositions.ContainsKey(fieldKey) ? System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length) : content.Length;
                    _cursorContent.text = content.Substring(0, cp2); Vector2 textSize = GUI.skin.label.CalcSize(_cursorContent);
                    GUI.Label(new Rect(_fieldRects[fieldKey].x + textSize.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|");
                }
            }
        }
    }

    private void DrawSpoofingSettings()
    {
        GUILayout.Label("Spoofing Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Level (1-100001):", GUILayout.Width(150));
        HandleCustomTextField(ref _spoofLevelInput, "spoofLevel", 150);
        if (GUILayout.Button("Save", GUILayout.Width(50)))
        {
            if (int.TryParse(_spoofLevelInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) && level >= 1 && level <= 100001)
                SkidMenu.spoofLevel = _spoofLevelInput;
            else
                _spoofLevelInput = SkidMenu.spoofLevel;
        }

        string prevMin = _randomMinInput;
        string prevMax = _randomMaxInput;
        HandleCustomTextField(ref _randomMinInput, "randomMin", 50);
        HandleCustomTextField(ref _randomMaxInput, "randomMax", 50);

        if (_randomMinInput != prevMin && int.TryParse(_randomMinInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedMin) && parsedMin >= 1 && parsedMin <= 100001)
            SkidMenu.spoofLevelRandomMin = parsedMin;
        if (_randomMaxInput != prevMax && int.TryParse(_randomMaxInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedMax) && parsedMax >= 1 && parsedMax <= 100001)
            SkidMenu.spoofLevelRandomMax = parsedMax;

        if (GUILayout.Button("Random", GUILayout.Width(60)))
        {
            int mn = SkidMenu.spoofLevelRandomMin;
            int mx = SkidMenu.spoofLevelRandomMax;
            if (mn > mx) { int t = mn; mn = mx; mx = t; }
            _spoofLevelInput = UnityEngine.Random.Range(mn, mx + 1).ToString();
            SkidMenu.spoofLevel = _spoofLevelInput;
        }
        if (GUILayout.Button("Disable", GUILayout.Width(60)))
        {
            SkidMenu.spoofLevel = "";
            _spoofLevelInput = "";
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Platform:", GUILayout.Width(150));
        HandleCustomTextField(ref _spoofPlatformInput, "spoofPlatform", 150);
        if (GUILayout.Button("Save", GUILayout.Width(50)))
        {
            SkidMenu.spoofPlatform = _spoofPlatformInput;
            if (Utils.StringToPlatformType(_spoofPlatformInput, out Platforms? savedPlatform))
                features.Spoofer.spoofedPlatform = (Platforms)savedPlatform;
        }
        if (GUILayout.Button("Random", GUILayout.Width(60)))
        {
            var pool = new List<string>();
            foreach (var p in AllPlatforms) if (!_excludedPlatforms.Contains(p)) pool.Add(p);
            if (pool.Count > 0)
            {
                var picked = pool[UnityEngine.Random.Range(0, pool.Count)];
                _spoofPlatformInput = picked;
                SkidMenu.spoofPlatform = picked;
                if (System.Enum.TryParse<Platforms>(picked, out var pickedPlatform))
                    features.Spoofer.spoofedPlatform = pickedPlatform;
            }
        }
        if (GUILayout.Button("Disable", GUILayout.Width(60)))
        {
            SkidMenu.spoofPlatform = "";
            _spoofPlatformInput = "";
            features.Spoofer.spoofedPlatform = Constants.GetPlatformType();
        }
        var excludeLabel = _platformExcludeOpen ? "Exclude ▲" : "Exclude ▼";
        if (GUILayout.Button(excludeLabel, GUILayout.Width(70))) _platformExcludeOpen = !_platformExcludeOpen;
        GUILayout.EndHorizontal();

        bool spoofingXbox = _spoofPlatformInput.Equals("Xbox", System.StringComparison.OrdinalIgnoreCase);
        bool spoofingPsn  = _spoofPlatformInput.Equals("Playstation", System.StringComparison.OrdinalIgnoreCase);
        if (spoofingXbox || spoofingPsn)
        {
            GUILayout.Space(5);
            if (spoofingXbox)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Xbox XUID:", GUILayout.Width(150));
                HandleCustomTextField(ref _spoofXboxIdInput, "xboxId", 150);
                if (GUILayout.Button("Random", GUILayout.Width(60)))
                {
                    _spoofXboxIdInput = GenerateRandomPlatformId();
                    features.Spoofer.spoofedXboxId = ulong.Parse(_spoofXboxIdInput);
                }
                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    if (ulong.TryParse(_spoofXboxIdInput, out var xid) && _spoofXboxIdInput.Length <= 20)
                        features.Spoofer.spoofedXboxId = xid;
                    else
                        _spoofXboxIdInput = features.Spoofer.spoofedXboxId.ToString();
                }
                GUILayout.EndHorizontal();
            }
            if (spoofingPsn)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("PSN ID:", GUILayout.Width(150));
                HandleCustomTextField(ref _spoofPsnIdInput, "psnId", 150);
                if (GUILayout.Button("Random", GUILayout.Width(60)))
                {
                    _spoofPsnIdInput = GenerateRandomPlatformId();
                    features.Spoofer.spoofedPsnId = ulong.Parse(_spoofPsnIdInput);
                }
                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    if (ulong.TryParse(_spoofPsnIdInput, out var pid) && _spoofPsnIdInput.Length <= 20)
                        features.Spoofer.spoofedPsnId = pid;
                    else
                        _spoofPsnIdInput = features.Spoofer.spoofedPsnId.ToString();
                }
                GUILayout.EndHorizontal();
            }
        }

        if (_platformExcludeOpen)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Exclude from Random:", GUILayout.Width(150));
            for (int i = 0; i < AllPlatforms.Length; i += 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = i; j < i + 3 && j < AllPlatforms.Length; j++)
                {
                    var platform = AllPlatforms[j];
                    bool excluded = _excludedPlatforms.Contains(platform);
                    bool newExcluded = GUIStylePreset.CustomToggle(excluded, platform, GUILayout.Width(130));
                    if (newExcluded != excluded)
                    {
                        if (newExcluded) _excludedPlatforms.Add(platform);
                        else _excludedPlatforms.Remove(platform);
                        SkidMenu.spoofPlatformExclusions = string.Join(",", _excludedPlatforms);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        GUILayout.Space(5);
        GUILayout.Label("Supported Platforms: StandaloneEpicPC, StandaloneSteamPC, StandaloneMac, StandaloneWin10, etc.");
    }

    private static readonly string[] NameModeLabels =
    {
        "Random String", "Random Words", "Space Theme", "Among Us",
        "Leetspeak", "Zalgo", "Repeating", "Fake Tag",
        "Numbers Only", "Cursed Mix"
    };

    private void DrawNameSpooferSettings()
    {
        GUILayout.Label("Name Spoofer", GUIStylePreset.TabSubtitle);

        if (features.NameSpoofer.Enabled)
            GUILayout.Label($"Active: {features.NameSpoofer.SpoofedName}");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom Name:", GUILayout.Width(110));
        HandleCustomTextField(ref _customNameInput, "customName", 180);
        if (GUILayout.Button("Apply", GUILayout.Width(55)) && !string.IsNullOrWhiteSpace(_customNameInput))
        {
            features.NameSpoofer.ApplyName(_customNameInput);
            SkidMenu.nameSpoofName    = features.NameSpoofer.SpoofedName;
            SkidMenu.nameSpoofEnabled = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        GUILayout.Label("Random Mode:");
        for (int row = 0; row < NameModeLabels.Length; row += 4)
        {
            GUILayout.BeginHorizontal();
            for (int i = row; i < row + 4 && i < NameModeLabels.Length; i++)
            {
                if (_nameModeIndex == i) GUI.color = Color.green;
                if (GUILayout.Button(NameModeLabels[i], GUILayout.Width(110)))
                {
                    _nameModeIndex = i;
                    features.NameSpoofer.Mode = (features.NameSpoofer.RandomizerMode)i;
                    SkidMenu.nameSpoofMode = i;
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Length: {_nameLength}", GUILayout.Width(80));
        int newLength = (int)GUILayout.HorizontalSlider(_nameLength, 3, 10, GUILayout.Width(150));
        if (newLength != _nameLength) { _nameLength = newLength; features.NameSpoofer.RandomLength = _nameLength; SkidMenu.nameSpoofLength = _nameLength; }
        if (GUILayout.Button("Generate & Apply", GUILayout.Width(130)))
        {
            features.NameSpoofer.Mode = (features.NameSpoofer.RandomizerMode)_nameModeIndex;
            features.NameSpoofer.RandomLength = _nameLength;
            string generated = features.NameSpoofer.Generate();
            _customNameInput = generated;
            features.NameSpoofer.ApplyName(generated);
            SkidMenu.nameSpoofName    = features.NameSpoofer.SpoofedName;
            SkidMenu.nameSpoofEnabled = true;
        }
        if (GUILayout.Button("Disable", GUILayout.Width(65)))
        {
            features.NameSpoofer.Disable();
            _customNameInput = "";
            SkidMenu.nameSpoofEnabled = false;
            SkidMenu.nameSpoofName    = "";
        }
        GUILayout.EndHorizontal();
    }

    private void DrawFullyRandomize()
    {
        GUILayout.Label("Full Randomizer", GUIStylePreset.TabSubtitle);

        EnsureLayoutOpts();
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.15f, 0.85f, 1f);
        if (GUILayout.Button("\u2726 Fully Randomize \u2726", GetFullRandStyle(), _h36))
            DoFullyRandomize();
        GUI.backgroundColor = oldColor;

        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        frSpamEnabled = GUIStylePreset.CustomToggle(frSpamEnabled, " Spam Randomize", _w155);
        GUILayout.Label($"Interval: {frSpamDelay:F1}s", GUILayout.Width(85));
        frSpamDelay = Mathf.Round(GUILayout.HorizontalSlider(frSpamDelay, 0.1f, 10f) * 10f) / 10f;
        GUILayout.EndHorizontal();

        GUILayout.Label("What to randomize:", GUIStylePreset.TabSubtitle);
        GUILayout.BeginHorizontal();
        SkidMenu.frRandLevel    = GUIStylePreset.CustomToggle(SkidMenu.frRandLevel, " Level", _w100);
        SkidMenu.frRandPlatform = GUIStylePreset.CustomToggle(SkidMenu.frRandPlatform, " Platform", _w100);
        SkidMenu.frRandName    = GUIStylePreset.CustomToggle(SkidMenu.frRandName, " Name", _w100);
        SkidMenu.frRandColor   = GUIStylePreset.CustomToggle(SkidMenu.frRandColor, " Color", _w100);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        SkidMenu.frRandHat     = GUIStylePreset.CustomToggle(SkidMenu.frRandHat, " Hat", _w100);
        SkidMenu.frRandSkin    = GUIStylePreset.CustomToggle(SkidMenu.frRandSkin, " Skin", _w100);
        SkidMenu.frRandVisor   = GUIStylePreset.CustomToggle(SkidMenu.frRandVisor, " Visor", _w100);
        SkidMenu.frRandPet       = GUIStylePreset.CustomToggle(SkidMenu.frRandPet, " Pet", _w100);
        SkidMenu.frRandNameplate = GUIStylePreset.CustomToggle(SkidMenu.frRandNameplate, " Nameplate", _w100);
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"RPC Delay: {frRpcDelayTemp:F2}s", GUILayout.Width(110));
        frRpcDelayTemp = Mathf.Round(GUILayout.HorizontalSlider(frRpcDelayTemp, 0f, 1f, GUILayout.Width(180)) * 100f) / 100f;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Auto-Randomize Triggers", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnDeath, " After you die"); if (_fv != FullyRandomizeTriggers.OnDeath) { FullyRandomizeTriggers.OnDeath = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnKill, " After you kill someone"); if (_fv != FullyRandomizeTriggers.OnKill) { FullyRandomizeTriggers.OnKill = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnMeetingStart, " When a meeting starts"); if (_fv != FullyRandomizeTriggers.OnMeetingStart) { FullyRandomizeTriggers.OnMeetingStart = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnMeetingEnd, " When a meeting ends"); if (_fv != FullyRandomizeTriggers.OnMeetingEnd) { FullyRandomizeTriggers.OnMeetingEnd = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnLobbyLeave, " When leaving a lobby"); if (_fv != FullyRandomizeTriggers.OnLobbyLeave) { FullyRandomizeTriggers.OnLobbyLeave = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnGameEnd, " When a game ends"); if (_fv != FullyRandomizeTriggers.OnGameEnd) { FullyRandomizeTriggers.OnGameEnd = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnShapeshift, " When you shapeshift"); if (_fv != FullyRandomizeTriggers.OnShapeshift) { FullyRandomizeTriggers.OnShapeshift = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnVent, " When you enter a vent"); if (_fv != FullyRandomizeTriggers.OnVent) { FullyRandomizeTriggers.OnVent = _fv; } }
        { bool _fv = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnTaskComplete, " When you complete a task"); if (_fv != FullyRandomizeTriggers.OnTaskComplete) { FullyRandomizeTriggers.OnTaskComplete = _fv; } }
        FullyRandomizeTriggers.OnEjected        = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnEjected, " When you get ejected");
        FullyRandomizeTriggers.OnSabotage       = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnSabotage, " When a sabotage starts");
        FullyRandomizeTriggers.OnExitVent       = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnExitVent, " When you exit a vent");
        FullyRandomizeTriggers.OnShapeshiftBack = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnShapeshiftBack, " When you shapeshift back");
        FullyRandomizeTriggers.OnVanish         = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnVanish, " When you vanish (Phantom)");
        FullyRandomizeTriggers.OnReappear       = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnReappear, " When you reappear (Phantom)");
        FullyRandomizeTriggers.OnVotekicked     = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnVotekicked, " When you get votekicked");
        FullyRandomizeTriggers.OnPlayerJoin     = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnPlayerJoin, " When someone joins the lobby");
        FullyRandomizeTriggers.OnPlayerLeave    = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.OnPlayerLeave, " When someone leaves the lobby");

        GUILayout.Space(4);
        FullyRandomizeTriggers.ShowNotification = GUIStylePreset.CustomToggle(FullyRandomizeTriggers.ShowNotification, " Show notification on randomize");
    }

    private static string GenerateRandomPlatformId()
    {
        var sb = new System.Text.StringBuilder(16);
        sb.Append((char)('0' + UnityEngine.Random.Range(2, 10)));
        for (int i = 1; i < 16; i++)
            sb.Append((char)('0' + UnityEngine.Random.Range(0, 10)));
        return sb.ToString();
    }

    public static void DoFullyRandomize()
    {
        float delay = frRpcDelayTemp;

        int randomLevel = 1;
        if (SkidMenu.frRandLevel)
        {
            int minVal = SkidMenu.spoofLevelRandomMin;
            int maxVal = SkidMenu.spoofLevelRandomMax;
            if (minVal > maxVal) { int t = minVal; minVal = maxVal; maxVal = t; }
            randomLevel = UnityEngine.Random.Range(minVal, maxVal + 1);
            _spoofLevelInput = randomLevel.ToString();
            SkidMenu.spoofLevel = _spoofLevelInput;
        }

        if (SkidMenu.frRandPlatform)
        {
            var pool = new List<string>();
            foreach (var p in AllPlatforms) if (!_excludedPlatforms.Contains(p)) pool.Add(p);
            if (pool.Count > 0)
            {
                var picked = pool[UnityEngine.Random.Range(0, pool.Count)];
                _spoofPlatformInput = picked;
                SkidMenu.spoofPlatform = picked;
                if (System.Enum.TryParse<Platforms>(picked, out var parsedPlatform))
                {
                    features.Spoofer.spoofedPlatform = parsedPlatform;
                    if (parsedPlatform == Platforms.Xbox)
                    {
                        _spoofXboxIdInput = GenerateRandomPlatformId();
                        features.Spoofer.spoofedXboxId = ulong.Parse(_spoofXboxIdInput);
                    }
                    else if (parsedPlatform == Platforms.Playstation)
                    {
                        _spoofPsnIdInput = GenerateRandomPlatformId();
                        features.Spoofer.spoofedPsnId = ulong.Parse(_spoofPsnIdInput);
                    }
                }
            }
        }

        string generatedName = features.NameSpoofer.SpoofedName;
        if (SkidMenu.frRandName)
        {
            features.NameSpoofer.Mode = (features.NameSpoofer.RandomizerMode)_nameModeIndex;
            features.NameSpoofer.RandomLength = _nameLength;
            generatedName = features.NameSpoofer.Generate();
            _customNameInput = generatedName;
            features.NameSpoofer.ApplyName(generatedName);
        }

        var hatManager = DestroyableSingleton<HatManager>.Instance;
        if (hatManager != null)
        {
            var allHats      = hatManager.allHats;
            var allSkins     = hatManager.allSkins;
            var allVisors    = hatManager.allVisors;
            var allPets      = hatManager.allPets;
            var allNameplates = hatManager.allNamePlates;
            if (SkidMenu.frRandHat      && allHats      != null && allHats.Count      > 0) AmongUs.Data.DataManager.Player.Customization.Hat      = allHats[UnityEngine.Random.Range(0, allHats.Count)].ProdId;
            if (SkidMenu.frRandSkin     && allSkins     != null && allSkins.Count     > 0) AmongUs.Data.DataManager.Player.Customization.Skin     = allSkins[UnityEngine.Random.Range(0, allSkins.Count)].ProdId;
            if (SkidMenu.frRandVisor    && allVisors    != null && allVisors.Count    > 0) AmongUs.Data.DataManager.Player.Customization.Visor    = allVisors[UnityEngine.Random.Range(0, allVisors.Count)].ProdId;
            if (SkidMenu.frRandPet      && allPets      != null && allPets.Count      > 0) AmongUs.Data.DataManager.Player.Customization.Pet      = allPets[UnityEngine.Random.Range(0, allPets.Count)].ProdId;
            if (SkidMenu.frRandNameplate && allNameplates != null && allNameplates.Count > 0) AmongUs.Data.DataManager.Player.Customization.NamePlate = allNameplates[UnityEngine.Random.Range(0, allNameplates.Count)].ProdId;
        }
        if (SkidMenu.frRandColor)
            AmongUs.Data.DataManager.Player.Customization.Color = (byte)UnityEngine.Random.Range(0, Palette.PlayerColors.Length);

        AmongUs.Data.DataManager.Player.Save();

        if (Utils.isPlayer)
        {
            var lp    = PlayerControl.LocalPlayer;
            var hat   = AmongUs.Data.DataManager.Player.Customization.Hat;
            var skin  = AmongUs.Data.DataManager.Player.Customization.Skin;
            var visor = AmongUs.Data.DataManager.Player.Customization.Visor;
            var pet   = AmongUs.Data.DataManager.Player.Customization.Pet;
            var color = AmongUs.Data.DataManager.Player.Customization.Color;
            uint lvl  = (uint)System.Math.Max(0, randomLevel - 1);

            var steps = new List<System.Action>();

            if (SkidMenu.frRandLevel)
                steps.Add(() => lp.RpcSetLevel(lvl));
            if (SkidMenu.frRandName)
                steps.Add(() => OutfitBypass.SetName(generatedName));
            if (SkidMenu.frRandColor)
                steps.Add(() => OutfitBypass.SetColor(AmongUsClient.Instance.AmHost || !Utilities.IsColorTaken(color) ? color : Utilities.GetFreeColor()));
            if (SkidMenu.frRandHat)
                steps.Add(() => lp.RpcSetHat(hat));
            if (SkidMenu.frRandSkin)
                steps.Add(() => lp.RpcSetSkin(skin));
            if (SkidMenu.frRandVisor)
                steps.Add(() => lp.RpcSetVisor(visor));
            if (SkidMenu.frRandPet)
                steps.Add(() => lp.RpcSetPet(pet));
            if (SkidMenu.frRandNameplate)
                steps.Add(() => lp.RpcSetNamePlate(AmongUs.Data.DataManager.Player.Customization.NamePlate));
            steps.Add(() => { if (FullyRandomizeTriggers.ShowNotification) SkidMenu.notifications.Send("Fully Randomized", $"Lv.{randomLevel} | {_spoofPlatformInput} | {generatedName}", 5f); });

            SkidMenu.routines.fullyRandomize.Schedule(steps, delay);
        }
        else
        {
            SkidMenu.notifications.Send("Fully Randomized", $"Lv.{randomLevel} | {_spoofPlatformInput}", 5f);
        }
    }
}




