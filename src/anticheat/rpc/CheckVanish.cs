using Hazel;
using InnerNet;
using AmongUs.GameOptions;

namespace SkidMenu.anticheat.rpc
{
	// CheckVanish is how a Phantom executes its vanish. Only a living impostor Phantom
	// role may do it, and never while in a vent or on a ladder.
	internal class CheckVanish : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			if (player.Data.IsDead
				|| player.Data.RoleType != RoleTypes.Phantom
				|| !RoleManager.IsImpostorRole(player.Data.RoleType)
				|| player.inVent
				|| player.inMovingPlat
				|| player.onLadder)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid vanish RPC.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckVanish;
		}
	}
}
