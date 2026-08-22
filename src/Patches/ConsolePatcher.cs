using System;
using HarmonyLib;
using UnityEngine;
using Hazel;
using InnerNet;
using AmongUs.GameOptions;

namespace SkidMenu;

internal static class ConsoleHelper
{
    public static string Hex(PlayerControl pc)
    {
        try { return ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId]); }
        catch { return "ffffff"; }
    }
    public static string Hex(NetworkedPlayerInfo info)
    {
        try { return ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[info.DefaultOutfit.ColorId]); }
        catch { return "ffffff"; }
    }
    public static string Fmt(PlayerControl pc)      => $"<color=#{Hex(pc)}>{pc?.Data?.PlayerName ?? "?"}</color>";
    public static string Fmt(NetworkedPlayerInfo i) => $"<color=#{Hex(i)}>{i?.PlayerName ?? "?"}</color>";
    public static string Room(PlayerControl pc)
    {
        try { var r = Utils.GetRoomFromPosition(pc.GetTruePosition()); return r != null ? $" <color=#888888>[{r.RoomId}]</color>" : ""; }
        catch { return ""; }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class Console_LogKill
{
    private static float _lastTime;
    private static (byte killer, byte victim) _last;
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.logDeaths || __instance == null || target == null) return;
        if (target.Data == null || !target.Data.IsDead) return;
        var key = (__instance.PlayerId, target.PlayerId);
        if (key == _last && UnityEngine.Time.time - _lastTime < 0.5f) return;
        _last = key; _lastTime = UnityEngine.Time.time;
        try
        {
            bool viperKill = ViperBodies.IsViper(target.PlayerId);
            var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
            string killer = __instance.AmOwner
                ? "<color=#00ff88>You</color>"
                : isDisguised
                    ? $"{realKillerName} (as {displayKillerName})"
                    : realKillerName;
            string targetFmt = ConsoleHelper.Fmt(target);
            string room = ConsoleHelper.Room(target);
            string distStr = "";
            if (SkidMenu.logShowDistance && PlayerControl.LocalPlayer != null)
            {
                float d = Utils.GetDistanceBetween(PlayerControl.LocalPlayer, target);
                string dCol = d < 3f ? "FF3333" : d < 8f ? "FFAA00" : "33FF88";
                distStr = $" <color=#{dCol}>({d:F1}u)</color>";
            }
            string line = $"{killer} killed {targetFmt}{room}{distStr}";
            if (viperKill)
                ConsoleUI.LogLiveKill(line, target.PlayerId, "FF4466");
            else
                ConsoleUI.Log(line, "FF4466");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(ViperRole), nameof(ViperRole.KillAnimSpecialSetup))]
public static class ViperKillSeed
{
    public static void Postfix(DeadBody deadBody, PlayerControl killer, PlayerControl victim)
    {
        if (victim == null || victim.PlayerId == byte.MaxValue) return;
        try
        {
            float maxTime = 0f;
            try { maxTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ViperDissolveTime); } catch { }
            if (maxTime <= 0f) maxTime = 10f;
            ViperBodies.RegisterViper(victim.PlayerId, maxTime);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class Console_LogKillAttempt
{
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.logKillAttempt || __instance == null || target == null) return;
        if (target.Data == null || target.Data.IsDead) return;
        try { ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} tried to kill {ConsoleHelper.Fmt(target)} <color=#88ffcc>(blocked)</color>{ConsoleHelper.Room(target)}", "FF8844"); }
        catch { }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), new[] { typeof(SystemTypes), typeof(byte) })]
public static class Console_LogSabotage
{
    public static void Postfix(SystemTypes systemType, byte amount)
    {
        if (!CheatToggles.logSabotages) return;
        if ((amount & 128) == 0) return;
        try { ConsoleUI.Log($"Sabotage: <color=#FF4444>{systemType}</color>", "FF4444"); }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class Console_LogMeetingOrReport
{
    public static void Prefix(PlayerControl __instance, NetworkedPlayerInfo target, out string __state)
    {
        __state = "";
        if (target == null) return;
        try
        {
            foreach (DeadBody body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                if (body == null || body.ParentId != target.PlayerId) continue;
                var room = Utils.GetRoomFromPosition(body.transform.position);
                if (room != null) __state = $" <color=#888888>[{room.RoomId}]</color>";
                break;
            }
        }
        catch { }
    }

    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target, string __state)
    {
        if (__instance == null) return;
        try
        {
            if (target == null)
            {
                if (!CheatToggles.logMeetingCalled) return;
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} called an Emergency Meeting{__state}", "FFDD44");
            }
            else
            {
                if (!CheatToggles.logBodyReport || target == null) return;
                string targetName = target.Object != null ? ConsoleHelper.Fmt(target.Object) : $"<color=#aaaaaa>{target.PlayerName} (disconnected)</color>";
                string acid = ViperBodies.AcidTag(target.PlayerId);
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} reported {targetName}{__state}{acid}", "FFDD44");
            }
        }
        catch { }
    }
}



[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class Console_LogEjection
{
    public static void Postfix(ExileController __instance)
    {
        if (!CheatToggles.logEjections) return;
        try
        {
            var exiled = __instance.initData?.networkedPlayer;
            if (exiled == null) ConsoleUI.Log("No one was ejected", "888888");
            else ConsoleUI.Log($"{ConsoleHelper.Fmt(exiled)} was ejected", "FF8888");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class Console_LogVerdict
{
    public static void Postfix(NetworkedPlayerInfo exiled, bool wasOverruled, int overruleNonce)
    {
        if (!CheatToggles.logVerdict || !wasOverruled || exiled == null) return;
        try
        {
            var judge = features.JudgeCheats.GetAttributedJudge(exiled.PlayerId);
            string judgeName = judge != null
                ? (judge.AmOwner ? "<color=#00ff88>You (Judge)</color>" : ConsoleHelper.Fmt(judge))
                : "<color=#aaaaaa>Unknown Judge</color>";
            string targetName = exiled.Object != null && !exiled.Object.AmOwner
                ? ConsoleHelper.Fmt(exiled.Object)
                : $"<color=#00ff88>{exiled.PlayerName}</color>";
            string taskInfo = "";
            if (judge != null)
            {
                var jr = judge.Data?.Role?.TryCast<JudgeRole>();
                if (jr != null) taskInfo = jr.HasAnOverruleUse
                    ? " <color=#88ff88>[gavel ready]</color>"
                    : " <color=#ffcc44>[gavel spent]</color>";
            }
            ConsoleUI.Log($"🔨 Vote overruled by {judgeName} → {targetName} ejected{taskInfo} <color=#888888>(nonce {overruleNonce})</color>", "5599FF");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class Console_LogVote
{
    private static readonly System.Collections.Generic.HashSet<byte> _logged = new();

    public static void Postfix(MeetingHud __instance)
    {
        if (!CheatToggles.logVotes) return;
        if (__instance.CurrentState >= MeetingHud.MeetingStates.Results) { _logged.Clear(); return; }
        try
        {
            foreach (var area in __instance.playerStates)
            {
                if (area == null) continue;
                if (area.VotedForId == PlayerVoteArea.HasNotVoted) continue;
                if (area.VotedForId == PlayerVoteArea.MissedVote) continue;
                if (area.VotedForId == PlayerVoteArea.DeadVote) continue;
                if (_logged.Contains(area.PlayerId)) continue;
                _logged.Add(area.PlayerId);

                PlayerControl voter = null;
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p.PlayerId == area.PlayerId) { voter = p; break; }
                if (voter == null) continue;

                if (area.VotedForId == PlayerVoteArea.SkippedVote)
                {
                    ConsoleUI.Log($"{ConsoleHelper.Fmt(voter)} voted to skip", "AAAAFF");
                }
                else
                {
                    PlayerControl suspect = null;
                    foreach (var p in PlayerControl.AllPlayerControls)
                        if (p.PlayerId == area.VotedForId) { suspect = p; break; }
                    ConsoleUI.Log($"{ConsoleHelper.Fmt(voter)} voted for {(suspect != null ? ConsoleHelper.Fmt(suspect) : "?")}", "AAAAFF");
                }
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
public static class Console_LogVotekick
{
    public static void Postfix(int srcClient, int clientId)
    {
        if (!CheatToggles.logVotekicks) return;
        try
        {
            PlayerControl voter = null, target = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p?.OwnerId == srcClient) voter = p;
                if (p?.OwnerId == clientId)  target = p;
            }
            if (voter == null) return;
            ConsoleUI.Log($"{ConsoleHelper.Fmt(voter)} votekicked {ConsoleHelper.Fmt(target)}", "FF8800");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class Console_LogChat
{
    public static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        if (!CheatToggles.logChat || sourcePlayer == null || sourcePlayer.Data == null) return;
        try
        {
            bool isWhisper = features.Whisper.PendingLog;
            features.Whisper.PendingLog = false;

            if (isWhisper && features.Whisper.Count > 0)
            {
                string names = "";
                for (int i = 0; i < features.Whisper.Targets.Count; i++)
                {
                    if (i > 0) names += ", ";
                    names += ConsoleHelper.Fmt(features.Whisper.Targets[i]);
                }
                ConsoleUI.Log($"{ConsoleHelper.Fmt(PlayerControl.LocalPlayer)} > {names}: {chatText}", "DDDDDD");
                return;
            }

            string deadTag = sourcePlayer.Data.IsDead ? "<color=#FF9090>[DEAD]</color> " : "";
            ConsoleUI.Log($"{deadTag}{ConsoleHelper.Fmt(sourcePlayer)}: {chatText}", "DDDDDD");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
public static class Console_LogDisconnect
{
    public static void Prefix(PlayerControl player, DisconnectReasons reason)
    {
        if (!CheatToggles.logDisconnects || player == null || player.AmOwner) return;
        try { ConsoleUI.Log($"{ConsoleHelper.Fmt(player)} disconnected <color=#888888>({reason})</color>", "888888"); }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class Console_LogTaskCompleted
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (!CheatToggles.logTaskCompleted || __instance == null) return;
        try
        {
            PlayerTask task = null;
            foreach (var t in __instance.myTasks) { if (t.Id == idx) { task = t; break; } }
            ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} completed: <color=#88ff88>{task?.TaskType.ToString() ?? "Unknown"}</color>{ConsoleHelper.Room(__instance)}", "44FF88");
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class Console_LogJoin
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!CheatToggles.logJoins || __instance == null || __instance.AmOwner || __instance.Data == null) return;
        try { ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance)} joined the lobby", "44DDFF"); }
        catch { }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class Console_LogLobbyJoin
{
    private static string MapName(int id) => id switch
    {
        0 => "The Skeld",
        1 => "Mira HQ",
        2 => "Polus",
        4 => "Airship",
        5 => "The Fungle",
        _ => "Unknown"
    };

    private static string MapColor(int id) => id switch
    {
        0 => "FF6666",
        1 => "66FF99",
        2 => "AAAAFF",
        4 => "FFD700",
        5 => "88FF44",
        _ => "AAAAAA"
    };

    public static void Postfix(AmongUsClient __instance, string gameIdString)
    {
        try
        {
            string code     = gameIdString ?? "?";
            string hostFmt  = "?";
            string platform = "?";
            int impostors   = 1;
            int mapId       = 0;
            int players     = 0;
            InnerNet.ClientData hostData = null;

            try
            {
                hostData = AmongUsClient.Instance.GetHost();
                if (hostData?.Character?.Data != null)
                    hostFmt = ConsoleHelper.Fmt(hostData.Character);
                else if (hostData != null)
                    hostFmt = hostData.PlayerName ?? "?";
                platform = hostData?.PlatformData?.Platform.ToString() ?? "?";
            }
            catch { }

            try
            {
                var opts = GameOptionsManager.Instance?.CurrentGameOptions;
                if (opts != null)
                {
                    impostors = opts.NumImpostors;
                    mapId     = opts.MapId;
                }
            }
            catch { }

            try { players = AmongUsClient.Instance.allClients.Count; } catch { }

            string mapCol = MapColor(mapId);
            string impCol = impostors >= 3 ? "FF3333" : impostors == 2 ? "FF8800" : "FF5555";
            string platCol = platform.ToLower() switch
            {
                "android" => "44DD44",
                "iphone"  => "AAAAFF",
                "switch"  => "FF4444",
                "standalone" or "steampc" or "epic" => "44AAFF",
                _ => "CCCCCC"
            };

            uint hostLevel = 0;
            try { if (hostData?.Character?.Data != null) hostLevel = hostData.Character.Data.PlayerLevel; } catch { }

            int maxPlayers = 15;
            try { maxPlayers = GameOptionsManager.Instance?.CurrentGameOptions?.MaxPlayers ?? 15; } catch { }

            string playerCol = players >= maxPlayers ? "FF3333" : players >= maxPlayers * 0.7f ? "FFAA00" : "44FF88";

            ConsoleUI.Log($"<color=#44FFDD>[ Joined Lobby ]</color>  <color=#FFDD44>{code}</color>", "44FFDD");
            ConsoleUI.Log($"  <color=#AAAAAA>Map</color>        <color=#{mapCol}>{MapName(mapId)}</color>", mapCol);
            ConsoleUI.Log($"  <color=#AAAAAA>Impostors</color>  <color=#{impCol}>{impostors}</color>", impCol);
            ConsoleUI.Log($"  <color=#AAAAAA>Host</color>       {hostFmt}  <color=#{platCol}>[{platform}]</color>  <color=#FFDD44>Lv.{hostLevel}</color>", "DDDDDD");
            ConsoleUI.Log($"  <color=#AAAAAA>Players</color>    <color=#{playerCol}>{players}/{maxPlayers}</color>", playerCol);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.UseAbility))]
public static class Console_LogPhantomVanish
{
    private static readonly System.Collections.Generic.Dictionary<byte, bool> _phantomState = new();

    public static void Prefix(PhantomRole __instance)
    {
        if (__instance?.Player == null) return;
        try
        {
            byte id = __instance.Player.PlayerId;
            _phantomState.TryGetValue(id, out bool currentlyInvisible);
            if (currentlyInvisible)
            {
                _phantomState[id] = false;
                if (!CheatToggles.logPhantomReappear) return;
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance.Player)} <color=#dd99ff>reappeared (Phantom)</color>{ConsoleHelper.Room(__instance.Player)}", "DD99FF");
            }
            else
            {
                _phantomState[id] = true;
                if (!CheatToggles.logPhantomVanish) return;
                ConsoleUI.Log($"{ConsoleHelper.Fmt(__instance.Player)} <color=#cc66ff>vanished (Phantom)</color>{ConsoleHelper.Room(__instance.Player)}", "CC66FF");
            }
        }
        catch { }
    }
}

