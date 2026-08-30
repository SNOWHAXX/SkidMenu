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
        private bool _flip = false;
        public static HashSet<byte> Marked = new();

        public override void Run()
        {
            if (Marked.Count == 0) return;
            _timer += Time.deltaTime;
            if (_timer < Interval) return;
            _timer = 0f;
            ZiplineBehaviour line = features.ZiplineTools.GetLine();
            if (line == null) return;
            _flip = !_flip;
            bool fromTop = _flip;
            foreach (byte pid in Marked)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (p != null && p.PlayerId == pid)
                    {
                        try { p.RpcUseZipline(p, line, fromTop); } catch { }
                        break;
                    }
            }
        }
    }
}
