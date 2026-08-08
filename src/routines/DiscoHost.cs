using UnityEngine;

namespace SkidMenu.routines
{
	public class DiscoHostRoutine : IRoutine
	{
		public DiscoHostRoutine()
		{
			RoutineName = "DiscoHost";
		}

		public float randomizationDelay = 0.15f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || !AmongUsClient.Instance.AmHost)
			{
				Enabled = false;
                SkidMenu.notifications.Send("Disco", "Disco mode has been disabled as you are not the host or you left the lobby.", 5);

				return;
			}

			timeElapsed += Time.deltaTime;
			if(timeElapsed < randomizationDelay) return;

			System.Random rnd = new System.Random();
			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				player.RpcSetColor((byte)rnd.Next(0, 18));
			}

			timeElapsed = 0f;
		}
	}
}