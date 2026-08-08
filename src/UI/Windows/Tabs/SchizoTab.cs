using UnityEngine;

namespace SkidMenu;

public class SchizoTab : ITab
{
    public string name => "Schizo/FakeSab";

    private static GUIStyle _boldLabel;
    private static GUIStyle BoldLabel => _boldLabel ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

    private Vector2 _scroll = Vector2.zero;

    public void Draw()
    {
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label($"Spam Cooldown: {BanHandler.SpamCooldown:F2}s");
        BanHandler.SpamCooldown = GUILayout.HorizontalSlider(BanHandler.SpamCooldown, 0f, 1f);

        GUILayout.Space(8f);

        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Schizo All", BoldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reactor", GUILayout.Height(28f)))
            foreach (PlayerControl p in PlayerCache.Get()) { if (p == null || p.AmOwner || p.Data == null) continue; BanHandler.FakeReactorTarget(p); }
        if (GUILayout.Button("Doors", GUILayout.Height(28f)))
            foreach (PlayerControl p in PlayerCache.Get()) { if (p == null || p.AmOwner || p.Data == null) continue; BanHandler.DoorHallucination(p); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        BanHandler.SpamReactorAll = GUIStylePreset.CustomToggle(BanHandler.SpamReactorAll, "Spam Reactor");
        BanHandler.SpamDoorsAll   = GUIStylePreset.CustomToggle(BanHandler.SpamDoorsAll, "Spam Doors");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(8f);

        var labelStyle = new GUIStyle(GUI.skin.label) { richText = true };

        foreach (PlayerControl player in PlayerCache.Get())
        {
            if (player == null || player.AmOwner || player.Data == null) continue;
            int clientId = player.Data.ClientId;

            bool isHost = player.OwnerId == AmongUsClient.Instance.HostId;
            bool isDead  = player.Data.IsDead;

            // Player's actual Among Us color
            string playerColorHex = "ffffff";
            try { playerColorHex = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.Data.DefaultOutfit.ColorId]); } catch { }

            // Card background — very subtle tint of player color
            Color tint = new Color(0.13f, 0.13f, 0.13f, 1f);

            var cardStyle = new GUIStyle(GUIStylePreset.SectionBox);
            cardStyle.normal.background = GUIStylePreset.MakeTex1x1(tint);
            cardStyle.padding = new RectOffset { left = 10, right = 10, top = 8, bottom = 8 };

            GUILayout.BeginVertical(cardStyle);

            Color roleColor     = Utils.GetCustomRoleColor(player.Data);
            string roleColorHex = ColorCache.ToHex(roleColor);
            string stateTag     = isDead ? " <color=#888888>[Dead]</color>" : " <color=#44dd44>[Alive]</color>";
            string hostTag      = isHost ? " <color=#ff4444>[HOST]</color>" : "";

            string line1 = $"<size=15><color=#{playerColorHex}><b>{player.Data.PlayerName}</b></color>{hostTag}{stateTag}</size>";

            string level    = $"<color=#ffbb33>Lv:{player.Data.PlayerLevel + 1}</color>";
            string platform = "";
            string fc       = "";
            var client = AmongUsClient.Instance.GetClientFromCharacter(player);
            try { if (client != null) platform = $" <color=#555>|</color> <color=#33aacc>{Utils.PlatformTypeToString(client.PlatformData.Platform)}</color>"; } catch { }
            try { if (!string.IsNullOrEmpty(player.Data.FriendCode)) fc = $" <color=#555>|</color> <color=#aa55ee>{player.Data.FriendCode}</color>"; } catch { }
            int vk = 0;
            try { if (client != null && VotekickHandler.UniqueVoters.TryGetValue(client.Id, out var uvs)) vk = uvs.Count; } catch { }
            string line2 = $"<size=13><color=#{roleColorHex}>{player.Data.RoleType}</color> <color=#555>|</color> {level}{platform}{fc} <color=#555>|</color> <color=#ff6600>VK:{vk}/3</color></size>";

            GUILayout.Label($"{line1}\n{line2}", labelStyle);

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reactor", GUILayout.Height(28f))) BanHandler.FakeReactorTarget(player);
            if (GUILayout.Button("Doors",   GUILayout.Height(28f))) BanHandler.DoorHallucination(player);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            BanHandler.SpamReactorPerPlayer.TryGetValue(clientId, out bool sr);
            BanHandler.SpamDoorsPerPlayer.TryGetValue(clientId, out bool sd);
            BanHandler.SpamReactorPerPlayer[clientId] = GUIStylePreset.CustomToggle(sr, "Spam Reactor");
            BanHandler.SpamDoorsPerPlayer[clientId]   = GUIStylePreset.CustomToggle(sd, "Spam Doors");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        GUILayout.EndScrollView();
    }
}


