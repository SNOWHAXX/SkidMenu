using HarmonyLib;
using InnerNet;

namespace SkidMenu.features
{
	internal static class NoKillChecks
	{
		public static bool Enabled { get; set; } = false;
		public static bool KillOtherImpostors { get; set; } = false;
		public static bool KillAsPhantom { get; set; } = false;

		// The backend AU servers rely on the CheckVanish and CheckAppear RPCs to know if a player has vanished
		// This information is then used by the server's CheckMurder RPC handler to know if a kill should be authorized
		// No idea why, but Innersloth has secured the CheckVanish and CheckAppear RPCs to hell, while the Vanish and Appear RPCs have almost-zero protection
		// Sending CheckVanish in the lobby, as non-phantom, or while already phantomed, in cooldown, or with the maxDuration field mismatched to the game settings phantom duration will result in a ban
		// Sending CheckAppear in the lobby, as non-phantom, or while non-vanished, will also result in a kick from the lobby
		// But sending Vanish or Appear in any of those conditions will result in the RPC being relayed to other players
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
		public static class VanishBypass
		{
			static bool Prefix(PlayerControl __instance)
			{
				if (!Enabled || !KillAsPhantom) return true;

				Network.BatchedMessage batch = new Network.BatchedMessage();
				batch.UseAnticheatBypass();
				batch.QueueVanish(__instance);
				batch.FinishBatch();
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
		public static class AppearBypass
		{
			static bool Prefix(PlayerControl __instance, bool shouldAnimate)
			{
				if (!Enabled || !KillAsPhantom) return true;

				Network.BatchedMessage batch = new Network.BatchedMessage();
				batch.UseAnticheatBypass();
				batch.QueueAppear(__instance, shouldAnimate);
				batch.FinishBatch();
				return false;
			}
		}

		public static bool IsValidTarget(NetworkedPlayerInfo target)
		{
			return target != null &&
			       target != PlayerControl.LocalPlayer.Data &&
			       !target.Disconnected &&
			       !target.IsDead &&
			       (!RoleManager.IsImpostorRole(target.RoleType) || KillOtherImpostors);
		}
	}
}