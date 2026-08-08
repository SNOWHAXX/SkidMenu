using HarmonyLib;
using UnityEngine;

namespace SkidMenu;

public static class CustomGameTheme
{
    public static bool Enabled = false;
    public static Color BgColor = new Color(0.133f, 0.133f, 0.133f, 1f);
    public static Color TextColor = Color.white;

    [HarmonyPatch(typeof(ChatBubble), "SetText")]
    public static class CustomGameTheme_ChatBubblePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ChatBubble __instance, ref string chatText)
        {
            if (!Enabled) return;
            try
            {
                Transform bg = ((Component)__instance).transform.Find("Background");
                SpriteRenderer sr = bg != null ? ((Component)bg).GetComponent<SpriteRenderer>() : null;
                if (sr != null) sr.color = BgColor;

                if (!chatText.Contains("░") && !chatText.Contains("▄") &&
                    !chatText.Contains("█") && !chatText.Contains("▌") &&
                    !chatText.Contains("▒"))
                {
                    string hex = ColorUtility.ToHtmlStringRGB(TextColor);
                    chatText = $"<color=#{hex}>{chatText.TrimEnd(default(char))}</color>";
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ChatController), "Update")]
    public static class CustomGameTheme_ChatControllerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (!Enabled) return;
            try
            {
                FreeChatInputField free = __instance.freeChatField;
                if (free != null)
                {
                    AbstractChatInputField freeBase = (AbstractChatInputField)free;
                    if (freeBase.background != null) freeBase.background.color = BgColor;
                    if (free.textArea?.outputText != null) free.textArea.outputText.color = TextColor;
                }

                AbstractChatInputField quick = (AbstractChatInputField)__instance.quickChatField;
                if (quick != null)
                {
                    if (quick.background != null) quick.background.color = BgColor;
                    var txt = ((Component)quick).GetComponentInChildren<TMPro.TextMeshPro>();
                    if (txt != null) txt.color = TextColor;
                }
            }
            catch { }
        }
    }
}
