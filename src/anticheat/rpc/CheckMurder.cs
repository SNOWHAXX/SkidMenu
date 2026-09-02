using Hazel;
using InnerNet;
using UnityEngine;

namespace SkidMenu.anticheat.rpc
{
	internal class CheckMurder : RpcCheck
	{
		private const float MaxKillDistance = 3f;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			PlayerControl target = reader.ReadNetObject<PlayerControl>();

			if (target == null || target.Data == null) return;

			// The CheckMurder RPC is the kill-execution check. The sender must be a living impostor.
			if (player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent a kill check while dead.");
				blockRpc = true;
				return;
			}

			if (!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent a kill check while not an impostor.");
				blockRpc = true;
				return;
			}

			// The target must be a living crewmate
			if (target.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to kill a dead player ({target.Data.PlayerName}).");
				blockRpc = true;
				return;
			}

			if (RoleManager.IsImpostorRole(target.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to kill fellow impostor {target.Data.PlayerName}.");
				blockRpc = true;
				return;
			}

			// Telekill detection: the killer must be physically within kill range of the target
			float dist = Vector2.Distance(player.GetTruePosition(), target.GetTruePosition());
			if (dist > MaxKillDistance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to kill {target.Data.PlayerName} from {dist:F1} units away (telekill).");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckMurder;
		}
	}
}
