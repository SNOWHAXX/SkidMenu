using System.IO;
using UnityEngine;

namespace SkidMenu;

public class PlayerInfosUI : MonoBehaviour
{
    public static PlayerInfosUI Instance { get; private set; }
    public static bool IsOpen = false;

    private Rect _windowRect;
    public Rect WindowRect => _windowRect;
    public static Rect LastWindowRect;
    private Vector2 _scroll = Vector2.zero;
    private string[] _saves = System.Array.Empty<string>();

    private const int W = 520;
    private const int H = 460;

    private void Start()
    {
        Instance = this;
        _windowRect = new Rect(
            Screen.width  / 2f - W / 2f,
            Screen.height / 2f - H / 2f,
            W, H
        );
    }

    private void OnGUI()
    {
        if (!IsOpen || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen.Value) || SkidMenu.isPanicked) return;
        var prevSkin = GUI.skin;
        GUI.skin = MenuUI.GetCustomSkin();
        _windowRect = GUI.Window((int)WindowId.PlayerInfosUI, _windowRect, (GUI.WindowFunction)Draw, "", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.skin = prevSkin;
    }

    private void Draw(int id)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        GUILayout.BeginVertical();

        GUILayout.Label("Saved Player Infos", GUIStylePreset.TabTitle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        GUILayout.Space(4);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(H - 160));

        if (_saves.Length == 0)
        {
            GUILayout.Space(8);
            GUILayout.Label("No saves found. Use 'Save Info' in the Players tab.", GUIStylePreset.ModernLabel);
        }
        else
        {
            foreach (var file in _saves)
            {
                GUILayout.BeginHorizontal();
                var label = Path.GetFileNameWithoutExtension(file);
                if (GUILayout.Button(label, GUIStylePreset.NormalButton))
                {
                    if (SavedPlayerInfo.LoadFromFile(file))
                    {
                        SavedPlayerInfo.ApplyToLocalPlayer();
                        SkidMenu.notifications.Send("Load Info", $"Applied {SavedPlayerInfo.Name}\u2019s info", 4f);
                        IsOpen = false;
                    }
                    else
                    {
                        SkidMenu.notifications.Send("Load Info", "Failed to load file", 4f);
                    }
                }
                if (GUILayout.Button("X", GUIStylePreset.NormalButton, GUILayout.Width(32)))
                {
                    try { File.Delete(file); } catch { }
                    RefreshSaves();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(6);
        GUILayout.Box("", GUIStylePreset.DarkSeparator, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        GUILayout.Space(4);

        var btnW = GUILayout.Width((W - 24) / 4);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUIStylePreset.NormalButton, btnW)) RefreshSaves();
        if (GUILayout.Button("Random", GUIStylePreset.NormalButton, btnW)) LoadRandom();
        if (GUILayout.Button("Open Folder", GUIStylePreset.NormalButton, btnW))
            System.Diagnostics.Process.Start("explorer.exe", SavedPlayerInfo.FolderPath);
        if (GUILayout.Button("Close", GUIStylePreset.NormalButton, btnW)) IsOpen = false;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        MenuUI.DrawBgAndOverlay(_windowRect.width, _windowRect.height);
        GUI.DragWindow();
    }

    public void RefreshSaves()
    {
        _saves = SavedPlayerInfo.ListSaves();
    }

    private void LoadRandom()
    {
        if (_saves.Length == 0) { SkidMenu.notifications.Send("Random Info", "No saves found.", 3f); return; }
        var file = _saves[UnityEngine.Random.Range(0, _saves.Length)];
        if (SavedPlayerInfo.LoadFromFile(file))
        {
            SavedPlayerInfo.ApplyToLocalPlayer();
            SkidMenu.notifications.Send("Random Info", $"Applied {SavedPlayerInfo.Name}", 4f);
            IsOpen = false;
        }
        else SkidMenu.notifications.Send("Random Info", "Failed to load file", 4f);
    }

    public static void Open()
    {
        if (Instance == null) return;
        Instance.RefreshSaves();
        IsOpen = true;
    }
}
