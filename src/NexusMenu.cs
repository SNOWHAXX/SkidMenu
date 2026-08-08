using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using SkidMenu.features;
using SkidMenu.routines;
using SkidMenu.ui;

namespace SkidMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class SkidMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static SkidMenu Plugin;
    public new static ManualLogSource Log;
    public static SkidMenu Instance { get; private set; }
    public static readonly string ProfilePath = Path.Combine(Paths.ConfigPath, "MalumProfile.txt");

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static StreamerUI streamerUI;
    public static KeybindListener keybindListener;

    public static string hyperVersion = "1.2.2";
    public static string hyperBuild = "Stable";
    public static List<string> supportedAU = new List<string> { "2026.3.31", "2026.6.5" };
    public static List<string> toleratedAU = new List<string> { "2026.2.24", "2026.3.17" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;
    public static bool overloadFixed = true;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<int> spoofLevelRandomMin;
    public static ConfigEntry<int> spoofLevelRandomMax;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<string> spoofPlatformExclusions;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;
    public static ConfigEntry<bool> findDaters;
    public static ConfigEntry<bool> extendedLobbyList;

    public static ConfigEntry<int> maxPlayerLevel;
    public static ConfigEntry<float> maxTeleportDistance;

    public static ConfigEntry<int>  chatHistorySize;
    public static ConfigEntry<bool> chatHistoryInfinite;
    public static bool autoReturnAfterMatch;

    public static bool autoHostEnabled;
    public static bool autoHostInstantStart;
    public static int  autoHostMinPlayers;
    public static int  autoHostForceMinPlayers;
    public static int  autoHostWarmupSeconds;
    public static int  autoHostStartDelaySeconds;
    public static int  autoHostFastStartPlayers;
    public static int  autoHostFastStartDelaySeconds;
    public static int  autoHostLoadGraceSeconds;
    public static int  autoHostForceAfterMinutes;
    public static int  autoHostBackoffSeconds;
    public static bool autoHostCancelBelowMin;
    public static bool autoHostWaitLoadedPlayers;
    public static bool autoHostReturnAfterMatch;
    public static bool autoHostForceLastMinute;

    public static ConfigEntry<bool> logPlayerJoin;
    public static ConfigEntry<bool> logGuardianProtect;
    public static ConfigEntry<bool> logShowDistance;
    public static ConfigEntry<bool> advancedLogging;

    public static ConfigEntry<bool> frOnDeath;
    public static ConfigEntry<bool> frOnKill;
    public static ConfigEntry<bool> frOnMeetingStart;
    public static ConfigEntry<bool> frOnMeetingEnd;
    public static ConfigEntry<bool> frOnLobbyLeave;
    public static ConfigEntry<bool> frOnGameEnd;
    public static ConfigEntry<bool> frOnShapeshift;
    public static ConfigEntry<bool> frOnVent;
    public static ConfigEntry<bool> frOnTaskComplete;

    public static ConfigEntry<string> nameSpoofName;
    public static ConfigEntry<bool>   nameSpoofEnabled;
    public static ConfigEntry<int>    nameSpoofMode;
    public static ConfigEntry<int>    nameSpoofLength;
    public static ConfigEntry<float> frRpcDelay;
    public static ConfigEntry<bool> frRandLevel;
    public static ConfigEntry<bool> frRandPlatform;
    public static ConfigEntry<bool> frRandName;
    public static ConfigEntry<bool> frRandHat;
    public static ConfigEntry<bool> frRandSkin;
    public static ConfigEntry<bool> frRandVisor;
    public static ConfigEntry<bool> frRandPet;
    public static ConfigEntry<bool> frRandNameplate;
    public static ConfigEntry<bool> frRandColor;
    public static ConfigEntry<bool> frRandNameHostOnly;
    public static ConfigEntry<bool> frRandColorHostOnly;

    public static ConfigEntry<bool>   darkGameTheme;
    public static ConfigEntry<bool>   customGameTheme;
    public static ConfigEntry<string> gameBgColorHex;
    public static ConfigEntry<string> gameTextColorHex;
    public static ConfigEntry<bool>   chatFont;
    public static ConfigEntry<int>    chatFontType;

    public static RoutineManager routines;
    public static NotificationManager notifications;

    public override void Load()
    {
        Instance = this;
        Log = base.Log;
        Log.LogInfo($"SkidMenu has loaded!");

        var toWrap = BepInEx.Logging.Logger.Listeners
            .FirstOrDefault(l => l is BepInEx.Logging.ConsoleLogListener);
        if (toWrap != null)
        {
            BepInEx.Logging.Logger.Listeners.Remove(toWrap);
            BepInEx.Logging.Logger.Listeners.Add(new FilteredConsoleLogListener(toWrap));
        }

        // Extract embedded assets to BepInEx root on load
        try {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var res in new[] { "image.png", "Roboto_Condensed-Regular.ttf", "Roboto_Condensed-Bold.ttf" }) {
                using var s = asm.GetManifestResourceStream("SkidMenu.Assets." + res);
                if (s == null) { Log.LogError("Missing resource: " + res); continue; }
                var bytes = new byte[s.Length]; s.Read(bytes, 0, bytes.Length);
                var dest = System.IO.Path.Combine(BepInEx.Paths.BepInExRootPath, res);
                System.IO.File.WriteAllBytes(dest, bytes);
                Log.LogInfo("Extracted: " + dest);
            }
        } catch (System.Exception ex) { Log.LogError("Asset extract failed: " + ex.Message); }
        Plugin = this;
        notifications = AddComponent<NotificationManager>();
        routines = AddComponent<RoutineManager>();
        anticheat.Blacklist.Load();
        AddComponent<features.AutoReturnAfterMatch>();
        AddComponent<ProtectionKeeper>();
        AddComponent<VentVisibilityKeeper>();

        menuKeybind = Config.Bind("SkidMenu.GUI",
                                "Keybind",
                                "Delete",
                                "The keyboard key used to toggle the GUI on and off. List of supported keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");

        menuHtmlColor = Config.Bind("SkidMenu.GUI",
                                "Color",
                                "",
                                "A custom color for your SkidMenu GUI. Supports html color codes");

        menuOpenOnMouse = Config.Bind("SkidMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "When enabled, the SkidMenu GUI will always be opened at the current mouse position");

        menuKeepSubwindowsOpen = Config.Bind("SkidMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "When enabled, closing the SkidMenu GUI will not automatically close its subwindows");

        advancedLogging = Config.Bind("SkidMenu.Settings",
                                "AdvancedLogging",
                                false,
                                "When enabled, writes everything (BepInEx logs, Unity exceptions and every in-game event) to a dated log file in BepInEx/Logs/SkidMenu. A new file is created on every launch");
        features.AdvancedLogger.Init();

        autoLoadProfile = Config.Bind("SkidMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "When enabled, your saved keybind and toggle profile will be automatically loaded at game startup");

        configEditor = Config.Bind("SkidMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "The program used to open the config file when using the Open Config toggle. Can be any executable, but using a text editor is recommended");

        spoofLevel = Config.Bind("SkidMenu.Spoofing",
                                "Level",
                                "",
                                "A custom player level to display to others in online games to hide your actual platform. IMPORTANT: Custom levels can only be within 1 and 100001. Decimal numbers will not work");

        spoofLevelRandomMin = Config.Bind("SkidMenu.Spoofing",
                                "RandomLevelMin",
                                1,
                                new ConfigDescription(
                                    "Minimum value for the Random Level button range",
                                    new AcceptableValueRange<int>(1, 100001)
                                ));

        spoofLevelRandomMax = Config.Bind("SkidMenu.Spoofing",
                                "RandomLevelMax",
                                100001,
                                new ConfigDescription(
                                    "Maximum value for the Random Level button range",
                                    new AcceptableValueRange<int>(1, 100001)
                                ));

        spoofPlatform = Config.Bind("SkidMenu.Spoofing",
                                "Platform",
                                "",
                                "A custom gaming platform to display to others in online lobbies to hide your actual platform. List of supported platforms: https://skeld.js.org/enums/_skeldjs_constant.Platform.html");

        spoofPlatformExclusions = Config.Bind("SkidMenu.Spoofing",
                                "RandomPlatformExclusions",
                                "",
                                "Comma-separated list of platforms excluded from the Random Platform button (e.g. Unknown,Xbox,Playstation)");

        findDaters        = Config.Bind("SkidMenu.DatingShit", "FindDaters",        false, "Filter lobby browser to show likely dating lobbies");
        extendedLobbyList = Config.Bind("SkidMenu.DatingShit", "ExtendedLobbyList", false, "Show 20+ lobbies in browser with scroll support");

        maxPlayerLevel       = Config.Bind("SkidMenu.Anticheat", "MaxPlayerLevel",       10000, new BepInEx.Configuration.ConfigDescription("Kick players whose SetLevel RPC exceeds this value", new BepInEx.Configuration.AcceptableValueRange<int>(1, 100000)));
        maxTeleportDistance  = Config.Bind("SkidMenu.Anticheat", "MaxTeleportDistance",  25f,   new BepInEx.Configuration.ConfigDescription("Flag SnapTo RPCs that move a player more than this many units", new BepInEx.Configuration.AcceptableValueRange<float>(1f, 500f)));

        chatHistorySize      = Config.Bind("SkidMenu.Chat", "HistorySize",     80,    new BepInEx.Configuration.ConfigDescription("Number of chat bubbles to keep", new BepInEx.Configuration.AcceptableValueRange<int>(5, 500)));
        chatHistoryInfinite  = Config.Bind("SkidMenu.Chat", "HistoryInfinite", false, "Keep all chat messages without limit");
        autoReturnAfterMatch = Config.Bind("SkidMenu.Lobby", "AutoReturnAfterMatch", true, "Automatically return to lobby after a match ends").Value;

        autoHostEnabled              = Config.Bind("SkidMenu.AutoHost", "Enabled",              false, "Automatically start matches in the lobby").Value;
        autoHostInstantStart         = Config.Bind("SkidMenu.AutoHost", "InstantStart",          true,  "Skip the start counter and start immediately").Value;
        autoHostMinPlayers           = Config.Bind("SkidMenu.AutoHost", "MinPlayers",            4,     new BepInEx.Configuration.ConfigDescription("Minimum players before starting countdown", new BepInEx.Configuration.AcceptableValueRange<int>(1, 15))).Value;
        autoHostForceMinPlayers      = Config.Bind("SkidMenu.AutoHost", "ForceMinPlayers",       2,     new BepInEx.Configuration.ConfigDescription("Minimum players for forced start", new BepInEx.Configuration.AcceptableValueRange<int>(1, 15))).Value;
        autoHostWarmupSeconds        = Config.Bind("SkidMenu.AutoHost", "WarmupSeconds",         5,     new BepInEx.Configuration.ConfigDescription("Seconds to wait after lobby opens before AutoHost activates", new BepInEx.Configuration.AcceptableValueRange<int>(0, 120))).Value;
        autoHostStartDelaySeconds    = Config.Bind("SkidMenu.AutoHost", "StartDelaySeconds",     3,     new BepInEx.Configuration.ConfigDescription("Countdown seconds before starting", new BepInEx.Configuration.AcceptableValueRange<int>(0, 180))).Value;
        autoHostFastStartPlayers     = Config.Bind("SkidMenu.AutoHost", "FastStartPlayers",      13,    new BepInEx.Configuration.ConfigDescription("Player count that triggers fast start", new BepInEx.Configuration.AcceptableValueRange<int>(0, 15))).Value;
        autoHostFastStartDelaySeconds= Config.Bind("SkidMenu.AutoHost", "FastStartDelaySeconds", 5,     new BepInEx.Configuration.ConfigDescription("Countdown seconds for fast start", new BepInEx.Configuration.AcceptableValueRange<int>(0, 60))).Value;
        autoHostLoadGraceSeconds     = Config.Bind("SkidMenu.AutoHost", "LoadGraceSeconds",      20,    new BepInEx.Configuration.ConfigDescription("Max seconds to wait for slow-loading players", new BepInEx.Configuration.AcceptableValueRange<int>(0, 90))).Value;
        autoHostForceAfterMinutes    = Config.Bind("SkidMenu.AutoHost", "ForceAfterMinutes",     0,     new BepInEx.Configuration.ConfigDescription("Force start after N minutes (0 = off)", new BepInEx.Configuration.AcceptableValueRange<int>(0, 10))).Value;
        autoHostBackoffSeconds       = Config.Bind("SkidMenu.AutoHost", "BackoffSeconds",        8,     new BepInEx.Configuration.ConfigDescription("Cooldown after a failed start attempt", new BepInEx.Configuration.AcceptableValueRange<int>(2, 60))).Value;
        autoHostCancelBelowMin       = Config.Bind("SkidMenu.AutoHost", "CancelBelowMin",        true,  "Cancel countdown if players drop below minimum").Value;
        autoHostWaitLoadedPlayers    = Config.Bind("SkidMenu.AutoHost", "WaitLoadedPlayers",     true,  "Wait for all players to finish loading before starting").Value;
        autoHostReturnAfterMatch     = Config.Bind("SkidMenu.AutoHost", "ReturnAfterMatch",      true,  "Return to lobby and re-host after a match ends").Value;
        autoHostForceLastMinute      = Config.Bind("SkidMenu.AutoHost", "ForceLastMinute",       true,  "Force start in the last minute of lobby lifetime").Value;

        logPlayerJoin     = Config.Bind("SkidMenu.Notifications", "PlayerJoin",      false, "Show notification when a player joins");
        logGuardianProtect = Config.Bind("SkidMenu.Notifications", "GuardianProtect", false, "Show notification when a Guardian Angel protects someone");
        logShowDistance   = Config.Bind("SkidMenu.Notifications", "ShowDistance",    false, "Append color-coded distance to kill/protect notifications");

        frOnDeath        = Config.Bind("SkidMenu.FullyRandomize", "OnDeath",        false, "Auto fully randomize after you die");
        frOnKill         = Config.Bind("SkidMenu.FullyRandomize", "OnKill",         false, "Auto fully randomize after you kill someone");
        frOnMeetingStart = Config.Bind("SkidMenu.FullyRandomize", "OnMeetingStart", false, "Auto fully randomize when a meeting starts");
        frOnMeetingEnd   = Config.Bind("SkidMenu.FullyRandomize", "OnMeetingEnd",   false, "Auto fully randomize when a meeting ends");
        frOnLobbyLeave   = Config.Bind("SkidMenu.FullyRandomize", "OnLobbyLeave",   false, "Auto fully randomize when leaving a lobby");
        frOnGameEnd      = Config.Bind("SkidMenu.FullyRandomize", "OnGameEnd",      false, "Auto fully randomize when a game ends");
        frOnShapeshift   = Config.Bind("SkidMenu.FullyRandomize", "OnShapeshift",   false, "Auto fully randomize when you shapeshift");
        frOnVent         = Config.Bind("SkidMenu.FullyRandomize", "OnVent",         false, "Auto fully randomize when you enter a vent");
        frOnTaskComplete = Config.Bind("SkidMenu.FullyRandomize", "OnTaskComplete", false, "Auto fully randomize when you complete a task");
        frRpcDelay       = Config.Bind("SkidMenu.FullyRandomize", "RpcDelay", 0.10f,
                                new ConfigDescription(
                                    "Delay in seconds between each RPC sent during Fully Randomize. Increase if you get kicked as non-host.",
                                    new AcceptableValueRange<float>(0f, 1f)
                                ));
        frRandLevel    = Config.Bind("SkidMenu.FullyRandomize", "RandomizeLevel",    true, "Include level in Fully Randomize");
        frRandPlatform = Config.Bind("SkidMenu.FullyRandomize", "RandomizePlatform", true, "Include platform in Fully Randomize");
        frRandName     = Config.Bind("SkidMenu.FullyRandomize", "RandomizeName",     true, "Include name in Fully Randomize");
        frRandHat      = Config.Bind("SkidMenu.FullyRandomize", "RandomizeHat",      true, "Include hat in Fully Randomize");
        frRandSkin     = Config.Bind("SkidMenu.FullyRandomize", "RandomizeSkin",     true, "Include skin in Fully Randomize");
        frRandVisor    = Config.Bind("SkidMenu.FullyRandomize", "RandomizeVisor",    true, "Include visor in Fully Randomize");
        frRandPet       = Config.Bind("SkidMenu.FullyRandomize", "RandomizePet",       true, "Include pet in Fully Randomize");
        frRandNameplate = Config.Bind("SkidMenu.FullyRandomize", "RandomizeNameplate", true, "Include nameplate in Fully Randomize");
        frRandColor    = Config.Bind("SkidMenu.FullyRandomize", "RandomizeColor",    true, "Include color in Fully Randomize");
        frRandNameHostOnly  = Config.Bind("SkidMenu.FullyRandomize", "NameHostOnly",  true, "Only send Name RPC when host");
        frRandColorHostOnly = Config.Bind("SkidMenu.FullyRandomize", "ColorHostOnly", true, "Only send Color RPC when host");

        nameSpoofName    = Config.Bind("SkidMenu.NameSpoofer", "SpoofedName", "",    "Last active spoofed name");
        nameSpoofEnabled = Config.Bind("SkidMenu.NameSpoofer", "Enabled",     false, "Whether name spoofer is active");
        nameSpoofMode    = Config.Bind("SkidMenu.NameSpoofer", "Mode",        1,     "Random name generation mode index");
        nameSpoofLength  = Config.Bind("SkidMenu.NameSpoofer", "Length",      10,    "Random name length");

        darkGameTheme   = Config.Bind("SkidMenu.Self", "DarkGameTheme",  true,     "Enable dark game theme in chat (on by default)");
        customGameTheme = Config.Bind("SkidMenu.Self", "CustomGameTheme", false,   "Enable custom game theme colors in chat");
        gameBgColorHex  = Config.Bind("SkidMenu.Self", "GameBgColor",   "222222", "Chat bubble background color as a 6-digit hex code (no #)");
        gameTextColorHex = Config.Bind("SkidMenu.Self", "GameTextColor", "FFFFFF", "Chat text color as a 6-digit hex code (no #)");
        chatFont        = Config.Bind("SkidMenu.Self", "ChatFont",       false,    "Enable custom chat font");
        chatFontType    = Config.Bind("SkidMenu.Self", "ChatFontType",   0,
                                new ConfigDescription(
                                    "Chat font type index (0-20). See SelfTab for the full font list.",
                                    new AcceptableValueRange<int>(0, 20)
                                ));

        DarkMode.Enabled         = darkGameTheme.Value;
        CustomGameTheme.Enabled  = customGameTheme.Value;
        ChatFontChanger.Enabled  = chatFont.Value;
        ChatFontChanger.FontType = chatFontType.Value;
        if (ColorUtility.TryParseHtmlString("#" + gameBgColorHex.Value, out Color bgCol))
            CustomGameTheme.BgColor = bgCol;
        if (ColorUtility.TryParseHtmlString("#" + gameTextColorHex.Value, out Color textCol))
            CustomGameTheme.TextColor = textCol;

        features.NameSpoofer.Mode         = (features.NameSpoofer.RandomizerMode)nameSpoofMode.Value;
        features.NameSpoofer.RandomLength = nameSpoofLength.Value;
        if (nameSpoofEnabled.Value && !string.IsNullOrWhiteSpace(nameSpoofName.Value))
            features.NameSpoofer.ApplyName(nameSpoofName.Value);

        FullyRandomizeTriggers.OnDeath        = frOnDeath.Value;
        FullyRandomizeTriggers.OnKill         = frOnKill.Value;
        FullyRandomizeTriggers.OnMeetingStart = frOnMeetingStart.Value;
        FullyRandomizeTriggers.OnMeetingEnd   = frOnMeetingEnd.Value;
        FullyRandomizeTriggers.OnLobbyLeave   = frOnLobbyLeave.Value;
        FullyRandomizeTriggers.OnGameEnd      = frOnGameEnd.Value;
        FullyRandomizeTriggers.OnShapeshift   = frOnShapeshift.Value;
        FullyRandomizeTriggers.OnVent         = frOnVent.Value;
        FullyRandomizeTriggers.OnTaskComplete = frOnTaskComplete.Value;

        CheatToggles.findDaters        = findDaters.Value;
        CheatToggles.extendedLobbyList = extendedLobbyList.Value;

        anticheat.ModDetection.RebuildIndex();

        Beacon("before PatchAll");
        var patchTypes = AccessTools.GetTypesFromAssembly(typeof(SkidMenu).Assembly)
            .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), false).Any())
            .OrderBy(t => t.FullName).ToList();
        Beacon($"patch count: {patchTypes.Count}");
        int patchFailures = 0;
        for (int i = 0; i < patchTypes.Count; i++)
        {
            Beacon($"patch {i}: {patchTypes[i].FullName}");
            try
            {
                Harmony.CreateClassProcessor(patchTypes[i]).Patch();
            }
            catch (System.Exception ex)
            {
                patchFailures++;
                Beacon($"patch {i} FAILED: {ex.GetType().Name}: {ex.Message}");
                Log.LogWarning($"Failed to apply patch {patchTypes[i].FullName}: {ex}");
            }
        }
        Beacon(patchFailures == 0 ? "PatchAll done" : $"PatchAll done with {patchFailures} failed");

        Beacon("reg MenuUI");      menuUI = AddComponent<MenuUI>();
        Beacon("reg ConsoleUI");   consoleUI = AddComponent<ConsoleUI>();
        Beacon("reg ChatUI");      AddComponent<ChatUI>();
        Beacon("reg DoorsUI");     doorsUI = AddComponent<DoorsUI>();
        Beacon("reg TasksUI");     tasksUI = AddComponent<TasksUI>();
        Beacon("reg ProtectUI");   protectUI = AddComponent<ProtectUI>();
        Beacon("reg StreamerUI");  streamerUI = AddComponent<StreamerUI>();
        Beacon("reg PlayerInfosUI"); AddComponent<PlayerInfosUI>();
        Beacon("reg Keybind");     keybindListener = AddComponent<KeybindListener>();
        Beacon("all registered");

        if (CheatToggles.disableTelemetry && !CheatToggles.spoofTelemetry)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        if (!File.Exists(ProfilePath))
        {
            CheatToggles.SaveTogglesToProfile();
        }

        CheatToggles.LoadWindowRectsFromProfile();

        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                if (!supportedAU.Contains(Application.version) && !toleratedAU.Contains(Application.version))
                {
                    Utils.ShowNewPopup("This version of SkidMenu and this version of Among Us are incompatible\n\nInstall the right version to avoid problems");
                } else if (!supportedAU.Contains(Application.version) && toleratedAU.Contains(Application.version))
                {
                    Utils.ShowNewPopup("This version of SkidMenu and this version of Among Us are not fully compatible\n\nSome features may not work properly, as SkidMenu is not updated to keep compatibility with older Among Us versions.");
                }
            }
        }));
        Beacon("sceneLoaded added");
    }

    private static void Beacon(string msg)
    {
        try { File.AppendAllText(Path.Combine(Path.GetTempPath(), "skid_beacon.log"), $"[{DateTime.Now:HH:mm:ss.fff}] {msg}{Environment.NewLine}"); }
        catch { }
    }
}

public class FilteredConsoleLogListener : BepInEx.Logging.ILogListener
{
    private readonly BepInEx.Logging.ILogListener _inner;
    public FilteredConsoleLogListener(BepInEx.Logging.ILogListener inner) => _inner = inner;

    public void LogEvent(object sender, BepInEx.Logging.LogEventArgs eventArgs)
    {
        if (eventArgs.Data?.ToString()?.Contains("modifying system") == true) return;
        _inner.LogEvent(sender, eventArgs);
    }

    public BepInEx.Logging.LogLevel LogLevelFilter => _inner.LogLevelFilter;
    public void Dispose() => _inner.Dispose();
}




