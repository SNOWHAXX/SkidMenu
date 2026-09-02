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
			{ RpcCalls.SetLevel,         new SetLevel() },
			{ RpcCalls.MurderPlayer,     new MurderPlayer() },
			{ RpcCalls.CheckMurder,      new CheckMurder() },
			{ RpcCalls.CheckShapeshift,  new CheckShapeshift() },
			{ RpcCalls.CheckVanish,      new CheckVanish() },
			{ RpcCalls.CheckProtect,     new CheckProtect() },
			{ RpcCalls.SetRole,          new SetRole() },
			{ RpcCalls.SetTasks,         new SetTasks() },
			{ RpcCalls.CloseMeeting,     new CloseMeeting() },
			{ RpcCalls.ExtendLobbyTimer, new ExtendLobbyTimer() },
			{ RpcCalls.SyncSettings,     new SyncSettings() },
			{ RpcCalls.CastVote,         new CastVote() },
			{ RpcCalls.ClearVote,        new ClearVote() },
			{ RpcCalls.QueueOverruleVotes, new QueueOverruleVotes() },
			{ RpcCalls.SnapTo,           new SnapTo() },
			{ RpcCalls.UsePlatform,      new UsePlatform() },
			{ RpcCalls.AddVote,          new AddVote() },
			{ RpcCalls.VotingComplete,   new VotingComplete() }
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

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.HandleRpc))]
		class OnVoteBanSystemRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				// Votekick votes do not map to a specific PlayerControl, so we pass null
				// and let the AddVote check resolve the source from the RPC payload itself
				return HandleRpc(typeof(VoteBanSystem), null, (RpcCalls)callId, reader);
			}
		}

		private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
		{
			features.AdvancedLogger.Rpc(sourceNetObj?.Name ?? "?", rpc.ToString(), player?.Data?.PlayerName);

			if (player != null && player.AmOwner) return true;

			RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
			if (!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

			if (CheckInLobbyRpc(player, rpc)) return false;
			if (CheckInGameplayCosmetics(player, rpc)) return false;

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

		// Ported from GreaterAmongUs/BetterAmongUs: RPCs that are only ever valid during a live
		// game are a guaranteed cheat when a non-host sends them from the lobby.
		private static readonly HashSet<RpcCalls> GameOnlyRpcs = new HashSet<RpcCalls>
		{
			RpcCalls.CompleteTask, RpcCalls.MurderPlayer, RpcCalls.CheckMurder,
			RpcCalls.ReportDeadBody, RpcCalls.StartMeeting, RpcCalls.Exiled,
			RpcCalls.EnterVent, RpcCalls.ExitVent, RpcCalls.BootFromVent,
			RpcCalls.ClimbLadder, RpcCalls.UsePlatform, RpcCalls.UseZipline,
			RpcCalls.CloseDoorsOfType, RpcCalls.CloseMeeting, RpcCalls.CastVote,
			RpcCalls.ClearVote, RpcCalls.SendChatNote, RpcCalls.SetRole, RpcCalls.SetTasks,
			RpcCalls.CheckShapeshift, RpcCalls.Shapeshift, RpcCalls.RejectShapeshift,
			RpcCalls.CheckProtect, RpcCalls.ProtectPlayer, RpcCalls.CheckAppear,
			RpcCalls.StartAppear, RpcCalls.CheckVanish, RpcCalls.StartVanish,
			RpcCalls.QueueOverruleVotes, RpcCalls.SetInfected, RpcCalls.TriggerSpores, RpcCalls.CheckSpore,
			RpcCalls.CancelPet, RpcCalls.Pet, RpcCalls.ExtendLobbyTimer, RpcCalls.SyncSettings,
			RpcCalls.LobbyTimeExpiring
		};

		private static bool CheckInLobbyRpc(PlayerControl player, RpcCalls rpc)
		{
			// Only applies while sitting in the lobby (not in a live game, not freeplay)
			if (!Utils.isLobby) return false;
			if (player == null || player.Data == null) return false;

			if (GameOnlyRpcs.Contains(rpc))
			{
				Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while in the lobby.");
				return true;
			}

			return false;
		}

		// Ported from GreaterAmongUs/BetterAmongUs: cosmetic RPCs are legit layout choices, so a
		// non-host firing them once the game is live is a strong cheat tell.
		private static readonly HashSet<RpcCalls> GameplayCosmeticsRpcs = new HashSet<RpcCalls>
		{
			RpcCalls.SetColor, RpcCalls.SetHatStr, RpcCalls.SetSkinStr, RpcCalls.SetVisorStr,
			RpcCalls.SetPetStr, RpcCalls.SetNamePlateStr, RpcCalls.CheckName, RpcCalls.CheckColor
		};

		private static bool CheckInGameplayCosmetics(PlayerControl player, RpcCalls rpc)
		{
			// Only applies during a live game outside the lobby
			if (!Utils.isInGame || Utils.isLobby) return false;
			if (player == null || player.Data == null) return false;

			if (GameplayCosmeticsRpcs.Contains(rpc))
			{
				Flag(player, $"{player.Data.PlayerName} changed cosmetics ({rpc}) during gameplay.");
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

		// For violations where we can not pin the blame on a specific player
		public static void Flag(string reason)
		{
			if (sendNotification)
				SkidMenu.notifications.Send("Anticheat", reason, NotificationDuration);
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
