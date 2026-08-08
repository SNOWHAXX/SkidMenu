using HarmonyLib;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SkidMenu.features;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
static class AutoHostLobbyOpenedPatch
{
    static void Postfix() => AutoHostService.OnLobbyOpened();
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
static class AutoHostTickPatch
{
    static void Postfix(GameStartManager __instance)
    {
        AutoHostService.Tick(__instance);
        features.DummySpawner.Tick();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
static class AutoHostGameEndedPatch
{
    static void Postfix() => AutoHostService.OnGameEnded();
}
