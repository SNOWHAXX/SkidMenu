using HarmonyLib;
using AmongUs.Data.Player;

namespace SkidMenu.features
{
	internal class Self
	{
		// When PlayerControl::RpcPlayAnimation or PlayerControl::RpcSetScanner is called, they check if visual tasks are on before sending the RPC
		// If we want to be able to send those RPCs even with visual tasks are off, then we will need to reimplement those functions
		// We could just patch LogicOptionsNormal::GetVisualTasks and LogicOptionsHnS::GetVisualTasks, however the latter is inlined so our patch won't actually get applied
		// meaning this will only show task animations on normal games and not hide and seek aswell
		public static bool AlwaysShowTaskAnimations { get; set; } = true;

		/*
		[HarmonyPatch(typeof(DataManager), nameof(DataManager.Player.Ban.IsBanned), MethodType.Getter)]
		public static class BypassIntentionalDisconnectionBlocks
		{
			public static bool Enabled { get; set; } = true;

			static void Postfix(ref bool __result)
			{
				if(Enabled) __result = false;
			}
		}
		*/

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
		class AlwaysDoScanAnimation
		{
			static bool Prefix(PlayerControl __instance, bool value)
			{
				if(__instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				if(AlwaysShowTaskAnimations)
				{
					Network.SendSetScanner(value);
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcPlayAnimation))]
		class AlwaysDoTaskAnimaton
		{
			static bool Prefix(PlayerControl __instance, byte animType)
			{
				if(__instance.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				if(AlwaysShowTaskAnimations)
				{
					Network.SendPlayAnimation(animType);
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerStatsData), nameof(PlayerStatsData.ValidateStat))]
		public static class UpdateStatsFreeplay
		{
			public static bool Enabled { get; set; } = false;

			static void Prefix(PlayerStatsData __instance)
			{
				if(Enabled)
				{
					__instance.isTrackingStats = true;
				}
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
		public static class CurrentSpeedChanger
		{
			public static bool Enabled { get; set; } = false;
			public static float Speed { get; set; } = 2.5f;

			static void Postfix(ref float __result)
			{
				if (Enabled) __result = __result < 0f ? -Speed : Speed;
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
		public static class PlayerSpeedModifier
		{
			public static bool Enabled { get; set; } = false;
			public static float Multiplier { get; set; } = 1.0f;

			static void Postfix(ref float __result)
			{
				if(Enabled) __result *= Multiplier;
			}
		}

		[HarmonyPatch]
		public static class VoteAnywhere_CanHighlight
		{
			static System.Reflection.MethodBase TargetMethod() =>
				AccessTools.Method(typeof(PlayerVoteArea), "CanBeHighlighted");

			static void Postfix(ref bool __result)
			{
				if (VoteAnywhere.InstantVote && VoteAnywhere.VoteAnyone)
					__result = true;
			}
		}

		[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
		public static class VoteAnywhere_VoteAreaSelect
		{
			static bool Prefix(PlayerVoteArea __instance)
			{
				if (!VoteAnywhere.InstantVote || !VoteAnywhere.VoteAnyone) return true;
				if (!__instance.AmDead) return true;

				var meeting = __instance.Parent;
				if (meeting == null) return false;

				int idx = -1;
				for (int i = 0; i < meeting.playerStates.Length; i++)
				{
					var ps = meeting.playerStates[i];
					if (ps != null && ps.PlayerId == __instance.PlayerId) { idx = i; break; }
				}

				if (idx >= 0) meeting.Select(idx);
				return false;
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
		public static class VoteAnywhere
		{
			public static bool InstantVote { get; set; } = false;
			public static bool VoteAnyone { get; set; } = false;
			public static bool VoteBeforeVotingStarts { get; set; } = false;

			static bool Prefix(MeetingHud __instance, int suspectStateIdx)
			{
				if (!InstantVote) return true;

				// suspectStateIdx is TargetPlayerId, NOT an array index —
				// PlayerVoteArea.Select() passes this.TargetPlayerId up to MeetingHud.Select
				byte targetId = (byte)suspectStateIdx;

				PlayerVoteArea voteArea = null;
				foreach (var area in __instance.playerStates)
				{
					if (area != null && area.PlayerId == targetId) { voteArea = area; break; }
				}
				if (voteArea == null) return true;

				PlayerControl targetPc = null;
				foreach (var pc in PlayerControl.AllPlayerControls)
				{
					if (pc != null && pc.PlayerId == targetId) { targetPc = pc; break; }
				}

				bool targetIsDead = targetPc?.Data?.IsDead ?? false;

				bool duringDiscussion = __instance.CurrentState == MeetingHud.MeetingStates.Discussion;
				bool duringVoting     = __instance.CurrentState == MeetingHud.MeetingStates.NotVoted
				                     || __instance.CurrentState == MeetingHud.MeetingStates.Voted;

				if (duringDiscussion && !VoteBeforeVotingStarts) return true;
				if (duringVoting && !VoteAnyone && targetIsDead) return true;
				if (duringDiscussion && !VoteAnyone && targetIsDead) return true;

				__instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, targetId);

				return false;
			}
		}
		[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
		public static class NoLadderCooldown
		{
			public static bool Enabled { get; set; } = true;
			static void Postfix(Ladder __instance)
			{
				if(Enabled)
				{
					SkidMenu.Log.LogMessage($"Used ladder");
					__instance.CoolDown = 0.0f;
					__instance.Destination.CoolDown = 0.0f;
				}
			}
		}

		[HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
		public static class UnlimitedMeetings
		{
			public static bool enabled = false;

			static void Prefix()
			{
				if(enabled) PlayerControl.LocalPlayer.RemainingEmergencies = 999999;
			}
		}
		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
		public static class ShowDeadVoteAreas
		{
			static void Postfix(MeetingHud __instance)
			{
				if (!VoteAnywhere.VoteAnyone) return;
				if (__instance.playerStates == null) return;
				foreach (var area in __instance.playerStates)
				{
					if (area == null) continue;
					var pc = GameData.Instance?.GetPlayerById(area.PlayerId)?.Object;
					if (pc == null || pc.Data == null || !pc.Data.IsDead) continue;
					if (!area.gameObject.activeSelf) area.gameObject.SetActive(true);
					try { area.SetEnabled(); } catch { }
				}
			}
		}
	}
}



