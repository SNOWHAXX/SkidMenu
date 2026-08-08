using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    // Prefix patch of PlayerControl.SetKillTimer to remove kill cooldown
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

        time = 0f;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class PlayerControl_CmdCheckMurder
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!Utils.isHost) return true;
        PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
        return false;
    }
}

[HarmonyPatch(typeof(GuardianAngelRole), nameof(GuardianAngelRole.UseAbility))]
public static class PlayerControl_GuardianProtect
{
    public static void Postfix(GuardianAngelRole __instance)
    {
        PlayerControl guardian = __instance?.Player;
        PlayerControl target   = __instance?.currentTarget;
        if (guardian == null || target == null || !guardian.AmOwner) return;

        if (CheatToggles.logGuardianProtect)
            try { ConsoleUI.Log($"{ConsoleHelper.Fmt(guardian)} <color=#88ffcc>protected</color> {ConsoleHelper.Fmt(target)}{ConsoleHelper.Room(target)}", "88ffcc"); }
            catch { }

        if (CheatToggles.notifGuardianProtect && !NotifHelper.Skip(guardian, 17))
            try {
                string gName = guardian.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(guardian);
                string prot  = target.AmOwner   ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(target);
                SkidMenu.notifications.Send("<color=#88ffcc>🛡 Protected</color>",
                    $"{gName} protected {prot}{NotifHelper.Room(target)}{NotifHelper.Dist(target)}", 3f);
            } catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    public static void Prefix(PlayerControl __instance, ref bool visible)
    {
        if (CheatToggles.seeGhosts) visible = true;
    }

    public static void Postfix(PlayerControl __instance, bool visible, int colorId, int guardianPlayerId)
    {
        try
        {
            int gId = __instance.protectedByGuardianId >= 0 ? __instance.protectedByGuardianId : guardianPlayerId;
            PlayerControl guardian = GameData.Instance?.GetPlayerById((byte)gId)?.Object;
            if (__instance == null || guardian == null) return;
            if (guardian.AmOwner && CheatToggles.logGuardianProtect) return;

            if (CheatToggles.logGuardianProtect)
                try { ConsoleUI.Log($"{ConsoleHelper.Fmt(guardian)} <color=#88ffcc>protected</color> {ConsoleHelper.Fmt(__instance)}{ConsoleHelper.Room(__instance)}", "88ffcc"); }
                catch { }

            if (CheatToggles.notifGuardianProtect && !NotifHelper.Skip(guardian, 17))
                try {
                    string gName = guardian.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(guardian);
                    string prot  = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
                    SkidMenu.notifications.Send("<color=#88ffcc>🛡 Protected</color>",
                        $"{gName} protected {prot}{NotifHelper.Room(__instance)}{NotifHelper.Dist(__instance)}", 3f);
                } catch { }
        } catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate)
    {
        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    // Postfix patch of PlayerControl.Shapeshift to log on ConsoleUI when a player shapeshifts into another player,
    // and who they shapeshifted into. Also logs when a shapeshift gets reverted.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (__instance == null || targetPlayer == null) return;
        try
        {
            if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;
            var room = ConsoleHelper.Room(__instance);
            if (targetPlayer.PlayerId == __instance.PlayerId)
            {
                if (!CheatToggles.logShapeshiftRevert) return;
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} reverted shapeshift{room}", "FF8C00");
            }
            else
            {
                if (!CheatToggles.logShapeshiftInto) return;
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} shapeshifted into {ConsoleHelper.Fmt(targetPlayer)}{room}", "FF8C00");
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    // Prefix patch of PlayerControl.RpcSyncSettings to prevent the anti-cheat from kicking you
    // for some settings that are out of the "original" valid range
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        return !CheatToggles.noOptionsLimits;
    }

}

[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.SetCooldown))]
static class NoVanishCooldownPatch
{
    static bool Prefix(PhantomRole __instance)
    {
        if (!CheatToggles.noVanishCooldown) return true;
        __instance.cooldownSecondsRemaining = 0f;
        return false;
    }
}

[HarmonyPatch(typeof(ShapeshifterRole), nameof(ShapeshifterRole.SetCooldown))]
static class NoShapeshiftCooldownPatch
{
    static bool Prefix(ShapeshifterRole __instance)
    {
        if (!CheatToggles.noShapeshiftCooldown) return true;
        __instance.cooldownSecondsRemaining = 0f;
        return false;
    }
}