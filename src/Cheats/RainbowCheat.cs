using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class RainbowCheat
{
    private static float _timer = 0f;
    private static float _randomizeTimer = 0f;
    private static int _colorIndex = 0;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    static class Patch
    {
        static void Postfix()
        {
            if (PlayerControl.LocalPlayer == null) return;

            if (SelfTab.RainbowEnabled)
            {
                _timer += Time.deltaTime;
                if (_timer >= SelfTab.RainbowDelay)
                {
                    _timer = 0f;
                    int attempts = 0;
                    do { _colorIndex = (_colorIndex + 1) % 18; attempts++; }
                    while (!AmongUsClient.Instance.AmHost && attempts < 18 && System.Array.Exists<PlayerControl>(PlayerControl.AllPlayerControls.ToArray(), p => p != PlayerControl.LocalPlayer && p.Data != null && p.Data.DefaultOutfit.ColorId == _colorIndex));
                    OutfitBypass.SetColor(_colorIndex);
                }
            }

            if (SelfTab.RandomizeSpam)
            {
                _randomizeTimer += Time.deltaTime;
                if (_randomizeTimer >= SelfTab.RandomizeDelay)
                {
                    _randomizeTimer = 0f;
                    if (AmongUsClient.Instance.AmConnected) Utilities.RandomizePlayer(true);
                    else { AccountManager.Instance.RandomizeName(); Utilities.RandomizePlayer(); }
                }
            }
            else _randomizeTimer = 0f;
        }
    }
}


