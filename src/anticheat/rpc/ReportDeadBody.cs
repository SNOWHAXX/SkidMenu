using Hazel;

namespace SkidMenu.anticheat.rpc
{
	internal class ReportDeadBody : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// Reporting only makes sense mid-game with roles assigned. Block it in the lobby / pre-game.
			if (!Utils.isInGame || Utils.isLobby)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call a meeting when the game is not in play.");
				blockRpc = true;
				return;
			}

			// You cannot report while in a meeting, venting, shapeshifting, on a moving platform or on a ladder.
			if (Utils.isMeeting || GameManager.Instance.IsHideAndSeek() || player.inVent || player.shapeshifting
				|| player.inMovingPlat || player.onLadder)
			{
				string incapable = GetIncapableReason(player);
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to report while {incapable}.");
				blockRpc = true;
				return;
			}

			// If a playerId was written, this is a body report: the target must be dead and must not be the sender.
			if (reader.BytesRemaining > 0)
			{
				byte targetId = reader.ReadByte();
				var targetInfo = GameData.Instance != null ? GameData.Instance.GetPlayerById(targetId) : null;
				if (targetInfo != null && !targetInfo.IsDead)
				{
					Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to report {targetInfo.PlayerName} who is not dead.");
					blockRpc = true;
					return;
				}

				if (targetId == player.PlayerId)
				{
					Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to report their own body.");
					blockRpc = true;
					return;
				}
			}
			// Otherwise this is an emergency meeting call, which costs an emergency.
			else if (player.RemainingEmergencies <= 0)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call an emergency meeting with no meetings remaining.");
				blockRpc = true;
				return;
			}
		}

		private static string GetIncapableReason(PlayerControl player)
		{
			if (Utils.isMeeting) return "a meeting is in progress";
			if (GameManager.Instance != null && GameManager.Instance.IsHideAndSeek()) return "in Hide and Seek";
			if (player.inVent) return "in a vent";
			if (player.shapeshifting) return "shapeshifting";
			if (player.inMovingPlat) return "on a moving platform";
			if (player.onLadder) return "on a ladder";
			return "in an invalid state";
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.ReportDeadBody;
		}
	}
}
