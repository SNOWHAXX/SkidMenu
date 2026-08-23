using HarmonyLib;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using UnityEngine;
using System;
using System.Security.Cryptography;
using InnerNet;
using System.Collections.Generic;

namespace SkidMenu;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetPlatformData))]
public static class Constants_GetPlatformData
{
    // Postfix patch of Constants.GetPlatformData to spoof the user's platform type
    public static void Postfix(ref PlatformSpecificData __result)
    {
        if (Utils.StringToPlatformType(SkidMenu.spoofPlatform, out Platforms? platformType))
        {
            __result = new PlatformSpecificData
            {
                Platform = (Platforms)platformType,
                PlatformName = Constants.GetPlatformName()
            };
        }
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
public static class GameData_HandleDisconnect
{
    public static HashSet<int> disconnectQueue = new();

    public static void Prefix(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (MeetingHud.Instance == null) return;

        MeetingHud_Update.votedPlayers.Remove(player.Data.PlayerId);
    }

    public static void Postfix(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        if (MeetingHud.Instance == null) return;

        try
        {
            foreach (var area in MeetingHud.Instance.playerStates)
            {
                if (area == null) continue;
                if (area.PlayerId != player.Data.PlayerId) continue;
                var pd = GameData.Instance?.GetPlayerById(area.PlayerId); if (pd != null) pd.Disconnected = true;
                break;
            }

            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
        catch { }
    }
}

[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
public static class FreeChatInputField_UpdateCharCount
{
    // Postfix patch of FreeChatInputField.UpdateCharCount to change how charCountText displays
    public static void Postfix(FreeChatInputField __instance)
    {
        // Only works if CheatToggles.longerMsgs is enabled
        if (!CheatToggles.longerMessages) return;

        // Update charCountText to account for longer characterLimit
        int length = __instance.textArea.text.Length;
        __instance.charCountText.SetText($"{length}/{__instance.textArea.characterLimit}");

        if (length < 90) // Under 75%
        {
            __instance.charCountText.color = Color.black;
        }
        else if (length < 120) // Under 100%
        {
            __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
        }
        else // Over or equal to 100%
        {
            __instance.charCountText.color = Color.red;
        }
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.AlignChildren))]
public static class ChatBubble_AlignChildren
{
    public static void Postfix(ChatBubble __instance)
	{
        MalumESP.ChatNametags(__instance);
    }
}

[HarmonyPatch(typeof(SystemInfo), nameof(SystemInfo.deviceUniqueIdentifier), MethodType.Getter)]
public static class SystemInfo_deviceUniqueIdentifier_Getter
{
    // Postfix patch of SystemInfo.deviceUniqueIdentifier Getter method.
    // Hide Device ID returns one persistent fake per session (not a new random
    // on every call, which would look suspicious). Spoof Device ID returns the
    // user-configured value and takes precedence.
    private static string _cachedFake;

    public static void Postfix(ref string __result)
    {
        if (CheatToggles.spoofDeviceId && !string.IsNullOrWhiteSpace(CheatToggles.spoofDeviceIdCustom))
        {
            __result = CheatToggles.spoofDeviceIdCustom;
            return;
        }
        if (!CheatToggles.hideDeviceId) return;

        if (_cachedFake == null)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            _cachedFake = BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        __result = _cachedFake;
    }
}

internal static class FpsCounter
{
    public static float accumulator = 0f;
    public static int sampleCount = 0;
    public static int cachedFps = 0;
    public static readonly float sampleInterval = 0.5f;
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    // Postfix patch of VersionShower.Start to show SkidMenu version
    public static VersionShower Instance;

    public static void Postfix(VersionShower __instance)
    {
        Instance = __instance;
    }

    public static void UpdateText()
    {
        if (Instance == null) return;
        if (SkidMenu.inStealthMode || SkidMenu.isPanicked) return;

        string animName = GradientText.Animate("SkidMenu");
        string animVer  = GradientText.Animate("V" + SkidMenu.hyperVersion, 1.0f);
        string glow     = "<color=#ffffff99><b>?</b></color>";
        string glowOpen = "<color=#ffffff99><b>?</b></color>";
        string versionLabel = $"{glowOpen} {animName} {animVer} {glow}";

        if (SkidMenu.supportedAU.Contains(Application.version))
        {
            Instance.text.text = $"<b>{versionLabel}</b> (AU V{Application.version})";
        }
        else if (SkidMenu.toleratedAU.Contains(Application.version))
        {
            Instance.text.text = $"<b>{versionLabel}</b> (<color=yellow>AU V{Application.version}</color>)";
        }
        else
        {
            Instance.text.text = $"<b>{versionLabel}</b> (<color=red>AU V{Application.version}</color>)";
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update_VersionAnim
{
    public static void Postfix()
    {
        VersionShower_Start.UpdateText();
    }
}

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    // Postfix patch of PingTracker.Update to show SkidMenu authors and colored ping text
    public static void Postfix(PingTracker __instance)
    {
        if (SkidMenu.inStealthMode)
        {
            __instance.text.alignment = TMPro.TextAlignmentOptions.TopLeft;

            return;
        }

        __instance.text.alignment = TMPro.TextAlignmentOptions.Center;

        int ping = Utils.GetPing();

        if (FpsCounter.sampleCount > 0)
            FpsCounter.accumulator += Time.unscaledDeltaTime;
        FpsCounter.sampleCount++;
        if (FpsCounter.accumulator >= FpsCounter.sampleInterval)
        {
            FpsCounter.cachedFps = Mathf.RoundToInt(FpsCounter.sampleCount / FpsCounter.accumulator);
            FpsCounter.accumulator = 0f;
            FpsCounter.sampleCount = 0;
        }

        string fpsText    = Utils.GetColoredFpsText(FpsCounter.cachedFps);
        string sep        = "<color=#ffffff99><size=70%>?</size></color>";
        string authorLine = $"<color=#ffffffE6><b>{GradientText.Animate("SkidMenu")} by {GradientText.Animate("SNOWHAXX", 1.5f)}</b></color>";
        string pingText   = Utils.GetColoredPingText($"Ping: {AmongUsClient.Instance.Ping} ms", AmongUsClient.Instance.Ping);
        string statsLine  = $"<color=#ffffffE6><b>{pingText} {sep} {fpsText}</b></color>";

        if (AmongUsClient.Instance.IsGameStarted)
        {
            __instance.aspectPosition.DistanceFromEdge = new Vector3(-0.21f, 0.50f, 0f);
            __instance.text.text = $"{authorLine} ~ {statsLine}";
            return;
        }

        __instance.text.text = $"{authorLine}\n{statsLine}";

    }
}

[HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.DoShow))]
public static class DisconnectPopup_DoShow
{
    // Postfix patch of DisconnectPopup.DoShow to copy lobby code to clipboard on disconnect
    public static void Postfix(DisconnectPopup __instance)
    {
        if (!CheatToggles.copyLobbyCodeOnDisconnect) return;

        GUIUtility.systemCopyBuffer = AmongUsClient_OnGameJoined.lastGameIdString;

        __instance.SetText(__instance._textArea.text + "\n\n<size=60%>Lobby code has been copied to the clipboard</size>");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class ForceCompleteAnyTask
{
    public static bool Prefix(PlayerControl __instance, uint idx)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!CheatToggles.impostorTasks && !CheatToggles.doAnyTask) return true;

        var tasks = __instance.Data?.Tasks;
        if (tasks != null)
        {
            foreach (var t in tasks)
                if (t.Id == idx) return true;
        }

        foreach (var task in __instance.myTasks)
        {
            if (task.Id != idx || task.IsComplete) continue;
            try { task.Complete(); } catch { }
            try { GameManager.Instance.CheckTaskCompletion(); } catch { }
            break;
        }

        return true;
    }
}

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
public static class PlayerBanData_BanMinutesLeft_Getter
{
    // Postfix patch of PlayerBanData.BanMinutesLeft Getter method to remove disconnect penalty
    public static void Postfix(PlayerBanData __instance, ref int __result)
    {
        if (!CheatToggles.avoidPenalties) return;

        __instance.BanPoints = 0f; // Removes all BanPoints
        __result = 0; // Removes all BanMinutes
    }
}

[HarmonyPatch(typeof(FullAccount), nameof(FullAccount.CanSetCustomName))]
public static class FullAccount_CanSetCustomName
{
    // Prefix patch of FullAccount.CanSetCustomName to allow the usage of custom names
    public static void Prefix(ref bool canSetName)
    {
        if (CheatToggles.unlockFeatures)
        {
            canSetName = true;
        }
    }
}

[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
public static class AccountManager_CanPlayOnline
{
    // Prefix patch of AccountManager.CanPlayOnline to allow online games
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
public static class InnerNetClient_JoinGame
{
    // Prefix patch of InnerNetClient.JoinGame to allow online games
    public static void Prefix()
    {
        if (CheatToggles.unlockFeatures)
        {
            DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
        }
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
public static class GameManager_CheckTaskCompletion
{
    // Prefix patch of GameManager.CheckTaskCompletion to prevent a running game from ending
    public static bool Prefix(ref bool __result)
    {
        if (!CheatToggles.noGameEnd && !features.FuckGame.Enabled) return true;

        __result = false;

        return false;
    }
}

[HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
public static class Mushroom_FixedUpdate
{
    public static void Postfix(Mushroom __instance)
    {
        MalumESP.SporeCloudVision(__instance);
    }
}

// Found here: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(DoorBreakerGame), nameof(DoorBreakerGame.Start))]
public static class DoorBreakerGame_Start
{
    // Prefix patch of DoorBreakerGame.Start to automatically open a door when the player interacts with it
    public static bool Prefix(DoorBreakerGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();

        return false;
    }
}

// Found here: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(DoorCardSwipeGame), nameof(DoorCardSwipeGame.Begin))]
public static class DoorCardSwipeGame_Begin
{
    // Prefix patch of DoorCardSwipeGame.Begin to automatically open a door when the player interacts with it
    public static bool Prefix(DoorCardSwipeGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();

        return false;
    }
}

// Found here: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(MushroomDoorSabotageMinigame), nameof(MushroomDoorSabotageMinigame.Begin))]
public static class MushroomDoorSabotageMinigame_Begin
{
    // Prefix patch of MushroomDoorSabotageMinigame.Begin to automatically open a door when the player interacts with it
    public static bool Prefix(MushroomDoorSabotageMinigame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        __instance.FixDoorAndCloseMinigame();

        return false;
    }
}

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class Console_CanUse
{
    public static void Prefix(Console __instance)
    {
        if ((CheatToggles.impostorTasks || CheatToggles.doAnyTask || CheatToggles.fakeTasks)
            && PlayerControl.LocalPlayer?.myTasks != null)
            __instance.AllowImpostor = true;
    }

    public static void Postfix(Console __instance, ref float __result, ref bool canUse, ref bool couldUse)
    {
        if (!CheatToggles.fakeTasks && !CheatToggles.doAnyTask) return;

        float distance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), __instance.transform.position);
        if (distance <= __instance.UsableDistance)
        {
            canUse    = true;
            couldUse  = true;
            __result  = distance;
        }
    }
}

[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class Console_Use
{
    public static readonly List<NormalPlayerTask> FakeInjected = new();

    public static void Prefix(Console __instance)
    {
        if (!CheatToggles.fakeTasks) return;
        var player = PlayerControl.LocalPlayer;
        if (player == null || ShipStatus.Instance == null) return;

        NormalPlayerTask[][] allArrays = {
            ShipStatus.Instance.CommonTasks,
            ShipStatus.Instance.LongTasks,
            ShipStatus.Instance.ShortTasks
        };

        foreach (var arr in allArrays)
        {
            if (arr == null) continue;
            foreach (var task in arr)
            {
                if (task == null) continue;
                bool alreadyOwned = false;
                foreach (var t in player.myTasks) if (t.TaskType == task.TaskType) { alreadyOwned = true; break; }
                if (alreadyOwned) continue;

                // match by checking if this console belongs to this task
                bool match = false;
                try
                {
                    var consoles = task.FindConsoles();
                    if (consoles != null)
                        foreach (var c in consoles)
                            if (c != null && c.ConsoleId == __instance.ConsoleId && c.TaskTypes != null && __instance.TaskTypes != null)
                            { match = true; break; }
                }
                catch { }

                if (!match) continue;

                uint nextId = 0;
                foreach (var t in player.myTasks) if (t.Id >= nextId) nextId = t.Id + 1;
                task.Id    = nextId;
                task.Owner = player;
                player.myTasks.Add(task);
                FakeInjected.Add(task);
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new System.Type[0])]
public static class Minigame_Close
{
    public static void Postfix()
    {
        if (Console_Use.FakeInjected.Count == 0) return;
        var player = PlayerControl.LocalPlayer;
        foreach (var task in Console_Use.FakeInjected)
        {
            try
            {
                if (player != null) player.myTasks.Remove(task);
                if (task != null) task.Owner = null;
            }
            catch { }
        }
        Console_Use.FakeInjected.Clear();
    }
}

[HarmonyPatch(typeof(IntroCutscene), "CoBegin")]
public static class IntroCutscene_CoBegin
{
    // Prefix patch of IntroCutscene.CoBegin to force the LocalPlayer's role to a specified role
    public static void Prefix()
    {
        if (!Utils.isHost || !CheatToggles.forcedRole.HasValue) return;

        var forcedRole = CheatToggles.forcedRole.Value;

        // If LocalPlayer already has the forced role, do nothing
        if (PlayerControl.LocalPlayer.Data.RoleType == forcedRole)
        {
            return;
        }

        // Find a player with the forced role to swap roles with
        PlayerControl roleSwapTarget = null;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.RoleType != forcedRole) continue;
            roleSwapTarget = player;
            break;
        }

        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, forcedRole);

        if (roleSwapTarget != null)
        {
            DestroyableSingleton<RoleManager>.Instance.SetRole(roleSwapTarget, PlayerControl.LocalPlayer.Data.RoleType);
        }
    }
}

// Found here: https://github.com/g0aty/SickoMenu/blob/main/hooks/LobbyBehaviour.cpp
[HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
public static class GameContainer_SetupGameInfo
{
    // Postfix patch of GameContainer.SetupGameInfo to show more information when finding a game:
    // host name (e.g. Astral), lobby code (e.g. KLHCEG), host platform (e.g. Epic), and lobby age in minutes (e.g. 4:20)
    public static void Postfix(GameContainer __instance)
    {
        if (!CheatToggles.seeLobbyInfo) return;

        // The Crewmate icon gets aligned properly with this
        const string separator = "<#0000>000000000000000</color>";

        var trueHostName = __instance.gameListing.TrueHostName;

        var age = __instance.gameListing.Age;
        var lobbyTime = $"Age: {age / 60}:{(age % 60 < 10 ? "0" : "")}{age % 60}";

        var platform = Utils.PlatformTypeToString(__instance.gameListing.Platform);

        // Sets the text of the capacity field to include the new information
        __instance.capacity.text = $"<size=40%>{separator}\n{trueHostName}\n{__instance.capacity.text}\n" +
                                   $"<#fb0>{GameCode.IntToGameName(__instance.gameListing.GameId)}</color>\n" +
                                   $"<#b0f>{platform}</color>\n{lobbyTime}\n{separator}</size>";
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
public static class BanMenu_SetVisible
{
    // Prefix patch of BanMenu.SetVisible to always show kick and ban buttons as host
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!Utils.isHost) return true;

        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;

        __instance.BanButton.gameObject.SetActive(true);
        __instance.KickButton.gameObject.SetActive(true);
        __instance.MenuButton.gameObject.SetActive(show);

        return false;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class IGameOptionsExtensions_GetAdjustedNumImpostors
{
    // Prefix patch of IGameOptionsExtensions.GetAdjustedNumImpostors to remove impostor limits
    public static bool Prefix(IGameOptions __instance, ref int __result)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;

        return false;
    }
}

[HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
public static class PlayerPurchasesData_GetPurchase
{
    // Postfix patch of PlayerPurchasesData.GetPurchase to unlock all cosmetics
    public static void Postfix(ref bool __result)
    {
        if (!CheatToggles.freeCosmetics) return;

        __result = true;
    }
}


[HarmonyPatch(typeof(ControllerHeldButtonBehaviour), nameof(ControllerHeldButtonBehaviour.Update))]
public static class ControllerHeldButtonBehaviour_Update_InstantPet
{
    public static void Prefix(ControllerHeldButtonBehaviour __instance)
    {
        if (!CheatToggles.instantPet) return;
        try
        {
            var tab = __instance.TargetActionButton;
            if (tab == null || tab.GetIl2CppType().Name != "PetButton") return;
            __instance.holdTimer = __instance.holdDuration;
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PetButton), nameof(PetButton.SetTarget))]
public static class PetButton_SetTarget_InstantPet
{
    public static void Postfix(PetButton __instance)
    {
        if (!CheatToggles.instantPet) return;
        foreach (var pb in __instance.GetComponentsInChildren<PassiveButton>(true))
        {
            pb.HoldToUse = false;
            pb.RepeatDuration = 0.01f;
        }
    }
}

[HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.SetGettingPet))]
public static class PetBehaviour_SetGettingPet
{
    public static void Postfix(PetBehaviour __instance, bool petting, UnityEngine.Vector2 petPos)
    {
        if (CheatToggles.instantPet && petting)
            __instance.StartPetAnim();
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetBodyType))]
public static class PlayerPhysics_SetBodyType
{
    public static void Prefix(PlayerPhysics __instance, ref PlayerBodyTypes bodyType)
    {
        if (!SelfTab.CustomBodyType) return;
        if (__instance != PlayerControl.LocalPlayer?.MyPhysics) return;
        if (bodyType == SelfTab.SelectedBodyType) return;
        bodyType = SelfTab.SelectedBodyType;
    }
}

[HarmonyPatch(typeof(LongBoiPlayerBody), "LateUpdate")]
public static class LongBoiPlayerBody_LateUpdate
{
    public static void Postfix(LongBoiPlayerBody __instance)
    {
        if (!SelfTab.CustomBodyType || SelfTab.SelectedBodyType != PlayerBodyTypes.Long) return;
        if (PlayerControl.LocalPlayer?.cosmetics?.GetLongBoi() != __instance) return;
        __instance.targetHeight = SelfTab.LongBodyHeight;
        __instance.skipNeckAnim = true;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHud_Close_BodyType
{
    public static void Postfix() => SelfTab._lastApplied = (PlayerBodyTypes)(-1);
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public static class ShipStatus_Start_BodyType
{
    public static void Postfix() => SelfTab._lastApplied = (PlayerBodyTypes)(-1);
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyBehaviour_Start_BodyType
{
    public static void Postfix() => SelfTab._lastApplied = (PlayerBodyTypes)(-1);
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), new[] { typeof(SystemTypes), typeof(PlayerControl), typeof(byte) })]
public static class SuppressVentLog
{
    private static readonly System.Collections.Generic.Dictionary<SystemTypes, float> _lastCall = new();

    public static bool Prefix(SystemTypes systemType)
    {
        if (systemType != SystemTypes.Ventilation) return true;
        float now = UnityEngine.Time.time;
        if (_lastCall.TryGetValue(systemType, out float last) && now - last < 0.5f) return false;
        _lastCall[systemType] = now;
        return true;
    }
}


