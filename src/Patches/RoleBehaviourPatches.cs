using HarmonyLib;
using System.Linq;
using Sentry.Internal.Extensions;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(EngineerRole), nameof(EngineerRole.FixedUpdate))]
public static class EngineerRole_FixedUpdate
{
    public static void Postfix(EngineerRole __instance)
    {
        if(__instance.Player.AmOwner)
        {
            MalumCheats.HandleEngineerCheats(__instance);
        }
    }
}

[HarmonyPatch(typeof(ShapeshifterRole), nameof(ShapeshifterRole.FixedUpdate))]
public static class ShapeshifterRole_FixedUpdate
{
    public static void Postfix(ShapeshifterRole __instance)
    {
        try
        {
            if(__instance.Player.AmOwner)
            {
                MalumCheats.HandleShapeshifterCheats(__instance);
            }
        } catch { }
    }
}

[HarmonyPatch(typeof(ScientistRole), nameof(ScientistRole.Update))]
public static class ScientistRole_Update
{
    public static void Postfix(ScientistRole __instance)
    {
        if(__instance.Player.AmOwner)
        {
            MalumCheats.HandleScientistCheats(__instance);
        }
    }
}

[HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FixedUpdate))]
public static class TrackerRole_FixedUpdate
{
    public static void Postfix(TrackerRole __instance)
    {
        if(__instance.Player.AmOwner)
        {
            MalumCheats.HandleTrackerCheats(__instance);
        }
    }
}

[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.IsValidTarget))]
public static class PhantomRole_IsValidTarget
{
    // Postfix patch of PhantomRole.IsValidTarget to allow killing while invisible
    public static void Postfix(PhantomRole __instance, NetworkedPlayerInfo target, ref bool __result)
    {
        if (target == null) return;

        if (features.NoKillChecks.Enabled)
        {
            __result = features.NoKillChecks.IsValidTarget(target) && (!__instance.isInvisible || features.NoKillChecks.KillAsPhantom);
            return;
        }

        if (CheatToggles.killVanished)
        {
            __result = Utils.IsValidTarget(target);
        }
    }
}

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class ImpostorRole_IsValidTarget
{
    // Postfix patch of ImpostorRole.IsValidTarget to allow forbidden kill targets for killAnyone cheat
    // Allows killing ghosts (with seeGhosts), impostors, players in vents, etc...
    public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
    {
        if (target == null) return;

        // NoKillChecks takes priority over all other kill target cheats
        if (features.NoKillChecks.Enabled)
        {
            __result = features.NoKillChecks.IsValidTarget(target);
            return;
        }

        if (CheatToggles.killAnyone)
        {
            __result = Utils.IsValidTarget(target);
            return;
        }

        if (CheatToggles.killGhosts && !__result && !target.Disconnected && target.IsDead)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.FindClosestTarget))]
public static class ImpostorRole_FindClosestTarget
{
    public static bool Prefix(ImpostorRole __instance, ref PlayerControl __result)
    {
        if (!CheatToggles.killReach) return true;

        var localPos = PlayerControl.LocalPlayer.transform.position;
        bool infinite = CheatToggles.killReachInfinite;
        float maxRange = CheatToggles.killReachRange;

        PlayerControl best = null;
        float bestDist = float.MaxValue;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsNull() || !__instance.IsValidTarget(player.Data) || !player.Collider.enabled) continue;

            float dist = Vector2.Distance(localPos, player.transform.position);
            if (!infinite && dist > maxRange) continue;
            if (dist < bestDist) { bestDist = dist; best = player; }
        }

        if (best == null) return true;

        __result = best;
        return false;
    }
}

[HarmonyPatch(typeof(DetectiveRole), nameof(DetectiveRole.FindClosestTarget))]
public static class DetectiveRole_FindClosestTarget
{
    // Prefix patch of DetectiveRole.FindClosestTarget to allow for infinite interrogate reach
    public static bool Prefix(DetectiveRole __instance, ref PlayerControl __result)
    {
        if (!CheatToggles.interrogateReach) return true;

        var playerList = Utils.GetPlayersSortedByDistance().Where(player => !player.IsNull() && __instance.IsValidTarget(player.Data) && player.Collider.enabled).ToList();

        __result = playerList[0];

        return false;
    }
}

[HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FindClosestTarget))]
public static class TrackerRole_FindClosestTarget
{
    // Prefix patch of TrackerRole.FindClosestTarget to allow for infinite track reach
    public static bool Prefix(TrackerRole __instance, ref PlayerControl __result)
    {
        if (!CheatToggles.trackReach) return true;

        var playerList = Utils.GetPlayersSortedByDistance().Where(player => !player.IsNull() && __instance.IsValidTarget(player.Data) && player.Collider.enabled).ToList();

        __result = playerList[0];

        return false;
    }
}
