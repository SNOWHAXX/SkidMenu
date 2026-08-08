using System;
using System.Collections.Generic;

namespace SkidMenu.ui
{
    internal static class NotificationLive
    {
        private static readonly Dictionary<Notification, Func<string>> _live = new Dictionary<Notification, Func<string>>();

        internal static void AttachAcid(Notification notification, string baseMessage, DeadBody body)
        {
            if (notification == null || body == null) return;
            _live[notification] = () =>
            {
                try
                {
                    float remaining = ViperBodies.Remaining(body);
                    return remaining > 0f
                        ? $"{baseMessage} <color=#00ff88>[ACID {remaining:F1}s]</color>"
                        : $"{baseMessage} <color=#00ff88>[ACID gone]</color>";
                }
                catch { return baseMessage; }
            };
        }

        internal static void AttachAcid(Notification notification, string baseMessage, byte parentId)
        {
            if (notification == null) return;
            _live[notification] = () =>
            {
                try
                {
                    float remaining = ViperBodies.Remaining(parentId);
                    if (remaining < 0f) return baseMessage;
                    return remaining > 0f
                        ? $"{baseMessage} <color=#00ff88>[ACID {remaining:F1}s]</color>"
                        : $"{baseMessage} <color=#00ff88>[ACID gone]</color>";
                }
                catch { return baseMessage; }
            };
        }

        internal static void Tick(List<Notification> notifications)
        {
            if (notifications == null) return;
            for (int i = 0; i < notifications.Count; i++)
            {
                Notification n = notifications[i];
                if (_live.TryGetValue(n, out Func<string> provider))
                {
                    string text = provider();
                    if (text == null) _live.Remove(n);
                    else n.message = text;
                }
            }
            if (_live.Count >= 64)
            {
                var gone = new List<Notification>();
                foreach (var kv in _live)
                    if (kv.Key.HasExpired) gone.Add(kv.Key);
                for (int i = 0; i < gone.Count; i++) _live.Remove(gone[i]);
            }
        }
    }
}
