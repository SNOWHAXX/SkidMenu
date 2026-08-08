using UnityEngine;
using System.Globalization;
using SkidMenu.features;

namespace SkidMenu;

public class SettingsTab : ITab
{
    public string name => "Settings";

    private string _menuKeybindInput = "";
    private string _menuColorInput = "";
    private string _scaleHInput = "";
    private string _scaleVInput = "";
    private string _deviceIdInput = "";

    private bool _initialized = false;

    public void Draw()
    {
        if (!_initialized)
        {
            InitializeInputFields();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGUISettings();
        GUILayout.Space(15);
        DrawFpsSettings();
        GUILayout.Space(15);
        DrawPrivacySettings();
        GUILayout.Space(15);
        DrawLoggerSettings();

        GUILayout.EndVertical();
    }

    private void InitializeInputFields()
    {
        _menuKeybindInput = SkidMenu.menuKeybind.Value;
        _menuColorInput = SkidMenu.menuHtmlColor.Value;
        _scaleHInput = CheatToggles.menuScaleH.ToString(CultureInfo.InvariantCulture);
        _scaleVInput = CheatToggles.menuScaleV.ToString(CultureInfo.InvariantCulture);
        _fpsInput = CheatToggles.maxFpsValue.ToString();
        _deviceIdInput = CheatToggles.spoofDeviceIdCustom;
        if (CheatToggles.maxFpsEnabled) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = CheatToggles.maxFpsValue; }
    }

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 200, int height = 20)
    {
        CustomTextField.Draw(ref content, fieldKey, width, height);
    }

    private void DrawGUISettings()
    {
        GUILayout.Label("GUI Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Keybind:", GUILayout.Width(150));
        HandleCustomTextField(ref _menuKeybindInput, "menuKeybind", 150);
        if (GUILayout.Button("Save", GUILayout.Width(100))) SkidMenu.menuKeybind.Value = _menuKeybindInput;
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Space(5);
        SkidMenu.menuOpenOnMouse.Value = GUIStylePreset.CustomToggle(SkidMenu.menuOpenOnMouse.Value, " Open Menu on Mouse Position");
        GUILayout.Space(5);
        SkidMenu.autoLoadProfile.Value = GUIStylePreset.CustomToggle(SkidMenu.autoLoadProfile.Value, " Auto-Load Profile on Startup");

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleCustomTextField(ref _scaleHInput, "scaleH", 70);
        GUILayout.Label("%", GUILayout.Width(15));
        GUILayout.Space(10);
        GUILayout.Label("Vertical:", GUILayout.Width(55));
        HandleCustomTextField(ref _scaleVInput, "scaleV", 70);
        GUILayout.Label("%", GUILayout.Width(15));
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
        {
            if (float.TryParse(_scaleHInput, NumberStyles.Float, CultureInfo.InvariantCulture, out var sh))
                CheatToggles.menuScaleH = System.Math.Clamp(sh, 50f, 300f);
            if (float.TryParse(_scaleVInput, NumberStyles.Float, CultureInfo.InvariantCulture, out var sv))
                CheatToggles.menuScaleV = System.Math.Clamp(sv, 50f, 300f);
        }
        GUILayout.EndHorizontal();
    }

    private string _fpsInput = CheatToggles.maxFpsValue.ToString();

    private void DrawFpsSettings()
    {
        GUILayout.Label("FPS", GUIStylePreset.TabSubtitle);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Max FPS:", GUILayout.Width(70));
        HandleCustomTextField(ref _fpsInput, "maxFps", 55);
        GUILayout.Label("fps", GUILayout.Width(30));

        bool wasEnabled = CheatToggles.maxFpsEnabled;
        CheatToggles.maxFpsEnabled = GUIStylePreset.CustomToggle(CheatToggles.maxFpsEnabled, " Enabled", GUILayout.Width(80));
        if (CheatToggles.maxFpsEnabled != wasEnabled)
        {
            if (int.TryParse(_fpsInput, out var v))
                CheatToggles.maxFpsValue = System.Math.Clamp(v, 30, 999);
            FpsCapHelper.Apply();
        }

        GUILayout.EndHorizontal();

        bool enterPressed = CustomTextField.IsFocused("maxFps")
            && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
        if (enterPressed)
        {
            if (int.TryParse(_fpsInput, out var v2))
                CheatToggles.maxFpsValue = System.Math.Clamp(v2, 30, 999);
            FpsCapHelper.Apply();
            Event.current.Use();
        }
        GUILayout.Label($"  Current: {(CheatToggles.maxFpsEnabled ? $"locked to {CheatToggles.maxFpsValue}" : "default (VSync)")}");
    }

    private void DrawPrivacySettings()
    {
        GUILayout.Label("Privacy Settings", GUIStylePreset.TabSubtitle);

        CheatToggles.hideDeviceId = GUIStylePreset.CustomToggle(CheatToggles.hideDeviceId, " Hide Device ID");
        GUILayout.Space(3);
        CheatToggles.spoofDeviceId = GUIStylePreset.CustomToggle(CheatToggles.spoofDeviceId, " Spoof Device ID");
        if (CheatToggles.spoofDeviceId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom ID:", GUILayout.Width(70));
            HandleCustomTextField(ref _deviceIdInput, "deviceId", 150);
            if (GUILayout.Button("Save", GUILayout.Width(50)))
                CheatToggles.spoofDeviceIdCustom = _deviceIdInput;
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
        CheatToggles.disableTelemetry = GUIStylePreset.CustomToggle(CheatToggles.disableTelemetry, " Disable Telemetry");
        GUILayout.Space(3);
        CheatToggles.spoofTelemetry = GUIStylePreset.CustomToggle(CheatToggles.spoofTelemetry, " Spoof Telemetry");
        GUILayout.Space(10);

        if (GUILayout.Button("Open Config File", GUILayout.Width(200)))
            Utils.OpenConfigFile();

        GUILayout.Space(5);
        GUILayout.Label("For more advanced configuration options, click 'Open Config File'", GUIStylePreset.TabSubtitle);
    }

    private void DrawLoggerSettings()
    {
        GUILayout.Label("Logging", GUIStylePreset.TabSubtitle);

        SkidMenu.advancedLogging.Value = GUIStylePreset.CustomToggle(SkidMenu.advancedLogging.Value, " Advanced Logging");
        GUILayout.Space(3);
        GUILayout.Label("Logs everything to a dated file so crashes and bugs are easy to report", GUIStylePreset.TabSubtitle);

        if (SkidMenu.advancedLogging.Value)
        {
            GUILayout.Space(5);
            if (!string.IsNullOrEmpty(AdvancedLogger.CurrentLogFile))
            {
                GUILayout.Label($"Log file: {AdvancedLogger.CurrentLogFile}", GUIStylePreset.TabSubtitle);
                GUILayout.Space(3);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Log Folder", GUILayout.Width(200)))
                AdvancedLogger.OpenLogFolder();
            if (GUILayout.Button("Save Unity Log", GUILayout.Width(200)))
                AdvancedLogger.SaveUnityLog();
            GUILayout.EndHorizontal();
        }
    }
}
