using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using AmongUs.GameOptions;
using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public struct CheatToggles
{
    // Movement
    public static bool noClip;
    public static bool lagCompEnabled;
    public static bool lagCompFreeze;
    public static bool lagCompJitter;
    public static bool teleportPlayer;
    public static bool teleportCursor;
    public static bool invertControls;

    // Roles
    public static bool setFakeRole;
    public static bool setFakeAlive;
    public static bool noKillCd;
    public static bool showTasksMenu;
    public static bool bypassVisualTasks;
    public static float menuOpacity = 1f;
    public static float menuScaleH = 100f;
    public static float menuScaleV = 100f;
    public static bool  maxFpsEnabled = true;
    public static int   maxFpsValue   = 240;
    public static bool  stretchedResEnabled = false;
    public static float stretchedResW       = 1280f;
    public static float stretchedResH       = 720f;
    public static float consoleScaleH = 100f;
    public static float consoleScaleV = 100f;
    public static float chatScaleH    = 100f;
    public static float chatScaleV    = 100f;
    public static float doorsScaleH   = 100f;
    public static float doorsScaleV   = 100f;
    public static float tasksScaleH   = 100f;
    public static float tasksScaleV   = 100f;
    public static float protectScaleH = 100f;
    public static float protectScaleV = 100f;
    public static bool completeMyTasks;
    public static bool completeAllTasks;
    public static bool impostorTasks;
    public static bool instantPet;
    public static bool spamPet;
    public static float spamPetDelay = 0.25f;
    public static bool killReach;
    public static float killReachRange = 5f;
    public static bool killReachInfinite = true;
    public static bool killAnyone;
    public static bool killOtherImpostors = false;
    public static bool endlessSsDuration;
    public static bool endlessBattery;
    public static bool endlessTracking;
    public static bool noTrackingCooldown;
    public static bool noTrackingDelay;
    public static bool trackReach;
    public static bool interrogateReach;
    public static bool gaInfiniteRange;
    public static bool gaIgnoreImpostors;
    public static bool noVitalsCooldown;
    public static bool noVentCooldown;
    public static bool endlessVentTime;
    public static bool endlessVanish;
    public static bool killVanished;
    public static bool noVanishAnim;
    public static bool noShapeshiftAnim;
    public static bool noShapeshiftCooldown;
    public static bool noVanishCooldown;
    public static bool endlessVanishDuration;

    // ESP
    public static bool noShadows;
    public static bool seeGhosts;
    public static bool seeRoles;
    public static bool seePlayerInfo;
    public static bool seeDisguises;
    public static bool taskArrows;
    public static bool revealVotes;
    public static bool seeLobbyInfo;

    // Notifications
    public static bool notifKill;
    public static bool notifShapeshift;
    public static bool notifShapeshiftRevert;
    public static bool notifVent;
    public static bool notifExitVent;
    public static bool notifPhantom;
    public static bool notifPhantomReappear;
    public static bool notifTask;
    public static bool notifMeeting;
    public static bool notifBodyReport;
    public static bool notifVote;
    public static bool notifVotekick;
    public static bool notifSabotage;
    public static bool notifDisconnect;
    public static bool notifChat;
    public static bool notifRoleAssign;
    public static bool notifJoin;
    public static bool notifGuardianProtect;
    public static bool notifKillAttempt;
    public static bool notifEjections;
    public static bool notifVerdict;
    public static bool notifSabotageFix;
    public static bool notifGameOver;
    public static bool notifCameras;
    public static bool notifRoomEntry;
    public static bool notifShowRoom;
    public static bool notifShowTaskCount;
    public static bool notifShowDistance;
    // index: 0=Kill 1=Sab 2=Vent 3=ExitVent 4=Shift 5=Phantom 6=Meeting 7=BodyReport 8=Vote 9=Votekick 10=Chat 11=Disconnect 12=RoleAssign 13=Task 14=Join 15=ShiftRevert 16=PhantomReappear 17=GuardianProtect 18=KillAttempt 19=Ejection 20=SabFix 21=GameOver 22=Verdict
    public static bool[] notifExSelf = new bool[23];
    public static bool[] notifExHost = new bool[23];
    public static bool espShowRole;
    public static bool espKillCooldown;
    public static bool espTasks;

    // ESP sub-options — Player Info
    public static bool espShowPlayerInfo;
    public static bool espFriendCode;
    public static bool espPuid;
    public static bool espDeviceId;
    public static bool espModUser;
    public static bool espIsHost;
    public static bool espLevel;
    public static bool espPlatform;
    public static bool espVotekicks;
    // Privacy — device ID and telemetry
    public static bool hideDeviceId       = true;
    public static bool spoofDeviceId      = false;
    public static string spoofDeviceIdCustom = "";
    public static bool disableTelemetry   = true;
    public static bool spoofTelemetry     = false;

    // Camera
    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    // Minimap
    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap;
    public static bool distanceBasedMap;
    public static bool simpleRoleBasedMap;

    // Tracers
    public static bool tracersImps;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies;
    public static bool colorBasedTracers;
    public static bool distanceBasedTracers;
    public static bool simpleRoleBasedTracers;

    // Chat
    public static bool enableChat;
    public static bool copyMessage = false;
    public static bool unlockCharacters;
    public static bool bypassUrlBlock;
    public static bool longerMessages;
    public static bool unlockClipboard;
    public static bool lowerRateLimits;

    // Ship
    public static bool closeMeeting;
    public static bool autoOpenDoorsOnUse;
    public static bool unfixableLights;
    public static bool callMeeting;
    public static bool reportBody;
    public static bool autoReportBodies;
    public static bool kickOffensiveNames;
    public static bool fakeTasks;
    public static bool doAnyTask;

    // Sabotage
    public static bool commsSab;
    public static bool elecSab;
    public static bool reactorSab;
    public static bool oxygenSab;
    public static bool mushSab;
    public static bool mushSpore;
    public static bool showDoorsMenu;
    public static bool openAllDoors;
    public static bool closeAllDoors;
    public static bool spamOpenAllDoors;
    public static bool spamCloseAllDoors;
    public static bool spamSabotageAll;
    public static bool spamFixAll;
    public static bool sabotageMap;

    // Vents
    public static bool unlockVents;
    public static bool walkInVents;
    public static bool kickVents;

    // Animations
    public static bool animShields;
    public static bool animAsteroids;
    public static bool animEmptyGarbage;
    public static bool animMedScan;
    public static bool animCamsInUse;
    public static bool animPet;
    public static bool moonWalk;

    // Console
    public static bool showConsole;
    public static bool showChatUI;
    public static bool logDeaths;
    public static bool logShapeshiftInto;
    public static bool logShapeshiftRevert;
    public static bool logVentIn;
    public static bool logVentOut;
    public static bool logSabotages;
    public static bool logMeetingCalled;
    public static bool logBodyReport;
    public static bool logEjections;
    public static bool logVerdict;
    public static bool logVotes;
    public static bool logVotekicks;
    public static bool logChat;
    public static bool logDisconnects;
    public static bool logJoins;
    public static bool logPhantomVanish;
    public static bool logPhantomReappear;
    public static bool logTaskCompleted;
    public static bool logGuardianProtect;
    public static bool logKillAttempt;
    public static bool logSabotageFix;
    public static bool logGameOver;
    public static bool logCameras;
    public static bool logRoomEntry;
    public static int maxLogEntries = 300;
    public static int chatMaxEntries = 300;

    // Dating Shit
    public static bool findDaters;
    public static bool extendedLobbyList;

    // Host-Only
    public static bool voteImmune;
    public static bool judgeImmune;
    public static bool forceRole;
    public static RoleTypes? forcedRole;
    public static bool showRolesMenu;
    public static bool skipMeeting;
    public static bool forceStartGame;
    public static bool noGameEnd;
    public static bool showProtectMenu;
    public static bool noOptionsLimits;
    public static bool ejectPlayer;
    public static bool killPlayer;
    public static bool telekillPlayer;
    public static bool killAll;
    public static bool killAllCrew;
    public static bool killAllImps;
    public static bool bypassHostOnly;
    public static bool noTaskMode;
    public static bool noSettingLimit;
    public static bool killAura;

    // Passive
    public static bool antiOverload;
    public static bool unlockFeatures = true;
    public static bool freeCosmetics = true;
    public static bool avoidPenalties = true;
    public static bool copyLobbyCodeOnDisconnect;
    public static bool spoofAprilFoolsDate;
    public static bool randomizeCosmetics;

    // Modes
    public static bool rgbMode;
    public static bool stealthMode;
    public static bool panicMode;
    public static bool streamerMode;

    // Config
    public static bool reloadConfig;
    public static bool openConfig;
    public static bool loadProfile;
    public static bool saveProfile;

    // Keybind Map: Toggle Name -> KeyCode (KeyCode.None == No Key)
    public static readonly Dictionary<string, KeyCode> Keybinds = new();

    // Map for Reflection Access: Toggle Name -> FieldInfo
    public static readonly Dictionary<string, FieldInfo> ToggleFields = new();

    static CheatToggles()
    {
        var fields = typeof(CheatToggles).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.FieldType != typeof(bool)) continue;

            ToggleFields[field.Name] = field;
            Keybinds[field.Name] = KeyCode.None;
        }
    }

    public static void DisablePPMCheats(string variableToKeep)
    {
        ejectPlayer = variableToKeep == "ejectPlayer" && ejectPlayer;
        reportBody = variableToKeep == "reportBody" && reportBody;
        killPlayer = variableToKeep == "killPlayer" && killPlayer;
        telekillPlayer = variableToKeep == "telekillPlayer" && telekillPlayer;
        spectate = variableToKeep == "spectate" && spectate;
        setFakeRole = variableToKeep == "setFakeRole" && setFakeRole;
        setFakeAlive = variableToKeep == "setFakeAlive" && setFakeAlive;
        forceRole = variableToKeep == "forceRole" && forceRole;
        teleportPlayer = variableToKeep == "teleportPlayer" && teleportPlayer;
    }

    public static bool ShouldPPMClose()
    {
        return !setFakeRole && !setFakeAlive && !forceRole && !ejectPlayer && !reportBody && !telekillPlayer && !killPlayer && !spectate && !teleportPlayer;
    }

    public static void DisableAll()
    {
        foreach (var field in ToggleFields.Values)
        {
            field.SetValue(null, false);
        }
    }

    public static void SaveTogglesToProfile()
    {
        using var writer = new StreamWriter(SkidMenu.ProfilePath);

        writer.WriteLine("# MalumProfile");
        writer.WriteLine("# Format: ToggleName = Value = KeyCode.KEY");
        writer.WriteLine("# - List of supported keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");
        writer.WriteLine("# - Setting a keybind is optional. Use KeyCode.None to not set a keybind");
        writer.WriteLine("# - Multiple toggles may have the same key, but multiple keys per toggle are NOT supported");
        writer.WriteLine("# - Keybinds are only applied after loading this profile by pressing 'Load from Profile' in the Config menu");
        writer.WriteLine();

        foreach (var field in ToggleFields.Values)
        {
            Keybinds.TryGetValue(field.Name, out var key);
            writer.WriteLine($"{field.Name} = {field.GetValue(null)} = KeyCode.{key}");
        }

        writer.WriteLine($"Self.AlwaysShowTaskAnimations = {Self.AlwaysShowTaskAnimations} = KeyCode.None");
        writer.WriteLine($"Self.NoLadderCooldown = {Self.NoLadderCooldown.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.NoZiplineCooldown = {Self.NoZiplineCooldown.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.VoteAnyone = {Self.VoteAnywhere.VoteAnyone} = KeyCode.None");
        writer.WriteLine($"Self.VoteBeforeVotingStarts = {Self.VoteAnywhere.VoteBeforeVotingStarts} = KeyCode.None");
        writer.WriteLine($"Self.InstantVote = {Self.VoteAnywhere.InstantVote} = KeyCode.None");
        writer.WriteLine($"Self.UnlimitedMeetings = {Self.UnlimitedMeetings.enabled} = KeyCode.None");
        writer.WriteLine($"Self.UpdateStatsFreeplay = {Self.UpdateStatsFreeplay.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.PlayerSpeedMultiplier = {Self.PlayerSpeedModifier.Multiplier.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Self.PlayerSpeedModifierEnabled = {Self.PlayerSpeedModifier.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.CurrentSpeedChanger = {Self.CurrentSpeedChanger.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.CurrentSpeedChangerValue = {Self.CurrentSpeedChanger.Speed.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Self.ColorSniper = {features.ColorSniper.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.ColorSniperLobbyOnly = {features.ColorSniper.InLobbyOnly} = KeyCode.None");
        writer.WriteLine($"Self.ColorSniperTarget = {features.ColorSniper.TargetColor} = KeyCode.None");
        writer.WriteLine($"Immortality = {Immortality.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.SelectedColor = {SelfTab.SelectedColor} = KeyCode.None");
        writer.WriteLine($"Self.ImmortalDisableNotification = {features.Immortality.DisableNotification} = KeyCode.None");
        writer.WriteLine($"Sabotage.UpdateSystemsDirectly = {Sabotage.UpdateSystemsDirectly} = KeyCode.None");
        writer.WriteLine($"Roles.SabotageAsCrewmate = {Roles.SkipSabotageChecks.SabotageAsCrewmate} = KeyCode.None");
        writer.WriteLine($"Roles.SabotageInVents = {Roles.SkipSabotageChecks.SabotageInVents} = KeyCode.None");
    
        writer.WriteLine($"NameSpoofer.Enabled = {features.NameSpoofer.Enabled} = KeyCode.None");
        writer.WriteLine($"NameSpoofer.SpoofedName = {features.NameSpoofer.SpoofedName} = KeyCode.None");
        writer.WriteLine($"NameSpoofer.Mode = {(int)features.NameSpoofer.Mode} = KeyCode.None");
        writer.WriteLine($"NameSpoofer.RandomLength = {features.NameSpoofer.RandomLength} = KeyCode.None");
        writer.WriteLine($"Votekick.ShowVotekickInfo = {VotekickHandler.ShowVotekickInfo} = KeyCode.None");
        writer.WriteLine($"Votekick.VotekickAllEnabled = {VotekickHandler.VotekickAllEnabled} = KeyCode.None");
        writer.WriteLine($"Votekick.AutoPurgeImpostors = {VotekickHandler.AutoPurgeImpostors} = KeyCode.None");
        writer.WriteLine($"Votekick.AutoPurgeCrew = {VotekickHandler.AutoPurgeCrew} = KeyCode.None");
        writer.WriteLine($"Votekick.AutoPurgeHost = {VotekickHandler.AutoPurgeHost} = KeyCode.None");
        writer.WriteLine($"Votekick.AutoRetaliate = {VotekickHandler.AutoRetaliate} = KeyCode.None");
        writer.WriteLine($"Votekick.FinishTheKick = {VotekickHandler.FinishTheKick} = KeyCode.None");
        writer.WriteLine($"Votekick.VoteCount = {VotekickHandler.VoteCount} = KeyCode.None");
        writer.WriteLine($"Votekick.AutoKickInterval = {VotekickHandler.AutoKickInterval.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Notif.ExSelf = {string.Join(",", notifExSelf)} = KeyCode.None");
        writer.WriteLine($"Notif.ExHost = {string.Join(",", notifExHost)} = KeyCode.None");
        writer.WriteLine($"Notif.Join = {notifJoin} = KeyCode.None");
        writer.WriteLine($"Notif.ShowDistance = {notifShowDistance} = KeyCode.None");
        writer.WriteLine($"Votekick.NotifyVotekickInfo = {VotekickHandler.NotifyVotekickInfo} = KeyCode.None");
        writer.WriteLine($"Votekick.IgnoreOwnVotekicks = {VotekickHandler.IgnoreOwnVotekicks} = KeyCode.None");
        writer.WriteLine($"FindDaters.UseImpostorFilter = {FindDatersLobbyPatch.useImpostorFilter} = KeyCode.None");
        writer.WriteLine($"FindDaters.ImpostorCount = {FindDatersLobbyPatch.impostorCount} = KeyCode.None");
        writer.WriteLine($"FindDaters.UsePlayerFilter = {FindDatersLobbyPatch.usePlayerFilter} = KeyCode.None");
        writer.WriteLine($"FindDaters.MinPlayers = {FindDatersLobbyPatch.minPlayers} = KeyCode.None");
        writer.WriteLine($"FindDaters.MaxPlayers = {FindDatersLobbyPatch.maxPlayers} = KeyCode.None");
        writer.WriteLine($"FindDaters.UseChatFilter = {FindDatersLobbyPatch.useChatFilter} = KeyCode.None");
        writer.WriteLine($"ExtendedList.ExtraSlots = {ExtendedLobbyListPatch.extraSlots} = KeyCode.None");
        writer.WriteLine($"FR.RandLevel = {SkidMenu.frRandLevel} = KeyCode.None");
        writer.WriteLine($"FR.SpamEnabled = {SpoofingTab.frSpamEnabled} = KeyCode.None");
        writer.WriteLine($"FR.SpamDelay = {SpoofingTab.frSpamDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"FR.ShowNotification = {FullyRandomizeTriggers.ShowNotification} = KeyCode.None");
        writer.WriteLine($"FR.RandPlatform = {SkidMenu.frRandPlatform} = KeyCode.None");
        writer.WriteLine($"FR.RandName = {SkidMenu.frRandName} = KeyCode.None");
        writer.WriteLine($"FR.RandHat = {SkidMenu.frRandHat} = KeyCode.None");
        writer.WriteLine($"FR.RandSkin = {SkidMenu.frRandSkin} = KeyCode.None");
        writer.WriteLine($"FR.RandVisor = {SkidMenu.frRandVisor} = KeyCode.None");
        writer.WriteLine($"FR.RandPet = {SkidMenu.frRandPet} = KeyCode.None");
        writer.WriteLine($"FR.RandNameplate = {SkidMenu.frRandNameplate} = KeyCode.None");
        writer.WriteLine($"FR.RandColor = {SkidMenu.frRandColor} = KeyCode.None");
        writer.WriteLine($"FR.RpcDelay = {SpoofingTab.frRpcDelayTemp.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"FR.OnDeath = {FullyRandomizeTriggers.OnDeath} = KeyCode.None");
        writer.WriteLine($"FR.OnKill = {FullyRandomizeTriggers.OnKill} = KeyCode.None");
        writer.WriteLine($"FR.OnMeetingStart = {FullyRandomizeTriggers.OnMeetingStart} = KeyCode.None");
        writer.WriteLine($"FR.OnMeetingEnd = {FullyRandomizeTriggers.OnMeetingEnd} = KeyCode.None");
        writer.WriteLine($"FR.OnLobbyLeave = {FullyRandomizeTriggers.OnLobbyLeave} = KeyCode.None");
        writer.WriteLine($"FR.OnGameEnd = {FullyRandomizeTriggers.OnGameEnd} = KeyCode.None");
        writer.WriteLine($"FR.OnShapeshift = {FullyRandomizeTriggers.OnShapeshift} = KeyCode.None");
        writer.WriteLine($"FR.OnVent = {FullyRandomizeTriggers.OnVent} = KeyCode.None");
        writer.WriteLine($"FR.OnTaskComplete = {FullyRandomizeTriggers.OnTaskComplete} = KeyCode.None");
        writer.WriteLine($"FR.OnEjected = {FullyRandomizeTriggers.OnEjected} = KeyCode.None");
        writer.WriteLine($"FR.OnSabotage = {FullyRandomizeTriggers.OnSabotage} = KeyCode.None");
        writer.WriteLine($"FR.OnExitVent = {FullyRandomizeTriggers.OnExitVent} = KeyCode.None");
        writer.WriteLine($"FR.OnShapeshiftBack = {FullyRandomizeTriggers.OnShapeshiftBack} = KeyCode.None");
        writer.WriteLine($"FR.OnVanish = {FullyRandomizeTriggers.OnVanish} = KeyCode.None");
        writer.WriteLine($"FR.OnReappear = {FullyRandomizeTriggers.OnReappear} = KeyCode.None");
        writer.WriteLine($"FR.OnVotekicked = {FullyRandomizeTriggers.OnVotekicked} = KeyCode.None");
        writer.WriteLine($"FR.OnPlayerJoin = {FullyRandomizeTriggers.OnPlayerJoin} = KeyCode.None");
        writer.WriteLine($"FR.OnPlayerLeave = {FullyRandomizeTriggers.OnPlayerLeave} = KeyCode.None");
        writer.WriteLine($"Self.DarkGameTheme = {DarkMode.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.CustomGameTheme = {CustomGameTheme.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.GameBgColor = {SelfTab.BgHex} = KeyCode.None");
        writer.WriteLine($"Self.GameTextColor = {SelfTab.TextHex} = KeyCode.None");
        writer.WriteLine($"Self.ChatFont = {ChatFontChanger.Enabled} = KeyCode.None");
        writer.WriteLine($"Self.ChatFontType = {ChatFontChanger.FontType} = KeyCode.None");
        writer.WriteLine($"NameSpoof.SpoofedName = {SkidMenu.nameSpoofName} = KeyCode.None");
        writer.WriteLine($"NameSpoof.Enabled = {SkidMenu.nameSpoofEnabled} = KeyCode.None");
        writer.WriteLine($"NameSpoof.Mode = {SkidMenu.nameSpoofMode} = KeyCode.None");
        writer.WriteLine($"NameSpoof.Length = {SkidMenu.nameSpoofLength} = KeyCode.None");
        writer.WriteLine($"Chat.History = {features.ChatEnhancements.EnableChatHistory} = KeyCode.None");
        writer.WriteLine($"Chat.ExtendedChat = {features.ChatEnhancements.EnableExtendedChat} = KeyCode.None");
        writer.WriteLine($"Chat.ColorCommand = {features.ChatEnhancements.EnableColorCommand} = KeyCode.None");
        writer.WriteLine($"Chat.SenderEnabled = {features.ChatSender.Enabled} = KeyCode.None");
        writer.WriteLine($"Chat.SenderMessage = {features.ChatSender.Message} = KeyCode.None");
        writer.WriteLine($"Chat.SenderDelay = {features.ChatSender.Delay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Chat.BypassCharLimit = {ChatTab.BypassCharLimit} = KeyCode.None");
        writer.WriteLine($"Chat.OnJoinEnabled = {features.ChatSender.OnJoinEnabled} = KeyCode.None");
        writer.WriteLine($"Chat.OnJoinMessage = {features.ChatSender.OnJoinMessage} = KeyCode.None");
        writer.WriteLine($"Chat.OnDeathEnabled = {features.ChatSender.OnDeathEnabled} = KeyCode.None");
        writer.WriteLine($"Chat.OnDeathMessage = {features.ChatSender.OnDeathMessage} = KeyCode.None");
        writer.WriteLine($"Chat.OnMeetingEnabled = {features.ChatSender.OnMeetingEnabled} = KeyCode.None");
        writer.WriteLine($"Chat.OnMeetingMessage = {features.ChatSender.OnMeetingMessage} = KeyCode.None");
        writer.WriteLine($"Chat.OnKillEnabled = {features.ChatSender.OnKillEnabled} = KeyCode.None");
        writer.WriteLine($"Chat.OnKillMessage = {features.ChatSender.OnKillMessage} = KeyCode.None");
        writer.WriteLine($"Chat.OnEjectionEnabled = {features.ChatSender.OnEjectionEnabled} = KeyCode.None");
        writer.WriteLine($"Chat.OnEjectionMessage = {features.ChatSender.OnEjectionMessage} = KeyCode.None");
        writer.WriteLine($"Spoofer.ShouldSpoofVersion = {Spoofer.shouldSpoofVersion} = KeyCode.None");
        writer.WriteLine($"Spoofer.SpoofedVersion = {Spoofer.spoofedVersion} = KeyCode.None");
        writer.WriteLine($"Spoofer.UseModdedProtocol = {Spoofer.useModdedProtocol} = KeyCode.None");
        writer.WriteLine($"Spoofer.XboxId = {Spoofer.spoofedXboxId} = KeyCode.None");
        writer.WriteLine($"Spoofer.PsnId = {Spoofer.spoofedPsnId} = KeyCode.None");
        writer.WriteLine($"GUI.MenuKeybind = {SkidMenu.menuKeybind} = KeyCode.None");
        writer.WriteLine($"GUI.MenuColor = {SkidMenu.menuHtmlColor} = KeyCode.None");
        writer.WriteLine($"GUI.OpenOnMouse = {SkidMenu.menuOpenOnMouse} = KeyCode.None");
        writer.WriteLine($"GUI.KeepSubwindows = {SkidMenu.menuKeepSubwindowsOpen} = KeyCode.None");
        writer.WriteLine($"Spoof.Level = {SkidMenu.spoofLevel} = KeyCode.None");
        writer.WriteLine($"Spoof.LevelMin = {SkidMenu.spoofLevelRandomMin} = KeyCode.None");
        writer.WriteLine($"Spoof.LevelMax = {SkidMenu.spoofLevelRandomMax} = KeyCode.None");
        writer.WriteLine($"Spoof.Platform = {SkidMenu.spoofPlatform} = KeyCode.None");
        writer.WriteLine($"Spoof.PlatformExclusions = {SkidMenu.spoofPlatformExclusions} = KeyCode.None");
        writer.WriteLine($"Guest.FriendCode = {SkidMenu.guestFriendCode} = KeyCode.None");
        writer.WriteLine($"Guest.Enabled = {SkidMenu.guestMode} = KeyCode.None");
        writer.WriteLine($"Profile.AutoLoad = {SkidMenu.autoLoadProfile} = KeyCode.None");
        writer.WriteLine($"Config.Editor = {SkidMenu.configEditor} = KeyCode.None");
        writer.WriteLine($"Dating.FindDaters = {CheatToggles.findDaters} = KeyCode.None");
        writer.WriteLine($"Dating.ExtendedList = {CheatToggles.extendedLobbyList} = KeyCode.None");
        writer.WriteLine($"AC.MaxLevel = {SkidMenu.maxPlayerLevel} = KeyCode.None");
        writer.WriteLine($"AC.MaxTeleport = {SkidMenu.maxTeleportDistance.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Chat.HistorySize = {SkidMenu.chatHistorySize} = KeyCode.None");
        writer.WriteLine($"Chat.HistoryInfinite = {SkidMenu.chatHistoryInfinite} = KeyCode.None");
        writer.WriteLine($"Log.PlayerJoin = {SkidMenu.logPlayerJoin} = KeyCode.None");
        writer.WriteLine($"Log.GuardianProtect = {CheatToggles.logGuardianProtect} = KeyCode.None");
        writer.WriteLine($"Log.ShowDistance = {SkidMenu.logShowDistance} = KeyCode.None");
        writer.WriteLine($"Log.Advanced = {SkidMenu.advancedLogging} = KeyCode.None");
        writer.WriteLine($"FR.NameHostOnly = {SkidMenu.frRandNameHostOnly} = KeyCode.None");
        writer.WriteLine($"FR.ColorHostOnly = {SkidMenu.frRandColorHostOnly} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnDeath = {FullyRandomizeTriggers.OnDeath} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnKill = {FullyRandomizeTriggers.OnKill} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnMeetingStart = {FullyRandomizeTriggers.OnMeetingStart} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnMeetingEnd = {FullyRandomizeTriggers.OnMeetingEnd} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnLobbyLeave = {FullyRandomizeTriggers.OnLobbyLeave} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnGameEnd = {FullyRandomizeTriggers.OnGameEnd} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnShapeshift = {FullyRandomizeTriggers.OnShapeshift} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnShapeshiftBack = {FullyRandomizeTriggers.OnShapeshiftBack} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnVent = {FullyRandomizeTriggers.OnVent} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnExitVent = {FullyRandomizeTriggers.OnExitVent} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnTaskComplete = {FullyRandomizeTriggers.OnTaskComplete} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnEjected = {FullyRandomizeTriggers.OnEjected} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnSabotage = {FullyRandomizeTriggers.OnSabotage} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnVanish = {FullyRandomizeTriggers.OnVanish} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnReappear = {FullyRandomizeTriggers.OnReappear} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnVotekicked = {FullyRandomizeTriggers.OnVotekicked} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnPlayerJoin = {FullyRandomizeTriggers.OnPlayerJoin} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.OnPlayerLeave = {FullyRandomizeTriggers.OnPlayerLeave} = KeyCode.None");
        writer.WriteLine($"FR.Trigger.ShowNotification = {FullyRandomizeTriggers.ShowNotification} = KeyCode.None");
        writer.WriteLine($"ZoomOut.ScrollSpeed = {MalumESP.ZoomScrollSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"ZoomOut.Smoothness = {MalumESP.ZoomSmoothness.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"ZoomOut.MaxDistance = {MalumESP.ZoomMaxDistance.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"ZoomOut.MinDistance = {MalumESP.ZoomMinDistance.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Freecam.Speed = {MalumESP.FreecamSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Freecam.Smoothness = {MalumESP.FreecamSmoothness.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"ESP.SeePlayersInVents = {SeePlayersInVents.Enabled} = KeyCode.None");
        writer.WriteLine($"ESP.SeePhantoms = {SeePlayersInVents.SeePhantoms} = KeyCode.None");
        writer.WriteLine($"ESP.LobbyTimer = {LobbyTimer.Enabled} = KeyCode.None");
        writer.WriteLine($"ESP.SubCtx = {ESPContexts.ShowRole},{ESPContexts.ShowInfo},{ESPContexts.KillCooldown},{ESPContexts.Tasks},{ESPContexts.IsHost},{ESPContexts.Level},{ESPContexts.Platform},{ESPContexts.Votekicks},{ESPContexts.FriendCode},{ESPContexts.Puid},{ESPContexts.DeviceId},{ESPContexts.ModUser} = KeyCode.None");
        writer.WriteLine($"Dummy.WalkToTasks = {DummySpawner.WalkToTasks} = KeyCode.None");
        writer.WriteLine($"Dummy.FixSabotages = {DummySpawner.FixSabotages} = KeyCode.None");
        writer.WriteLine($"Dummy.ReportAndChat = {DummySpawner.ReportAndChat} = KeyCode.None");
        writer.WriteLine($"Dummy.UseKeybind = {DummySpawner.UseKeybind} = KeyCode.None");
        writer.WriteLine($"Dummy.SpawnKey = {DummySpawner.SpawnKey} = KeyCode.None");
        writer.WriteLine($"Dummy.SpamEnabled = {DummySpawner.SpamEnabled} = KeyCode.None");
        writer.WriteLine($"Dummy.SpamDelay = {DummySpawner.SpamDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"HO.BypassHostOnly = {bypassHostOnly} = KeyCode.None");
        writer.WriteLine($"HO.KillVanished = {killVanished} = KeyCode.None");
        writer.WriteLine($"HO.KillAnyone = {killAnyone} = KeyCode.None");
        writer.WriteLine($"HO.NoKillCd = {noKillCd} = KeyCode.None");
        writer.WriteLine($"HO.ShowProtectMenu = {showProtectMenu} = KeyCode.None");
        writer.WriteLine($"HO.NoTaskMode = {noTaskMode} = KeyCode.None");
        writer.WriteLine($"HO.NoSettingLimit = {noSettingLimit} = KeyCode.None");
        writer.WriteLine($"HO.BanMidGame = {Host.BanMidGame.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.FlippedSkeld = {Host.FlippedSkeld} = KeyCode.None");
        writer.WriteLine($"HO.SkipMeeting = {skipMeeting} = KeyCode.None");
        writer.WriteLine($"HO.VoteImmune = {voteImmune} = KeyCode.None");
        writer.WriteLine($"HO.JudgeImmune = {judgeImmune} = KeyCode.None");
        writer.WriteLine($"HO.EjectPlayer = {ejectPlayer} = KeyCode.None");
        writer.WriteLine($"HO.ForceStartGame = {forceStartGame} = KeyCode.None");
        writer.WriteLine($"HO.NoGameEnd = {noGameEnd} = KeyCode.None");
        writer.WriteLine($"HO.DisableMeetings = {Host.DisableMeetings.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.DisableSabotages = {Host.DisableSabotages.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.DisableCloseDoors = {Host.DisableCloseDoors.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.DisableCameras = {Host.DisableCameras.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.PreGameRoleForce = {HostFeatures.preGameRoleForce} = KeyCode.None");
        writer.WriteLine($"HO.PreGameImpCount = {HostFeatures.preGameImpCount} = KeyCode.None");
        writer.WriteLine($"HO.DiscoParty = {SkidMenu.routines.discoHost.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.DiscoDelay = {SkidMenu.routines.discoHost.randomizationDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"HO.BlockLowLevels = {Host.BlockLowLevels.Enabled} = KeyCode.None");
        writer.WriteLine($"HO.BlockLowLevelsMin = {Host.BlockLowLevels.MinLevel} = KeyCode.None");

        writer.WriteLine($"AC.Enabled = {anticheat.Anticheat.Enabled} = KeyCode.None");
        writer.WriteLine($"AC.SendNotification = {anticheat.Anticheat.sendNotification} = KeyCode.None");
        writer.WriteLine($"AC.DiscardRpc = {anticheat.Anticheat.discardRpc} = KeyCode.None");
        writer.WriteLine($"AC.CheckSpoofedPlatforms = {anticheat.Anticheat.CheckSpoofedPlatforms} = KeyCode.None");
        writer.WriteLine($"AC.Punishment = {(int)anticheat.Anticheat.punishment} = KeyCode.None");
        writer.WriteLine($"AC.NonHostPunishment = {(int)anticheat.Anticheat.nonHostPunishment} = KeyCode.None");
        foreach (var kvp in anticheat.Anticheat.RpcHandlers)
            writer.WriteLine($"AC.Rpc.{kvp.Key} = {kvp.Value.Enabled} = KeyCode.None");
        writer.WriteLine($"MD.Enabled = {anticheat.ModDetection.Enabled} = KeyCode.None");
        writer.WriteLine($"Privacy.HideDeviceId = {hideDeviceId} = KeyCode.None");
        writer.WriteLine($"Privacy.SpoofDeviceId = {spoofDeviceId} = KeyCode.None");
        writer.WriteLine($"Privacy.CustomDeviceId = {spoofDeviceIdCustom} = KeyCode.None");
        writer.WriteLine($"Privacy.DisableTelemetry = {disableTelemetry} = KeyCode.None");
        writer.WriteLine($"Privacy.SpoofTelemetry = {spoofTelemetry} = KeyCode.None");
        writer.WriteLine($"Lobby.AutoReturnAfterMatch = {SkidMenu.autoReturnAfterMatch} = KeyCode.None");
        foreach (var mod in anticheat.ModDetection.KnownMods)
        {
            writer.WriteLine($"MD.Mod.{mod.Name.Replace(" ", "_").Replace("/", "_")} = {mod.Enabled} = KeyCode.None");
            writer.WriteLine($"MD.ModPunish.{mod.Name.Replace(" ", "_").Replace("/", "_")} = {mod.ShouldPunish} = KeyCode.None");
        }

        writer.WriteLine($"BL.AutoAddFlagged = {anticheat.Blacklist.AutoAddFlagged} = KeyCode.None");
        writer.WriteLine($"BL.AutoAddModDetected = {anticheat.Blacklist.AutoAddModDetected} = KeyCode.None");
        writer.WriteLine($"BL.AutoPunish = {anticheat.Blacklist.AutoPunish} = KeyCode.None");
        writer.WriteLine($"BL.NotifyOnJoin = {anticheat.Blacklist.NotifyOnJoin} = KeyCode.None");
        writer.WriteLine($"BL.KickOnJoin = {anticheat.Blacklist.KickOnJoin} = KeyCode.None");
        writer.WriteLine($"BL.BanOnJoin = {anticheat.Blacklist.BanOnJoin} = KeyCode.None");
        writer.WriteLine($"BL.VentKickOnJoin = {anticheat.Blacklist.VentKickOnJoin} = KeyCode.None");

        writer.WriteLine($"KillReach.Range = {killReachRange.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"KillAura.Enabled = {features.KillAura.Enabled} = KeyCode.None");
        writer.WriteLine($"Impostor.KillOtherImpostors = {killOtherImpostors} = KeyCode.None");
        writer.WriteLine($"Impostor.KillImpostors = {KillImpostors.Enabled} = KeyCode.None");
        writer.WriteLine($"KillAura.Range = {features.KillAura.Range.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"KillAura.InfiniteRange = {features.KillAura.InfiniteRange} = KeyCode.None");
        writer.WriteLine($"KillAura.FireRate = {features.KillAura.FireRate.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"KillAura.RespectMeeting = {features.KillAura.RespectMeeting} = KeyCode.None");
        writer.WriteLine($"KillAura.RespectVent = {features.KillAura.RespectVent} = KeyCode.None");
        writer.WriteLine($"KillAura.WaitAfterStart = {features.KillAura.WaitAfterStart} = KeyCode.None");
        writer.WriteLine($"KillAura.StartDelay = {features.KillAura.StartDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"KillAura.Telemurder = {features.KillAura.Telemurder} = KeyCode.None");
        writer.WriteLine($"KillAura.IgnoreCooldownAsHost = {features.KillAura.IgnoreCooldownAsHost} = KeyCode.None");
        writer.WriteLine($"GA.InfiniteRange = {gaInfiniteRange} = KeyCode.None");
        writer.WriteLine($"GA.IgnoreImpostors = {gaIgnoreImpostors} = KeyCode.None");
        writer.WriteLine($"Judge.InstantUnlock = {features.JudgeCheats.InstantUnlock} = KeyCode.None");
        writer.WriteLine($"Judge.InfiniteGavels = {features.JudgeCheats.InfiniteGavels} = KeyCode.None");
        writer.WriteLine($"Crewmate.InstantPet = {instantPet} = KeyCode.None");
        writer.WriteLine($"Crewmate.SpamPet = {spamPet} = KeyCode.None");
        writer.WriteLine($"Crewmate.SpamPetDelay = {spamPetDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Troll.AutoReportBodies = {features.Troll.AutoReportBodies.Enabled} = KeyCode.None");
        writer.WriteLine($"Troll.AutoReportBodiesDelay = {features.Troll.AutoReportBodies.ReportDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Troll.AutoReportBodiesCrewOnly = {features.Troll.AutoReportBodies.CrewOnly} = KeyCode.None");
        writer.WriteLine($"Troll.FuckGame = {features.FuckGame.Enabled} = KeyCode.None");
        writer.WriteLine($"GUI.MenuOpacity = {menuOpacity.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Passive.UnlockFeatures = {unlockFeatures} = KeyCode.None");
        writer.WriteLine($"Passive.AntiOverload = {antiOverload} = KeyCode.None");
        writer.WriteLine($"Passive.FreeCosmetics = {freeCosmetics} = KeyCode.None");
        writer.WriteLine($"Passive.AvoidPenalties = {avoidPenalties} = KeyCode.None");
        writer.WriteLine($"Passive.CopyLobbyCodeOnDisconnect = {copyLobbyCodeOnDisconnect} = KeyCode.None");
        writer.WriteLine($"Passive.SpoofAprilFoolsDate = {spoofAprilFoolsDate} = KeyCode.None");
        writer.WriteLine($"Passive.RandomizeCosmetics = {randomizeCosmetics} = KeyCode.None");
        writer.WriteLine($"Protections.ForceDTLS = {features.Protections.ForceDTLS.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.BlockServerTeleports = {features.Protections.BlockServerTeleports.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.HardenedReadPackedUInt = {features.Protections.HardenedReadPackedUInt.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.BlockInvalidLadderOverload = {features.Protections.BlockInvalidLadderOverload} = KeyCode.None");
        writer.WriteLine($"Protections.BlockLargeGameMessages = {features.Protections.BlockLargeGameMessages} = KeyCode.None");
        writer.WriteLine($"Protections.BlockInvalidGameDataMessages = {features.Protections.BlockInvalidGameDataMessages} = KeyCode.None");
        writer.WriteLine($"Protections.BlockUnauthorizedSystemUpdates = {features.Protections.BlockUnauthorizedSystemUpdates} = KeyCode.None");
        writer.WriteLine($"Protections.ProtectAgainstNonHostKickExploit = {features.Protections.ProtectAgainstNonHostKickExploit} = KeyCode.None");
        writer.WriteLine($"Protections.BlockZiplineForce = {features.Protections.BlockZiplineForce} = KeyCode.None");
        writer.WriteLine($"Protections.BlockVentTpForce = {features.Protections.BlockVentTpForce} = KeyCode.None");
        writer.WriteLine($"Protections.Votekicks = {features.Protections.Votekicks.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.MemoryAllocationOverload = {features.Protections.MemoryAllocationOverload.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.BypassShapeshiftRatelimits = {features.Protections.BypassShapeshiftRatelimits.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.AntiExploits = {features.Protections.AntiExploits} = KeyCode.None");
        writer.WriteLine($"GUI.MenuScaleH = {menuScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.MenuScaleV = {menuScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.MaxFpsEnabled = {maxFpsEnabled} = KeyCode.None");
        writer.WriteLine($"GUI.MaxFpsValue = {maxFpsValue} = KeyCode.None");
        writer.WriteLine($"GUI.StretchedResEnabled = {stretchedResEnabled} = KeyCode.None");
        writer.WriteLine($"GUI.StretchedResW = {stretchedResW.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.StretchedResH = {stretchedResH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.ConsoleScaleH = {consoleScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.ConsoleScaleV = {consoleScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.DoorsScaleH = {doorsScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.DoorsScaleV = {doorsScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.TasksScaleH = {tasksScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.TasksScaleV = {tasksScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.ProtectScaleH = {protectScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.ProtectScaleV = {protectScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Protections.AntiExploits = {features.Protections.AntiExploits} = KeyCode.None");
        writer.WriteLine($"Blacklist.Enabled = {anticheat.Blacklist.Enabled} = KeyCode.None");
        writer.WriteLine($"ESP.SkipShhhAnimation = {features.Visuals.SkipShhhAnimation.Enabled} = KeyCode.None");
        writer.WriteLine($"ESP.SkipRoleReveal = {SkipRoleReveal.Enabled} = KeyCode.None");
        writer.WriteLine($"Filter.UseImpostor = {FindDatersLobbyPatch.useImpostorFilter} = KeyCode.None");
        writer.WriteLine($"Filter.ImpostorCount = {FindDatersLobbyPatch.impostorCount} = KeyCode.None");
        writer.WriteLine($"Filter.UsePlayer = {FindDatersLobbyPatch.usePlayerFilter} = KeyCode.None");
        writer.WriteLine($"Filter.MinPlayers = {FindDatersLobbyPatch.minPlayers} = KeyCode.None");
        writer.WriteLine($"Filter.MaxPlayers = {FindDatersLobbyPatch.maxPlayers} = KeyCode.None");
        writer.WriteLine($"Filter.UseChat = {FindDatersLobbyPatch.useChatFilter} = KeyCode.None");
        writer.WriteLine($"Filter.UseLang = {FindDatersLobbyPatch.useLangFilter} = KeyCode.None");
        writer.WriteLine($"Filter.SelectedLangs = {string.Join(",", FindDatersLobbyPatch.selectedLangs.Select(l => (int)l))} = KeyCode.None");
        writer.WriteLine($"Filter.UseHostPlatform = {FindDatersLobbyPatch.useHostPlatformFilter} = KeyCode.None");
        writer.WriteLine($"Filter.SelectedPlatforms = {string.Join(",", FindDatersLobbyPatch.selectedPlatforms.Select(p => (int)p))} = KeyCode.None");
        writer.WriteLine($"Filter.UseHostName = {FindDatersLobbyPatch.useHostNameFilter} = KeyCode.None");
        writer.WriteLine($"Self.CustomBodyType = {SelfTab.CustomBodyType} = KeyCode.None");
        writer.WriteLine($"Self.SelectedBodyType = {(int)SelfTab.SelectedBodyType} = KeyCode.None");
        writer.WriteLine($"Self.LongBodyHeight = {SelfTab.LongBodyHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Protections.BypassShapeshiftRatelimits = {features.Protections.BypassShapeshiftRatelimits.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.ForceDTLS = {features.Protections.ForceDTLS.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.BlockServerTeleports = {features.Protections.BlockServerTeleports.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.HardenedReadPackedUInt = {features.Protections.HardenedReadPackedUInt.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.BlockInvalidLadderOverload = {features.Protections.BlockInvalidLadderOverload} = KeyCode.None");
        writer.WriteLine($"Protections.Votekicks = {features.Protections.Votekicks.Enabled} = KeyCode.None");
        writer.WriteLine($"Protections.MemoryAllocationOverload = {features.Protections.MemoryAllocationOverload.Enabled} = KeyCode.None");
        writer.WriteLine($"AutoHost.Enabled = {SkidMenu.autoHostEnabled} = KeyCode.None");
        writer.WriteLine($"AutoHost.InstantStart = {SkidMenu.autoHostInstantStart} = KeyCode.None");
        writer.WriteLine($"AutoHost.CancelBelowMin = {SkidMenu.autoHostCancelBelowMin} = KeyCode.None");
        writer.WriteLine($"AutoHost.WaitLoadedPlayers = {SkidMenu.autoHostWaitLoadedPlayers} = KeyCode.None");
        writer.WriteLine($"AutoHost.ReturnAfterMatch = {SkidMenu.autoHostReturnAfterMatch} = KeyCode.None");
        writer.WriteLine($"AutoHost.ForceLastMinute = {SkidMenu.autoHostForceLastMinute} = KeyCode.None");
        writer.WriteLine($"AutoHost.MinPlayers = {SkidMenu.autoHostMinPlayers} = KeyCode.None");
        writer.WriteLine($"AutoHost.ForceMinPlayers = {SkidMenu.autoHostForceMinPlayers} = KeyCode.None");
        writer.WriteLine($"AutoHost.WarmupSeconds = {SkidMenu.autoHostWarmupSeconds} = KeyCode.None");
        writer.WriteLine($"AutoHost.StartDelaySeconds = {SkidMenu.autoHostStartDelaySeconds} = KeyCode.None");
        writer.WriteLine($"AutoHost.FastStartPlayers = {SkidMenu.autoHostFastStartPlayers} = KeyCode.None");
        writer.WriteLine($"AutoHost.FastStartDelaySeconds = {SkidMenu.autoHostFastStartDelaySeconds} = KeyCode.None");
        writer.WriteLine($"AutoHost.LoadGraceSeconds = {SkidMenu.autoHostLoadGraceSeconds} = KeyCode.None");
        writer.WriteLine($"AutoHost.ForceAfterMinutes = {SkidMenu.autoHostForceAfterMinutes} = KeyCode.None");
        writer.WriteLine($"AutoHost.BackoffSeconds = {SkidMenu.autoHostBackoffSeconds} = KeyCode.None");

        writer.WriteLine($"Console.ShowConsole = {showConsole} = KeyCode.None");
        writer.WriteLine($"Console.LogDeaths = {logDeaths} = KeyCode.None");
        writer.WriteLine($"Console.LogShapeshiftInto = {logShapeshiftInto} = KeyCode.None");
        writer.WriteLine($"Console.LogVentIn = {logVentIn} = KeyCode.None");
        writer.WriteLine($"Console.LogVentOut = {logVentOut} = KeyCode.None");
        writer.WriteLine($"Console.LogShapeshiftRevert = {logShapeshiftRevert} = KeyCode.None");
        writer.WriteLine($"Console.LogSabotages = {logSabotages} = KeyCode.None");
        writer.WriteLine($"Console.LogMeetingCalled = {logMeetingCalled} = KeyCode.None");
        writer.WriteLine($"Console.LogBodyReport = {logBodyReport} = KeyCode.None");
        writer.WriteLine($"Console.LogEjections = {logEjections} = KeyCode.None");
        writer.WriteLine($"Console.LogVerdict = {logVerdict} = KeyCode.None");
        writer.WriteLine($"Console.LogVotes = {logVotes} = KeyCode.None");
        writer.WriteLine($"Console.LogVotekicks = {logVotekicks} = KeyCode.None");
        writer.WriteLine($"Console.LogChat = {logChat} = KeyCode.None");
        writer.WriteLine($"Chat.CopyMessage = {copyMessage} = KeyCode.None");
        writer.WriteLine($"Console.LogDisconnects = {logDisconnects} = KeyCode.None");
        writer.WriteLine($"Console.LogJoins = {logJoins} = KeyCode.None");
        writer.WriteLine($"Console.LogPhantomVanish = {logPhantomVanish} = KeyCode.None");
        writer.WriteLine($"Console.LogPhantomReappear = {logPhantomReappear} = KeyCode.None");
        writer.WriteLine($"Console.LogTaskCompleted = {logTaskCompleted} = KeyCode.None");
        writer.WriteLine($"Console.MaxLogEntries = {maxLogEntries} = KeyCode.None");
        writer.WriteLine($"GUI.ChatScaleH = {chatScaleH.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"GUI.ChatScaleV = {chatScaleV.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Console.ShowChatUI = {showChatUI} = KeyCode.None");
        writer.WriteLine($"Console.ChatMaxEntries = {chatMaxEntries} = KeyCode.None");
        writer.WriteLine($"LagComp.Enabled = {features.LagCompensation.Enabled} = KeyCode.None");
        writer.WriteLine($"LagComp.FreezePosition = {features.LagCompensation.FreezePosition} = KeyCode.None");
        writer.WriteLine($"LagComp.Jitter = {features.LagCompensation.Jitter} = KeyCode.None");
        writer.WriteLine($"LagComp.SkipTicks = {features.LagCompensation.SkipTicks} = KeyCode.None");
        writer.WriteLine($"LagComp.JitterMin = {features.LagCompensation.JitterMin.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"LagComp.JitterMax = {features.LagCompensation.JitterMax.ToString(System.Globalization.CultureInfo.InvariantCulture)} = KeyCode.None");
        writer.WriteLine($"Invisibility.Enabled = {features.Invisibility.Enabled} = KeyCode.None");
        writer.WriteLine($"Invisibility.OnlyInGame = {features.Invisibility.OnlyInGame} = KeyCode.None");

        static string R(Rect? r) => r.HasValue ? $"{r.Value.x:F1},{r.Value.y:F1},{r.Value.width:F1},{r.Value.height:F1}" : "";
        if (MenuUI.Instance != null)     writer.WriteLine($"WindowRect.Menu = {R(MenuUI.Instance.WindowRect)}");
        if (ConsoleUI.Instance != null)  writer.WriteLine($"WindowRect.Console = {R(ConsoleUI.Instance.WindowRect)}");
        if (DoorsUI.Instance != null)    writer.WriteLine($"WindowRect.Doors = {R(DoorsUI.Instance.WindowRect)}");
        if (TasksUI.Instance != null)    writer.WriteLine($"WindowRect.Tasks = {R(TasksUI.Instance.WindowRect)}");
    }

    public static void LoadTogglesFromProfile()
    {
        if (!File.Exists(SkidMenu.ProfilePath)) return;

        using var reader = new StreamReader(SkidMenu.ProfilePath);

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            line = line.Trim();
            if (line.StartsWith("#")) continue;

            var parts = line.Split('=', 3);
            if (parts.Length < 2) continue;

            var name = parts[0].Trim();
            var valuePart = parts[1].Trim();

            if (name.StartsWith("AC.Rpc."))
            {
                if (System.Enum.TryParse<RpcCalls>(name.Substring(7), out var rpc) &&
                    anticheat.Anticheat.RpcHandlers.TryGetValue(rpc, out var handler) &&
                    bool.TryParse(valuePart, out var rb)) handler.Enabled = rb;
                continue;
            }
            if (name.StartsWith("MD.Mod."))
            {
                string modKey = name.Substring(7);
                var mod = anticheat.ModDetection.KnownMods.Find(m => m.Name.Replace(" ", "_").Replace("/", "_") == modKey);
                if (mod != null && bool.TryParse(valuePart, out var mb)) mod.Enabled = mb;
                continue;
            }
            if (name.StartsWith("MD.ModPunish."))
            {
                string modKey = name.Substring(13);
                var mod = anticheat.ModDetection.KnownMods.Find(m => m.Name.Replace(" ", "_").Replace("/", "_") == modKey);
                if (mod != null && bool.TryParse(valuePart, out var mp)) mod.ShouldPunish = mp;
                continue;
            }

            switch (name)
            {
                case "Self.AlwaysShowTaskAnimations":
                    if (bool.TryParse(valuePart, out var v1)) Self.AlwaysShowTaskAnimations = v1;
                    continue;
                case "Self.NoLadderCooldown":
                    if (bool.TryParse(valuePart, out var v2)) Self.NoLadderCooldown.Enabled = v2;
                    continue;
                case "Self.NoZiplineCooldown":
                    if (bool.TryParse(valuePart, out var vz)) Self.NoZiplineCooldown.Enabled = vz;
                    continue;
                case "Self.VoteAnyone":
                    if (bool.TryParse(valuePart, out var va)) Self.VoteAnywhere.VoteAnyone = va;
                    continue;
                case "Self.VoteBeforeVotingStarts":
                    if (bool.TryParse(valuePart, out var vb)) Self.VoteAnywhere.VoteBeforeVotingStarts = vb;
                    continue;
                case "Self.InstantVote":
                    if (bool.TryParse(valuePart, out var vi)) Self.VoteAnywhere.InstantVote = vi;
                    continue;
                case "Self.UnlimitedMeetings":
                    if (bool.TryParse(valuePart, out var v3)) Self.UnlimitedMeetings.enabled = v3;
                    continue;
                case "Self.UpdateStatsFreeplay":
                    if (bool.TryParse(valuePart, out var v4)) Self.UpdateStatsFreeplay.Enabled = v4;
                    continue;
                case "Self.PlayerSpeedMultiplier":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var spd)) Self.PlayerSpeedModifier.Multiplier = System.Math.Clamp(spd, 0.01f, 10f);
                    continue;
                case "Self.PlayerSpeedModifierEnabled":
                    if (bool.TryParse(valuePart, out var spme)) Self.PlayerSpeedModifier.Enabled = spme;
                    continue;
                case "Self.CurrentSpeedChanger":
                    if (bool.TryParse(valuePart, out var cscEnabled)) Self.CurrentSpeedChanger.Enabled = cscEnabled;
                    continue;
                case "Self.CurrentSpeedChangerValue":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var cscSpeed)) Self.CurrentSpeedChanger.Speed = System.Math.Clamp(cscSpeed, 0f, 10f);
                    continue;
                case "Self.ColorSniperLobbyOnly":
                    if (bool.TryParse(valuePart, out var cslo)) features.ColorSniper.InLobbyOnly = cslo;
                    continue;
                case "Self.ColorSniper":
                    if (bool.TryParse(valuePart, out var cs)) features.ColorSniper.Enabled = cs;
                    continue;
                case "Self.ColorSniperTarget":
                    if (byte.TryParse(valuePart, out var cst)) features.ColorSniper.TargetColor = cst;
                    continue;
                case "Self.SelectedColor":
                    if (int.TryParse(valuePart, out var selc)) SelfTab.SelectedColor = System.Math.Clamp(selc, 0, 17);
                    continue;
                case "Immortality":
                    if (bool.TryParse(valuePart, out var v5)) Immortality.Enabled = v5;
                    continue;
                case "Self.ImmortalDisableNotification":
                    if (bool.TryParse(valuePart, out var idn)) features.Immortality.DisableNotification = idn;
                    continue;
                case "Sabotage.UpdateSystemsDirectly":
                    if (bool.TryParse(valuePart, out var v6)) Sabotage.UpdateSystemsDirectly = v6;
                    continue;
                case "Roles.SabotageAsCrewmate":
                    if (bool.TryParse(valuePart, out var v7)) Roles.SkipSabotageChecks.SabotageAsCrewmate = v7;
                    continue;
                case "Roles.SabotageInVents":
                    if (bool.TryParse(valuePart, out var v8)) Roles.SkipSabotageChecks.SabotageInVents = v8;
                    continue;
                case "NameSpoofer.Enabled":
                    if (bool.TryParse(valuePart, out var nse)) features.NameSpoofer.Enabled = nse;
                    continue;
                case "NameSpoofer.SpoofedName":
                    features.NameSpoofer.SpoofedName = valuePart;
                    if (features.NameSpoofer.Enabled && !string.IsNullOrEmpty(valuePart))
                        DataManager.Player.Customization.Name = valuePart;
                    continue;
                case "NameSpoofer.Mode":
                    if (int.TryParse(valuePart, out var nsm)) features.NameSpoofer.Mode = (features.NameSpoofer.RandomizerMode)nsm;
                    continue;
                case "NameSpoofer.RandomLength":
                    if (int.TryParse(valuePart, out var nsl)) features.NameSpoofer.RandomLength = System.Math.Clamp(nsl, 3, 10);
                    continue;
                case "Votekick.ShowVotekickInfo":
                    if (bool.TryParse(valuePart, out var vsvi)) VotekickHandler.ShowVotekickInfo = vsvi;
                    continue;
                case "Notif.ExSelf":
                    var exs = valuePart.Split(',');
                    for (int i = 0; i < exs.Length && i < 23; i++) if (bool.TryParse(exs[i].Trim(), out var b)) notifExSelf[i] = b;
                    continue;
                case "Notif.ExHost":
                    var exh = valuePart.Split(',');
                    for (int i = 0; i < exh.Length && i < 23; i++) if (bool.TryParse(exh[i].Trim(), out var b2)) notifExHost[i] = b2;
                    continue;
                case "Votekick.NotifyVotekickInfo":
                    if (bool.TryParse(valuePart, out var vnvi)) VotekickHandler.NotifyVotekickInfo = vnvi;
                    continue;
                case "Notif.Join":
                    if (bool.TryParse(valuePart, out var nj)) notifJoin = nj;
                    continue;
                case "Notif.ShowDistance":
                    if (bool.TryParse(valuePart, out var nsd)) notifShowDistance = nsd;
                    continue;
                case "Votekick.IgnoreOwnVotekicks":
                    if (bool.TryParse(valuePart, out var viov)) VotekickHandler.IgnoreOwnVotekicks = viov;
                    continue;
                case "Votekick.VotekickAllEnabled":
                    if (bool.TryParse(valuePart, out var vvae)) VotekickHandler.VotekickAllEnabled = vvae;
                    continue;
                case "Votekick.VoteCount":
                    if (int.TryParse(valuePart, out var vvc)) VotekickHandler.VoteCount = System.Math.Clamp(vvc, 1, 10);
                    continue;
                case "Votekick.AutoKickInterval":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var vaki)) VotekickHandler.AutoKickInterval = System.Math.Clamp(vaki, 0.5f, 10f);
                    continue;
                case "Votekick.AutoPurgeImpostors":
                    if (bool.TryParse(valuePart, out var vapi)) VotekickHandler.AutoPurgeImpostors = vapi;
                    continue;
                case "Votekick.AutoPurgeCrew":
                    if (bool.TryParse(valuePart, out var vapc)) VotekickHandler.AutoPurgeCrew = vapc;
                    continue;
                case "Votekick.AutoPurgeHost":
                    if (bool.TryParse(valuePart, out var vaph)) VotekickHandler.AutoPurgeHost = vaph;
                    continue;
                case "Votekick.AutoRetaliate":
                    if (bool.TryParse(valuePart, out var var2)) VotekickHandler.AutoRetaliate = var2;
                    continue;
                case "Votekick.FinishTheKick":
                    if (bool.TryParse(valuePart, out var ftk)) VotekickHandler.FinishTheKick = ftk;
                    continue;
                case "FindDaters.UseImpostorFilter":
                    if (bool.TryParse(valuePart, out var fdui)) FindDatersLobbyPatch.useImpostorFilter = fdui;
                    continue;
                case "FindDaters.ImpostorCount":
                    if (int.TryParse(valuePart, out var fdic)) FindDatersLobbyPatch.impostorCount = System.Math.Clamp(fdic, 1, 3);
                    continue;
                case "FindDaters.UsePlayerFilter":
                    if (bool.TryParse(valuePart, out var fdupf)) FindDatersLobbyPatch.usePlayerFilter = fdupf;
                    continue;
                case "FindDaters.MinPlayers":
                    if (int.TryParse(valuePart, out var fdmin)) FindDatersLobbyPatch.minPlayers = System.Math.Clamp(fdmin, 1, 15);
                    continue;
                case "FindDaters.MaxPlayers":
                    if (int.TryParse(valuePart, out var fdmax)) FindDatersLobbyPatch.maxPlayers = System.Math.Clamp(fdmax, FindDatersLobbyPatch.minPlayers, 15);
                    continue;
                case "FindDaters.UseChatFilter":
                    if (bool.TryParse(valuePart, out var fducf)) FindDatersLobbyPatch.useChatFilter = fducf;
                    continue;
                case "Lobby.AutoReturnAfterMatch":
                    if (bool.TryParse(valuePart, out var larm)) SkidMenu.autoReturnAfterMatch = larm;
                    continue;
                case "ExtendedList.ExtraSlots":
                    if (int.TryParse(valuePart, out var eles)) ExtendedLobbyListPatch.extraSlots = System.Math.Clamp(eles, 5, 30);
                    continue;
                case "KillReach.Range":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var krr)) killReachRange = System.Math.Clamp(krr, 0.5f, 20f);
                    continue;
                case "KillAura.Enabled":
                    if (bool.TryParse(valuePart, out var kae)) features.KillAura.Enabled = kae;
                    continue;
                case "Impostor.KillOtherImpostors":
                    if (bool.TryParse(valuePart, out var koi)) killOtherImpostors = koi;
                    continue;
                case "Impostor.KillImpostors":
                    if (bool.TryParse(valuePart, out var iki)) KillImpostors.Enabled = iki;
                    continue;
                case "KillAura.Range":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var kar)) features.KillAura.Range = System.Math.Clamp(kar, 0.5f, 20f);
                    continue;
                case "KillAura.InfiniteRange":
                    if (bool.TryParse(valuePart, out var kair)) features.KillAura.InfiniteRange = kair;
                    continue;
                case "KillAura.RespectMeeting":
                    if (bool.TryParse(valuePart, out var karm)) features.KillAura.RespectMeeting = karm;
                    continue;
                case "KillAura.RespectVent":
                    if (bool.TryParse(valuePart, out var karv)) features.KillAura.RespectVent = karv;
                    continue;
                case "KillAura.WaitAfterStart":
                    if (bool.TryParse(valuePart, out var kawas)) features.KillAura.WaitAfterStart = kawas;
                    continue;
                case "KillAura.StartDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var kasd)) features.KillAura.StartDelay = System.Math.Clamp(kasd, 1f, 30f);
                    continue;
                case "KillAura.Telemurder":
                    if (bool.TryParse(valuePart, out var kat)) features.KillAura.Telemurder = kat;
                    continue;
                case "KillAura.IgnoreCooldownAsHost":
                    if (bool.TryParse(valuePart, out var kaich)) features.KillAura.IgnoreCooldownAsHost = kaich;
                    continue;
                case "GA.InfiniteRange":
                    if (bool.TryParse(valuePart, out var gair)) gaInfiniteRange = gair;
                    continue;
                case "GA.IgnoreImpostors":
                    if (bool.TryParse(valuePart, out var gaii)) gaIgnoreImpostors = gaii;
                    continue;
                case "Judge.InstantUnlock":
                    if (bool.TryParse(valuePart, out var jiu)) features.JudgeCheats.InstantUnlock = jiu;
                    continue;
                case "Judge.InfiniteGavels":
                    if (bool.TryParse(valuePart, out var jig)) features.JudgeCheats.InfiniteGavels = jig;
                    continue;
                case "Crewmate.InstantPet":
                    if (bool.TryParse(valuePart, out var cip)) instantPet = cip;
                    continue;
                case "Crewmate.SpamPet":
                    if (bool.TryParse(valuePart, out var spp)) spamPet = spp;
                    continue;
                case "Crewmate.SpamPetDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var sppd)) spamPetDelay = System.Math.Clamp(sppd, 0.01f, 1f);
                    continue;
                case "Crewmate.BypassPetWallCheck":
                    if (bool.TryParse(valuePart, out var cbpwc)) _ = cbpwc;
                    continue;
                case "Troll.AutoReportBodies":
                    if (bool.TryParse(valuePart, out var trab)) features.Troll.AutoReportBodies.Enabled = trab;
                    continue;
                case "Troll.AutoReportBodiesDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var trabd)) features.Troll.AutoReportBodies.ReportDelay = System.Math.Clamp(trabd, 0f, 2f);
                    continue;
                case "Troll.AutoReportBodiesCrewOnly":
                    if (bool.TryParse(valuePart, out var trabco)) features.Troll.AutoReportBodies.CrewOnly = trabco;
                    continue;
                case "Troll.FuckGame":
                    if (bool.TryParse(valuePart, out var tfg)) features.FuckGame.Enabled = tfg;
                    continue;
                case "GUI.MenuOpacity":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gmo)) menuOpacity = System.Math.Clamp(gmo, 0.01f, 1f);
                    continue;
                case "Passive.UnlockFeatures":
                    if (bool.TryParse(valuePart, out var puf)) unlockFeatures = puf;
                    continue;
                case "Passive.AntiOverload":
                    if (bool.TryParse(valuePart, out var pao)) antiOverload = pao;
                    continue;
                case "Passive.FreeCosmetics":
                    if (bool.TryParse(valuePart, out var pfc)) freeCosmetics = pfc;
                    continue;
                case "Passive.AvoidPenalties":
                    if (bool.TryParse(valuePart, out var pap)) avoidPenalties = pap;
                    continue;
                case "Passive.CopyLobbyCodeOnDisconnect":
                    if (bool.TryParse(valuePart, out var pclcod)) copyLobbyCodeOnDisconnect = pclcod;
                    continue;
                case "Passive.SpoofAprilFoolsDate":
                    if (bool.TryParse(valuePart, out var psafd)) spoofAprilFoolsDate = psafd;
                    continue;
                case "Passive.RandomizeCosmetics":
                    if (bool.TryParse(valuePart, out var prc)) randomizeCosmetics = prc;
                    continue;
                case "Protections.ForceDTLS":
                    if (bool.TryParse(valuePart, out var pfd)) features.Protections.ForceDTLS.Enabled = pfd;
                    continue;
                case "Protections.BlockServerTeleports":
                    if (bool.TryParse(valuePart, out var pbst)) features.Protections.BlockServerTeleports.Enabled = pbst;
                    continue;
                case "Protections.HardenedReadPackedUInt":
                    if (bool.TryParse(valuePart, out var phrpu)) features.Protections.HardenedReadPackedUInt.Enabled = phrpu;
                    continue;
                case "Protections.BlockInvalidLadderOverload":
                    if (bool.TryParse(valuePart, out var pbilo)) features.Protections.BlockInvalidLadderOverload = pbilo;
                    continue;
                case "Protections.BlockLargeGameMessages":
                    if (bool.TryParse(valuePart, out var pblgm)) features.Protections.BlockLargeGameMessages = pblgm;
                    continue;
                case "Protections.BlockInvalidGameDataMessages":
                    if (bool.TryParse(valuePart, out var pbigdm)) features.Protections.BlockInvalidGameDataMessages = pbigdm;
                    continue;
                case "Protections.BlockUnauthorizedSystemUpdates":
                    if (bool.TryParse(valuePart, out var pbusu)) features.Protections.BlockUnauthorizedSystemUpdates = pbusu;
                    continue;
                case "Protections.ProtectAgainstNonHostKickExploit":
                    if (bool.TryParse(valuePart, out var ppanhke)) features.Protections.ProtectAgainstNonHostKickExploit = ppanhke;
                    continue;
                case "Protections.BlockZiplineForce":
                    if (bool.TryParse(valuePart, out var pbzf)) features.Protections.BlockZiplineForce = pbzf;
                    continue;
                case "Protections.BlockVentTpForce":
                    if (bool.TryParse(valuePart, out var pbvtf)) features.Protections.BlockVentTpForce = pbvtf;
                    continue;
                case "Protections.Votekicks":
                    if (bool.TryParse(valuePart, out var pvk)) features.Protections.Votekicks.Enabled = pvk;
                    continue;
                case "Protections.MemoryAllocationOverload":
                    if (bool.TryParse(valuePart, out var pmao)) features.Protections.MemoryAllocationOverload.Enabled = pmao;
                    continue;
                case "Protections.BypassShapeshiftRatelimits":
                    if (bool.TryParse(valuePart, out var pbsr)) features.Protections.BypassShapeshiftRatelimits.Enabled = pbsr;
                    continue;
                case "Protections.AntiExploits":
                    if (bool.TryParse(valuePart, out var pae)) features.Protections.AntiExploits = pae;
                    continue;
                case "GUI.MenuScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gmsh)) menuScaleH = System.Math.Clamp(gmsh, 50f, 300f);
                    continue;
                case "GUI.MenuScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gmsv)) menuScaleV = System.Math.Clamp(gmsv, 50f, 300f);
                    continue;
                case "GUI.MaxFpsEnabled":
                    if (bool.TryParse(valuePart, out var mfe)) maxFpsEnabled = mfe;
                    continue;
                case "GUI.MaxFpsValue":
                    if (int.TryParse(valuePart, out var mfv)) maxFpsValue = System.Math.Clamp(mfv, 30, 999);
                    continue;
                case "GUI.StretchedResEnabled":
                    if (bool.TryParse(valuePart, out var sre)) stretchedResEnabled = sre;
                    continue;
                case "GUI.StretchedResW":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var srw)) stretchedResW = System.Math.Clamp(srw, 320f, 7680f);
                    continue;
                case "GUI.StretchedResH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var srh)) stretchedResH = System.Math.Clamp(srh, 180f, 4320f);
                    continue;
                case "GUI.ConsoleScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var csh)) consoleScaleH = System.Math.Clamp(csh, 50f, 300f);
                    continue;
                case "GUI.ConsoleScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var csv)) consoleScaleV = System.Math.Clamp(csv, 50f, 300f);
                    continue;
                case "GUI.DoorsScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dsh)) doorsScaleH = System.Math.Clamp(dsh, 50f, 300f);
                    continue;
                case "GUI.DoorsScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dsv)) doorsScaleV = System.Math.Clamp(dsv, 50f, 300f);
                    continue;
                case "GUI.TasksScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var tsh)) tasksScaleH = System.Math.Clamp(tsh, 50f, 300f);
                    continue;
                case "GUI.TasksScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var tsv)) tasksScaleV = System.Math.Clamp(tsv, 50f, 300f);
                    continue;
                case "GUI.ProtectScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var psh)) protectScaleH = System.Math.Clamp(psh, 50f, 300f);
                    continue;
                case "GUI.ProtectScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var psv)) protectScaleV = System.Math.Clamp(psv, 50f, 300f);
                    continue;
                case "GUI.BlockClicksBehindMenu":
                    if (bool.TryParse(valuePart, out var gbcbm)) _ = gbcbm;
                    continue;
                case "Blacklist.Enabled":
                    if (bool.TryParse(valuePart, out var ble)) anticheat.Blacklist.Enabled = ble;
                    continue;
                case "ESP.SkipShhhAnimation":
                    if (bool.TryParse(valuePart, out var essa)) features.Visuals.SkipShhhAnimation.Enabled = essa;
                    continue;
                case "ESP.SkipRoleReveal":
                    if (bool.TryParse(valuePart, out var esrr)) SkipRoleReveal.Enabled = esrr;
                    continue;
                case "Filter.UseImpostor":
                    if (bool.TryParse(valuePart, out var fui)) FindDatersLobbyPatch.useImpostorFilter = fui;
                    continue;
                case "Filter.ImpostorCount":
                    if (int.TryParse(valuePart, out var fic)) FindDatersLobbyPatch.impostorCount = System.Math.Clamp(fic, 1, 3);
                    continue;
                case "Filter.UsePlayer":
                    if (bool.TryParse(valuePart, out var fup)) FindDatersLobbyPatch.usePlayerFilter = fup;
                    continue;
                case "Filter.MinPlayers":
                    if (int.TryParse(valuePart, out var fmin)) FindDatersLobbyPatch.minPlayers = System.Math.Clamp(fmin, 1, 15);
                    continue;
                case "Filter.MaxPlayers":
                    if (int.TryParse(valuePart, out var fmax)) FindDatersLobbyPatch.maxPlayers = System.Math.Clamp(fmax, 1, 15);
                    continue;
                case "Filter.UseChat":
                    if (bool.TryParse(valuePart, out var fuc)) FindDatersLobbyPatch.useChatFilter = fuc;
                    continue;
                case "Filter.UseLang":
                    if (bool.TryParse(valuePart, out var ful)) FindDatersLobbyPatch.useLangFilter = ful;
                    continue;
                case "Filter.SelectedLangs":
                    FindDatersLobbyPatch.selectedLangs.Clear();
                    foreach (var s in valuePart.Split(','))
                        if (int.TryParse(s.Trim(), out var li)) FindDatersLobbyPatch.selectedLangs.Add((SupportedLangs)li);
                    continue;
                case "Filter.UseHostLevel":
                    continue;
                case "Filter.UseHostPlatform":
                    if (bool.TryParse(valuePart, out var fuhp)) FindDatersLobbyPatch.useHostPlatformFilter = fuhp;
                    continue;
                case "Filter.SelectedPlatforms":
                    FindDatersLobbyPatch.selectedPlatforms.Clear();
                    foreach (var s in valuePart.Split(','))
                        if (int.TryParse(s.Trim(), out var pi)) FindDatersLobbyPatch.selectedPlatforms.Add((Platforms)pi);
                    continue;
                case "Filter.UseHostName":
                    if (bool.TryParse(valuePart, out var fuhn)) FindDatersLobbyPatch.useHostNameFilter = fuhn;
                    continue;
                case "Self.CustomBodyType":
                    if (bool.TryParse(valuePart, out var scbt)) SelfTab.CustomBodyType = scbt;
                    continue;
                case "Self.SelectedBodyType":
                    if (int.TryParse(valuePart, out var ssbt)) SelfTab.SelectedBodyType = (PlayerBodyTypes)ssbt;
                    continue;
                case "Self.LongBodyHeight":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var slbh)) SelfTab.LongBodyHeight = System.Math.Clamp(slbh, 0.1f, 5f);
                    continue;
                case "AutoHost.Enabled":
                    if (bool.TryParse(valuePart, out var ahe)) SkidMenu.autoHostEnabled = ahe;
                    continue;
                case "AutoHost.InstantStart":
                    if (bool.TryParse(valuePart, out var ahis)) SkidMenu.autoHostInstantStart = ahis;
                    continue;
                case "AutoHost.CancelBelowMin":
                    if (bool.TryParse(valuePart, out var ahcbm)) SkidMenu.autoHostCancelBelowMin = ahcbm;
                    continue;
                case "AutoHost.WaitLoadedPlayers":
                    if (bool.TryParse(valuePart, out var ahwlp)) SkidMenu.autoHostWaitLoadedPlayers = ahwlp;
                    continue;
                case "AutoHost.ReturnAfterMatch":
                    if (bool.TryParse(valuePart, out var aham)) SkidMenu.autoHostReturnAfterMatch = aham;
                    continue;
                case "AutoHost.ForceLastMinute":
                    if (bool.TryParse(valuePart, out var ahflm)) SkidMenu.autoHostForceLastMinute = ahflm;
                    continue;
                case "AutoHost.MinPlayers":
                    if (int.TryParse(valuePart, out var ahmp)) SkidMenu.autoHostMinPlayers = System.Math.Clamp(ahmp, 1, 15);
                    continue;
                case "AutoHost.ForceMinPlayers":
                    if (int.TryParse(valuePart, out var ahfmp)) SkidMenu.autoHostForceMinPlayers = System.Math.Clamp(ahfmp, 1, 15);
                    continue;
                case "AutoHost.WarmupSeconds":
                    if (int.TryParse(valuePart, out var ahws)) SkidMenu.autoHostWarmupSeconds = System.Math.Clamp(ahws, 0, 120);
                    continue;
                case "AutoHost.StartDelaySeconds":
                    if (int.TryParse(valuePart, out var ahsds)) SkidMenu.autoHostStartDelaySeconds = System.Math.Clamp(ahsds, 0, 180);
                    continue;
                case "AutoHost.FastStartPlayers":
                    if (int.TryParse(valuePart, out var ahfsp)) SkidMenu.autoHostFastStartPlayers = System.Math.Clamp(ahfsp, 0, 15);
                    continue;
                case "AutoHost.FastStartDelaySeconds":
                    if (int.TryParse(valuePart, out var ahfsds)) SkidMenu.autoHostFastStartDelaySeconds = System.Math.Clamp(ahfsds, 0, 60);
                    continue;
                case "AutoHost.LoadGraceSeconds":
                    if (int.TryParse(valuePart, out var ahlgs)) SkidMenu.autoHostLoadGraceSeconds = System.Math.Clamp(ahlgs, 0, 90);
                    continue;
                case "AutoHost.ForceAfterMinutes":
                    if (int.TryParse(valuePart, out var ahfam)) SkidMenu.autoHostForceAfterMinutes = System.Math.Clamp(ahfam, 0, 10);
                    continue;
                case "AutoHost.BackoffSeconds":
                    if (int.TryParse(valuePart, out var ahbs)) SkidMenu.autoHostBackoffSeconds = System.Math.Clamp(ahbs, 2, 60);
                    continue;
                case "Console.ShowConsole":
                    if (bool.TryParse(valuePart, out var csc)) showConsole = csc;
                    continue;
                case "Console.LogDeaths":
                    if (bool.TryParse(valuePart, out var cld)) logDeaths = cld;
                    continue;
                case "Console.LogShapeshiftInto":
                    if (bool.TryParse(valuePart, out var clsi)) logShapeshiftInto = clsi;
                    continue;
                case "Console.LogShapeshiftRevert":
                    if (bool.TryParse(valuePart, out var clsr)) logShapeshiftRevert = clsr;
                    continue;
                case "Console.LogVentIn":
                    if (bool.TryParse(valuePart, out var clvi)) logVentIn = clvi;
                    continue;
                case "Console.LogVentOut":
                    if (bool.TryParse(valuePart, out var clvo2)) logVentOut = clvo2;
                    continue;
                case "Console.LogSabotages":
                    if (bool.TryParse(valuePart, out var clsab)) logSabotages = clsab;
                    continue;
                case "Console.LogMeetingCalled":
                    if (bool.TryParse(valuePart, out var clmc)) logMeetingCalled = clmc;
                    continue;
                case "Console.LogBodyReport":
                    if (bool.TryParse(valuePart, out var clbr)) logBodyReport = clbr;
                    continue;
                case "Console.LogEjections":
                    if (bool.TryParse(valuePart, out var cle)) logEjections = cle;
                    continue;
                case "Console.LogVerdict":
                    if (bool.TryParse(valuePart, out var clvd)) logVerdict = clvd;
                    continue;
                case "Console.LogVotes":
                    if (bool.TryParse(valuePart, out var clvo)) logVotes = clvo;
                    continue;
                case "Console.LogVotekicks":
                    if (bool.TryParse(valuePart, out var clvk)) logVotekicks = clvk;
                    continue;
                case "Console.LogChat":
                    if (bool.TryParse(valuePart, out var clch)) logChat = clch;
                    continue;
                case "Chat.CopyMessage":
                    if (bool.TryParse(valuePart, out var ccm)) copyMessage = ccm;
                    continue;
                case "Console.LogDisconnects":
                    if (bool.TryParse(valuePart, out var cldis)) logDisconnects = cldis;
                    continue;
                case "Console.LogJoins":
                    if (bool.TryParse(valuePart, out var clj)) logJoins = clj;
                    continue;
                case "Console.LogPhantomVanish":
                    if (bool.TryParse(valuePart, out var clpv)) logPhantomVanish = clpv;
                    continue;
                case "Console.LogPhantomReappear":
                    if (bool.TryParse(valuePart, out var clpr)) logPhantomReappear = clpr;
                    continue;
                case "Console.LogTaskCompleted":
                    if (bool.TryParse(valuePart, out var cltc)) logTaskCompleted = cltc;
                    continue;
                case "Console.MaxLogEntries":
                    if (int.TryParse(valuePart, out var clme)) maxLogEntries = System.Math.Clamp(clme, 50, 2000);
                    continue;
                case "GUI.ChatScaleH":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var cth)) chatScaleH = System.Math.Clamp(cth, 50f, 300f);
                    continue;
                case "GUI.ChatScaleV":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var ctv)) chatScaleV = System.Math.Clamp(ctv, 50f, 300f);
                    continue;
                case "Console.ShowChatUI":
                    if (bool.TryParse(valuePart, out var sch)) showChatUI = sch;
                    continue;
                case "Console.ChatMaxEntries":
                    if (int.TryParse(valuePart, out var ccme)) chatMaxEntries = System.Math.Clamp(ccme, 50, 2000);
                    continue;
                case "LagComp.Enabled":
                    if (bool.TryParse(valuePart, out var lce)) features.LagCompensation.Enabled = lce;
                    continue;
                case "LagComp.FreezePosition":
                    if (bool.TryParse(valuePart, out var lcfp)) features.LagCompensation.FreezePosition = lcfp;
                    continue;
                case "LagComp.Jitter":
                    if (bool.TryParse(valuePart, out var lcj)) features.LagCompensation.Jitter = lcj;
                    continue;
                case "LagComp.SkipTicks":
                    if (int.TryParse(valuePart, out var lcst)) features.LagCompensation.SkipTicks = System.Math.Clamp(lcst, 1, 60);
                    continue;
                case "Invisibility.Enabled":
                    if (bool.TryParse(valuePart, out var inv)) features.Invisibility.Enabled = inv;
                    continue;
                case "Invisibility.OnlyInGame":
                    if (bool.TryParse(valuePart, out var invOnly)) features.Invisibility.OnlyInGame = invOnly;
                    continue;
                case "LagComp.JitterMin":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lcjn)) features.LagCompensation.JitterMin = System.Math.Clamp(lcjn, 1f, 60f);
                    continue;
                case "LagComp.JitterMax":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lcjx)) features.LagCompensation.JitterMax = System.Math.Clamp(lcjx, 1f, 60f);
                    continue;
                case "FR.RandLevel":
                    if (bool.TryParse(valuePart, out var frl)) SkidMenu.frRandLevel = frl;
                    continue;
                case "FR.SpamEnabled":
                    if (bool.TryParse(valuePart, out var frse)) SpoofingTab.frSpamEnabled = frse;
                    continue;
                case "FR.SpamDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var frsd)) SpoofingTab.frSpamDelay = System.Math.Clamp(frsd, 0.1f, 10f);
                    continue;
                case "FR.ShowNotification":
                    if (bool.TryParse(valuePart, out var frsn)) FullyRandomizeTriggers.ShowNotification = frsn;
                    continue;
                case "FR.RandPlatform":
                    if (bool.TryParse(valuePart, out var frp)) SkidMenu.frRandPlatform = frp;
                    continue;
                case "FR.RandName":
                    if (bool.TryParse(valuePart, out var frn)) SkidMenu.frRandName = frn;
                    continue;
                case "FR.RandHat":
                    if (bool.TryParse(valuePart, out var frh)) SkidMenu.frRandHat = frh;
                    continue;
                case "FR.RandSkin":
                    if (bool.TryParse(valuePart, out var frs)) SkidMenu.frRandSkin = frs;
                    continue;
                case "FR.RandVisor":
                    if (bool.TryParse(valuePart, out var frv)) SkidMenu.frRandVisor = frv;
                    continue;
                case "FR.RandPet":
                    if (bool.TryParse(valuePart, out var frpe)) SkidMenu.frRandPet = frpe;
                    continue;
                case "FR.RandNameplate":
                    if (bool.TryParse(valuePart, out var frnp)) SkidMenu.frRandNameplate = frnp;
                    continue;
                case "FR.RandColor":
                    if (bool.TryParse(valuePart, out var frc)) SkidMenu.frRandColor = frc;
                    continue;
                case "FR.RpcDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var frd)) { SkidMenu.frRpcDelay = frd; SpoofingTab.frRpcDelayTemp = frd; }
                    continue;
                case "FR.OnDeath":
                    if (bool.TryParse(valuePart, out var fod)) FullyRandomizeTriggers.OnDeath = fod;
                    continue;
                case "FR.OnKill":
                    if (bool.TryParse(valuePart, out var fok)) FullyRandomizeTriggers.OnKill = fok;
                    continue;
                case "FR.OnMeetingStart":
                    if (bool.TryParse(valuePart, out var foms)) FullyRandomizeTriggers.OnMeetingStart = foms;
                    continue;
                case "FR.OnMeetingEnd":
                    if (bool.TryParse(valuePart, out var fome)) FullyRandomizeTriggers.OnMeetingEnd = fome;
                    continue;
                case "FR.OnLobbyLeave":
                    if (bool.TryParse(valuePart, out var foll)) FullyRandomizeTriggers.OnLobbyLeave = foll;
                    continue;
                case "FR.OnGameEnd":
                    if (bool.TryParse(valuePart, out var foge)) FullyRandomizeTriggers.OnGameEnd = foge;
                    continue;
                case "FR.OnShapeshift":
                    if (bool.TryParse(valuePart, out var foss)) FullyRandomizeTriggers.OnShapeshift = foss;
                    continue;
                case "FR.OnVent":
                    if (bool.TryParse(valuePart, out var fov)) FullyRandomizeTriggers.OnVent = fov;
                    continue;
                case "FR.OnTaskComplete":
                    if (bool.TryParse(valuePart, out var fotc)) FullyRandomizeTriggers.OnTaskComplete = fotc;
                    continue;
                case "FR.OnEjected":
                    if (bool.TryParse(valuePart, out var foe)) FullyRandomizeTriggers.OnEjected = foe;
                    continue;
                case "FR.OnSabotage":
                    if (bool.TryParse(valuePart, out var fosab)) FullyRandomizeTriggers.OnSabotage = fosab;
                    continue;
                case "FR.OnExitVent":
                    if (bool.TryParse(valuePart, out var foev)) FullyRandomizeTriggers.OnExitVent = foev;
                    continue;
                case "FR.OnShapeshiftBack":
                    if (bool.TryParse(valuePart, out var fossb)) FullyRandomizeTriggers.OnShapeshiftBack = fossb;
                    continue;
                case "FR.OnVanish":
                    if (bool.TryParse(valuePart, out var fovan)) FullyRandomizeTriggers.OnVanish = fovan;
                    continue;
                case "FR.OnReappear":
                    if (bool.TryParse(valuePart, out var forea)) FullyRandomizeTriggers.OnReappear = forea;
                    continue;
                case "FR.OnVotekicked":
                    if (bool.TryParse(valuePart, out var fovk)) FullyRandomizeTriggers.OnVotekicked = fovk;
                    continue;
                case "FR.OnPlayerJoin":
                    if (bool.TryParse(valuePart, out var fopj)) FullyRandomizeTriggers.OnPlayerJoin = fopj;
                    continue;
                case "FR.OnPlayerLeave":
                    if (bool.TryParse(valuePart, out var fopl)) FullyRandomizeTriggers.OnPlayerLeave = fopl;
                    continue;
                case "Self.DarkGameTheme":
                    if (bool.TryParse(valuePart, out var sdgt)) DarkMode.Enabled = sdgt;
                    continue;
                case "Self.CustomGameTheme":
                    if (bool.TryParse(valuePart, out var scgt)) CustomGameTheme.Enabled = scgt;
                    continue;
                case "Self.GameBgColor":
                    SelfTab.BgHex = valuePart;
                    if (ColorUtility.TryParseHtmlString("#" + valuePart, out Color bgc)) CustomGameTheme.BgColor = bgc;
                    continue;
                case "Self.GameTextColor":
                    SelfTab.TextHex = valuePart;
                    if (ColorUtility.TryParseHtmlString("#" + valuePart, out Color txc)) CustomGameTheme.TextColor = txc;
                    continue;
                case "Self.ChatFont":
                    if (bool.TryParse(valuePart, out var scf)) ChatFontChanger.Enabled = scf;
                    continue;
                case "Self.ChatFontType":
                    if (int.TryParse(valuePart, out var scft)) ChatFontChanger.FontType = System.Math.Clamp(scft, 0, 20);
                    continue;
                case "NameSpoof.SpoofedName":
                    SkidMenu.nameSpoofName = valuePart;
                    continue;
                case "NameSpoof.Enabled":
                    if (bool.TryParse(valuePart, out var nsse)) { SkidMenu.nameSpoofEnabled = nsse; if (nsse && !string.IsNullOrWhiteSpace(SkidMenu.nameSpoofName)) NameSpoofer.ApplyName(SkidMenu.nameSpoofName); }
                    continue;
                case "NameSpoof.Mode":
                    if (int.TryParse(valuePart, out var nsme)) { SkidMenu.nameSpoofMode = nsme; NameSpoofer.Mode = (NameSpoofer.RandomizerMode)nsme; }
                    continue;
                case "NameSpoof.Length":
                    if (int.TryParse(valuePart, out var nsle)) { SkidMenu.nameSpoofLength = nsle; NameSpoofer.RandomLength = nsle; }
                    continue;
                case "Chat.History":
                    if (bool.TryParse(valuePart, out var ch)) features.ChatEnhancements.EnableChatHistory = ch;
                    continue;
                case "Chat.ExtendedChat":
                    if (bool.TryParse(valuePart, out var cec)) features.ChatEnhancements.EnableExtendedChat = cec;
                    continue;
                case "Chat.ColorCommand":
                    if (bool.TryParse(valuePart, out var ccc)) features.ChatEnhancements.EnableColorCommand = ccc;
                    continue;
                case "Chat.SenderEnabled":
                    if (bool.TryParse(valuePart, out var cse)) features.ChatSender.Enabled = cse;
                    continue;
                case "Chat.SenderMessage":
                    if (!string.IsNullOrWhiteSpace(valuePart)) { features.ChatSender.Message = valuePart; }
                    continue;
                case "Chat.SenderDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var csd)) features.ChatSender.Delay = System.Math.Clamp(csd, 0.5f, 10f);
                    continue;
                case "Chat.BypassCharLimit":
                    if (bool.TryParse(valuePart, out var cbcl)) ChatTab.BypassCharLimit = cbcl;
                    continue;
                case "Chat.OnJoinEnabled":
                    if (bool.TryParse(valuePart, out var coje)) features.ChatSender.OnJoinEnabled = coje;
                    continue;
                case "Chat.OnJoinMessage":
                    features.ChatSender.OnJoinMessage = valuePart;
                    continue;
                case "Chat.OnDeathEnabled":
                    if (bool.TryParse(valuePart, out var code)) features.ChatSender.OnDeathEnabled = code;
                    continue;
                case "Chat.OnDeathMessage":
                    features.ChatSender.OnDeathMessage = valuePart;
                    continue;
                case "Chat.OnMeetingEnabled":
                    if (bool.TryParse(valuePart, out var come)) features.ChatSender.OnMeetingEnabled = come;
                    continue;
                case "Chat.OnMeetingMessage":
                    features.ChatSender.OnMeetingMessage = valuePart;
                    continue;
                case "Chat.OnKillEnabled":
                    if (bool.TryParse(valuePart, out var coke)) features.ChatSender.OnKillEnabled = coke;
                    continue;
                case "Chat.OnKillMessage":
                    features.ChatSender.OnKillMessage = valuePart;
                    continue;
                case "Chat.OnEjectionEnabled":
                    if (bool.TryParse(valuePart, out var coee)) features.ChatSender.OnEjectionEnabled = coee;
                    continue;
                case "Chat.OnEjectionMessage":
                    features.ChatSender.OnEjectionMessage = valuePart;
                    continue;
                case "Spoofer.ShouldSpoofVersion":
                    if (bool.TryParse(valuePart, out var ssv)) Spoofer.shouldSpoofVersion = ssv;
                    continue;
                case "Spoofer.SpoofedVersion":
                    if (int.TryParse(valuePart, out var spv)) Spoofer.spoofedVersion = spv;
                    continue;
                case "Spoofer.UseModdedProtocol":
                    if (bool.TryParse(valuePart, out var sump)) Spoofer.useModdedProtocol = sump;
                    continue;
                case "Spoofer.XboxId":
                    if (ulong.TryParse(valuePart, out var sxid)) Spoofer.spoofedXboxId = sxid;
                    continue;
                case "Spoofer.PsnId":
                    if (ulong.TryParse(valuePart, out var spsid)) Spoofer.spoofedPsnId = spsid;
                    continue;
                case "GUI.MenuKeybind":
                    if (!string.IsNullOrWhiteSpace(valuePart)) SkidMenu.menuKeybind = valuePart;
                    continue;
                case "GUI.MenuColor":
                    SkidMenu.menuHtmlColor = valuePart;
                    continue;
                case "GUI.OpenOnMouse":
                    if (bool.TryParse(valuePart, out var gom)) SkidMenu.menuOpenOnMouse = gom;
                    continue;
                case "GUI.KeepSubwindows":
                    if (bool.TryParse(valuePart, out var gks)) SkidMenu.menuKeepSubwindowsOpen = gks;
                    continue;
                case "Spoof.Level":
                    SkidMenu.spoofLevel = valuePart;
                    continue;
                case "Spoof.LevelMin":
                    if (int.TryParse(valuePart, out var slmin)) SkidMenu.spoofLevelRandomMin = System.Math.Clamp(slmin, 1, 100001);
                    continue;
                case "Spoof.LevelMax":
                    if (int.TryParse(valuePart, out var slmax)) SkidMenu.spoofLevelRandomMax = System.Math.Clamp(slmax, 1, 100001);
                    continue;
                case "Spoof.Platform":
                    SkidMenu.spoofPlatform = valuePart;
                    if (Utils.StringToPlatformType(valuePart, out Platforms? spoofedPlat))
                        Spoofer.spoofedPlatform = (Platforms)spoofedPlat;
                    continue;
                case "Spoof.PlatformExclusions":
                    SkidMenu.spoofPlatformExclusions = valuePart;
                    continue;
                case "Guest.FriendCode":
                    SkidMenu.guestFriendCode = valuePart;
                    continue;
                case "Guest.Enabled":
                    if (bool.TryParse(valuePart, out var ge)) SkidMenu.guestMode = ge;
                    continue;
                case "Profile.AutoLoad":
                    if (bool.TryParse(valuePart, out var pal)) SkidMenu.autoLoadProfile = pal;
                    continue;
                case "Config.Editor":
                    if (!string.IsNullOrWhiteSpace(valuePart)) SkidMenu.configEditor = valuePart;
                    continue;
                case "Dating.FindDaters":
                    if (bool.TryParse(valuePart, out var dfd)) { CheatToggles.findDaters = dfd; }
                    continue;
                case "Dating.ExtendedList":
                    if (bool.TryParse(valuePart, out var del)) { CheatToggles.extendedLobbyList = del; }
                    continue;
                case "AC.MaxLevel":
                    if (int.TryParse(valuePart, out var aml)) SkidMenu.maxPlayerLevel = System.Math.Clamp(aml, 1, 100000);
                    continue;
                case "AC.MaxTeleport":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var amt)) SkidMenu.maxTeleportDistance = System.Math.Clamp(amt, 1f, 500f);
                    continue;
                case "Chat.HistorySize":
                    if (int.TryParse(valuePart, out var chs)) SkidMenu.chatHistorySize = System.Math.Clamp(chs, 5, 500);
                    continue;
                case "Chat.HistoryInfinite":
                    if (bool.TryParse(valuePart, out var chi)) SkidMenu.chatHistoryInfinite = chi;
                    continue;
                case "Log.PlayerJoin":
                    if (bool.TryParse(valuePart, out var lpj)) SkidMenu.logPlayerJoin = lpj;
                    continue;
                case "Log.GuardianProtect":
                    if (bool.TryParse(valuePart, out var lgp)) { CheatToggles.logGuardianProtect = lgp; }
                    continue;
                case "Log.ShowDistance":
                    if (bool.TryParse(valuePart, out var lsd)) SkidMenu.logShowDistance = lsd;
                    continue;
                case "Log.Advanced":
                    if (bool.TryParse(valuePart, out var lad)) SkidMenu.advancedLogging = lad;
                    continue;
                case "FR.NameHostOnly":
                    if (bool.TryParse(valuePart, out var frnho)) SkidMenu.frRandNameHostOnly = frnho;
                    continue;
                case "FR.ColorHostOnly":
                    if (bool.TryParse(valuePart, out var frcho)) SkidMenu.frRandColorHostOnly = frcho;
                    continue;
                case "FR.Trigger.OnDeath":
                    if (bool.TryParse(valuePart, out var tOnDeath)) { FullyRandomizeTriggers.OnDeath = tOnDeath; }
                    continue;
                case "FR.Trigger.OnKill":
                    if (bool.TryParse(valuePart, out var tOnKill)) { FullyRandomizeTriggers.OnKill = tOnKill; }
                    continue;
                case "FR.Trigger.OnMeetingStart":
                    if (bool.TryParse(valuePart, out var tOnMeetingStart)) { FullyRandomizeTriggers.OnMeetingStart = tOnMeetingStart; }
                    continue;
                case "FR.Trigger.OnMeetingEnd":
                    if (bool.TryParse(valuePart, out var tOnMeetingEnd)) { FullyRandomizeTriggers.OnMeetingEnd = tOnMeetingEnd; }
                    continue;
                case "FR.Trigger.OnLobbyLeave":
                    if (bool.TryParse(valuePart, out var tOnLobbyLeave)) { FullyRandomizeTriggers.OnLobbyLeave = tOnLobbyLeave; }
                    continue;
                case "FR.Trigger.OnGameEnd":
                    if (bool.TryParse(valuePart, out var tOnGameEnd)) { FullyRandomizeTriggers.OnGameEnd = tOnGameEnd; }
                    continue;
                case "FR.Trigger.OnShapeshift":
                    if (bool.TryParse(valuePart, out var tOnShapeshift)) { FullyRandomizeTriggers.OnShapeshift = tOnShapeshift; }
                    continue;
                case "FR.Trigger.OnShapeshiftBack":
                    if (bool.TryParse(valuePart, out var tOnShapeshiftBack)) FullyRandomizeTriggers.OnShapeshiftBack = tOnShapeshiftBack;
                    continue;
                case "FR.Trigger.OnVent":
                    if (bool.TryParse(valuePart, out var tOnVent)) { FullyRandomizeTriggers.OnVent = tOnVent; }
                    continue;
                case "FR.Trigger.OnExitVent":
                    if (bool.TryParse(valuePart, out var tOnExitVent)) FullyRandomizeTriggers.OnExitVent = tOnExitVent;
                    continue;
                case "FR.Trigger.OnTaskComplete":
                    if (bool.TryParse(valuePart, out var tOnTaskComplete)) { FullyRandomizeTriggers.OnTaskComplete = tOnTaskComplete; }
                    continue;
                case "FR.Trigger.OnEjected":
                    if (bool.TryParse(valuePart, out var tOnEjected)) FullyRandomizeTriggers.OnEjected = tOnEjected;
                    continue;
                case "FR.Trigger.OnSabotage":
                    if (bool.TryParse(valuePart, out var tOnSabotage)) FullyRandomizeTriggers.OnSabotage = tOnSabotage;
                    continue;
                case "FR.Trigger.OnVanish":
                    if (bool.TryParse(valuePart, out var tOnVanish)) FullyRandomizeTriggers.OnVanish = tOnVanish;
                    continue;
                case "FR.Trigger.OnReappear":
                    if (bool.TryParse(valuePart, out var tOnReappear)) FullyRandomizeTriggers.OnReappear = tOnReappear;
                    continue;
                case "FR.Trigger.OnVotekicked":
                    if (bool.TryParse(valuePart, out var tOnVotekicked)) FullyRandomizeTriggers.OnVotekicked = tOnVotekicked;
                    continue;
                case "FR.Trigger.OnPlayerJoin":
                    if (bool.TryParse(valuePart, out var tOnPlayerJoin)) FullyRandomizeTriggers.OnPlayerJoin = tOnPlayerJoin;
                    continue;
                case "FR.Trigger.OnPlayerLeave":
                    if (bool.TryParse(valuePart, out var tOnPlayerLeave)) FullyRandomizeTriggers.OnPlayerLeave = tOnPlayerLeave;
                    continue;
                case "FR.Trigger.ShowNotification":
                    if (bool.TryParse(valuePart, out var tShowNotification)) FullyRandomizeTriggers.ShowNotification = tShowNotification;
                    continue;
                case "ZoomOut.ScrollSpeed":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var zss)) MalumESP.ZoomScrollSpeed = System.Math.Clamp(zss, 0.5f, 5f);
                    continue;
                case "ZoomOut.Smoothness":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var zsm)) MalumESP.ZoomSmoothness = System.Math.Clamp(zsm, 0f, 20f);
                    continue;
                case "ZoomOut.MaxDistance":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var zmd)) MalumESP.ZoomMaxDistance = System.Math.Clamp(zmd, 5f, 50f);
                    continue;
                case "ZoomOut.MinDistance":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var zmind)) MalumESP.ZoomMinDistance = System.Math.Clamp(zmind, 1f, 20f);
                    continue;
                case "Freecam.Speed":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fcs)) MalumESP.FreecamSpeed = System.Math.Clamp(fcs, 1f, 50f);
                    continue;
                case "Freecam.Smoothness":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fcsm)) MalumESP.FreecamSmoothness = System.Math.Clamp(fcsm, 0f, 20f);
                    continue;
                case "ESP.SeePlayersInVents":
                    if (bool.TryParse(valuePart, out var spiv)) SeePlayersInVents.Enabled = spiv;
                    continue;
                case "ESP.SeePhantoms":
                    if (bool.TryParse(valuePart, out var esp)) SeePlayersInVents.SeePhantoms = esp;
                    continue;
                case "ESP.LobbyTimer":
                    if (bool.TryParse(valuePart, out var elt)) LobbyTimer.Enabled = elt;
                    continue;
                case "ESP.SubCtx":
                    var sc = valuePart.Split(',');
                    if (sc.Length >= 11)
                    {
                        if (byte.TryParse(sc[0],  out var b0))  ESPContexts.ShowRole     = b0;
                        if (byte.TryParse(sc[1],  out var b1))  ESPContexts.ShowInfo     = b1;
                        if (byte.TryParse(sc[2],  out var b2))  ESPContexts.KillCooldown = b2;
                        if (byte.TryParse(sc[3],  out var b3))  ESPContexts.Tasks        = b3;
                        if (byte.TryParse(sc[4],  out var b4))  ESPContexts.IsHost       = b4;
                        if (byte.TryParse(sc[5],  out var b5))  ESPContexts.Level        = b5;
                        if (byte.TryParse(sc[6],  out var b6))  ESPContexts.Platform     = b6;
                        if (byte.TryParse(sc[7],  out var b7))  ESPContexts.Votekicks    = b7;
                        if (byte.TryParse(sc[8],  out var b8))  ESPContexts.FriendCode   = b8;
                        if (byte.TryParse(sc[9],  out var b9))  ESPContexts.Puid         = b9;
                        if (byte.TryParse(sc[10], out var b10)) ESPContexts.DeviceId     = b10;
                        if (sc.Length >= 12 && byte.TryParse(sc[11], out var b11)) ESPContexts.ModUser = b11;
                    }
                    continue;
                case "Dummy.WalkToTasks":
                    if (bool.TryParse(valuePart, out var dwt)) DummySpawner.WalkToTasks = dwt;
                    continue;
                case "Dummy.FixSabotages":
                    if (bool.TryParse(valuePart, out var dfs)) DummySpawner.FixSabotages = dfs;
                    continue;
                case "Dummy.ReportAndChat":
                    if (bool.TryParse(valuePart, out var drc)) DummySpawner.ReportAndChat = drc;
                    continue;
                case "Dummy.UseKeybind":
                    if (bool.TryParse(valuePart, out var duk)) DummySpawner.UseKeybind = duk;
                    continue;
                case "Dummy.SpawnKey":
                    if (System.Enum.TryParse<KeyCode>(valuePart, out var dsk)) DummySpawner.SpawnKey = dsk;
                    continue;
                case "Dummy.SpamEnabled":
                    if (bool.TryParse(valuePart, out var dse)) DummySpawner.SpamEnabled = dse;
                    continue;
                case "Dummy.SpamDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dsd)) DummySpawner.SpamDelay = System.Math.Clamp(dsd, 0.1f, 10f);
                    continue;
                case "HO.BypassHostOnly":
                    if (bool.TryParse(valuePart, out var hobo)) bypassHostOnly = hobo;
                    continue;
                case "HO.KillVanished":
                    if (bool.TryParse(valuePart, out var hokv)) killVanished = hokv;
                    continue;
                case "HO.KillAnyone":
                    if (bool.TryParse(valuePart, out var hoka)) killAnyone = hoka;
                    continue;
                case "HO.NoKillCd":
                    if (bool.TryParse(valuePart, out var honkcd)) noKillCd = honkcd;
                    continue;
                case "HO.ShowProtectMenu":
                    if (bool.TryParse(valuePart, out var hospm)) showProtectMenu = hospm;
                    continue;
                case "HO.NoTaskMode":
                    if (bool.TryParse(valuePart, out var hontm)) noTaskMode = hontm;
                    continue;
                case "HO.NoSettingLimit":
                    if (bool.TryParse(valuePart, out var honsl)) noSettingLimit = honsl;
                    continue;
                case "HO.BanMidGame":
                    if (bool.TryParse(valuePart, out var hobmg)) Host.BanMidGame.Enabled = hobmg;
                    continue;
                case "HO.FlippedSkeld":
                    if (bool.TryParse(valuePart, out var hofs)) Host.FlippedSkeld = hofs;
                    continue;
                case "HO.SkipMeeting":
                    if (bool.TryParse(valuePart, out var hoskm)) skipMeeting = hoskm;
                    continue;
                case "HO.VoteImmune":
                    if (bool.TryParse(valuePart, out var hovi)) voteImmune = hovi;
                    continue;
                case "HO.JudgeImmune":
                    if (bool.TryParse(valuePart, out var hoji)) judgeImmune = hoji;
                    continue;
                case "HO.EjectPlayer":
                    if (bool.TryParse(valuePart, out var hoep)) ejectPlayer = hoep;
                    continue;
                case "HO.ForceStartGame":
                    if (bool.TryParse(valuePart, out var hofsg)) forceStartGame = hofsg;
                    continue;
                case "HO.NoGameEnd":
                    if (bool.TryParse(valuePart, out var honge)) noGameEnd = honge;
                    continue;
                case "HO.DisableMeetings":
                    if (bool.TryParse(valuePart, out var hodm)) Host.DisableMeetings.Enabled = hodm;
                    continue;
                case "HO.DisableSabotages":
                    if (bool.TryParse(valuePart, out var hods)) Host.DisableSabotages.Enabled = hods;
                    continue;
                case "HO.DisableCloseDoors":
                    if (bool.TryParse(valuePart, out var hodcd)) Host.DisableCloseDoors.Enabled = hodcd;
                    continue;
                case "HO.DisableCameras":
                    if (bool.TryParse(valuePart, out var hodcam)) Host.DisableCameras.Enabled = hodcam;
                    continue;
                case "HO.PreGameRoleForce":
                    if (bool.TryParse(valuePart, out var hopgrf)) HostFeatures.preGameRoleForce = hopgrf;
                    continue;
                case "HO.PreGameImpCount":
                    if (int.TryParse(valuePart, out var hopgic)) HostFeatures.preGameImpCount = System.Math.Clamp(hopgic, 1, 5);
                    continue;
                case "HO.DiscoParty":
                    if (bool.TryParse(valuePart, out var hodp)) SkidMenu.routines.discoHost.Enabled = hodp;
                    continue;
                case "HO.DiscoDelay":
                    if (float.TryParse(valuePart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var hodd)) SkidMenu.routines.discoHost.randomizationDelay = System.Math.Clamp(hodd, 0.1f, 2f);
                    continue;
                case "HO.BlockLowLevels":
                    if (bool.TryParse(valuePart, out var hobll)) Host.BlockLowLevels.Enabled = hobll;
                    continue;
                case "HO.BlockLowLevelsMin":
                    if (uint.TryParse(valuePart, out var hobllm)) Host.BlockLowLevels.MinLevel = System.Math.Clamp(hobllm, 0u, 100u);
                    continue;
                case "BL.AutoAddFlagged":
                    if (bool.TryParse(valuePart, out var blaf)) anticheat.Blacklist.AutoAddFlagged = blaf;
                    continue;
                case "BL.AutoAddModDetected":
                    if (bool.TryParse(valuePart, out var blamd)) anticheat.Blacklist.AutoAddModDetected = blamd;
                    continue;
                case "BL.AutoPunish":
                    if (bool.TryParse(valuePart, out var blap)) anticheat.Blacklist.AutoPunish = blap;
                    continue;
                case "BL.NotifyOnJoin":
                    if (bool.TryParse(valuePart, out var blnoj)) anticheat.Blacklist.NotifyOnJoin = blnoj;
                    continue;
                case "BL.KickOnJoin":
                    if (bool.TryParse(valuePart, out var blkoj)) anticheat.Blacklist.KickOnJoin = blkoj;
                    continue;
                case "BL.BanOnJoin":
                    if (bool.TryParse(valuePart, out var blboj)) anticheat.Blacklist.BanOnJoin = blboj;
                    continue;
                case "BL.VentKickOnJoin":
                    if (bool.TryParse(valuePart, out var blvkoj)) anticheat.Blacklist.VentKickOnJoin = blvkoj;
                    continue;
                case "AC.Enabled":
                    if (bool.TryParse(valuePart, out var ace)) anticheat.Anticheat.Enabled = ace;
                    continue;
                case "AC.SendNotification":
                    if (bool.TryParse(valuePart, out var acsn)) anticheat.Anticheat.sendNotification = acsn;
                    continue;
                case "AC.DiscardRpc":
                    if (bool.TryParse(valuePart, out var acdr)) anticheat.Anticheat.discardRpc = acdr;
                    continue;
                case "AC.CheckSpoofedPlatforms":
                    if (bool.TryParse(valuePart, out var accsp)) anticheat.Anticheat.CheckSpoofedPlatforms = accsp;
                    continue;
                case "AC.Punishment":
                    if (int.TryParse(valuePart, out var acp)) anticheat.Anticheat.punishment = (anticheat.Anticheat.Punishments)System.Math.Clamp(acp, 0, 3);
                    continue;
                case "AC.NonHostPunishment":
                    if (int.TryParse(valuePart, out var acnhp)) anticheat.Anticheat.nonHostPunishment = (anticheat.Anticheat.NonHostPunishments)System.Math.Clamp(acnhp, 0, 3);
                    continue;
                case "MD.Enabled":
                    if (bool.TryParse(valuePart, out var mde)) anticheat.ModDetection.Enabled = mde;
                    continue;
                case "Privacy.HideDeviceId":
                    if (bool.TryParse(valuePart, out var phdi)) hideDeviceId = phdi;
                    continue;
                case "Privacy.SpoofDeviceId":
                    if (bool.TryParse(valuePart, out var psdi)) spoofDeviceId = psdi;
                    continue;
                case "Privacy.CustomDeviceId":
                    spoofDeviceIdCustom = valuePart;
                    continue;
                case "Privacy.DisableTelemetry":
                    if (bool.TryParse(valuePart, out var pdt)) disableTelemetry = pdt;
                    continue;
                case "Privacy.SpoofTelemetry":
                    if (bool.TryParse(valuePart, out var pst)) spoofTelemetry = pst;
                    continue;
                case "WindowRect.Menu":
                    if (TryParseRect(valuePart, out var rm)) { MenuUI.PendingRect = rm; MenuUI.PendingRectSet = true; if (MenuUI.Instance != null) MenuUI.Instance.WindowRect = rm; }
                    continue;
                case "WindowRect.Console":
                    if (TryParseRect(valuePart, out var rc)) { ConsoleUI.PendingRect = rc; ConsoleUI.PendingRectSet = true; if (ConsoleUI.Instance != null) ConsoleUI.Instance.WindowRect = rc; }
                    continue;
                case "WindowRect.Doors":
                    if (TryParseRect(valuePart, out var rd)) { DoorsUI.PendingRect = rd; DoorsUI.PendingRectSet = true; if (DoorsUI.Instance != null) DoorsUI.Instance.WindowRect = rd; }
                    continue;
                case "WindowRect.Tasks":
                    if (TryParseRect(valuePart, out var rt)) { TasksUI.PendingRect = rt; TasksUI.PendingRectSet = true; if (TasksUI.Instance != null) TasksUI.Instance.WindowRect = rt; }
                    continue;
            }

            if (!ToggleFields.TryGetValue(name, out var field)) continue;

            if (bool.TryParse(valuePart, out var boolVal))
                field.SetValue(null, boolVal);

            KeyCode key = KeyCode.None;
            if (parts.Length >= 3)
            {
                var keyPart = parts[2].Trim();
                if (keyPart.StartsWith("KeyCode."))
                    keyPart = keyPart["KeyCode.".Length..];

                if (!string.IsNullOrEmpty(keyPart) && System.Enum.TryParse<KeyCode>(keyPart, true, out var parsed))
                    key = parsed;
            }

            Keybinds[name] = key;
        }

        PostLoadApply();
    }

    public static void LoadWindowRectsFromProfile()
    {
        if (!File.Exists(SkidMenu.ProfilePath)) return;
        using var reader = new StreamReader(SkidMenu.ProfilePath);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;
            var parts = line.Split('=', 3);
            if (parts.Length < 2) continue;
            var name = parts[0].Trim();
            var valuePart = parts[1].Trim();
            switch (name)
            {
                case "WindowRect.Menu":    if (TryParseRect(valuePart, out var rm)) { MenuUI.PendingRect = rm; MenuUI.PendingRectSet = true; } break;
                case "WindowRect.Console": if (TryParseRect(valuePart, out var rc)) { ConsoleUI.PendingRect = rc; ConsoleUI.PendingRectSet = true; } break;
                case "WindowRect.Doors":   if (TryParseRect(valuePart, out var rd)) { DoorsUI.PendingRect = rd; DoorsUI.PendingRectSet = true; } break;
                case "WindowRect.Tasks":   if (TryParseRect(valuePart, out var rt)) { TasksUI.PendingRect = rt; TasksUI.PendingRectSet = true; } break;
                case "Profile.AutoLoad":   if (bool.TryParse(valuePart, out var pal2)) SkidMenu.autoLoadProfile = pal2; break;
            }
        }
    }

    private static bool TryParseRect(string s, out Rect r)
    {
        r = default;
        var p = s.Split(',');
        if (p.Length != 4) return false;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        if (float.TryParse(p[0], System.Globalization.NumberStyles.Float, ci, out var x) &&
            float.TryParse(p[1], System.Globalization.NumberStyles.Float, ci, out var y) &&
            float.TryParse(p[2], System.Globalization.NumberStyles.Float, ci, out var w) &&
            float.TryParse(p[3], System.Globalization.NumberStyles.Float, ci, out var h))
        {
            r = new Rect(x, y, w, h);
            return true;
        }
        return false;
    }

    private static void PostLoadApply()
    {
        if (maxFpsEnabled) { UnityEngine.QualitySettings.vSyncCount = 0; UnityEngine.Application.targetFrameRate = maxFpsValue; }
        else { UnityEngine.QualitySettings.vSyncCount = 1; UnityEngine.Application.targetFrameRate = -1; }
        FindDatersLobbyPatch.LoadHostNameFilter();
    }
}















