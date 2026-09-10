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

    // Nametag cache — keyed by PlayerId, invalidated every 0.3s
    private static readonly System.Collections.Generic.Dictionary<byte, string> _nametagCache = new();
    private static float _nametagTimer = 0f;
    private const float NametagRefreshInterval = 0.3f;
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
        if (!(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen)) return false;
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
            if (hudManager.Chat.IsOpenOrOpening || MatchInfoGuide.Instance.IsActive || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
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
                hudManager.UICamera.orthographicSize = HudTargetSize(newSize);
                Utils.AdjustResolution();
            }
            else
            {
                float wantUI = HudTargetSize(newSize);
                if (Mathf.Abs(wantUI - hudManager.UICamera.orthographicSize) > 0.001f)
                    hudManager.UICamera.orthographicSize = wantUI;
            }
        }
        else
        {
            _targetZoom = 3f;
            Camera.main.orthographicSize = 3f;

            if (_resolutionChangeNeeded)
            {
                Utils.AdjustResolution();
                _resolutionChangeNeeded = false;
            }
        }
    }

    private static float HudTargetSize(float worldSize)
    {
        if (!CheatToggles.resizeHUD) return worldSize;
        float size = worldSize * 100f / Mathf.Clamp(CheatToggles.hudScale, 25f, 200f);
        return Mathf.Clamp(size, 0.5f, 30f);
    }

    private static bool HudSizeCustomized()
    {
        return CheatToggles.resizeHUD && (
            Mathf.Abs(CheatToggles.hudScale - 100f) > 0.01f
            || Mathf.Abs(CheatToggles.hudSizeV - 100f) > 0.01f
            || Mathf.Abs(CheatToggles.hudOffsetX) > 0.01f
            || Mathf.Abs(CheatToggles.hudOffsetY) > 0.01f);
    }

    private static bool HudAlphaCustomized()
    {
        return CheatToggles.hideHUD
            || Mathf.Abs(CheatToggles.hudOpacity - 100f) > 0.01f;
    }

    private static bool HudCustomized()
    {
        return HudSizeCustomized() || HudAlphaCustomized();
    }

    private static int _lastScreenW;
    private static int _lastScreenH;
    private static float _cachedOx;
    private static float _cachedOy;

    public static void HudResize(HudManager hudManager)
    {
        if (hudManager == null || hudManager.UICamera == null) return;
        if (!HudCustomized())
        {
            if (_wasCustomized)
            {
                _wasCustomized = false;
                if (Mathf.Abs(3f - hudManager.UICamera.orthographicSize) > 0.001f)
                    hudManager.UICamera.orthographicSize = 3f;
                Rect full = new Rect(0f, 0f, 1f, 1f);
                if (hudManager.UICamera.rect != full)
                    hudManager.UICamera.rect = full;
                try
                {
                    hudManager.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer?.Data?.Role, true);
                }
                catch { }
                ApplyHudAlpha(hudManager, 1f);
                _fadedIds.Clear();
                _visibleSnapshotted = false;
                _visibleIds.Clear();
            }
            return;
        }
        _wasCustomized = true;

        var uiCam = hudManager.UICamera;
        float baseWorld = CheatToggles.zoomOut && Camera.main != null ? Camera.main.orthographicSize : 3f;
        float size = HudTargetSize(baseWorld);
        if (Mathf.Abs(size - uiCam.orthographicSize) > 0.001f)
            uiCam.orthographicSize = size;

        // V scale + offset via camera rect (offsets cached per resolution)
        if (Screen.width != _lastScreenW || Screen.height != _lastScreenH)
        {
            _lastScreenW = Screen.width;
            _lastScreenH = Screen.height;
            _cachedOx = CheatToggles.hudOffsetX / Mathf.Max(1f, Screen.width);
            _cachedOy = CheatToggles.hudOffsetY / Mathf.Max(1f, Screen.height);
        }
        else
        {
            _cachedOx = CheatToggles.hudOffsetX / Mathf.Max(1f, Screen.width);
            _cachedOy = CheatToggles.hudOffsetY / Mathf.Max(1f, Screen.height);
        }
        float h = HudSizeCustomized() ? Mathf.Clamp(CheatToggles.hudSizeV, 25f, 300f) / 100f : 1f;
        Rect want = HudSizeCustomized()
            ? new Rect(_cachedOx - (h - 1f) * 0.5f, (1f - h) * 0.5f + _cachedOy, 1f + (h - 1f), h)
            : new Rect(0f, 0f, 1f, 1f);
        if (uiCam.rect != want)
            uiCam.rect = want;

        // Hide toggle + opacity only under the master toggle
        if (CheatToggles.resizeHUD && HudAlphaCustomized())
        {
            float alpha = CheatToggles.hideHUD ? 0f : Mathf.Clamp01(CheatToggles.hudOpacity / 100f);
            ApplyHudAlpha(hudManager, alpha);
        }
    }

    private static readonly System.Collections.Generic.HashSet<int> _fadedIds = new();
    private static readonly System.Collections.Generic.HashSet<int> _visibleIds = new();
    private static bool _visibleSnapshotted;
    private static bool _wasCustomized;
    private static SpriteRenderer[] _hudSprites = System.Array.Empty<SpriteRenderer>();
    private static TMPro.TMP_Text[] _hudTexts = System.Array.Empty<TMPro.TMP_Text>();
    private static UnityEngine.UI.Image[] _hudImages = System.Array.Empty<UnityEngine.UI.Image>();
    private static float _hudCacheTimer;

    private static float _snapshotTimer;

    private static void RefreshHudCache(HudManager hudManager)
    {
        _hudCacheTimer += Time.unscaledDeltaTime;
        if (_hudCacheTimer < 1f && (_hudSprites.Length > 0 || _hudTexts.Length > 0 || _hudImages.Length > 0)) return;
        _hudCacheTimer = 0f;
        try
        {
            _hudSprites = hudManager.GetComponentsInChildren<SpriteRenderer>(true);
            _hudTexts = hudManager.GetComponentsInChildren<TMPro.TMP_Text>(true);
            _hudImages = hudManager.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        }
        catch { }
    }

    private static bool FadedAlready(Object o)
    {
        int id = o.GetInstanceID();
        if (_fadedIds.Contains(id)) return true;
        _fadedIds.Add(id);
        return false;
    }

    private static bool WasVisible(Object o)
    {
        return _visibleSnapshotted && _visibleIds.Contains(o.GetInstanceID());
    }

    private static void SnapshotVisible(HudManager hudManager)
    {
        _visibleIds.Clear();
        try
        {
            RefreshHudCache(hudManager);
            foreach (var r in _hudSprites)
            {
                if (r == null || !r.gameObject.activeInHierarchy || !r.enabled || r.color.a <= 0f) continue;
                _visibleIds.Add(r.GetInstanceID());
            }
            foreach (var t in _hudTexts)
            {
                if (t == null || !t.gameObject.activeInHierarchy || !t.enabled || t.color.a <= 0f) continue;
                _visibleIds.Add(t.GetInstanceID());
            }
            foreach (var im in _hudImages)
            {
                if (im == null || !im.gameObject.activeInHierarchy || !im.enabled || im.color.a <= 0f) continue;
                _visibleIds.Add(im.GetInstanceID());
            }
        }
        catch { }
        _visibleSnapshotted = true;
    }

    private static void ApplyHudAlpha(HudManager hudManager, float alpha)
    {
        try
        {
            if (!_visibleSnapshotted) SnapshotVisible(hudManager);
            _snapshotTimer += Time.unscaledDeltaTime;
            if (_snapshotTimer >= 5f)
            {
                _snapshotTimer = 0f;
                SnapshotVisible(hudManager);
            }
            RefreshHudCache(hudManager);
            bool restoring = Mathf.Abs(alpha - 1f) < 0.001f;
            foreach (var r in _hudSprites)
            {
                if (r == null || !WasVisible(r)) continue;
                if (restoring && !r.gameObject.activeInHierarchy) continue;
                if (r.color.a <= 0f && !FadedAlready(r)) continue;
                if (Mathf.Abs(r.color.a - alpha) < 0.001f) continue;
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }
            foreach (var t in _hudTexts)
            {
                if (t == null || !WasVisible(t)) continue;
                if (restoring && !t.gameObject.activeInHierarchy) continue;
                if (t.color.a <= 0f && !FadedAlready(t)) continue;
                if (Mathf.Abs(t.color.a - alpha) < 0.001f) continue;
                Color c = t.color;
                c.a = alpha;
                t.color = c;
            }
            foreach (var im in _hudImages)
            {
                if (im == null || !WasVisible(im)) continue;
                if (restoring && !im.gameObject.activeInHierarchy) continue;
                if (im.color.a <= 0f && !FadedAlready(im)) continue;
                if (Mathf.Abs(im.color.a - alpha) < 0.001f) continue;
                Color c = im.color;
                c.a = alpha;
                im.color = c;
            }
        }
        catch { }
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
                var data = GameData.Instance.GetPlayerById(playerState.PlayerId);
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
