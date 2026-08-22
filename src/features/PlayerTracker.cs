using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using InnerNet;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SkidMenu;

public static class PlayerTracker
{
    public static readonly Dictionary<byte, List<string>> History = new();
    public static readonly Dictionary<byte, string> LastRoom = new();

    public static void Log(byte playerId, string entry)
    {
        if (!History.TryGetValue(playerId, out var list))
            History[playerId] = list = new List<string>();
        var ts = System.DateTime.Now.ToString("HH:mm:ss");
        list.Add($"<color=#888888>[{ts}]</color> {entry}");
        if (list.Count > 60) list.RemoveAt(0);
    }

    public static void Clear()
    {
        History.Clear();
        LastRoom.Clear();
        TrackVote.ClearTracked();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    static class TrackRoom
    {
        static void Postfix(PlayerControl __instance)
        {
            if (__instance?.Data == null || ShipStatus.Instance == null) return;
            try
            {
                var plain = Utils.GetRoomFromPosition(__instance.GetTruePosition());
                if (plain != null) LastRoom[__instance.PlayerId] = plain.RoomId.ToString();
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    static class ClearOnLobby { static void Postfix() => Clear(); }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    static class TrackKill
    {
        static void Postfix(PlayerControl __instance, PlayerControl target)
        {
            if (__instance?.Data == null || target?.Data == null) return;
            var room = Utils.GetRoomFromPosition(target.GetTruePosition());
            string roomStr = room != null ? $" <color=#555555>in {room.RoomId}</color>" : "";
            string victim = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[target.Data.DefaultOutfit.ColorId])}>{target.Data.PlayerName}</color>";
            Log(__instance.PlayerId, $"<color=#ff4444>☠ Killed {victim}{roomStr}</color>");
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    static class TrackVentIn
    {
        static void Postfix(PlayerControl pc)
        {
            if (pc?.Data == null) return;
            var room = Utils.GetRoomFromPosition(pc.GetTruePosition());
            string roomStr = room != null ? $" <color=#555555>({room.RoomId})</color>" : "";
            Log(pc.PlayerId, $"<color=#ffff00>▼ Entered vent{roomStr}</color>");
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    static class TrackVentOut
    {
        static void Postfix(PlayerControl pc)
        {
            if (pc?.Data == null) return;
            var room = Utils.GetRoomFromPosition(pc.GetTruePosition());
            string roomStr = room != null ? $" <color=#555555>({room.RoomId})</color>" : "";
            Log(pc.PlayerId, $"<color=#ffff88>▲ Exited vent{roomStr}</color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    static class TrackShapeshift
    {
        static void Postfix(PlayerControl __instance, PlayerControl targetPlayer)
        {
            if (__instance?.Data == null || targetPlayer?.Data == null) return;
            if (targetPlayer.PlayerId == __instance.PlayerId)
            {
                Log(__instance.PlayerId, "<color=#FF8C00>◈ Reverted shift</color>");
                return;
            }
            string c = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[targetPlayer.Data.DefaultOutfit.ColorId]);
            Log(__instance.PlayerId, $"<color=#FF8C00>◈ Shifted into <color=#{c}>{targetPlayer.Data.PlayerName}</color></color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    static class TrackReport
    {
        static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
        {
            if (__instance?.Data == null) return;
            if (target == null)
                Log(__instance.PlayerId, "<color=#00bfff>📢 Called emergency meeting</color>");
            else
            {
                string c = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[target.DefaultOutfit.ColorId]);
                Log(__instance.PlayerId, $"<color=#ff6666>💀 Reported <color=#{c}>{target.PlayerName}</color></color>");
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    static class TrackVote
    {
        private static readonly HashSet<byte> _trackedVotes = new HashSet<byte>();

        internal static void ClearTracked() => _trackedVotes.Clear();

        static void Postfix(MeetingHud __instance)
        {
            try
            {
                if (__instance.CurrentState >= MeetingHud.MeetingStates.Results) { _trackedVotes.Clear(); return; }
                foreach (var area in __instance.playerStates)
                {
                    if (area == null) continue;
                    if (area.VotedForId == PlayerVoteArea.HasNotVoted) continue;
                    if (area.VotedForId == PlayerVoteArea.MissedVote) continue;
                    if (area.VotedForId == PlayerVoteArea.DeadVote) continue;
                    if (_trackedVotes.Contains(area.PlayerId)) continue;
                    _trackedVotes.Add(area.PlayerId);

                    var voter = GameData.Instance?.GetPlayerById(area.PlayerId)?.Object;
                    if (voter?.Data == null) continue;
                    byte suspectPlayerId = (byte)area.VotedForId;
                    if (suspectPlayerId == 255) { Log(voter.PlayerId, "<color=#aaaaaa>◌ Voted skip</color>"); continue; }
                    var suspect = GameData.Instance?.GetPlayerById(suspectPlayerId)?.Object;
                    string c = suspect?.Data != null ? ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[suspect.Data.DefaultOutfit.ColorId]) : "ffffff";
                    Log(voter.PlayerId, $"<color=#ff9900>◉ Voted <color=#{c}>{suspect?.Data?.PlayerName ?? "?"}</color></color>");
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    static class TrackVotekick
    {
        static void Postfix(int srcClient, int clientId)
        {
            PlayerControl voter = null, target = null;
            foreach (var p in PlayerControl.AllPlayerControls) {
                if (p?.OwnerId == srcClient) voter = p;
                if (p?.OwnerId == clientId) target = p;
            }
            if (voter?.Data == null) return;
            string tc = target?.Data != null ? ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[target.Data.DefaultOutfit.ColorId]) : "ffffff";
            Log(voter.PlayerId, $"<color=#ff6600>⚡ Votekicked <color=#{tc}>{target?.Data?.PlayerName ?? "?"}</color></color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    static class TrackTask
    {
        static void Postfix(PlayerControl __instance, uint idx)
        {
            if (__instance?.Data == null || __instance.AmOwner) return;
            try {
                int done = 0, total = 0;
                foreach (var t in __instance.myTasks) { total++; if (t.IsComplete) done++; }
                Log(__instance.PlayerId, $"<color=#88ff88>✓ Task complete <color=#555555>({done}/{total})</color></color>");
            } catch { Log(__instance.PlayerId, "<color=#88ff88>✓ Task complete</color>"); }
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    static class TrackDisconnect
    {
        static void Prefix(PlayerControl player, DisconnectReasons reason)
        {
            if (player?.Data == null || player.AmOwner) return;
            Log(player.PlayerId, $"<color=#888888>✕ Disconnected ({reason})</color>");
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    static class TrackChat
    {
        static void Postfix(PlayerControl sourcePlayer, string chatText)
        {
            if (sourcePlayer?.Data == null || sourcePlayer.AmOwner) return;
            string preview = chatText.Length > 35 ? chatText[..35] + "…" : chatText;
            Log(sourcePlayer.PlayerId, $"<color=#aaddff>💬 \"{preview}\"</color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    static class TrackKillAttempt
    {
        static void Postfix(PlayerControl __instance, PlayerControl target)
        {
            if (__instance?.Data == null || target?.Data == null) return;
            if (target.Data.IsDead) return;
            string c = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[target.Data.DefaultOutfit.ColorId]);
            Log(__instance.PlayerId, $"<color=#ff7744>⚔ Kill attempt on <color=#{c}>{target.Data.PlayerName}</color> (blocked)</color>");
        }
    }

    [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.UseAbility))]
    static class TrackPhantom
    {
        private static bool _wasInvisible;
        static void Prefix(PhantomRole __instance) => _wasInvisible = __instance?.isInvisible ?? false;
        static void Postfix(PhantomRole __instance)
        {
            if (__instance?.Player?.Data == null || __instance.Player.AmOwner) return;
            if (!_wasInvisible && __instance.isInvisible)
                Log(__instance.Player.PlayerId, "<color=#8B0000>👻 Vanished</color>");
            else if (_wasInvisible && !__instance.isInvisible)
                Log(__instance.Player.PlayerId, "<color=#cc88ff>👻 Reappeared</color>");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    static class TrackVerdict
    {
        static void Postfix(NetworkedPlayerInfo exiled, bool wasOverruled, int overruleNonce)
        {
            if (!wasOverruled || exiled == null) return;
            try
            {
                var judge = features.JudgeCheats.GetAttributedJudge(exiled.PlayerId);
                string vc = exiled.Object?.Data != null
                    ? ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[exiled.Object.Data.DefaultOutfit.ColorId])
                    : "ffffff";
                string nonceStr = $" <color=#888888>(nonce {overruleNonce})</color>";

                if (judge?.Data != null)
                    Log(judge.PlayerId, $"<color=#5599ff>🔨 Gavelled <color=#{vc}>{exiled.Object?.Data?.PlayerName ?? exiled.PlayerName}</color>{nonceStr}</color>");

                if (exiled.Object != null)
                {
                    string jc = judge?.Data != null
                        ? ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[judge.Data.DefaultOutfit.ColorId])
                        : "aaaaaa";
                    string jname = judge == null
                        ? "<color=#aaaaaa>unknown</color>"
                        : judge.AmOwner ? "<color=#00ff88>You</color>" : $"<color=#{jc}>{judge.Data.PlayerName}</color>";
                    Log(exiled.PlayerId, $"<color=#5599ff>🔨 Ejected by gavel ({jname}){nonceStr}</color>");
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    static class TrackExiled
    {
        static void Postfix(PlayerControl __instance)
        {
            if (__instance?.Data == null) return;
            string c = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[__instance.Data.DefaultOutfit.ColorId]);
            Log(__instance.PlayerId, $"<color=#ff8888>⚖ Was exiled <color=#{c}>{__instance.Data.PlayerName}</color></color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    static class TrackDeath
    {
        static void Postfix(PlayerControl __instance, DeathReason reason)
        {
            if (__instance?.Data == null || reason == DeathReason.Kill) return;
            Log(__instance.PlayerId, $"<color=#ff6666>✖ Died ({reason})</color>");
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    static class TrackRole
    {
        static void Postfix(PlayerControl targetPlayer, RoleTypes roleType)
        {
            if (targetPlayer?.Data == null) return;
            string c = ColorUtility.ToHtmlStringRGB(Utils.GetCustomRoleColor(targetPlayer.Data));
            Log(targetPlayer.PlayerId, $"<color=#cc88ff>★ Role: <color=#{c}>{roleType}</color></color>");
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    static class TrackJoin
    {
        static void Postfix(PlayerControl __instance)
        {
            if (__instance == null || __instance.AmOwner || __instance.Data == null) return;
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;
            try { __instance.StartCoroutine(LogJoin(__instance).WrapToIl2Cpp()); } catch { }
        }

        private static System.Collections.IEnumerator LogJoin(PlayerControl player)
        {
            yield return new WaitForSeconds(0.6f);
            if (player == null || player.Data == null) yield break;
            try
            {
                var client = AmongUsClient.Instance.GetClientFromCharacter(player);
                if (client == null) yield break;
                string platform = Utils.PlatformTypeToString(client.PlatformData?.Platform ?? Platforms.Unknown);
                string c = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.Data.DefaultOutfit.ColorId]);
                Log(player.PlayerId, $"<color=#44ddff>→ Joined <color=#{c}>{client.PlayerName}</color> <color=#888888>({platform} · Lv{player.Data.PlayerLevel + 1})</color></color>");
            }
            catch { }
        }
    }
}
