using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class ChatTab : ITab
{
    public string name => "Chat";

    private bool _msgFocused = false;
    private Rect _msgRect;
    private bool _msgCursorVisible = true;
    private float _msgLastBlink = 0f;
    private int _msgCursorPos = 0;
    private const float BlinkRate = 0.5f;
    private const int MaxChars = 120;

    private string _chatScaleHInput = "100", _chatScaleVInput = "100";
    private bool _chatScaleFocused = false;
    private Rect _chatScaleRect;
    private int _chatScaleCursor = 0;
    private string _chatScaleActiveKey = "";
    private bool _chatScaleCursorVisible = true;
    private float _chatScaleLastBlink = 0f;

    private void HandleScaleField(ref string content, string fieldKey)
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(50), GUILayout.Height(20));

        if (Event.current.type == EventType.Repaint)
            _chatScaleRect = GUILayoutUtility.GetLastRect();

        if (Event.current.type == EventType.MouseDown)
        {
            bool hit = _chatScaleRect.Contains(Event.current.mousePosition);
            if (hit != _chatScaleFocused)
            {
                _chatScaleFocused = hit;
                _chatScaleActiveKey = hit ? fieldKey : "";
                _chatScaleCursor = content.Length;
            }
            if (hit) Event.current.Use();
        }

        if (_chatScaleFocused && _chatScaleActiveKey == fieldKey && Event.current.type == EventType.KeyDown)
        {
            _chatScaleCursor = System.Math.Clamp(_chatScaleCursor, 0, content.Length);
            if (Event.current.keyCode == KeyCode.Backspace && _chatScaleCursor > 0) { content = content.Substring(0, _chatScaleCursor - 1) + content.Substring(_chatScaleCursor); _chatScaleCursor--; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.LeftArrow && _chatScaleCursor > 0) { _chatScaleCursor--; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.RightArrow && _chatScaleCursor < content.Length) { _chatScaleCursor++; Event.current.Use(); }
            else if (char.IsDigit(Event.current.character) && content.Length < 3) { content = content.Substring(0, _chatScaleCursor) + Event.current.character + content.Substring(_chatScaleCursor); _chatScaleCursor++; Event.current.Use(); }
        }

        GUI.Label(new Rect(_chatScaleRect.x + 5, _chatScaleRect.y + 2, _chatScaleRect.width - 10, _chatScaleRect.height), content);

        if (_chatScaleFocused && _chatScaleActiveKey == fieldKey)
        {
            if (Time.time - _chatScaleLastBlink > BlinkRate) { _chatScaleCursorVisible = !_chatScaleCursorVisible; _chatScaleLastBlink = Time.time; }
            if (_chatScaleCursorVisible)
            {
                int cp = System.Math.Clamp(_chatScaleCursor, 0, content.Length);
                Vector2 ts = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp)));
                GUI.Label(new Rect(_chatScaleRect.x + ts.x + 7, _chatScaleRect.y + 2, 10, _chatScaleRect.height - 4), "|");
            }
        }
    }

    public static bool BypassCharLimit = false;
    public static bool CopyMessage     = false;

    public void Draw()
    {
        if (!_triggersInit)
        {
            _triggersInit = true;
            _joinE = ChatSender.OnJoinEnabled; _joinM = ChatSender.OnJoinMessage;
            _deathE = ChatSender.OnDeathEnabled; _deathM = ChatSender.OnDeathMessage;
            _meetE = ChatSender.OnMeetingEnabled; _meetM = ChatSender.OnMeetingMessage;
            _killE = ChatSender.OnKillEnabled; _killM = ChatSender.OnKillMessage;
            _ejectE = ChatSender.OnEjectionEnabled; _ejectM = ChatSender.OnEjectionMessage;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.85f));

        GUILayout.Label("General", GUIStylePreset.TabSubtitle);
        CheatToggles.enableChat      = GUIStylePreset.CustomToggle(CheatToggles.enableChat, " Enable Chat");
        CheatToggles.bypassUrlBlock  = GUIStylePreset.CustomToggle(CheatToggles.bypassUrlBlock, " Bypass URL Block");
        CheatToggles.lowerRateLimits = GUIStylePreset.CustomToggle(CheatToggles.lowerRateLimits, " Lower Rate Limits");

        GUILayout.Space(8);

        GUILayout.Label("Chat Window", GUIStylePreset.TabSubtitle);
        CheatToggles.showChatUI = GUIStylePreset.CustomToggle(CheatToggles.showChatUI, " Show Chat Window");
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleScaleField(ref _chatScaleHInput, "chatScaleH");
        GUILayout.Label("%  Vertical:", GUILayout.Width(70));
        HandleScaleField(ref _chatScaleVInput, "chatScaleV");
        GUILayout.Label("%", GUILayout.Width(20));
        if (GUILayout.Button("Apply", GUILayout.Width(60))) { if (float.TryParse(_chatScaleHInput, out var h)) CheatToggles.chatScaleH = System.Math.Clamp(h, 50f, 300f); if (float.TryParse(_chatScaleVInput, out var v)) CheatToggles.chatScaleV = System.Math.Clamp(v, 50f, 300f); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Max Chat Entries: {CheatToggles.chatMaxEntries}", GUILayout.Width(220));
        int newMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(CheatToggles.chatMaxEntries, 50, 2000, GUILayout.Width(150)));
        if (newMax != CheatToggles.chatMaxEntries) CheatToggles.chatMaxEntries = newMax;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.Label("Textbox", GUIStylePreset.TabSubtitle);
        CheatToggles.unlockCharacters       = GUIStylePreset.CustomToggle(CheatToggles.unlockCharacters, " Unlock Extra Characters");
        CheatToggles.unlockClipboard        = GUIStylePreset.CustomToggle(CheatToggles.unlockClipboard, " Unlock Clipboard");
        ChatEnhancements.EnableExtendedChat = GUIStylePreset.CustomToggle(ChatEnhancements.EnableExtendedChat, " Extended Chat (120 chars in game)");
        ChatEnhancements.EnableColorCommand = GUIStylePreset.CustomToggle(ChatEnhancements.EnableColorCommand, " Enable /color #RRGGBB");

        GUILayout.Space(8);

        GUILayout.Label("Chat History", GUIStylePreset.TabSubtitle);
        ChatEnhancements.EnableChatHistory = GUIStylePreset.CustomToggle(ChatEnhancements.EnableChatHistory, " Chat History (Up/Down in chat)");
        CheatToggles.copyMessage = GUIStylePreset.CustomToggle(CheatToggles.copyMessage, " Copy Message (double-click bubble)");
        if (ChatEnhancements.History.Sent.Count > 0 && GUILayout.Button("Clear History", GUILayout.Width(120)))
            ChatEnhancements.History.Sent.Clear();

        GUILayout.Space(4);

        bool inf = GUIStylePreset.CustomToggle(SkidMenu.chatHistoryInfinite.Value, " Infinite bubble pool");
        if (inf != SkidMenu.chatHistoryInfinite.Value)
        {
            SkidMenu.chatHistoryInfinite.Value = inf;
            if (HudManager.Instance?.Chat != null)
                ChatHistoryPatch.ApplyHistorySize(HudManager.Instance.Chat);
        }
        if (!SkidMenu.chatHistoryInfinite.Value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Pool size: {SkidMenu.chatHistorySize.Value}", GUILayout.Width(100));
            int size = Mathf.RoundToInt(GUILayout.HorizontalSlider(SkidMenu.chatHistorySize.Value, 5, 500));
            if (size != SkidMenu.chatHistorySize.Value)
            {
                SkidMenu.chatHistorySize.Value = size;
                if (HudManager.Instance?.Chat != null)
                    ChatHistoryPatch.ApplyHistorySize(HudManager.Instance.Chat);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(8);

        GUILayout.Label("Chat Sender", GUIStylePreset.TabSubtitle);

        bool newEnabled = GUIStylePreset.CustomToggle(ChatSender.Enabled, " Spam Enabled");
        if (newEnabled != ChatSender.Enabled) ChatSender.Enabled = newEnabled;

        GUILayout.Space(4);

        ChatSender.Message ??= "";
        int charCount = ChatSender.Message.Length;
        int limit = BypassCharLimit ? 512 : MaxChars;
        GUILayout.Label(BypassCharLimit ? $"Message: ({charCount} chars)" : $"Message: ({charCount}/{MaxChars})");

        DrawMessageField(limit);

        GUILayout.Space(2);
        BypassCharLimit = GUIStylePreset.CustomToggle(BypassCharLimit, " Bypass 120 char limit");
        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Send Once", GUILayout.Width(110))) SendMessage();
        GUILayout.Label("  Shift+Enter = new line  |  Enter = send (in field)", GUILayout.Width(300));
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Spam delay: {ChatSender.Delay:0.0}s", GUILayout.Width(110));
        ChatSender.Delay = GUILayout.HorizontalSlider(ChatSender.Delay, 0.5f, 10f);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Trigger Messages", GUIStylePreset.TabSubtitle);

        DrawBigTriggerBox(0, "On Join",     ref _joinE,    ref _joinM);
        DrawBigTriggerBox(1, "On Death",    ref _deathE,   ref _deathM);
        DrawBigTriggerBox(2, "On Meeting",  ref _meetE,    ref _meetM);
        DrawBigTriggerBox(3, "On Kill",     ref _killE,    ref _killM);
        DrawBigTriggerBox(4, "On Ejection", ref _ejectE,   ref _ejectM);

        GUILayout.EndVertical();
    }

    private bool _joinE, _deathE, _meetE, _killE, _ejectE, _triggersInit;
    private string _joinM, _deathM, _meetM, _killM, _ejectM;
    private readonly bool[] _trigFocused = new bool[5];
    private readonly Rect[] _trigRect = new Rect[5];
    private readonly int[] _trigCursorPos = new int[5];
    private readonly bool[] _trigCursorVisible = { true, true, true, true, true };
    private readonly float[] _trigLastBlink = new float[5];

    private void DrawBigTriggerBox(int id, string label, ref bool enabled, ref string message)
    {
        message ??= "";
        int charLimit = BypassCharLimit ? 512 : MaxChars;

        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        bool ne = GUIStylePreset.CustomToggle(enabled, $" {label}", GUILayout.Width(120));
        if (ne != enabled) { enabled = ne; SyncTriggerState(label, enabled, message); }
        GUILayout.Label($"({message.Length}/{charLimit})");
        GUILayout.EndHorizontal();

        float boxWidth = MenuUI.windowWidth * 0.61f;
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(boxWidth), GUILayout.Height(80));

        if (Event.current.type == EventType.Repaint)
            _trigRect[id] = GUILayoutUtility.GetLastRect();

        if (Event.current.type == EventType.MouseDown)
        {
            bool wasUnfocused = !_trigFocused[id];
            _trigFocused[id] = _trigRect[id].Contains(Event.current.mousePosition);
            if (_trigFocused[id]) { if (wasUnfocused) _trigCursorPos[id] = message.Length; Event.current.Use(); }
        }

        if (_trigFocused[id] && Event.current.type == EventType.KeyDown)
        {
            int cp = System.Math.Clamp(_trigCursorPos[id], 0, message.Length);
            bool ctrl = Event.current.control || Event.current.command;
            bool shift = Event.current.shift;

            if (Event.current.keyCode == KeyCode.Return && shift)
            {
                if (message.Length < charLimit)
                {
                    message = message.Substring(0, cp) + "\n" + message.Substring(cp);
                    cp++;
                }
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Return && !shift)
            {
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.C) { GUIUtility.systemCopyBuffer = message; Event.current.Use(); }
            else if (ctrl && Event.current.keyCode == KeyCode.X) { GUIUtility.systemCopyBuffer = message; message = ""; cp = 0; Event.current.Use(); }
            else if (ctrl && Event.current.keyCode == KeyCode.V)
            {
                string clip = GUIUtility.systemCopyBuffer ?? "";
                var sb = new System.Text.StringBuilder();
                foreach (char c in clip) if (c == '\n' || !char.IsControl(c)) sb.Append(c);
                clip = sb.ToString();
                int room = charLimit - message.Length;
                if (room > 0)
                {
                    clip = clip.Substring(0, System.Math.Min(clip.Length, room));
                    message = message.Substring(0, cp) + clip + message.Substring(cp);
                    cp = System.Math.Clamp(cp + clip.Length, 0, message.Length);
                }
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.A) { GUIUtility.systemCopyBuffer = message; cp = message.Length; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Backspace) { if (cp > 0) { message = message.Substring(0, cp - 1) + message.Substring(cp); cp--; } Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Delete) { if (cp < message.Length) message = message.Substring(0, cp) + message.Substring(cp + 1); Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.LeftArrow) { if (cp > 0) cp--; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.RightArrow) { if (cp < message.Length) cp++; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Home) { cp = 0; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.End) { cp = message.Length; Event.current.Use(); }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character) && message.Length < charLimit)
            {
                message = message.Substring(0, cp) + Event.current.character + message.Substring(cp);
                cp++;
                Event.current.Use();
            }

            _trigCursorPos[id] = System.Math.Clamp(cp, 0, message.Length);
            SyncTriggerState(label, enabled, message);
        }

        var wrapStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
        GUI.Label(new Rect(_trigRect[id].x + 6, _trigRect[id].y + 6, _trigRect[id].width - 12, _trigRect[id].height - 8), message, wrapStyle);

        if (_trigFocused[id])
        {
            if (Time.time - _trigLastBlink[id] > BlinkRate) { _trigCursorVisible[id] = !_trigCursorVisible[id]; _trigLastBlink[id] = Time.time; }
            if (_trigCursorVisible[id])
            {
                int cp = System.Math.Clamp(_trigCursorPos[id], 0, message.Length);
                string beforeCursor = message.Substring(0, cp);
                int lastNL = beforeCursor.LastIndexOf('\n');
                string currentLine = lastNL >= 0 ? beforeCursor.Substring(lastNL + 1) : beforeCursor;
                int lineNum = 0; foreach (char c in beforeCursor) if (c == '\n') lineNum++;
                float lineH = GUI.skin.label.CalcSize(new GUIContent("A")).y;
                float lineW = GUI.skin.label.CalcSize(new GUIContent(currentLine)).x;
                GUI.Label(new Rect(_trigRect[id].x + lineW + 8, _trigRect[id].y + 6 + lineNum * lineH, 10, lineH), "|");
            }
        }
    }

    private void SyncTriggerState(string label, bool enabled, string message)
    {
        switch (label)
        {
            case "On Join": ChatSender.OnJoinEnabled = enabled; ChatSender.OnJoinMessage = message; break;
            case "On Death": ChatSender.OnDeathEnabled = enabled; ChatSender.OnDeathMessage = message; break;
            case "On Meeting": ChatSender.OnMeetingEnabled = enabled; ChatSender.OnMeetingMessage = message; break;
            case "On Kill": ChatSender.OnKillEnabled = enabled; ChatSender.OnKillMessage = message; break;
            case "On Ejection": ChatSender.OnEjectionEnabled = enabled; ChatSender.OnEjectionMessage = message; break;
        }
    }

    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(ChatSender.Message)) return;
        try
        {
            var chat = DestroyableSingleton<HudManager>.Instance?.Chat;
            if (chat?.freeChatField?.textArea == null) return;
            chat.freeChatField.textArea.SetText(ChatSender.Message, string.Empty);
            chat.SendChat();
        }
        catch { }
    }

    private void DrawMessageField(int charLimit)
    {
        float boxWidth = MenuUI.windowWidth * 0.61f;
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(boxWidth), GUILayout.Height(80));

        if (Event.current.type == EventType.Repaint)
            _msgRect = GUILayoutUtility.GetLastRect();

        if (Event.current.type == EventType.MouseDown)
        {
            bool wasUnfocused = !_msgFocused;
            _msgFocused = _msgRect.Contains(Event.current.mousePosition);
            if (_msgFocused) { if (wasUnfocused) _msgCursorPos = (ChatSender.Message ?? "").Length; Event.current.Use(); }
        }

        if (_msgFocused && Event.current.type == EventType.KeyDown)
        {
            string msg = ChatSender.Message ?? "";
            _msgCursorPos = System.Math.Clamp(_msgCursorPos, 0, msg.Length);
            bool ctrl  = Event.current.control || Event.current.command;
            bool shift = Event.current.shift;

            if (Event.current.keyCode == KeyCode.Return && shift)
            {
                if (msg.Length < charLimit)
                {
                    ChatSender.Message = msg.Substring(0, _msgCursorPos) + "\n" + msg.Substring(_msgCursorPos);
                    _msgCursorPos++;
                }
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Return && !shift) { SendMessage(); Event.current.Use(); }
            else if (ctrl && Event.current.keyCode == KeyCode.C) { GUIUtility.systemCopyBuffer = msg; Event.current.Use(); }
            else if (ctrl && Event.current.keyCode == KeyCode.X) { GUIUtility.systemCopyBuffer = msg; ChatSender.Message = ""; _msgCursorPos = 0; Event.current.Use(); }
            else if (ctrl && Event.current.keyCode == KeyCode.V)
            {
                string clip = GUIUtility.systemCopyBuffer ?? "";
                var sb = new System.Text.StringBuilder();
                foreach (char c in clip) if (c == '\n' || !char.IsControl(c)) sb.Append(c);
                clip = sb.ToString();
                int room = charLimit - msg.Length;
                if (room > 0) { clip = clip.Substring(0, System.Math.Min(clip.Length, room)); ChatSender.Message = msg.Substring(0, _msgCursorPos) + clip + msg.Substring(_msgCursorPos); _msgCursorPos = System.Math.Clamp(_msgCursorPos + clip.Length, 0, ChatSender.Message.Length); }
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.A) { GUIUtility.systemCopyBuffer = msg; _msgCursorPos = msg.Length; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Backspace) { if (_msgCursorPos > 0) { ChatSender.Message = msg.Substring(0, _msgCursorPos - 1) + msg.Substring(_msgCursorPos); _msgCursorPos--; } Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Delete) { if (_msgCursorPos < msg.Length) ChatSender.Message = msg.Substring(0, _msgCursorPos) + msg.Substring(_msgCursorPos + 1); Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.LeftArrow)  { if (_msgCursorPos > 0) _msgCursorPos--; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.RightArrow) { if (_msgCursorPos < msg.Length) _msgCursorPos++; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Home) { _msgCursorPos = 0; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.End)  { _msgCursorPos = msg.Length; Event.current.Use(); }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character) && msg.Length < charLimit)
            {
                ChatSender.Message = msg.Substring(0, _msgCursorPos) + Event.current.character + msg.Substring(_msgCursorPos);
                _msgCursorPos++;
                Event.current.Use();
            }

            ChatSender.Message ??= "";
            _msgCursorPos = System.Math.Clamp(_msgCursorPos, 0, ChatSender.Message.Length);
        }

        string display = ChatSender.Message ?? "";
        var wrapStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
        GUI.Label(new Rect(_msgRect.x + 6, _msgRect.y + 6, _msgRect.width - 12, _msgRect.height - 8), display, wrapStyle);

        if (_msgFocused)
        {
            if (Time.time - _msgLastBlink > BlinkRate) { _msgCursorVisible = !_msgCursorVisible; _msgLastBlink = Time.time; }
            if (_msgCursorVisible)
            {
                int cp = System.Math.Clamp(_msgCursorPos, 0, display.Length);
                string beforeCursor = display.Substring(0, cp);
                int lastNL = beforeCursor.LastIndexOf('\n');
                string currentLine = lastNL >= 0 ? beforeCursor.Substring(lastNL + 1) : beforeCursor;
                int lineNum = 0; foreach (char c in beforeCursor) if (c == '\n') lineNum++;
                float lineH = GUI.skin.label.CalcSize(new GUIContent("A")).y;
                float lineW = GUI.skin.label.CalcSize(new GUIContent(currentLine)).x;
                GUI.Label(new Rect(_msgRect.x + lineW + 8, _msgRect.y + 6 + lineNum * lineH, 10, lineH), "|");
            }
        }
    }
}
