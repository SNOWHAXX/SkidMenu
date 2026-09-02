using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SkidMenu;

public static class ChatTheme
{
    // Dark theme
    public static bool  DarkModeEnabled = true;
    public static float DarkBgAlpha     = 1f;

    // Custom alived theme
    public static bool   CustomEnabled = false;
    public static Color  BgColor       = new Color(0.133f, 0.133f, 0.133f, 1f);
    public static Color  TextColor     = Color.white;
    public static float  BgAlpha       = 1f;
    public static float  TextAlpha     = 1f;

    // Dead theme (separate custom theme for dead players)
    public static bool   DeadCustomEnabled = false;
    public static Color  DeadBgColor       = new Color(0.08f, 0.08f, 0.08f, 1f);
    public static Color  DeadTextColor     = new Color(0.67f, 0.67f, 0.67f, 1f);
    public static float  DeadBgAlpha       = 0.6f;
    public static float  DeadTextAlpha     = 0.6f;

    // UI hex strings
    public static string BgHex       = "222222";
    public static string TextHex     = "FFFFFF";
    public static string DeadBgHex   = "141414";
    public static string DeadTextHex = "AAAAAA";

    public static Color EffectiveBg(System.Nullable<bool> isDead)
    {
        bool dead = isDead == true;
        if (dead && DeadCustomEnabled) return WithAlpha(DeadBgColor, DeadBgAlpha);
        if (CustomEnabled) return WithAlpha(BgColor, BgAlpha);
        if (DarkModeEnabled) return WithAlpha(DarkBg, DarkBgAlpha);
        return new Color(1f, 1f, 1f, 1f);
    }

    public static Color EffectiveText(System.Nullable<bool> isDead)
    {
        bool dead = isDead == true;
        if (dead && DeadCustomEnabled) return WithAlpha(DeadTextColor, DeadTextAlpha);
        if (CustomEnabled) return WithAlpha(TextColor, TextAlpha);
        if (DarkModeEnabled) return WithAlpha(Color.white, DarkTextAlpha);
        return new Color(1f, 1f, 1f, 1f);
    }

    public static bool AffectsBubble(System.Nullable<bool> isDead)
    {
        bool dead = isDead == true;
        if (dead && DeadCustomEnabled) return true;
        if (CustomEnabled) return true;
        return DarkModeEnabled;
    }

    private static readonly Color DarkBg       = new Color(0.15f, 0.15f, 0.15f, 1f);
    private const   float DarkTextAlpha = 1f;

    private static Color WithAlpha(Color c, float alpha) => new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));

    [HarmonyPatch(typeof(ChatBubble), "SetText")]
    public static class ChatTheme_ChatBubblePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ChatBubble __instance, ref string chatText)
        {
            try
            {
                bool? isDead = null;
                if (__instance.playerInfo != null && __instance.playerInfo.IsDead) isDead = true;
                else if (__instance.playerInfo != null) isDead = false;

                if (!AffectsBubble(isDead)) return;

                Transform bg = ((Component)__instance).transform.Find("Background");
                SpriteRenderer sr = bg != null ? ((Component)bg).GetComponent<SpriteRenderer>() : null;
                if (sr != null) sr.color = EffectiveBg(isDead);

                if (chatText.Contains("░") || chatText.Contains("▄") ||
                    chatText.Contains("█") || chatText.Contains("▌") ||
                    chatText.Contains("▒")) return;

                string hex = ColorUtility.ToHtmlStringRGBA(EffectiveText(isDead));
                chatText = $"<color=#{hex}>{chatText.TrimEnd(default(char))}</color>";
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ChatController), "Update")]
    public static class ChatTheme_ChatControllerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (!CustomEnabled && !DarkModeEnabled) return;
            try
            {
                Color bg = CustomEnabled ? WithAlpha(BgColor, BgAlpha) : WithAlpha(DarkBg, DarkBgAlpha);
                Color text = CustomEnabled ? WithAlpha(TextColor, TextAlpha) : WithAlpha(Color.white, DarkTextAlpha);

                FreeChatInputField free = __instance.freeChatField;
                if (free != null)
                {
                    var freeBg = ((AbstractChatInputField)free).background;
                    if (freeBg != null) freeBg.color = bg;

                    if (free.textArea?.outputText != null)
                        free.textArea.outputText.color = text;
                }

                var quickField = __instance.quickChatField;
                if (quickField != null)
                {
                    var quickBg = ((AbstractChatInputField)quickField).background;
                    if (quickBg != null) quickBg.color = bg;

                    var txt = ((Component)quickField).GetComponentInChildren<TMPro.TextMeshPro>();
                    if (txt != null) txt.color = text;
                }
            }
            catch { }
        }
    }
}