using Hazel;
using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu.anticheat.rpc
{
	internal class CompleteTask : RpcCheck
	{
		private class TaskTrack
		{
			public uint lastId;
			public float lastTime;
			public bool hasLast;
		}

		private static readonly Dictionary<byte, TaskTrack> _track = new Dictionary<byte, TaskTrack>();

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			uint taskIndex = reader.ReadPackedUInt32();

			// If there is no instance of ShipStatus (such as if the game has not started yet or the map was despawned), then it is not possible to complete tasks.
			if (ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when there was no valid instance of ShipStatus.");
				blockRpc = true;
			}

			if (RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} while being an impostor.");
				blockRpc = true;
			}

			// Task IDs are zero-indexed
			if (taskIndex + 1 > player.Data.Tasks.Count)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when they only have {player.Data.Tasks.Count} tasks.");
				blockRpc = true;
			}

			// Rate + repeat detection (mirrors GreaterAmongUs/BetterAmongUs): completing the same
			// task twice in a row, or two different tasks less than 1.25s apart, is a clear tell.
			if (_track.TryGetValue(player.PlayerId, out var t))
			{
				if (t.hasLast)
				{
					if (t.lastId == taskIndex)
					{
						Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} again back to back.");
						blockRpc = true;
					}
					else if (Time.unscaledTime - t.lastTime < 1.25f)
					{
						Anticheat.Flag(player, $"{player.Data.PlayerName} completed tasks too fast ({(Time.unscaledTime - t.lastTime):F2}s).");
						blockRpc = true;
					}
				}

				t.lastId = taskIndex;
				t.lastTime = Time.unscaledTime;
			}
			else
			{
				_track[player.PlayerId] = new TaskTrack { lastId = taskIndex, lastTime = Time.unscaledTime, hasLast = true };
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CompleteTask;
		}
	}
}
