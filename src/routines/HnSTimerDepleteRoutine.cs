using UnityEngine;

namespace SkidMenu.routines
{
	public class HnSTimerDepleteRoutine : IRoutine
	{
		public HnSTimerDepleteRoutine()
		{
			RoutineName = "HnSTimerDeplete";
		}

		private float _spamTimer = 0f;
		private float _cycleTimer = 0f;
		private float _pauseTimer = 0f;
		private bool _isPaused = false;

		private const float SPAM_RATE = 0.25f;
		private const float ACTIVE_DURATION = 2.0f;
		private const float PAUSE_DURATION = 1.0f;

		public override void Run()
		{
			if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.myTasks == null)
			{
				Enabled = false;
				SkidMenu.notifications.Send("HnS Timer", "Deplete HnS timer was disabled as you left the game.", 10);
				return;
			}

			float dt = Time.deltaTime;

			if (_isPaused)
			{
				_pauseTimer += dt;
				if (_pauseTimer >= PAUSE_DURATION)
				{
					_isPaused = false;
					_pauseTimer = 0f;
					_cycleTimer = 0f;
				}
				return;
			}

			_cycleTimer += dt;
			if (_cycleTimer >= ACTIVE_DURATION)
			{
				_isPaused = true;
				_pauseTimer = 0f;
				return;
			}

			_spamTimer += dt;
			if (_spamTimer >= SPAM_RATE)
			{
				_spamTimer = 0f;
				var tasks = PlayerControl.LocalPlayer.myTasks;
				for (int i = 0; i < tasks.Count; i++)
				{
					try { PlayerControl.LocalPlayer.RpcCompleteTask(tasks[i].Id); }
					catch { }
				}
			}
		}
	}
}
