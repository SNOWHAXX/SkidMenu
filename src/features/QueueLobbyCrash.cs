using HarmonyLib;
using InnerNet;

namespace SkidMenu.features
{
	internal static class QueueLobbyCrash
	{
		public static bool Enabled { get; set; } = false;

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
		public static class OnGameStart
		{
			static void Postfix()
			{
				if (!Enabled || PlayerControl.LocalPlayer == null) return;

				PlayerControl.LocalPlayer.CmdReportDeadBody(null);

				SkidMenu.notifications.Send("Queue Lobby Crash", "The next game will crash...");
			}
		}
	}
}