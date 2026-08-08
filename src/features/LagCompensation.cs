using HarmonyLib;
using UnityEngine;

namespace SkidMenu.features;

public static class LagCompensation
{
    public static bool Enabled        = false;
    public static bool FreezePosition = false;
    public static bool Jitter         = false;
    public static int  SkipTicks      = 5;
    public static float JitterMin     = 2f;
    public static float JitterMax     = 4f;

    private static int   _tickCounter  = 0;
    private static int   _jitterCount  = 0;
    private static bool  _jitterSend   = false;

    public static void Reset()
    {
        _tickCounter = 0;
        _jitterCount = 0;
        _jitterSend  = false;
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public static class LagCompensation_Patch
    {
        public static bool Prefix(CustomNetworkTransform __instance)
        {
            if (!Enabled) return true;
            if (__instance.myPlayer == null || __instance.myPlayer != PlayerControl.LocalPlayer) return true;

            if (FreezePosition) return false;

            if (Jitter)
            {
                if (_jitterCount <= 0)
                {
                    _jitterSend  = !_jitterSend;
                    int range    = Mathf.Max(1, Mathf.RoundToInt(Random.Range(JitterMin, JitterMax)));
                    _jitterCount = range;
                }
                _jitterCount--;
                if (!_jitterSend)
                    __instance.lastPosSent = __instance.myPlayer.GetTruePosition();
                return _jitterSend;
            }

            _tickCounter++;
            if (_tickCounter < SkipTicks)
            {
                __instance.lastPosSent = __instance.myPlayer.GetTruePosition();
                return false;
            }
            _tickCounter = 0;
            return true;
        }
    }
}
