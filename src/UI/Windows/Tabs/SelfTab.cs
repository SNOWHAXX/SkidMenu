using System.Collections;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SkidMenu.features;

namespace SkidMenu
{
    internal class SelfTab : ITab
    {
        public string name => "Self";

        private uint _level = 0;
        public static int SelectedColor = 0;

        public static bool  RainbowEnabled  = false;
        public static float RainbowDelay    = 0.15f;
        public static bool  RandomizeSpam   = false;
        public static float RandomizeDelay  = 1.0f;

        private static bool _customBodyType = false;
        public static bool CustomBodyType
        {
            get => _customBodyType;
            set
            {
                if (_customBodyType == value) return;
                _customBodyType = value;
                if (!value)
                {
                    var lp = PlayerControl.LocalPlayer;
                    if (lp?.cosmetics != null && lp.MyPhysics != null)
                    {
                        var lb = lp.cosmetics.GetLongBoi();
                        if (lb != null) lb.skipNeckAnim = false;
                        lp.cosmetics.EnsureInitialized(PlayerBodyTypes.Normal);
                        lp.MyPhysics.SetBodyType(PlayerBodyTypes.Normal);
                        lp.MyPhysics.ResetAnimState();
                    }
                    _lastApplied = PlayerBodyTypes.Normal;
                }
            }
        }
        public static PlayerBodyTypes SelectedBodyType  = PlayerBodyTypes.Normal;
        public static float           LongBodyHeight    = 1f;
        public static PlayerBodyTypes _lastApplied     = (PlayerBodyTypes)(-1);

        public static string BgHex   = "222222";
        public static string TextHex = "FFFFFF";

        private bool  _configInitialized = false;
        private bool  _bgFocused         = false;
        private bool  _textFocused       = false;
        private Rect  _bgRect;
        private Rect  _textRect;
        private bool  _bgCursorVisible   = true;
        private bool  _textCursorVisible = true;
        private float _bgLastBlink       = 0f;
        private float _textLastBlink     = 0f;
        private int   _bgCursorPos       = 0;
        private int   _textCursorPos     = 0;
        private const float BlinkRate    = 0.5f;

        private static readonly GUIStyle _richStyle = null;
        private GUIStyle RichStyle => _richStyle ?? new GUIStyle(GUI.skin.label) { richText = true };

        public void Draw()
        {
            if (!_configInitialized)
            {
                BgHex   = SkidMenu.gameBgColorHex;
                TextHex = SkidMenu.gameTextColorHex;
                _configInitialized = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

            // ── Status ───────────────────────────────────────────────────
            if (PlayerControl.LocalPlayer?.Data != null)
            {
                var roleColor = Utils.GetCustomRoleColor(PlayerControl.LocalPlayer.Data);
                var hex = ColorCache.ToHex(roleColor);
                GUILayout.Label($"Role: <color=#{hex}>{PlayerControl.LocalPlayer.Data.RoleType}</color>", RichStyle);
            }

            GUILayout.Space(8);

            // ── General ──────────────────────────────────────────────────
            GUILayout.Label("General", GUIStylePreset.TabSubtitle);
            Self.UpdateStatsFreeplay.Enabled  = GUIStylePreset.CustomToggle(Self.UpdateStatsFreeplay.Enabled, " Update Stats in Freeplay");
            GUILayout.BeginHorizontal();
        Immortality.Enabled = GUIStylePreset.CustomToggle(Immortality.Enabled, " Become Immortal");
        if (Immortality.Enabled)
            Immortality.DisableNotification = GUIStylePreset.CustomToggle(Immortality.DisableNotification, " Disable Kill Notification", GUILayout.Width(200));
        GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
        Invisibility.Enabled = GUIStylePreset.CustomToggle(Invisibility.Enabled, " Become Invisible");
        if (Invisibility.Enabled)
            Invisibility.OnlyInGame = GUIStylePreset.CustomToggle(Invisibility.OnlyInGame, " Only In-Game", GUILayout.Width(200));
        GUILayout.EndHorizontal();
            Self.AlwaysShowTaskAnimations     = GUIStylePreset.CustomToggle(Self.AlwaysShowTaskAnimations, " Always Show Task Animations");
            Self.NoLadderCooldown.Enabled     = GUIStylePreset.CustomToggle(Self.NoLadderCooldown.Enabled, " No Ladder Cooldown");
            Self.VoteAnywhere.InstantVote            = GUIStylePreset.CustomToggle(Self.VoteAnywhere.InstantVote, " Instant Vote");
            if (Self.VoteAnywhere.InstantVote)
            {
                Self.VoteAnywhere.VoteAnyone             = GUIStylePreset.CustomToggle(Self.VoteAnywhere.VoteAnyone, "   - Vote Anyone");
                Self.VoteAnywhere.VoteBeforeVotingStarts = GUIStylePreset.CustomToggle(Self.VoteAnywhere.VoteBeforeVotingStarts, "   - Vote Before Voting Starts");
            }
            Self.UnlimitedMeetings.enabled    = GUIStylePreset.CustomToggle(Self.UnlimitedMeetings.enabled, " Unlimited Meetings");

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Call Meeting"))
            {
                if (AmongUsClient.Instance.AmHost) Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
                else PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            }
            if (GUILayout.Button("Complete All Tasks"))
                PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // ── Avatar ───────────────────────────────────────────────────
            GUILayout.Label("Avatar", GUIStylePreset.TabSubtitle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Randomize"))
            {
                if (AmongUsClient.Instance.AmConnected)
                { Utilities.RandomizePlayer(true); SkidMenu.notifications.Send("Randomizer", "Avatar randomized for this game.", 4); }
                else
                { AccountManager.Instance.RandomizeName(); Utilities.RandomizePlayer(); SkidMenu.notifications.Send("Randomizer", "Name and avatar randomized.", 4); }
            }
            if (GUILayout.Button("Load Info"))
                PlayerInfosUI.Open();
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            bool prevCustomBody = CustomBodyType;
            CustomBodyType = GUIStylePreset.CustomToggle(CustomBodyType, " Custom Body Type");
            if (CustomBodyType)
            {
                string[] bodyNames = { "Normal", "Horse", "Seeker", "Long" };
                int bodyIdx = (int)SelectedBodyType;
                GUILayout.Label($"Body Type: {bodyNames[bodyIdx]}");
                bodyIdx = (int)GUILayout.HorizontalSlider(bodyIdx, 0, 3);
                SelectedBodyType = (PlayerBodyTypes)bodyIdx;

                if (SelectedBodyType == PlayerBodyTypes.Long)
                {
                    GUILayout.Label($"Long Height: {LongBodyHeight:F2}");
                    LongBodyHeight = GUILayout.HorizontalSlider(LongBodyHeight, 0.1f, 5f);
                }
            }
            if ((CustomBodyType != prevCustomBody || SelectedBodyType != _lastApplied) && PlayerControl.LocalPlayer?.cosmetics != null)
            {
                PlayerBodyTypes target = CustomBodyType ? SelectedBodyType : PlayerBodyTypes.Normal;
                PlayerControl.LocalPlayer.cosmetics.EnsureInitialized(target);
                PlayerControl.LocalPlayer.MyPhysics?.SetBodyType(target);
                PlayerControl.LocalPlayer.MyPhysics?.ResetAnimState();
                if (target == PlayerBodyTypes.Long)
                {
                    var lb = PlayerControl.LocalPlayer.cosmetics.GetLongBoi();
                    if (lb != null) lb.targetHeight = LongBodyHeight;
                }
                _lastApplied = target;
            }

            string[] colorNames = { "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown", "Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral" };
            string colorName = SelectedColor < colorNames.Length ? colorNames[SelectedColor] : "Unknown";
            string colorHex = SelectedColor < Palette.PlayerColors.Length ? ColorCache.ToHex(Palette.PlayerColors[SelectedColor]) : "FFFFFF";
            GUILayout.Label($"Color: <color=#{colorHex}>{colorName}</color>", RichStyle);
            SelectedColor = (int)GUILayout.HorizontalSlider(SelectedColor, 0, 17);
            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();
        ColorSniper.TargetColor = (byte)SelectedColor;
        ColorSniper.Enabled = GUIStylePreset.CustomToggle(ColorSniper.Enabled, "Snipe", GUILayout.Width(60));
        ColorSniper.InLobbyOnly = GUIStylePreset.CustomToggle(ColorSniper.InLobbyOnly, "Lobby Only", GUILayout.Width(95));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Set Color"))
        {
            bool taken = false;
            if (!AmongUsClient.Instance.AmHost)
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p != PlayerControl.LocalPlayer && p.Data != null && p.Data.DefaultOutfit.ColorId == SelectedColor)
                        { taken = true; break; }
            if (taken)
                SkidMenu.notifications.Send("Set Color", "That color is already taken. Pick a different one.", 4f);
            else
                OutfitBypass.SetColor(SelectedColor);
        }
            RainbowEnabled = GUIStylePreset.CustomToggle(RainbowEnabled, " Rainbow");
            GUILayout.EndHorizontal();
            if (RainbowEnabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  Speed: {RainbowDelay:F2}s", GUILayout.Width(90));
                RainbowDelay = GUILayout.HorizontalSlider(RainbowDelay, 0.05f, 2f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);

            // ── Level ────────────────────────────────────────────────────
            GUILayout.Label("Level", GUIStylePreset.TabSubtitle);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Set to: {_level + 1}", GUILayout.Width(80));
            _level = (uint)GUILayout.HorizontalSlider(_level, 0, 199);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Send Level Update"))
            {
                PlayerControl.LocalPlayer.RpcSetLevel(_level);
                SkidMenu.notifications.Send("Level Updater", $"Level changed to {_level + 1}", 5);
            }

            GUILayout.Space(8);

            // ── Game Theme ───────────────────────────────────────────────
            GUILayout.Label("Game Theme", GUIStylePreset.TabSubtitle);
            bool newDark = GUIStylePreset.CustomToggle(DarkMode.Enabled, " Dark Game Theme");
            if (newDark != DarkMode.Enabled) DarkMode.Enabled = newDark;

            bool newCustom = GUIStylePreset.CustomToggle(CustomGameTheme.Enabled, " Custom Game Theme");
            if (newCustom != CustomGameTheme.Enabled) CustomGameTheme.Enabled = newCustom;

            if (CustomGameTheme.Enabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("BG #", GUILayout.Width(32));
                DrawHexField(ref BgHex, ref _bgFocused, ref _bgRect, ref _bgCursorVisible, ref _bgLastBlink, ref _bgCursorPos, true);
                GUILayout.Space(10);
                GUILayout.Label("Text #", GUILayout.Width(42));
                DrawHexField(ref TextHex, ref _textFocused, ref _textRect, ref _textCursorVisible, ref _textLastBlink, ref _textCursorPos, false);
                GUILayout.EndHorizontal();
            }

            bool newFont = GUIStylePreset.CustomToggle(ChatFontChanger.Enabled, " Change Chat Font");
            if (newFont != ChatFontChanger.Enabled) ChatFontChanger.Enabled = newFont;
            if (ChatFontChanger.Enabled)
            {
                GUILayout.Label($"  Font: {ChatFontChanger.FontNames[ChatFontChanger.FontType]}");
                int newFontType = (int)GUILayout.HorizontalSlider(ChatFontChanger.FontType, 0, ChatFontChanger.FontNames.Length - 1);
                if (newFontType != ChatFontChanger.FontType) ChatFontChanger.FontType = newFontType;
            }

            GUILayout.Space(8);

            // ── Task Animations ──────────────────────────────────────────
            GUILayout.Label("Task Animations", GUIStylePreset.TabSubtitle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Medbay Scan"))
                TaskAnim(!Utils.isLobby, () => Network.SendSetScanner(true));
            if (GUILayout.Button("Finish Medbay Scan"))
                TaskAnim(!Utils.isLobby, () => Network.SendSetScanner(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Asteroids"))
                TaskAnim(!Utils.isLobby, () => Network.SendPlayAnimation((byte)TaskTypes.ClearAsteroids));
            if (GUILayout.Button("Empty Garbage"))
                TaskAnim(!Utils.isLobby, () => Network.SendPlayAnimation((byte)TaskTypes.EmptyGarbage));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Prime Shields"))
                TaskAnim(!Utils.isLobby, () => Network.SendPlayAnimation((byte)TaskTypes.PrimeShields));

            GUILayout.EndVertical();
        }

        private static void TaskAnim(bool allowed, System.Action action)
        {
            if (allowed) action();
            else SkidMenu.notifications.Send("Anticheat Notice", "Disabled in lobby. Use once the game starts.");
        }

        private void DrawHexField(ref string hex, ref bool focused, ref Rect rect, ref bool cursorVisible, ref float lastBlink, ref int cursorPos, bool isBg)
        {
            GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(70), GUILayout.Height(20));

            if (Event.current.type == EventType.Repaint)
                rect = GUILayoutUtility.GetLastRect();

            if (Event.current.type == EventType.MouseDown)
            {
                bool wasUnfocused = !focused;
                focused = rect.Contains(Event.current.mousePosition);
                if (focused) { if (wasUnfocused) cursorPos = hex.Length; Event.current.Use(); }
            }

            if (focused && Event.current.type == EventType.KeyDown)
            {
                cursorPos = System.Math.Clamp(cursorPos, 0, hex.Length);
                bool ctrl = Event.current.control || Event.current.command;

                if (ctrl && Event.current.keyCode == KeyCode.C) { GUIUtility.systemCopyBuffer = hex; Event.current.Use(); }
                else if (ctrl && Event.current.keyCode == KeyCode.X) { GUIUtility.systemCopyBuffer = hex; hex = ""; cursorPos = 0; Event.current.Use(); }
                else if (ctrl && Event.current.keyCode == KeyCode.V)
                {
                    string clip = GUIUtility.systemCopyBuffer ?? "";
                    var sb = new System.Text.StringBuilder();
                    foreach (char c in clip) if (!char.IsControl(c)) sb.Append(c);
                    clip = sb.ToString();
                    int room = 6 - hex.Length;
                    if (room > 0) { clip = clip.Substring(0, System.Math.Min(clip.Length, room)); hex = hex.Substring(0, cursorPos) + clip + hex.Substring(cursorPos); cursorPos = System.Math.Clamp(cursorPos + clip.Length, 0, hex.Length); }
                    Event.current.Use();
                }
                else if (ctrl && Event.current.keyCode == KeyCode.A) { GUIUtility.systemCopyBuffer = hex; cursorPos = hex.Length; Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.Backspace) { if (cursorPos > 0) { hex = hex.Substring(0, cursorPos - 1) + hex.Substring(cursorPos); cursorPos--; } Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.Delete) { if (cursorPos < hex.Length) hex = hex.Substring(0, cursorPos) + hex.Substring(cursorPos + 1); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.LeftArrow) { if (cursorPos > 0) cursorPos--; Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.RightArrow) { if (cursorPos < hex.Length) cursorPos++; Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.Home) { cursorPos = 0; Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.End) { cursorPos = hex.Length; Event.current.Use(); }
                else if (Event.current.character != '\0' && !char.IsControl(Event.current.character) && hex.Length < 6)
                {
                    hex = hex.Substring(0, cursorPos) + Event.current.character + hex.Substring(cursorPos);
                    cursorPos++;
                    Event.current.Use();
                }

                cursorPos = System.Math.Clamp(cursorPos, 0, hex.Length);
                if (hex.Length == 6 && ColorUtility.TryParseHtmlString("#" + hex, out Color parsedColor))
                {
                    if (isBg) CustomGameTheme.BgColor = parsedColor;
                    else      CustomGameTheme.TextColor = parsedColor;
                }
            }

            GUI.Label(new Rect(rect.x + 5, rect.y + 2, rect.width - 10, rect.height), hex);

            if (focused)
            {
                if (Time.time - lastBlink > BlinkRate) { cursorVisible = !cursorVisible; lastBlink = Time.time; }
                if (cursorVisible)
                {
                    int cp = System.Math.Clamp(cursorPos, 0, hex.Length);
                    Vector2 sz = GUI.skin.label.CalcSize(new GUIContent(hex.Substring(0, cp)));
                    GUI.Label(new Rect(rect.x + sz.x + 7, rect.y + 2, 10, rect.height - 4), "|");
                }
            }
        }

        private IEnumerator CompleteAllTasks()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.IsComplete) continue;
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
                yield return Effects.Wait(0.40f);
            }
            SkidMenu.notifications.Send("Task Finisher", "All tasks finished.", 5);
        }
    }
}






