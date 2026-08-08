using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;

public static class ColorCache
{
    private static readonly Dictionary<Color, string> _cache = new();

    public static string ToHex(Color c)
    {
        if (!_cache.TryGetValue(c, out var hex))
            _cache[c] = ColorUtility.ToHtmlStringRGB(c);
        return hex;
    }

    public static string ToHex(Color32 c) => ToHex((Color)c);
}
