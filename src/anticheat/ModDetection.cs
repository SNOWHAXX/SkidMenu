using HarmonyLib;
using InnerNet;
using System.Collections.Generic;

namespace SkidMenu.anticheat
{
    internal class ModEntry
    {
        public string Name { get; }
        public bool Enabled { get; set; } = true;
        public bool ShouldPunish { get; set; } = true;
        public byte[] RpcIds { get; }

        public ModEntry(string name, params byte[] rpcIds)
        {
            Name = name;
            RpcIds = rpcIds;
        }
    }

    internal static class ModDetection
    {
        public static bool Enabled { get; set; } = true;

        public static readonly List<ModEntry> KnownMods = new List<ModEntry>
        {
            new ModEntry("Chocoo",                    121),
            new ModEntry("TuffMenu",                  167),
            new ModEntry("Hydra / Sicko / SickoMenu", 164),
            new ModEntry("HostGuard / TOH",           176),
            new ModEntry("Polar",                     195, 204),
            new ModEntry("GNC",                       154),
            new ModEntry("KillNet",                   250, 80, 85),
            new ModEntry("BetterAmongUs",             150),
            new ModEntry("Unknown (RPC 82)",          82),
            new ModEntry("Unknown (Askinchik)",       103),
            new ModEntry("BanMod",                    212, 213, 214, 215, 216, 217, 218),
            new ModEntry("Gaff Menu",                 144, 145),
            new ModEntry("GMM",                       188, 189),
            new ModEntry("Malum Menu",                169),
            new ModEntry("Lunar / NjordMenu",         133, 210),
            new ModEntry("NjordMenu (Old)",           89),
        };

        private static readonly Dictionary<byte, ModEntry> _rpcToMod = new();
        private static readonly HashSet<int> _alreadyFlagged = new();
        public static readonly Dictionary<byte, HashSet<string>> DetectedMods = new();

        static ModDetection()
        {
            RebuildIndex();
        }

        public static void RebuildIndex()
        {
            _rpcToMod.Clear();
            foreach (var mod in KnownMods)
                foreach (byte id in mod.RpcIds)
                    _rpcToMod[id] = mod;
        }

        public static void ClearFlagged() { _alreadyFlagged.Clear(); DetectedMods.Clear(); }

        public static bool IsModUser(byte playerId) => DetectedMods.ContainsKey(playerId) && DetectedMods[playerId].Count > 0;

        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        static class ClearOnLobby
        {
            static void Postfix() => _alreadyFlagged.Clear();
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
        static class ClearOnGameEnd
        {
            static void Prefix() => _alreadyFlagged.Clear();
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        static class ModDetectionRpcPatch
        {
            static void Postfix(PlayerControl __instance, byte callId)
            {
                TryDetect(__instance, callId);
            }
        }

        static void TryDetect(PlayerControl player, byte callId)
        {
            if (!Enabled) return;
            if (player == null || player.Data == null) return;
            if (player.AmOwner) return;
            if (!_rpcToMod.TryGetValue(callId, out ModEntry mod)) return;
            if (!mod.Enabled) return;
            int key = (player.PlayerId << 16) | callId;
            if (_alreadyFlagged.Contains(key)) return;
            _alreadyFlagged.Add(key);
            if (!DetectedMods.TryGetValue(player.PlayerId, out var mods))
                DetectedMods[player.PlayerId] = mods = new HashSet<string>();
            mods.Add(mod.Name);
            Blacklist.OnModDetected(player, mod.Name);
            string playerName = player.Data.PlayerName ?? "Unknown";
            Anticheat.Flag(player, $"{playerName} is using {mod.Name} (RPC {callId})", shouldPunish: Anticheat.Enabled && mod.ShouldPunish);
        }
    }
}
