using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public class ChatUI : MonoBehaviour
{
    public static int windowHeight = 460;
    public static int windowWidth = 640;
    private const int MaxInput = 120;

    private Rect _windowRect;
    public static ChatUI Instance { get; private set; }
    public Rect WindowRect { get => _windowRect; set => _windowRect = value; }
    public static Rect LastWindowRect;
    public static Rect PendingRect;
    public static bool PendingRectSet;

    private GUIStyle _logStyle;
    private GUIStyle _clearStyle;
    private GUIStyle _copyStyle;
    private GUIStyle _saveStyle;
    private GUIStyle _sendStyle;
    private GUIStyle _inputStyle;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static readonly Queue<string> _logEntries = new();
    private static string _cachedText = "";
    private static bool _dirty = false;

    private string _inputText = "";
    private bool _inputFocused = false;
    private Rect _inputRect;
    private static string _lastKey = "";
    private static float _lastAt = -10f;

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
        if (!CheatToggles.showChatUI || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen) || SkidMenu.isPanicked) return;

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
        _sendStyle  ??= new GUIStyle(GUIStylePreset.NormalButton) { normal = { textColor = new Color(0.4f, 1f,   0.6f) }, hover = { textColor = new Color(0.6f, 1f,   0.75f)} };
        _inputStyle ??= new GUIStyle(GUI.skin.label)
        {
            font     = GUIStylePreset.FontRegular,
            fontSize = 13,
            richText = false,
            alignment = TextAnchor.MiddleLeft,
            clipping  = TextClipping.Clip,
            normal    = { textColor = new Color(0.9f, 0.9f, 0.9f) }
        };

        UIHelpers.ApplyUIColor();

        Matrix4x4 prev = GUI.matrix;
        if (CheatToggles.chatScaleH != 100f || CheatToggles.chatScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.chatScaleH / 100f, CheatToggles.chatScaleV / 100f), pivot);
        }
        _windowRect = GUI.Window((int)WindowId.ChatUI, _windowRect, (GUI.WindowFunction)ChatWindow, "Chat", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.matrix = prev;
    }

    private void ChatWindow(int windowID)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        if (_dirty)
        {
            _cachedText = string.Join("\n", _logEntries);
            _dirty = false;
        }

        GUILayout.BeginVertical(GUIStylePreset.ModernBox);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        GUILayout.Label(_cachedText, _logStyle);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        float inputW = windowWidth - 24 - 60 - 8;

        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(inputW), GUILayout.Height(26));
        if (Event.current.type == EventType.Repaint) _inputRect = GUILayoutUtility.GetLastRect();
        HandleInput(Event.current);

        bool caret = _inputFocused && (int)(Time.time * 2f) % 2 == 0;
        string shown = _inputText.Length == 0 && !_inputFocused ? "Message..." : _inputText + (caret ? "|" : "");
        GUI.Label(new Rect(_inputRect.x + 6, _inputRect.y, _inputRect.width - 12, _inputRect.height), shown, _inputStyle);

        if (GUILayout.Button("Send", _sendStyle, GUILayout.Width(60), GUILayout.Height(26))) Send();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        float btnW = (windowWidth - 24) / 3f;

        if (GUILayout.Button("Clear Chat", _clearStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
        {
            _logEntries.Clear();
            _cachedText = "";
            _dirty = false;
        }

        if (GUILayout.Button("Copy Chat", _copyStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
            GUIUtility.systemCopyBuffer = _cachedText.Length > 0 ? System.Text.RegularExpressions.Regex.Replace(_cachedText, "<.*?>", "") : "";

        if (GUILayout.Button("Save to File", _saveStyle, GUILayout.Width(btnW), GUILayout.Height(30)))
        {
            try
            {
                System.IO.Directory.CreateDirectory("SkidMenu/Logs");
                var path = $"SkidMenu/Logs/Chat_Export.{System.DateTime.Now:MM_dd_yyyy.HH_mm_ss}.log";
                var plain = System.Text.RegularExpressions.Regex.Replace(_cachedText, "<.*?>", "");
                System.IO.File.WriteAllText(path, plain);
            }
            catch { }
        }

        GUILayout.EndHorizontal();

        MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
        GUI.DragWindow();
    }

    private void HandleInput(Event e)
    {
        if (e == null) return;
        if (e.type == EventType.MouseDown)
        {
            if (_inputRect.Contains(e.mousePosition)) { _inputFocused = true; e.Use(); }
            else if (!_windowRect.Contains(e.mousePosition)) _inputFocused = false;
            return;
        }
        if (!_inputFocused || e.type != EventType.KeyDown) return;

        if (e.keyCode == KeyCode.Backspace)
        {
            if (_inputText.Length > 0) _inputText = _inputText.Substring(0, _inputText.Length - 1);
            e.Use();
        }
        else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { Send(); e.Use(); }
        else if (e.keyCode == KeyCode.Escape) { _inputFocused = false; e.Use(); }
        else if (e.character != '\0' && !char.IsControl(e.character) && _inputText.Length < MaxInput) { _inputText += e.character; e.Use(); }
    }

    private void Send()
    {
        string msg = (_inputText ?? "").Trim();
        if (msg.Length == 0) return;
        var pc = PlayerControl.LocalPlayer;
        if (pc == null) return;
        try { pc.RpcSendChat(msg); } catch { }
        _inputText = "";
        _scrollPosition.y = float.MaxValue;
    }

    public static void Feed(PlayerControl source, string chatText)
    {
        if (string.IsNullOrWhiteSpace(chatText)) return;
        try
        {
            string name = "?";
            bool local = false, dead = false;
            if (source != null && source.Data != null)
            {
                name = source.Data.PlayerName;
                local = source == PlayerControl.LocalPlayer;
                dead = source.Data.IsDead;
            }
            name = Clean(name, 24);
            string text = Clean(chatText, 200);
            if (text.Length == 0) return;

            string key = name + "|" + text;
            float now = Time.unscaledTime;
            if (key == _lastKey && now - _lastAt < 0.75f) return;
            _lastKey = key;
            _lastAt = now;

            string nameColor = local ? "4FC3F7" : (dead ? "D7BFFF" : "DDDDDD");
            var entry = $"<color=#888888>[{System.DateTime.Now:HH:mm:ss}]</color> <color=#{nameColor}>{name}</color>: {text}";

            if (_logEntries.Count >= CheatToggles.chatMaxEntries)
                _logEntries.Dequeue();

            _logEntries.Enqueue(entry);
            _dirty = true;
            _scrollPosition.y = float.MaxValue;
        }
        catch { }
    }

    private static string Clean(string value, int max)
    {
        if (string.IsNullOrEmpty(value)) return "";
        string s = StripTags(value).Replace('\n', ' ').Replace('\r', ' ').Trim();
        return s.Length > max ? s.Substring(0, max) : s;
    }

    private static string StripTags(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        bool tag = false;
        foreach (char c in s)
        {
            if (c == '<') tag = true;
            else if (c == '>') tag = false;
            else if (!tag) sb.Append(c);
        }
        return sb.ToString();
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    internal static class ChatUI_FeedPatch
    {
        public static void Postfix(PlayerControl sourcePlayer, string chatText) => Feed(sourcePlayer, chatText);
    }
}
