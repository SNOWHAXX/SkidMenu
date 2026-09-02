using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections;
using UnityEngine;

namespace SkidMenu.features
{
	internal class Troll
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		public static class AutoReportBodies
		{
			public static bool Enabled { get; set; } = false;
			public static float ReportDelay { get; set; } = 0f;
			public static bool CrewOnly { get; set; } = false;

			private static readonly RoleTypes[] CrewRoles =
			{
				RoleTypes.Crewmate,
				RoleTypes.Noisemaker,
				RoleTypes.Detective,
				RoleTypes.Tracker,
				RoleTypes.Scientist,
				RoleTypes.Engineer,
				RoleTypes.Judge
			};

			static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				if(!Enabled || PlayerControl.LocalPlayer.Data.IsDead) return;
				if(!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;
				if(CrewOnly && System.Array.IndexOf(CrewRoles, PlayerControl.LocalPlayer.Data.RoleType) < 0) return;
				bool viperKill = __instance?.Data?.RoleType == RoleTypes.Viper;
				SkidMenu.notifications.Send("Auto Report Bodies", $"{target.Data.PlayerName} was just killed by {__instance.Data.PlayerName} {__instance.Data.ColorName}, their body has been automatically reported.");
				AmongUsClient.Instance.StartCoroutine(DelayedReport(target, viperKill).WrapToIl2Cpp());
			}

			private static IEnumerator DelayedReport(PlayerControl target, bool viperKill)
			{
				if(ReportDelay > 0f) yield return new WaitForSeconds(ReportDelay);
				if(target == null || target.Data == null) yield break;
				if(viperKill && !ViperBodies.CanReport(target.PlayerId)) yield break;
				PendingReportTeleport.Position = target.transform.position;
				PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
		public static class BlockVenting
		{
			public static bool Enabled { get; set; } = false;

			static void Postfix(VentilationSystem __instance)
			{
				if(!Enabled) return;
				if(__instance.PlayersInsideVents.Count >= PlayerControl.AllPlayerControls.Count) return;
				foreach(byte ventId in __instance.PlayersInsideVents.Values)
				{
					if(ventId >= ShipStatus.Instance.AllVents.Count) continue;
					VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, ventId);
				}
			}
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
		public static class BlockSabotages
		{
			private static bool enabled = false;
			public static bool Enabled
			{
				get { return enabled; }
				set
				{
					if(enabled == value) return;
					if(value && AmongUsClient.Instance.AmHost)
					{
						SkidMenu.notifications.Send("Block Sabotages", "This option should be used when you are not the host of the lobby. Use Disable Sabotages in the Host section instead.");
						Host.DisableSabotages.Enabled = true;
						return;
					}
					enabled = value;
				}
			}

			static void Postfix(SabotageSystemType __instance)
			{
				if(!Enabled || __instance.Timer > 0.1f) return;
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, 255);
			}
		}

		public static void ScanForAll(bool scanning)
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				batch.QueueSetScanner(player, scanning, ++player.scannerCount);
			batch.FinishBatch();
		}

		public static void RandomizeColors()
		{
			System.Random rnd = new System.Random();
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				batch.QueueSetColor(player, (byte)rnd.Next(0, 17));
			batch.FinishBatch();
		}

		public static void ShapeshiftAll()
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				batch.QueueShapeshift(player, player, true);
			batch.FinishBatch();
		}

		public static void TPTo(PlayerControl target)
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				ushort seqId = (ushort)(player.NetTransform.lastSequenceId + 2);
				batch.QueueSnapTo(player, seqId, target.transform.position);
			}
			batch.FinishBatch();
		}

		public static void FunnyLobbyTimer()
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			batch.QueueLobbyTimeExpiring(69420);
			batch.FinishBatch();
		}

		public static void CrazyLevels()
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				batch.QueueSetLevel(player, uint.MaxValue - 1);
			batch.FinishBatch();
		}

		public static void TurnAllTo(PlayerControl target)
		{
			NetworkedPlayerInfo.PlayerOutfit outfit = target.Data.DefaultOutfit;
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				batch.QueueSetName(player, outfit.PlayerName);
				batch.QueueSetColor(player, (byte)outfit.ColorId);
				batch.QueueSetLevel(player, 465);
				batch.QueueSetHatStr(player, outfit.HatId, (byte)(player.Data.DefaultOutfit.HatSequenceId + 2));
			}
			batch.FinishBatch();

			Network.BatchedMessage batch2 = new Network.BatchedMessage();
			batch2.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				batch2.QueueSetNameplate(player, outfit.NamePlateId, (byte)(player.Data.DefaultOutfit.NamePlateSequenceId + 2));
				batch2.QueueSetSkinStr(player, outfit.SkinId, (byte)(player.Data.DefaultOutfit.SkinSequenceId + 2));
				batch2.QueueSetVisorStr(player, outfit.VisorId, (byte)(player.Data.DefaultOutfit.VisorSequenceId + 2));
				batch2.QueueSetPetStr(player, outfit.PetId, (byte)(player.Data.DefaultOutfit.PetSequenceId + 2));
			}
			batch2.FinishBatch();
		}

		public static IEnumerator KillAllHost()
		{
			PlayerControl host = AmongUsClient.Instance.GetHost().Character;
			PlayerControl[] players = PlayerControl.AllPlayerControls.ToArray();
			int numOfImps = 0;
			foreach(PlayerControl player in players)
				if(RoleManager.IsImpostorRole(player.Data.RoleType)) numOfImps++;
			foreach(PlayerControl player in players)
			{
				if(!RoleManager.IsImpostorRole(player.Data.RoleType)) continue;
				numOfImps--;
				if(numOfImps == 0) break;
				Network.BatchedMessage batch = new Network.BatchedMessage();
				batch.UseAnticheatBypass();
				batch.QueueMurderPlayer(host, player, MurderResultFlags.Succeeded);
				batch.FinishBatch();
			}
			foreach(PlayerControl player in players)
			{
				if(player == host || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;
				Network.BatchedMessage batch = new Network.BatchedMessage();
				batch.UseAnticheatBypass();
				batch.QueueMurderPlayer(host, player, MurderResultFlags.Succeeded);
				batch.FinishBatch();
				yield return Effects.Wait(0.3f);
			}
			yield break;
		}

		public static void SceneChange(string scene)
		{
			Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
			batch.UseAnticheatBypass();
			batch.QueueSceneChange(AmongUsClient.Instance.HostId, scene);
			batch.FinishBatch();
		}

		public static void MakeAllVote(PlayerControl player, bool crazyVotes = false)
		{
			int voteCount = MeetingHud.Instance.playerStates.Length;
			if(crazyVotes) voteCount *= 3;
			MeetingHud.VoterState[] array = new MeetingHud.VoterState[voteCount];
			for(int i = 0; i < array.Length; i++)
			{
				array[i].VoterId = (byte)(PlayerControl.AllPlayerControls.Count % 15);
				array[i].VotedForId = player.PlayerId;
			}
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			batch.QueueVotingComplete(array, player.Data, false);
			batch.FinishBatch();
		}

		public static void ChangeAllRole(RoleTypes role, bool excludeHost)
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				RoleTypes r = (excludeHost && player.OwnerId == AmongUsClient.Instance.HostId) ? RoleTypes.Crewmate : role;
				batch.QueueSetRole(player, r, false);
			}
			batch.FinishBatch();
		}

		public static System.Collections.Generic.Dictionary<PlayerControl, ushort> VentSeqIds = new System.Collections.Generic.Dictionary<PlayerControl, ushort>();

		public static void TeleportToVent(PlayerControl player, int ventId)
		{
			if (ShipStatus.Instance == null)
			{
				SkidMenu.notifications.Send("Vent TP", "The game must have started for this to work.");
				return;
			}

			if (AmongUsClient.Instance.AmHost)
			{
				player.MyPhysics.RpcBootFromVent(ventId);
				return;
			}

			if (!VentSeqIds.ContainsKey(player))
				VentSeqIds[player] = 10000;

			MessageWriter enterVent = MessageWriter.Get(SendOption.None);
			enterVent.Write(++VentSeqIds[player]);
			enterVent.Write((byte)VentilationSystem.Operation.Enter);
			enterVent.Write((byte)ventId);

			MessageWriter bootFromVent = MessageWriter.Get(SendOption.None);
			bootFromVent.Write(++VentSeqIds[player]);
			bootFromVent.Write((byte)VentilationSystem.Operation.BootImpostors);
			bootFromVent.Write((byte)ventId);

			Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, enterVent);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, bootFromVent);
			batch.FinishBatch();

			enterVent.Recycle();
			bootFromVent.Recycle();
		}

		public static void Despawn(InnerNetObject netObject)
		{
			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			batch.QueueDespawn(netObject.NetId);
			batch.FinishBatch();
		}

		public static void NukeLobby()
		{
			int index = GameManager.Instance.IsHideAndSeek() ? 5 : 4;
			IGameOptions options = GameManager.Instance.LogicOptions.currentGameOptions;
			options.SetInt(Int32OptionNames.DiscussionTime, 99999);
			options.SetByte(ByteOptionNames.MapId, 8);
			MessageWriter writer = MessageWriter.Get(SendOption.None);
			writer.StartMessage((byte)index);
			writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
			writer.EndMessage();

			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			batch.QueueLobbyTimeExpiring(69420);
			byte i = 0;
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				i++;
				Vector2 pos = new Vector2(2 * i, 2 * i);
				batch.QueueSnapTo(player, 32767, pos);
				batch.QueueSetScanner(player, true, 255);
				batch.QueueShapeshift(player, player, true);
			}
			batch.QueueDataFlag(GameManager.Instance.NetId, writer);
			batch.QueueSceneChange(AmongUsClient.Instance.ClientId, "Tutorial");
			batch.QueueSetStartCounter(PlayerControl.LocalPlayer, -69, ++PlayerControl.LocalPlayer.LastStartCounter);
			batch.FinishBatch();
		}

		public static void DisableNewLobbyCreation()
		{
			int index = GameManager.Instance.IsHideAndSeek() ? 5 : 4;
			IGameOptions options = GameManager.Instance.LogicOptions.currentGameOptions;
			options.SetByte(ByteOptionNames.MapId, 8);
			MessageWriter writer = MessageWriter.Get(SendOption.None);
			writer.StartMessage((byte)index);
			writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
			writer.EndMessage();
			Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
			batch.UseAnticheatBypass();
			batch.QueueDataFlag(GameManager.Instance.NetId, writer);
			batch.FinishBatch();
		}

		// Sends a quick-chat RPC (33) with a fixed category payload so it renders as a
		// coloured text/emote in chat. Matches ChocooMenu byte-for-byte: the payload is
		// written TWICE - once broadcast to the server (-1) and once targeted at our own
		// client id so the emote actually renders in our own chat.
		public static void SendColouredChat(int id)
		{
			try
			{
				if (PlayerControl.LocalPlayer == null) return;

				MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
					PlayerControl.LocalPlayer.NetId,
					(byte)33,
					SendOption.Reliable,
					-1);
				writer.Write((byte)3);
				writer.Write((ushort)78);
				writer.Write((byte)1);
				writer.Write((byte)2);
				writer.Write((ushort)id);
				AmongUsClient.Instance.FinishRpcImmediately(writer);

				MessageWriter self = AmongUsClient.Instance.StartRpcImmediately(
					PlayerControl.LocalPlayer.NetId,
					(byte)33,
					SendOption.Reliable,
					PlayerControl.LocalPlayer.OwnerId);
				self.Write((byte)3);
				self.Write((ushort)78);
				self.Write((byte)1);
				self.Write((byte)2);
				self.Write((ushort)id);
				AmongUsClient.Instance.FinishRpcImmediately(self);
			}
			catch (System.Exception ex)
			{
				SkidMenu.Log.LogError("Troll.SendColouredChat: " + ex.Message);
			}
		}

	}
}

