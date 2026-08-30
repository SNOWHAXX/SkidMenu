using UnityEngine;

namespace SkidMenu;

public static class ESPContexts
{
    public const byte InGame    = 1;
    public const byte InLobby   = 2;
    public const byte InMeeting = 4;
    public const byte InChat    = 8;
    public const byte InGuide   = 16;
    public const byte All       = 31;

    public static byte KillCooldown = All;
    public static byte Tasks        = All;
    public static byte ShowRole     = All;
    public static byte ShowInfo     = All;
    public static byte IsHost       = All;
    public static byte Level        = All;
    public static byte Platform     = All;
    public static byte Votekicks    = All;
    public static byte FriendCode   = All;
    public static byte Puid         = All;
    public static byte DeviceId     = All;
    public static byte ModUser      = All;

    private static byte _cachedContext = InGame;

    public static void UpdateContext()
    {
        if      (MeetingHud.Instance      != null) _cachedContext = InMeeting;
        else if (LobbyBehaviour.Instance  != null) _cachedContext = InLobby;
        else                                       _cachedContext = InGame;
    }

    public static bool Allow(byte ctx, bool isChat)
    {
        byte effective = isChat ? InChat : _cachedContext;
        return (ctx & effective) != 0;
    }

    public static void Toggle(ref byte ctx, byte flag) =>
        ctx = (ctx & flag) != 0 ? (byte)(ctx & ~flag) : (byte)(ctx | flag);
}
