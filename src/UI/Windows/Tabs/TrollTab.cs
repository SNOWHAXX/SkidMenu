using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SkidMenu.features;
using SkidMenu.routines;
using UnityEngine;
namespace SkidMenu;

public class TrollTab : ITab
{
    private int _selectedVent = 0;
    public string name => "Troll";

    public void Draw()
    {
        if (PlayerControl.LocalPlayer == null)
            GUILayout.Label("You are not currently in a game, these options will not work.");

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawAutoReport();
        DrawAutoSpores();
        DrawGlitterBomb();
        DrawExposeImpostors();
        DrawDisableCloseDoors();
        DrawDisableCameras();
        DrawQueueLobbyCrash();
        DrawBlockToggles();
        DrawHnSTimer();
        DrawActionButtons();
        DrawDoorTroller();
        DrawVentTp();
        DrawZipline();
        DrawColouredChat();
        DrawSkidMenuBrand();

        GUILayout.EndVertical();
    }

    private void DrawAutoReport()
    {
        Troll.AutoReportBodies.Enabled = GUIStylePreset.CustomToggle(Troll.AutoReportBodies.Enabled, "Automatically Report Bodies");
        if (Troll.AutoReportBodies.Enabled)
        {
            GUILayout.Label($"   Report Delay: {Troll.AutoReportBodies.ReportDelay:F2}s");
            Troll.AutoReportBodies.ReportDelay = GUILayout.HorizontalSlider(Troll.AutoReportBodies.ReportDelay, 0f, 2f);
            Troll.AutoReportBodies.CrewOnly = GUIStylePreset.CustomToggle(Troll.AutoReportBodies.CrewOnly, "   Only If Crew");
        }
    }

    private void DrawAutoSpores()
    {
        SkidMenu.routines.autoTriggerSpores.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
    }

    private void DrawGlitterBomb()
    {
        SkidMenu.routines.glitterBomb.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.glitterBomb.Enabled, "GlitterBomb");
    }

    private void DrawExposeImpostors()
    {
        AutoExposeImpostors.Enabled = GUIStylePreset.CustomToggle(AutoExposeImpostors.Enabled, "Auto Expose Impostors");
        if (AutoExposeImpostors.Enabled)
        {
            AutoExposeImpostors.ExposeOnMurder = GUIStylePreset.CustomToggle(AutoExposeImpostors.ExposeOnMurder, "   On Murder");
            AutoExposeImpostors.ExposeOnShapeshift = GUIStylePreset.CustomToggle(AutoExposeImpostors.ExposeOnShapeshift, "   On Shapeshift");
            AutoExposeImpostors.ExposeOnPhantom = GUIStylePreset.CustomToggle(AutoExposeImpostors.ExposeOnPhantom, "   On Phantom");
        }
    }

    private void DrawDisableCloseDoors()
    {
        DisableCloseDoors.Enabled = GUIStylePreset.CustomToggle(DisableCloseDoors.Enabled, "Disable Close Doors");
    }

    private void DrawDisableCameras()
    {
        DisableCameras.Enabled = GUIStylePreset.CustomToggle(DisableCameras.Enabled, "Disable Security Cameras");
    }

    private void DrawQueueLobbyCrash()
    {
        QueueLobbyCrash.Enabled = GUIStylePreset.CustomToggle(QueueLobbyCrash.Enabled, "Queue Lobby Crash");
    }

    private void DrawBlockToggles()
    {
        Troll.BlockSabotages.Enabled = GUIStylePreset.CustomToggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
        Troll.BlockVenting.Enabled = GUIStylePreset.CustomToggle(Troll.BlockVenting.Enabled, "Disable Vents");
    }

    private void DrawHnSTimer()
    {
        SkidMenu.routines.hnsTimerDeplete.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.hnsTimerDeplete.Enabled, "Deplete HnS Timer");
    }



    private void DrawActionButtons()
    {
        GUILayout.Space(5);

        if (GUILayout.Button("Copy Random Player"))
            CopyRandomPlayer();

        if (GUILayout.Button("Trigger All Spores"))
            TriggerAllSpores();
    }

    private void DrawDoorTroller()
    {
        GUILayout.Space(5);
        GUILayout.Label("Door Troller:");
        SkidMenu.routines.doorTroller.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.doorTroller.Enabled, "Enabled");
        GUILayout.Label($"Lock and Unlock Delay: {SkidMenu.routines.doorTroller.lockAndUnlockDelay:F2}s");
        SkidMenu.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(SkidMenu.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
    }

    private void DrawVentTp()
    {
        GUILayout.Space(5);
        GUILayout.Label("Vent TP:");
        SkidMenu.routines.teleportFlooder.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.teleportFlooder.Enabled, "Teleport Flooder");

        int maxVent = ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10;
        if (_selectedVent > maxVent) _selectedVent = maxVent;
        GUILayout.Label($"Teleport everyone to vent: {_selectedVent}");
        _selectedVent = (int)GUILayout.HorizontalSlider(_selectedVent, 0, maxVent);

        if (GUILayout.Button("Teleport to Vent"))
            TeleportEveryoneToVent(_selectedVent);

        if (GUILayout.Button("Teleport to Random Vent"))
            TeleportEveryoneToRandomVent();
    }

    private void CopyRandomPlayer()
    {
        PlayerControl randomPl = Utilities.GetRandomPlayer();
        Utilities.CopyPlayer(randomPl);
    }

    private void TriggerAllSpores()
    {
        if (Utilities.GetCurrentMap() != MapNames.Fungle)
        {
            SkidMenu.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
            return;
        }

        FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();
        foreach (Mushroom mushroom in shipStatus.sporeMushrooms.Values)
            PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
        SkidMenu.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
    }

    private void TeleportEveryoneToVent(int ventId)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            Troll.TeleportToVent(player, ventId);
    }

    private void TeleportEveryoneToRandomVent()
    {
        System.Random rnd = new System.Random();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == PlayerControl.LocalPlayer) continue;
            Troll.TeleportToVent(player, rnd.Next(0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count : 1));
        }
    }

    private static void DrawZipline()
    {
        GUILayout.Space(8);
        GUILayout.Label("Zipline (Fungle)");

        GUILayout.BeginHorizontal();
        GUILayout.Space(2);
        if (GUILayout.Button("Select All", GUILayout.Width(110), GUILayout.Height(28))) { foreach (var p in PlayerControl.AllPlayerControls) if (p != null && p != PlayerControl.LocalPlayer) ZiplineSpamRoutine.Marked.Add(p.PlayerId); }
        GUILayout.Space(4);
        if (GUILayout.Button("Clear All", GUILayout.Width(90), GUILayout.Height(28))) { ZiplineSpamRoutine.Marked.Clear(); ZiplineSpamRoutine.Active = false; }
        GUILayout.Space(4);
        if (GUILayout.Button("Down All", GUILayout.Width(90), GUILayout.Height(28))) features.ZiplineTools.RideAll(ZiplineSpamRoutine.Marked, true);
        GUILayout.Space(4);
        if (GUILayout.Button("Up All", GUILayout.Width(80), GUILayout.Height(28))) features.ZiplineTools.RideAll(ZiplineSpamRoutine.Marked, false);
        GUILayout.Space(4);
        var spamGreen = new Color(0.0f, 0.45f, 0.0f);
        var spamBg = GUI.backgroundColor;

        bool downAllActive = ZiplineSpamRoutine.Active && ZiplineSpamRoutine.SpamDirection && !ZiplineSpamRoutine.IsPerPlayer;
        bool upAllActive = ZiplineSpamRoutine.Active && !ZiplineSpamRoutine.SpamDirection && !ZiplineSpamRoutine.IsPerPlayer;

        GUI.backgroundColor = downAllActive ? spamGreen : spamBg;
        if (GUILayout.Button("Spam Down All", GUILayout.Width(110), GUILayout.Height(28)))
        {
            if (downAllActive) ZiplineSpamRoutine.Stop();
            else { ZiplineSpamRoutine.IsPerPlayer = false; ZiplineSpamRoutine.SpamDirection = true; ZiplineSpamRoutine.Active = true; }
        }
        GUILayout.Space(4);
        GUI.backgroundColor = upAllActive ? spamGreen : spamBg;
        if (GUILayout.Button("Spam Up All", GUILayout.Width(100), GUILayout.Height(28)))
        {
            if (upAllActive) ZiplineSpamRoutine.Stop();
            else { ZiplineSpamRoutine.IsPerPlayer = false; ZiplineSpamRoutine.SpamDirection = false; ZiplineSpamRoutine.Active = true; }
        }
        GUI.backgroundColor = spamBg;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p == PlayerControl.LocalPlayer || p.Data == null) continue;
            bool mark = ZiplineSpamRoutine.Marked.Contains(p.PlayerId);
            string skinHex = ColorCache.ToHex(Palette.PlayerColors[p.Data.DefaultOutfit.ColorId]);
            string roleHex = ColorCache.ToHex(Utils.GetCustomRoleColor(p.Data));
            string roleName = p.Data.RoleType.ToString();
            string label = $"<color=#{skinHex}><b>{p.Data.PlayerName}</b></color>  <color=#{roleHex}>{roleName}</color>";

            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = mark ? new Color(0.0f, 0.45f, 0.0f) : prev;
            if (GUILayout.Button(label, GUILayout.Height(26)))
            {
                if (!ZiplineSpamRoutine.Marked.Remove(p.PlayerId)) ZiplineSpamRoutine.Marked.Add(p.PlayerId);
            }
            GUI.backgroundColor = prev;
            GUILayout.Space(4);
            if (GUILayout.Button("Down", GUILayout.Width(70), GUILayout.Height(26))) features.ZiplineTools.Ride(p, true);
            GUILayout.Space(4);
            if (GUILayout.Button("Up", GUILayout.Width(60), GUILayout.Height(26))) features.ZiplineTools.Ride(p, false);
            GUILayout.Space(4);
            bool pDownActive = ZiplineSpamRoutine.Active && ZiplineSpamRoutine.SpamDirection && ZiplineSpamRoutine.IsPerPlayer && ZiplineSpamRoutine.PerPlayerId == p.PlayerId;
            bool pUpActive = ZiplineSpamRoutine.Active && !ZiplineSpamRoutine.SpamDirection && ZiplineSpamRoutine.IsPerPlayer && ZiplineSpamRoutine.PerPlayerId == p.PlayerId;

            GUI.backgroundColor = pDownActive ? spamGreen : prev;
            if (GUILayout.Button("Spam Down", GUILayout.Width(90), GUILayout.Height(26)))
                ZiplineSpamRoutine.StartPerPlayer(p.PlayerId, true);
            GUILayout.Space(4);
            GUI.backgroundColor = pUpActive ? spamGreen : prev;
            if (GUILayout.Button("Spam Up", GUILayout.Width(80), GUILayout.Height(26)))
                ZiplineSpamRoutine.StartPerPlayer(p.PlayerId, false);
            GUI.backgroundColor = prev;
            GUILayout.Space(2);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }


    }

    private static void DrawColouredChat()
    {
        GUILayout.Space(8);
        GUILayout.Label("Coloured Chat (send emotes)");
        GUILayout.Label("Each button sends a coloured quick-chat entry to the lobby.", new GUIStyle(GUI.skin.label) { fontSize = 10 });

        // (label, rgb, quick-chat id) — copied from ChocooMenu's Coloured Chat feature.
        var buttons = new (string label, Color rgb, int id)[]
        {
            ("1",  new Color(0.8f, 0.2f, 0.2f), 1912),
            ("2",  new Color(0.2f, 0.6f, 0.8f), 197),
            ("3",  new Color(0.2f, 0.7f, 0.5f), 155),
            ("4",  new Color(0.6f, 0.4f, 0.8f), 156),
            ("5",  new Color(0.8f, 0.6f, 0.2f), 73),
            ("6",  new Color(0.9f, 0.3f, 0.9f), 1914),
            ("7",  new Color(0.9f, 0.4f, 0.6f), 1567),
            ("8",  new Color(0.7f, 0.5f, 0.9f), 6),
            ("9",  new Color(0.5f, 0.6f, 0.9f), 269),
            ("10", new Color(1f,   0.5f, 0.3f), 96),
            ("11", new Color(0.3f, 0.9f, 0.5f), 95),
            ("12", new Color(0.5f, 0.3f, 0.9f), 700),
            ("13", new Color(0.9f, 0.7f, 0.3f), 350),
            ("14", new Color(0.3f, 0.5f, 0.9f), 400),
            ("15", new Color(0.9f, 0.3f, 0.5f), 1650),
        };

        int index = 0;
        while (index < buttons.Length)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 5 && index < buttons.Length; col++, index++)
            {
                var b = buttons[index];
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = b.rgb;
                if (GUILayout.Button(b.label, GUILayout.Width(40), GUILayout.Height(30)))
                    features.Troll.SendColouredChat(b.id);
                GUI.backgroundColor = prev;
                GUILayout.Space(3);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
    }

    private static void DrawSkidMenuBrand()
    {
        GUILayout.Space(8);
        GUILayout.Label("SkidMenu (test emotes)", new GUIStyle(GUIStylePreset.TabSubtitle) { fontSize = 12 });
        GUILayout.Label("5 new quick-chat emotes - test which render coloured in the lobby.", new GUIStyle(GUI.skin.label) { fontSize = 10 });

        // (label, rgb, quick-chat id) - 198/1913/1915 confirmed from ChocooMenu,
        // 1916/1917 are the next sequential eggs for testing.
        var tests = new (string label, Color rgb, int id)[]
        {
            ("A", new Color(0.4f, 0.9f, 0.4f), 198),
            ("B", new Color(0.9f, 0.4f, 0.4f), 1913),
            ("C", new Color(0.4f, 0.4f, 0.9f), 1915),
            ("D", new Color(0.9f, 0.9f, 0.3f), 1916),
            ("E", new Color(0.8f, 0.3f, 0.8f), 1917),
        };

        var prev = GUI.backgroundColor;
        foreach (var t in tests)
        {
            GUI.backgroundColor = t.rgb;
            if (GUILayout.Button(t.label, GUILayout.Width(60), GUILayout.Height(30)))
                features.Troll.SendColouredChat(t.id);
            GUI.backgroundColor = prev;
            GUILayout.Space(3);
        }
        GUILayout.Space(5);
    }

    private static void DrawSpam()
    {
    }
}


