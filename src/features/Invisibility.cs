using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features;

public static class Invisibility
{
    public static bool Enabled    = false;
    public static bool OnlyInGame = true;

    // Far off the map so the server and every client place us in the void.
    // Randomized each reinforce so it's never the exact same spot, max 100 off.
    private const float VoidCenter = 25000f;
    private const float VoidSpread = 100f;

    // The server keeps authority and teleports us on kills, vents, meetings
    // and corrections, so re-assert the off-map position every 800ms. One RPC
    // per interval, far below any RPC-flood threshold.
    private const float ReinforceInterval = 0.8f;
    private static float _lastReinforceTime = 0f;

    private static bool ShouldRun()
    {
        if (!Enabled) return false;
        if (OnlyInGame && ShipStatus.Instance == null) return false;
        return true;
    }

    private static void Reinforce()
    {
        if (!ShouldRun()) return;
        if (PlayerControl.LocalPlayer == null) return;
        CustomNetworkTransform netTransform = PlayerControl.LocalPlayer.NetTransform;
        if (netTransform == null) return;
        ReinforceOffMap(netTransform);
    }

    private static void ReinforceOffMap(CustomNetworkTransform netTransform)
    {
        try
        {
            // Server-only SnapTo RPC: broadcast the off-map position without
            // touching our local transform, so we keep walking normally.
            ushort seq = (ushort)(netTransform.lastSequenceId + 1);
            netTransform.lastSequenceId = seq;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                netTransform.NetId,
                (byte)RpcCalls.SnapTo,
                SendOption.Reliable,
                -1
            );
            NetHelpers.WriteVector2(NextVoidPosition(), writer);
            writer.Write(seq);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        catch { }
    }

    private static Vector2 NextVoidPosition()
    {
        return new Vector2(
            VoidCenter + UnityEngine.Random.Range(-VoidSpread, VoidSpread),
            VoidCenter + UnityEngine.Random.Range(-VoidSpread, VoidSpread)
        );
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    private static class Invisibility_Patch
    {
        private static bool Prefix(CustomNetworkTransform __instance)
        {
            if (!ShouldRun()) return true;
            if (__instance.myPlayer == null || __instance.myPlayer != PlayerControl.LocalPlayer) return true;

            if (Time.time - _lastReinforceTime >= ReinforceInterval)
            {
                _lastReinforceTime = Time.time;
                ReinforceOffMap(__instance);
            }

            // Freeze: suppress the vanilla position broadcast so the real
            // position never leaks out. Stays active in meetings too.
            return false;
        }
    }

    // The server teleports us on these events, so re-assert the off-map
    // position immediately instead of waiting for the next interval.
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    private static class OnKill
    {
        private static void Postfix() => Reinforce();
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
    private static class OnVentEnter
    {
        private static void Postfix() => Reinforce();
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    private static class OnVentExit
    {
        private static void Postfix() => Reinforce();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    private static class OnMeetingStart
    {
        private static void Postfix() => Reinforce();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    private static class OnMeetingEnd
    {
        private static void Postfix() => Reinforce();
    }

    // The server snaps us via SnapTo RPCs (game start spawn, meeting spawn,
    // exile teleport, position corrections). Re-assert off-map instantly
    // instead of waiting for the next interval, so the real position never
    // gets shown to other clients.
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
    private static class OnSnapTo
    {
        private static void Postfix(CustomNetworkTransform __instance)
        {
            if (__instance.myPlayer != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }

    // Ladder climbing is a direct position-sync RPC outside the net transform,
    // so the freeze does not cover it.
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcClimbLadder))]
    private static class OnClimbLadder
    {
        private static void Postfix() => Reinforce();
    }

    // Being booted out of a vent is a direct position-sync RPC.
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcBootFromVent))]
    private static class OnBootFromVent
    {
        private static void Postfix() => Reinforce();
    }

    // Riding a moving platform is a direct position-sync RPC.
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    private static class OnUsePlatform
    {
        private static void Postfix() => Reinforce();
    }

    // Exile teleports the player back to the ship.
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    private static class OnExiled
    {
        private static void Postfix() => Reinforce();
    }

    // Blanket receive-side catch: the server can force vent/ladder/boot
    // physics RPCs on us that skip the local Rpc* calls, so catch them all.
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    private static class OnPhysicsRpc
    {
        private static void Postfix(PlayerPhysics __instance)
        {
            if (__instance == null || __instance.myPlayer != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }

    // Blanket receive-side catch: exile, murder and teleport RPCs arrive here
    // (there is no separate RpcExiled in this game build).
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    private static class OnControlRpc
    {
        private static void Postfix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }

    // Server-forced vent entry/exit that bypasses RpcEnterVent/RpcExitVent.
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    private static class OnVentEnterComponent
    {
        private static void Postfix(PlayerControl pc)
        {
            if (pc != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    private static class OnVentExitComponent
    {
        private static void Postfix(PlayerControl pc)
        {
            if (pc != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }

    // Death routes other than MurderPlayer (poison, cleanup kills, mod kills).
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    private static class OnDie
    {
        private static void Postfix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            Reinforce();
        }
    }
}
