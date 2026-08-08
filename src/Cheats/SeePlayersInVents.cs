using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using InnerNet;
using AmongUs.GameOptions;

namespace SkidMenu;

public static class SeePlayersInVents
{
    public static bool Enabled     = false;
    public static bool SeePhantoms = false;

    private const float VentHideDelay = 0.8f;
    private const float VentRestoreWindow = 2f;
    private const float PhantomAlpha = 0.3f;
    private static readonly Dictionary<byte, float>            _ventEnterTime = new();
    private static readonly Dictionary<byte, SpriteRenderer[]> _rendererCache = new();
    private static readonly Dictionary<byte, float>            LastVentSeen = new();

    // ── vent players ──────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    static class VentFixedUpdatePatch
    {
        static void Postfix(PlayerPhysics __instance)
        {
            // fix 5: bail immediately when both features off
            if (!Enabled && !SeePhantoms) return;

            var pc = __instance?.myPlayer;
            if (pc == null || pc.AmOwner || pc.Data == null || pc.Data.IsDead) return;
            if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.Started) return;
            if (MeetingHud.Instance != null) return;

            bool inVent = pc.inVent;

            if (inVent)
            {
                LastVentSeen[pc.PlayerId] = Time.time;

                if (!_ventEnterTime.ContainsKey(pc.PlayerId))
                    _ventEnterTime[pc.PlayerId] = Time.time;

                float elapsed    = Time.time - _ventEnterTime[pc.PlayerId];
                bool animPlaying = elapsed < VentHideDelay;

                bool isPhantom = pc.Data.RoleType == RoleTypes.Phantom;

                if (Enabled || (SeePhantoms && isPhantom))
                {
                    pc.Visible = true;
                    SetBodyAlpha(pc, 0.3f);
                }
                else if (animPlaying)
                {
                    pc.Visible = true;
                }
            }
            else
            {
                if (_ventEnterTime.Remove(pc.PlayerId))
                {
                    pc.Visible = true;
                    if (SeePhantoms && pc.Data.RoleType == RoleTypes.Phantom && pc.shouldAppearInvisible)
                        SetPhantomAlpha(pc, PhantomAlpha);
                    else
                        SetBodyAlpha(pc, 1f);
                }

                if (LastVentSeen.TryGetValue(pc.PlayerId, out float lastSeen) && Time.time - lastSeen > VentRestoreWindow)
                    LastVentSeen.Remove(pc.PlayerId);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    static class ExitVentPatch
    {
        static void Postfix(PlayerPhysics __instance)
        {
            var pc = __instance?.myPlayer;
            if (pc == null) return;
            _ventEnterTime.Remove(pc.PlayerId);
            LastVentSeen.Remove(pc.PlayerId);
            if (SeePhantoms && pc.Data?.RoleType == RoleTypes.Phantom && pc.shouldAppearInvisible)
            {
                pc.Visible = true;
                SetPhantomAlpha(pc, PhantomAlpha);
                return;
            }
            pc.Visible = true;
            SetBodyAlpha(pc, 1f);
        }
    }

    // fix 6: cleanup on death
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    static class PlayerDiePatch
    {
        static void Postfix(PlayerControl __instance)
        {
            if (__instance == null || __instance.AmOwner) return;
            if (!_ventEnterTime.Remove(__instance.PlayerId)) return;
            _rendererCache.Remove(__instance.PlayerId);
            LastVentSeen.Remove(__instance.PlayerId);
            SetBodyAlpha(__instance, 1f);
        }
    }

    // fix 6: cleanup on voted out / disconnect
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    static class PlayerExiledPatch
    {
        static void Postfix(PlayerControl __instance)
        {
            if (__instance == null) return;
            _ventEnterTime.Remove(__instance.PlayerId);
            _rendererCache.Remove(__instance.PlayerId);
            LastVentSeen.Remove(__instance.PlayerId);
        }
    }

    // ── phantoms ─────────────────────────────────────────────────────────────

    // fix 2+3: merged into one patch with Priority.Last so we run after AU sets its own state
    [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.FixedUpdate))]
    [HarmonyPriority(Priority.Last)]
    static class PhantomFixedUpdatePatch
    {
        static void Postfix(PhantomRole __instance)
        {
            // fix 3: endless vanish handled first, before we read isInvisible
            if (CheatToggles.endlessVanishDuration && __instance.isInvisible)
                __instance.durationSecondsRemaining = float.MaxValue;
        }
    }

    // LateUpdate-phase enforcement, called from VentVisibilityKeeper. Runs after the game's own
    // animators and vent coroutines (CoEnterVent/CoExitVent, vanish/appear poofs) so our visibility
    // writes win the render race instead of being overwritten after FixedUpdate each frame.
    public static void EnforceVisibility()
    {
        if (!Enabled && !SeePhantoms) return;
        if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.Started) return;
        if (MeetingHud.Instance != null) return;

        if (Enabled || SeePhantoms)
        {
            foreach (var kvp in LastVentSeen)
            {
                var pd = GameData.Instance?.GetPlayerById(kvp.Key);
                if (pd?.Object == null) continue;
                var pc = pd.Object;
                if (pc.AmOwner || pc.Data == null || pc.Data.IsDead) continue;
                if (Time.time - kvp.Value > VentRestoreWindow) continue;

                bool isPhantom = pc.Data.RoleType == RoleTypes.Phantom;
                if (!Enabled && !(SeePhantoms && isPhantom)) continue;

                pc.Visible = true;
                SetBodyAlpha(pc, pc.inVent ? 0.3f : 1f);
            }
        }

        if (SeePhantoms)
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.AmOwner) continue;
                if (pc.Data?.RoleType != RoleTypes.Phantom) continue;
                PhantomRole pr = pc.Data.Role as PhantomRole;
                if (!pc.shouldAppearInvisible && (pr == null || pr.durationSecondsRemaining <= 0f)) continue;
                SetPhantomAlpha(pc, PhantomAlpha);
            }
        }
    }

    [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.MakePlayerVisible))]
    static class MakePlayerVisiblePatch
    {
        static void Postfix(PhantomRole __instance)
        {
            if (!SeePhantoms) return;
            PlayerControl pc = __instance.Player;
            if (pc == null) return;
            SetPhantomAlpha(pc, 1f);
        }
    }

    // fix 2: restore phantom alpha correctly when meeting ends
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    static class MeetingClosePatch
    {
        static void Postfix()
        {
            if (!SeePhantoms) return;
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.AmOwner) continue;
                if (pc.Data?.RoleType != RoleTypes.Phantom) continue;
                PhantomRole pr = pc.Data.Role as PhantomRole;
                bool stillInvisible = pr != null && pr.durationSecondsRemaining > 0f;
                if (stillInvisible)
                {
                    pc.shouldAppearInvisible = true;
                    SetPhantomAlpha(pc, PhantomAlpha);
                }
                else
                {
                    SetPhantomAlpha(pc, 1f);
                }
            }
        }
    }

    // ── shared cleanup ────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class MeetingStartPatch
    {
        static void Postfix()
        {
            foreach (var id in _ventEnterTime.Keys)
            {
                var pd = GameData.Instance?.GetPlayerById(id);
                if (pd?.Object != null)
                {
                    pd.Object.Visible = true;
                    SetBodyAlpha(pd.Object, 1f);
                }
            }
            _ventEnterTime.Clear();
            LastVentSeen.Clear();
            if (SeePhantoms)
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.AmOwner) continue;
                    if (pc.Data?.RoleType != RoleTypes.Phantom) continue;
                    SetPhantomAlpha(pc, 1f);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    static class PhantomDisconnectPatch
    {
        static void Postfix(PlayerControl player)
        {
            if (player == null) return;
            _ventEnterTime.Remove(player.PlayerId);
            _rendererCache.Remove(player.PlayerId);
            LastVentSeen.Remove(player.PlayerId);
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    static class ClearOnLobby
    {
        static void Postfix()
        {
            foreach (var id in _ventEnterTime.Keys)
            {
                var pd = GameData.Instance?.GetPlayerById(id);
                if (pd?.Object != null) SetBodyAlpha(pd.Object, 1f);
            }
            _ventEnterTime.Clear();
            _rendererCache.Clear();
            LastVentSeen.Clear();
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void SetPhantomAlpha(PlayerControl pc, float alpha)
    {
        try
        {
            pc.shouldAppearInvisible = false;
            pc.invisibilityAlpha = 1f;
            pc.Visible = alpha > 0f;
            pc.cosmetics.SetPhantomRoleAlpha(alpha);
            pc.cosmetics.SetForcedVisible(alpha < 1f);
            pc.cosmetics.isNameVisible = alpha > 0f;
            if (alpha > 0f && pc.cosmetics.nameText != null)
            {
                var nc = pc.cosmetics.nameText.color;
                nc.a = 1f;
                pc.cosmetics.nameText.color = nc;
            }
        }
        catch { }
    }

    // fix 4: direct SpriteRenderer iteration with per-player caching
    // no longer calls SetPhantomRoleAlpha on non-phantom players
    private static void SetBodyAlpha(PlayerControl pc, float alpha)
    {
        try
        {
            if (!_rendererCache.TryGetValue(pc.PlayerId, out var renderers) || renderers == null || renderers.Length == 0)
            {
                renderers = pc.GetComponentsInChildren<SpriteRenderer>(true);
                _rendererCache[pc.PlayerId] = renderers;
            }
            foreach (var sr in renderers)
            {
                if (sr == null) continue;
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            if (pc.cosmetics.nameText != null)
            {
                var nc = pc.cosmetics.nameText.color;
                nc.a = alpha;
                pc.cosmetics.nameText.color = nc;
            }
        }
        catch { }
    }
}

public class VentVisibilityKeeper : MonoBehaviour
{
    private void LateUpdate()
    {
        SeePlayersInVents.EnforceVisibility();
    }
}