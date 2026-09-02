using Hazel;
using InnerNet;

namespace SkidMenu.anticheat.rpc
{
	internal class AddVote : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			int source = reader.ReadInt32();
			int target = reader.ReadInt32();

			ClientData client = AmongUsClient.Instance != null ? AmongUsClient.Instance.FindClientById(source) : null;
			if(client == null || client.Character == null)
			{
				// An unknown client attempted to cast a votekick vote
				Anticheat.Flag($"{source} attempted to votekick {target} from an unknown client.");
				blockRpc = true;
				return;
			}

			PlayerControl sourcePlayer = client.Character;

			if(sourcePlayer.Data.IsDead)
			{
				Anticheat.Flag(sourcePlayer, $"{sourcePlayer.Data.PlayerName} attempted to votekick a player while dead.");
				blockRpc = true;
				return;
			}

			if(MeetingHud.Instance == null)
			{
				Anticheat.Flag(sourcePlayer, $"{sourcePlayer.Data.PlayerName} attempted to votekick a player outside of a meeting.");
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.AddVote;
		}

		public override System.Type GetExpectedNetObject()
		{
			return typeof(VoteBanSystem);
		}
	}
}
