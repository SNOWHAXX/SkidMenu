using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu.features
{
    public static class ZiplineTools
    {
        public static ZiplineBehaviour GetLine()
        {
            try
            {
                ShipStatus ship = ShipStatus.Instance;
                if (ship == null) return null;
                FungleShipStatus fungle = ship.TryCast<FungleShipStatus>();
                if (fungle != null && fungle.Zipline != null) return fungle.Zipline;
                return UnityEngine.Object.FindObjectOfType<ZiplineBehaviour>();
            }
            catch { return null; }
        }

        public static string Ride(PlayerControl target, bool fromTop)
        {
            if (target == null || target.Data == null || target.Data.Disconnected) return "No target";
            if (ShipStatus.Instance == null) return "In-match only";
            ZiplineBehaviour line = GetLine();
            if (line == null) return "Zipline only on Fungle";
            try { target.RpcUseZipline(target, line, fromTop); return (fromTop ? "Down: " : "Up: ") + target.Data.PlayerName; }
            catch { return "Failed"; }
        }

        public static string RideAll(HashSet<byte> marked, bool fromTop)
        {
            if (ShipStatus.Instance == null) return "In-match only";
            ZiplineBehaviour line = GetLine();
            if (line == null) return "Zipline only on Fungle";
            int n = 0;
            foreach (byte pid in marked)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (p != null && p.PlayerId == pid)
                    { try { p.RpcUseZipline(p, line, fromTop); n++; } catch { } break; }
            }
            return (fromTop ? "Down: " : "Up: ") + n;
        }
    }
}
