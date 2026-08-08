using UnityEngine;
using Sentry.Internal.Extensions;

namespace SkidMenu;
public static class MalumESP
{
    private static bool _freecamActive;
    private static bool _resolutionChangeNeeded;
    private static float _targetZoom = 3f;
    private static Vector3 _freecamTargetPos;

    public static float ZoomScrollSpeed = 1f;
    public static float ZoomSmoothness  = 0f;
    public static float ZoomMaxDistance = 20f;
    public static float ZoomMinDistance = 3f;
    public static float FreecamSpeed      = 10f;
    public static float FreecamSmoothness = 0f;

    // Nametag cache — keyed by PlayerId, invalidated every 0.1s
    private static readonly System.Collections.Generic.Dictionary<byte, string> _nametagCache = new();
    private static float _nametagTimer = 0f;
    private const float NametagRefreshInterval = 0.1f;
    private static int _cachedLineCount = 0;
    private static FollowerCamera _followerCamera;

    public static void InvalidateNametagCache()
    {
        _nametagTimer += Time.deltaTime;
        if (_nametagTimer >= NametagRefreshInterval)
        {
            _nametagTimer = 0f;
            _nametagCache.Clear();
            ESPContexts.UpdateContext();
            _cachedLineCount = ComputeNametagLineCount();
        }
    }

    private static FollowerCamera GetFollowerCamera()
    {
        if (_followerCamera == null)
            _followerCamera = Camera.main.gameObject.GetComponent<FollowerCamera>();
        return _followerCamera;
    }

    public static bool IsCursorOverSkidMenu()
    {
        if (!(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen.Value)) return false;
        Vector2 guiMouse = new(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        return MenuUI.LastWindowRect.Contains(guiMouse)
            || ChatUI.LastWindowRect.Contains(guiMouse)
            || ConsoleUI.LastWindowRect.Contains(guiMouse)
            || DoorsUI.LastWindowRect.Contains(guiMouse)
            || ProtectUI.LastWindowRect.Contains(guiMouse)
            || RolesUI.LastWindowRect.Contains(guiMouse)
            || TasksUI.LastWindowRect.Contains(guiMouse)
            || PlayerInfosUI.LastWindowRect.Contains(guiMouse);
    }

    public static void SporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.noShadows)
        {
            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool IsFullbrightActive()
    {
        return CheatToggles.noShadows || Camera.main.orthographicSize > 3f || GetFollowerCamera().Target != PlayerControl.LocalPlayer;
    }

    public static void ZoomOut(HudManager hudManager)
    {
        if (CheatToggles.zoomOut)
        {
            if (hudManager.Chat.IsOpenOrOpening || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
                GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

            _resolutionChangeNeeded = true;

            _targetZoom = Mathf.Clamp(_targetZoom, ZoomMinDistance, ZoomMaxDistance);

            if (!IsCursorOverSkidMenu() && Input.GetAxis("Mouse ScrollWheel") < 0f)
                _targetZoom = Mathf.Min(_targetZoom + ZoomScrollSpeed, ZoomMaxDistance);
            else if (!IsCursorOverSkidMenu() && Input.GetAxis("Mouse ScrollWheel") > 0f)
                _targetZoom = Mathf.Max(_targetZoom - ZoomScrollSpeed, ZoomMinDistance);

            float newSize = ZoomSmoothness > 0f
                ? Mathf.Lerp(Camera.main.orthographicSize, _targetZoom, Time.deltaTime * ZoomSmoothness)
                : _targetZoom;

            if (Mathf.Abs(newSize - Camera.main.orthographicSize) > 0.001f)
            {
                Camera.main.orthographicSize = newSize;
                hudManager.UICamera.orthographicSize = newSize;
                Utils.AdjustResolution();
            }
        }
        else
        {
            _targetZoom = 3f;
            Camera.main.orthographicSize = 3f;
            hudManager.UICamera.orthographicSize = 3f;

            if (_resolutionChangeNeeded)
            {
                Utils.AdjustResolution();
                _resolutionChangeNeeded = false;
            }
        }
    }

    private static int ComputeNametagLineCount()
    {
        int lines = 0;

        bool showRole = CheatToggles.espShowRole && ESPContexts.Allow(ESPContexts.ShowRole, false);
        bool showInfo = CheatToggles.espShowPlayerInfo && ESPContexts.Allow(ESPContexts.ShowInfo, false);

        if (showRole) lines++;
        if (CheatToggles.espKillCooldown && ESPContexts.Allow(ESPContexts.KillCooldown, false)) lines++;
        if (CheatToggles.espTasks && ESPContexts.Allow(ESPContexts.Tasks, false) && showRole) lines++;

        if (showInfo)
        {
            bool anyIdentity = (CheatToggles.espIsHost     && ESPContexts.Allow(ESPContexts.IsHost, false))
                             || (CheatToggles.espLevel       && ESPContexts.Allow(ESPContexts.Level, false))
                             || (CheatToggles.espPlatform    && ESPContexts.Allow(ESPContexts.Platform, false))
                             || (CheatToggles.espVotekicks   && ESPContexts.Allow(ESPContexts.Votekicks, false));

            bool anyAccount = (CheatToggles.espFriendCode  && ESPContexts.Allow(ESPContexts.FriendCode, false))
                             || (CheatToggles.espPuid        && ESPContexts.Allow(ESPContexts.Puid, false))
                             || (CheatToggles.espDeviceId    && ESPContexts.Allow(ESPContexts.DeviceId, false));

            if (anyIdentity) lines++;
            if (anyAccount)  lines++;
        }

        return lines;
    }

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            if (GameData.Instance == null) return;
            ESPContexts.UpdateContext();

            foreach (var playerState in meetingHud.playerStates)
            {
                if (playerState == null || playerState.NameText == null) continue;
                var data = GameData.Instance.GetPlayerById(playerState.TargetPlayerId);
                if (data == null || data.IsNull() || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName, false);

                if (features.Whisper.IsArmedById(data.PlayerId))
                    playerState.NameText.text += $" <color=#{features.Whisper.RoleHexFor(data)}>[WHISPER]</color>";

                // Bug 2: use espShowRole/espShowPlayerInfo, not the old seeRoles/seePlayerInfo fields
                bool showRole = CheatToggles.espShowRole;
                bool showInfo = CheatToggles.espShowPlayerInfo;

                if (showRole && showInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.06f, 0f);
                    playerState.NameText.transform.localScale    = new Vector3(0.6f, 0.6f, 0.6f);
                }
                else if (showRole || showInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.09f, -0.1f);
                    playerState.NameText.transform.localScale    = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else
                {
                    int lines = _cachedLineCount;
                    if (lines <= 0)
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, -0.001f, -0.1f);
                        playerState.NameText.transform.localScale    = new Vector3(0.9f, 1f, 1f);
                    }
                    else
                    {
                        // Bug 3: +1 accounts for the name line itself in the total text block height
                        float scale = Mathf.Max(0.55f, 1f - 0.08f * (lines + 1));
                        float yOff  = (lines + 1) * 0.032f * scale;
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, -0.001f + yOff, -0.1f);
                        playerState.NameText.transform.localScale    = new Vector3(scale, scale, scale);
                    }
                }
            }
        } catch { }
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        try
        {
            byte pid = playerPhysics.myPlayer.PlayerId;
            if (!_nametagCache.TryGetValue(pid, out string tag))
            {
                tag = Utils.GetNameTag(playerPhysics.myPlayer.Data, playerPhysics.myPlayer.CurrentOutfit.PlayerName);
                _nametagCache[pid] = tag;
            }
            if (features.Whisper.IsArmed(playerPhysics.myPlayer))
                tag += $" <color=#{features.Whisper.RoleHexFor(playerPhysics.myPlayer.Data)}>[WHISPER]</color>";
            playerPhysics.myPlayer.cosmetics.SetName(tag);

            if (CheatToggles.espShowRole && CheatToggles.espShowPlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.186f, 0f);
                playerPhysics.myPlayer.cosmetics.nameText.transform.localScale    = new Vector3(0.75f, 0.75f, 0.75f);
            }
            else if (CheatToggles.espShowRole || CheatToggles.espShowPlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.093f, 0f);
                playerPhysics.myPlayer.cosmetics.nameText.transform.localScale    = new Vector3(0.75f, 0.75f, 0.75f);
            }
            else if (_cachedLineCount <= 0)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                playerPhysics.myPlayer.cosmetics.nameText.transform.localScale    = Vector3.one;
            }
            else
            {
                // Bug 3: +1 for the name line itself
                float scale = Mathf.Max(0.65f, 1f - 0.04f * (_cachedLineCount + 1));
                float yOff  = (_cachedLineCount + 1) * 0.05f * scale;
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, yOff + 0.02f, 0f);
                playerPhysics.myPlayer.cosmetics.nameText.transform.localScale    = new Vector3(scale, scale, scale);
            }
        } catch { }
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            string newTag = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);
            if (chatBubble.NameText.text == newTag) return;

            float oldNameH = chatBubble.NameText.GetNotDumbRenderedHeight();
            chatBubble.NameText.text = newTag;
            chatBubble.NameText.ForceMeshUpdate(true, true);

            float newNameH = chatBubble.NameText.GetNotDumbRenderedHeight();
            float delta    = newNameH - oldNameH;

            if (delta > 0.001f)
            {
                var p = chatBubble.TextArea.transform.localPosition;
                chatBubble.TextArea.transform.localPosition = new Vector3(p.x, p.y - delta, p.z);
            }

            chatBubble.Background.size = new Vector2(5.52f, 0.2f + newNameH + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size   = chatBubble.Background.size - new Vector2(0f, 0.03f);
        } catch { }
    }

    public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try
        {
            if (playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }
        } catch { }
    }

    public static void FreecamCheat()
    {
        if (CheatToggles.freecam)
        {
            if (!_freecamActive)
            {
                var fc = GetFollowerCamera();
                _freecamTargetPos = Camera.main.transform.position;
                fc.enabled = false;
                fc.Target = null;
                _freecamActive = true;
            }

            PlayerControl.LocalPlayer.moveable = false;

            var input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
            _freecamTargetPos += input * FreecamSpeed * Time.deltaTime;

            Camera.main.transform.position = FreecamSmoothness > 0f
                ? Vector3.Lerp(Camera.main.transform.position, _freecamTargetPos, Time.deltaTime * FreecamSmoothness)
                : _freecamTargetPos;
        }
        else
        {
            if (!_freecamActive) return;
            var fc = GetFollowerCamera();
            PlayerControl.LocalPlayer.moveable = true;
            fc.enabled = true;
            fc.SetTarget(PlayerControl.LocalPlayer);
            _freecamActive = false;
        }
    }
}
