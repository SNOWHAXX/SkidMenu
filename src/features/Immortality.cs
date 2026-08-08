using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features
{
    internal class Immortality
    {
        // Works by sending VentilationSystem.Update(Enter, CUSTOM_VENT_ID) to the server,
        // making the backend think we're inside a vent and block CheckMurder RPCs targeting us.
        // Managed by a FixedUpdate loop so it re-applies automatically on new games,
        // exits cleanly when entering real vents, and re-applies when we leave them.
        private const int CUSTOM_VENT_ID = 250;
        private const int REAPPLY_INTERVAL = 10;
        private static int _tickCount = 0;

        private static bool _enabled = false;
        private static bool _ventApplied = false;

        public static bool DisableNotification = false;

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;
                _enabled = value;
                if (!value && _ventApplied && ShipStatus.Instance != null)
                {
                    VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID);
                    _ventApplied = false;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class TickImmortality
        {
            static void Postfix(PlayerControl __instance)
            {
                if (__instance != PlayerControl.LocalPlayer) return;
                if (ShipStatus.Instance == null) { _ventApplied = false; return; }
                if (!_enabled) { if (_ventApplied) { VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID); _ventApplied = false; } return; }
                if (__instance.Data == null || __instance.Data.IsDead) { if (_ventApplied) { VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID); _ventApplied = false; } return; }
                if (MeetingHud.Instance != null) { if (_ventApplied) { VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID); _ventApplied = false; } return; }

                if (__instance.inVent)
                {
                    if (_ventApplied) { VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID); _ventApplied = false; }
                }
                else
                {
                    if (!_ventApplied) { VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID); _ventApplied = true; _tickCount = 0; }
                    else
                    {
                        _tickCount++;
                        if (_tickCount >= REAPPLY_INTERVAL)
                        {
                            VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
                            _tickCount = 0;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class OnMurder
        {
            static void Postfix(PlayerControl __instance, PlayerControl target)
            {
                if (_enabled && target == PlayerControl.LocalPlayer)
                    if (!DisableNotification) SkidMenu.notifications.Send("Immortality", $"{__instance.Data.PlayerName} attempted to kill you!", 5);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        class OnGameStart
        {
            static void Postfix()
            {
                if (!_enabled || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;
                VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
                _ventApplied = true;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        class OnMeetingEnd
        {
            static void Postfix()
            {
                if (!_enabled || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;
                if (PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.inVent) return;
                VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
                _ventApplied = true;
            }
        }
    }
}




