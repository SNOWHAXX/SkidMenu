using UnityEngine;

namespace SkidMenu;

public class TasksUI : MonoBehaviour
{
    public static int windowHeight = 300;
    public static int windowWidth = 500;
    private Rect _windowRect;
    public static TasksUI Instance { get; private set; }
    public Rect WindowRect { get => _windowRect; set => _windowRect = value; }
    public static Rect LastWindowRect;
    public static Rect PendingRect;
    public static bool PendingRectSet;

    private Vector2 _scrollPosition = Vector2.zero;
    private GUIStyle _playerHeaderStyle;
    private Il2CppSystem.Text.StringBuilder _tasksString = new();
    private readonly System.Collections.Generic.Dictionary<string, bool> _expandedPlayers = new();
    private static readonly System.Text.StringBuilder _replaceBuilder = new(256);

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
        if (!CheatToggles.showTasksMenu || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen) || SkidMenu.isPanicked) return;

        _playerHeaderStyle ??= new GUIStyle(GUI.skin.button)
        {
            font      = GUIStylePreset.FontBold,
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset { left = 8, right = 8, top = 7, bottom = 7 },
            richText  = true,
            normal    = { background = GUIStylePreset.MakeTex1x1(new Color(0.13f, 0.13f, 0.13f, 1f)), textColor = new Color(0.93f, 0.93f, 0.95f) },
            hover     = { background = GUIStylePreset.MakeTex1x1(new Color(0.18f, 0.18f, 0.18f, 1f)), textColor = Color.white },
            active    = { background = GUIStylePreset.MakeTex1x1(new Color(0.22f, 0.22f, 0.22f, 1f)), textColor = Color.white }
        };

        UIHelpers.ApplyUIColor();

        Matrix4x4 prev = GUI.matrix;
        if (CheatToggles.tasksScaleH != 100f || CheatToggles.tasksScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.tasksScaleH / 100f, CheatToggles.tasksScaleV / 100f), pivot);
        }
        _windowRect = GUI.Window((int)WindowId.TasksUI, _windowRect, (GUI.WindowFunction)TasksWindow, "Tasks", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.matrix = prev;
    }

    private void TasksWindow(int windowID)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName)) continue;

            GUILayout.BeginVertical();

            var nameKey = player.Data.PlayerName;
            _expandedPlayers.TryGetValue(nameKey, out var expanded);
            var arrow = expanded ? "\u25BC" : "\u25B6"; // � or ?

            var taskCount = player.myTasks.Count;
            int completeCount = 0;
            foreach (var t in player.myTasks) if (t.IsComplete) completeCount++;

            if (player == PlayerControl.LocalPlayer && player.Data.IsDead)      taskCount -= 1;
            if (player == PlayerControl.LocalPlayer && Utils.isAnySabotageActive) taskCount -= 1;
            if (player == PlayerControl.LocalPlayer && player.Data.Role.IsImpostor) taskCount -= 1;

            if (GUILayout.Button($"{arrow} [{completeCount}/{taskCount}] <color=#{ColorCache.ToHex(player.Data.Color)}>{nameKey}</color>", _playerHeaderStyle))
            {
                _expandedPlayers[nameKey] = !expanded;
                expanded = !expanded;
            }

            if (expanded)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                foreach (var task in player.myTasks)
                {
                    // Do some checks to not show texts: sabotage active, dead hint, impostor hint
                    if (task.TaskType is TaskTypes.ResetReactor or TaskTypes.RestoreOxy or TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.ResetSeismic or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage) continue;

                    _tasksString.Clear();
                    task.AppendTaskText(_tasksString);
                    var rawText = _tasksString.ToString();

                    if (rawText.Contains("You're dead") || rawText.Contains("Sabotage and kill")) continue;

                    _replaceBuilder.Clear();
                    foreach (char c in rawText)
                    {
                        if (c != '\n') _replaceBuilder.Append(c);
                    }
                    var taskText = _replaceBuilder.ToString()
                        .Replace("</color>", "")
                        .Replace("<color=#00DD00FF>", "")
                        .Replace("<color=#FFFF00FF>", "");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(taskText);
                    GUILayout.FlexibleSpace();

                    if (task.IsComplete)
                    {
                        GUILayout.Label("<color=#00ff00>? Complete</color>");
                    }
                    else
                    {
                        if (GUILayout.Button("Complete", GUIStylePreset.NormalButton))
                        {
                            Utils.CompleteTask(player, task.Cast<PlayerTask>());
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Complete My Tasks", GUIStylePreset.NormalButton, GUILayout.Height(30)))
        {
            CheatToggles.completeMyTasks = true;
        }


        GUILayout.EndVertical();

        MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
        GUI.DragWindow();
    }
}




