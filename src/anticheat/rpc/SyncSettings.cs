using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// Only the host synchronizes the game settings. A non-host sending SyncSettings is a cheat.
	// Handled by the host-only dispatch in Anticheat.HandleRpc; host-only.
	internal class SyncSettings : RpcCheck
	{
		public override bool IsHostOnly() => true;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SyncSettings;
		}
	}
}
