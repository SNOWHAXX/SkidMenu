using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// CastVote is only valid from a living player during a meeting, one vote per player.
	internal class CastVote : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			if (player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} cast a vote while dead.");
				blockRpc = true;
				return;
			}

			if (MeetingHud.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} cast a vote outside of a meeting.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CastVote;
		}
	}
}
