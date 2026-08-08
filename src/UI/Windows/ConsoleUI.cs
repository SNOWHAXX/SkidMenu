using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using SkidMenu.features;

namespace SkidMenu;

public class ConsoleUI : MonoBehaviour
{
    public static int windowHeight = 460;
    public static int windowWidth = 640;
    private Rect _windowRect;
    public static ConsoleUI Instance { get; private set; }
    public Rect WindowRect { get => _windowRect; set => _windowRect = value; }
    public static Rect LastWindowRect;
    public static Rect PendingRect;
    public static bool PendingRectSet;

    private GUIStyle _logStyle;
    private GUIStyle _clearStyle;
    private GUIStyle _copyStyle;
    private GUIStyle _saveStyle;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private static Dictionary<byte, int> _liveIndex = new();
    private static Dictionary<byte, string> _liveBase = new();
    private static string _cachedText = "";
    private static bool _dirty = false;

    private static readonly string _logFilePath = $"SkidMenu/Logs/Console.{System.DateTime.Now:MM_dd_yyyy.HH_mm_ss}.log";

    private void Start()
    {
        Instance = this;
        _windowRect = PendingRectSet ? PendingRect : new Rect(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
        PendingRectSet = false;
    }

    private void OnGUI()
    {
        if (!CheatToggles.showConsole || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen.Value) || SkidMenu.isPanicked) return;

        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            font     = GUIStylePreset.FontRegular,
            fontSize = 13,
            richText = true,
            wordWrap = false,
            padding  = new RectOffset { left = 4, right = 4, top = 2, bottom = 2 },
            normal   = { textColor = new Color(0.93f, 0.93f, 0.95f) }
        };

        _clearStyle ??= new GUIStyle(GUIStylePreset.NormalButton) { normal = { textColor = new Color(1f, 0.45f, 0.45f) }, hover = { textColor = new Color(1f, 0.6f, 0.6f) } };
        _copyStyle  ??= new GUIStyle(GUIStylePreset.NormalButton) { normal = { textColor = new Color(0.4f, 0.8f,  1f)  }, hover = { textColor = new Color(0.6f, 0.9f,  1f)  } };
        _saveStyle  ??= new GUIStyle(GUIStylePreset.NormalButton) { normal = { textColor = new Color(0.4f, 1f,   0.6f) }, hover = { textColor = new Color(0.6f, 1f,   0.75f)} };

        UIHelpers.ApplyUIColor();

        Matrix4x4 prev = GUI.matrix;
        if (CheatToggles.consoleScaleH != 100f || CheatToggles.consoleScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.consoleScaleH / 100f, CheatToggles.consoleScaleV / 100f), pivot);
        }
        _windowRect = GUI.Window((int)WindowId.ConsoleUI, _windowRect, (GUI.WindowFunction)ConsoleWindow, "Console", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.matrix = prev;
    }

    private void ConsoleWindow(int windowID)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        if (_dirty || _liveIndex.Count > 0)
        {
            _cachedText = BuildText();
            _dirty = false;
        }

        GUILayout.BeginVertical(GUIStylePreset.ModernBox);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        GUILayout.Label(_cachedText, _logStyle);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        float btnW = (windowWidth - 24) / 3f;

        if (GUILayout.Button("Clear Log", _clearStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
        {
            _logEntries.Clear();
            _liveIndex.Clear();
            _liveBase.Clear();
            _cachedText = "";
            _dirty = false;
        }

        if (GUILayout.Button("Copy to Clipboard", _copyStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
            GUIUtility.systemCopyBuffer = _cachedText.Length > 0 ? System.Text.RegularExpressions.Regex.Replace(_cachedText, "<.*?>", "") : "";

        if (GUILayout.Button("Save to File", _saveStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
        {
            try
            {
                System.IO.Directory.CreateDirectory("SkidMenu/Logs");
                var path = $"SkidMenu/Logs/Console_Export.{System.DateTime.Now:MM_dd_yyyy.HH_mm_ss}.log";
                var plain = System.Text.RegularExpressions.Regex.Replace(_cachedText, "<.*?>", "");
                System.IO.File.WriteAllText(path, plain);
            }
            catch { }
        }

        GUILayout.EndHorizontal();

        MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
        GUI.DragWindow();
    }

    public static void Log(string message, string hexColor = null)
    {
        var timestamp = $"<color=#888888>[{System.DateTime.Now:HH:mm:ss}]</color> ";
        var entry = hexColor != null
            ? $"{timestamp}<color=#{hexColor}>{message}</color>"
            : $"{timestamp}{message}";

        if (_logEntries.Count >= CheatToggles.maxLogEntries)
            EvictFront();

        _logEntries.Add(entry);
        _dirty = true;

        try
        {
            var plain = System.Text.RegularExpressions.Regex.Replace(entry, "<.*?>", "");
            System.IO.File.AppendAllText(_logFilePath, plain + "\n");
            AdvancedLogger.Mirror(plain);
        }
        catch { }

        _scrollPosition.y = float.MaxValue;
    }

    public static void LogLiveKill(string message, byte key, string hexColor)
    {
        var timestamp = $"<color=#888888>[{System.DateTime.Now:HH:mm:ss}]</color> ";
        var entry = hexColor != null
            ? $"{timestamp}<color=#{hexColor}>{message}</color>"
            : $"{timestamp}{message}";

        if (_liveIndex.TryGetValue(key, out int oldIdx) && oldIdx >= 0 && oldIdx < _logEntries.Count)
            _logEntries[oldIdx] = _liveBase.TryGetValue(key, out var oldBase) ? oldBase : entry;

        if (_logEntries.Count >= CheatToggles.maxLogEntries)
            EvictFront();

        _logEntries.Add(entry);
        _liveIndex[key] = _logEntries.Count - 1;
        _liveBase[key] = entry;
        _dirty = true;

        try
        {
            var plain = System.Text.RegularExpressions.Regex.Replace(entry, "<.*?>", "");
            System.IO.File.AppendAllText(_logFilePath, plain + "\n");
            AdvancedLogger.Mirror(plain);
        }
        catch { }

        _scrollPosition.y = float.MaxValue;
    }

    private static void EvictFront()
    {
        _logEntries.RemoveAt(0);
        var next = new Dictionary<byte, int>();
        foreach (var kv in _liveIndex)
        {
            int ni = kv.Value - 1;
            if (ni >= 0) next[kv.Key] = ni;
            else _liveBase.Remove(kv.Key);
        }
        _liveIndex = next;
    }

    private static string BuildText()
    {
        var done = new List<byte>();
        foreach (var kv in _liveIndex)
        {
            if (ViperBodies.Remaining(kv.Key) <= 0f) done.Add(kv.Key);
        }
        for (int i = 0; i < done.Count; i++)
        {
            byte key = done[i];
            if (_liveBase.TryGetValue(key, out string baseEntry)
                && _liveIndex.TryGetValue(key, out int idx)
                && idx >= 0 && idx < _logEntries.Count)
                _logEntries[idx] = baseEntry + ViperBodies.AcidTag(key);
            _liveIndex.Remove(key);
            _liveBase.Remove(key);
        }

        var sb = new StringBuilder();
        for (int i = 0; i < _logEntries.Count; i++)
        {
            if (i > 0) sb.Append('\n');
            string line = _logEntries[i];
            foreach (var kv in _liveIndex)
            {
                if (kv.Value != i) continue;
                if (_liveBase.TryGetValue(kv.Key, out string baseEntry))
                    line = baseEntry + ViperBodies.AcidTag(kv.Key);
                break;
            }
            sb.Append(line);
        }
        return sb.ToString();
    }
}




