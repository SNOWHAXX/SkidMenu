using Hazel;
using InnerNet;
using System.Collections.Generic;

namespace SkidMenu.anticheat.rpc
{
	internal class MurderPlayer : RpcCheck
	{
		// Track how many times a single killer attempts to kill the same already-dead target.
		// Rapidly murdering a dead target is the hallmark of a kill/ban exploit.
		private static readonly Dictionary<byte, byte> killAttempts = new Dictionary<byte, byte>();

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			if(player == null || player.Data == null) return;
			if(GameManager.Instance == null || AmongUsClient.Instance == null) return;

			PlayerControl target = reader.ReadNetObject<PlayerControl>();

			// A dead player cannot send the MurderPlayer RPC
			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to kill while dead.");
				blockRpc = true;
				return;
			}

			// Only impostor-team roles are able to murder in normal gameplay
			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} ({player.Data.RoleType}) attempted to send a murder RPC while not an impostor.");
				blockRpc = true;
				return;
			}

			if(target == null || target.Data == null) return;

			// Killing a fellow impostor is always invalid in normal play
			if(RoleManager.IsImpostorRole(target.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to kill fellow impostor {target.Data.PlayerName}.");
				blockRpc = true;
				return;
			}

			// Repeatedly trying to kill someone who is already dead looks like a ban/kill exploit
			if(target.Data.IsDead)
			{
				if(!killAttempts.TryGetValue(player.PlayerId, out byte count))
					count = 0;

				if(++count >= 5)
				{
					Anticheat.Flag(player, $"{player.Data.PlayerName} repeatedly attempted to kill a dead player ({target.Data.PlayerName}) - possible kill/ban exploit.");
					blockRpc = true;
					killAttempts.Remove(player.PlayerId);
					return;
				}

				killAttempts[player.PlayerId] = count;
			}
			else if(killAttempts.ContainsKey(player.PlayerId))
			{
				// The killer has since targeted a living player, so reset their attempt counter
				killAttempts.Remove(player.PlayerId);
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.MurderPlayer;
		}
	}
}
