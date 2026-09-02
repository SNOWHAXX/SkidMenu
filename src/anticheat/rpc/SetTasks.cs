using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// Only the host ever assigns tasks. A non-host sending SetTasks is a cheat.
	// Handled by the host-only dispatch in Anticheat.HandleRpc; host-only.
	internal class SetTasks : RpcCheck
	{
		public override bool IsHostOnly() => true;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetTasks;
		}
	}
}
