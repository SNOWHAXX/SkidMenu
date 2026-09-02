using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	// Only the host extends the lobby timer. A non-host sending ExtendLobbyTimer is a cheat.
	// Handled by the host-only dispatch in Anticheat.HandleRpc; host-only.
	internal class ExtendLobbyTimer : RpcCheck
	{
		public override bool IsHostOnly() => true;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ExtendLobbyTimer;
		}
	}
}
