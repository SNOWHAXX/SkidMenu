using UnityEngine;

namespace SkidMenu;

public static class UIHelpers
{
    private static readonly Color DefaultDarkColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color ModernAccentColor = new Color(0.2f, 0.4f, 0.8f, 1f);

    public static void ApplyUIColor()
    {
        // Color override removed — styles handle their own backgrounds via GUIStylePreset
    }

    /// <summary>
    /// Gets a contrast color based on the current UI color for better readability
    /// </summary>
    public static Color GetContrastColor(Color baseColor)
    {
        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }

    /// <summary>
    /// Creates a modern highlighted color for interactive elements
    /// </summary>
    public static Color GetHighlightColor(Color baseColor)
    {
        return new Color(baseColor.r + 0.1f, baseColor.g + 0.1f, baseColor.b + 0.1f, baseColor.a);
    }
}
