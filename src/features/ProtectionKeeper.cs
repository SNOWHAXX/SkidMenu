using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features;

public class ProtectionKeeper : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (SkidMenu.isPanicked) return;
        if (!Visuals.ShowProtections.Enabled) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.Started) return;

        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.protectedByGuardianId < 0) continue;
            try
            {
                KeepProtection(pc);
            }
            catch { }
        }
    }

    private static void KeepProtection(PlayerControl pc)
    {
        if (!Visuals.ShowProtections.Monitor.TryGetValue(pc.PlayerId, out var entry)) return;

        var options = GameOptionsManager.Instance.CurrentGameOptions;
        float full = options.GetFloat(FloatOptionNames.ProtectionDurationSeconds);
        float remaining = full - (Time.time - entry.StartTime);
        if (remaining <= 0f) return;

        foreach (RoleEffectAnimation anim in pc.currentRoleAnimations)
        {
            if (anim != null && anim.effectType == RoleEffectAnimation.EffectType.ProtectLoop)
                return;
        }

        options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, remaining);
        try
        {
            pc.TurnOnProtection(Visuals.ShowProtections.Enabled, entry.ColorId, entry.GuardianId);
        }
        finally
        {
            options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, full);
        }
    }
}
