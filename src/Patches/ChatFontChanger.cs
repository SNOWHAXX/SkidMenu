using HarmonyLib;

namespace SkidMenu;

public static class ChatFontChanger
{
    public static bool Enabled = false;
    public static int FontType = 0;

    public static readonly string[] FontNames =
    {
        "Barlow-Italic SDF", "Barlow-Medium SDF", "Barlow-Bold SDF",
        "Barlow-SemiBold SDF", "Barlow-SemiBold Masked", "Barlow-ExtraBold SDF",
        "Barlow-BoldItalic SDF", "Barlow-BoldItalic Masked", "Barlow-Black SDF",
        "Barlow-Light SDF", "Barlow-Regular SDF", "Barlow-Regular Masked",
        "Barlow-Regular Outline", "Brook SDF", "LiberationSans SDF",
        "NotoSansJP-Regular SDF", "VCR SDF", "CONSOLA SDF",
        "digital-7 SDF", "OCRAEXT SDF", "DIN_Pro_Bold_700 SDF"
    };

    [HarmonyPatch(typeof(ChatBubble), "SetText")]
    public static class ChatFontChanger_ChatBubblePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ChatBubble __instance, ref string chatText)
        {
            if (!Enabled) return;
            int idx = System.Math.Clamp(FontType, 0, FontNames.Length - 1);
            chatText = $"<font=\"{FontNames[idx]}\">{chatText}</font>";
        }
    }
}
