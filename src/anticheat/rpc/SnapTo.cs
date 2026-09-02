using Hazel;

namespace SkidMenu.anticheat.rpc
{
	internal class SnapTo : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(player == null || player.Data == null) return;

			// Nobody should be able to teleport while still in the lobby
			if(LobbyBehaviour.Instance != null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.", false);

				// We are not able to send SnapTo RPCs with other players' NetTransform net ids on vanilla servers
				// Snap them back to where they actually are so the lobby teleport does not stick
				if(AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost && !Constants.IsVersionModded())
					player.NetTransform.RpcSnapTo(player.transform.position);

				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SnapTo;
		}

		public override System.Type GetExpectedNetObject()
		{
			return typeof(CustomNetworkTransform);
		}
	}
}
