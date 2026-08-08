using HarmonyLib;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace SkidMenu;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat
{
	// Prefix patch of ChatController.AddChat to receive ghost messages if CheatSettings.seeGhosts is enabled even if LocalPlayer is alive
	// Basically does what the original method did with the required modifications
	public static bool Prefix(PlayerControl sourcePlayer, string chatText, bool censor, ChatController __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return true;
        if (!CheatToggles.seeGhosts || PlayerControl.LocalPlayer.Data.IsDead) return true;
        if (!sourcePlayer || sourcePlayer.Data == null) return true;
        if (__instance == null || __instance.scroller == null || __instance.scroller.Inner == null) return true;

		NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
		NetworkedPlayerInfo data2 = sourcePlayer.Data; // Remove isDead check for LocalPlayer

		ChatBubble pooledBubble = __instance.GetPooledBubble();

		try
		{
			pooledBubble.transform.SetParent(__instance.scroller.Inner);
			pooledBubble.transform.localScale = Vector3.one;
			bool flag = sourcePlayer == PlayerControl.LocalPlayer;
			if (flag)
			{
				pooledBubble.SetRight();
			}
			else
			{
				pooledBubble.SetLeft();
			}
			bool didVote = false;
			try { didVote = MeetingHud.Instance != null && MeetingHud.Instance.playerStates != null && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId); } catch { }
			pooledBubble.SetCosmetics(data2);
			__instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
			if (censor && AmongUs.Data.DataManager.Settings.Multiplayer.CensorChat)
			{
				chatText = BlockedWords.CensorWords(chatText, false);
			}
			pooledBubble.SetText(chatText);
			pooledBubble.AlignChildren();
			__instance.AlignAllBubbles();
			if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
			{
				__instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
			}
			if (!flag && !__instance.IsOpenOrOpening)
			{
				SoundManager.Instance.PlaySound(__instance.messageSound, false).pitch = 0.5f + sourcePlayer.PlayerId / 15f;
				__instance.chatNotification.SetUp(sourcePlayer, chatText);
			}
		}
		catch (Exception message)
		{
			ChatController.Logger.Error(message.ToString(), null);
			__instance.chatBubblePool.Reclaim(pooledBubble);
		}

        return false; // Skips the original method completly
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatController_Update
{
    // Postfix patch of ChatController.Update to unlock longer message length and handle copy-on-double-click
    public static void Postfix(ChatController __instance)
    {
        ChatBubbleCopyHandler.Check(__instance);
        //__instance.freeChatField.textArea.allowAllCharacters = CheatToggles.chatJailbreak;
        //__instance.freeChatField.textArea.allowAllCharacters = CheatToggles.chatJailbreak; // Not really used by the game's code, but I include it anyway
        //__instance.freeChatField.textArea.AllowSymbols = true; // Allow sending certain symbols
        //__instance.freeChatField.textArea.AllowEmail = CheatToggles.chatJailbreak; // Allow sending email addresses when chatJailbreak is enabled
        //__instance.freeChatField.textArea.AllowPaste = CheatToggles.chatJailbreak; // Allow pasting from clipboard in chat when chatJailbreak is enabled

        if (CheatToggles.longerMessages)
		{
			try { __instance.freeChatField.textArea.characterLimit = 120; } catch { }
        }
		else
		{
			try { __instance.freeChatField.textArea.characterLimit = 100; } catch { }
        }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatController_SendChat
{
    // Postfix patch of ChatController.SendChat to unlock lower chat rate limits
    public static void Postfix(ChatController __instance)
    {
        if (!CheatToggles.lowerRateLimits) return;

		if (__instance.timeSinceLastMessage == 0f)
		{
			// Decreasing rate limit by 1 sec max still avoids anticheat kicks
			__instance.timeSinceLastMessage += 1f;
		}
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendFreeChat))]
public static class ChatController_SendFreeChat
{
    // Prefix patch of ChatController.SendFreeChat to allow sending URLs without being censored
    public static bool Prefix(ChatController __instance)
    {
		// Only works if CheatSettings.bypassUrlBlock is enabled
        if (!CheatToggles.bypassUrlBlock) return true;

        string text = __instance.freeChatField.Text;

        // Replace periods in URLs and email addresses with commas to avoid censorship
        string modifiedText = CensorUrlsAndEmails(text);

        ChatController.Logger.Debug("SendFreeChat () :: Sending message: '" + modifiedText + "'", null);
        PlayerControl.LocalPlayer.RpcSendChat(modifiedText);

        return false;
    }

    private static string CensorUrlsAndEmails(string text)
    {
        // Regular expression pattern to match URLs and email addresses
        string pattern = @"(http[s]?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,6}(/[\w-./?%&=]*)?|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)";
        Regex regex = new Regex(pattern);

        // Censor periods in each match
        return regex.Replace(text, match =>
        {
            var censored = match.Value;
            censored = censored.Replace('.', ',');
            return censored;
        });
    }
}

public static class ChatBubbleCopyHandler
{
    private const float DoubleClickWindow = 0.33f;
    private const float CopyDedupWindow   = 1.5f;

    private static float  _lastClickAt   = -10f;
    private static string _lastClickKey  = string.Empty;
    private static float  _lastCopyAt    = -10f;
    private static string _lastCopiedKey = string.Empty;

    public static void Check(ChatController chat)
    {
        if (!CheatToggles.copyMessage || chat == null) return;
        if (!UnityEngine.Input.GetMouseButtonDown(0)) return;
        try
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            UnityEngine.Vector2 mouse = UnityEngine.Input.mousePosition;
            var bubbles = ((UnityEngine.Component)chat).GetComponentsInChildren<ChatBubble>(false);
            for (int i = bubbles.Length - 1; i >= 0; i--)
            {
                var bubble = bubbles[i];
                if (bubble == null) continue;
                if (HitsBubble(bubble, cam, mouse))
                {
                    string msg = ReadBubbleText(bubble);
                    if (TryCopy(msg)) return;
                }
            }
        }
        catch { }
    }

    private static bool TryCopy(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        float now = UnityEngine.Time.unscaledTime;
        string key = "msg:" + text;
        bool isDouble = key == _lastClickKey && now - _lastClickAt <= DoubleClickWindow;
        _lastClickAt  = now;
        _lastClickKey = key;
        if (!isDouble) return true;
        if (key == _lastCopiedKey && now - _lastCopyAt < CopyDedupWindow) return true;
        GUIUtility.systemCopyBuffer = text;
        _lastCopyAt    = now;
        _lastCopiedKey = key;
        _lastClickAt   = -10f;
        SkidMenu.notifications.Send("<color=#66CCFF>Chat</color>", "Message copied!", 2f);
        return true;
    }

    private static bool HitsBubble(ChatBubble bubble, UnityEngine.Camera cam, UnityEngine.Vector2 mouse)
    {
        try
        {
            var bg = ((UnityEngine.Component)bubble).transform.Find("Background");
            if (bg == null) return false;
            var sr = ((UnityEngine.Component)bg).GetComponent<UnityEngine.SpriteRenderer>();
            if (sr == null) return false;
            var bounds = sr.bounds;
            if (bounds.size.sqrMagnitude < 0.001f) return false;
            var smin = cam.WorldToScreenPoint(bounds.min);
            var smax = cam.WorldToScreenPoint(bounds.max);
            var rect = new UnityEngine.Rect(
                UnityEngine.Mathf.Min(smin.x, smax.x), UnityEngine.Mathf.Min(smin.y, smax.y),
                UnityEngine.Mathf.Abs(smax.x - smin.x), UnityEngine.Mathf.Abs(smax.y - smin.y));
            return rect.Contains(mouse);
        }
        catch { return false; }
    }

    private static string ReadBubbleText(ChatBubble bubble)
    {
        try { return bubble.TextArea == null ? string.Empty : (((TMPro.TMP_Text)bubble.TextArea).text ?? string.Empty).Trim(); }
        catch { return string.Empty; }
    }
}
