using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features
{
	internal class PlayerLogger
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class OnJoin
		{
			static void Postfix(PlayerControl __instance)
			{
				if (__instance.PlayerId == PlayerControl.LocalPlayer?.PlayerId || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if (clientData == null) return;
				if (__instance.Data == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;
				string platform = Utils.PlatformTypeToString(platformData?.Platform ?? Platforms.Unknown);
				uint level = __instance.Data.PlayerLevel + 1;
				int colorId = __instance.Data.DefaultOutfit?.ColorId ?? 0;
				string nameHex = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[colorId]);

				SkidMenu.Log.LogMessage($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {platformData.Platform}. friendcode {clientData.FriendCode}, puid {clientData.ProductUserId}");

				if (!SkidMenu.logPlayerJoin.Value) return;

				string msg = $"<color=#{nameHex}>{clientData.PlayerName}</color> <color=#44ff44>joined</color>  <color=#fb0>Lv:{level}</color> <color=#555>|</color> <color=#88ddff>{platform}</color>";
				ConsoleUI.Log(msg, "44ff44");
			}
		}
	}
}