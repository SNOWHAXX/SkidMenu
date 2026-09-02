using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// Only the host closes the meeting. A non-host sending CloseMeeting hijacks the game.
	// Handled by the host-only dispatch in Anticheat.HandleRpc; host-only.
	internal class CloseMeeting : RpcCheck
	{
		public override bool IsHostOnly() => true;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CloseMeeting;
		}
	}
}
