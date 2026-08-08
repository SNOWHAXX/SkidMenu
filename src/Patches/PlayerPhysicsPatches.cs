using System;
using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    private static GameObject[] _cachedBodyObjects = System.Array.Empty<GameObject>();
    private static DeadBody[]   _cachedDeadBodies  = System.Array.Empty<DeadBody>();
    private const float BodySearchInterval = 0.3f;

    public static void Postfix(PlayerPhysics __instance)
    {
        MalumESP.PlayerNametags(__instance);
        MalumESP.SeeGhostsCheat(__instance);

        if (__instance.AmOwner)
        {
            MalumCheats.NoClipCheat();
            MalumCheats.ProtectCheat();
            MalumCheats.KillAllCheat();
            MalumCheats.KillAllCrewCheat();
            MalumCheats.KillAllImpsCheat();
            MalumCheats.ForceStartGameCheat();
            MalumCheats.TeleportCursorCheat();
            MalumCheats.CompleteMyTasksCheat();
            MalumCheats.CompleteAllTasksCheat();
            MalumCheats.PlayAnimationCheat();
            MalumCheats.SpamPetCheat();
            MalumCheats.PlayScannerCheat();
            MalumCheats.RoomEntryCheat();

            MalumPPMCheats.EjectPlayerPPM();
            MalumPPMCheats.SpectatePPM();
            MalumPPMCheats.KillPlayerPPM();
            MalumPPMCheats.TelekillPlayerPPM();
            MalumPPMCheats.TeleportPlayerPPM();
            MalumPPMCheats.SetFakeRolePPM();
            MalumPPMCheats.SetFakeAlivePPM();

            MalumESP.InvalidateNametagCache();
            Utils.TickClientCache();
            Utils.TickTracerFrame();

            _cachedBodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
            _cachedDeadBodies  = new DeadBody[_cachedBodyObjects.Length];
            for (int i = 0; i < _cachedBodyObjects.Length; i++)
                _cachedDeadBodies[i] = _cachedBodyObjects[i]?.GetComponent<DeadBody>();

            ViperBodies.TickBodies(_cachedDeadBodies);

            if (VotekickHandler.VotekickAllEnabled) VotekickHandler.VotekickAll();
        }

        TracersHandler.DrawPlayerTracer(__instance);

        if (!__instance.AmOwner) return;

        foreach (DeadBody deadBody in _cachedDeadBodies)
        {
            if (!deadBody || !deadBody.gameObject) continue;

            TracersHandler.DrawBodyTracer(deadBody);

            if (!deadBody.gameObject.activeInHierarchy) continue;

            if (CheatToggles.autoReportBodies)
            {
                if (deadBody.Reported) continue;
                if (!ViperBodies.CanReport(deadBody)) continue;
                deadBody.Reported = true;
                PlayerControl.LocalPlayer.CmdReportDeadBody(GameData.Instance.GetPlayerById(deadBody.ParentId));
            }
        }

        try
        {
            if (CheatToggles.invertControls)
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
        } catch (NullReferenceException) { }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysics_HandleAnimation
{
    // Prefix patch of PlayerPhysics.HandleAnimation to disable walking animation
    public static bool Prefix(PlayerPhysics __instance)
    {
        if (CheatToggles.moonWalk && __instance.AmOwner)
        {
            __instance.ResetAnimState();

            return false;
        }

        return true;
    }
}
