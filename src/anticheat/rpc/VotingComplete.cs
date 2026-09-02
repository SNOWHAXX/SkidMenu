using Hazel;

namespace SkidMenu.anticheat.rpc
{
	internal class VotingComplete : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// The actual host-only enforcement for this RPC is handled by the dispatcher
			// via IsHostOnly(). This exists so the handler is registered and counted.
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.VotingComplete;
		}

		// Only the host is allowed to send the VotingComplete RPC
		public override bool IsHostOnly()
		{
			return true;
		}
	}
}
