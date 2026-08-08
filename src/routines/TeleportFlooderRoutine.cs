using UnityEngine;

namespace SkidMenu.routines
{
    public class TeleportFlooderRoutine : IRoutine
    {
        public TeleportFlooderRoutine() { RoutineName = "TeleportFlooder"; }

        private float _timer = 0f;
        private const float Interval = 0.5f;
        private static readonly System.Random rnd = new System.Random();

        public override void Run()
        {
            if (ShipStatus.Instance == null) { Enabled = false; return; }
            _timer += Time.deltaTime;
            if (_timer < Interval) return;
            _timer = 0f;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == PlayerControl.LocalPlayer) continue;
                int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);
                features.Troll.TeleportToVent(player, ventId);
            }
        }
    }
}
