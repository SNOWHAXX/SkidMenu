using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// Only the host ever assigns roles mid-game. A non-host sending SetRole is a cheat.
	// Handled by the host-only dispatch in Anticheat.HandleRpc (fires when we are host and
	// the sender is not). Runs only on the host; inert as a non-host client.
	internal class SetRole : RpcCheck
	{
		public override bool IsHostOnly() => true;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// No-op: the host-only dispatch block in HandleRpc performs the actual flag/block.
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetRole;
		}
	}
}
