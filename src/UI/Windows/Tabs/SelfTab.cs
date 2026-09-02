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

        private GUIStyle RichStyle => new GUIStyle(GUI.skin.label) { richText = true };

        public void Draw()
        {
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
            Self.NoZiplineCooldown.Enabled    = GUIStylePreset.CustomToggle(Self.NoZiplineCooldown.Enabled, " No Zipline Cooldown");
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
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Set to: {_level + 1}", GUILayout.Width(80));
            _level = (uint)GUILayout.HorizontalSlider(_level, 0, 199);
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            if (GUILayout.Button("Send Level Update"))
            {
                PlayerControl.LocalPlayer.RpcSetLevel(_level);
                SkidMenu.notifications.Send("Level Updater", $"Level changed to {_level + 1}", 5);
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

            GUILayout.Space(8);

            GUILayout.EndVertical();
        }

        private static void TaskAnim(bool allowed, System.Action action)
        {
            if (allowed) action();
            else SkidMenu.notifications.Send("Anticheat Notice", "Disabled in lobby. Use once the game starts.");
        }

        private IEnumerator CompleteAllTasks()
        {
            uint lastId = 0;
            bool hasLast = false;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.IsComplete) continue;
                if (hasLast && task.Id == lastId) continue;
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
                lastId = task.Id;
                hasLast = true;
                yield return Effects.Wait(Utils.TaskCompleteSpacing);
            }
            SkidMenu.notifications.Send("Task Finisher", "All tasks finished.", 5);
        }
    }
}






