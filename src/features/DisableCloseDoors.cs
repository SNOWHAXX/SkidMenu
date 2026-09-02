using HarmonyLib;
using Hazel;
using InnerNet;

namespace SkidMenu.features
{
	internal static class DisableCloseDoors
	{
		private static bool enabled = false;
		public static bool Enabled
		{
			get => enabled;
			set
			{
				if (enabled == value) return;
				if (value && !Sabotage.CanUnlockDoors())
					SkidMenu.notifications.Send("Disable Close Doors", "Disable Close Doors only works if you are the host of the lobby, or you are playing on Polus, Airship, or The Fungle.");
				enabled = value;
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		public static class CloseDoorsPatch
		{
			static bool Prefix()
			{
				return !Enabled;
			}
		}

		// This runs on clients whenever the host's DoorsSystemType gets deserialized, and only on non-host clients
		// So this only works if we are not the host of the lobby
		[HarmonyPatch(typeof(DoorsSystemType), nameof(DoorsSystemType.Deserialize))]
		public static class DoorsDeserializePatch
		{
			static void Prefix(MessageReader reader)
			{
				if (!Enabled || AmongUsClient.Instance.AmHost || ShipStatus.Instance == null || reader == null) return;

				int oldReadPosition = reader.Position;

				if (reader.BytesRemaining < 1) return;

				byte systems = reader.ReadByte();

				if (systems > ShipStatus.Instance.Systems.Count || systems > reader.BytesRemaining) return;

				// Systems are serialized as 1 byte for the system id, and 4 bytes for the dirty bits
				reader.Position += systems * (1 + 4);

				Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);

				for (byte i = 0; i < systems; i++)
				{
					if (batch.msgCount >= AmongUsClient.Instance.GetMaxMessagePackingLimit())
					{
						batch.FinishBatch();
						batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
					}

					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Doors, (byte)(i | 64));
				}

				batch.FinishBatch();

				reader.Position = oldReadPosition;
			}
		}
	}
}