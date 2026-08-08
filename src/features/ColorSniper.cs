using HarmonyLib;

namespace SkidMenu.features;

public static class ColorSniper
{
    public static bool Enabled = false;
    public static bool InLobbyOnly = true;
    public static byte TargetColor = 0;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    static class ColorSniperPatch
    {
        static void Postfix(PlayerControl __instance)
        {
            if (!Enabled || __instance != PlayerControl.LocalPlayer) return;
            if (InLobbyOnly && LobbyBehaviour.Instance == null) return;

            if (AmongUsClient.Instance.AmHost)
            {
                OutfitBypass.SetColor(TargetColor);
                return;
            }

            foreach (var p in PlayerControl.AllPlayerControls)
                if (p != PlayerControl.LocalPlayer && p.Data != null && p.Data.DefaultOutfit.ColorId == TargetColor)
                    return;

            OutfitBypass.SetColor(TargetColor);
        }
    }
}



