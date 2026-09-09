using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SkidMenu;

public class KeybindListener : MonoBehaviour
{
    public static bool KeybindsDisabled = false;
    public static bool KeybindNotifications = true;

    private static void NotifyKeybind(string label)
    {
        if (!KeybindNotifications) return;
        try { SkidMenu.notifications.Send("Keybind", label, 2f); } catch { }
    }

    public static readonly System.Collections.Generic.Dictionary<string, KeyCode> ActionKeys = new()
    {
        { "ClearNotifications", KeyCode.None },
        { "CloseRoomDoors", KeyCode.F1 },
        { "CloseAllDoors", KeyCode.F2 },
        { "OpenAllDoors", KeyCode.F3 },
        { "SabotageAll", KeyCode.F6 },
        { "FixAllSabotages", KeyCode.F7 },
        { "SpamElectrical", KeyCode.Alpha7 },
        { "CompleteTasks", KeyCode.F4 },
        { "ReportRandomBody", KeyCode.F5 },
        { "VotekickAll", KeyCode.F8 },
        { "VentKickAll", KeyCode.F9 },
        { "VentKickImpostors", KeyCode.F10 },
        { "VentKickRandom", KeyCode.F11 },
        { "CallMeeting", KeyCode.Alpha0 },
        { "MurderRandom", KeyCode.Alpha9 },
        { "TeleMurderRandom", KeyCode.Alpha8 },
        { "BecomeInvisible", KeyCode.None },
        { "LagCompensation", KeyCode.None },
        { "NoClip", KeyCode.None },
        { "KillAura", KeyCode.None },
        { "SpamPet", KeyCode.None },
        { "ZoomOut", KeyCode.None },
        { "Rainbow", KeyCode.None },
        { "Freecam", KeyCode.None },
        { "NoShadows", KeyCode.None },
        { "RandomSabotage", KeyCode.None },
        { "ProtectEveryone", KeyCode.None },
        { "ForceStartGame", KeyCode.None },
        { "AutoReportBodies", KeyCode.None },
        { "FullyRandomize", KeyCode.None },
        { "BecomeImmortal", KeyCode.None },
        { "ShowRadar", KeyCode.None },
        { "ShowDoorsMenu", KeyCode.None },
        { "ShowChatWindow", KeyCode.None },
        { "ShowConsole", KeyCode.None },
        { "ShowProtectMenu", KeyCode.None },
    };

    public static KeyCode GetActionKey(string name)
    {
        return ActionKeys.TryGetValue(name, out var key) ? key : KeyCode.None;
    }

    private float _f1Timer  = 0f;
    private float _f2Timer  = 0f;
    private float _f3Timer  = 0f;
    private float _f6Timer  = 0f;
    private float _f7Timer  = 0f;
    private float _key7Timer = 0f;

    private const float HoldInterval   = 0.4f;
    private const float SpamInterval   = 0.05f;

    // Cache reflected FieldInfo per toggle name to avoid per-press reflection lookups.
    private static readonly System.Collections.Generic.Dictionary<string, System.Reflection.FieldInfo> _toggleFieldCache = new();

    private static System.Reflection.FieldInfo GetToggleField(string name)
    {
        if (_toggleFieldCache.TryGetValue(name, out var fi)) return fi;
        fi = CheatToggles.ToggleFields.TryGetValue(name, out var f) ? f : null;
        _toggleFieldCache[name] = fi;
        return fi;
    }

    private static bool InGame => ShipStatus.Instance != null;

    public void Update()
    {
        if (SkidMenu.isPanicked) return;
        InfoTab.HandleKeybindCapture();
        if (KeybindsDisabled) return;

        // Suppress all keybinds when typing in our menu or in Among Us chat
        if (GUIUtility.keyboardControl != 0) return;
        if (HudManager.InstanceExists && HudManager.Instance.Chat != null &&
            HudManager.Instance.Chat.IsOpenOrOpening) return;

        if (Input.anyKeyDown)
        {
            foreach (var (name, key) in CheatToggles.Keybinds)
            {
                if (key == KeyCode.None) continue;
                if (!Input.GetKeyDown(key)) continue;
                var field = GetToggleField(name);
                if (field == null) continue;
                bool next = !(bool)field.GetValue(null);
                field.SetValue(null, next);
                NotifyKeybind($"{name} {(next ? "ON" : "OFF")}");
            }
        }

        if (PlayerControl.LocalPlayer == null) return;

        HandleHoldAction("CloseRoomDoors", "Close room doors", ref _f1Timer,  TryCloseCurrentRoom);
        HandleHoldAction("CloseAllDoors", "Close all doors", ref _f2Timer,  () => { try { DoorsHandler.CloseAllDoors(); } catch { } });
        HandleHoldAction("OpenAllDoors", "Open all doors", ref _f3Timer,  TryOpenAllDoors);
        HandleHoldAction("SabotageAll", "Sabotage all", ref _f6Timer,  () => { Notif_Sabotage.SuppressNext = true; try { Sabotage.SabotageAll(); } catch { } });
        HandleHoldAction("FixAllSabotages", "Fix all sabotages", ref _f7Timer,  () => { try { Sabotage.FixAllSabotages(); } catch { } });
        HandleHoldAction("SpamElectrical", "Spam electrical", ref _key7Timer, TryElectricalSabotage, SpamInterval);

        if (PressedAction("CompleteTasks") && !RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
        { PlayerControl.LocalPlayer.StartCoroutine(CompleteTasksWithDelay().WrapToIl2Cpp()); NotifyKeybind("Complete tasks"); }
        if (PressedAction("VotekickAll"))  { TryVotekickAll(); NotifyKeybind("Votekick all"); }
        if (PressedAction("VentKickAll"))  { TryVentKickAll(); NotifyKeybind("Vent kick all"); }
        if (PressedAction("VentKickImpostors")) { TryVentKickImpostors(); NotifyKeybind("Vent kick impostors"); }
        if (PressedAction("VentKickRandom")) { TryVentKickRandom(); NotifyKeybind("Vent kick random"); }

        HandleToggleAction("BecomeInvisible", "Become Invisible", () => ToggleInvisibility(), () => features.Invisibility.Enabled);
        HandleToggleAction("LagCompensation", "Lag Compensation", () => { features.LagCompensation.Enabled = !features.LagCompensation.Enabled; }, () => features.LagCompensation.Enabled);
        HandleToggleAction("NoClip", "NoClip", () => { CheatToggles.noClip = !CheatToggles.noClip; }, () => CheatToggles.noClip);
        HandleToggleAction("KillAura", "Kill Aura", () => { features.KillAura.Enabled = !features.KillAura.Enabled; }, () => features.KillAura.Enabled);
        HandleToggleAction("SpamPet", "Spam Pet", () => { CheatToggles.spamPet = !CheatToggles.spamPet; }, () => CheatToggles.spamPet);
        HandleToggleAction("ZoomOut", "Zoom Out", () => { CheatToggles.zoomOut = !CheatToggles.zoomOut; }, () => CheatToggles.zoomOut);
        HandleToggleAction("Rainbow", "Rainbow", () => { SelfTab.RainbowEnabled = !SelfTab.RainbowEnabled; }, () => SelfTab.RainbowEnabled);
        HandleToggleAction("Freecam", "Freecam", () => { CheatToggles.freecam = !CheatToggles.freecam; }, () => CheatToggles.freecam);
        HandleToggleAction("NoShadows", "No Shadows", () => { CheatToggles.noShadows = !CheatToggles.noShadows; }, () => CheatToggles.noShadows);
        HandleToggleAction("AutoReportBodies", "Auto Report Bodies", () => { features.Troll.AutoReportBodies.Enabled = !features.Troll.AutoReportBodies.Enabled; }, () => features.Troll.AutoReportBodies.Enabled);
        if (PressedAction("RandomSabotage")) { TryRandomSabotage(); NotifyKeybind("Random sabotage"); }
        if (PressedAction("ProtectEveryone")) { try { features.HostProtection.ProtectEveryone(true); } catch { } NotifyKeybind("Protect everyone"); }
        if (PressedAction("ForceStartGame")) { CheatToggles.forceStartGame = true; NotifyKeybind("Force start game"); }
        if (PressedAction("FullyRandomize")) { TryFullyRandomize(); NotifyKeybind("Fully randomize"); }
        if (PressedAction("ClearNotifications")) { try { SkidMenu.notifications.ClearNotifications(); } catch { } NotifyKeybind("Clear notifications"); }
        HandleToggleAction("BecomeImmortal", "Become Immortal", () => { features.Immortality.Enabled = !features.Immortality.Enabled; }, () => features.Immortality.Enabled);
        HandleToggleAction("ShowRadar", "Show Radar", () => { CheatToggles.radarShow = !CheatToggles.radarShow; }, () => CheatToggles.radarShow);
        HandleToggleAction("ShowDoorsMenu", "Show Doors Menu", () => { CheatToggles.showDoorsMenu = !CheatToggles.showDoorsMenu; }, () => CheatToggles.showDoorsMenu);
        HandleToggleAction("ShowChatWindow", "Show Chat Window", () => { CheatToggles.showChatUI = !CheatToggles.showChatUI; }, () => CheatToggles.showChatUI);
        HandleToggleAction("ShowConsole", "Show Console", () => { CheatToggles.showConsole = !CheatToggles.showConsole; }, () => CheatToggles.showConsole);
        HandleToggleAction("ShowProtectMenu", "Show Protect Menu", () => { CheatToggles.showProtectMenu = !CheatToggles.showProtectMenu; }, () => CheatToggles.showProtectMenu);

        if (!InGame) return;

        if (PressedAction("ReportRandomBody"))  { TryReportRandomBody(); NotifyKeybind("Report random body"); }
        if (PressedAction("CallMeeting")) { TryCallMeeting(); NotifyKeybind("Call meeting"); }
        if (PressedAction("MurderRandom")) { TryMurderRandom(); NotifyKeybind("Murder random"); }
        if (PressedAction("TeleMurderRandom")) { TryTeleMurderRandom(); NotifyKeybind("Telemurder random"); }
    }

    private static bool PressedAction(string name)
    {
        KeyCode key = GetActionKey(name);
        return key != KeyCode.None && Input.GetKeyDown(key);
    }

    private static void HandleHoldAction(string name, string label, ref float timer, System.Action action, float interval = HoldInterval)
    {
        KeyCode key = GetActionKey(name);
        if (key == KeyCode.None) { timer = 0f; return; }
        if (Input.GetKey(key))
        {
            if (timer <= 0f) { action(); NotifyKeybind(label); }
            timer += Time.unscaledDeltaTime;
            if (timer >= interval) timer = 0f;
        }
        else timer = 0f;
    }

    private static readonly System.Collections.Generic.HashSet<string> _togglePressed = new();

    private static void HandleToggleAction(string name, string label, System.Action toggle, System.Func<bool> getState)
    {
        KeyCode key = GetActionKey(name);
        if (key == KeyCode.None) { _togglePressed.Remove(name); return; }
        if (Input.GetKeyDown(key) && _togglePressed.Add(name))
        {
            try
            {
                toggle();
                bool on = false;
                try { on = getState(); } catch { }
                NotifyKeybind($"{label} {(on ? "ON" : "OFF")}");
            }
            catch { }
        }
        if (Input.GetKeyUp(key)) _togglePressed.Remove(name);
    }

    private static void ToggleInvisibility()
    {
        try { features.Invisibility.Enabled = !features.Invisibility.Enabled; } catch { }
    }

    private void HandleHold(KeyCode key, ref float timer, System.Action action, float interval = HoldInterval)
    {
        if (Input.GetKey(key))
        {
            if (timer <= 0f) action();
            timer += Time.unscaledDeltaTime;
            if (timer >= interval) timer = 0f;
        }
        else timer = 0f;
    }

    private static void TryElectricalSabotage()
    {
        try
        {
            if (ShipStatus.Instance == null) return;
            Sabotage.SabotageSystem(SystemTypes.Electrical);
        }
        catch { }
    }

    private static void TryCloseCurrentRoom()
    {
        try
        {
            var room = HudManager.Instance?.roomTracker?.LastRoom;
            if (room != null) DoorsHandler.CloseDoorsInRoom(room.RoomId);
        }
        catch { }
    }

    private static void TryOpenAllDoors()
    {
        try
        {
            if (ShipStatus.Instance?.AllDoors == null) return;
            foreach (OpenableDoor door in ShipStatus.Instance.AllDoors)
                try { DoorsHandler.OpenDoor(door); } catch { }
        }
        catch { }
    }

    private static void TryReportRandomBody()
    {
        try
        {
            var bodies = Object.FindObjectsOfType<DeadBody>();
            if (bodies == null || bodies.Length == 0) return;
            var body = bodies[Random.Range(0, bodies.Length)];
            if (body == null || !ViperBodies.CanReport(body)) return;
            Teleporter.TeleportToLocal(body.transform.position);
            PlayerControl.LocalPlayer.CmdReportDeadBody(GameData.Instance.GetPlayerById(body.ParentId));
        }
        catch { }
    }

    private static void TryVotekickAll()
    {
        try
        {
            VotekickHandler.ResetTracking();
            VotekickHandler.VotekickAllNow();
        }
        catch { }
    }

    private static void TryVentKickAll()
    {
        try
        {
            if (PlayerControl.AllPlayerControls == null) return;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p == null || p.AmOwner || p.Data == null) continue;
                VentKickTab.VentKick(p);
            }
        }
        catch { }
    }

    private static void TryVentKickImpostors()
    {
        try
        {
            if (PlayerControl.AllPlayerControls == null) return;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p == null || p.AmOwner || p.Data == null) continue;
                if (RoleManager.IsImpostorRole(p.Data.RoleType)) VentKickTab.VentKick(p);
            }
        }
        catch { }
    }

    private static void TryVentKickRandom()
    {
        try
        {
            if (PlayerControl.AllPlayerControls == null) return;
            var candidates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => p != null && !p.AmOwner && p.Data != null && !p.Data.IsDead)
                .ToList();
            if (candidates.Count == 0) return;
            VentKickTab.VentKick(candidates[Random.Range(0, candidates.Count)]);
        }
        catch { }
    }

    private static void TryRandomSabotage()
    {
        try
        {
            var sabotages = Sabotage.GetSabotages();
            if (sabotages == null || sabotages.Count == 0) return;
            int pick = UnityEngine.Random.Range(0, sabotages.Count);
            int idx = 0;
            foreach (var (key, value) in sabotages)
            {
                if (idx == pick) { Sabotage.SabotageSystem(value); break; }
                idx++;
            }
        }
        catch { }
    }

    private static void TryFullyRandomize()
    {
        try { SpoofingTab.DoFullyRandomize(); } catch { }
    }

    private static void TryCallMeeting()
    {
        try
        {
            if (AmongUsClient.Instance.AmHost)
                Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
            else
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }
        catch { }
    }

    private static void TryMurderRandom()
    {
        try
        {
            if (PlayerControl.LocalPlayer?.Data == null) return;
            bool isHost = AmongUsClient.Instance.AmHost;
            bool isImpostor = RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType);
            if (!isHost && !isImpostor) return;

            if (PlayerControl.AllPlayerControls == null) return;
            var candidates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => p != null && !p.AmOwner && p.Data != null && !p.Data.IsDead &&
                            !RoleManager.IsImpostorRole(p.Data.RoleType))
                .ToList();
            if (candidates.Count == 0) return;
            PlayerControl.LocalPlayer.CmdCheckMurder(candidates[Random.Range(0, candidates.Count)]);
        }
        catch { }
    }

    private static void TryTeleMurderRandom()
    {
        try
        {
            if (PlayerControl.LocalPlayer?.Data == null) return;
            bool isHost = AmongUsClient.Instance.AmHost;
            bool isImpostor = RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType);
            if (!isHost && !isImpostor) return;

            if (PlayerControl.AllPlayerControls == null) return;
            var candidates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(p => p != null && !p.AmOwner && p.Data != null && !p.Data.IsDead &&
                            !RoleManager.IsImpostorRole(p.Data.RoleType))
                .ToList();
            if (candidates.Count == 0) return;
            PlayerControl.LocalPlayer.StartCoroutine(
                PlayersTab.TeleMurder(candidates[Random.Range(0, candidates.Count)]).WrapToIl2Cpp());
        }
        catch { }
    }

    private static System.Collections.IEnumerator CompleteTasksWithDelay()
    {
        var player = PlayerControl.LocalPlayer;
        int count = player.myTasks.Count;
        uint lastId = 0;
        bool hasLast = false;
        for (int i = 0; i < count; i++)
        {
            var task = player.myTasks[i];
            if (task == null || task.IsComplete) continue;
            if (hasLast && task.Id == lastId) continue;
            player.RpcCompleteTask(task.Id);
            lastId = task.Id;
            hasLast = true;
            yield return Effects.Wait(Utils.TaskCompleteSpacing);
        }
    }
}

