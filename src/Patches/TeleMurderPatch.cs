using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class TeleMurderPatch
{
    private static bool _active = false;
    private static Vector2 _savedPos;

    public static void Intercept(Vector2 savedPos) { _savedPos = savedPos; _active = true; }
    public static void Cancel() { _active = false; }

    [HarmonyPatch(typeof(CustomNetworkTransform), "SnapTo", new[] { typeof(Vector2) })]
    static class PatchLocal
    {
        static bool Prefix(CustomNetworkTransform __instance, ref Vector2 position)
        {
            if (!_active) return true;
            if (__instance != PlayerControl.LocalPlayer?.NetTransform) return true;
            position = _savedPos;
            return true;
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.RpcSnapTo))]
    static class PatchRpc
    {
        static bool Prefix(CustomNetworkTransform __instance, ref Vector2 position)
        {
            if (!_active) return true;
            if (__instance != PlayerControl.LocalPlayer?.NetTransform) return true;
            position = _savedPos;
            return true;
        }
    }
}
