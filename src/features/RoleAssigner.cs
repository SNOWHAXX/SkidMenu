using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppCollections = Il2CppSystem.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class RoleAssigner
{
    public static bool Enabled = false;
    public const RoleTypes Random = (RoleTypes)255;
    private static readonly Dictionary<byte, RoleTypes> _assignments = new();
    private static readonly RoleTypes[] _randomPool = new[] {
        RoleTypes.Impostor, RoleTypes.Shapeshifter, RoleTypes.Phantom,
        RoleTypes.Scientist, RoleTypes.Engineer, RoleTypes.Noisemaker,
        RoleTypes.Tracker, RoleTypes.GuardianAngel, RoleTypes.Judge
    };

    public static bool IsActive => Enabled && (AmongUsClient.Instance?.AmHost == true || CheatToggles.bypassHostOnly);
    public static void SetRole(byte pid, RoleTypes role) => _assignments[pid] = role;
    public static void ClearRole(byte pid) => _assignments.Remove(pid);
    public static void ClearAll() => _assignments.Clear();
    public static bool TryGetRole(byte pid, out RoleTypes role) => _assignments.TryGetValue(pid, out role);
    private static RoleTypes GetRandomRole() => _randomPool[UnityEngine.Random.Range(0, _randomPool.Length)];

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    static class AssignPatch
    {
        static bool Prefix()
        {
            if (!IsActive || _assignments.Count == 0) return true;
            try
            {
                var all = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p?.Data != null && !p.Data.Disconnected && !p.Data.IsDead)
                    .ToList();

                var forced   = new List<NetworkedPlayerInfo>();
                var unforced = new List<NetworkedPlayerInfo>();

                foreach (var pc in all)
                {
                    if (_assignments.ContainsKey(pc.PlayerId))
                        forced.Add(pc.Data);
                    else
                        unforced.Add(pc.Data);
                }

                var opts = GameOptionsManager.Instance.CurrentGameOptions;
                int impCount = Mathf.Clamp(opts.NumImpostors, 1, unforced.Count - 1);

                // Sort unforced into imp/crew based on game settings
                var rng  = new System.Random();
                var impsRaw = unforced.OrderBy(_ => rng.Next()).Take(impCount).ToList();
                var crewRaw = unforced.Where(p => !impsRaw.Contains(p)).ToList();

                var imps = new Il2CppCollections.List<NetworkedPlayerInfo>();
                foreach (var x in impsRaw) imps.Add(x);
                var crew = new Il2CppCollections.List<NetworkedPlayerInfo>();
                foreach (var x in crewRaw) crew.Add(x);

                if (imps.Count > 0)
                    GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(imps, opts, RoleTeamTypes.Impostor, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>());
                if (crew.Count > 0)
                    GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(crew, opts, RoleTeamTypes.Crewmate, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>(RoleTypes.Crewmate));

                // Apply forced roles
                foreach (var pc in all)
                {
                    if (!_assignments.TryGetValue(pc.PlayerId, out var role)) continue;
                    if (role == Random) role = GetRandomRole();
                    pc.RpcSetRole(role, false);
                }

                // Initialize all roles
                foreach (var pc in all)
                {
                    var roleBeh = pc.Data?.Role;
                    if (roleBeh != null) roleBeh.Initialize(pc);
                }

                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    static class ClearOnEnd
    {
        static void Postfix() => _assignments.Clear();
    }
}

