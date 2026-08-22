using SkidMenu.anticheat;
using UnityEngine;

namespace SkidMenu
{
    internal class AnticheatTab : ITab
    {
        public string name => "Anticheat";

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _blScroll       = Vector2.zero;

        private static readonly string[] HostPunishLabels = { "None", "Kick", "Error Kick", "Ban" };
        private static readonly Color[] HostPunishColors =
        {
            new Color(0.5f, 0.5f, 0.5f),
            new Color(1f, 0.65f, 0f),
            new Color(1f, 0.4f, 0f),
            new Color(0.9f, 0.1f, 0.1f),
        };

        private static readonly string[] NonHostPunishLabels = { "None", "Votekick", "Ban Exploit", "Vent Kick" };
        private static readonly Color[] NonHostPunishColors =
        {
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.3f, 0.7f, 1f),
            new Color(0.8f, 0.2f, 0.9f),
            new Color(0.15f, 0.5f, 0.75f),
        };

        public void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawGeneral();
            GUILayout.Space(12);
            DrawHostPunishment();
            GUILayout.Space(12);
            DrawNonHostPunishment();
            GUILayout.Space(12);
            DrawRpcDetections();
            GUILayout.Space(12);
            DrawLevelCheck();
            GUILayout.Space(12);
            DrawModDetection();
            GUILayout.Space(12);
            DrawBlacklist();

            GUILayout.EndScrollView();
        }

        private void DrawGeneral()
        {
            GUILayout.Label("General", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            Anticheat.Enabled = GUIStylePreset.CustomToggle(Anticheat.Enabled, " Enable SkidMenu Anticheat");
            GUILayout.Space(2);
            Anticheat.sendNotification = GUIStylePreset.CustomToggle(Anticheat.sendNotification, " Send notification on detection");
            GUILayout.Space(2);
            Anticheat.discardRpc = GUIStylePreset.CustomToggle(Anticheat.discardRpc, " Block flagged RPCs");
            GUILayout.Space(2);
            Anticheat.CheckSpoofedPlatforms = GUIStylePreset.CustomToggle(Anticheat.CheckSpoofedPlatforms, " Flag spoofed platform data");

            if (!Anticheat.Enabled)
            {
                GUILayout.Space(3);
                GUI.color = new Color(1f, 0.6f, 0.2f);
                GUILayout.Label("  ⚠ Anticheat is disabled — no checks are active.");
                GUI.color = Color.white;
            }
        }

        private void DrawHostPunishment()
        {
            GUILayout.Label("Host Punishment", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
            if (!isHost)
            {
                GUI.color = new Color(1f, 0.6f, 0.2f);
                GUILayout.Label("  ⚠ You are not host — these will have no effect right now.");
                GUI.color = Color.white;
                GUILayout.Space(4);
            }

            GUILayout.BeginHorizontal();
            for (int i = 0; i < HostPunishLabels.Length; i++)
            {
                bool selected = (int)Anticheat.punishment == i;
                var prev = GUI.backgroundColor;
                var punBg = GUIStylePreset.WhiteButtonBg;
                var punStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
                punStyle.normal.background = punBg; punStyle.hover.background = punBg; punStyle.active.background = punBg;
                punStyle.normal.textColor = punStyle.hover.textColor = punStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
                GUI.backgroundColor = selected ? HostPunishColors[i] : new Color(0.55f, 0.55f, 0.55f);
                if (GUILayout.Button(HostPunishLabels[i], punStyle, GUILayout.Width(90), GUILayout.Height(28)))
                    Anticheat.punishment = (Anticheat.Punishments)i;
                GUI.backgroundColor = prev;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            int sel = (int)Anticheat.punishment;
            GUI.color = HostPunishColors[sel];
            GUILayout.Label($"  {HostPunishDescription(sel)}");
            GUI.color = Color.white;
        }

        private void DrawNonHostPunishment()
        {
            GUILayout.Label("Non-Host Punishment", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
            if (isHost)
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("  You are host — use Host Punishment above instead.");
                GUI.color = Color.white;
                GUILayout.Space(4);
            }

            GUILayout.BeginHorizontal();
            for (int i = 0; i < NonHostPunishLabels.Length; i++)
            {
                bool selected = (int)Anticheat.nonHostPunishment == i;
                var prev = GUI.backgroundColor;
                var punBg2 = GUIStylePreset.WhiteButtonBg;
                var punStyle2 = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
                punStyle2.normal.background = punBg2; punStyle2.hover.background = punBg2; punStyle2.active.background = punBg2;
                punStyle2.normal.textColor = punStyle2.hover.textColor = punStyle2.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
                GUI.backgroundColor = selected ? NonHostPunishColors[i] : new Color(0.55f, 0.55f, 0.55f);
                if (GUILayout.Button(NonHostPunishLabels[i], punStyle2, GUILayout.Width(100), GUILayout.Height(28)))
                    Anticheat.nonHostPunishment = (Anticheat.NonHostPunishments)i;
                GUI.backgroundColor = prev;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            int sel = (int)Anticheat.nonHostPunishment;
            GUI.color = NonHostPunishColors[sel];
            GUILayout.Label($"  {NonHostPunishDescription(sel)}");
            GUI.color = Color.white;
        }

        private static string HostPunishDescription(int i) => i switch
        {
            0 => "Detections are logged and notified only — nobody gets kicked.",
            1 => "Cheater is kicked from the lobby.",
            2 => "Cheater is kicked with a connection error message. Works in-game.",
            3 => "Cheater is permanently banned from the lobby.",
            _ => ""
        };

        private static string NonHostPunishDescription(int i) => i switch
        {
            0 => "Detection logged and notified only.",
            1 => "Casts 3 votekick votes against the cheater. Leave and rejoin twice to complete.",
            2 => "Sends the ban exploit RPC to the cheater's client. Requires an active game.",
            3 => "Uses the vent kick exploit on the cheater. Requires an active game.",
            _ => ""
        };

        private void DrawRpcDetections()
        {
            GUILayout.Label("RPC Detections", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            int col = 0;
            bool rowOpen = false;
            foreach (var (rpcCall, handler) in Anticheat.RpcHandlers)
            {
                if (col == 0) { GUILayout.BeginHorizontal(); rowOpen = true; }
                handler.Enabled = GUIStylePreset.CustomToggle(handler.Enabled, $" {rpcCall}", GUILayout.Width(200));
                col++;
                if (col == 2) { GUILayout.EndHorizontal(); rowOpen = false; col = 0; }
            }
            if (rowOpen) GUILayout.EndHorizontal();
        }

        private void DrawLevelCheck()
        {
            GUILayout.Label("Level Check", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            bool levelEnabled = Anticheat.RpcHandlers.TryGetValue(RpcCalls.SetLevel, out var levelHandler) && levelHandler.Enabled;
            if (!levelEnabled)
            {
                GUI.color = new Color(0.75f, 0.75f, 0.75f);
                GUILayout.Label("  SetLevel detection is disabled above in RPC Detections.");
                GUI.color = Color.white;
                GUILayout.Space(2);
            }

            int maxLevel = SkidMenu.maxPlayerLevel;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  Max allowed level:  {maxLevel}", GUILayout.Width(200));
            int newMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(maxLevel, 100, 100001, GUILayout.Width(160)));
            if (newMax != maxLevel)
                SkidMenu.maxPlayerLevel = newMax;
            GUILayout.EndHorizontal();
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            GUILayout.Label("  Players above this level will be flagged on SetLevel RPC.");
            GUI.color = Color.white;
        }

        private void DrawModDetection()
        {
            GUILayout.Label("Mod / Menu Detection", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            ModDetection.Enabled = GUIStylePreset.CustomToggle(ModDetection.Enabled, " Enable mod detection");
            GUILayout.Space(4);

            if (!ModDetection.Enabled)
            {
                GUI.color = new Color(0.75f, 0.75f, 0.75f);
                GUILayout.Label("  Detection is off — no mod RPC signatures will be checked.");
                GUI.color = Color.white;
                return;
            }

            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            GUILayout.Label("  Fires when a player sends a known cheat menu RPC signature.");
            GUILayout.Label("  Deduplicated per player per session. Clears on lobby join and game end.");
            GUI.color = Color.white;
            GUILayout.Space(5);

            foreach (var mod in ModDetection.KnownMods)
            {
                GUILayout.BeginHorizontal();
                mod.Enabled = GUIStylePreset.CustomToggle(mod.Enabled, "", GUILayout.Width(18));
                GUI.color = mod.Enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label($"{mod.Name}", GUILayout.Width(180));
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label($"RPC: {string.Join(", ", mod.RpcIds)}", GUILayout.Width(130));
                GUI.color = Color.white;
                if (mod.Enabled)
                {
                    GUI.color = mod.ShouldPunish ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
                    mod.ShouldPunish = GUIStylePreset.CustomToggle(mod.ShouldPunish, " Punish");
                    GUI.color = Color.white;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawBlacklist()
        {
            GUILayout.Label("Blacklist", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            Blacklist.Enabled            = GUIStylePreset.CustomToggle(Blacklist.Enabled, " Enable Blacklist");
            Blacklist.AutoAddFlagged     = GUIStylePreset.CustomToggle(Blacklist.AutoAddFlagged, " Auto-add flagged players");
            Blacklist.AutoAddModDetected = GUIStylePreset.CustomToggle(Blacklist.AutoAddModDetected, " Auto-add mod-detected players");
            Blacklist.AutoPunish         = GUIStylePreset.CustomToggle(Blacklist.AutoPunish, " Auto-punish blacklisted players");
            Blacklist.NotifyOnJoin       = GUIStylePreset.CustomToggle(Blacklist.NotifyOnJoin, " Notify when blacklisted player joins");
            Blacklist.KickOnJoin         = GUIStylePreset.CustomToggle(Blacklist.KickOnJoin,     " Kick on join (host only)");
            Blacklist.BanOnJoin          = GUIStylePreset.CustomToggle(Blacklist.BanOnJoin,      " Ban exploit on join");
            Blacklist.VentKickOnJoin     = GUIStylePreset.CustomToggle(Blacklist.VentKickOnJoin, " Vent kick on join");

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            var prevBg = GUI.backgroundColor;
            var acBg = GUIStylePreset.WhiteButtonBg;
            var acStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
            acStyle.normal.background = acBg; acStyle.hover.background = acBg; acStyle.active.background = acBg;
            acStyle.normal.textColor = acStyle.hover.textColor = acStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
            GUI.backgroundColor = new Color(0.6f, 0.1f, 0.1f);
            if (GUILayout.Button("Clear All", acStyle)) Blacklist.Clear();
            GUI.backgroundColor = prevBg;
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // add current players
            GUILayout.Label($"In-Lobby ({Blacklist.Entries.Count} entries):", GUIStylePreset.ModernLabel);
            if (PlayerControl.AllPlayerControls != null)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.AmOwner || p.Data == null) continue;
                    bool listed = Blacklist.Match(p) != null;
                    var blBg = GUIStylePreset.WhiteButtonBg;
                    var blStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
                    blStyle.normal.background = blBg; blStyle.hover.background = blBg; blStyle.active.background = blBg;
                    blStyle.normal.textColor = blStyle.hover.textColor = blStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
                    GUI.backgroundColor = listed ? new Color(0.5f, 0.1f, 0.1f) : new Color(0.55f, 0.55f, 0.55f);
                    if (GUILayout.Button(listed ? $"✕ Remove {p.Data.PlayerName}" : $"+ Add {p.Data.PlayerName}", blStyle))
                    {
                        if (listed) { var idx = Blacklist.Entries.FindIndex(e => e.FriendCode == p.Data.FriendCode || e.Puid == p.Data.Puid); Blacklist.Remove(idx); }
                        else Blacklist.Add(p, "Manual");
                    }
                    GUI.backgroundColor = prevBg;
                }
            }

            GUILayout.Space(6);
            GUILayout.Label("Blacklist Entries:", GUIStylePreset.ModernLabel);
            _blScroll = GUILayout.BeginScrollView(_blScroll, GUILayout.Height(150));
            for (int i = Blacklist.Entries.Count - 1; i >= 0; i--)
            {
                var e = Blacklist.Entries[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<color=#ffaaaa>{e.Name}</color> <color=#888>{e.FriendCode} | {e.Reason}</color>", GUIStylePreset.ModernLabel);
                GUI.backgroundColor = new Color(0.6f, 0.1f, 0.1f);
                if (GUILayout.Button("✕", GUILayout.Width(24))) Blacklist.Remove(i);
                GUI.backgroundColor = prevBg;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}
