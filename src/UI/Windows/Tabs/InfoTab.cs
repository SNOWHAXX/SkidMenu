using UnityEngine;

namespace SkidMenu;

public class InfoTab : ITab
{
    public string name => "Info";

    private GUIStyle _header;
    private GUIStyle _subheader;
    private GUIStyle _sub2;
    private GUIStyle _body;
    private GUIStyle _line;
    private float _contentWidth;

    private void InitStyles()
    {
        if (_header != null) return;
        _contentWidth = MenuUI.windowWidth * 0.78f;
        _header    = new GUIStyle(GUI.skin.label) { font = GUIStylePreset.FontBold, fontSize = 16, fontStyle = FontStyle.Bold, richText = true, wordWrap = false };
        _subheader = new GUIStyle(GUI.skin.label) { font = GUIStylePreset.FontBold, fontSize = 15, fontStyle = FontStyle.Bold, richText = true, wordWrap = false };
        _sub2      = new GUIStyle(GUI.skin.label) { font = GUIStylePreset.FontBold, fontSize = 14, fontStyle = FontStyle.Bold, richText = true, wordWrap = false };
        _body      = new GUIStyle(GUI.skin.label) { font = GUIStylePreset.FontRegular, fontSize = 13, richText = true, wordWrap = true };
        _line      = new GUIStyle(GUI.skin.label) { font = GUIStylePreset.FontRegular, fontSize = 13, richText = true, wordWrap = false };
    }

    public void Draw()
    {
        InitStyles();
        var w = GUILayout.Width(_contentWidth);
        GUILayout.BeginVertical();

        // About
        GUILayout.Label("<color=#A2FAFC><b>About SkidMenu</b></color>", _header, w);
        GUILayout.Space(4);
        GUILayout.Label("Created by SNOWHAXX", _line, w);
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is a host-side toolkit for running private Among Us lobbies. It puts per-player host controls, lobby management, and game-state monitoring in one panel, so a host can run clean, organized custom games without fighting the client. It also bundles a large set of convenience features for private play.", _body, w);
        GUILayout.Space(8);

        // Warnings
        GUILayout.Label("<color=#63CCCF><b>Things to keep in mind</b></color>", _subheader, w);
        GUILayout.Space(4);
        GUILayout.Label("This build is labeled Stable. No crashes or major performance issues were observed during testing, though some functions may still behave unexpectedly in certain situations. Using this menu can get you banned, so use with caution.", _body, w);
        GUILayout.Space(6);

        GUILayout.Label("<color=#FF8888><b>For hosts and private lobbies</b></color>", _sub2, w);
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is a host tool. It is meant to be run by the host of a private lobby, with friends who know what you're running and are fine with it. It is not a tool for public lobbies against strangers. Keep it in lobbies you host or run, and everyone keeps having a good time.", _body, w);
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is not affiliated with, endorsed by, or sponsored by Innersloth in any way. Whatever you choose to do with this menu, including any bans, account issues, or other consequences, is entirely your own responsibility, not the menu's.", _body, w);
        GUILayout.Space(10);

        // Links
        GUILayout.Label("<color=#A2FAFC><b>Links</b></color>", _subheader, w);
        GUILayout.Space(4);
        GUILayout.Label("<color=#63CCCF>github.com/SNOWHAXX/SkidMenu</color>", _line, w);
        GUILayout.Space(4);
        if (GUILayout.Button("GitHub Repository", GUILayout.Width(150)))
        {
            Application.OpenURL("https://github.com/SNOWHAXX/SkidMenu");
        }
        GUILayout.Space(8);
        GUILayout.Label("We have an official Discord server for the community:", _body, w);
        GUILayout.Space(4);
        if (GUILayout.Button("Join Discord", GUILayout.Width(120)))
        {
            Application.OpenURL("https://discord.gg/zgwTD4FFFx");
        }
        GUILayout.Space(4);

        // Disable keybinds toggle
        GUILayout.Label("<color=#A2FAFC><b>Keybinds</b></color>", _subheader, w);
        GUILayout.Space(4);
        KeybindListener.KeybindsDisabled = GUIStylePreset.CustomToggle(KeybindListener.KeybindsDisabled, " Disable all keybinds");
        GUILayout.Space(6);

        // Static keybinds
        GUILayout.Label("<color=#63CCCF>Hold F1</color>   Close doors in your current room", _line, w);
        GUILayout.Label("<color=#63CCCF>Hold F2</color>   Close every door on the map", _line, w);
        GUILayout.Label("<color=#63CCCF>Hold F3</color>   Open every door on the map", _line, w);
        GUILayout.Label("<color=#63CCCF>Hold F6</color>   Trigger all sabotages at once", _line, w);
        GUILayout.Label("<color=#63CCCF>Hold F7</color>   Fix all active sabotages", _line, w);
        GUILayout.Label("<color=#63CCCF>Hold 7</color>    Spam electrical sabotage", _line, w);
        GUILayout.Label("<color=#63CCCF>F4</color>        Complete your tasks one by one with a small delay between each", _line, w);
        GUILayout.Label("<color=#63CCCF>F5</color>        Report a random dead body (in game only)", _line, w);
        GUILayout.Label("<color=#63CCCF>F8</color>        Votekick everyone in the lobby", _line, w);
        GUILayout.Label("<color=#63CCCF>F9</color>        Ban everyone in the lobby", _line, w);
        GUILayout.Label("<color=#63CCCF>F10</color>       Ban all impostors", _line, w);
        GUILayout.Label("<color=#63CCCF>F11</color>       Ban a random player", _line, w);
        GUILayout.Label("<color=#63CCCF>0</color>         Call an emergency meeting (in game only)", _line, w);
        GUILayout.Label("<color=#63CCCF>9</color>         Kill a random player (in game only, host or impostor)", _line, w);
        GUILayout.Label("<color=#63CCCF>8</color>         Teleport kill a random player (in game only, host or impostor)", _line, w);
        GUILayout.Space(8);

        // Custom keybinds
        if (CheatToggles.Keybinds != null && CheatToggles.Keybinds.Count > 0)
        {
            bool hasAny = false;
            foreach (var (name, key) in CheatToggles.Keybinds)
            {
                if (key == UnityEngine.KeyCode.None) continue;
                if (!hasAny)
                {
                    GUILayout.Label("<color=#A2FAFC>Custom toggle keybinds (set in Config tab)</color>", _sub2, w);
                    GUILayout.Space(4);
                    hasAny = true;
                }
                GUILayout.Label($"<color=#63CCCF>{key}</color>   {name}", _line, w);
            }
        }

        GUILayout.EndVertical();
    }
}
