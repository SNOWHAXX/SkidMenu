using UnityEngine;

namespace SkidMenu;

public class PassiveTab : ITab
{
    public string name => "Passive";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.antiOverload = GUIStylePreset.CustomToggle(CheatToggles.antiOverload, " Anti-Overload");

        CheatToggles.freeCosmetics = GUIStylePreset.CustomToggle(CheatToggles.freeCosmetics, " Free Cosmetics");

        CheatToggles.avoidPenalties = GUIStylePreset.CustomToggle(CheatToggles.avoidPenalties, " Avoid Penalties");

        CheatToggles.unlockFeatures = GUIStylePreset.CustomToggle(CheatToggles.unlockFeatures, " Unlock Extra Features");

        CheatToggles.copyLobbyCodeOnDisconnect = GUIStylePreset.CustomToggle(CheatToggles.copyLobbyCodeOnDisconnect, " Copy Lobby Code on Disconnect");

        bool ret = GUIStylePreset.CustomToggle(SkidMenu.autoReturnAfterMatch, " Auto Return After Match");
        if (ret != SkidMenu.autoReturnAfterMatch) SkidMenu.autoReturnAfterMatch = ret;

        CheatToggles.spoofAprilFoolsDate = GUIStylePreset.CustomToggle(CheatToggles.spoofAprilFoolsDate, " Spoof Date to April 1st");

        CheatToggles.randomizeCosmetics = GUIStylePreset.CustomToggle(CheatToggles.randomizeCosmetics, " Randomize on Lobby Join");

        if (GUILayout.Button(" Randomize Now", GUILayout.Width(200)))
        {
            MalumRandomizer.Randomize();
        }
    }
}
