using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SkidMenu;

public class KeybindListener : MonoBehaviour
{
    public static bool KeybindsDisabled = false;

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
                field.SetValue(null, !(bool)field.GetValue(null));
            }
        }

        if (PlayerControl.LocalPlayer == null) return;

        HandleHold(KeyCode.F1, ref _f1Timer,  TryCloseCurrentRoom);
        HandleHold(KeyCode.F2, ref _f2Timer,  () => { try { DoorsHandler.CloseAllDoors(); } catch { } });
        HandleHold(KeyCode.F3, ref _f3Timer,  TryOpenAllDoors);
        HandleHold(KeyCode.F6, ref _f6Timer,  () => { Notif_Sabotage.SuppressNext = true; try { Sabotage.SabotageAll(); } catch { } });
        HandleHold(KeyCode.F7, ref _f7Timer,  () => { try { Sabotage.FixAllSabotages(); } catch { } });
        HandleHold(KeyCode.Alpha7, ref _key7Timer, TryElectricalSabotage, SpamInterval);

        if (Input.GetKeyDown(KeyCode.F4) && !RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
            PlayerControl.LocalPlayer.StartCoroutine(CompleteTasksWithDelay().WrapToIl2Cpp());
        if (Input.GetKeyDown(KeyCode.F8))  TryVotekickAll();
        if (Input.GetKeyDown(KeyCode.F9))  TryVentKickAll();
        if (Input.GetKeyDown(KeyCode.F10)) TryVentKickImpostors();
        if (Input.GetKeyDown(KeyCode.F11)) TryVentKickRandom();

        if (!InGame) return;

        if (Input.GetKeyDown(KeyCode.F5))  TryReportRandomBody();
        if (Input.GetKeyDown(KeyCode.Alpha0)) TryCallMeeting();
        if (Input.GetKeyDown(KeyCode.Alpha9)) TryMurderRandom();
        if (Input.GetKeyDown(KeyCode.Alpha8)) TryTeleMurderRandom();
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

