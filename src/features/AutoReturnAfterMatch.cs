using HarmonyLib;
using UnityEngine;

namespace SkidMenu.features;

public sealed class AutoReturnAfterMatch : MonoBehaviour
{
    public AutoReturnAfterMatch(System.IntPtr ptr) : base(ptr) { }

    private static bool ShouldAutoReturn() =>
        SkidMenu.autoReturnAfterMatch ||
        (SkidMenu.autoHostEnabled && SkidMenu.autoHostReturnAfterMatch);

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
    public static class AutoReturn_ShowButtonsPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            if (!ShouldAutoReturn()) return;
            try { __instance.Navigation?.NextGame(); }
            catch { }
        }
    }
}
