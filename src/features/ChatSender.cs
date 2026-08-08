using HarmonyLib;
using UnityEngine;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;

namespace SkidMenu.features;

public static class ChatSender
{
    public static bool Enabled = false;
    public static string Message = "hello";
    public static float Delay = 2.5f;
    private static float _timer = 0f;

    public static bool OnJoinEnabled = false;
    public static string OnJoinMessage = "hi";

    public static bool OnDeathEnabled = false;
    public static string OnDeathMessage = "gg";

    public static bool OnMeetingEnabled = false;
    public static string OnMeetingMessage = "meeting time";

    public static bool OnKillEnabled = false;
    public static string OnKillMessage = "ez";

    public static bool OnEjectionEnabled = false;
    public static string OnEjectionMessage = "rip me";

    private static bool _wasDead = false;
    private static float _ejectedAt = -999f;
    private const float EjectionSuppressWindow = 5f;

    public static void SendChat(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        try
        {
            var hud = DestroyableSingleton<HudManager>.Instance;
            ChatController chat = hud?.Chat;
            if (chat?.freeChatField?.textArea == null) return;
            chat.freeChatField.textArea.SetText(message, string.Empty);
            chat.SendChat();
        }
        catch { }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class ChatSender_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;

            if (Enabled && !string.IsNullOrWhiteSpace(Message))
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= Delay)
                {
                    _timer = 0f;
                    SendChat(Message);
                }
            }

            bool isDead = __instance.Data != null && __instance.Data.IsDead;
            if (isDead && !_wasDead)
            {
                _wasDead = true;
                bool wasEjected = Time.time - _ejectedAt <= EjectionSuppressWindow;
                if (OnDeathEnabled && !wasEjected)
                    SendChat(OnDeathMessage);
            }
            else if (!isDead)
            {
                _wasDead = false;
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    public static class ChatSender_OnJoin
    {
        public static void Postfix()
        {
            if (!OnJoinEnabled) return;
            AmongUsClient.Instance.StartCoroutine(DelayedJoinMessage().WrapToIl2Cpp());
        }

        private static IEnumerator DelayedJoinMessage()
        {
            yield return new WaitForSeconds(1.5f);
            SendChat(OnJoinMessage);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class ChatSender_OnMeeting
    {
        public static void Postfix()
        {
            if (OnMeetingEnabled) SendChat(OnMeetingMessage);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class ChatSender_OnKill
    {
        public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
        {
            if (!OnKillEnabled || __instance != PlayerControl.LocalPlayer) return;
            if (!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;
            SendChat(OnKillMessage);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class ChatSender_OnEjection
    {
        public static void Postfix(NetworkedPlayerInfo exiled)
        {
            if (exiled == null || PlayerControl.LocalPlayer == null) return;
            if (exiled.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
            _ejectedAt = Time.time;
            if (OnEjectionEnabled) SendChat(OnEjectionMessage);
        }
    }
}
