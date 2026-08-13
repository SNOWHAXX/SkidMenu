using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(ChatController), "Awake")]
static class ChatHistoryPatch
{
    static void Postfix(ChatController __instance) => ApplyHistorySize(__instance);

    internal static void ApplyHistorySize(ChatController ctrl)
    {
        if (ctrl == null) return;
        ctrl.chatBubblePool.poolSize = SkidMenu.chatHistoryInfinite ? 9999 : SkidMenu.chatHistorySize;
    }
}
