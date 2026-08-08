using HarmonyLib;
using System;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
	public static void Postfix(HudManager __instance)
	{
		__instance.MapButton.OnClick.RemoveAllListeners();

		__instance.MapButton.OnClick.AddListener((Action) (() =>
        {
			__instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Normal
			});
		}));
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
	public static void Postfix(HudManager __instance)
    {
		__instance.ShadowQuad.gameObject.SetActive(!MalumESP.IsFullbrightActive());

		if (Utils.IsChatUiActive())
		{
			__instance.Chat.gameObject.SetActive(true);
		}
		else
		{
			Utils.CloseChat();
			__instance.Chat.gameObject.SetActive(false);
		}

		MalumCheats.UseVentCheat(__instance);
		MalumESP.ZoomOut(__instance);
		MalumESP.FreecamCheat();

		VotekickHandler.CheckForNewPlayers();
		if (VotekickHandler.VotekickAllEnabled)
			VotekickHandler.VotekickAll();
		VotekickHandler.TickConditionKicks();

		if (PlayerPickMenu.playerpickMenu != null && CheatToggles.ShouldPPMClose())
		{
            PlayerPickMenu.playerpickMenu.Close();
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
static class HudManager_Start_SkipIntro
{
    static void Postfix() { }
}
