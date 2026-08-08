using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SkidMenu;

public static class DarkMode
{
    public static bool Enabled = true;

    private static readonly Color DarkBg   = new Color(40f / 255f, 40f / 255f, 40f / 255f, 1f);
    private static readonly Color BubbleBg = new Color(0.15f, 0.15f, 0.15f, 1f);

    [HarmonyPatch(typeof(ChatBubble), "SetText")]
    public static class DarkMode_ChatBubblePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ChatBubble __instance, ref string chatText)
        {
            if (!Enabled || CustomGameTheme.Enabled) return;
            try
            {
                Transform bg = ((Component)__instance).transform.Find("Background");
                SpriteRenderer sr = bg != null ? ((Component)bg).GetComponent<SpriteRenderer>() : null;
                if (sr != null)
                    sr.color = BubbleBg;

                if (!chatText.Contains("░") && !chatText.Contains("▄") &&
                    !chatText.Contains("█") && !chatText.Contains("▌") &&
                    !chatText.Contains("▒"))
                {
                    chatText = "<color=#FFFFFF>" + chatText.TrimEnd(default(char)) + "</color>";
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ChatController), "Update")]
    public static class DarkMode_ChatControllerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (!Enabled || CustomGameTheme.Enabled) return;
            try
            {
                FreeChatInputField free = __instance.freeChatField;
                if (free != null)
                {
                    var freeBg = ((AbstractChatInputField)free).background;
                    if (freeBg != null) freeBg.color = DarkBg;

                    if (free.textArea?.outputText != null)
                        free.textArea.outputText.color = Color.white;
                }

                var quickField = __instance.quickChatField;
                if (quickField != null)
                {
                    var quickBg = ((AbstractChatInputField)quickField).background;
                    if (quickBg != null) quickBg.color = DarkBg;

                    var txt = ((Component)quickField).GetComponentInChildren<TMPro.TextMeshPro>();
                    if (txt != null) txt.color = Color.white;
                }
            }
            catch { }
        }
    }
}
