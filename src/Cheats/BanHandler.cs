using System.Collections.Generic;
using Hazel;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using UnityEngine;

namespace SkidMenu;

public static class BanHandler
{
    public static float SpamCooldown = 0.02f;
    public static bool SpamReactorAll = false;
    public static bool SpamDoorsAll = false;
    public static readonly Dictionary<int, bool> SpamReactorPerPlayer = new();
    public static readonly Dictionary<int, bool> SpamDoorsPerPlayer = new();
    private static float _timer = 0f;

    private static SystemTypes GetReactorSys() => (SystemTypes)(GameOptionsManager.Instance.currentGameOptions.MapId switch
    {
        4 => 58,
        2 => 21,
        5 => 57,
        _ => 3,
    });

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    static class SpamDriver
    {
        static void Postfix()
        {
            _timer += Time.deltaTime;
            if (SpamCooldown > 0f && _timer < SpamCooldown) return;
            _timer = 0f;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (player == null || player.AmOwner || player.Data == null) continue;

                if (SpamReactorAll || (SpamReactorPerPlayer.TryGetValue(player.OwnerId, out bool sr) && sr))
                    FakeReactorTarget(player);
                if (SpamDoorsAll || (SpamDoorsPerPlayer.TryGetValue(player.OwnerId, out bool sd) && sd))
                    DoorHallucination(player);
            }
        }
    }

    public static void BanPlayer(PlayerControl target)
    {
        if (target == null || ShipStatus.Instance == null || target.AmOwner) return;
        uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
        int ownerId  = target.OwnerId;
        int clientId = target.Data.ClientId;

        SendUpdateSystem(AmongUsClient.Instance, (SystemTypes)37, netId, new byte[4] { 0, 0, 2, 0 }, ownerId);
        SendUpdateSystem(AmongUsClient.Instance, (SystemTypes)37, netId, new byte[4] { 1, 0, 5, 0 }, ownerId);
        SendUpdateSystem(AmongUsClient.Instance, (SystemTypes)37, netId, new byte[4] { 0, 0, 2, 0 }, clientId);
        SendUpdateSystem(AmongUsClient.Instance, (SystemTypes)37, netId, new byte[4] { 1, 0, 5, 0 }, clientId);
    }

    public static void FakeReactorTarget(PlayerControl target)
    {
        if (target == null || ShipStatus.Instance == null) return;
        uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
        SendUpdateSystem(AmongUsClient.Instance, GetReactorSys(), netId, new byte[1] { 128 }, target.OwnerId);
        SendUpdateSystem(AmongUsClient.Instance, GetReactorSys(), netId, new byte[1] { 128 }, target.Data.ClientId);
    }

    public static void DoorHallucination(PlayerControl target)
    {
        if (target == null || ShipStatus.Instance == null) return;
        foreach (OpenableDoor door in (Il2CppArrayBase<OpenableDoor>)(object)ShipStatus.Instance.AllDoors)
        {
            if (door == null) continue;
            foreach (int targetId in new[] { target.OwnerId, target.Data.ClientId })
            {
                MessageWriter msg = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(
                    ((InnerNetObject)ShipStatus.Instance).NetId, (byte)27, (SendOption)1, targetId);
                msg.Write((byte)(int)door.Room);
                ((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(msg);
            }
        }
    }

    private static void SendUpdateSystem(AmongUsClient client, SystemTypes sys, uint netId, byte[] data, int targetId)
    {
        if (ShipStatus.Instance == null) return;
        MessageWriter msg = ((InnerNetClient)client).StartRpcImmediately(
            ((InnerNetObject)ShipStatus.Instance).NetId, (byte)35, (SendOption)1, targetId);
        msg.Write((byte)(int)sys);
        msg.WritePacked(netId);
        foreach (byte b in data)
            msg.Write(b);
        ((InnerNetClient)client).FinishRpcImmediately(msg);
    }
}
