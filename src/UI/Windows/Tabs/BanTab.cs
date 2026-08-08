using AmongUs.GameOptions;
using UnityEngine;

namespace SkidMenu;

public class BanTab : ITab
{
    public string name => "Fun Ban Exploit";

    private Vector2 _scroll = Vector2.zero;

    public void Draw()
    {
        _scroll = GUILayout.BeginScrollView(_scroll);

        DrawQuickActions();
        GUILayout.Space(10);
        DrawPlayerList();

        GUILayout.EndScrollView();
    }

    private void DrawQuickActions()
    {
        GUILayout.Label("Quick Actions", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        var old = GUI.backgroundColor;
        var quickBg = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
        var quickStyle = new GUIStyle(GUI.skin.button);
        quickStyle.normal.background = quickBg;
        quickStyle.hover.background  = quickBg;
        quickStyle.active.background = quickBg;

        GUI.backgroundColor = new Color(0.8f, 0.15f, 0.15f, 1f);
        if (GUILayout.Button("BAN ALL", quickStyle, GUILayout.Height(34)))
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p == null || p.AmOwner || p.Data == null) continue;
                BanHandler.BanPlayer(p);
            }
        }

        GUI.backgroundColor = new Color(0.7f, 0.1f, 0.1f, 1f);
        if (GUILayout.Button("BAN IMPOSTORS", quickStyle, GUILayout.Height(34)))
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p == null || p.AmOwner || p.Data == null) continue;
                if (RoleManager.IsImpostorRole(p.Data.RoleType))
                    BanHandler.BanPlayer(p);
            }
        }

        GUI.backgroundColor = new Color(0.1f, 0.5f, 0.2f, 1f);
        if (GUILayout.Button("BAN CREWMATES", quickStyle, GUILayout.Height(34)))
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p == null || p.AmOwner || p.Data == null) continue;
                if (!RoleManager.IsImpostorRole(p.Data.RoleType))
                    BanHandler.BanPlayer(p);
            }
        }

        GUI.backgroundColor = old;
        GUILayout.EndHorizontal();
    }

    private void DrawPlayerList()
    {
        GUILayout.Label("Players", GUIStylePreset.TabSubtitle);
        GUILayout.Space(4);

        foreach (PlayerControl p in PlayerControl.AllPlayerControls.ToArray())
        {
            if (p == null || p.AmOwner || p.Data == null) continue;

            bool isHost = p.OwnerId == AmongUsClient.Instance.HostId;
            bool isDead = p.Data.IsDead;

            Color roleColor = Utils.GetCustomRoleColor(p.Data);
            string roleColorHex = ColorCache.ToHex(roleColor);

            // Line 1: name + host + state
            string stateTag = isDead ? " <color=#aaaaaa>[💀 Dead]</color>" : " <color=#88ff88>[Alive]</color>";
            string hostTag = isHost ? " <color=#ff4444>[HOST]</color>" : "";
            string line1 = $"<size=15><color=#ffffff><b>{p.Data.PlayerName}</b></color>{hostTag}{stateTag}</size>";

            // Line 2: role | level | platform | friendcode | votekicks
            string level = $"<color=#ffdd44>Lv:{p.Data.PlayerLevel + 1}</color>";
            string platform = "";
            string fc = "";
            var client = AmongUsClient.Instance.GetClientFromCharacter(p);
            try { if (client != null) platform = $" <color=#555>|</color> <color=#00ccff>{Utils.PlatformTypeToString(client.PlatformData.Platform)}</color>"; } catch { }
            try { if (!string.IsNullOrEmpty(p.Data.FriendCode)) fc = $" <color=#555>|</color> <color=#cc88ff>{p.Data.FriendCode}</color>"; } catch { }
            int vk = 0;
            try { if (client != null && VotekickHandler.UniqueVoters.TryGetValue(client.Id, out var uvs)) vk = uvs.Count; } catch { }
            string line2 = $"<size=13><color=#{roleColorHex}>{p.Data.RoleType}</color> <color=#555>|</color> {level}{platform}{fc} <color=#555>|</color> <color=#ff8800>VK:{vk}/3</color></size>";

            string label = $"{line1}\n{line2}";

            Color playerColor = Palette.PlayerColors[p.Data.DefaultOutfit.ColorId];
            var old = GUI.backgroundColor;
            var oldContent = GUI.contentColor;

            Color.RGBToHSV(playerColor, out float h, out float s, out float v);
            GUI.backgroundColor = Color.HSVToRGB(h, Mathf.Min(1f, s * 1.2f), Mathf.Clamp(v * 1.3f, 0.5f, 1f));
            GUI.contentColor = Color.white;

            var lightBg = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
            var coloredStyle = new GUIStyle(GUIStylePreset.NormalButton);
            coloredStyle.normal.background = lightBg;
            coloredStyle.hover.background  = lightBg;
            coloredStyle.active.background = lightBg;

            if (GUILayout.Button(label, coloredStyle, GUILayout.Height(44)))
                BanHandler.BanPlayer(p);

            GUI.backgroundColor = old;
            GUI.contentColor = oldContent;

            GUILayout.Space(2);
        }
    }
}
