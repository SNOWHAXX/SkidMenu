using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace SkidMenu.features;

public static class FuckGame
{
    public static bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            if (!value) Cleanup();
        }
    }

    private static bool _enabled;

    private const float INTERVAL    = 0.5f;
    private const float VK_INTERVAL = 2f;
    private const float BAN_AFTER   = 60f;
    private const float HOST_REPORT = 1f;
    private const float HOST_SKIP   = 2f;
    private const int   MAX_RPCS    = 30;

    private static float _doorTimer, _reportTimer, _sabTimer, _vkTimer;
    private static float _elapsed, _rpcWindow;
    private static float _hostReportTimer, _meetingTimer;
    private static int   _rpcCount;
    private static bool  _banFired;

    private static bool TryRpc()
    {
        if (_rpcCount >= MAX_RPCS) return false;
        _rpcCount++;
        return true;
    }

    private static void Cleanup()
    {
        _doorTimer = _reportTimer = _sabTimer = _vkTimer =
        _elapsed = _rpcWindow = _hostReportTimer = _meetingTimer = 0f;
        _rpcCount = 0;
        _banFired = false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    static class Tick
    {
        static void Postfix(PlayerControl __instance)
        {
            if (!_enabled || __instance != PlayerControl.LocalPlayer) return;
            if (ShipStatus.Instance == null) { Cleanup(); return; }
            if (__instance.Data?.IsDead == true) return;

            float dt    = Time.fixedDeltaTime;
            bool isHost = AmongUsClient.Instance.AmHost;
            bool inMtg  = MeetingHud.Instance != null;

            _rpcWindow += dt;
            if (_rpcWindow >= 1f) { _rpcWindow = 0f; _rpcCount = 0; }

            _elapsed += dt;

            // Door prison
            _doorTimer += dt;
            if (_doorTimer >= INTERVAL) { _doorTimer = 0f; if (TryRpc()) Sabotage.LockAll(); }

            // Report all bodies
            if (!inMtg)
            {
                _reportTimer += dt;
                if (_reportTimer >= INTERVAL)
                {
                    _reportTimer = 0f;
                    foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
                    {
                        if (pc == null || pc.Data == null || !pc.Data.IsDead) continue;
                        if (!TryRpc()) break;
                        if (isHost) Utilities.OpenMeeting(PlayerControl.LocalPlayer, pc.Data);
                        else        PlayerControl.LocalPlayer.CmdReportDeadBody(pc.Data);
                        break;
                    }
                }
            }

            // Sabotage chaos
            _sabTimer += dt;
            if (_sabTimer >= INTERVAL)
            {
                _sabTimer = 0f;
                if (TryRpc()) Sabotage.SabotageSystem(SystemTypes.Comms);
                if (TryRpc()) try { Sabotage.SabotageSystem(SystemTypes.MushroomMixupSabotage); } catch { }
                if (TryRpc()) Sabotage.SabotageSystem(SystemTypes.Reactor);
                if (TryRpc()) Sabotage.FixSabotage(SystemTypes.Reactor);
                if (TryRpc()) Sabotage.SabotageSystem(SystemTypes.LifeSupp);
                if (TryRpc()) Sabotage.FixSabotage(SystemTypes.LifeSupp);
            }

            // Unfixable lights — keep forcing the toggle on
            CheatToggles.unfixableLights = true;

            // Votekick all
            _vkTimer += dt;
            if (_vkTimer >= VK_INTERVAL) { _vkTimer = 0f; VotekickHandler.VotekickAllNow(); }

            // Ban all after 60s
            if (!_banFired && _elapsed >= BAN_AFTER)
            {
                _banFired = true;
                foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
                    BanHandler.BanPlayer(pc);
            }

            if (!isHost) return;

            // Host addons
            CheatToggles.noGameEnd = true;

            // Kill all
            foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
            {
                if (pc == null || pc.AmOwner || pc.Data?.IsDead == true) continue;
                if (!TryRpc()) break;
                PlayerControl.LocalPlayer.RpcMurderPlayer(pc, true);
            }

            // Report random body every 1s, skip after 2s
            if (!inMtg)
            {
                _meetingTimer = 0f;
                _hostReportTimer += dt;
                if (_hostReportTimer >= HOST_REPORT)
                {
                    _hostReportTimer = 0f;
                    if (TryRpc())
                    {
                        var deadBody = PlayerControl.AllPlayerControls.ToArray()
                            .FirstOrDefault(p => p != null && !p.AmOwner && p.Data != null && p.Data.IsDead);
                        Utilities.OpenMeeting(PlayerControl.LocalPlayer, deadBody?.Data);
                    }
                }
            }
            else
            {
                _hostReportTimer = 0f;
                _meetingTimer += dt;
                if (_meetingTimer >= HOST_SKIP)
                {
                    _meetingTimer = 0f;
                    try { MeetingHud.Instance.RpcClose(); } catch { }
                }
            }
        }
    }
}
