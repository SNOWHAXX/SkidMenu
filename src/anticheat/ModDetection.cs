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
            // BetterAmongUs and its fork GreaterAmongUs share the same RPC protocol / handshake,
            // so they are indistinguishable at the RPC layer and share the same fingerprint.
            new ModEntry("BetterAmongUs / GreaterAmongUs", 150),
            new ModEntry("Unknown (RPC 82)",          82),
            new ModEntry("Unknown (Askinchik)",       103),
            new ModEntry("BanMod",                    212, 213, 214, 215, 216, 217, 218),
            new ModEntry("Gaff Menu",                 144, 145),
            new ModEntry("GMM",                       188, 189),
            new ModEntry("Malum Menu",                169),
            new ModEntry("Lunar / NjordMenu",         133, 210),
            new ModEntry("NjordMenu (Old)",            89),
            // Starlight (All Of Us mod loader) reports itself via platform 112, not an RPC
            // signature. It gets a KnownMods entry so it shows up in the UI / config and can be
            // toggled and punished like every other mod; detection itself is platform-based and
            // handled in PlatformSpoofer.DetectStarlight (no RPC ids are used here).
            new ModEntry("Starlight"),
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

        // Comma-joined names of every mod/cheat detected on a player, or null when none.
        public static string GetModNames(byte playerId)
        {
            if (DetectedMods.TryGetValue(playerId, out var mods) && mods.Count > 0)
                return string.Join(", ", mods);
            return null;
        }

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

			// Fall back to a known cheat-menu signature set first
			if (_rpcToMod.TryGetValue(callId, out ModEntry mod))
			{
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
				return;
			}

			// An RPC id that is not part of the vanilla game's RpcCalls enum from a non-host is
			// an unregistered/cheat custom RPC. Dedup once per (player, callId) per session.
			if (!IsVanillaRpc(callId))
			{
				CheckUnknownRpc(player, callId);
			}
		}

		private static bool IsVanillaRpc(byte callId)
		{
			foreach (RpcCalls rpc in System.Enum.GetValues(typeof(RpcCalls)))
			{
				if (unchecked((byte)rpc) == callId) return true;
			}
			return false;
		}

		private static void CheckUnknownRpc(PlayerControl player, byte callId)
		{
			int key = (player.PlayerId << 16) | callId;
			if (_alreadyFlagged.Contains(key)) return;
			_alreadyFlagged.Add(key);

			if (!DetectedMods.TryGetValue(player.PlayerId, out var mods))
				DetectedMods[player.PlayerId] = mods = new HashSet<string>();
			mods.Add("Unregistered RPC");

			Blacklist.OnModDetected(player, "Unregistered RPC");
			string playerName = player.Data.PlayerName ?? "Unknown";
			Anticheat.Flag(player, $"{playerName} sent unregistered RPC {callId}", shouldPunish: Anticheat.Enabled);
		}
    }
}
