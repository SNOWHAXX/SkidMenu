using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class ConfigTab : ITab
{
    public string name => "Config";

    public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
        {
            { $"17.4 / {Constants.AddressablesVersion} (Current)", Constants.GetBroadcastVersion() },
            { "16.1.0", 50632950 },
            { "17.1",   50643450 },
            { "17.1.2", 50647000 },
            { "17.2",   50645050 },
            { "17.2.1", 50652900 },
            { "17.2.2", 50653700 },
            { "17.3",   50655150 },
            { "17.4",   50656300 },
        };

    private int versionSelection = 0;

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(10);
        
		Chat.OnChat.LogChatMessages = GUIStylePreset.CustomToggle(Chat.OnChat.LogChatMessages, "Log chat messages to console");

		if(GUILayout.Button("Clear Notifications"))
		{
			SkidMenu.notifications.ClearNotifications();
			SkidMenu.notifications.Send("Notifications", "All notifications have been cleared.", 5);
		}

        GUILayout.Space(10);

        Spoofer.shouldSpoofVersion = GUIStylePreset.CustomToggle(Spoofer.shouldSpoofVersion, "Enable Version Spoofing");

        GUILayout.Label($"Spoofed Version: {versions.ElementAt(versionSelection).Key} ({Spoofer.spoofedVersion})");
        versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
        Spoofer.spoofedVersion = versions.ElementAt(versionSelection).Value;

        Spoofer.useModdedProtocol = GUIStylePreset.CustomToggle(Spoofer.useModdedProtocol, "Use Modded Protocol");

        GUILayout.Label($"Spoofed Platform: {Spoofer.spoofedPlatform}");
        Spoofer.spoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)Spoofer.spoofedPlatform, 0, 10);

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        if (GUILayout.Button("Open Config"))  CheatToggles.openConfig   = true;
        if (GUILayout.Button("Reload Config")) CheatToggles.reloadConfig = true;
        if (GUILayout.Button("Save to Profile"))   CheatToggles.saveProfile  = true;
        if (GUILayout.Button("Load from Profile")) CheatToggles.loadProfile  = true;
    }
}
