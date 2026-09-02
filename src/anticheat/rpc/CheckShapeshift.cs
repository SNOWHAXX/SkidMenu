using Hazel;
using InnerNet;
using AmongUs.GameOptions;

namespace SkidMenu.anticheat.rpc
{
	// CheckShapeshift is how a Shapeshifter executes the shift. Only a living impostor
	// Shapeshifter role may do it, and never while mid-animation without a valid conceal.
	internal class CheckShapeshift : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			bool flag = reader.ReadBoolean();

			// Only a living, impostor-team Shapeshifter may initiate a shift
			if (player.Data.IsDead
				|| player.Data.RoleType != RoleTypes.Shapeshifter
				|| !RoleManager.IsImpostorRole(player.Data.RoleType)
				|| player.inMovingPlat
				|| player.shapeshifting
				|| player.onLadder)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid shapeshift RPC.");
				blockRpc = true;
				return;
			}

			// Shapeshifting without animation outside of a vent/meeting is a cheat tell
			if (!player.inVent && !Utils.isMeeting && !Utils.isExiling && !flag)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} shapeshifted without animation outside a vent.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckShapeshift;
		}
	}
}
