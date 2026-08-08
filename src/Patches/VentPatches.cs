using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse
{
    // Postfix patch of Vent.CanUse to allow usage of vents when useVents cheat is enabled
    public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if (PlayerControl.LocalPlayer.Data.Role.CanVent || PlayerControl.LocalPlayer.Data.IsDead) return;
        if (!CheatToggles.unlockVents) return;

        var @object = pc.Object;

        var center = @object.Collider.bounds.center;
        var position = __instance.transform.position;
        var num = Vector2.Distance(center, position);

        // Allow usage of vents unless the vent is too far or there are objects blocking the player's path
        canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        couldUse = canUse;
        __result = num;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class Vent_EnterVent
{
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (pc != null && pc.AmOwner && CheatToggles.unlockVents && !CheatToggles.walkInVents)
        {
            try { pc.inVent = true; pc.moveable = false; } catch { }
        }

        if (!CheatToggles.logVentIn || !Utils.isShip) return;
        try
        {
            var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);
            var room = Utils.GetRoomFromPosition(__instance.transform.position);
            var roomName = room != null ? $" <color=#888888>[{room.RoomId}]</color>" : "";
            ConsoleUI.Log(isDisguised
                ? $"{realPlayerName} (as {displayPlayerName}) entered a vent{roomName}"
                : $"{realPlayerName} entered a vent{roomName}", "FFFF44");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class Vent_ExitVent
{
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (pc != null && pc.AmOwner && CheatToggles.walkInVents)
        {
            try
            {
                pc.inVent = false;
                pc.moveable = true;
                pc.Visible = true;
            }
            catch { }
        }

        if (!CheatToggles.logVentOut || !Utils.isShip) return;
        try
        {
            var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);
            var room = Utils.GetRoomFromPosition(__instance.transform.position);
            var roomName = room != null ? $" <color=#888888>[{room.RoomId}]</color>" : "";
            ConsoleUI.Log(isDisguised
                ? $"{realPlayerName} (as {displayPlayerName}) exited a vent{roomName}"
                : $"{realPlayerName} exited a vent{roomName}", "FFFF44");
        }
        catch { }
    }
}
