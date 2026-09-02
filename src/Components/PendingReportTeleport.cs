using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class PendingReportTeleport
{
    public static Vector2? Position = null;

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class TeleportOnMeetingStart
    {
        static void Prefix()
        {
            if (Position == null) return;
            Teleporter.TeleportToLocal(Position.Value);
            Position = null;
        }
    }
}
