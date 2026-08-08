using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace SkidMenu.features;

public static class KillAura
{
    public static bool Enabled = false;
    public static float Range = 2.2f;
    public static bool InfiniteRange = false;
    public static float FireRate = 0.05f;
    public static bool Telemurder = false;
    public static bool RespectMeeting = true;
    public static bool RespectVent = true;
    public static bool WaitAfterStart = true;
    public static float StartDelay = 8f;
    public static bool IgnoreCooldownAsHost = false;
    private static float _timer = 0f;
    public static float GameStartTime = -1f;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class KillAura_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            if (!Enabled || __instance != PlayerControl.LocalPlayer) return;
            if (__instance.Data == null || __instance.Data.IsDead) return;
            if (__instance.Data.RoleType != RoleTypes.Impostor &&
                __instance.Data.RoleType != RoleTypes.Shapeshifter &&
                __instance.Data.RoleType != RoleTypes.Phantom) return;
            if (RespectMeeting && MeetingHud.Instance != null) return;
            if (RespectVent && (__instance.inVent || __instance.onLadder)) return;
            if (WaitAfterStart && (GameStartTime < 0f || Time.time - GameStartTime < StartDelay)) return;
            bool skipCooldown = (IgnoreCooldownAsHost && AmongUsClient.Instance.AmHost) || CheatToggles.noKillCd;
            float cd = __instance.killTimer;
            if (!skipCooldown && cd > 0f) return;

            _timer += Time.fixedDeltaTime;
            if (_timer < FireRate) return;
            _timer = 0f;

            if (PlayerControl.AllPlayerControls == null) return;

            PlayerControl nearest = null;
            float nearestDist = float.MaxValue;
            Vector2 localPos = __instance.transform.position;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc == __instance || pc.Data == null) continue;
                if (pc.Data.Disconnected || pc.Data.IsDead || pc.inVent) continue;
                if (!CheatToggles.killOtherImpostors && RoleManager.IsImpostorRole(pc.Data.RoleType)) continue;
                float dist = Vector2.Distance(localPos, pc.transform.position);
                float threshold = InfiniteRange ? float.MaxValue : Range;
                if (dist <= threshold && dist < nearestDist) { nearestDist = dist; nearest = pc; }
            }

            if (nearest == null) return;

            if (skipCooldown)
                __instance.SetKillTimer(0f);

            if (Telemurder)
                __instance.StartCoroutine(PlayersTab.TeleMurder(nearest).WrapToIl2Cpp());
            else
                try { __instance.CmdCheckMurder(nearest); } catch { }

            if (skipCooldown)
                __instance.SetKillTimer(0f);
        }
    }

}
