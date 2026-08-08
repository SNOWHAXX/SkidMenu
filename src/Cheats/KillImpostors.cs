using HarmonyLib;
using AmongUs.GameOptions;

namespace SkidMenu;

public static class KillImpostors
{
    public static bool Enabled { get; set; } = false;

    // ImpostorRole.IsValidTarget filters kill targets — normally returns false for
    // other impostors. Postfix overrides that to true when our toggle is on.
    [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
    static class Patch
    {
        static void Postfix(NetworkedPlayerInfo target, ref bool __result)
        {
            if (!Enabled) return;
            if (__result) return;
            if (target == null || target.IsDead || target.Disconnected) return;
            if (target.PlayerId == PlayerControl.LocalPlayer?.PlayerId) return;

            if (target.RoleType == RoleTypes.Impostor     ||
                target.RoleType == RoleTypes.Shapeshifter ||
                target.RoleType == RoleTypes.Phantom      ||
                target.RoleType == RoleTypes.Viper)
                __result = true;
        }
    }
}
