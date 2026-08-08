using UnityEngine;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using System;
using System.Collections;

namespace SkidMenu;

public class PlayersTab : ITab
{
    public string name => "Players";

    private Vector2 _subsectionScrollVector = Vector2.zero;
    private Vector2 _subsectionScrollVector2 = Vector2.zero;
    private static CrewmateColor _selectedColor = CrewmateColor.Red;
    private static int _selectedVent = 0;

    // cached textures + styles (fix 1+2)
    private static Texture2D _playerButtonTex;
    private static GUIStyle  _playerButtonStyle;
    private static GUIStyle  _watchButtonStyle;

    // cached info string (fix 5)
    private static string        _cachedInfoString;
    private static PlayerControl _lastInfoTarget;
    private static float         _infoStringTimer;
    private const  float         InfoStringInterval = 0.25f;

    // cached dead bodies (fix 4)
    private static DeadBody[] _cachedDeadBodies  = Array.Empty<DeadBody>();
    private static float      _deadBodyCacheTimer;
    private const  float      DeadBodyCacheInterval = 0.5f;

    // cached history (fix 6)
    private static string _cachedHistoryString;
    private static int    _lastHistoryCount = -1;

    public void Draw()
    {
        if (PlayerControl.AllPlayerControls.Count == 0)
        {
            GUILayout.Label("There are currently no online players.");
            return;
        }

        GUILayout.BeginHorizontal();

        // Left panel: Player list
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.35f));
        _subsectionScrollVector = GUILayout.BeginScrollView(_subsectionScrollVector);
        DrawPlayerList();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Right panel: Player controls
        if (PlayersSection.selectedPlayer != null)
        {
            GUILayout.BeginVertical();
            _subsectionScrollVector2 = GUILayout.BeginScrollView(_subsectionScrollVector2);
            DrawPlayerControls(PlayersSection.selectedPlayer);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawPlayerList()
    {
        for (byte i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
        {
            PlayerControl player = PlayerControl.AllPlayerControls[i];
            if (player.Data == null) continue;

            RenderPlayerSelection(i, player);
        }
    }

    private void RenderPlayerSelection(byte position, PlayerControl player)
    {
        Color roleColor = Utils.GetCustomRoleColor(player.Data);
        string roleColorHex = ColorCache.ToHex(roleColor);
        bool isHost = player.OwnerId == AmongUsClient.Instance.HostId;
        bool isDead = player.Data.IsDead;

        // Line 1: name + host + state
        string stateTag = isDead ? " <color=#ff6666>[?? Dead]</color>" : " <color=#88ff88>[Alive]</color>";
        string hostTag = isHost ? " <color=#ff4444>[HOST]</color>" : "";
        string line1 = $"<color=#ffffff><b>{player.Data.PlayerName}</b></color>{hostTag}{stateTag}";

        // Line 2: role | level | platform
        string level = $"<color=#ffdd44>Lv:{player.Data.PlayerLevel + 1}</color>";
        // fix 3: one GetClientFromCharacter call instead of two
        ClientData cd = null;
        try { cd = AmongUsClient.Instance.GetClientFromCharacter(player); } catch { }

        string platform = cd != null ? $" <color=#555>|</color> <color=#00ccff>{Utils.PlatformTypeToString(cd.PlatformData.Platform)}</color>" : "";
        string line2 = $"<color=#{roleColorHex}>{player.Data.RoleType}</color> <color=#555>|</color> {level}{platform}";

        string fc = "";
        try { fc = $"<color=#cc88ff>{player.Data.FriendCode}</color> <color=#555>|</color> "; } catch { }
        int vk = 0;
        if (cd != null && VotekickHandler.UniqueVoters.TryGetValue(cd.Id, out var uvs)) vk = uvs.Count;
        string line3 = $"{fc}<color=#ff8800>VK:{vk}/3</color>";

        string label = $"{line1}\n{line2}\n{line3}";

        Color playerColor = Palette.PlayerColors[player.Data.DefaultOutfit.ColorId];
        var old = GUI.backgroundColor;
        var oldContent = GUI.contentColor;

        Color.RGBToHSV(playerColor, out float h, out float s, out float v);
        GUI.backgroundColor = Color.HSVToRGB(h, Mathf.Min(1f, s * 1.2f), Mathf.Clamp(v * 1.3f, 0.5f, 1f));
        GUI.contentColor = Color.white;

        // fix 1+2: lazy-init once, reuse forever
        if (_playerButtonTex == null)
            _playerButtonTex = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
        if (_playerButtonStyle == null)
        {
            _playerButtonStyle = new GUIStyle(GUIStylePreset.NormalButton);
            _playerButtonStyle.normal.background = _playerButtonTex;
            _playerButtonStyle.hover.background  = _playerButtonTex;
            _playerButtonStyle.active.background = _playerButtonTex;
        }

        if (GUILayout.Button(label, _playerButtonStyle))
            PlayersSection.selectedPlayer = player;

        GUI.backgroundColor = old;
        GUI.contentColor = oldContent;
    }

    private static void DrawPlayerControls(PlayerControl target)
    {
        if (target == null)
        {
            GUILayout.Label("Specified target is not valid.");
            return;
        }

        ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
        if (clientData != null)
        {
            // fix 4+5: rebuild info string at most every 0.25s, not every frame
            bool targetChanged = target != _lastInfoTarget;
            _infoStringTimer += Time.deltaTime;
            if (targetChanged || _infoStringTimer >= InfoStringInterval || _cachedInfoString == null)
            {
                _lastInfoTarget  = target;
                _infoStringTimer = 0f;

                bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;
                var roleColor    = Utils.GetCustomRoleColor(target.Data);
                var roleColorHex = ColorCache.ToHex(roleColor);
                var skinColorHex = ColorCache.ToHex(Palette.PlayerColors[target.Data.DefaultOutfit.ColorId]);

                // fix 4: refresh dead body cache every 0.5s instead of FindObjectsOfType every frame
                _deadBodyCacheTimer += Time.deltaTime;
                if (targetChanged || _deadBodyCacheTimer >= DeadBodyCacheInterval)
                {
                    _deadBodyCacheTimer = 0f;
                    try { _cachedDeadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>(); }
                    catch { _cachedDeadBodies = Array.Empty<DeadBody>(); }
                }

                string stateStr;
                if (target.Data.IsDead)
                {
                    string corpseRoom = "";
                    try {
                        foreach (DeadBody b in _cachedDeadBodies)
                            if (b?.ParentId == target.PlayerId)
                            {
                                var room = Utils.GetRoomFromPosition(b.transform.position);
                                if (room != null) corpseRoom = $" <color=#888888>({room.RoomId})</color>";
                                break;
                            }
                    } catch { }
                    stateStr = $"<color=#ff6666>Dead{corpseRoom}</color>";
                }
                else if (MeetingHud.Instance != null)
                    stateStr = "<color=#00bfff>In Meeting</color>";
                else if (target.inVent)
                    stateStr = "<color=#ffff00>In Vent</color>";
                else if (target.CurrentOutfitType != PlayerOutfitType.Default)
                    stateStr = "<color=#FF8C00>Shapeshifted</color>";
                else
                    stateStr = "<color=#88ff88>Alive</color>";

                string tasksStr = "N/A";
                try
                {
                    int done = 0, total = 0;
                    foreach (var t in target.myTasks) { total++; if (t.IsComplete) done++; }
                    tasksStr = $"{done}/{total}";
                } catch { }

                int vkCount = 0;
                string vkVoters = "";
                try {
                    if (VotekickHandler.UniqueVoters.TryGetValue(clientData.Id, out var uvs))
                    {
                        vkCount = uvs.Count;
                        var voterNames = new System.Collections.Generic.List<string>();
                        foreach (var voterId in uvs)
                            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                                if (p?.OwnerId == voterId)
                                    voterNames.Add($"<color=#{ColorCache.ToHex(Palette.PlayerColors[p.Data.DefaultOutfit.ColorId])}>{p.Data.PlayerName}</color>");
                        if (voterNames.Count > 0) vkVoters = $" ({string.Join(", ", voterNames)})";
                    }
                } catch { }

                string lastRoom = PlayerTracker.LastRoom.TryGetValue(target.PlayerId, out var lr) ? lr : "Unknown";

                string modStr = "";
                if (anticheat.ModDetection.DetectedMods.TryGetValue(target.PlayerId, out var detectedMods) && detectedMods.Count > 0)
                    modStr = $"\n<color=#ffffff>Mods:</color> <color=#ff4444>{string.Join(", ", detectedMods)}</color>";

                _cachedInfoString =
                    $"<color=#ffffff>Name:</color> <color=#{skinColorHex}>{target.Data.PlayerName}</color> ({target.Data.ColorName})\n" +
                    $"<color=#ffffff>Role:</color> <color=#{roleColorHex}>{target.Data.RoleType}</color>\n" +
                    $"<color=#ffffff>State:</color> {stateStr}\n" +
                    $"<color=#ffffff>Last Room:</color> <color=#aaaaaa>{lastRoom}</color>\n" +
                    $"<color=#ffffff>Friendcode:</color> <color=#aaaaff>{(streamerMode ? "REDACTED" : target.Data.FriendCode)}</color>\n" +
                    $"<color=#ffffff>PUID:</color> <color=#ff88cc>{(streamerMode ? "REDACTED" : target.Data.Puid)}</color>\n" +
                    $"<color=#ffffff>Level:</color> <color=#ffdd00>{target.Data.PlayerLevel + 1}</color>\n" +
                    $"<color=#ffffff>Device:</color> <color=#00ccff>{Utils.PlatformTypeToString(clientData.PlatformData.Platform)}</color>\n" +
                    $"<color=#ffffff>Device ID:</color> <color=#ff6644>{clientData.Id}</color>\n" +
                    $"<color=#ffffff>Tasks:</color> <color=#88ff88>{tasksStr}</color>\n" +
                    $"<color=#ffffff>Votekicks:</color> <color=#ff8800>{vkCount}/3</color>{vkVoters}" +
                    (target.OwnerId == AmongUsClient.Instance.HostId ? "\n<color=#ff4444>HOST</color>" : "") +
                    modStr;
            }

            GUILayout.Label(_cachedInfoString, GUIStylePreset.ModernLabel);
        }
        else
        {
            GUILayout.Label(
                $"Name: {target.Data.PlayerName} {target.Data.ColorName}" +
                $"\nRole: {target.Data.RoleType}" +
                $"\nState: " + (target.Data.IsDead ? "Dead" : "Alive") +
                $"\nIs Dummy: true"
            );
        }

        if (GUILayout.Button("Copy Info"))
        {
            ClientData copyClient = AmongUsClient.Instance.GetClientFromCharacter(target);
            string info;
            if (copyClient != null)
            {
                PlatformSpecificData platform = copyClient.PlatformData;
                bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;
                info = $"Name: {target.Data.PlayerName}" +
                       $"\nFriendcode: {(streamerMode ? "REDACTED" : target.Data.FriendCode)}" +
                       $"\nPUID: {(streamerMode ? "REDACTED" : target.Data.Puid)}" +
                       $"\nLevel: {target.Data.PlayerLevel + 1}" +
                       $"\nDevice: {platform.Platform}" +
                       $"\nDeviceID: {copyClient.Id}" +
                       (target.OwnerId == AmongUsClient.Instance.HostId ? "\nHost: true" : "");
            }
            else
            {
                info = $"Name: {target.Data.PlayerName}\nIs Dummy: true";
            }
            GUIUtility.systemCopyBuffer = info;
            SkidMenu.notifications.Send("Copy Info", $"Copied info for {target.Data.PlayerName}");
        }

        if (GUILayout.Button("Save Info"))
        {
            SavedPlayerInfo.Name        = target.Data.PlayerName;
            SavedPlayerInfo.HatId       = target.Data.DefaultOutfit.HatId;
            SavedPlayerInfo.SkinId      = target.Data.DefaultOutfit.SkinId;
            SavedPlayerInfo.VisorId     = target.Data.DefaultOutfit.VisorId;
            SavedPlayerInfo.PetId       = target.Data.DefaultOutfit.PetId;
            SavedPlayerInfo.NameplateId = target.Data.DefaultOutfit.NamePlateId;
            SavedPlayerInfo.ColorId     = target.Data.DefaultOutfit.ColorId;
            SavedPlayerInfo.Level    = (int)target.Data.PlayerLevel;
            try { SavedPlayerInfo.Platform = Utils.PlatformTypeToString(AmongUsClient.Instance.GetClientFromCharacter(target).PlatformData.Platform); } catch { SavedPlayerInfo.Platform = ""; }
            SavedPlayerInfo.HasData  = true;
            SavedPlayerInfo.SaveToDisk(target.Data.PlayerName);
            SkidMenu.notifications.Send("Save Info", $"Saved {target.Data.PlayerName}'s info", 4f);
        }

        GUILayout.BeginHorizontal();
        bool isWhisperTarget = features.Whisper.IsArmed(target);
        if (_playerButtonTex == null)
            _playerButtonTex = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
        if (_watchButtonStyle == null)
        {
            _watchButtonStyle = new GUIStyle(GUI.skin.button);
            _watchButtonStyle.normal.background = _playerButtonTex;
            _watchButtonStyle.hover.background  = _playerButtonTex;
            _watchButtonStyle.active.background = _playerButtonTex;
        }
        var whisperPrevBg = GUI.backgroundColor;
        GUI.backgroundColor = isWhisperTarget ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.95f, 0.8f, 0.1f);
        if (GUILayout.Button(isWhisperTarget ? "Stop Whisper" : "Whisper", _watchButtonStyle))
            features.Whisper.Toggle(target);
        GUI.backgroundColor = whisperPrevBg;
        GUILayout.EndHorizontal();

        if (features.Whisper.Count > 0)
        {
            GUILayout.Label($"<color=#ffdd44>Whisper armed ({features.Whisper.Count}): {features.Whisper.Names()}</color>. All your chat goes only to them until you Stop Whisper.", GUIStylePreset.ModernLabel);
            if (GUILayout.Button("Stop All Whisper", GUILayout.Width(150)))
                features.Whisper.Clear();
        }

        GUILayout.BeginHorizontal();
        var prevBg = GUI.backgroundColor;
        bool isWatching = CheatToggles.spectate &&
                          Camera.main.gameObject.GetComponent<FollowerCamera>().Target == target;
        // fix 1+2: lazy-init watch style, reuse _playerButtonTex (same color)
        if (_playerButtonTex == null)
            _playerButtonTex = GUIStylePreset.MakeTex1x1(new Color(0.45f, 0.45f, 0.45f, 1f));
        if (_watchButtonStyle == null)
        {
            _watchButtonStyle = new GUIStyle(GUI.skin.button);
            _watchButtonStyle.normal.background = _playerButtonTex;
            _watchButtonStyle.hover.background  = _playerButtonTex;
            _watchButtonStyle.active.background = _playerButtonTex;
        }
        GUI.backgroundColor = isWatching ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.1f, 0.4f, 0.7f);
        if (GUILayout.Button(isWatching ? "Stop Watch" : "Watch", _watchButtonStyle))
        {
            if (isWatching) MalumPPMCheats.SpectateDirectly(PlayerControl.LocalPlayer);
            else MalumPPMCheats.SpectateDirectly(target);
        }
        GUI.backgroundColor = prevBg;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Teleport"))
            Teleporter.TeleportTo(target.transform.position);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Murder"))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                SkidMenu.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are host so we are using the MurderPlayer RPC");
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
            }
            else
            {
                SkidMenu.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are not the host so we have to use the CheckMurder RPC");
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            }
        }

        if (GUILayout.Button("Telemurder"))
            PlayerControl.LocalPlayer.StartCoroutine(TeleMurder(target).WrapToIl2Cpp());

        bool canShapeshift = PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Shapeshifter || AmongUsClient.Instance.AmHost;
        bool canProtect    = PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.GuardianAngel || AmongUsClient.Instance.AmHost;

        GUI.enabled = canShapeshift;
        if (GUILayout.Button("Shapeshift Into"))
            PlayerControl.LocalPlayer.RpcShapeshift(target, !CheatToggles.noShapeshiftAnim);
        GUI.enabled = true;

        GUI.enabled = canProtect;
        if (GUILayout.Button("Protect"))
            PlayerControl.LocalPlayer.RpcProtectPlayer(target, PlayerControl.LocalPlayer.cosmetics.ColorId);
        GUI.enabled = true;

        if (GUILayout.Button("Reveal Role in Chat"))
        {
            string msg = $"{target.Data.PlayerName} is a {target.Data.RoleType}";
            var chat = DestroyableSingleton<HudManager>.Instance?.Chat;
            if (chat != null) { chat.freeChatField.textArea.SetText(msg, string.Empty); chat.SendFreeChat(); }
        }

        if (GUILayout.Button("Copy Avatar"))
        {
            var o = target.CurrentOutfit;
            if (AmongUsClient.Instance.AmHost || !Utilities.IsColorTaken(o.ColorId))
                OutfitBypass.SetColor(o.ColorId);
            else
                OutfitBypass.SetColor(Utilities.GetFreeColor());
            PlayerControl.LocalPlayer.RpcSetHat(o.HatId);
            PlayerControl.LocalPlayer.RpcSetSkin(o.SkinId);
            PlayerControl.LocalPlayer.RpcSetVisor(o.VisorId);
            PlayerControl.LocalPlayer.RpcSetPet(o.PetId);
            PlayerControl.LocalPlayer.RpcSetNamePlate(o.NamePlateId);
        }

        if (GUILayout.Button("Copy Player"))
        {
            var o      = target.CurrentOutfit;
            var client = AmongUsClient.Instance.GetClientFromCharacter(target);
            var lp     = PlayerControl.LocalPlayer;
            var steps  = new System.Collections.Generic.List<System.Action>();
            if (AmongUsClient.Instance.AmHost || !Utilities.IsColorTaken(o.ColorId))
                steps.Add(() => OutfitBypass.SetColor(o.ColorId));
            else
                steps.Add(() => OutfitBypass.SetColor(Utilities.GetFreeColor()));
            steps.Add(() => lp.RpcSetHat(o.HatId));
            steps.Add(() => lp.RpcSetSkin(o.SkinId));
            steps.Add(() => lp.RpcSetVisor(o.VisorId));
            steps.Add(() => lp.RpcSetPet(o.PetId));
            steps.Add(() => lp.RpcSetNamePlate(o.NamePlateId));
            if (client != null)
            {
                uint lvl = target.Data.PlayerLevel;
                steps.Add(() => lp.RpcSetLevel(lvl));
                string platform = Utils.PlatformTypeToString(client.PlatformData.Platform);
                steps.Add(() => { SkidMenu.spoofPlatform.Value = platform; });
            }
            SkidMenu.routines.fullyRandomize.Schedule(steps, SkidMenu.frRpcDelay.Value);
        }

        if (GUILayout.Button("Report Body"))
            AttemptReportBody(target);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Vote"))
        {
            if (MeetingHud.Instance == null)
                SkidMenu.notifications.Send("Vote", "No active meeting.");
            else
                MeetingHud.Instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
        }
        if (GUILayout.Button("Ban Exploit"))
            BanHandler.BanPlayer(target);
        if (GUILayout.Button("Votekick"))
            VotekickHandler.VotekickPlayer(target);
        if (GUILayout.Button("Vent Kick"))
            VentKickTab.VentKick(target);
        GUILayout.EndHorizontal();


        int maxVent = ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10;
        if (_selectedVent > maxVent) _selectedVent = maxVent;
        GUILayout.Label($"Teleport player to vent: {_selectedVent}");
        _selectedVent = (int)GUILayout.HorizontalSlider(_selectedVent, 0, maxVent);
        if (GUILayout.Button("Teleport to Vent"))
            features.Troll.TeleportToVent(target, _selectedVent);
        GUILayout.Space(5);
        GUILayout.Label("Host Only Features:" + (AmongUsClient.Instance.AmHost ? "" : "\n(Using these will get you kicked!)"));

        if (GUILayout.Button("Force Meeting As"))
        {
            if (Utils.isHost)
            {
                Utilities.OpenMeeting(target, null);
            } else
            {
                SkidMenu.notifications.Send("Meeting Forcer", "This is a host-only cheat.");
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Force All Votes To"))
        {
            if (MeetingHud.Instance == null)
               {
                   SkidMenu.notifications.Send("Vote Forcer", "This option can only be used when there is an active meeting.");
            }
            else if (!Utils.isHost)
            {
                SkidMenu.notifications.Send("Vote Forcer", "This is a host-only cheat.");
            }
            else
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    PlayerVoteArea votingArea = MeetingHud.Instance.playerStates[player.PlayerId];
                    votingArea.SetVote(target.PlayerId);
                }

                MeetingHud.Instance.SetDirtyBit(1);
                MeetingHud.Instance.CheckForEndVoting();
            }
        }

        if (GUILayout.Button("Eject"))
        {
            if (!Utils.isHost)
            {
                SkidMenu.notifications.Send("Eject", "This is a host-only cheat.");
            } else
            {
                if (MeetingHud.Instance == null)
                {
                    MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
                    AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, SpawnFlags.None);
                }

                MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();
                MeetingHud.Instance.RpcVotingComplete(votes, target.Data, false);
                MeetingHud.Instance.RpcClose();
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Frame Shapeshift"))
        {
            if (!Utils.isHost)
                SkidMenu.notifications.Send("Frame Shapesift", "This is a host-only cheat.");
            else
                target.StartCoroutine(AttemptShapeshiftFrame(target).WrapToIl2Cpp());
        }

        if (GUILayout.Button("Frame for Killing All"))
        {
            if (!Utils.isHost)
                SkidMenu.notifications.Send("Framer", "This is a host-only cheat.");
            else
                AmongUsClient.Instance.StartCoroutine(AttemptFrameForKillingAll(target).WrapToIl2Cpp());
        }

        if (GUILayout.Button("Shapeshift Everyone Into"))
        {
            if (!Utils.isHost)
                SkidMenu.notifications.Send("Shapeshift Everyone", "This is a host-only cheat.");
            else
                AmongUsClient.Instance?.StartCoroutine(ShapeshiftEveryoneInto(target).WrapToIl2Cpp());
        }


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Flood Player with Tasks"))
        {
            if (!Utils.isHost)
            {
                SkidMenu.notifications.Send("Task Flooder", "This is a host-only cheat.");
            }
            else
            {
                byte[] taskIds = new byte[255];
                for (byte i = 0; i < 255; i++)
                {
                    taskIds[i] = i;
                }
                target.Data.RpcSetTasks(taskIds);
            }
        }

        if (GUILayout.Button("Clear Tasks"))
        {
            if (!Utils.isHost)
            {
                SkidMenu.notifications.Send("Clear Tasks", "This is a host-only cheat.");
            } else
            {
                target.Data.RpcSetTasks(Array.Empty<byte>());
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Kick"))
        {
            if (!Utils.isHost) SkidMenu.notifications.Send("Kick", "Host only.");
            else AmongUsClient.Instance.KickPlayer(target.OwnerId, false);
        }
        if (GUILayout.Button("Error Kick"))
        {
            if (!Utils.isHost) SkidMenu.notifications.Send("Error Kick", "Host only.");
            else if (LobbyBehaviour.Instance != null) AmongUsClient.Instance.KickPlayer(target.OwnerId, false);
            else AmongUsClient.Instance.SendLateRejection(target.OwnerId, DisconnectReasons.ClientTimeout);
        }
        if (GUILayout.Button("Ban"))
        {
            if (!Utils.isHost) SkidMenu.notifications.Send("Ban", "Host only.");
            else AmongUsClient.Instance.KickPlayer(target.OwnerId, true);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Game Options Modifier:");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Blind"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.CrewLightMod, -1.0f);
            gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, -1.0f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        if (GUILayout.Button("Fullbright"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.CrewLightMod, 1000f);
            gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, 1000f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Slow Speed"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        if (GUILayout.Button("Super Speed"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 3.0f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset to Defaults"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        GUILayout.Space(5);
        GUILayout.Label($"Change color to: {_selectedColor}");
        _selectedColor = (CrewmateColor)GUILayout.HorizontalSlider((float)_selectedColor, 0, 17);

        if (GUILayout.Button("Set Color"))
        {
            target.RpcSetColor((byte)_selectedColor);
        }

        GUILayout.Space(10);
        GUILayout.Label("Player History", GUIStylePreset.TabSubtitle);
        PlayerTracker.History.TryGetValue(target.PlayerId, out var history);
        int currentHistoryCount = history?.Count ?? 0;
        // fix 6: only rebuild history string when target changes or new entries added
        if (target != _lastInfoTarget || currentHistoryCount != _lastHistoryCount)
        {
            _lastHistoryCount = currentHistoryCount;
            if (history != null && history.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = history.Count - 1; i >= 0; i--)
                    sb.AppendLine(history[i]);
                _cachedHistoryString = sb.ToString();
            }
            else
                _cachedHistoryString = null;
        }
        if (_cachedHistoryString != null)
            GUILayout.Label(_cachedHistoryString, GUIStylePreset.ModernLabel);
        else
            GUILayout.Label("<color=#666666>No activity recorded yet.</color>", GUIStylePreset.ModernLabel);
    }

    private static void AttemptReportBody(PlayerControl target)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            SkidMenu.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are the host so we directly use the StartMeeting RPC");
            Utilities.OpenMeeting(PlayerControl.LocalPlayer, target.Data);
            return;
        }

        SkidMenu.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are not the host so we have to use the ReportDeadBody RPC");

        bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();
        if (hasAnticheat)
        {
             if (LobbyBehaviour.Instance != null)
            {
                SkidMenu.notifications.Send("Report Body", "The game must have started for this option to work.");
                return;
            }

             if (!target.Data.IsDead)
            {
                SkidMenu.notifications.Send("Report Body", "You can only report bodies of players who have died in this round.");
                return;
            }

            bool bodyExists = false;
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(0, 0), 99999f, Constants.PlayersOnlyMask))
            {
                if (collider.tag != "DeadBody") continue;

                DeadBody bodyComponent = collider.GetComponent<DeadBody>();
                if (bodyComponent && bodyComponent.ParentId == target.PlayerId)
                {
                    bodyExists = true;
                    break;
                }
            }

             if (!bodyExists)
            {
                SkidMenu.notifications.Send("Report Body", "Unable to find a dead body for this player, you can only report a player's body if they have died this round and their body has not dissolved.");
                return;
            }
        }

        if (!ViperBodies.CanReport(target.PlayerId))
        {
            SkidMenu.notifications.Send("Report Body", $"{target.Data.PlayerName}'s body has dissolved, it can't be reported.");
            return;
        }

        SkidMenu.Log.LogInfo($"All checks passed, we are able to report {target.Data.PlayerName}'s body.");

        PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
    }

    public static IEnumerator TeleMurder(PlayerControl target)
    {
        Vector2 savedPos = PlayerControl.LocalPlayer.NetTransform.transform.position;
        if (AmongUsClient.Instance.AmHost)
            PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
        else
            PlayerControl.LocalPlayer.CmdCheckMurder(target);
        float elapsed = 0f;
        float duration = Mathf.Max((AmongUsClient.Instance.Ping + 26) / 1000f, 0.252f);
        while (elapsed < duration)
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(savedPos);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private static IEnumerator ShapeshiftEveryoneInto(PlayerControl target)
    {
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc == target) continue;
            pc.RpcShapeshift(target, true);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private static IEnumerator AttemptShapeshiftFrame(PlayerControl target)
    {
        bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();

         if (ShipStatus.Instance == null && hasAnticheat)
        {
            SkidMenu.notifications.Send("Framer", "The game must have started for this option to work.");
            yield break;
        }

        PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);

        if (target.Data.RoleType != RoleTypes.Shapeshifter && hasAnticheat)
        {
            RoleTypes currentRole = target.Data.RoleType;

            target.RpcSetRole(RoleTypes.Shapeshifter, true);
            yield return Effects.Wait(0.5f);
            target.RpcShapeshift(randomPl, true);
            target.RpcSetRole(currentRole, true);
        }
        else
        {
            target.RpcShapeshift(randomPl, true);
        }
    }

    private static IEnumerator AttemptFrameForKillingAll(PlayerControl target)
    {
        bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();
        if (hasAnticheat && !AmongUsClient.Instance.AmHost)
        {
            SkidMenu.notifications.Send("Framer", "You must be host to use this option.");
            yield break;
        }
        if (hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            SkidMenu.notifications.Send("Framer", "The game must have started for this option to work.");
            yield break;
        }
        features.Host.DisableGameEnd.Enabled = true;
        if (target != PlayerControl.LocalPlayer)
        {
            PlayerControl.LocalPlayer.RpcShapeshift(target, false);
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == target) continue;
            PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
        }
        yield return Effects.Wait(3.0f);
        features.Host.DisableGameEnd.Enabled = false;
        SkidMenu.notifications.Send("Framer", $"Framed {target.Data.PlayerName} for killing all players!");
    }
}

public static class PlayersSection
{
    public static PlayerControl selectedPlayer;
}



