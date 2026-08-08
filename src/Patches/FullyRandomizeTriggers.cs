using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class FullyRandomizeTriggers
{
    public static bool OnDeath          = false;
    public static bool OnKill           = false;
    public static bool OnMeetingStart   = false;
    public static bool OnMeetingEnd     = false;
    public static bool OnLobbyLeave     = false;
    public static bool OnGameEnd        = false;
    public static bool OnShapeshift     = false;
    public static bool OnShapeshiftBack = false;
    public static bool OnVent           = false;
    public static bool OnExitVent       = false;
    public static bool OnTaskComplete   = false;
    public static bool OnEjected        = false;
    public static bool OnSabotage       = false;
    public static bool OnVanish         = false;
    public static bool OnReappear       = false;
    public static bool OnVotekicked     = false;
    public static bool OnPlayerJoin     = false;
    public static bool OnPlayerLeave    = false;
    public static bool ShowNotification = true;

    private static float _spamTimer = 0f;

    public static void Fire(string reason)
    {
        if (SkidMenu.routines == null) return;
        SpoofingTab.DoFullyRandomize();
        if (ShowNotification) SkidMenu.notifications.Send("Auto-Randomized", reason, 4f);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Patch_SpamTick
    {
        public static void Postfix()
        {
            if (!SpoofingTab.frSpamEnabled) return;
            _spamTimer += Time.deltaTime;
            if (_spamTimer < SpoofingTab.frSpamDelay) return;
            _spamTimer = 0f;
            SpoofingTab.DoFullyRandomize();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static class Patch_OnDeath
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!OnDeath || !__instance.AmOwner) return;
            Fire("Randomized on death");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class Patch_OnKill
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!OnKill || !__instance.AmOwner) return;
            Fire("Randomized on kill");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class Patch_OnMeetingStart
    {
        public static void Postfix()
        {
            if (!OnMeetingStart || PlayerControl.LocalPlayer == null) return;
            Fire("Randomized on meeting start");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    public static class Patch_OnMeetingEnd
    {
        public static void Postfix()
        {
            if (!OnMeetingEnd || PlayerControl.LocalPlayer == null) return;
            Fire("Randomized on meeting end");
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    public static class Patch_OnLobbyLeave
    {
        public static void Prefix()
        {
            if (!OnLobbyLeave || PlayerControl.LocalPlayer == null) return;
            Fire("Randomized on leave");
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    public static class Patch_OnGameEnd
    {
        public static void Prefix()
        {
            if (!OnGameEnd || PlayerControl.LocalPlayer == null) return;
            Fire("Randomized on game end");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    public static class Patch_OnShapeshift
    {
        public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer)
        {
            if (!__instance.AmOwner || targetPlayer == null) return;
            bool isRevert = targetPlayer.PlayerId == __instance.PlayerId;
            if (!isRevert && OnShapeshift)     Fire("Randomized on shapeshift");
            if (isRevert  && OnShapeshiftBack) Fire("Randomized on shapeshift back");
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    public static class Patch_OnVent
    {
        public static void Postfix(PlayerControl pc)
        {
            if (!OnVent || pc == null || !pc.AmOwner) return;
            Fire("Randomized on vent enter");
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    public static class Patch_OnExitVent
    {
        public static void Postfix(PlayerControl pc)
        {
            if (!OnExitVent || pc == null || !pc.AmOwner) return;
            Fire("Randomized on vent exit");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcCompleteTask))]
    public static class Patch_OnTaskComplete
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!OnTaskComplete || !__instance.AmOwner) return;
            Fire("Randomized on task complete");
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public static class Patch_OnEjected
    {
        public static void Postfix(ExileController __instance)
        {
            if (!OnEjected || PlayerControl.LocalPlayer == null) return;
            try
            {
                var exiled = __instance.initData?.networkedPlayer;
                if (exiled == null || exiled.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
                Fire("Randomized on ejection");
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), new[] { typeof(SystemTypes), typeof(byte) })]
    public static class Patch_OnSabotage
    {
        public static void Postfix(SystemTypes systemType, byte amount)
        {
            if (!OnSabotage || PlayerControl.LocalPlayer == null) return;
            if ((amount & 128) == 0) return;
            Fire("Randomized on sabotage");
        }
    }

    [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.UseAbility))]
    public static class Patch_OnVanish
    {
        public static void Postfix(PhantomRole __instance)
        {
            if (!OnVanish || __instance?.Player == null || !__instance.Player.AmOwner) return;
            Fire("Randomized on vanish");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetInvisibility))]
    public static class Patch_OnReappear
    {
        public static void Postfix(PlayerControl __instance, bool isActive)
        {
            if (!OnReappear || !__instance.AmOwner || isActive) return;
            Fire("Randomized on reappear");
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    public static class Patch_OnVotekicked
    {
        public static void Postfix(int srcClient, int clientId)
        {
            if (!OnVotekicked || PlayerControl.LocalPlayer == null) return;
            if (clientId != AmongUsClient.Instance.ClientId) return;
            Fire("Randomized on votekick");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    public static class Patch_OnPlayerJoin
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!OnPlayerJoin || __instance == null || __instance.AmOwner) return;
            if (PlayerControl.LocalPlayer == null) return;
            Fire("Randomized on player join");
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    public static class Patch_OnPlayerLeave
    {
        public static void Prefix(PlayerControl player)
        {
            if (!OnPlayerLeave || player == null || player.AmOwner) return;
            Fire("Randomized on player leave");
        }
    }
}

