using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SkidMenu.features;
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
        DrawBlockToggles();
        DrawHnSTimer();
        DrawActionButtons();
        DrawDoorTroller();
        DrawVentTp();

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
}
