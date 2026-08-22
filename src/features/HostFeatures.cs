using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;

namespace SkidMenu.features;

public static class HostFeatures
{
    public static readonly RoleTypes[] ValidRoles = {
        RoleTypes.Crewmate, RoleTypes.Impostor, RoleTypes.Scientist,
        RoleTypes.Engineer, RoleTypes.GuardianAngel, RoleTypes.Shapeshifter,
        RoleTypes.Tracker, RoleTypes.Noisemaker, RoleTypes.Phantom, RoleTypes.Detective,
        RoleTypes.Judge
    };

    public static int selectedRoleIndex = 0;
    public static bool preGameRoleForce = false;
    public static int preGameImpCount = 2;
    public static readonly Dictionary<byte, RoleTypes> forcedPreGameRoles = new();

    public static void SetAllPlayersRole(RoleTypes role)
    {
        if (!AmongUsClient.Instance.AmHost || PlayerControl.AllPlayerControls == null) return;
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            if (pc?.Data != null && !pc.Data.Disconnected) pc.RpcSetRole(role, true);
    }

    public static void ForceEndImpDisconnect()
    {
        if (GameManager.Instance == null || !AmongUsClient.Instance.AmHost) return;
        GameManager.Instance.RpcEndGame((GameOverReason)5, false);
    }

    public static void KickAll()
    {
        if (!AmongUsClient.Instance.AmHost || PlayerControl.AllPlayerControls == null) return;
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            if (pc != null && !pc.AmOwner && pc.Data != null)
                AmongUsClient.Instance.KickPlayer(pc.OwnerId, false);
    }

    public static IEnumerator MassMorphCoroutine()
    {
        if (!AmongUsClient.Instance.AmHost || PlayerControl.AllPlayerControls == null) yield break;
        var origRoles = new Dictionary<byte, RoleTypes>();
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            if (pc?.Data != null && !pc.Data.Disconnected) { origRoles[pc.PlayerId] = pc.Data.RoleType; pc.RpcSetRole(RoleTypes.Shapeshifter, true); }
        yield return new WaitForSeconds(0.5f);
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            if (pc?.Data != null && !pc.Data.Disconnected) pc.RpcShapeshift(pc, true);
        yield return new WaitForSeconds(0.5f);
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            if (pc?.Data != null && !pc.Data.Disconnected && origRoles.TryGetValue(pc.PlayerId, out var r)) pc.RpcSetRole(r, true);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    static class NoTaskModePatch
    {
        static bool Prefix() => !CheatToggles.noTaskMode;
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
    static class NumberOptionIncreasePatch
    {
        static bool Prefix(NumberOption __instance)
        {
            try
            {
                if (!CheatToggles.noSettingLimit) return true;
                if (__instance.Title == StringNames.GameNumImpostors || __instance.Title == StringNames.GamePlayerSpeed) return true;
                __instance.Value += __instance.Increment;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                __instance.AdjustButtonsActiveState();
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
    static class NumberOptionDecreasePatch
    {
        static bool Prefix(NumberOption __instance)
        {
            try
            {
                if (!CheatToggles.noSettingLimit) return true;
                if (__instance.Title == StringNames.GameNumImpostors || __instance.Title == StringNames.GamePlayerSpeed) return true;
                __instance.Value -= __instance.Increment;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                __instance.AdjustButtonsActiveState();
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
    static class NumberOptionInitializePatch
    {
        static void Postfix(NumberOption __instance)
        {
            try
            {
                if (!CheatToggles.noSettingLimit) return;
                if (__instance.Title == StringNames.GameNumImpostors || __instance.Title == StringNames.GamePlayerSpeed) return;
                __instance.ValidRange = new FloatRange(-999f, 999f);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    static class NumImpostorsPatch
    {
        static bool Prefix(IGameOptions __instance, ref int __result)
        {
            try
            {
                if (!CheatToggles.noSettingLimit) return true;
                __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    static class PreGameRoleForcePatch
    {
        static bool Prefix()
        {
            if (!preGameRoleForce || !AmongUsClient.Instance.AmHost) return true;
            try
            {
                var all = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p?.Data != null && !p.Data.Disconnected && !p.Data.IsDead).ToList();
                int impCount = Mathf.Clamp(preGameImpCount, 1, all.Count - 1);
                var rng = new System.Random();
                var imps = all.OrderBy(_ => rng.Next()).Take(impCount).ToList();
                var crew = all.Where(p => !imps.Contains(p)).ToList();
                var impData = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
                foreach (var i in imps) impData.Add(i.Data);
                var crewData = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
                foreach (var c in crew) crewData.Add(c.Data);
                var opts = GameOptionsManager.Instance.CurrentGameOptions;
                GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(impData, opts, (RoleTeamTypes)1, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>());
                GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(crewData, opts, (RoleTeamTypes)0, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>((RoleTypes)0));
                foreach (var pc in all) pc.Data.Role?.Initialize(pc);
                return false;
            }
            catch { return true; }
        }
    }
}
