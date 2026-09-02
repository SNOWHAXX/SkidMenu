using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace SkidMenu;

internal static class MovementEvents
{
    private const float Interval = 1f;
    private static readonly Dictionary<(byte player, byte kind), float> _last = new();

    private static bool Throttle(byte playerId, byte kind)
    {
        var key = (playerId, kind);
        if (_last.TryGetValue(key, out float t) && Time.unscaledTime - t < Interval) return false;
        _last[key] = Time.unscaledTime;
        return true;
    }

    public static void Fire(byte kind, PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (!Throttle(player.PlayerId, kind)) return;

        string who = player.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(player);

        switch (kind)
        {
            case 0:
                if (CheatToggles.logZipline) ConsoleUI.Log($"{who} used a zipline", "ffff88");
                if (CheatToggles.notifZipline && !NotifHelper.Skip(player, 23))
                    SkidMenu.notifications.Send("<color=#ffff88>Zipline</color>", $"{who} used a zipline", 3f);
                break;
            case 1:
                if (CheatToggles.logPlatform) ConsoleUI.Log($"{who} used a platform", "88ffff");
                if (CheatToggles.notifPlatform && !NotifHelper.Skip(player, 24))
                    SkidMenu.notifications.Send("<color=#88ffff>Platform</color>", $"{who} used a platform", 3f);
                break;
            default:
                if (CheatToggles.logLadder) ConsoleUI.Log($"{who} climbed a ladder", "88ff88");
                if (CheatToggles.notifLadder && !NotifHelper.Skip(player, 25))
                    SkidMenu.notifications.Send("<color=#88ff88>Ladder</color>", $"{who} climbed a ladder", 3f);
                break;
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
public static class Mov_ClimbLadderRecv
{
    static void Postfix(PlayerPhysics __instance, byte callId)
    {
        if (callId != (byte)RpcCalls.ClimbLadder) return;
        MovementEvents.Fire(2, __instance.myPlayer);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcClimbLadder))]
public static class Mov_ClimbLadderSend
{
    static void Postfix(PlayerPhysics __instance)
    {
        MovementEvents.Fire(2, __instance.myPlayer);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUseZipline))]
public static class Mov_ZiplineSend
{
    static void Postfix(PlayerControl __instance)
    {
        MovementEvents.Fire(0, __instance);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
public static class Mov_PlatformSend
{
    static void Postfix(PlayerControl __instance)
    {
        MovementEvents.Fire(1, __instance);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class Mov_PlayerControlRecv
{
    static void Postfix(PlayerControl __instance, byte callId)
    {
        if (callId == (byte)RpcCalls.UseZipline) MovementEvents.Fire(0, __instance);
        else if (callId == (byte)RpcCalls.UsePlatform) MovementEvents.Fire(1, __instance);
    }
}