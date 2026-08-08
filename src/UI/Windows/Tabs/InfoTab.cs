using UnityEngine;

namespace SkidMenu;

public class InfoTab : ITab
{
    public string name => "Info";

    private GUIStyle _header;
    private GUIStyle _body;

    private void InitStyles()
    {
        if (_header != null) return;
        _header = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, richText = true };
        _body   = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, richText = true };
    }

    public void Draw()
    {
        InitStyles();
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.78f));

        // About
        GUILayout.Label("<color=#A2FAFC><b>About SkidMenu</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true });
        GUILayout.Space(4);
        GUILayout.Label("Created by SNOWHAXX", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is a host-side toolkit for running private Among Us lobbies. It puts per-player host controls, lobby management, and game-state monitoring in one panel, so a host can run clean, organized custom games without fighting the client. It also bundles a large set of convenience features for private play.", new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true });
        GUILayout.Space(8);

        // Warnings
        GUILayout.Label("<color=#63CCCF><b>Things to keep in mind</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true });
        GUILayout.Space(4);
        GUILayout.Label("This build is labeled Stable. No crashes or major performance issues were observed during testing, though some functions may still behave unexpectedly in certain situations. Using this menu can get you banned, so use with caution.", new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true });
        GUILayout.Space(6);

        GUILayout.Label("<color=#FF8888><b>For hosts and private lobbies</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true });
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is a host tool. It is meant to be run by the host of a private lobby, with friends who know what you're running and are fine with it. It is not a tool for public lobbies against strangers. Keep it in lobbies you host or run, and everyone keeps having a good time.", new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true });
        GUILayout.Space(4);
        GUILayout.Label("SkidMenu is not affiliated with, endorsed by, or sponsored by Innersloth in any way. Whatever you choose to do with this menu, including any bans, account issues, or other consequences, is entirely your own responsibility, not the menu's.", new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true });
        GUILayout.Space(10);

        // Links
        GUILayout.Label("<color=#A2FAFC><b>Links</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true });
        GUILayout.Space(4);
        GUILayout.Label("<color=#63CCCF>github.com/SNOWHAXX/SkidMenu</color>", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Space(4);
        if (GUILayout.Button("GitHub Repository", GUILayout.Width(150)))
        {
            Application.OpenURL("https://github.com/SNOWHAXX/SkidMenu");
        }
        GUILayout.Space(8);
        GUILayout.Label("We have an official Discord server for the community:", new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, richText = true });
        GUILayout.Space(4);
        if (GUILayout.Button("Join Discord", GUILayout.Width(120)))
        {
            Application.OpenURL("https://discord.gg/zgwTD4FFFx");
        }
        GUILayout.Space(4);

        // Disable keybinds toggle
        GUILayout.Label("<color=#A2FAFC><b>Keybinds</b></color>", new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true });
        GUILayout.Space(4);
        KeybindListener.KeybindsDisabled = GUIStylePreset.CustomToggle(KeybindListener.KeybindsDisabled, " Disable all keybinds");
        GUILayout.Space(6);

        // Static keybinds
        GUILayout.Label("<color=#63CCCF>Hold F1</color>   Close doors in your current room", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>Hold F2</color>   Close every door on the map", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>Hold F3</color>   Open every door on the map", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>Hold F6</color>   Trigger all sabotages at once", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>Hold F7</color>   Fix all active sabotages", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>Hold 7</color>    Spam electrical sabotage", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F4</color>        Complete your tasks one by one with a small delay between each", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F5</color>        Report a random dead body (in game only)", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F8</color>        Votekick everyone in the lobby", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F9</color>        Ban everyone in the lobby", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F10</color>       Ban all impostors", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>F11</color>       Ban a random player", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>0</color>         Call an emergency meeting (in game only)", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>9</color>         Kill a random player (in game only, host or impostor)", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
        GUILayout.Label("<color=#63CCCF>8</color>         Teleport kill a random player (in game only, host or impostor)", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
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
                    GUILayout.Label("<color=#A2FAFC>Custom toggle keybinds (set in Config tab)</color>", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
                    GUILayout.Space(4);
                    hasAny = true;
                }
                GUILayout.Label($"<color=#63CCCF>{key}</color>   {name}", new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true });
            }
        }

        GUILayout.EndVertical();
    }
}
