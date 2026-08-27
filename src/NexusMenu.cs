using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
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

    public static string hyperVersion = "1.3.3";
    public static string hyperBuild = "Stable";
    public static List<string> supportedAU = new List<string> { "2026.6.5", "2026.8.18" };
    public static List<string> toleratedAU = new List<string> { "2026.2.24", "2026.3.17", "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;
    public static bool overloadFixed = true;

    public static string menuKeybind = "Delete";
    public static string menuHtmlColor = "";
    public static bool menuOpenOnMouse = false;
    public static bool menuKeepSubwindowsOpen = false;
    public static string spoofLevel = "";
    public static int spoofLevelRandomMin = 1;
    public static int spoofLevelRandomMax = 100001;
    public static string spoofPlatform = "";
    public static string spoofPlatformExclusions = "";
    public static string guestFriendCode = "";
    public static bool guestMode = false;
    public static bool autoLoadProfile = false;
    public static string configEditor = "notepad.exe";

    public static int maxPlayerLevel = 10000;
    public static float maxTeleportDistance = 25f;

    public static int  chatHistorySize = 80;
    public static bool chatHistoryInfinite = false;
    public static bool autoReturnAfterMatch = true;

    public static bool autoHostEnabled = false;
    public static bool autoHostInstantStart = true;
    public static int  autoHostMinPlayers = 4;
    public static int  autoHostForceMinPlayers = 2;
    public static int  autoHostWarmupSeconds = 5;
    public static int  autoHostStartDelaySeconds = 3;
    public static int  autoHostFastStartPlayers = 13;
    public static int  autoHostFastStartDelaySeconds = 5;
    public static int  autoHostLoadGraceSeconds = 20;
    public static int  autoHostForceAfterMinutes = 0;
    public static int  autoHostBackoffSeconds = 8;
    public static bool autoHostCancelBelowMin = true;
    public static bool autoHostWaitLoadedPlayers = true;
    public static bool autoHostReturnAfterMatch = true;
    public static bool autoHostForceLastMinute = true;

    public static bool logPlayerJoin = false;
    public static bool logShowDistance = false;
    public static bool advancedLogging = false;

    public static string nameSpoofName = "";
    public static bool   nameSpoofEnabled = false;
    public static int    nameSpoofMode = 1;
    public static int    nameSpoofLength = 10;
    public static float frRpcDelay = 0.10f;
    public static bool frRandLevel = true;
    public static bool frRandPlatform = true;
    public static bool frRandName = true;
    public static bool frRandHat = true;
    public static bool frRandSkin = true;
    public static bool frRandVisor = true;
    public static bool frRandPet = true;
    public static bool frRandNameplate = true;
    public static bool frRandColor = true;
    public static bool frRandNameHostOnly = true;
    public static bool frRandColorHostOnly = true;

    public static bool   darkGameTheme = true;
    public static bool   customGameTheme = false;
    public static string gameBgColorHex = "222222";
    public static string gameTextColorHex = "FFFFFF";
    public static bool   chatFont = false;
    public static int    chatFontType = 0;

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

        DarkMode.Enabled         = darkGameTheme;
        CustomGameTheme.Enabled  = customGameTheme;
        ChatFontChanger.Enabled  = chatFont;
        ChatFontChanger.FontType = chatFontType;
        if (ColorUtility.TryParseHtmlString("#" + gameBgColorHex, out Color bgCol))
            CustomGameTheme.BgColor = bgCol;
        if (ColorUtility.TryParseHtmlString("#" + gameTextColorHex, out Color textCol))
            CustomGameTheme.TextColor = textCol;

        features.NameSpoofer.Mode         = (features.NameSpoofer.RandomizerMode)nameSpoofMode;
        features.NameSpoofer.RandomLength = nameSpoofLength;
        if (nameSpoofEnabled && !string.IsNullOrWhiteSpace(nameSpoofName))
            features.NameSpoofer.ApplyName(nameSpoofName);

        if (Utils.StringToPlatformType(spoofPlatform, out Platforms? savedPlatform))
            features.Spoofer.spoofedPlatform = (Platforms)savedPlatform;
        features.AdvancedLogger.Init();
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

        if (autoLoadProfile)
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




