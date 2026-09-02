using System.Collections.Generic;
using Hazel;
using InnerNet;
using UnityEngine;

namespace SkidMenu.anticheat
{
    internal class SpoofEntry
    {
        public string Name { get; }
        public byte RpcId { get; }
        // ModMenuCrew identifies with an MMC_vX + version payload rather than a bare ping
        public bool IsModMenuCrew { get; }

        public SpoofEntry(string name, byte rpcId, bool isModMenuCrew = false)
        {
            Name = name;
            RpcId = rpcId;
            IsModMenuCrew = isModMenuCrew;
        }
    }

    // Spoofs the mod-menu identification RPC that other mods / anticheat systems fingerprint.
    // Lets us present ourselves to a lobby as a different menu (or a valid version of one).
    internal static class SpoofIdentity
    {
        public static bool Enabled { get; set; } = false;
        public static int SelectedIndex { get; set; } = 0;
        // Re-broadcast our spoofed identity every ~30s so scanners keep seeing it.
        public static bool AutoBroadcast { get; set; } = false;

        private static float _nextBroadcastTime = 0f;

        // Merged from ModDetection.KnownMods signature set + ChocooMenu's spoof list, so the
        // menus we can impersonate line up with the ones we actually fingerprint.
        public static readonly IReadOnlyList<SpoofEntry> Menus = new List<SpoofEntry>
        {
            new SpoofEntry("Chocoo (Default)",       121),
            new SpoofEntry("TuffMenu",               167),
            new SpoofEntry("Hydra / Sicko / SickoMenu", 164),
            new SpoofEntry("KillNet",                250),
            new SpoofEntry("HostGuard / TOH",        176),
            new SpoofEntry("BetterAmongUs / GreaterAmongUs", 150),
            new SpoofEntry("GNC / GoatNetClient",    154),
            new SpoofEntry("Polar",                  195),
            new SpoofEntry("Malum Menu",             169),
            new SpoofEntry("Gaff Menu",              144),
            new SpoofEntry("GMM",                    188),
            new SpoofEntry("BanMod",                 212),
            new SpoofEntry("Lunar / NjordMenu",      133),
            new SpoofEntry("NjordMenu (Old)",        89),
            new SpoofEntry("AmongUsMenu",            85),
            new SpoofEntry("NetMenu",                162),
            new SpoofEntry("ModMenuCrew",            202, true),
            new SpoofEntry("UnknownMenu",            255),
        };

        public static SpoofEntry Current => Menus[SelectedIndex % Menus.Count];

        // Write our own spoofed identity into ModDetection so our own ESP / mod-user tagging
        // shows us as the menu we are impersonating, keeping everything consistent.
        public static void TrackOwnUsage()
        {
            if (PlayerControl.LocalPlayer == null) return;
            byte pid = PlayerControl.LocalPlayer.PlayerId;

            if (!Enabled)
            {
                ModDetection.DetectedMods.Remove(pid);
                return;
            }

            var mods = new HashSet<string>();
            SpoofEntry cur = Current;
            mods.Add(cur.Name);
            if (cur.IsModMenuCrew) mods.Add("ModMenuCrew");

            // Tag ourselves as that menu so ESP shows it, without triggering Anticheat.Flag on self.
            ModDetection.DetectedMods[pid] = mods;
        }

        public static void Broadcast()
        {
            try
            {
                if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null) return;

                SpoofEntry cur = Current;
                byte id = cur.RpcId;

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    id,
                    SendOption.Reliable,
                    -1);

                if (cur.IsModMenuCrew)
                {
                    // Impersonate a current ModMenuCrew client so version checks pass.
                    writer.Write("MMC_v5");
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write("6.0.0");
                }
                else if (id == 121)
                {
                    writer.Write("CHOCOO_PING");
                }

                AmongUsClient.Instance.FinishRpcImmediately(writer);
                SkidMenu.notifications?.Send("Spoof Identity", $"Broadcasting as {cur.Name} (RPC {id})", 4);
            }
            catch (System.Exception ex)
            {
                SkidMenu.Log.LogError("SpoofIdentity.Broadcast: " + ex.Message);
            }
        }

        // Periodically re-broadcast the spoofed identity so other scanners keep seeing it.
        [HarmonyLib.HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        static class AutoBroadcastTimer
        {
            static void Postfix(HudManager __instance)
            {
                if (!Enabled || !AutoBroadcast) return;
                if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null) return;
                if (Time.unscaledTime < _nextBroadcastTime) return;

                _nextBroadcastTime = Time.unscaledTime + 30f;
                Broadcast();
            }
        }
    }
}
