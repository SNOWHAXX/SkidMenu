using UnityEngine;
using AmongUs.GameOptions;

namespace SkidMenu.routines
{
	public class GlitterBombRoutine : IRoutine
	{
		public GlitterBombRoutine()
		{
			RoutineName = "GlitterBomb";
		}

		public readonly float PHANTOM_DELAY = 0.05f;

		private float timeElapsed = 0f;

		public override void Run()
		{
			if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || PlayerControl.LocalPlayer.Data.IsDead)
			{
				Enabled = false;

				if (PlayerControl.LocalPlayer == null)
					SkidMenu.notifications.Send("GlitterBomb", "GlitterBomb was disabled as you left the game.");
				else
					SkidMenu.notifications.Send("GlitterBomb", "GlitterBomb was disabled as you died.");

				return;
			}

			if (PlayerControl.LocalPlayer.Data.RoleType != RoleTypes.Phantom)
			{
				Enabled = false;

				SkidMenu.notifications.Send("GlitterBomb", "GlitterBomb was disabled as you are not the Phantom.");

				return;
			}

			timeElapsed += Time.deltaTime;
			if (timeElapsed < PHANTOM_DELAY) return;
			timeElapsed = 0f;

			PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetFreeColor());

			Network.BatchedMessage batch = new Network.BatchedMessage();
			batch.UseAnticheatBypass();
			batch.QueueAppear(PlayerControl.LocalPlayer);
			batch.QueueVanish(PlayerControl.LocalPlayer);
			batch.FinishBatch();
		}
	}
}