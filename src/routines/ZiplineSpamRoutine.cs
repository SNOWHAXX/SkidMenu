using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SkidMenu.routines
{
    public class ZiplineSpamRoutine : IRoutine
    {
        public ZiplineSpamRoutine() { RoutineName = "ZiplineSpam"; }
        private float _timer = 0f;
        private const float Interval = 0.5f;
        public static HashSet<byte> Marked = new();
        public static bool Active = false;
        public static bool SpamDirection = false;
        public static bool IsPerPlayer = false;
        public static byte PerPlayerId = 0;
        private static HashSet<byte> _savedMarked = new();

        public override void Run()
        {
            if (!Active) return;
            _timer += Time.deltaTime;
            if (_timer < Interval) return;
            _timer = 0f;
            ZiplineBehaviour line = features.ZiplineTools.GetLine();
            if (line == null) return;
            HashSet<byte> targets = IsPerPlayer ? new HashSet<byte> { PerPlayerId } : Marked;
            if (targets.Count == 0) { Active = false; return; }
            foreach (byte pid in targets)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (p != null && p.PlayerId == pid)
                    {
                        try { p.RpcUseZipline(p, line, SpamDirection); } catch { }
                        break;
                    }
            }
        }

        public static void StartPerPlayer(byte playerId, bool direction)
        {
            if (Active && IsPerPlayer && PerPlayerId == playerId && SpamDirection == direction)
            {
                Stop();
                return;
            }
            if (Active) _savedMarked = new HashSet<byte>(Marked);
            IsPerPlayer = true;
            PerPlayerId = playerId;
            SpamDirection = direction;
            Active = true;
        }

        public static void Stop()
        {
            Active = false;
            if (IsPerPlayer && _savedMarked.Count > 0)
            {
                Marked = _savedMarked;
                _savedMarked = new HashSet<byte>();
            }
            IsPerPlayer = false;
            PerPlayerId = 0;
        }
    }
}
