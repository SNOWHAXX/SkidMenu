using HarmonyLib;
using InnerNet;

namespace SkidMenu;

public static class LobbyTimer
{
    public static bool Enabled = false;
    private static bool _shown = false;

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    static class Patch
    {
        static void Postfix(GameStartManager __instance)
        {
            if (!Enabled) { _shown = false; return; }
            if (AmongUsClient.Instance == null) return;
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Joined) return;
            if (_shown) return;
            _shown = true;
            HudManager.Instance?.ShowLobbyTimer(600);
        }
    }
}
