using UnityEngine;

namespace SkidMenu;

public static class PlayerCache
{
    private static PlayerControl[] _cached = System.Array.Empty<PlayerControl>();
    private static float _nextRefresh = 0f;

    public static PlayerControl[] Get()
    {
        if (Time.time >= _nextRefresh)
        {
            _cached = PlayerControl.AllPlayerControls?.ToArray() ?? System.Array.Empty<PlayerControl>();
            _nextRefresh = Time.time + 0.5f;
        }
        return _cached;
    }

    public static void Invalidate() => _nextRefresh = 0f;
}
