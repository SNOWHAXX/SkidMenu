using System;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Sentry.Internal.Extensions;
using System.Runtime.CompilerServices;
using AmongUs.InnerNet.GameDataMessages;
using Il2CppInterop.Runtime.Injection;

namespace SkidMenu;

public static class Utils
{
    public static bool isPastingInput;
    public static ReferenceDataManager ReferenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance; // Useful for getting full lists of all the Among Us cosmetics IDs
    public static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
    public static bool isShip => ShipStatus.Instance;
    public static bool isClient => AmongUsClient.Instance;
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool isPlayer => PlayerControl.LocalPlayer;
    public static bool isHost => (AmongUsClient.Instance && AmongUsClient.Instance.AmHost) || CheatToggles.bypassHostOnly;
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;
    public static bool isMeeting => MeetingHud.Instance;
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Voted or MeetingHud.MeetingStates.NotVoted;
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Proceeding;
    public static bool isExiling => ExileController.Instance && !(isAirshipMap && SpawnInMinigame.Instance.isActiveAndEnabled);
    public static bool isAnySabotageActive => ShipStatus.Instance && SabotageSystem.AnyActive;
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool isSkeldMap => (MapNames)GetCurrentMapID() == MapNames.Skeld;
    public static bool isMiraHQMap => (MapNames)GetCurrentMapID() == MapNames.MiraHQ;
    public static bool isPolusMap => (MapNames)GetCurrentMapID() == MapNames.Polus;
    public static bool isDleksMap => (MapNames)GetCurrentMapID() == MapNames.Dleks;
    public static bool isAirshipMap => (MapNames)GetCurrentMapID() == MapNames.Airship;
    public static bool isFungleMap => (MapNames)GetCurrentMapID() == MapNames.Fungle;
    public const float DefaultSpeed = 2.5f;
    public const float DefaultGhostSpeed = 3f;

    // Checks if LocalPlayer's speed is at its default value
    public static bool IsSpeedDefault(bool forGhost = false)
    {
        return forGhost ? Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, DefaultGhostSpeed) :
            Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.Speed, DefaultSpeed);
    }

    // Snaps LocalPlayer's speed to the default if within snapRange
    public static void SnapSpeedToDefault(float snapRange, bool forGhost = false)
    {
        if (forGhost)
        {
            PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed - DefaultGhostSpeed)
                                                             < snapRange ? DefaultGhostSpeed : PlayerControl.LocalPlayer.MyPhysics.GhostSpeed;
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed - DefaultSpeed)
                                                        < snapRange ? DefaultSpeed : PlayerControl.LocalPlayer.MyPhysics.Speed;
        }
    }

    // Gets a player's real name, display name, and whether they are disguised or not
    public static (string realName, string displayName, bool isDisguised) GetPlayerIdentity(PlayerControl player)
    {
        if (player == null || player.Data == null) return ("", "", false);

        var realName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>";
        var displayName = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.CurrentOutfit.PlayerName}</color>";
        var isDisguised = player.CurrentOutfit.PlayerName != player.Data.PlayerName;

        return (realName, displayName, isDisguised);
    }

    // Checks if player is currently vanished
    public static bool IsVanished(NetworkedPlayerInfo playerInfo)
    {
        PhantomRole phantomRole = playerInfo.Role as PhantomRole;

        if (phantomRole != null)
        {
            return phantomRole.fading || phantomRole.isInvisible;
        }

        return false;
    }

    // Checks whether a player is a valid target depending on whether killAnyone cheat is enabled or not
    public static bool IsValidTarget(NetworkedPlayerInfo target)
    {
        var killAnyoneRequirements = target && !target.Disconnected && target.Object && target.Object.Visible && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && target.Role;

        var fullRequirements = killAnyoneRequirements && !target.IsDead && !target.Object.inVent && !target.Object.inMovingPlat && target.Role.CanBeKilled;

        return CheatToggles.killAnyone ? killAnyoneRequirements : fullRequirements;
    }

    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null)
            {
                playerDataList.Add(player.Data);
            }
        }

        return playerDataList;
    }

    // Adjusts HUD resolution
    // Used to fix UI problems when zooming out
    public static void AdjustResolution()
    {
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }

    // Gets RoleBehaviour from a RoleType
    public static RoleBehaviour GetBehaviourByRoleType(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);
    }

    // Gets RoleBehaviour from a TeamType
    public static RoleBehaviour GetBehaviourByTeamType(RoleTeamTypes roleTeamType)
    {
        RoleTypes roleType = (RoleTypes)Enum.Parse(typeof(RoleTypes), roleTeamType.ToString(), true);
        RoleBehaviour role = GetBehaviourByRoleType(roleType);

        return role;
    }

    public static void ForceSetScanner(PlayerControl player, bool toggle)
    {
        var count = ++player.scannerCount;
        player.SetScanner(toggle, count);
        RpcSetScannerMessage rpcMessage = new(player.NetId, toggle, count);
        AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    public static void ForcePlayAnimation(byte animationType)
    {
        // PlayerControl.LocalPlayer.RpcPlayAnimation(1) wouldn't work if visual tasks are turned off
        // The below way makes sure it works regardless of visual task settings

        PlayerControl.LocalPlayer.PlayAnimation(animationType);
        RpcPlayAnimationMessage rpcMessage = new(PlayerControl.LocalPlayer.NetId, animationType);
        AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    // Coroutine to teleport the LocalPlayer to a position after a delay
    public static System.Collections.IEnumerator DelayedSnapTo(Vector2 position, float delay = 0.25f)
    {
        yield return new WaitForSeconds(delay);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
    }

    // Kills any player using RPC calls
    public static void MurderPlayer(PlayerControl target, MurderResultFlags result)
    {
        if (isFreePlay)
        {

            PlayerControl.LocalPlayer.MurderPlayer(target, MurderResultFlags.Succeeded);
            return;

        }

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            writer.WriteNetObject(target);
            writer.Write((int)result);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    // Minimum spacing between task-completion RPCs. BetterAmongUs flags a second task completed
    // less than 1.25s after a different one, so we buffer above that to stay under the radar.
    public const float TaskCompleteSpacing = 1.4f;

    public static void CompleteTask(PlayerTask task)
    {
        CompleteTask(PlayerControl.LocalPlayer, task);
    }

    public static void CompleteTask(PlayerControl player, PlayerTask task)
    {
        if (player == null || task == null || task.IsComplete) return;
        try
        {
            if (task is NormalPlayerTask npt && npt.taskStep < npt.MaxStep)
                npt.taskStep = npt.MaxStep;
        }
        catch { }
        player.RpcCompleteTask(task.Id);
    }

    // Opens Chat UI
    public static void OpenChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
            if (DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.gameObject.activeSelf)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.Close();
			}
        }

    }

    // LineRenderer cache � avoids GetComponent every frame
    // Full re-setup every 20 frames to catch color/death/material changes
    private static readonly System.Collections.Generic.Dictionary<int, LineRenderer> _lineRendererCache = new();
    private static int _tracerFrameCounter = 0;

    // Draws a tracer line between two GameObjects
    public static void DrawTracer(GameObject sourceObject, GameObject targetObject, Color color, float width = 0.007f)
    {
        int id = sourceObject.GetInstanceID();

        if (!_lineRendererCache.TryGetValue(id, out var lineRenderer) || !lineRenderer)
        {
            // Do not create LineRenderers when the tracer is invisible — avoids
            // attaching a component to every player while tracers are disabled.
            if (color.a <= 0f) return;
            lineRenderer = sourceObject.GetComponent<LineRenderer>() ?? sourceObject.AddComponent<LineRenderer>();
            lineRenderer.SetVertexCount(2);
            lineRenderer.SetWidth(width, width);
            lineRenderer.material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            _lineRendererCache[id] = lineRenderer;
        }

        lineRenderer.enabled = color.a > 0f;
        lineRenderer.SetColors(color, color);
        lineRenderer.SetPosition(0, sourceObject.transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }

    public static void HideTracer(GameObject sourceObject)
    {
        if (sourceObject == null) return;
        try
        {
            int id = sourceObject.GetInstanceID();
            if (_lineRendererCache.TryGetValue(id, out var lineRenderer) && lineRenderer)
                lineRenderer.enabled = false;
        }
        catch { }
    }

    public static void TickTracerFrame() => _tracerFrameCounter++;

    // Returns whether the ChatUI should be active or not
    public static bool IsChatUiActive()
    {
        try
        {
            return CheatToggles.enableChat || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }

    // Returns the max number of nested RPCs that can be in a GameData message
    // without getting kicked by AC
    public static int GetMaxRpcPackingLimit()
    {
        int num = 0;

        if (isClient && AmongUsClient.Instance.AmHost)
        {
            num = GameManager.Instance.LogicOptions.MaxPlayers * 2;
        }

        return 10 + num;
    }

    // Overloads target with set strength using Pet RPCs that
    // repeatedly restart the hand-petting animation, preventing old petting coroutines
    // from resolving
    public static void Overload(int targetId, int strength)
    {
        if (strength < 1) return;

        int maxRpc = GetMaxRpcPackingLimit();

        uint netId = PlayerControl.LocalPlayer.MyPhysics.NetId;
        byte rpcCall = (byte)RpcCalls.Pet;

        if (strength <= maxRpc)
        {
            // SendOption.None has no flow control, allowing for flooding without limits

            var messageWriter = MessageWriter.Get(SendOption.None);

            if (targetId < 0) // -1 = Broadcast
            {
                messageWriter.StartMessage(Tags.GameData);
                messageWriter.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                messageWriter.StartMessage(Tags.GameDataTo);
                messageWriter.Write(AmongUsClient.Instance.GameId);
                messageWriter.WritePacked(targetId);
            }

            for (var msg = 0; msg < strength; msg++)
            {
                messageWriter.StartMessage((byte)GameDataTypes.RpcFlag);

                messageWriter.WritePacked(netId);

                messageWriter.Write(rpcCall);

                // Use LocalPlayer.GetTruePosition() as the petting position
                // to minimize WalkPlayerTo delay and start the hand-petting animation immediately

                NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), messageWriter);

                // Pet position is decoded as (-50, -50) on target clients
                // This keeps the hand-petting animation out of normal view

                messageWriter.Write((ushort)0);

                messageWriter.Write((ushort)0);

                messageWriter.EndMessage();
            }

            messageWriter.EndMessage();

            AmongUsClient.Instance.connection.Send(messageWriter);

            messageWriter.Recycle();
        }
        else
        {
            int strengthGroups = strength / maxRpc;
            int remainder = strength % maxRpc;

            for (int group = 0; group < strengthGroups; group++)
            {
                Overload(targetId, maxRpc);
            }

            Overload(targetId, remainder);
        }
    }

    // Closes Chat UI
    public static void CloseChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }
    }

    // Gets the distance between two players
    public static float GetDistanceBetween(PlayerControl source, PlayerControl target)
    {

        Vector2 vector = target.GetTruePosition() - source.GetTruePosition();
		float magnitude = vector.magnitude;

        return magnitude;

    }

    // Returns a list of all the players in the game ordered from closest to farthest (from LocalPlayer by default)
    public static System.Collections.Generic.List<PlayerControl> GetPlayersSortedByDistance(PlayerControl source = null)
    {

        if (source.IsNull())
        {
            source = PlayerControl.LocalPlayer;
        }

        System.Collections.Generic.List<PlayerControl> outputList = new System.Collections.Generic.List<PlayerControl>();

        outputList.Clear();

        var allPlayers = GameData.Instance.AllPlayers;
        foreach (var playerInfo in allPlayers)
        {
            var player = playerInfo.Object;
            if (player)
            {
                outputList.Add(player);
            }
        }

        outputList = outputList.OrderBy(target => GetDistanceBetween(source, target)).ToList();

        return outputList.Count <= 0 ? null : outputList;
    }

    // Returns current map ID if available
    private static byte _cachedMapId = byte.MaxValue;
    private static int _mapIdFrame = 0;

    // Cached host � refreshed every frame. Client lookups cached 1s.
    private static ClientData _cachedHost;
    private static readonly System.Collections.Generic.Dictionary<int, ClientData> _clientCache = new();
    private static float _clientCacheTimer = 0f;
    private const float ClientCacheInterval = 1f;

    public static void TickClientCache()
    {
        _cachedHost = AmongUsClient.Instance?.GetHost();

        _clientCacheTimer += Time.deltaTime;
        if (_clientCacheTimer >= ClientCacheInterval)
        {
            _clientCacheTimer = 0f;
            _clientCache.Clear();
        }
    }

    private static ClientData GetCachedClient(NetworkedPlayerInfo playerInfo)
    {
        int key = playerInfo.ClientId;
        if (!_clientCache.TryGetValue(key, out var client))
        {
            client = AmongUsClient.Instance?.GetClientFromPlayerInfo(playerInfo);
            _clientCache[key] = client;
        }
        return client;
    }

    public static byte GetCurrentMapID()
    {
        if (++_mapIdFrame < 10) return _cachedMapId;
        _mapIdFrame = 0;

        if (isFreePlay)
        {
            if (isFreePlay)
            {
                _cachedMapId = (byte)AmongUsClient.Instance.TutorialMapId;
                return _cachedMapId;
            }
        }

        if (GameOptionsManager.Instance?.currentGameOptions != null)
        {
            _cachedMapId = GameOptionsManager.Instance.currentGameOptions.MapId;
            return _cachedMapId;
        }

        _cachedMapId = byte.MaxValue;
        return _cachedMapId;
    }

    // Gets SystemType of the room the player is currently in
    public static SystemTypes GetCurrentRoom()
    {
        return HudManager.Instance.roomTracker.LastRoom.RoomId;
    }

    // Gets the PlainShipRoom of room that overlaps specified position
    public static PlainShipRoom GetRoomFromPosition(Vector2 position)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.AllRooms == null) return null;

        int count = ship.AllRooms.Count;
        for (int i = 0; i < count; i++)
        {
            var room = ship.AllRooms[i];
            if (room == null || room.roomArea == null) continue;
            if (room.roomArea.OverlapPoint(position)) return room;
        }
        return null;
    }

    // Returns colored ping text with smooth lerped color transitions
    public static string GetColoredPingText(string pingText, int ping)
    {
        Color col;
        if (ping <= 0)
            col = new Color(0.72f, 0.72f, 0.72f);
        else if (ping < 100)
            col = Color.Lerp(new Color(0f, 1f, 1f), new Color(0f, 1f, 0f), 1f - (ping / 100f));
        else if (ping < 400)
            col = Color.Lerp(new Color(1f, 1f, 0f), new Color(0f, 1f, 0f), 1f - ((ping - 100f) / 300f));
        else
            col = Color.Lerp(new Color(1f, 0f, 0f), new Color(1f, 1f, 0f), Mathf.Clamp01(1f - ((ping - 400f) / 300f)));

        return $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>{pingText}</color>";
    }

    public static string GetColoredFpsText(int fps)
    {
        string text = $"FPS: {fps}";
        Color col;
        if (fps >= 100)
            col = Color.Lerp(new Color(0f, 1f, 1f), new Color(0f, 1f, 1f), 1f);
        else if (fps >= 60)
            col = Color.Lerp(new Color(0f, 1f, 0f), new Color(0f, 1f, 1f), (fps - 60f) / 40f);
        else if (fps >= 30)
            col = Color.Lerp(new Color(1f, 1f, 0f), new Color(0f, 1f, 0f), (fps - 30f) / 30f);
        else
            col = Color.Lerp(new Color(1f, 0f, 0f), new Color(1f, 1f, 0f), fps / 30f);

        return $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>{text}</color>";
    }

    // Returns the current approximate FPS
    public static int GetFps()
    {
        return (int)(1f / Time.unscaledDeltaTime);
    }

    // Gets a UnityEngine.KeyCode from a string
    public static KeyCode StringToKeycode(string keyCodeStr)
    {

        if(!string.IsNullOrEmpty(keyCodeStr)) // Empty strings are automatically invalid
        {
            try
            {
                // Case-insensitive parse of UnityEngine.KeyCode to check if string is valid
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeStr, true);

                return keyCode;

            }

            catch { }
        }

        return KeyCode.Delete; // If string is invalid, return Delete as the default key
    }

    // Gets a platform type from a string
    public static bool StringToPlatformType(string platformStr, out Platforms? platform)
    {
        if (!string.IsNullOrEmpty(platformStr)) // Empty strings are automatically invalid
        {
            if (platformStr.Equals("Starlight", System.StringComparison.OrdinalIgnoreCase))
            {
                platform = (Platforms)112;
                return true;
            }
            try
            {
                // Case-insensitive parse of Platforms from string (if it valid)
                platform = (Platforms)Enum.Parse(typeof(Platforms), platformStr, true);

                return true; // If platform type is valid, return false
            }catch{}
        }

        platform = null;
        return false; // If platform type is invalid, return false
    }

    public static string PlatformTypeToString(Platforms platform)
    {
        return platform switch
        {
            Platforms.StandaloneEpicPC => "Epic Games",
            Platforms.StandaloneSteamPC => "Steam",
            Platforms.StandaloneMac => "Mac",
            Platforms.StandaloneWin10 => "Microsoft Store",
            Platforms.StandaloneItch => "Itch.io",
            Platforms.IPhone => "iPhone / iPad",
            Platforms.Android => "Android",
            Platforms.Switch => "Nintendo Switch",
            Platforms.Xbox => "Xbox",
            Platforms.Playstation => "PlayStation",
            (Platforms)112 => "Starlight",
            _ => "Unknown"
        };
    }

    // Gets the name for a specified player's role as a string
    // Strings are automatically translated
    public static string GetRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;

        translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(GetBehaviourByTeamType(playerData.Role.TeamType).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        return translatedRole;
    }

    public static Color GetCustomRoleColor(NetworkedPlayerInfo playerInfo)
    {
        if (playerInfo?.Role == null) return Color.white;

        switch (playerInfo.Role.Role)
        {
            case RoleTypes.Shapeshifter:  return new Color(1f,    0.55f, 0f);
            case RoleTypes.Phantom:       return new Color(0.55f, 0f,    0f);
            case RoleTypes.Scientist:     return new Color(0f,    0f,    0.7f);
            case RoleTypes.Noisemaker:    return new Color(0.56f, 0.93f, 0.56f);
            case RoleTypes.Engineer:      return new Color(1f,    0.41f, 0.71f);
            case RoleTypes.Tracker:       return new Color(0.5f,  0f,    0.5f);
            case RoleTypes.Judge:         return new Color(0.25f, 0.5f,  1f);
            default:
                var rn = GetRoleName(playerInfo).ToLowerInvariant();
                if (rn.Contains("viper"))     return new Color(0f,   0.39f, 0f);
                if (rn.Contains("detective")) return new Color(1f,   0.84f, 0f);
                return playerInfo.Role.TeamColor;
        }
    }

    public static Color GetRoleDisplayColor(NetworkedPlayerInfo playerInfo)
    {
        if (CheatToggles.espShowRoleSimple)
        {
            bool imp = playerInfo?.Role != null && playerInfo.Role.IsImpostor;
            return imp ? new Color(1f, 0.5f, 0.5f) : new Color(0.6f, 0.8f, 1f);
        }
        return GetCustomRoleColor(playerInfo);
    }

    public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false)
    {
        var nameTag = playerName;

        // Only hard-bail on truly missing data; disconnected players still get ESP info
        if (playerInfo.IsNull() || playerInfo.Object.IsNull()) return nameTag;

        bool showRole    = CheatToggles.espShowRole;
        bool showInfo    = CheatToggles.espShowPlayerInfo;
        bool anyOn       = showRole || CheatToggles.espKillCooldown || CheatToggles.espTasks || showInfo;
        bool localGame   = isLocalGame;

        // Role colouring requires a valid role; skip role-specific work if role is null
        bool roleValid = !playerInfo.Role.IsNull();

        if (!anyOn)
        {
            if (isChat) return nameTag;
            if (roleValid && PlayerControl.LocalPlayer.Data.Role.NameColor == playerInfo.Role.NameColor)
                nameTag = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color>";
            return nameTag;
        }

        // Move client lookup after early returns — only pay the cost when we'll actually use it
        var player = GetCachedClient(playerInfo);
        var host   = _cachedHost ?? AmongUsClient.Instance?.GetHost();

        // Lazy role-colour hex, only paid when showRole is on and a role exists
        string roleColorHex = null;

        if (showRole && roleValid && ESPContexts.Allow(ESPContexts.ShowRole, isChat))
        {
            roleColorHex = ColorUtility.ToHtmlStringRGB(GetRoleDisplayColor(playerInfo));
            var (_, _, isDisguised) = GetPlayerIdentity(playerInfo.Object);
            if (isDisguised && !isChat)
            {
                var realColor = ColorUtility.ToHtmlStringRGB(playerInfo.Color);
                nameTag = $"{nameTag} <size=70%>(<color=#{realColor}>{playerInfo.PlayerName}</color>)</size>";
            }
            nameTag = $"<color=#{roleColorHex}>{nameTag}</color>";
        }

        // Build lines with StringBuilder — no List<string> allocations, no LINQ
        var sb = new System.Text.StringBuilder(128);

        if (showRole && roleValid && ESPContexts.Allow(ESPContexts.ShowRole, isChat))
        {
            if (sb.Length > 0) sb.Append("\r\n");
            sb.Append($"<color=#{roleColorHex}>{GetRoleName(playerInfo)}</color>");
        }

        if (CheatToggles.espKillCooldown && showRole && roleValid && ESPContexts.Allow(ESPContexts.KillCooldown, isChat) && playerInfo.Role.CanUseKillButton)
            try
            {
                if (sb.Length > 0) sb.Append("\r\n");
                sb.Append($"<color=#ff6666>CD: {KillCooldownTracker.GetRemainingCooldown(playerInfo.PlayerId):F1}s</color>");
            }
            catch { }

        if (CheatToggles.espTasks && showRole && roleValid && ESPContexts.Allow(ESPContexts.Tasks, isChat) && !playerInfo.Role.IsImpostor)
            try
            {
                int done = 0, total = 0;
                foreach (var t in playerInfo.Object.myTasks) { total++; if (t.IsComplete) done++; }
                if (sb.Length > 0) sb.Append("\r\n");
                sb.Append($"<color=#88ff88>Tasks: {done}/{total}</color>");
            }
            catch { }

        if (showInfo && ESPContexts.Allow(ESPContexts.ShowInfo, isChat))
        {
            var id = new System.Text.StringBuilder(96);
            var ac = new System.Text.StringBuilder(96);
            const string sep = " <color=#555>|</color> ";

            if (CheatToggles.espIsHost     && ESPContexts.Allow(ESPContexts.IsHost,    isChat) && player == host)
                { if (id.Length > 0) id.Append(sep); id.Append("<color=#ff4444>HOST</color>"); }
            string modUser = CheatToggles.espModUser && ESPContexts.Allow(ESPContexts.ModUser, isChat)
                ? anticheat.ModDetection.GetModNames(playerInfo.PlayerId) : null;
            if (!string.IsNullOrEmpty(modUser))
                { if (id.Length > 0) id.Append(sep); id.Append($"<color=#00ff88>{modUser}</color>"); }
            if (CheatToggles.espLevel      && ESPContexts.Allow(ESPContexts.Level,     isChat))
                { if (id.Length > 0) id.Append(sep); id.Append($"<color=#fb0>Lv:{playerInfo.PlayerLevel + 1}</color>"); }
            if (CheatToggles.espPlatform   && ESPContexts.Allow(ESPContexts.Platform,  isChat) && !localGame)
                try { if (id.Length > 0) id.Append(sep); id.Append($"<color=#fb0>{PlatformTypeToString(player.PlatformData.Platform)}</color>"); } catch { }
            if (CheatToggles.espVotekicks  && ESPContexts.Allow(ESPContexts.Votekicks, isChat))
                try { if (id.Length > 0) id.Append(sep); id.Append($"<color=#ff8800>VK:{(VotekickHandler.UniqueVoters.TryGetValue(playerInfo.ClientId, out var uvs) ? uvs.Count : 0)}/3</color>"); } catch { }

            if (CheatToggles.espFriendCode && ESPContexts.Allow(ESPContexts.FriendCode, isChat))
                try { if (ac.Length > 0) ac.Append(sep); ac.Append($"<color=#aaaaff>{playerInfo.FriendCode}</color>"); } catch { }
            if (CheatToggles.espPuid       && ESPContexts.Allow(ESPContexts.Puid,       isChat))
                try { if (ac.Length > 0) ac.Append(sep); ac.Append($"<color=#aaaaff>{playerInfo.Puid}</color>"); } catch { }
            if (CheatToggles.espDeviceId   && ESPContexts.Allow(ESPContexts.DeviceId,   isChat))
                try { if (ac.Length > 0) ac.Append(sep); ac.Append($"<color=#aaaaff>ID:{player.Id}</color>"); } catch { }

            if (id.Length > 0) { if (sb.Length > 0) sb.Append("\r\n"); sb.Append(id); }
            if (ac.Length > 0) { if (sb.Length > 0) sb.Append("\r\n"); sb.Append(ac); }
        }

        if (sb.Length == 0) return nameTag;

        if (isChat)
        {
            var inlineSb = new System.Text.StringBuilder(sb.Length + 32);
            int start = 0;
            string sbStr = sb.ToString();
            for (int i = 0; i <= sbStr.Length; i++)
            {
                if (i == sbStr.Length || (sbStr[i] == '\r' && i + 1 < sbStr.Length && sbStr[i+1] == '\n'))
                {
                    if (inlineSb.Length > 0) inlineSb.Append(" <color=#555>|</color> ");
                    inlineSb.Append("<size=60%>");
                    inlineSb.Append(sbStr, start, i - start);
                    inlineSb.Append("</size>");
                    start = i + 2;
                    i++;
                }
            }
            return $"<size=85%>{nameTag}</size> {inlineSb}";
        }

        var stackSb = new System.Text.StringBuilder(sb.Length + 32);
        {
            int start = 0;
            string sbStr = sb.ToString();
            for (int i = 0; i <= sbStr.Length; i++)
            {
                if (i == sbStr.Length || (sbStr[i] == '\r' && i + 1 < sbStr.Length && sbStr[i+1] == '\n'))
                {
                    if (stackSb.Length > 0) stackSb.Append("\r\n");
                    stackSb.Append("<size=70%>");
                    stackSb.Append(sbStr, start, i - start);
                    stackSb.Append("</size>");
                    start = i + 2;
                    i++;
                }
            }
        }
        return $"{stackSb}\r\n{nameTag}";
    }

    // Returns a player's NetworkedPlayerInfo from their client ID
    public static NetworkedPlayerInfo GetPlayerDataFromClientId(int clientId)
    {
        var players = PlayerControl.AllPlayerControls.ToArray();

        for (int i = 0; i < players.Count; i++)
		{   NetworkedPlayerInfo playerData = players[i].Data;

			if (playerData.ClientId == clientId)
			{
				return playerData;
			}
		}

        return null; // Returns null if no matching player is found
    }

    // Returns a random 1 - 12 characters long name
    public static string GetRandomName()
    {
        // Delegates to Among Us's built-in name randomizer.
        return DestroyableSingleton<AccountManager>.Instance.GetRandomName();
    }

    // Returns current AmongUsClient ping in ms
    public static int GetPing()
    {
        if (isClient && AmongUsClient.Instance.AmClient)
        {
            return AmongUsClient.Instance.Ping;
        }
        else
        {
            return 0; // Returns 0 if not connected to a game
        }
    }

    // Shows a custom popup ingame
    // Found here: https://github.com/NuclearPowered/Reactor/blob/6eb0bf19c30733b78532dada41db068b2b247742/Reactor/Networking/Patches/HttpPatches.cs
    public static void ShowPopup(string text)
    {
        var popup = UnityEngine.Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);

        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = background.size;
        size.x *= 2.5f;
        background.size = size;

        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(text);
    }

    public static void ShowNewPopup(string text)
    {
        DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(text);
    }

    // Loads sprites from manifest resources
    // Found here: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;

            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            SkidMenu.Log.LogError($"Failed to read Texture: {path}");
        }
        return null;
    }

    // Loads textures from manifest resources
    // Found here: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();

            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch
        {
            SkidMenu.Log.LogError($"Failed to read Texture: {path}");
        }
        return null;
    }

    // Opens the config file in the default text editor
    public static void OpenConfigFile()
    {
        var configFilePath = SkidMenu.ProfilePath;
        var configEditor = SkidMenu.configEditor;

        if (!string.IsNullOrWhiteSpace(configEditor))
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configEditor,
                        Arguments = configFilePath,
                        UseShellExecute = true
                        //Verb = "edit"
                    });
                }
                catch (Exception ex)
                {
                    SkidMenu.Log.LogError(ex.Message);
                }
            }
            else
            {
                SkidMenu.Log.LogError("Configuration file does not exist");
            }
        }
        else
        {
            SkidMenu.Log.LogError("Configuration editor not specified");
        }
    }

    public class PanicCleaner : MonoBehaviour
    {
        // Creates a PanicCleaner to unpatch Harmony
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("NexusMenu_PanicCleaner");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PanicCleaner>();
        }

        // Unpatching Harmony in handled in the next frame after creation
        // This allows some patches to run for a last time and finish properly
        private void LateUpdate()
        {
            try { Harmony.UnpatchID(SkidMenu.Id); } catch { }
            Destroy(gameObject);
        }
    }

    public static void Panic()
    {
        SkidMenu.isPanicked = true;

        CheatToggles.DisableAll();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = false;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "MainMenu" || scene.name == "MatchMaking")
        {
            SceneManager.LoadScene(scene.name);
        }

        UnityEngine.Object.Destroy(SkidMenu.menuUI);

        UnityEngine.Object.Destroy(SkidMenu.consoleUI);
        UnityEngine.Object.Destroy(SkidMenu.doorsUI);
        UnityEngine.Object.Destroy(SkidMenu.tasksUI);
        UnityEngine.Object.Destroy(SkidMenu.protectUI);
        UnityEngine.Object.Destroy(SkidMenu.streamerUI);
        // UnityEngine.Object.Destroy(SkidMenu.rolesUI);

        UnityEngine.Object.Destroy(SkidMenu.keybindListener);

        PanicCleaner.Create();
    }
}

