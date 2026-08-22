using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


namespace SkidMenu;

public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 720;
    public static int windowWidth = 980;

    public static bool isGUIActive = false;
    private Rect _windowRect;
    public static MenuUI Instance { get; private set; }
    public Rect WindowRect { get => _windowRect; set => _windowRect = value; }
    public static Rect LastWindowRect;
    public static Rect PendingRect;
    public static bool PendingRectSet;
    private List<ITab> _tabs = new();
    private int _selectedTab;
    private Vector2 _tabScrollPosition = Vector2.zero;
    private Vector2 _contentScrollPosition = Vector2.zero;
    public static float hue; // For RGB mode
    private bool _wasInGameplay = false;
    private static Texture2D _overlayTex;
    private static bool _overlayLoaded = false;

    private void Start()
    {
        Instance = this;

        // Instantiate 2D area of MenuUI
        _windowRect = PendingRectSet ? PendingRect : new Rect(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
        PendingRectSet = false;

        if (!_overlayLoaded)
        {
            _overlayTex = GUIStylePreset.LoadEmbeddedTexture("image.png");
            _overlayLoaded = true;
        }
    }

    private bool _stylesInitialized = false;
    private static readonly string _windowTitle = $"SkidMenu {SkidMenu.hyperVersion}, {SkidMenu.hyperBuild} build.";
    private bool _tabsInitialized = false;
    private void EnsureTabsInitialized()
    {
        if (_tabsInitialized) return;
        _tabsInitialized = true;
        _tabs.Add(new InfoTab());
        _tabs.Add(new MovementTab());
        _tabs.Add(new SelfTab());
        _tabs.Add(new ESPTab());
        _tabs.Add(new RolesTab());
        _tabs.Add(new PlayersTab());
        _tabs.Add(new SabotageTab());
        _tabs.Add(new ChatTab());
        _tabs.Add(new AnimationsTab());
        _tabs.Add(new ConsoleTab());
        _tabs.Add(new HostOnlyTab());
        _tabs.Add(new HostSettingsTab());
        _tabs.Add(new AutoHostTab());
        _tabs.Add(new BanTab());
        _tabs.Add(new VentKickTab());
        _tabs.Add(new SchizoTab());
        _tabs.Add(new PassiveTab());
        _tabs.Add(new TrollTab());
        _tabs.Add(new VotekickTab());
        _tabs.Add(new ProtectionsTab());
        _tabs.Add(new AnticheatTab());
        _tabs.Add(new SpoofingTab());
        _tabs.Add(new DatingShitTab());
        _tabs.Add(new ModesTab());
        _tabs.Add(new ConfigTab());
        _tabs.Add(new SettingsTab());
    }

    public void InitStyles()
    {
        if (_stylesInitialized) return;
        _stylesInitialized = true;
        GUIStylePreset.Reset();
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 14;
        GUI.skin.window.padding = new RectOffset { left = 12, right = 12, top = 30, bottom = 12 };
        GUI.skin.window.margin = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
    }

    private void Update()
    {

        if (Input.GetKeyDown(Utils.StringToKeycode(SkidMenu.menuKeybind)))
        {
            // Enable or disable GUI with DELETE key
            isGUIActive = !isGUIActive;

            if (SkidMenu.menuOpenOnMouse)
            {
                // Teleport the window to the mouse for immediate use
                Vector2 mousePosition = Input.mousePosition;
                _windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.3f; // Adjust speed of color change, higher multiplier = faster
            if (hue > 1f) hue -= 1f; // Loop hue back to 0 when it exceeds 1
        }

        if (CheatToggles.stealthMode != SkidMenu.inStealthMode)
        {
            SkidMenu.inStealthMode = CheatToggles.stealthMode;
            _stylesInitialized = false;
            _customSkin = null;
            GUIStylePreset.Reset();
            _layoutCached = false;

            Scene scene = SceneManager.GetActiveScene();

            if (scene.name == "MainMenu" || scene.name == "MatchMaking")
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        if (CheatToggles.panicMode) Utils.Panic();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(SkidMenu.inStealthMode || SkidMenu.isPanicked);

        if (CheatToggles.openConfig)
        {
            Utils.OpenConfigFile();
            CheatToggles.openConfig = false;
        }

        // Check if round just ended and disable sabotage cheats
        bool currentlyInGameplay = Utils.isPlayer && Utils.isShip;
        if (_wasInGameplay && !currentlyInGameplay)
        {
            DisableSabotageCheats();
        }
        _wasInGameplay = currentlyInGameplay;
        if (CheatToggles.reloadConfig)
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.reloadConfig = false;
            FpsCapHelper.Apply();
        }

        if (CheatToggles.saveProfile)
        {
            CheatToggles.saveProfile = false; // Disable first to avoid saving it to profile
            CheatToggles.SaveTogglesToProfile();
        }

        if (CheatToggles.loadProfile)
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.loadProfile = false;
        }

        // Some cheats only work if the LocalPlayer exists, so they are turned off if it does not
        if(!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false;
            CheatToggles.setFakeAlive = false;
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false;
            CheatToggles.spectate = false;
            CheatToggles.freecam = false;
            CheatToggles.killPlayer = false;
            CheatToggles.callMeeting = false;


        }

        // Some cheats only work if the ship exists, so they are turned off if it does not
        if(!Utils.isShip)
        {
            CheatToggles.sabotageMap = false;
            CheatToggles.unfixableLights = false;
            CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false;
            CheatToggles.reportBody = false;
            CheatToggles.closeMeeting = false;
            CheatToggles.reactorSab = false;
            CheatToggles.oxygenSab = false;
            CheatToggles.commsSab = false;
            CheatToggles.elecSab = false;
            CheatToggles.mushSab = false;
            CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false;
            CheatToggles.spamCloseAllDoors = false;
            CheatToggles.spamOpenAllDoors = false;
            CheatToggles.spamSabotageAll = false;
            CheatToggles.spamFixAll = false;
            CheatToggles.mushSpore = false;

            MalumCheats.StopShipAnimCheats();
            MalumCheats.CleanUpInjectedTasks();
        }

        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.killPlayer = false;
            CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false;
            CheatToggles.killAnyone = false;
            CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false;
            CheatToggles.skipMeeting = false;
            CheatToggles.voteImmune = false;
            CheatToggles.judgeImmune = false;
            CheatToggles.noGameEnd = false;
            CheatToggles.showProtectMenu = false;
            CheatToggles.showRolesMenu = false;
            CheatToggles.noOptionsLimits = false;
        }

        // Some cheats only work if in a meeting, so they are turned off if it does not
        if (!Utils.isMeeting)
        {
            CheatToggles.skipMeeting = false;
            CheatToggles.ejectPlayer = false;
        }
    }

    public void OnGUI()
    {
        if (!isGUIActive || SkidMenu.isPanicked) return;

        EnsureTabsInitialized();
        InitStyles();

        GUI.backgroundColor = Color.white;

        Color prevColor = GUI.color;
        Matrix4x4 prevMatrix = GUI.matrix;

        GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, CheatToggles.menuOpacity);

        if (CheatToggles.menuScaleH != 100f || CheatToggles.menuScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.menuScaleH / 100f, CheatToggles.menuScaleV / 100f), pivot);
        }

        _windowRect = GUI.Window((int)WindowId.MenuUI, _windowRect, (GUI.WindowFunction)WindowFunction, _windowTitle, GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;

        GUI.color = prevColor;
        GUI.matrix = prevMatrix;
    }

    private void DisableSabotageCheats()
    {
        CheatToggles.sabotageMap = false;
        CheatToggles.unfixableLights = false;
        CheatToggles.commsSab = false;
        CheatToggles.elecSab = false;
        CheatToggles.reactorSab = false;
        CheatToggles.oxygenSab = false;
        CheatToggles.mushSab = false;
        CheatToggles.mushSpore = false;
        CheatToggles.closeAllDoors = false;
        CheatToggles.openAllDoors = false;
        CheatToggles.spamCloseAllDoors = false;
        CheatToggles.spamOpenAllDoors = false;
        CheatToggles.spamSabotageAll = false;
        CheatToggles.spamFixAll = false;
        SabotageTab.SpamIndividual.Clear();
    }

    private static GUISkin _customSkin;
    private static Texture2D _bgTexture;
    private static bool _bgLoaded = false;
    private static GUIStyle _bgBoxStyle;
    private static GUIStyle _overlayBoxStyle;

    private static Texture2D GetBgTexture()
    {
        if (_bgLoaded) return _bgTexture;
        _bgLoaded = true;
        try
        {
            _bgTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            using var ms = new System.IO.MemoryStream(EmbeddedImage.ImagePng);
            ImageConversion.LoadImage(_bgTexture, ms.ToArray(), false);
        }
        catch { }
        return _bgTexture;
    }
    private GUILayoutOption[] _tabColWidth;
    private GUILayoutOption[] _contentColWidth;
    private GUILayoutOption[] _tabBtnHeight;
    private GUILayoutOption[] _separatorOpts;
    private bool _layoutCached;

    private void EnsureLayoutCache()
    {
        if (_layoutCached) return;
        _tabColWidth     = new[] { GUILayout.Width(windowWidth * 0.2f) };
        _contentColWidth = new[] { GUILayout.Width(windowWidth * 0.8f) };
        _tabBtnHeight    = new[] { GUILayout.Height(38f) };
        _separatorOpts   = new[] { GUILayout.Height(2f), GUILayout.ExpandWidth(true) };
        _layoutCached = true;
    }

    public static void DrawBgAndOverlay(float w, float h)
    {
        if (_bgBoxStyle == null) { _bgBoxStyle = new GUIStyle(); _bgBoxStyle.normal.background = GetBgTexture(); }
        if (_bgBoxStyle.normal.background != null)
        {
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.09f);
            GUI.Box(new Rect(0, 0, w, h), GUIContent.none, _bgBoxStyle);
            GUI.color = prev;
        }
        if (_overlayTex != null)
        {
            if (_overlayBoxStyle == null) { _overlayBoxStyle = new GUIStyle(); _overlayBoxStyle.normal.background = _overlayTex; }
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.05f);
            GUI.Box(new Rect(0, 0, w, h), GUIContent.none, _overlayBoxStyle);
            GUI.color = prev;
        }
        GUI.Box(new Rect(0, 0, w, h), GUIContent.none, GUIStylePreset.CornerOverlayStyle);
    }

    public static GUISkin GetCustomSkin()
    {
        if (_customSkin != null)
        {
            var f = GUIStylePreset.FontRegular;
            if (f != null)
            {
                _customSkin.label.font  = f;
                _customSkin.button.font = f;
                _customSkin.toggle.font = f;
            }
            return _customSkin;
        }
        _customSkin = UnityEngine.Object.Instantiate(GUI.skin);
        _customSkin.button.fontSize        = 13;
        _customSkin.button.padding         = new RectOffset { left = 8, right = 8, top = 6, bottom = 7 };
        _customSkin.button.margin          = new RectOffset { left = 3, right = 3, top = 3, bottom = 3 };
        _customSkin.button.border          = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 };
        _customSkin.button.normal.background  = GUIStylePreset.MakeRoundedPanel(new Color(0.13f, 0.13f, 0.13f, 1f), 6, 0.5f, 0.05f, 0.10f);
        _customSkin.button.hover.background   = GUIStylePreset.MakeRoundedPanel(new Color(0.18f, 0.18f, 0.18f, 1f), 6, 0.5f, 0.05f, 0.08f);
        _customSkin.button.active.background  = GUIStylePreset.MakeRoundedPanel(new Color(0.23f, 0.23f, 0.23f, 1f), 6, 0.5f, 0.04f, 0.06f);
        _customSkin.button.normal.textColor   = new Color(0.88f, 0.88f, 0.90f, 1f);
        _customSkin.button.hover.textColor    = Color.white;
        _customSkin.button.active.textColor   = Color.white;
        _customSkin.toggle.fontSize              = 13;
        _customSkin.toggle.padding               = new RectOffset { left = 22, right = 5, top = 5, bottom = 5 };
        _customSkin.toggle.normal.textColor   = new Color(0.70f, 0.70f, 0.73f, 1f);
        _customSkin.toggle.onNormal.textColor  = new Color(0.93f, 0.93f, 0.95f, 1f);
        _customSkin.toggle.hover.textColor       = new Color(0.88f, 0.88f, 0.90f, 1f);
        _customSkin.toggle.onHover.textColor     = Color.white;
        _customSkin.toggle.alignment             = UnityEngine.TextAnchor.MiddleLeft;
        _customSkin.button.alignment             = UnityEngine.TextAnchor.MiddleCenter;

        _customSkin.label.font                      = GUIStylePreset.FontRegular;
        _customSkin.label.fontSize                  = 13;
        _customSkin.button.font                     = GUIStylePreset.FontRegular;
        _customSkin.toggle.font                     = GUIStylePreset.FontRegular;
        _customSkin.label.normal.textColor       = new Color(0.88f, 0.88f, 0.90f, 1f);
        _customSkin.horizontalSlider.normal.background      = GUIStylePreset.SliderTrack;
        _customSkin.horizontalSlider.hover.background       = GUIStylePreset.SliderTrack;
        _customSkin.horizontalSlider.active.background      = GUIStylePreset.SliderTrack;
        _customSkin.horizontalSlider.padding                = new RectOffset();
        _customSkin.horizontalSlider.margin                 = new RectOffset();
        _customSkin.horizontalSlider.border                 = new RectOffset { left = 12, right = 12, top = 12, bottom = 12 };
        _customSkin.horizontalSlider.fixedHeight            = 14f;
        _customSkin.horizontalSliderThumb.normal.background = GUIStylePreset.SliderThumb;
        _customSkin.horizontalSliderThumb.hover.background  = GUIStylePreset.SliderThumbHover;
        _customSkin.horizontalSliderThumb.active.background = GUIStylePreset.SliderThumbHover;
        _customSkin.horizontalSliderThumb.padding           = new RectOffset();
        _customSkin.horizontalSliderThumb.margin            = new RectOffset();
        _customSkin.horizontalSliderThumb.border            = new RectOffset();
        _customSkin.horizontalSliderThumb.fixedWidth        = 16f;
        _customSkin.horizontalSliderThumb.fixedHeight       = 16f;

        var scrollTrack = GUIStylePreset.MakeRoundedSolid(8, new Color(0.10f, 0.10f, 0.10f, 1f), 3, 0.3f);
        var scrollThumb = GUIStylePreset.MakeRoundedSolid(8, new Color(0.28f, 0.28f, 0.28f, 1f), 3, 0.4f);
        var scrollThumbHov = GUIStylePreset.MakeRoundedSolid(8, new Color(0.38f, 0.38f, 0.38f, 1f), 3, 0.4f);

        _customSkin.verticalScrollbar.normal.background        = scrollTrack;
        _customSkin.verticalScrollbar.fixedWidth               = 6f;
        _customSkin.verticalScrollbar.border                   = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 };
        _customSkin.verticalScrollbarThumb.normal.background   = scrollThumb;
        _customSkin.verticalScrollbarThumb.hover.background    = scrollThumbHov;
        _customSkin.verticalScrollbarThumb.active.background   = scrollThumbHov;
        _customSkin.verticalScrollbarThumb.fixedWidth          = 6f;
        _customSkin.verticalScrollbarThumb.border              = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 };
        _customSkin.verticalScrollbarUpButton.fixedHeight      = 0f;
        _customSkin.verticalScrollbarDownButton.fixedHeight    = 0f;

        _customSkin.horizontalScrollbar.normal.background      = scrollTrack;
        _customSkin.horizontalScrollbar.fixedHeight            = 6f;
        _customSkin.horizontalScrollbar.border                 = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 };
        _customSkin.horizontalScrollbarThumb.normal.background = scrollThumb;
        _customSkin.horizontalScrollbarThumb.hover.background  = scrollThumbHov;
        _customSkin.horizontalScrollbarThumb.active.background = scrollThumbHov;
        _customSkin.horizontalScrollbarThumb.fixedHeight       = 6f;
        _customSkin.horizontalScrollbarThumb.border            = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 };
        _customSkin.horizontalScrollbarLeftButton.fixedWidth   = 0f;
        _customSkin.horizontalScrollbarRightButton.fixedWidth  = 0f;

        return _customSkin;
    }

    public void WindowFunction(int windowID)
    {
        EnsureLayoutCache();
        var prevSkin = GUI.skin;
        GUI.skin = GetCustomSkin();
        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUIStylePreset.ModernBox, _tabColWidth);
        GUILayout.Space(2);
        _tabScrollPosition = GUILayout.BeginScrollView(_tabScrollPosition, false, true);
        for (var i = 0; i < _tabs.Count; i++)
        {
            var style = i == _selectedTab ? GUIStylePreset.TabButtonSelected : GUIStylePreset.TabButton;
            if (GUILayout.Button(_tabs[i].name, style, _tabBtnHeight))
            {
                _contentScrollPosition = Vector2.zero;
                _selectedTab = i;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Space(4);
        GUILayout.EndVertical();

        GUILayout.Space(10f);

        GUILayout.BeginVertical(GUIStylePreset.ModernBox, _contentColWidth);
        GUILayout.Space(2);
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);
            GUILayout.Box("", GUIStylePreset.Separator, _separatorOpts);
            GUILayout.Space(6);
            _contentScrollPosition = GUILayout.BeginScrollView(_contentScrollPosition, false, true);
            _tabs[_selectedTab].Draw();
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.skin = prevSkin;
        GUI.backgroundColor = Color.white;

        DrawBgAndOverlay(windowWidth, windowHeight);

        GUI.DragWindow();
    }
}


















