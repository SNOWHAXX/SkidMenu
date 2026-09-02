using HarmonyLib;
using Hazel;
using InnerNet;

namespace SkidMenu.features
{
	internal static class DisableCameras
	{
		private static bool enabled = false;
		public static bool Enabled
		{
			get => enabled;
			set
			{
				enabled = value;
				if (value) Refresh();
			}
		}

		// It is not possible to watch security cameras when the comms sabotage is active. We can abuse this to disable security cameras
		// When a player starts to watch security cameras, sabotage comms for that player, when the player stops watching cameras, fix comms sabotage for that player
		// The host and normal clients alike process SecurityCameraSystemType::UpdateSystem locally, so a single patch on it covers both cases
		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
		public static class CamerasSystemPatch
		{
			static void Postfix(PlayerControl player, MessageReader msgReader)
			{
				if (!Enabled) return;

				// If we update Comms for the host, then everybody will be affected by Comms
				if (player == null || player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer) return;
				if (ShipStatus.Instance == null || !ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Security)) return;

				int oldReadPosition = msgReader.Position;

				try
				{
					msgReader.Position--;
					// 1 = Player started to watch cameras, 2 (and every other value) = Player stopped watching cameras
					byte operation = msgReader.ReadByte();
					msgReader.Position++;

					if (operation == 1)
					{
						EnableCommsFor(player);
					}
					else
					{
						// Prevent an exploit where if the comms sabotage is active, someone could enter and leave the security cameras to remove the comms effect from themselves
						if (Sabotage.IsSabotageActive(SystemTypes.Comms))
						{
							// There is an edge case where if someone is on the security cameras panel when comms are actively sabotaged, and the sabotage is fixed,
							// then the player will be able to watch the security cameras
							// I don't think it is worthwhile to fix this edge case considering this feature is unlikely to even be used by anyone
							return;
						}

						DisableCommsFor(player);
					}
				}
				finally
				{
					msgReader.Position = oldReadPosition;
				}
			}
		}

		private static void Refresh()
		{
			if (ShipStatus.Instance == null || !ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Security)) return;

			SecurityCameraSystemType securitySystem = ShipStatus.Instance.Systems[SystemTypes.Security].Cast<SecurityCameraSystemType>();
			if (securitySystem == null) return;

			foreach (PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if (player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer || !securitySystem.PlayersUsing.Contains(player.PlayerId)) continue;

				EnableCommsFor(player);
			}
		}

		private static void EnableCommsFor(PlayerControl player)
		{
			SkidMenu.Log.LogMessage($"{player.Data.PlayerName} started to watch cameras, sending Comms system update");

			if (!AmongUsClient.Instance.AmHost)
			{
				Sabotage.SabotageSystem(SystemTypes.Comms, player.OwnerId);
				return;
			}

			Network.BatchedMessage batch = new Network.BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
			systemUpdate.Write(1);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			systemUpdate.Recycle();
			batch.FinishBatch();
		}

		private static void DisableCommsFor(PlayerControl player)
		{
			SkidMenu.Log.LogMessage($"{player.Data.PlayerName} stopped watching cameras, sending Comms system update");

			if (!AmongUsClient.Instance.AmHost)
			{
				Sabotage.FixSabotage(SystemTypes.Comms, player.OwnerId);
				return;
			}

			Network.BatchedMessage batch = new Network.BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = Comms sabotage is active, 0 = Comms sabotage is inactive
			systemUpdate.Write(0);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			systemUpdate.Recycle();
			batch.FinishBatch();
		}
	}
}