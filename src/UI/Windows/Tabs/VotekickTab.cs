using UnityEngine;

namespace SkidMenu;

public class VotekickTab : ITab
{
    public string name => "Votekick";

    private Vector2 _scrollPosition = Vector2.zero;
    private Vector2 _playerListScroll = Vector2.zero;

    public void Draw()
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        DrawVoteSettings();
        GUILayout.Space(12);
        DrawMassVotekick();
        GUILayout.Space(12);
        DrawRejoin();
        GUILayout.Space(12);
        DrawPlayerList();
        GUILayout.Space(12);
        DrawInfo();

        GUILayout.EndScrollView();
    }

    private void DrawVoteSettings()
    {
        GUILayout.Label("Vote Settings", GUIStylePreset.TabSubtitle);
        GUILayout.Space(3);

        int vc = VotekickHandler.VoteCount;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  Votes per kick:  {vc}", GUILayout.Width(160));
        int newVc = Mathf.RoundToInt(GUILayout.HorizontalSlider(vc, 1, 3, GUILayout.Width(140)));
        if (newVc != vc) VotekickHandler.VoteCount = newVc;
        GUILayout.EndHorizontal();
        GUI.color = new Color(0.7f, 0.7f, 0.7f);
        GUILayout.Label("  3 votes = full kick (you + 2 rejoin cycles). Lower if lobby is large.");
        GUI.color = Color.white;

        GUILayout.Space(6);

        float interval = VotekickHandler.AutoKickInterval;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  Auto-kick interval:  {interval:F1}s", GUILayout.Width(160));
        float newInterval = Mathf.Round(GUILayout.HorizontalSlider(interval, 0.5f, 10f, GUILayout.Width(140)) * 10f) / 10f;
        if (System.Math.Abs(newInterval - interval) > 0.05f) VotekickHandler.AutoKickInterval = newInterval;
        GUILayout.EndHorizontal();
    }

    private void DrawMassVotekick()
    {
        GUILayout.Label("Mass Votekick", GUIStylePreset.TabSubtitle);
        GUILayout.Space(3);

        bool newAll = GUIStylePreset.CustomToggle(VotekickHandler.VotekickAllEnabled, " Auto Votekick All Players (polls every interval)");
        if (newAll != VotekickHandler.VotekickAllEnabled)
        {
            VotekickHandler.VotekickAllEnabled = newAll;
            VotekickHandler.ResetTracking();
        }

        VotekickHandler.AutoPurgeImpostors = GUIStylePreset.CustomToggle(VotekickHandler.AutoPurgeImpostors, " Auto Votekick Impostors");
        VotekickHandler.AutoPurgeCrew      = GUIStylePreset.CustomToggle(VotekickHandler.AutoPurgeCrew, " Auto Votekick Crew");
        VotekickHandler.AutoPurgeHost      = GUIStylePreset.CustomToggle(VotekickHandler.AutoPurgeHost, " Auto Votekick Host");
        VotekickHandler.FinishTheKick = GUIStylePreset.CustomToggle(VotekickHandler.FinishTheKick, "Finish the Kick (auto kick players with 2+ votes)");
        VotekickHandler.AutoRetaliate      = GUIStylePreset.CustomToggle(VotekickHandler.AutoRetaliate, " Auto Votekick Back");
        GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        GUILayout.Label("   kicks anyone who voted to kick you");
        GUI.color = Color.white;

        if (VotekickHandler.VotekickAllEnabled)
        {
            GUILayout.Space(3);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            GUILayout.Label($"  ⚡ AUTO-KICK ACTIVE  |  {VotekickHandler.VotekickedCount} players votekicked this cycle");
            GUI.color = Color.white;
        }

        GUILayout.Space(5);

        var prevBg = GUI.backgroundColor;
        var actionBg = GUIStylePreset.WhiteButtonBg;
        var actionStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
        actionStyle.normal.background = actionBg;
        actionStyle.hover.background  = actionBg;
        actionStyle.active.background = actionBg;
        actionStyle.normal.textColor = actionStyle.hover.textColor = actionStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        GUI.backgroundColor = new Color(0.7f, 0.15f, 0.15f, 1f);
        if (GUILayout.Button("VOTEKICK ALL NOW", actionStyle, GUILayout.Height(32)))
        {
            VotekickHandler.ResetTracking();
            VotekickHandler.VotekickAllNow();
        }
        GUI.backgroundColor = prevBg;
    }

    private void DrawRejoin()
    {
        GUILayout.Label("Rejoin (leave lobby first)", GUIStylePreset.TabSubtitle);
        GUILayout.Space(3);

        float delay = VotekickHandler.RejoinDelay;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  Rejoin delay:  {delay:F1}s", GUILayout.Width(150));
        float newDelay = Mathf.Round(GUILayout.HorizontalSlider(delay, 0f, 10f, GUILayout.Width(140)) * 10f) / 10f;
        if (System.Math.Abs(newDelay - delay) > 0.05f) VotekickHandler.RejoinDelay = newDelay;
        GUILayout.EndHorizontal();
        GUI.color = new Color(0.65f, 0.65f, 0.65f);
        GUILayout.Label("  Leaves the lobby first, waits for the menu, then rejoins last code.");
        GUI.color = Color.white;

        GUILayout.Space(5);

        VotekickHandler.AutoRejoinEnabled      = GUIStylePreset.CustomToggle(VotekickHandler.AutoRejoinEnabled, " Auto-rejoin on disconnect");
        VotekickHandler.AutoRejoinVotekickAll  = GUIStylePreset.CustomToggle(VotekickHandler.AutoRejoinVotekickAll, " Auto-rejoin after Votekick All");
        VotekickHandler.AutoRejoinVotekickHost = GUIStylePreset.CustomToggle(VotekickHandler.AutoRejoinVotekickHost, " Auto-rejoin after Votekick Host");

        GUILayout.Space(5);

        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.15f, 0.45f, 0.9f, 1f);
        if (GUILayout.Button("VOTEKICK ALL + REJOIN", GUILayout.Height(32)))
            VotekickHandler.VotekickAllAndRejoin();
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        if (GUILayout.Button("REJOIN LAST GAME", GUILayout.Height(28)))
            VotekickHandler.RejoinGame();
        GUI.backgroundColor = prevBg;
    }

    private void DrawPlayerList()
    {
        GUILayout.Label("Players", GUIStylePreset.TabSubtitle);
        GUILayout.Space(5);

        bool hasPlayers = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.AmOwner || player.Data == null) continue;
            hasPlayers = true;
            break;
        }

        if (!hasPlayers)
        {
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label("  No other players in lobby.");
            GUI.color = Color.white;
            return;
        }

        _playerListScroll = GUILayout.BeginScrollView(_playerListScroll, GUILayout.Height(280));

        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.AmOwner || player.Data == null) continue;

            int clientId = player.Data.ClientId;
            bool selected = VotekickHandler.SelectedTargetId == clientId;
            bool autoKick = VotekickHandler.IsPerPlayerAutoKickEnabled(clientId);
            bool isDead = player.Data.IsDead;
            bool isHost = player.OwnerId == AmongUsClient.Instance.HostId;

            int colorId = player.Data.DefaultOutfit.ColorId;
            Color playerColor = (colorId >= 0 && colorId < Palette.PlayerColors.Count)
                ? (Color)Palette.PlayerColors[colorId]
                : Color.white;
            Color.RGBToHSV(playerColor, out float h, out float s, out float v);
            Color buttonBg = s < 0.15f ? Color.HSVToRGB(0f, 0f, Mathf.Clamp(v * 2f, 0.5f, 1f)) : Color.HSVToRGB(h, Mathf.Min(1f, s), Mathf.Clamp(v * 1.3f, 0.5f, 1f));
            var lightBg = GUIStylePreset.WhiteButtonBg;
            var coloredStyle = new GUIStyle(GUIStylePreset.NormalButton);
            coloredStyle.normal.background = lightBg;
            coloredStyle.hover.background  = lightBg;
            coloredStyle.active.background = lightBg;
            coloredStyle.normal.textColor = coloredStyle.hover.textColor = coloredStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);

            string playerName = player.Data.DefaultOutfit.PlayerName ?? $"Client {clientId}";
            string hostTag  = isHost ? " <color=#ff4444>[HOST]</color>" : "";
            string stateTag = isDead ? "<color=#ff6666>[💀 Dead]</color>" : "<color=#88ff88>[Alive]</color>";
            string roleHex  = ColorCache.ToHex(Utils.GetCustomRoleColor(player.Data));
            string level    = $"<color=#ffdd44>Lv:{player.Data.PlayerLevel + 1}</color>";
            string platform = "";
            string fc       = "";
            int vk          = 0;
            var client      = AmongUsClient.Instance.GetClientFromCharacter(player);
            try { if (client != null) platform = $" <color=#555>|</color> <color=#00ccff>{Utils.PlatformTypeToString(client.PlatformData.Platform)}</color>"; } catch { }
            try { if (!string.IsNullOrEmpty(player.Data.FriendCode)) fc = $" <color=#555>|</color> <color=#cc88ff>{player.Data.FriendCode}</color>"; } catch { }
            try { if (client != null && VotekickHandler.UniqueVoters.TryGetValue(client.Id, out var uvs)) vk = uvs.Count; } catch { }

            string line1 = $"<b>{playerName}</b>{hostTag}  {stateTag}";
            string line2 = $"<color=#{roleHex}>{player.Data.RoleType}</color> <color=#555>|</color> {level}{platform}{fc} <color=#555>|</color> <color=#ff8800>VK:{vk}/3</color>";
            string label = $"{line1}\n{line2}";

            GUILayout.BeginHorizontal();
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = selected ? new Color(0.75f, 0.65f, 0.1f, 1f) : buttonBg;
            GUI.contentColor = Color.white;
            if (GUILayout.Button(label, coloredStyle, GUILayout.Width(460), GUILayout.Height(38)))
                VotekickHandler.SelectedTargetId = selected ? -1 : clientId;
            GUI.backgroundColor = prevBg;
            GUI.contentColor = Color.white;

            GUI.backgroundColor = autoKick ? new Color(0.8f, 0.3f, 0.1f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f);
            if (GUILayout.Button(autoKick ? "AUTO ✓" : "AUTO", GUILayout.Width(62), GUILayout.Height(26)))
                VotekickHandler.TogglePerPlayerAutoKick(clientId);
            GUI.backgroundColor = prevBg;

            if (GUILayout.Button("KICK", GUILayout.Width(50), GUILayout.Height(26)))
                VotekickHandler.VotekickPlayer(player);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(5);

        GUI.enabled = VotekickHandler.SelectedTargetId != -1;
        var prevBg2 = GUI.backgroundColor;
        var selectedBg = GUIStylePreset.WhiteButtonBg;
        var selectedStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
        selectedStyle.normal.background = selectedBg;
        selectedStyle.hover.background  = selectedBg;
        selectedStyle.active.background = selectedBg;
        selectedStyle.normal.textColor = selectedStyle.hover.textColor = selectedStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        GUI.backgroundColor = VotekickHandler.SelectedTargetId != -1 ? new Color(0.8f, 0.6f, 0.1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
        if (GUILayout.Button("VOTEKICK SELECTED TARGET", selectedStyle, GUILayout.Height(32)))
            VotekickHandler.VotekickTarget();
        GUI.backgroundColor = prevBg2;
        GUI.enabled = true;
    }

    private void DrawInfo()
    {
        GUILayout.Label("Info & Settings", GUIStylePreset.TabSubtitle);
        GUILayout.Space(3);

        VotekickHandler.ShowVotekickInfo = GUIStylePreset.CustomToggle(VotekickHandler.ShowVotekickInfo, " Show votekick events in chat");
        VotekickHandler.NotifyVotekickInfo = GUIStylePreset.CustomToggle(VotekickHandler.NotifyVotekickInfo, " Notify on votekick events");
        if (VotekickHandler.ShowVotekickInfo || VotekickHandler.NotifyVotekickInfo)
            VotekickHandler.IgnoreOwnVotekicks = GUIStylePreset.CustomToggle(VotekickHandler.IgnoreOwnVotekicks, " Hide your own votekick events");

        GUILayout.Space(6);
        GUI.color = new Color(0.65f, 0.65f, 0.65f);
        GUILayout.Label("  3 votes needed total, 1 per join. Votekick, leave, rejoin twice to complete.");
        GUILayout.Label("  AUTO column: auto-votekick a specific player every interval.");
        GUILayout.Label("  KICK button: instant single votekick, no auto cycle.");
        GUI.color = Color.white;
    }
}

