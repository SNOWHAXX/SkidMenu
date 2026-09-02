using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkidMenu.features;

public class MatchInfoEnhancer : MonoBehaviour
{
    private static MatchInfoEnhancer _instance;
    private float _timer;
    private readonly HashSet<int> _setupDone = new();
    private MatchInfoGuide _lastGuide;

    public static void Init()
    {
        if (_instance != null) return;
        _instance = SkidMenu.Instance.AddComponent<MatchInfoEnhancer>();
    }

    private void Awake() => _instance = this;

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = 0.25f;

        var guide = MatchInfoGuide.Instance;
        if (guide == null) return;

        if (guide != _lastGuide)
        {
            _setupDone.Clear();
            _lastGuide = guide;
        }

        var players = GameData.Instance?.AllPlayers;
        if (players == null || players.Count == 0) return;

        InjectRecursive(guide.transform, players);
    }

    private void InjectRecursive(Transform current, Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> players)
    {
        for (int i = 0; i < current.childCount; i++)
        {
            var child = current.GetChild(i);
            if (child == null || !child.gameObject.activeInHierarchy) continue;

            TMP_Text tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = child.GetComponent<TextMeshPro>();
            if (tmp == null || string.IsNullOrEmpty(tmp.text))
            {
                InjectRecursive(child, players);
                continue;
            }

            for (int p = 0; p < players.Count; p++)
            {
                var pi = players[p];
                if (pi == null) continue;
                if (!tmp.text.Contains(pi.PlayerName)) continue;

                bool roleGuide = CheatToggles.espShowRole && (ESPContexts.ShowRole & ESPContexts.InGuide) != 0;
                bool infoGuide = CheatToggles.espShowPlayerInfo && (ESPContexts.ShowInfo & ESPContexts.InGuide) != 0;

                var lines = new List<string>();

                if (roleGuide && pi.Role != null)
                {
                    var rc = ColorCache.ToHex(Utils.GetRoleDisplayColor(pi));
                    lines.Add($"<color=#{rc}>{pi.RoleType}</color>");
                }

                if (roleGuide && CheatToggles.espKillCooldown && (ESPContexts.KillCooldown & ESPContexts.InGuide) != 0)
                {
                    try
                    {
                        if (pi.Role != null && pi.Role.CanUseKillButton)
                        {
                            var cd = KillCooldownTracker.GetRemainingCooldown(pi.PlayerId);
                            lines.Add($"<color=#ff6666>CD:{cd:F1}s</color>");
                        }
                    } catch { }
                }

                if (roleGuide && CheatToggles.espTasks && (ESPContexts.Tasks & ESPContexts.InGuide) != 0)
                {
                    try
                    {
                        if (pi.Role != null && !pi.Role.IsImpostor)
                        {
                            int done = 0, total = 0;
                            foreach (var t in pi.Object.myTasks) { total++; if (t.IsComplete) done++; }
                            lines.Add($"<color=#88ff88>T:{done}/{total}</color>");
                        }
                    } catch { }
                }

                if (infoGuide && CheatToggles.espIsHost && (ESPContexts.IsHost & ESPContexts.InGuide) != 0)
                {
                    if (AmongUsClient.Instance != null && pi.ClientId == AmongUsClient.Instance.HostId)
                        lines.Add("<color=#ff4444>HOST</color>");
                }

                if (infoGuide && CheatToggles.espModUser && (ESPContexts.ModUser & ESPContexts.InGuide) != 0)
                {
                    try { var mods = anticheat.ModDetection.GetModNames(pi.PlayerId); if (!string.IsNullOrEmpty(mods)) lines.Add($"<color=#00ff88>{mods}</color>"); } catch { }
                }

                if (infoGuide && CheatToggles.espLevel && (ESPContexts.Level & ESPContexts.InGuide) != 0)
                    lines.Add($"<color=#fb0>Lv:{pi.PlayerLevel + 1}</color>");

                if (infoGuide && CheatToggles.espPlatform && (ESPContexts.Platform & ESPContexts.InGuide) != 0)
                {
                    try
                    {
                        var cl = AmongUsClient.Instance?.GetClient(pi.ClientId);
                        if (cl != null)
                            lines.Add($"<color=#fb0>{Utils.PlatformTypeToString(cl.PlatformData.Platform)}</color>");
                    } catch { }
                }

                if (infoGuide && CheatToggles.espVotekicks && (ESPContexts.Votekicks & ESPContexts.InGuide) != 0)
                {
                    try
                    {
                        int vkCount = VotekickHandler.UniqueVoters.TryGetValue(pi.ClientId, out var uvs) ? uvs.Count : 0;
                        lines.Add($"<color=#ff8800>VK:{vkCount}/3</color>");
                    } catch { }
                }

                var accts = new List<string>();

                if (infoGuide && CheatToggles.espPuid && !string.IsNullOrEmpty(pi.Puid) && (ESPContexts.Puid & ESPContexts.InGuide) != 0)
                    accts.Add($"<color=#ff88cc>{pi.Puid}</color>");

                if (infoGuide && CheatToggles.espFriendCode && !string.IsNullOrEmpty(pi.FriendCode) && (ESPContexts.FriendCode & ESPContexts.InGuide) != 0)
                    accts.Add($"<color=#aaaaff>{pi.FriendCode}</color>");

                if (infoGuide && CheatToggles.espDeviceId && (ESPContexts.DeviceId & ESPContexts.InGuide) != 0)
                    accts.Add($"<color=#ff6644>ID:{pi.ClientId}</color>");

                if (lines.Count == 0 && accts.Count == 0) break;

                int cid = child.GetInstanceID();
                if (_setupDone.Add(cid))
                {
                    tmp.fontSize *= 0.8f;
                    var pos = child.localPosition;
                    pos.y += 0.1f;
                    child.localPosition = pos;
                }

                string built = lines.Count > 0 ? string.Join(" ", lines) : "";
                if (accts.Count > 0)
                {
                    if (built.Length > 0) built += " <color=#555>|</color> ";
                    built += string.Join(" ", accts);
                }

                string marker = "<color=#00000000>⠀</color>";
                string tag = $"{marker}{built}";

                int idx = tmp.text.IndexOf(marker);
                if (idx >= 0)
                    tmp.text = tmp.text.Substring(0, idx) + tag;
                else
                    tmp.text += $"\n<size=75%>{tag}</size>";

                break;
            }

            InjectRecursive(child, players);
        }
    }
}
