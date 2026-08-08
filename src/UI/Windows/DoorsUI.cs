using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace SkidMenu;

public class DoorsUI : MonoBehaviour
{
    public static int windowHeight = 420;
    public static int windowWidth = 520;
    private Rect _windowRect;
    private Vector2 _scrollPos;

    public static DoorsUI Instance { get; private set; }
    public Rect WindowRect { get => _windowRect; set => _windowRect = value; }
    public static Rect LastWindowRect;
    public static Rect PendingRect;
    public static bool PendingRectSet;

    private List<SystemTypes> _doorsToSpamOpen  = new();
    private List<SystemTypes> _doorsToSpamClose = new();
    private System.Collections.Generic.HashSet<SystemTypes> _spamOpenSet  = new();
    private System.Collections.Generic.HashSet<SystemTypes> _spamCloseSet = new();
    private MapNames _cachedMap;
    private bool _mapCached;

    private void Start()
    {
        Instance = this;
        _windowRect = PendingRectSet ? PendingRect : new Rect(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
        PendingRectSet = false;
    }

    private void OnGUI()
    {
        if (!CheatToggles.showDoorsMenu || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen.Value) || SkidMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();

        Matrix4x4 prev = GUI.matrix;
        if (CheatToggles.doorsScaleH != 100f || CheatToggles.doorsScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.doorsScaleH / 100f, CheatToggles.doorsScaleV / 100f), pivot);
        }
        _windowRect = GUI.Window((int)WindowId.DoorsUI, _windowRect, (GUI.WindowFunction)DoorsWindow, "Doors", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.matrix = prev;
    }

    private void DoorsWindow(int windowID)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        if (!Utils.isShip)
        {
            MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
            GUI.DragWindow();
            return;
        }

        if (!_mapCached) { _cachedMap = (MapNames)Utils.GetCurrentMapID(); _mapCached = true; }
        var map = _cachedMap;

        if (map is MapNames.MiraHQ)
        {
            MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
            GUI.DragWindow();
            return;
        }

        GUILayout.BeginVertical();

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

        foreach (var doorRoom in DoorsHandler.GetRoomsWithDoors())
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{doorRoom.ToString()}", GUIStylePreset.ModernLabel, GUILayout.Width(110f));

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{DoorsHandler.GetStatusOfDoorsInRoom(doorRoom, true)}");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUIStylePreset.NormalButton, GUILayout.Width(70f)))
                DoorsHandler.CloseDoorsInRoom(doorRoom);

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                if (GUILayout.Button("Open", GUIStylePreset.NormalButton, GUILayout.Width(70f)))
                    DoorsHandler.OpenDoorsInRoom(doorRoom);
            }

            if (Utils.isHost)
            {
                var spamClose = _spamCloseSet.Contains(doorRoom);
                var newSpamClose = GUIStylePreset.CustomToggle(spamClose, "Spam Close");
                if (newSpamClose != spamClose)
                {
                    if (newSpamClose) { _spamCloseSet.Add(doorRoom); _doorsToSpamClose.Add(doorRoom); }
                    else              { _spamCloseSet.Remove(doorRoom); _doorsToSpamClose.Remove(doorRoom); }
                }

                if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
                {
                    var spamOpen = _spamOpenSet.Contains(doorRoom);
                    var newSpamOpen = GUIStylePreset.CustomToggle(spamOpen, "Spam Open");
                    if (newSpamOpen != spamOpen)
                    {
                        if (newSpamOpen) { _spamOpenSet.Add(doorRoom); _doorsToSpamOpen.Add(doorRoom); }
                        else             { _spamOpenSet.Remove(doorRoom); _doorsToSpamOpen.Remove(doorRoom); }
                    }
                }
            }
            else
            {
                if (_spamCloseSet.Count != 0 || _spamOpenSet.Count != 0)
                {
                    _doorsToSpamClose.Clear(); _spamCloseSet.Clear();
                    _doorsToSpamOpen.Clear();  _spamOpenSet.Clear();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.ExpandWidth(true));
        GUILayout.Space(1f);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Close All", GUIStylePreset.NormalButton))
        {
            CheatToggles.closeAllDoors = true;
        }

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            if (GUILayout.Button("Open All", GUIStylePreset.NormalButton))
            {
                CheatToggles.openAllDoors = true;
            }
        }

        GUILayout.FlexibleSpace();

        if (Utils.isHost)
        {
            CheatToggles.spamCloseAllDoors = GUIStylePreset.CustomToggle(CheatToggles.spamCloseAllDoors, "Spam Close All");

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                CheatToggles.spamOpenAllDoors = GUIStylePreset.CustomToggle(CheatToggles.spamOpenAllDoors, "Spam Open All");
            }
        }
        else
        {
            CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
        GUI.DragWindow();
    }

    public void Update()
    {
        if (!Utils.isShip) { _mapCached = false; return; }

        if (!_mapCached) _cachedMap = (MapNames)Utils.GetCurrentMapID();
        _mapCached = true;

        // Spam close selected doors
        foreach (var doorRoom in _doorsToSpamClose)
        {
            DoorsHandler.CloseDoorsInRoom(doorRoom);
        }

        // Spam open selected doors
        if (_cachedMap is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            foreach (var doorRoom in _doorsToSpamOpen)
            {
                DoorsHandler.OpenDoorsInRoom(doorRoom);
            }
        }
    }
}




