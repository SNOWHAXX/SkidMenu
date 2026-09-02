using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

namespace SkidMenu.features
{
	internal static class AutoExposeImpostors
	{
		public static bool Enabled { get; set; } = false;
		public static bool ExposeOnMurder { get; set; } = true;
		public static bool ExposeOnShapeshift { get; set; } = true;
		// Only triggers on vanish and not unvanish
		// The unvanish animation is much shorter so once everyone gets teleported they might not notice the cloud
		public static bool ExposeOnPhantom { get; set; } = true;

		private const float MIN_KILL_DISTANCE = 1.0f;
		private const float MAX_DISTANCE = 5.0f;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		public static class MurderPatch
		{
			static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				OnPlayerMurder(__instance, target, resultFlags);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
		public static class ShapeshiftPatch
		{
			static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
			{
				OnPlayerShapeshift(__instance, targetPlayer, animate);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleServerVanish))]
		public static class PhantomPatch
		{
			static void Prefix(PlayerControl __instance)
			{
				OnPlayerPhantom(__instance);
			}
		}

		private static void OnPlayerMurder(PlayerControl murderer, PlayerControl target, MurderResultFlags flags)
		{
			if (!Enabled || !ExposeOnMurder || ShipStatus.Instance == null || !flags.HasFlag(MurderResultFlags.Succeeded) || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(murderer, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if (selectedVent == null)
			{
				SkidMenu.Log.LogMessage("Found no applicable vents to teleport players to");
				return;
			}

			foreach (PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if (player == murderer || player == target || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Troll.TeleportToVent(player, selectedVent.Id);
			}
		}

		private static void OnPlayerShapeshift(PlayerControl shapeshifter, PlayerControl target, bool shouldAnimate)
		{
			if (!Enabled || !ExposeOnShapeshift || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(shapeshifter, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if (selectedVent == null)
			{
				SkidMenu.Log.LogMessage("Found no applicable vents to teleport players to");
				return;
			}

			foreach (PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if (player == shapeshifter || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Troll.TeleportToVent(player, selectedVent.Id);
			}
		}

		private static void OnPlayerPhantom(PlayerControl phantom)
		{
			if (!Enabled || !ExposeOnPhantom || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(phantom, 0.0f, MAX_DISTANCE);
			if (selectedVent == null)
			{
				SkidMenu.Log.LogMessage("Found no applicable vents to teleport players to");
				return;
			}

			foreach (PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if (player == phantom || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Troll.TeleportToVent(player, selectedVent.Id);
			}
		}

		private static Vent FindClosestVent(PlayerControl player, float minDistance, float maxDistance)
		{
			foreach (Vent vent in ShipStatus.Instance.AllVents)
			{
				if (vent == null) continue;

				float distance = Vector2.Distance(player.transform.position, vent.transform.position);

				// If the kill is too far away from the vent, then the teleported players will not be able to see the kill
				// If the kill is too close, then players will not be able to determine who killed in the stack
				if (distance < minDistance || distance > maxDistance) continue;

				// We also want to make sure that there isn't an object that would block the teleported player's view to the kill
				// Not perfect, as a lot of objects allow you to see through them
				if (PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false)) continue;

				return vent;
			}

			return null;
		}
	}
}