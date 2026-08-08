using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features
{
	public static class Whisper
	{
		private static readonly List<PlayerControl> _targets = new List<PlayerControl>();
		public static bool PendingLog;

		public static IReadOnlyList<PlayerControl> Targets => _targets;
		public static int Count => _targets.Count;

		public static bool IsArmed(PlayerControl pc) => pc != null && _targets.Contains(pc);

		public static bool IsArmedById(byte playerId)
		{
			for (int i = 0; i < _targets.Count; i++)
				if (_targets[i] != null && _targets[i].PlayerId == playerId)
					return true;
			return false;
		}

		public static void Toggle(PlayerControl pc)
		{
			if (pc == null || pc == PlayerControl.LocalPlayer) return;
			if (_targets.Contains(pc)) _targets.Remove(pc);
			else _targets.Add(pc);
		}

		public static void Remove(PlayerControl pc)
		{
			if (pc != null) _targets.Remove(pc);
		}

		public static void Clear() => _targets.Clear();

		public static string Names()
		{
			var sb = new System.Text.StringBuilder();
			for (int i = 0; i < _targets.Count; i++)
			{
				if (i > 0) sb.Append(", ");
				var pc = _targets[i];
				sb.Append(pc != null && pc.Data != null ? pc.Data.PlayerName : "?");
			}
			return sb.ToString();
		}

		public static string RoleHexFor(NetworkedPlayerInfo data)
		{
			try
			{
				if (data == null) return "ffffff";
				return ColorUtility.ToHtmlStringRGB(Utils.GetCustomRoleColor(data));
			}
			catch { return "ffffff"; }
		}
	}

	[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
	public static class Whisper_RpcSendChat
	{
		public static bool Prefix(PlayerControl __instance, string chatText)
		{
			if (!__instance.AmOwner) return true;
			if (Whisper.Count == 0) return true;

			if (string.IsNullOrWhiteSpace(chatText)) return false;

			var ids = new List<int>(Whisper.Count);
			for (int i = Whisper.Targets.Count - 1; i >= 0; i--)
			{
				PlayerControl t = Whisper.Targets[i];
				if (t == null || t == PlayerControl.LocalPlayer)
				{
					Whisper.Remove(t);
					continue;
				}
				int clientId = AmongUsClient.Instance.GetClientIdFromCharacter(t);
				if (clientId < 0)
				{
					Whisper.Remove(t);
					continue;
				}
				ids.Add(clientId);
			}

			if (ids.Count == 0) return true;

			try
			{
				ChatBubble_WhisperName.SetPending();
				Whisper.PendingLog = true;

				if (HudManager.Instance != null && HudManager.Instance.Chat != null)
					HudManager.Instance.Chat.AddChat(__instance, chatText, false);
				Whisper.PendingLog = false;

				for (int i = 0; i < ids.Count; i++)
				{
					MessageWriter mw = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(
						__instance.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, ids[i]);
					mw.Write(chatText);
					((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(mw);
				}
			}
			catch (System.Exception ex)
			{
				Whisper.PendingLog = false;
				SkidMenu.Log.LogWarning($"Whisper send failed: {ex.Message}");
				return true;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
	public static class Whisper_HandleDisconnect
	{
		public static void Prefix(PlayerControl player, DisconnectReasons reason)
		{
			Whisper.Remove(player);
		}
	}

	[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.AlignChildren))]
	[HarmonyPriority(Priority.Last)]
	public static class ChatBubble_WhisperName
	{
		private static bool _pending;

		public static void SetPending() => _pending = true;

		public static void Postfix(ChatBubble __instance)
		{
			if (!_pending) return;
			if (__instance == null || __instance.NameText == null || __instance.playerInfo == null) return;
			if (__instance.playerInfo.Object != PlayerControl.LocalPlayer) return;

			_pending = false;

			try
			{
				if (Whisper.Count == 0) return;

				string roleHex = Whisper.RoleHexFor(Whisper.Targets[0].Data);
				string targetText;
				if (Whisper.Count == 1)
					targetText = Whisper.Targets[0].Data != null
						? Utils.GetNameTag(Whisper.Targets[0].Data, Whisper.Targets[0].Data.PlayerName, true)
						: "?";
				else
					targetText = Whisper.Names();

				__instance.NameText.text = $"<color=#{roleHex}>You -></color> {targetText}";

				float oldNameH = __instance.NameText.GetNotDumbRenderedHeight();
				__instance.NameText.ForceMeshUpdate(true, true);
				float newNameH = __instance.NameText.GetNotDumbRenderedHeight();
				float delta = newNameH - oldNameH;

				if (delta > 0.001f)
				{
					Vector3 p = __instance.TextArea.transform.localPosition;
					__instance.TextArea.transform.localPosition = new Vector3(p.x, p.y - delta, p.z);
				}

				__instance.Background.size = new Vector2(5.52f, 0.2f + newNameH + __instance.TextArea.GetNotDumbRenderedHeight());
				__instance.MaskArea.size = __instance.Background.size - new Vector2(0f, 0.03f);
			}
			catch { }
		}
	}
}
