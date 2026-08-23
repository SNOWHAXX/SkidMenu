using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SkidMenu;

internal static class NotifHelper
{
    private static readonly string[] _colorCache = new string[Palette.PlayerColors.Length];

    private static string GetColorHex(int colorId)
    {
        if (colorId < 0 || colorId >= _colorCache.Length) return "ffffff";
        if (_colorCache[colorId] == null)
            _colorCache[colorId] = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[colorId]);
        return _colorCache[colorId];
    }

    public static bool Skip(PlayerControl p, int idx)
    {
        if (p == null) return false;
        if (CheatToggles.notifExSelf[idx] && p.AmOwner) return true;
        if (CheatToggles.notifExHost[idx] && p.OwnerId == AmongUsClient.Instance.HostId) return true;
        return false;
    }
    public static string Col(PlayerControl p)
    {
        if (p?.Data == null) return "<color=#ffffff>";
        return $"<color=#{GetColorHex(p.Data.DefaultOutfit.ColorId)}>";
    }
    public static string Name(PlayerControl p) => p?.Data?.PlayerName ?? "?";
    public static string Fmt(PlayerControl p) => $"{Col(p)}{Name(p)}</color>";

    public static string Room(PlayerControl p)
    {
        if (!CheatToggles.notifShowRoom || p == null) return "";
        var room = Utils.GetRoomFromPosition(p.GetTruePosition());
        return room != null ? $" <color=#aaaaaa>[{room.RoomId}]</color>" : "";
    }

    public static string TaskCount(PlayerControl p)
    {
        if (!CheatToggles.notifShowTaskCount || p?.Data == null) return "";
        try
        {
            int done = 0, total = 0;
            foreach (var t in p.myTasks) { total++; if (t.IsComplete) done++; }
            return $" <color=#88ff88>[{done}/{total}]</color>";
        }
        catch { return ""; }
    }

    public static string Dist(PlayerControl p)
    {
        if (!CheatToggles.notifShowDistance || p == null || PlayerControl.LocalPlayer == null || p.AmOwner) return "";
        try
        {
            float d = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), p.GetTruePosition());
            string col = d < 5f ? "#ff4444" : d < 12f ? "#ffaa44" : "#44ff88";
            return $" <color={col}>[{d:F1}u]</color>";
        }
        catch { return ""; }
    }
}

// Kill
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class Notif_MurderPlayer
{
    private static float _lastTime;
    private static (byte killer, byte victim) _last;

    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.notifKill || __instance == null || target == null) return;
        if (target.Data == null || !target.Data.IsDead) return;
        if (NotifHelper.Skip(__instance, 0)) return;
        var key = (__instance.PlayerId, target.PlayerId);
        if (key == _last && Time.time - _lastTime < 0.5f) return;
        _last = key; _lastTime = Time.time;
        string killer = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
        string victim = target.AmOwner    ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(target);
        string baseMsg = $"{killer} killed {victim}{NotifHelper.Room(target)}{NotifHelper.Dist(target)}";
        if (ViperBodies.IsViper(target.PlayerId))
            SkidMenu.notifications.SendLive("<color=#ff4444>☠ Kill</color>", baseMsg, 3.5f, target.PlayerId);
        else
            SkidMenu.notifications.Send("<color=#ff4444>☠ Kill</color>", baseMsg, 3.5f);
    }
}

// Shapeshift
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class Notif_Shapeshift
{
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (!CheatToggles.notifShapeshift || __instance == null || targetPlayer == null) return;
        if (NotifHelper.Skip(__instance, 4)) return;
        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;
        if (targetPlayer.PlayerId == __instance.PlayerId) return;
        string shifter = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
        SkidMenu.notifications.Send("<color=#FF8C00>◈ Shapeshift</color>",
            $"{shifter} shifted into {NotifHelper.Fmt(targetPlayer)}{NotifHelper.Room(__instance)}{NotifHelper.Dist(__instance)}", 3.5f);
    }
}

// Vent enter
[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class Notif_EnterVent
{
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.notifVent || pc == null) return;
        if (NotifHelper.Skip(pc, 2)) return;
        string name = pc.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(pc);
        SkidMenu.notifications.Send("<color=#ffff00>▼ Vent In</color>",
            $"{name} entered a vent{NotifHelper.Room(pc)}{NotifHelper.Dist(pc)}", 3f);
    }
}

// Vent exit
[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class Notif_ExitVent
{
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.notifExitVent || pc == null) return;
        if (NotifHelper.Skip(pc, 3)) return;
        string name = pc.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(pc);
        SkidMenu.notifications.Send("<color=#ffff88>▲ Vent Out</color>",
            $"{name} exited a vent{NotifHelper.Room(pc)}{NotifHelper.Dist(pc)}", 3f);
    }
}

// Phantom vanish/reappear now fired from SeePlayersInVents.PhantomAlphaPatch (SetPhantomRoleAlpha hook),
// which catches remote phantoms too - UseAbility only fires for the local ability button.

// Shapeshift revert
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class Notif_ShapeshiftRevert
{
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer)
    {
        if (!CheatToggles.notifShapeshiftRevert || __instance == null || targetPlayer == null) return;
        if (targetPlayer.PlayerId != __instance.PlayerId) return;
        if (NotifHelper.Skip(__instance, 15)) return;
        string shifter = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
        SkidMenu.notifications.Send("<color=#FF8C00>◈ Revert</color>",
            $"{shifter} reverted shift{NotifHelper.Room(__instance)}{NotifHelper.Dist(__instance)}", 3.5f);
    }
}

// Kill attempt (blocked kill)
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class Notif_KillAttempt
{
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.notifKillAttempt || __instance == null || target == null) return;
        if (target.Data == null || target.Data.IsDead) return;
        if (NotifHelper.Skip(__instance, 18)) return;
        string attacker = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
        string victim = target.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(target);
        SkidMenu.notifications.Send("<color=#ff4444>⚔ Kill Attempt</color>",
            $"{attacker} tried to kill {victim} (blocked){NotifHelper.Room(target)}{NotifHelper.Dist(target)}", 3f);
    }
}

// Task complete
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class Notif_TaskComplete
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (!CheatToggles.notifTask || __instance == null) return;
        if (NotifHelper.Skip(__instance, 13)) return;
        string name = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
        SkidMenu.notifications.Send("<color=#88ff88>✓ Task</color>",
            $"{name} completed a task{NotifHelper.TaskCount(__instance)}{NotifHelper.Room(__instance)}{NotifHelper.Dist(__instance)}", 2.5f);
    }
}

// Emergency meeting / body report
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class Notif_Meeting
{
    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        if (__instance == null) return;
        if (target == null && CheatToggles.notifMeeting && !NotifHelper.Skip(__instance, 6))
        {
            string caller = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
            SkidMenu.notifications.Send("<color=#00bfff>📢 Meeting</color>",
                $"{caller} called an emergency meeting{NotifHelper.Room(__instance)}", 4f);
        }
        else if (target != null && CheatToggles.notifBodyReport && !NotifHelper.Skip(__instance, 7))
        {
            try
            {
                string victimName = target.DefaultOutfit != null
                    ? $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[target.DefaultOutfit.ColorId])}>{target.PlayerName}</color>"
                    : $"<color=#aaaaaa>{target.PlayerName}</color>";
                string bodyRoom = GetBodyRoom(target.PlayerId);
                string reporter = __instance.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(__instance);
                string baseMsg = $"{reporter} reported {victimName}{bodyRoom}{NotifHelper.Dist(__instance)}";
                DeadBody body = ViperBodies.FindBody(target.PlayerId);
                if (body != null && body.TryCast<ViperDeadBody>() != null)
                    SkidMenu.notifications.SendLive("<color=#ff6666>💀 Body Report</color>", baseMsg, 4f, body);
                else if (ViperBodies.IsViper(target.PlayerId))
                    SkidMenu.notifications.SendLive("<color=#ff6666>💀 Body Report</color>", baseMsg, 4f, target.PlayerId);
                else
                    SkidMenu.notifications.Send("<color=#ff6666>💀 Body Report</color>", baseMsg, 4f);
            }
            catch { }
        }
    }

    public static string GetBodyRoom(byte victimId)
    {
        try
        {
            foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
            {
                if (body == null || body.ParentId != victimId) continue;
                var room = Utils.GetRoomFromPosition(body.transform.position);
                return room != null ? $" <color=#aaaaaa>[{room.RoomId}]</color>" : "";
            }
        }
        catch { }
        return "";
    }
}

// Vote cast
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class Notif_Vote
{
    private static readonly System.Collections.Generic.HashSet<byte> _logged = new();

    public static void Postfix(MeetingHud __instance)
    {
        if (!CheatToggles.notifVote) return;
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
                if (voter == null || NotifHelper.Skip(voter, 8)) continue;
                string voterName = voter.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(voter);
                if (area.VotedForId == PlayerVoteArea.SkippedVote)
                {
                    SkidMenu.notifications.Send("<color=#aaaaaa>◆ Vote</color>", $"{voterName} skipped", 2.5f);
                    continue;
                }
                PlayerControl suspect = null;
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p.PlayerId == area.VotedForId) { suspect = p; break; }
                string suspectName = suspect != null ? NotifHelper.Fmt(suspect) : $"<color=#aaaaaa>#{(byte)area.VotedForId}</color>";
                SkidMenu.notifications.Send("<color=#ff9900>◉ Vote</color>", $"{voterName} voted {suspectName}", 2.5f);
            }
        }
        catch { }
    }
}



internal static class NotifVoteHelper
{
    internal static void Fire(byte voterId, byte suspectId) { }
}

// Votekick
[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
public static class Notif_Votekick
{
    public static void Postfix(VoteBanSystem __instance, int srcClient, int clientId)
    {
        if (!CheatToggles.notifVotekick) return;
        PlayerControl voter = null, target = null;
        foreach (var p in PlayerControl.AllPlayerControls) {
            if (p?.OwnerId == srcClient) voter = p;
            if (p?.OwnerId == clientId) target = p;
        }
        if (voter == null || NotifHelper.Skip(voter, 9)) return;
        string voterName = voter.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(voter);
        SkidMenu.notifications.Send("<color=#ff6600>⚡ Votekick</color>",
            $"{voterName} votekicked {NotifHelper.Fmt(target)}", 3f);
    }
}

// Sabotage
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), new[] { typeof(SystemTypes), typeof(byte) })]
public static class Notif_Sabotage
{
    public static bool SuppressNext = false;

    public static void Prefix() => SuppressNext = true;

    public static void Postfix(SystemTypes systemType, byte amount)
    {
        bool wasLocal = SuppressNext;
        SuppressNext = false;
        if (!CheatToggles.notifSabotage) return;
        if ((amount & 128) == 0) return;
        if (CheatToggles.notifExSelf[1] && wasLocal) return;
        SkidMenu.notifications.Send("<color=#ff3300>⚠ Sabotage</color>",
            $"<color=#ffaa00>{systemType}</color> sabotaged", 4f);
    }
}

// Disconnect
[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
public static class Notif_Disconnect
{
    public static void Prefix(PlayerControl player, DisconnectReasons reason)
    {
        if (!CheatToggles.notifDisconnect || player == null || player.AmOwner) return;
        if (NotifHelper.Skip(player, 11)) return;
        SkidMenu.notifications.Send("<color=#888888>✕ Disconnect</color>",
            $"{NotifHelper.Fmt(player)} disconnected <color=#666666>({reason})</color>", 4f);
    }
}

// Chat message
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class Notif_Chat
{
    public static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        if (!CheatToggles.notifChat || sourcePlayer == null) return;
        if (NotifHelper.Skip(sourcePlayer, 10)) return;
        string preview = chatText.Length > 40 ? chatText[..40] + "…" : chatText;
        string deadTag = sourcePlayer.Data != null && sourcePlayer.Data.IsDead ? "<color=#FF9090>[DEAD]</color> " : "";
        SkidMenu.notifications.Send("<color=#aaddff>💬 Chat</color>",
            $"{deadTag}{NotifHelper.Fmt(sourcePlayer)}: <color=#dddddd>{preview}</color>", 4f);
    }
}

// Role assigned
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
public static class Notif_RoleAssign
{
    public static void Postfix(PlayerControl targetPlayer, RoleTypes roleType)
    {
        if (!CheatToggles.notifRoleAssign || targetPlayer == null) return;
        if (NotifHelper.Skip(targetPlayer, 12)) return;
        var roleColor = ColorUtility.ToHtmlStringRGB(Utils.GetCustomRoleColor(targetPlayer.Data));
        SkidMenu.notifications.Send("<color=#cc88ff>★ Role</color>",
            $"{NotifHelper.Fmt(targetPlayer)} → <color=#{roleColor}>{roleType}</color>", 3f);
    }
}

// Ejection
[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class Notif_Ejection
{
    public static void Postfix(ExileController __instance)
    {
        if (!CheatToggles.notifEjections || __instance?.initData?.networkedPlayer == null) return;
        var exiled = __instance.initData.networkedPlayer;
        if (NotifHelper.Skip(exiled.Object, 19)) return;
        string name = exiled.Object != null
            ? (exiled.Object.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(exiled.Object))
            : $"<color=#aaaaaa>{exiled.PlayerName} (disconnected)</color>";
        SkidMenu.notifications.Send("<color=#ff8888>💀 Ejected</color>",
            $"{name} was ejected{NotifHelper.Room(exiled.Object)}", 4f);
    }
}

// Judge verdict
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class Notif_Verdict
{
    public static void Postfix(NetworkedPlayerInfo exiled, bool wasOverruled, int overruleNonce)
    {
        if (!CheatToggles.notifVerdict || !wasOverruled || exiled == null) return;
        try
        {
            var judge = features.JudgeCheats.GetAttributedJudge(exiled.PlayerId);
            if (exiled.Object != null && NotifHelper.Skip(exiled.Object, 22) && judge != null && judge.AmOwner) return;
            string targetName = exiled.Object != null
                ? (exiled.Object.AmOwner ? "<color=#ff4444>You</color>" : NotifHelper.Fmt(exiled.Object))
                : $"<color=#aaaaaa>{exiled.PlayerName}</color>";
            string judgeName = judge != null
                ? (judge.AmOwner ? "<color=#00ff88>You</color>" : NotifHelper.Fmt(judge))
                : "<color=#aaaaaa>unknown</color>";
            bool hitUs = exiled.Object != null && exiled.Object.AmOwner;
            string title = hitUs ? "<color=#ff4444>🔨 Gavelled</color>" : "<color=#5599ff>🔨 Verdict</color>";
            string body = hitUs
                ? $"Ejected by gavel ({judgeName}) <color=#888888>· nonce {overruleNonce}</color>"
                : $"{targetName} ejected by gavel ({judgeName}) <color=#888888>· nonce {overruleNonce}{NotifHelper.Room(exiled.Object)}</color>";
            SkidMenu.notifications.Send(title, body, 5f);
        }
        catch { }
    }
}

// Player join
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class Notif_Join
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!CheatToggles.notifJoin || __instance == null || __instance.AmOwner) return;
        if (NotifHelper.Skip(__instance, 14)) return;
        try { __instance.StartCoroutine(DelayedJoinNotif(__instance).WrapToIl2Cpp()); }
        catch { }
    }

    private static System.Collections.IEnumerator DelayedJoinNotif(PlayerControl player)
    {
        yield return new WaitForSeconds(0.6f);
        if (player == null || player.Data == null) yield break;
        try
        {
            var client = AmongUsClient.Instance.GetClientFromCharacter(player);
            if (client == null) yield break;
            string platform = client.PlatformData?.Platform.ToString() ?? "?";
            string level    = player.Data.PlayerLevel.ToString();
            string col      = NotifHelper.Col(player);
            SkidMenu.notifications.Send($"<color=#44ddff>→ Join</color>",
                $"{col}{client.PlayerName}</color> <color=#aaaaaa>{platform} · Lv{level}</color>", 4f);
        }
        catch { }
    }
}



