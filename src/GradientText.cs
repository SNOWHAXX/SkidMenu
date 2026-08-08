using System.Text;
using UnityEngine;

namespace SkidMenu
{
    internal static class GradientText
    {
        private static readonly Color CyanLight = new Color(0.635f, 0.980f, 0.988f);
        private static readonly Color CyanDark  = new Color(0.388f, 0.800f, 0.812f);

        private static Color Lerp(Color a, Color b, float t)
        {
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t
            );
        }

        private static string ToHex(Color c)
        {
            int r = Mathf.Clamp((int)(c.r * 255), 0, 255);
            int g = Mathf.Clamp((int)(c.g * 255), 0, 255);
            int b = Mathf.Clamp((int)(c.b * 255), 0, 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        public static string Animate(string text, float timeOffset = 0f)
        {
            if (string.IsNullOrEmpty(text)) return text;

            float speed  = 1.2f;
            float spread = 0.5f;
            var sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                float wave = Mathf.Sin((Time.time * speed) + (i * spread) + timeOffset);
                float t    = (wave + 1f) * 0.5f;
                Color col  = Lerp(CyanDark, CyanLight, t);
                sb.Append($"<color=#{ToHex(col)}>{text[i]}</color>");
            }

            return sb.ToString();
        }
    }
}
