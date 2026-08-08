using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;

public class ModesTab : ITab
{
    public string name => "Modes";

    private string _fpsCap    = null;
    private string _resoScale = null;

    private readonly Dictionary<string, Rect>  _fieldRects      = new();
    private readonly Dictionary<string, bool>  _focusedFields   = new();
    private readonly Dictionary<string, int>   _cursorPositions = new();
    private readonly Dictionary<string, float> _lastBlinkTime   = new();
    private readonly Dictionary<string, bool>  _cursorVisible   = new();
    private const float _cursorBlinkTime = 0.5f;

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 200, int height = 20)
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(width), GUILayout.Height(height));
        if (Event.current.type == EventType.Repaint)
            _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();
        if (!_focusedFields.ContainsKey(fieldKey)) _focusedFields[fieldKey] = false;
        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey))
        {
            if (_fieldRects[fieldKey].Contains(Event.current.mousePosition))
            { _focusedFields[fieldKey] = true; _lastBlinkTime[fieldKey] = Time.time; _cursorVisible[fieldKey] = true; Event.current.Use(); }
            else _focusedFields[fieldKey] = false;
        }
        if (_focusedFields[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (!_cursorPositions.ContainsKey(fieldKey)) _cursorPositions[fieldKey] = content.Length;
            int cp = System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length);
            if (Event.current.keyCode == KeyCode.Backspace) { if (cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; } Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Delete) { if (cp < content.Length) content = content.Substring(0, cp) + content.Substring(cp + 1); Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.LeftArrow)  { if (cp > 0) cp--; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.RightArrow) { if (cp < content.Length) cp++; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Home) { cp = 0; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.End)  { cp = content.Length; Event.current.Use(); }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            { content = content.Substring(0, cp) + Event.current.character + content.Substring(cp); cp++; Event.current.Use(); }
            _cursorPositions[fieldKey] = System.Math.Clamp(cp, 0, content.Length);
        }
        if (_fieldRects.ContainsKey(fieldKey))
        {
            GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content);
            if (_focusedFields.TryGetValue(fieldKey, out bool foc) && foc)
            {
                if (!_lastBlinkTime.ContainsKey(fieldKey)) _lastBlinkTime[fieldKey] = Time.time;
                if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime) { _cursorVisible[fieldKey] = !_cursorVisible[fieldKey]; _lastBlinkTime[fieldKey] = Time.time; }
                if (_cursorVisible.TryGetValue(fieldKey, out bool vis) && vis)
                {
                    int cp2 = _cursorPositions.ContainsKey(fieldKey) ? System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length) : content.Length;
                    Vector2 sz = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp2)));
                    GUI.Label(new Rect(_fieldRects[fieldKey].x + sz.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|");
                }
            }
        }
    }

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));
        DrawGeneral();
        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.stealthMode  = GUIStylePreset.CustomToggle(CheatToggles.stealthMode,  " Stealth Mode");
        CheatToggles.streamerMode = GUIStylePreset.CustomToggle(CheatToggles.streamerMode, " Streamer Mode");

        if (CheatToggles.streamerMode)
        {
            if (_fpsCap    == null || !int.TryParse(_fpsCap, out int syncFps)    || syncFps    != StreamerUI.FpsCap)         _fpsCap    = StreamerUI.FpsCap.ToString();
            if (_resoScale == null || !int.TryParse(_resoScale, out int syncReso) || syncReso   != StreamerUI.ResolutionScale) _resoScale = StreamerUI.ResolutionScale.ToString();

            GUILayout.Space(6);
            GUILayout.Label("Streamer Mode Settings", GUIStylePreset.TabSubtitle);
            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.Label("FPS Cap:", GUILayout.Width(90));
            HandleCustomTextField(ref _fpsCap, "fpsCap", 60);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                if (int.TryParse(_fpsCap, out int fps) && fps >= 5)
                    StreamerUI.FpsCap = fps;
                else
                    _fpsCap = StreamerUI.FpsCap.ToString();
            }
            GUILayout.Label($"(current: {StreamerUI.FpsCap})", GUILayout.Width(90));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Res Scale %:", GUILayout.Width(90));
            HandleCustomTextField(ref _resoScale, "resoScale", 60);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                if (int.TryParse(_resoScale, out int scale) && scale >= 1 && scale <= 100)
                    StreamerUI.ResolutionScale = scale; // applied in EnsureCaptureCamera via ResolutionScale
                else
                    _resoScale = StreamerUI.ResolutionScale.ToString();
            }
            GUILayout.Label($"(current: {StreamerUI.ResolutionScale}%)", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            try
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("  FPS: min 5    Res: 1-100%");
            }
            finally
            {
                GUI.color = Color.white;
            }
        }
    }
}
