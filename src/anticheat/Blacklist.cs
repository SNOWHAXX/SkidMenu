using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SkidMenu.anticheat;

public static class Blacklist
{
    public static readonly List<BlacklistEntry> Entries = new();

    public static bool Enabled = true;
    public static bool AutoAddModDetected = false;
    public static bool AutoAddFlagged     = false;
    public static bool AutoPunish         = false;
    public static bool NotifyOnJoin       = true;
    public static bool KickOnJoin         = false;
    public static bool BanOnJoin          = false;
    public static bool VentKickOnJoin     = false;

    private static string FilePath =>
        Path.Combine(BepInEx.Paths.BepInExRootPath, "SkidMenu_Blacklist.txt");

    public static void Add(PlayerControl player, string reason = "Manual")
    {
        if (player?.Data == null) return;
        string fc   = player.Data.FriendCode ?? "";
        string puid = player.Data.Puid ?? "";
        string name = player.Data.PlayerName ?? "?";
        if (string.IsNullOrEmpty(fc) && string.IsNullOrEmpty(puid)) return;
        if (!string.IsNullOrEmpty(fc) && Entries.Exists(e => e.FriendCode == fc)) return;
        Entries.Add(new BlacklistEntry(name, fc, puid, reason));
        SkidMenu.notifications.Send("<color=#ff4444>Blacklist</color>",
            $"Added {name} — {reason}", 3f);
        Save();
    }

    public static void Remove(int index)
    {
        if (index >= 0 && index < Entries.Count) { Entries.RemoveAt(index); Save(); }
    }

    public static void Clear() { Entries.Clear(); Save(); }

    public static BlacklistEntry Match(PlayerControl player)
    {
        if (player?.Data == null) return null;
        string fc   = player.Data.FriendCode ?? "";
        string puid = player.Data.Puid ?? "";
        return Entries.Find(e =>
            (!string.IsNullOrEmpty(fc)   && e.FriendCode == fc)   ||
            (!string.IsNullOrEmpty(puid) && e.Puid       == puid));
    }

    public static void OnFlagged(PlayerControl player)
    {
        if (AutoAddFlagged) Add(player, "Anticheat flag");
        if (AutoPunish) Punish(player);
    }

    public static void OnModDetected(PlayerControl player, string modName)
    {
        if (AutoAddModDetected) Add(player, $"Mod: {modName}");
        if (AutoPunish) Punish(player);
    }

    public static void Punish(PlayerControl player)
    {
        if (BanOnJoin)      { BanHandler.BanPlayer(player); return; }
        if (VentKickOnJoin) { VentKickTab.VentKick(player); return; }
        if (KickOnJoin && AmongUsClient.Instance.AmHost)
            AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
    }

    public static void Save()
    {
        using var w = new StreamWriter(FilePath, false);
        foreach (var e in Entries)
            w.WriteLine($"{e.Name}|{e.FriendCode}|{e.Puid}|{e.Reason}");
    }

    public static void Load()
    {
        Entries.Clear();
        if (!File.Exists(FilePath)) return;
        foreach (var line in File.ReadAllLines(FilePath))
        {
            var p = line.Split('|');
            if (p.Length >= 4) Entries.Add(new BlacklistEntry(p[0], p[1], p[2], p[3]));
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class CheckOnJoin
    {
        private static readonly HashSet<string> _checked = new();

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        static class ClearOnLeave
        {
            static void Postfix(InnerNet.ClientData data)
            {
                if (data?.Character?.Data != null)
                    _checked.Remove(data.Character.Data.FriendCode ?? "");
            }
        }

        static void Postfix(PlayerControl __instance)
        {
            if (!Enabled) return;
            if (__instance == null || __instance.AmOwner) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (AmongUsClient.Instance == null) return;
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) return;

            string fc = __instance.Data?.FriendCode ?? __instance.Data?.Puid ?? "";
            if (string.IsNullOrEmpty(fc) || _checked.Contains(fc)) return;
            _checked.Add(fc);

            PlayerControl.LocalPlayer.StartCoroutine(DelayCheck(__instance).WrapToIl2Cpp());
        }

        static System.Collections.IEnumerator DelayCheck(PlayerControl player)
        {
            yield return new WaitForSeconds(2f);
            if (player?.Data == null) yield break;
            if (MeetingHud.Instance != null) yield break;
            if (AmongUsClient.Instance?.GameState == InnerNet.InnerNetClient.GameStates.Started) yield break;

            var match = Match(player);
            if (match == null) yield break;

            if (NotifyOnJoin)
                SkidMenu.notifications.Send("<color=#ff4444>⚠ Blacklisted</color>",
                    $"{player.Data.PlayerName} joined — {match.Reason}", 5f);

            if (AmongUsClient.Instance.AmHost)
                Punish(player);
        }
    }
}

public class BlacklistEntry
{
    public string Name;
    public string FriendCode;
    public string Puid;
    public string Reason;

    public BlacklistEntry(string name, string fc, string puid, string reason)
    { Name = name; FriendCode = fc; Puid = puid; Reason = reason; }
}
