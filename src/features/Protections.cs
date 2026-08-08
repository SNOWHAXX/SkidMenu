using HarmonyLib;
using Hazel;
using InnerNet;

namespace SkidMenu.features
{
	internal class Protections
	{
		public static bool BlockInvalidLadderOverload { get; set; } = true;
		public static bool AntiExploits { get; set; } = false;
		public static bool BlockLargeGameMessages { get; set; } = true;
		public static bool BlockInvalidGameDataMessages { get; set; } = true;
		public static bool BlockUnauthorizedSystemUpdates { get; set; } = true;
		public static bool ProtectAgainstNonHostKickExploit { get; set; } = true;

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
		public static class ForceDTLS
		{
			public static bool Enabled { get; set; } = true;

			static void Prefix(ref bool dtls)
			{
				if(Enabled) dtls = true;
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		public static class BlockServerTeleports
		{
			public static bool Enabled { get; set; } = false;

			static bool Prefix(CustomNetworkTransform __instance, byte callId)
			{
				if(!Enabled || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				SkidMenu.Log.LogMessage($"Recived SnapTo RPC for our player, since block server teleports is enabled we will disregard the RPC");
				return false;
			}
		}

		// Among Us had this bug where if you reported the body of a player who has left, the anticheat would incorrectly ban you from the lobby
		// To prevent incorrect lobby bans, we block reporting bodies of players who left
		/*
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
		public static class PreventReportBan
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
			{
				if(
					// Only make it so only our body reports run
					__instance.NetId == PlayerControl.LocalPlayer.NetId &&
					// Make sure its not an emergency meeting
					target != null &&
					target.Disconnected &&
					// Hosts are exempt from the anticheat detection
					!AmongUsClient.Instance.AmHost
				)
				{
					SkidMenu.notifications.Send("Protections Alert", $"Saved you from getting banned by {target.PlayerName}'s glitched body.");
					return false;
				}
				return true;
			}
		}
		*/

		[HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
		public static class HardenedReadPackedUInt
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(MessageReader __instance, ref uint __result)
			{
				if(!Enabled) return true;

				bool readMore = true;
				int shift = 0;
				uint output = 0;

				while(readMore)
				{
					if(__instance.BytesRemaining < 1) break;

					byte b = __instance.ReadByte();
					if(b >= 0x80)
					{
						readMore = true;
						b ^= 0x80;
					}
					else
					{
						readMore = false;
					}

					output |= (uint)(b << shift);
					shift += 7;
				}

				__result = output;
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		public static class OnPlayerPhysicsRpc
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls rpcId = (RpcCalls)callId;

				switch(rpcId)
				{
					case RpcCalls.ClimbLadder:
						byte ladderId = reader.ReadByte();

						if(BlockInvalidLadderOverload && (!ShipStatus.Instance || ladderId > ShipStatus.Instance.Ladders.Length - 1))
						{
							return false;
						}
						break;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}

		[HarmonyPatch(typeof(AmongUsClient), nameof(InnerNetClient.CoStartGame))]
		public static class BypassShapeshiftRatelimits
		{
			public static bool Enabled { get; set; } = true;

			static void Postfix()
			{
				if (!Enabled || !AmongUsClient.Instance.AmHost) return;
				try { GameOptionsManager.Instance.currentGameOptions.SetFloat(AmongUs.GameOptions.FloatOptionNames.ShapeshifterCooldown, 0.0f); } catch { }
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
		public static class MemoryAllocationOverload
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(byte callId, MessageReader reader)
			{
				if (!Enabled || callId != (byte)RpcCalls.VotingComplete) return true;
				int oldReadPosition = reader.Position;
				int arrayLength = reader.ReadPackedInt32();
				if (arrayLength > 1024 || arrayLength > reader.BytesRemaining) return false;
				reader.Position = oldReadPosition;
				return true;
			}
		}

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		public static class Votekicks
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(int srcClient, int clientId)
			{
				SkidMenu.Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");
				if(clientId != PlayerControl.LocalPlayer.OwnerId) return true;

				ClientData player = AmongUsClient.Instance.GetClient(srcClient);

				SkidMenu.notifications.Send("Votekick Logger", $"{player.PlayerName} has voted to kick you out.");

				// Prevent players from being able to votekick you as host
				return !(Enabled && AmongUsClient.Instance.AmHost);
			}
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
		public static class OnHandleMessage
		{
			static bool Prefix(MessageReader reader)
			{
				if (BlockLargeGameMessages && reader.Length > 1024 * 64) return false;
				if (BlockInvalidGameDataMessages && reader.Tag != 5 && reader.Tag != 6 && reader.Tag != 10 && reader.Tag != 12 && reader.Tag != 13) return true;
				return true;
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		public static class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;

				switch((RpcCalls)callId)
				{
					case RpcCalls.CloseDoorsOfType:
						if(BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
						break;

					case RpcCalls.UpdateSystem:
						SystemTypes system = (SystemTypes)reader.ReadByte();
						try
						{
							PlayerControl player = reader.ReadNetObject<PlayerControl>();
							if(ProtectAgainstNonHostKickExploit && system == SystemTypes.Ventilation && !AmongUsClient.Instance.AmHost)
							{
								if(player?.Data != null)
									SkidMenu.notifications.Send("Protections Alert", $"{player.Data.PlayerName} attempted the VentilationSystem kick exploit on you!");
								return false;
							}
						}
						catch { }
						if(BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
						break;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}
	}
}