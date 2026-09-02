using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SkidMenu.features;

public static class ChatEnhancements
{
    public static bool EnableChatHistory = false;
    public static bool EnableExtendedChat = false;
    public static bool EnableColorCommand = false;

    public static class History
    {
        public static List<string> Sent = new();
        public static int Index = -1;
        public static string Draft = "";
        public static bool Browsing = false;

        public static void Remember(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            if (Sent.Count == 0 || Sent[Sent.Count - 1] != msg) Sent.Add(msg);
            Index = Sent.Count;
            Browsing = false;
        }

        public static void Navigate(ChatController chat)
        {
            if (Sent.Count == 0 || chat.freeChatField?.textArea == null) return;
            if (!chat.freeChatField.textArea.hasFocus) return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (!Browsing) { Draft = chat.freeChatField.textArea.text; Browsing = true; }
                if (Index <= 0) return;
                Index = Mathf.Clamp(Index - 1, 0, Sent.Count - 1);
                chat.freeChatField.textArea.SetText(Sent[Index], string.Empty);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && Browsing)
            {
                Index++;
                if (Index < Sent.Count) chat.freeChatField.textArea.SetText(Sent[Index], string.Empty);
                else { chat.freeChatField.textArea.SetText(Draft, string.Empty); Browsing = false; }
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ChatHistory_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (EnableChatHistory) History.Navigate(__instance);
            if (EnableExtendedChat && __instance.freeChatField?.textArea != null)
                __instance.freeChatField.textArea.characterLimit = 120;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class SendChat_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ChatController __instance)
        {
            if (__instance.freeChatField?.textArea == null) return true;
            string text = __instance.freeChatField.textArea.text.Trim();

            if (EnableChatHistory) History.Remember(text);

            if (EnableColorCommand && text.StartsWith("/color "))
            {
                string colorStr = text.Substring(7).Trim();
                if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
                if (ColorUtility.TryParseHtmlString(colorStr, out Color c))
                {
                    ChatTheme.TextColor = c;
                    ChatTheme.TextHex = ColorUtility.ToHtmlStringRGB(c);
                    ChatTheme.CustomEnabled = true;
                    __instance.freeChatField.textArea.SetText(string.Empty, string.Empty);
                    return false;
                }
            }

            return true;
        }
    }
}
