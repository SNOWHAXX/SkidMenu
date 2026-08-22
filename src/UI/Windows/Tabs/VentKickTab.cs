using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

namespace SkidMenu;

public class VentKickTab : ITab
{
    public string name => "Vent Kick Exploit";

    private Vector2 _scroll = Vector2.zero;

    private static Texture2D _lightBg;
    private static GUIStyle  _quickStyle;
    private static GUIStyle  _cardStyle;

    private static void EnsureStyles()
    {
        if (_lightBg != null) return;
        _lightBg    = GUIStylePreset.WhiteButtonBg;
        _quickStyle = new GUIStyle(GUI.skin.button)  { richText = true, border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 } };
        _quickStyle.normal.background = _quickStyle.hover.background = _quickStyle.active.background = _lightBg;
        _quickStyle.normal.textColor = _quickStyle.hover.textColor = _quickStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        _cardStyle  = new GUIStyle(GUIStylePreset.NormalButton) { richText = true };
        _cardStyle.normal.background  = _cardStyle.hover.background  = _cardStyle.active.background  = _lightBg;
        _cardStyle.normal.textColor = _cardStyle.hover.textColor = _cardStyle.active.textColor = new Color(0.10f, 0.10f, 0.12f, 1f);
    }

    public void Draw()
    {
        EnsureStyles();
        var players = PlayerControl.AllPlayerControls.ToArray();
        _scroll = GUILayout.BeginScrollView(_scroll);
        DrawQuickActions(players);
        GUILayout.Space(10);
        DrawPlayerList(players);
        GUILayout.EndScrollView();
    }

    private void DrawQuickActions(PlayerControl[] players)
    {
        GUILayout.Label("Quick Actions", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        var old = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.8f, 0.15f, 0.15f, 1f);
        if (GUILayout.Button("KICK ALL", _quickStyle, GUILayout.ExpandWidth(true), GUILayout.Height(34)))
            foreach (var p in players)
                if (p != null && !p.AmOwner && p.Data != null) VentKick(p);

        GUI.backgroundColor = new Color(0.7f, 0.1f, 0.1f, 1f);
        if (GUILayout.Button("KICK IMPOSTORS", _quickStyle, GUILayout.ExpandWidth(true), GUILayout.Height(34)))
            foreach (var p in players)
                if (p != null && !p.AmOwner && p.Data != null && RoleManager.IsImpostorRole(p.Data.RoleType)) VentKick(p);

        GUI.backgroundColor = new Color(0.1f, 0.5f, 0.2f, 1f);
        if (GUILayout.Button("KICK CREWMATES", _quickStyle, GUILayout.ExpandWidth(true), GUILayout.Height(34)))
            foreach (var p in players)
                if (p != null && !p.AmOwner && p.Data != null && !RoleManager.IsImpostorRole(p.Data.RoleType)) VentKick(p);

        GUI.backgroundColor = old;
        GUILayout.EndHorizontal();
    }

    private void DrawPlayerList(PlayerControl[] players)
    {
        GUILayout.Label("Players", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        foreach (PlayerControl p in players)
        {
            if (p == null || p.AmOwner || p.Data == null) continue;

            bool isHost = p.OwnerId == AmongUsClient.Instance.HostId;
            bool isDead = p.Data.IsDead;
            Color roleColor = Utils.GetCustomRoleColor(p.Data);
            string roleColorHex = ColorCache.ToHex(roleColor);
            string stateTag = isDead ? " <color=#aaaaaa>[Dead]</color>" : " <color=#88ff88>[Alive]</color>";
            string hostTag = isHost ? " <color=#ff4444>[HOST]</color>" : "";
            string line1 = $"<size=15><color=#14141a><b>{p.Data.PlayerName}</b></color>{hostTag}{stateTag}</size>";
            string level = $"<color=#ffdd44>Lv:{p.Data.PlayerLevel + 1}</color>";
            string platform = ""; string fc = "";
            var client = AmongUsClient.Instance.GetClientFromCharacter(p);
            try { if (client != null) platform = $" <color=#555>|</color> <color=#00ccff>{Utils.PlatformTypeToString(client.PlatformData.Platform)}</color>"; } catch { }
            try { if (!string.IsNullOrEmpty(p.Data.FriendCode)) fc = $" <color=#555>|</color> <color=#cc88ff>{p.Data.FriendCode}</color>"; } catch { }
            string line2 = $"<size=13><color=#{roleColorHex}>{p.Data.RoleType}</color> <color=#555>|</color> {level}{platform}{fc}</size>";
            string label = $"{line1}\n{line2}";

            Color playerColor = Palette.PlayerColors[p.Data.DefaultOutfit.ColorId];
            var old = GUI.backgroundColor; var oldC = GUI.contentColor;
            Color.RGBToHSV(playerColor, out float h, out float s, out float v);
            GUI.backgroundColor = s < 0.15f ? Color.HSVToRGB(0f, 0f, Mathf.Clamp(v * 2f, 0.5f, 1f)) : Color.HSVToRGB(h, Mathf.Min(1f, s), Mathf.Clamp(v * 1.3f, 0.5f, 1f));
            GUI.contentColor = Color.white;
            if (GUILayout.Button(label, _cardStyle, GUILayout.Width(520), GUILayout.Height(44))) VentKick(p);
            GUI.backgroundColor = old; GUI.contentColor = oldC;
            GUILayout.Space(2);
        }
    }

    public static void VentKick(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        try
        {
            if (AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                SkidMenu.notifications.Send("Vent Kick", $"{player.Data.PlayerName} has been kicked.");
                return;
            }

            if (ShipStatus.Instance == null)
            {
                SkidMenu.notifications.Send("Vent Kick", "Game must have started for this to work.");
                return;
            }

            Network.BatchedMessage batch = new Network.BatchedMessage(player.OwnerId);

            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.Write((ushort)0);
            writer.Write((byte)VentilationSystem.Operation.Enter);
            writer.Write((byte)0);
            batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
            writer.Recycle();

            MessageWriter writer2 = MessageWriter.Get(SendOption.Reliable);
            writer2.Write((ushort)1);
            writer2.Write((byte)VentilationSystem.Operation.BootImpostors);
            writer2.Write((byte)0);
            batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer2);
            writer2.Recycle();

            batch.FinishBatch();
            SkidMenu.notifications.Send("Vent Kick", $"{player.Data.PlayerName} has been kicked.");
        }
        catch { }
    }
}
