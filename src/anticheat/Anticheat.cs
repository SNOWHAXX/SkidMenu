using HarmonyLib;
using Hazel;
using SkidMenu.anticheat.rpc;
using System;
using System.Collections.Generic;

namespace SkidMenu.anticheat
{
	internal class Anticheat
	{
		public static bool Enabled { get; set; } = true;

		public static Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>(16)
		{
			{ RpcCalls.PlayAnimation,    new PlayAnimation() },
			{ RpcCalls.CompleteTask,     new CompleteTask() },
			{ RpcCalls.Exiled,           new Exiled() },
			{ RpcCalls.CheckName,        new CheckName() },
			{ RpcCalls.SetName,          new SetName() },
			{ RpcCalls.SetColor,         new SetColor() },
			{ RpcCalls.ReportDeadBody,   new ReportDeadBody() },
			{ RpcCalls.SetScanner,       new SetScanner() },
			{ RpcCalls.SetStartCounter,  new SetStartCounter() },
			{ RpcCalls.EnterVent,        new EnterVent() },
			{ RpcCalls.ExitVent,         new ExitVent() },
			{ RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
			{ RpcCalls.ClimbLadder,      new ClimbLadder() },
			{ RpcCalls.UpdateSystem,     new UpdateSystem() },
			{ RpcCalls.SetLevel,         new SetLevel() }
		};

		public static bool CheckSpoofedPlatforms { get; set; } = true;

		public enum Punishments { None, Kick, ErrorKick, Ban }
		public enum NonHostPunishments { None, Votekick, BanExploit, VentKick }

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static NonHostPunishments nonHostPunishment = NonHostPunishments.None;
		public static bool sendNotification = true;
		public static bool discardRpc = true;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(ShipStatus __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
			}
		}

		private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
		{
			if (player != null && player.AmOwner) return true;

			RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
			if (!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

			if (rpcCheck.GetExpectedNetObject() != sourceNetObj) return false;

			if (player != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
			{
				Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while non-host.");
				return false;
			}

			int oldReadPosition = reader.Position;
			bool blockRpc = false;

			rpcCheck.Validate(player, reader, ref blockRpc);

			if (!discardRpc || !blockRpc)
			{
				reader.Position = oldReadPosition;
				return true;
			}

			return false;
		}

		public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
		{
			if (player != null && player.AmOwner) return;

			Blacklist.OnFlagged(player);

			if (sendNotification)
				SkidMenu.notifications.Send("Anticheat", reason, NotificationDuration);

			if (shouldPunish)
			{
				if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
					HostPunish(player);
				else
					NonHostPunish(player);
			}
		}

		private static void HostPunish(PlayerControl player)
		{
			if (player == null) return;
			switch (punishment)
			{
				case Punishments.Kick:
				case Punishments.ErrorKick:
					SkidMenu.Log.LogMessage($"{player.Data.PlayerName} was kicked by SkidMenu Anticheat");
					if (punishment == Punishments.Kick || LobbyBehaviour.Instance != null)
						AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					else
						AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
					break;

				case Punishments.Ban:
					SkidMenu.Log.LogMessage($"{player.Data.PlayerName} was banned by SkidMenu Anticheat");
					AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
					break;
			}
		}

		private static void NonHostPunish(PlayerControl player)
		{
			if (player == null || player.Data == null) return;
			switch (nonHostPunishment)
			{
				case NonHostPunishments.Votekick:
					if (VoteBanSystem.Instance == null) break;
					for (int i = 0; i < 3; i++)
						VoteBanSystem.Instance.CmdAddVote(player.Data.ClientId);
					break;

				case NonHostPunishments.BanExploit:
					BanHandler.BanPlayer(player);
					break;

				case NonHostPunishments.VentKick:
					VentKickTab.VentKick(player);
					break;
			}
		}
	}
}
