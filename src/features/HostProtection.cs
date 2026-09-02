using System.Collections.Generic;
using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features;

public class HostProtection : MonoBehaviour
{
    private float _godModeTimer = -1f;
    private float _godModeAllTimer = -1f;
    private float _autoAngelTimer = -1f;
    private byte _lastAngelTarget = byte.MaxValue;

    private void FixedUpdate()
    {
        if (SkidMenu.isPanicked) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.Started) return;
        if (LobbyBehaviour.Instance != null) return;

        TryGodModeTick();
        TryGodModeAllTick();
        TryAutoAngelTick();
    }

    private static bool CanRun()
    {
        if (AmongUsClient.Instance == null) return false;
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return false;
        return ShipStatus.Instance != null && LobbyBehaviour.Instance == null;
    }

    private void TryGodModeTick()
    {
        if (!CheatToggles.godMode)
        {
            _godModeTimer = -1f;
            return;
        }

        if (!CanRun() || !Utils.isHost) return;

        PlayerControl local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.Disconnected || local.Data.IsDead) return;
        if (local.protectedByGuardianId >= 0) return;

        float now = Time.unscaledTime;
        if (_godModeTimer > 0f && now < _godModeTimer) return;
        _godModeTimer = now + 0.20f;

        try { local.RpcProtectPlayer(local, local.Data.DefaultOutfit.ColorId); }
        catch { }
    }

    private void TryGodModeAllTick()
    {
        if (!CheatToggles.godModeAll)
        {
            _godModeAllTimer = -1f;
            return;
        }

        if (!CanRun() || !Utils.isHost) return;

        float now = Time.unscaledTime;
        if (_godModeAllTimer > 0f && now < _godModeAllTimer) return;
        _godModeAllTimer = now + 0.20f;

        ProtectEveryone(false);
    }

    private void TryAutoAngelTick()
    {
        if (!CheatToggles.autoAngel)
        {
            _autoAngelTimer = -1f;
            _lastAngelTarget = byte.MaxValue;
            return;
        }

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

        PlayerControl local = PlayerControl.LocalPlayer;
        if (!IsLocalGuardianAngel(local))
        {
            _autoAngelTimer = -1f;
            _lastAngelTarget = byte.MaxValue;
            return;
        }

        float now = Time.unscaledTime;
        if (_autoAngelTimer > 0f && now < _autoAngelTimer) return;
        _autoAngelTimer = now + Mathf.Clamp(CheatToggles.autoAngelInterval, 0.1f, 2f);

        PlayerControl target = PickTarget(local);
        if (target == null) return;

        try { local.CmdCheckProtect(target); _lastAngelTarget = target.PlayerId; }
        catch { }
    }

    public static void ProtectEveryone(bool notify)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!CanRun() || PlayerControl.AllPlayerControls == null) return;

        PlayerControl local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null) return;

        int count = 0;
        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target.Data == null || target.PlayerId >= 100) continue;
            if (target.Data.Disconnected || target.Data.IsDead || target.protectedByGuardianId >= 0) continue;

            try { local.RpcProtectPlayer(target, target.Data.DefaultOutfit.ColorId); count++; }
            catch { }
        }
    }

    private static bool IsLocalGuardianAngel(PlayerControl local)
    {
        return local.Data != null &&
               !local.Data.Disconnected &&
               local.Data.Role != null &&
               local.Data.Role.Role == RoleTypes.GuardianAngel;
    }

    private static bool IsValidTarget(PlayerControl local, PlayerControl pc)
    {
        if (pc == null || pc == local || pc.Data == null) return false;
        if (pc.PlayerId >= 100 || pc.Data.Disconnected || pc.Data.IsDead) return false;
        if (pc.inVent || pc.onLadder || pc.inMovingPlat) return false;
        return pc.Visible;
    }

    private PlayerControl PickTarget(PlayerControl local)
    {
        if (PlayerControl.AllPlayerControls == null) return null;

        List<PlayerControl> plrs = new List<PlayerControl>();
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            if (IsValidTarget(local, pc)) plrs.Add(pc);
        }

        if (plrs.Count == 0) return null;
        if (plrs.Count == 1) return plrs[0];

        for (int i = 0; i < 6; i++)
        {
            PlayerControl pc = plrs[Random.Range(0, plrs.Count)];
            if (pc != null && pc.PlayerId != _lastAngelTarget) return pc;
        }

        return plrs[Random.Range(0, plrs.Count)];
    }
}
