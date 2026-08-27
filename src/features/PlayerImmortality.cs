using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features
{
    internal static class PlayerImmortality
    {
        private const byte GRANT_VENT = 231;
        private const int REAPPLY_INTERVAL = 10;
        private static readonly HashSet<byte> _immortal = new();
        private static int _tickCount = 0;

        public static bool IsImmortal(byte pid) => _immortal.Contains(pid);

        public static string Toggle(PlayerControl target)
        {
            if (target == null || target.Data == null) return "No target";
            if (AmongUsClient.Instance == null || ShipStatus.Instance == null) return "In-match only";
            if (target == PlayerControl.LocalPlayer) return "Use Self Immortal for yourself";

            bool on = !_immortal.Contains(target.PlayerId);
            if (!SendVentRpc(target, on ? 2 : 3)) return "Failed";

            if (on) _immortal.Add(target.PlayerId);
            else _immortal.Remove(target.PlayerId);
            return on ? "Immortality granted" : "Immortality removed";
        }

        public static void Forget() => _immortal.Clear();

        private static bool SendVentRpc(PlayerControl target, int op)
        {
            MessageWriter body = null;
            try
            {
                var net = (InnerNetClient)AmongUsClient.Instance;
                body = MessageWriter.Get(SendOption.Reliable);
                body.Write((ushort)0);
                body.Write((byte)op);
                body.Write(GRANT_VENT);

                MessageWriter w = net.StartRpcImmediately(
                    ((InnerNetObject)ShipStatus.Instance).NetId,
                    35,
                    SendOption.Reliable,
                    net.HostId);
                if (w == null) return false;
                w.Write((byte)SystemTypes.Ventilation);
                w.WriteNetObject(target);
                w.Write(body, false);
                net.FinishRpcImmediately(w);
                return true;
            }
            catch { return false; }
            finally { try { body?.Recycle(); } catch { } }
        }

        private static void ReapplyAll()
        {
            foreach (byte pid in _immortal)
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && pc.PlayerId == pid && !pc.Data.IsDead && !pc.inVent)
                        SendVentRpc(pc, 2);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class TickImmortality
        {
            static void Postfix(PlayerControl __instance)
            {
                if (__instance != PlayerControl.LocalPlayer) return;
                if (_immortal.Count == 0) return;
                if (ShipStatus.Instance == null) return;
                if (MeetingHud.Instance != null) return;

                _tickCount++;
                if (_tickCount >= REAPPLY_INTERVAL)
                {
                    ReapplyAll();
                    _tickCount = 0;
                }
            }
        }

        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        class OnLobby { static void Postfix() => Forget(); }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnEnable))]
        class OnShip
        {
            static void Postfix()
            {
                if (_immortal.Count == 0) return;
                ReapplyAll();
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        class OnMeetingEnd
        {
            static void Postfix()
            {
                if (_immortal.Count == 0) return;
                ReapplyAll();
            }
        }
    }
}
