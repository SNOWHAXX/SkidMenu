using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class AnimationsTab : ITab
{
    public string name => "Animations";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawClientSided();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.animShields = GUIStylePreset.CustomToggle(CheatToggles.animShields, " Shields");

        CheatToggles.animAsteroids = GUIStylePreset.CustomToggle(CheatToggles.animAsteroids, " Asteroids");

        CheatToggles.animEmptyGarbage = GUIStylePreset.CustomToggle(CheatToggles.animEmptyGarbage, " Empty Garbage");

        CheatToggles.animMedScan = GUIStylePreset.CustomToggle(CheatToggles.animMedScan, " Medbay Scan");

        CheatToggles.animCamsInUse = GUIStylePreset.CustomToggle(CheatToggles.animCamsInUse, " Cams In Use");

        // CheatToggles.animPet = GUIStylePreset.CustomToggle(CheatToggles.animPet, " Pet");
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Client-Sided", GUIStylePreset.TabSubtitle);

        CheatToggles.moonWalk = GUIStylePreset.CustomToggle(CheatToggles.moonWalk, " Moonwalk");

        Visuals.SkipShhhAnimation.Enabled = GUIStylePreset.CustomToggle(Visuals.SkipShhhAnimation.Enabled, " Skip Shhh Animation");
        SkipRoleReveal.Enabled    = GUIStylePreset.CustomToggle(SkipRoleReveal.Enabled,    " Skip Role Reveal");
        var warningStyle = new GUIStyle(GUIStylePreset.ModernLabel) { normal = { textColor = new UnityEngine.Color(1f, 0.2f, 0.2f, 1f) } };
        GUILayout.Label("  Warning: this feature can cause issues with chat and meetings in-game.", warningStyle);
    }
}
