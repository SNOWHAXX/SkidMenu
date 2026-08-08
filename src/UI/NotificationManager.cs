using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu.ui
{
    public class NotificationManager : MonoBehaviour
    {
        public Vector2 boxSize = new Vector2(325, 90);
        public List<Notification> notifications = new List<Notification>();
        public bool DisableNotifications = false;

        public void Update()
        {
            int notificaions = Math.Min(GetMaxNotifications(), notifications.Count);

            for (byte i = 0; i < notificaions; i++)
            {
                Notification notification = notifications[i];
                notification.lifetime += Time.deltaTime;

                if (notification.HasExpired)
                {
                    notifications.RemoveAt(i);

                    // Since we removed an element from the notifications list, we have to decrement both the current notification index
                    // and the max notifications to avoid errors from accessing outside the list length
                    i--;
                    notificaions--;
                    continue;
                }
            }

            NotificationLive.Tick(notifications);
        }

        public void OnGUI()
        {
            if (DisableNotifications) return;

            int notificaions = Math.Min(GetMaxNotifications(), notifications.Count);

            for (byte i = 0; i < notificaions; i++)
            {
                RenderNotification(i, notifications[i]);
            }
        }

        private static Texture2D _notifBg;
        private static Texture2D _barBg;
        private static Texture2D _barFill;

        private static Texture2D NotifBg   => _notifBg  ??= MakeTex(new Color(0.07f, 0.07f, 0.07f, 0.92f));
        private static Texture2D BarBg     => _barBg    ??= MakeTex(new Color(0.14f, 0.14f, 0.14f, 1f));
        private static Texture2D BarFill   => _barFill  ??= MakeTex(new Color(0.60f, 1.00f, 0.99f, 1f));

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }

        private static GUIStyle _titleStyle;
        private static GUIStyle _msgStyle;

        private GUIStyle TitleStyle => _titleStyle ??= new GUIStyle
        {
            font      = GUIStylePreset.FontBold,
            fontSize  = 13,
            richText  = true,
            wordWrap  = false,
            normal    = { textColor = new Color(0.60f, 1.00f, 0.99f, 1f) },
            padding   = new RectOffset { left = 10, right = 6, top = 6,  bottom = 0 }
        };

        private GUIStyle MsgStyle => _msgStyle ??= new GUIStyle
        {
            font      = GUIStylePreset.FontRegular,
            fontSize  = 12,
            richText  = true,
            wordWrap  = true,
            normal    = { textColor = new Color(0.88f, 0.88f, 0.90f, 1f) },
            padding   = new RectOffset { left = 10, right = 6, top = 4, bottom = 0 }
        };

        private static GUIStyle _boxStyle;
        private static GUIStyle _barBgStyle;
        private static GUIStyle _barFillStyle;

        private GUIStyle BoxStyle => _boxStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = NotifBg, textColor = Color.clear },
            border  = new RectOffset(),
            padding = new RectOffset()
        };

        private GUIStyle BarBgStyle => _barBgStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal  = { background = BarBg },
            border  = new RectOffset(),
            padding = new RectOffset()
        };

        private GUIStyle BarFillStyle => _barFillStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal  = { background = BarFill },
            border  = new RectOffset(),
            padding = new RectOffset()
        };

        private void RenderNotification(byte position, Notification notification)
        {
            float boxX = Screen.width  - boxSize.x - 8f;
            float boxY = Screen.height - boxSize.y * (position + 1) - 8f;

            GUI.Box(new Rect(boxX, boxY, boxSize.x, boxSize.y), GUIContent.none, BoxStyle);

            GUI.Label(new Rect(boxX, boxY,       boxSize.x, 24f),              notification.title,   TitleStyle);
            GUI.Label(new Rect(boxX, boxY + 22f, boxSize.x, boxSize.y - 36f), notification.message, MsgStyle);

            float progress = 1f - (notification.lifetime / notification.ttl);
            float barH = 4f;
            float barY = boxY + boxSize.y - barH;
            GUI.Box(new Rect(boxX, barY, boxSize.x, barH), GUIContent.none, BarBgStyle);
            GUI.Box(new Rect(boxX, barY, boxSize.x * progress, barH), GUIContent.none, BarFillStyle);
        }

        public int GetMaxNotifications()
        {
            return (Screen.height / 2) / (int)boxSize.y;
        }

        // The time to live value for a notification should be five seconds if it is a success message, and ten seconds if it is a failure message
        public void Send(string title, string message, float ttl = 10)
        {
            SkidMenu.Log.LogMessage($"[Notification] [{title}] {message}");

            if (DisableNotifications) return;

            Notification notification = new Notification(title, message, ttl);
            notifications.Add(notification);
        }

        public void SendLive(string title, string baseMessage, float ttl, DeadBody body)
        {
            SkidMenu.Log.LogMessage($"[Notification] [{title}] {baseMessage}");

            Notification notification = new Notification(title, baseMessage, ttl);
            NotificationLive.AttachAcid(notification, baseMessage, body);
            if (!DisableNotifications) notifications.Add(notification);
        }

        public void SendLive(string title, string baseMessage, float ttl, byte parentId)
        {
            SkidMenu.Log.LogMessage($"[Notification] [{title}] {baseMessage}");

            Notification notification = new Notification(title, baseMessage, ttl);
            NotificationLive.AttachAcid(notification, baseMessage, parentId);
            if (!DisableNotifications) notifications.Add(notification);
        }

        public void ClearNotifications()
        {
            notifications.Clear();
        }
    }
}