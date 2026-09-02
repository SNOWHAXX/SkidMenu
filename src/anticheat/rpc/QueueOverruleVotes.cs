using Hazel;
using InnerNet;
using AmongUs.GameOptions;

namespace SkidMenu.anticheat.rpc
{
	// QueueOverruleVotes is the Judge's power to force-eject. Only a living Judge may use it;
	// anyone else who does is hijacking the role over the network. It is a client->host command,
	// so only the host is the authority for validating it. Mirrors GreaterAmongUs.
	internal class QueueOverruleVotes : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if (player == null || player.Data == null) return;
			if (AmongUsClient.Instance == null) return;

			// Only the host enforces this client->host command
			if (!AmongUsClient.Instance.AmHost) return;

			// The overrule power belongs to a living Judge alone
			if (player.Data.IsDead || player.Data.RoleType != RoleTypes.Judge)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} queued a vote overrule while not a living Judge.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.QueueOverruleVotes;
		}
	}
}
