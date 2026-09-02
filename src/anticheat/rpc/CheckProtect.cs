using Hazel;
using InnerNet;
using AmongUs.GameOptions;
using UnityEngine;

namespace SkidMenu.anticheat.rpc
{
	// CheckProtect is how a dead Guardian Angel shields a living crewmate. Only a dead,
	// non-impostor Guardian Angel in range may do it.
	internal class CheckProtect : RpcCheck
	{
		private const float MaxProtectDistance = 3f;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			PlayerControl target = reader.ReadNetObject<PlayerControl>();
			if (target == null || target.Data == null) return;

			// The Guardian Angel is dead when it can protect
			if (player.Data.RoleType != RoleTypes.GuardianAngel
				|| !player.Data.IsDead
				|| RoleManager.IsImpostorRole(player.Data.RoleType)
				|| target.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid protect RPC.");
				blockRpc = true;
				return;
			}

			// Guardian Angel must be physically near the protected player
			float dist = Vector2.Distance(player.GetTruePosition(), target.GetTruePosition());
			if (dist > MaxProtectDistance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried to protect {target.Data.PlayerName} from {dist:F1} units away.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckProtect;
		}
	}
}
