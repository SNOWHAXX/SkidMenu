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

        private const float SlideInDuration = 0.12f;
        private const float SlideOutDuration = 0.15f;

        public void Update()
        {
            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                Notification n = notifications[i];
                n.lifetime += Time.deltaTime;

                if (!n.dying && n.HasExpired)
                {
                    n.dying = true;
                    n.deathProgress = 0;
                }

                if (n.dying)
                {
                    n.deathProgress += Time.deltaTime / SlideOutDuration;
                    if (n.deathProgress >= 1f)
                        notifications.RemoveAt(i);
                }
                else
                {
                    n.slideProgress = Mathf.Clamp01(n.slideProgress + Time.deltaTime / SlideInDuration);
                }
            }

            NotificationLive.Tick(notifications);
        }

        public void OnGUI()
        {
            if (DisableNotifications) return;

            int count = Math.Min(GetMaxNotifications(), notifications.Count);

            for (byte i = 0; i < count; i++)
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
            clipping  = TextClipping.Clip,
            normal    = { textColor = new Color(0.60f, 1.00f, 0.99f, 1f) },
            padding   = new RectOffset { left = 10, right = 6, top = 6,  bottom = 0 }
        };

        private GUIStyle MsgStyle => _msgStyle ??= new GUIStyle
        {
            font      = GUIStylePreset.FontRegular,
            fontSize  = 12,
            richText  = true,
            wordWrap  = true,
            clipping  = TextClipping.Clip,
            normal    = { textColor = new Color(0.88f, 0.88f, 0.90f, 1f) },
            padding   = new RectOffset { left = 10, right = 6, top = 4, bottom = 0 }
        };

        private static Texture2D _roundedBg;
        private static Texture2D RoundedBg => _roundedBg ??= GUIStylePreset.MakeRoundedPanel(
            new Color(0.07f, 0.07f, 0.07f, 0.92f), 10, 0.55f, 0f, 0f);

        private static GUIStyle _roundedBgStyle;
        private GUIStyle RoundedBgStyle => _roundedBgStyle ??= new GUIStyle
        {
            normal = { background = RoundedBg },
            border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
            padding = new RectOffset(),
            margin = new RectOffset()
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

        private static GUIStyle _barBgStyle;
        private static GUIStyle _barFillStyle;

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private void RenderNotification(byte position, Notification notification)
        {
            float targetX = Screen.width  - boxSize.x - 8f;
            float targetY = Screen.height - boxSize.y * (position + 1) - 8f;

            float slideOffset;
            if (notification.dying)
            {
                slideOffset = EaseOutCubic(Mathf.Clamp01(notification.deathProgress)) * (boxSize.x + 16f);
            }
            else
            {
                slideOffset = (1f - EaseOutCubic(notification.slideProgress)) * (boxSize.x + 16f);
            }

            float alpha = notification.dying
                ? Mathf.Clamp01(1f - notification.deathProgress)
                : Mathf.Clamp01(notification.slideProgress);

            float boxX = targetX + slideOffset;
            float boxY = targetY;

            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUI.Box(new Rect(boxX, boxY, boxSize.x, boxSize.y), GUIContent.none, RoundedBgStyle);

            GUI.Label(new Rect(boxX, boxY,       boxSize.x, 24f),              notification.title,   TitleStyle);
            GUI.Label(new Rect(boxX, boxY + 22f, boxSize.x, boxSize.y - 36f), notification.message, MsgStyle);

            float progress = 1f - (notification.lifetime / notification.ttl);
            float barH = 4f;
            float barY = boxY + boxSize.y - barH;
            GUI.Box(new Rect(boxX + 10f, barY, boxSize.x - 20f, barH), GUIContent.none, BarBgStyle);
            GUI.Box(new Rect(boxX + 10f, barY, (boxSize.x - 20f) * progress, barH), GUIContent.none, BarFillStyle);

            GUI.color = prevColor;
        }

        public int GetMaxNotifications()
        {
            return (Screen.height / 2) / (int)boxSize.y;
        }

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
