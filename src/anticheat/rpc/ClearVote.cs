using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// ClearVote is only valid during a meeting. A player clearing a vote outside one is a cheat.
	internal class ClearVote : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

			if (MeetingHud.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} cleared a vote outside of a meeting.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ClearVote;
		}
	}
}
