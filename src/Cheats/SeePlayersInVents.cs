using HarmonyLib;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    // CosmeticsLayer layout (v18, x64): bool "layers visible" lives at object offset 0x81.
    // The vanish path writes this byte DIRECTLY, bypassing SetBodyCosmeticsVisible -
    // confirmed by disassembling CosmeticsLayer.UpdateVisibility in GameAssembly.dll.
    private const int LayersVisibleOffset = 0x81;

    private static readonly Dictionary<byte, float> _ventEnterTime = new();
    private static readonly Dictionary<byte, float> LastVentSeen = new();
    private static readonly Dictionary<byte, bool>  _phantomVanished = new();
    private static bool _forcingLayers;

    // ── phantom layer forcing ─────────────────────────────────────────────────

    private static bool IsRemoteLivingPhantom(CosmeticsLayer cosmetics)
    {
        try
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.AmOwner || p.cosmetics != cosmetics) continue;
                if (p.Data == null || p.Data.IsDead) return false;
                return p.Data.RoleType == RoleTypes.Phantom;
            }
        }
        catch { }
        return false;
    }

    private static bool IsPhantomHidden(CosmeticsLayer cosmetics)
    {
        try
        {
            var pc = FindOwner(cosmetics);
            if (pc?.Data?.Role == null) return false;
            var pr = pc.Data.Role.TryCast<PhantomRole>();
            return pr != null && (pr.IsInvisible || pr.IsFading);
        }
        catch { }
        return false;
    }

    private static PlayerControl FindOwner(CosmeticsLayer cosmetics)
    {
        try
        {
            foreach (var p in PlayerControl.AllPlayerControls)
                if (p != null && p.cosmetics == cosmetics) return p;
        }
        catch { }
        return null;
    }

    // Flip the layer-visible byte back to true and re-run the game's own
    // layer applier, so every skin/hat/pet layer renders again.
    private static void ForceLayersVisible(CosmeticsLayer cosmetics)
    {
        if (_forcingLayers || cosmetics == null) return;
        try
        {
            _forcingLayers = true;
            Marshal.WriteByte(cosmetics.Pointer + LayersVisibleOffset, 1);
            cosmetics.UpdateVisibility();
            cosmetics.SetPhantomRoleAlpha(PhantomAlpha);
        }
        catch { }
        finally { _forcingLayers = false; }
    }

    // Runs right after the game's own visibility application - if it just hid a
    // phantom we care about, undo it inside the same call, no timing race.
    [HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.UpdateVisibility))]
    static class PhantomUpdateVisibilityPatch
    {
        static void Postfix(CosmeticsLayer __instance)
        {
            if (!SeePhantoms || _forcingLayers) return;
            if (!IsRemoteLivingPhantom(__instance)) return;
            if (!IsPhantomHidden(__instance)) return;
            ForceLayersVisible(__instance);
        }
    }

    // ── vent players ──────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    static class VentFixedUpdatePatch
    {
        static void Postfix(PlayerPhysics __instance)
        {
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
                    pc.cosmetics?.SetPhantomRoleAlpha(PhantomAlpha);
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
                    pc.cosmetics?.SetPhantomRoleAlpha(1f);
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
            pc.cosmetics?.SetPhantomRoleAlpha(1f);
        }
    }

    // ── phantoms ─────────────────────────────────────────────────────────────

    // Endless vanish support: keep the duration timer pinned while invisible.
    [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.FixedUpdate))]
    [HarmonyPriority(Priority.Last)]
    static class PhantomFixedUpdatePatch
    {
        static void Postfix(PhantomRole __instance)
        {
            if (CheatToggles.endlessVanishDuration && __instance.isInvisible)
                __instance.durationSecondsRemaining = float.MaxValue;
        }
    }

    // See Phantoms: the vanish animation runs untouched, but when the game tries to
    // apply the FINAL hide to a remote living phantom, we flip the argument back -
    // nothing gets disabled, so the 0.3 alpha pin in EnforceVisibility sticks.
    [HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetBodyCosmeticsVisible))]
    static class PhantomHideBlockPatch
    {
        static void Prefix(CosmeticsLayer __instance, ref bool b)
        {
            if (!SeePhantoms || b) return;
            try
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.AmOwner || p.cosmetics != __instance) continue;
                    if (p.Data?.RoleType != RoleTypes.Phantom) return;
                    if (!p.Data.IsDead) b = true;
                    return;
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetForcedVisible))]
    static class PhantomForcedVisiblePatch
    {
        static void Prefix(CosmeticsLayer __instance, ref bool isVisible)
        {
            if (!SeePhantoms || isVisible) return;
            try
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || p.AmOwner || p.cosmetics != __instance) continue;
                    if (p.Data?.RoleType != RoleTypes.Phantom) return;
                    if (!p.Data.IsDead) isVisible = true;
                    return;
                }
            }
            catch { }
        }
    }

    private static void FireVanish(PlayerControl pc)
    {
        try
        {
            if (CheatToggles.notifPhantom && !NotifHelper.Skip(pc, 5))
                SkidMenu.notifications.Send("<color=#8B0000>👻 Phantom</color>",
                    $"{(pc.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(pc))} vanished{NotifHelper.Room(pc)}{NotifHelper.Dist(pc)}", 3f);

            if (CheatToggles.logPhantomVanish)
                ConsoleUI.Log($"{ConsoleHelper.Fmt(pc)} <color=#cc66ff>vanished (Phantom)</color>{ConsoleHelper.Room(pc)}", "CC66FF");

            PlayerTracker.PhantomVanished(pc);
        }
        catch { }
    }

    private static void FireReappear(PlayerControl pc)
    {
        try
        {
            if (CheatToggles.notifPhantomReappear && !NotifHelper.Skip(pc, 16))
                SkidMenu.notifications.Send("<color=#cc88ff>👻 Reappear</color>",
                    $"{(pc.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(pc))} reappeared{NotifHelper.Room(pc)}{NotifHelper.Dist(pc)}", 3f);

            if (CheatToggles.logPhantomReappear)
                ConsoleUI.Log($"{ConsoleHelper.Fmt(pc)} <color=#dd99ff>reappeared (Phantom)</color>{ConsoleHelper.Room(pc)}", "DD99FF");

            PlayerTracker.PhantomReappeared(pc);
        }
        catch { }
    }

    // ── late-phase vent enforcement ───────────────────────────────────────────

    // Called from VentVisibilityKeeper each LateUpdate so vent visibility writes win
    // over the game's own vent coroutines before rendering.
    public static void EnforceVisibility()
    {
        if (!Enabled && !SeePhantoms) return;
        if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.Started) return;
        if (MeetingHud.Instance != null) return;

        if (Enabled)
        {
            foreach (var kvp in LastVentSeen)
            {
                var pd = GameData.Instance?.GetPlayerById(kvp.Key);
                if (pd?.Object == null) continue;
                var pc = pd.Object;
                if (pc.AmOwner || pc.Data == null || pc.Data.IsDead) continue;
                if (Time.time - kvp.Value > VentRestoreWindow) continue;

                pc.Visible = true;
                pc.cosmetics?.SetPhantomRoleAlpha(pc.inVent ? PhantomAlpha : 1f);
            }
        }

        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.AmOwner || pc.Data?.Role == null) continue;
            if (pc.Data.RoleType != RoleTypes.Phantom) continue;

            try
            {
                var pr = pc.Data.Role.TryCast<PhantomRole>();
                if (pr == null) continue;

                byte id = pc.PlayerId;
                _phantomVanished.TryGetValue(id, out bool wasVanished);
                bool nowVanished = pr.IsInvisible;

                if (nowVanished && !wasVanished)
                {
                    _phantomVanished[id] = true;
                    FireVanish(pc);
                }
                else if (!nowVanished && wasVanished)
                {
                    _phantomVanished.Remove(id);
                    FireReappear(pc);
                }

                if (!SeePhantoms) continue;

                float target = (nowVanished || pr.IsFading) ? PhantomAlpha : 1f;
                pc.Visible = true;

                try
                {
                    if (target < 1f)
                    {
                        ForceLayersVisible(pc.cosmetics);
                    }
                    else
                    {
                        pc.cosmetics?.SetPhantomRoleAlpha(1f);
                    }
                }
                catch { }
            }
            catch { }
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
                    pd.Object.cosmetics?.SetPhantomRoleAlpha(1f);
                }
            }
            _ventEnterTime.Clear();
            LastVentSeen.Clear();
            _phantomVanished.Clear();
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    static class PhantomDisconnectPatch
    {
        static void Postfix(PlayerControl player)
        {
            if (player == null) return;
            _ventEnterTime.Remove(player.PlayerId);
            LastVentSeen.Remove(player.PlayerId);
            _phantomVanished.Remove(player.PlayerId);
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
                if (pd?.Object != null) pd.Object.cosmetics?.SetPhantomRoleAlpha(1f);
            }
            _ventEnterTime.Clear();
            LastVentSeen.Clear();
            _phantomVanished.Clear();
        }
    }
}

public class VentVisibilityKeeper : MonoBehaviour
{
    private void LateUpdate()
    {
        SeePlayersInVents.EnforceVisibility();
    }
}
