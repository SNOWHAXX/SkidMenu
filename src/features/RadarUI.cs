using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu.features;

public class RadarUI : MonoBehaviour
{
    private const int WindowId = 843207;
    private const float ReferenceScale = 0.5f;

    private sealed class RadarMap
    {
        public int id;
        public string res;
        public string realisticRes;
        public float x;
        public float y;
        public float scale;
        public float rx;
        public float ry;
        public float artOx;
        public float artOy;
        public Texture2D tex;
        public GUIStyle style;
        public Texture2D realisticTex;
        public GUIStyle realisticStyle;
    }

    private static readonly RadarMap[] Maps =
    {
        new RadarMap { id = 0, res = "SkidMenu.Assets.radar_skeld.png", realisticRes = "SkidMenu.Assets.radar_realistic_skeld.png", x = 277f, y = 77f, scale = 11.5f, rx = 241f, ry = 77f, artOx = 0f, artOy = 0f },
        new RadarMap { id = 1, res = "SkidMenu.Assets.radar_mira_hq.png", realisticRes = "SkidMenu.Assets.radar_realistic_mira_hq.png", x = 115f, y = 240f, scale = 9.25f, rx = 115f, ry = 240f },
        new RadarMap { id = 2, res = "SkidMenu.Assets.radar_polus.png", realisticRes = "SkidMenu.Assets.radar_realistic_polus.png", x = 8f, y = 21f, scale = 10f, rx = 8f, ry = 21f },
        new RadarMap { id = 3, res = "SkidMenu.Assets.radar_skeld.png", realisticRes = "SkidMenu.Assets.radar_realistic_skeld.png", x = 277f, y = 77f, scale = 11.5f, rx = 277f, ry = 77f },
        new RadarMap { id = 4, res = "SkidMenu.Assets.radar_airship.png", realisticRes = "SkidMenu.Assets.radar_realistic_airship.png", x = 162f, y = 107f, scale = 6f, rx = 162f, ry = 107f },
        new RadarMap { id = 5, res = "SkidMenu.Assets.radar_fungle.png", realisticRes = "SkidMenu.Assets.radar_realistic_fungle.png", x = 237f, y = 140f, scale = 8.5f, rx = 237f, ry = 140f },
    };

    private static Rect _rect = new Rect(15f, 90f, 220f, 180f);
    private static GUIStyle _winStyle;
    private static GUIStyle _dotStyle;
    private static Texture2D _pixelTex;
    private static GUIStyle _pixelStyle;
    private static Texture2D _playerTex;
    private static Texture2D _visorTex;
    private static Texture2D _bodyTex;
    private static GUIStyle _playerStyle;
    private static GUIStyle _visorStyle;
    private static GUIStyle _bodyStyle;

    private static GUIStyle IconStyle(Texture2D tex, ref GUIStyle style)
    {
        if (tex == null) return null;
        if (style == null || style.normal.background != tex)
        {
            style = new GUIStyle(GUIStyle.none);
            style.normal.background = tex;
        }
        return style;
    }
    private static readonly List<Bounds> _worldRooms = new();
    private static Vector2 _worldMin;
    private static Vector2 _worldMax;
    private static int _worldMap = -1;
    private static float _nextTpAt;

    private void OnGUI()
    {
        if (!CheatToggles.radarShow) return;
        if ((!MenuUI.isGUIActive && !CheatToggles.radarShowNoMenu) || SkidMenu.isPanicked) return;
        if (!CanDraw()) return;

        RadarMap map = GetMap();
        if (map == null) return;

        InitGui();
        FitRect(map);
        Color old = GUI.color;
        try
        {
            GUI.color = Color.white;
            _rect = GUI.Window(WindowId, _rect, (GUI.WindowFunction)DrawWindow, GUIContent.none, _winStyle);
        }
        catch { }
        finally { GUI.color = old; }
        ClampRect();
    }

    private static bool CanDraw()
    {
        if (CheatToggles.radarHideMeeting && (MeetingHud.Instance != null || ExileController.Instance != null || IntroCutscene.Instance != null))
            return false;
        if (MapBehaviour.Instance != null && MapBehaviour.Instance.IsOpen)
            return false;
        if (PlayerControl.LocalPlayer == null || PlayerControl.AllPlayerControls == null)
            return false;
        try
        {
            return AmongUsClient.Instance != null &&
                (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined ||
                 AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ||
                 AmongUsClient.Instance.IsGameStarted);
        }
        catch { return false; }
    }

    private static int CurrentMapId()
    {
        try
        {
            MapNames map = Utilities.GetCurrentMap();
            return map switch
            {
                MapNames.Skeld => 0,
                MapNames.MiraHQ => 1,
                MapNames.Polus => 2,
                MapNames.Airship => 4,
                MapNames.Fungle => 5,
                _ => 0,
            };
        }
        catch { return 0; }
    }

    private static RadarMap GetMap()
    {
        int id = Mathf.Clamp(CurrentMapId(), 0, 5);
        foreach (RadarMap map in Maps)
        {
            if (map.id != id) continue;
            if (map.tex == null)
                map.tex = LoadTex(map.res);
            if (map.tex != null && map.style == null)
            {
                map.style = new GUIStyle(GUIStyle.none);
                map.style.normal.background = map.tex;
            }
            if (map.realisticTex == null && map.realisticRes != null)
                map.realisticTex = LoadTex(map.realisticRes);
            if (map.realisticTex != null && map.realisticStyle == null)
            {
                map.realisticStyle = new GUIStyle(GUIStyle.none);
                map.realisticStyle.normal.background = map.realisticTex;
            }
            return map.tex == null ? null : map;
        }
        return null;
    }

    private static Texture2D LoadTex(string res)
    {
        try
        {
            using var s = typeof(RadarUI).Assembly.GetManifestResourceStream(res);
            if (s == null) return null;
            byte[] buf = new byte[s.Length];
            s.Read(buf, 0, buf.Length);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(buf)) return null;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
        catch { return null; }
    }

    private static void InitGui()
    {
        _winStyle ??= new GUIStyle(GUIStyle.none);
        if (_dotStyle == null)
            _dotStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = false };
        if (_pixelTex == null)
        {
            _pixelTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _pixelTex.SetPixel(0, 0, Color.white);
            _pixelTex.Apply();
            _pixelTex.hideFlags = HideFlags.HideAndDontSave;
            _pixelStyle = new GUIStyle(GUIStyle.none);
            _pixelStyle.normal.background = _pixelTex;
        }
        if (CheatToggles.radarIcons)
        {
            _playerTex ??= LoadTex("SkidMenu.Assets.radar_player.png");
            _visorTex ??= LoadTex("SkidMenu.Assets.radar_player_visor.png");
            _bodyTex ??= LoadTex("SkidMenu.Assets.radar_dead_body.png");
        }
    }

    private static void FitRect(RadarMap map)
    {
        CheatToggles.radarScale = Mathf.Clamp(CheatToggles.radarScale, 0.65f, 1.6f);
        CheatToggles.radarAlpha = Mathf.Clamp(CheatToggles.radarAlpha, 0.2f, 1f);
        _rect.width = Mathf.Max(120f, map.tex.width * 0.5f * CheatToggles.radarScale + 10f);
        _rect.height = Mathf.Max(90f, map.tex.height * 0.5f * CheatToggles.radarScale + 10f);
    }

    private static void ClampRect()
    {
        _rect.x = Mathf.Clamp(_rect.x, 0f, Mathf.Max(0f, Screen.width - _rect.width));
        _rect.y = Mathf.Clamp(_rect.y, 0f, Mathf.Max(0f, Screen.height - _rect.height));
    }

    private static void DrawWindow(int id)
    {
        RadarMap map = GetMap();
        if (map == null) return;

        Rect img = MapRect(map);
        Color old = GUI.color;
        try
        {
            GUI.color = new Color(1f, 1f, 1f, CheatToggles.radarAlpha);
            if (map.style != null) GUI.Box(img, GUIContent.none, map.style);
            GUI.color = Color.white;

            if (CheatToggles.radarBorder)
            {
                Color accent = new Color(0.4f, 0.85f, 1f, 0.92f);
                Stroke(img, accent);
            }

            DrawPlayers(map);
            if (CheatToggles.radarBodies) DrawBodies(map);
            if (CheatToggles.radarClickTp) ClickTp(map);
            if (CheatToggles.radarClickDoors) ClickDoors(map);
        }
        catch { }
        finally { GUI.color = old; }

        if (!CheatToggles.radarLock)
            GUI.DragWindow(new Rect(0f, 0f, _rect.width, _rect.height));
    }

    private static Rect MapRect(RadarMap map)
    {
        return new Rect(5f, 5f, map.tex.width * 0.5f * CheatToggles.radarScale, map.tex.height * 0.5f * CheatToggles.radarScale);
    }

    private static void DrawPlayers(RadarMap map)
    {
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            try
            {
                if (pc == null || pc.Data == null || pc.Data.Disconnected) continue;
                if (pc.Data.IsDead && !CheatToggles.radarGhosts) continue;

                Vector2 p = RadarPoint(map, pc.GetTruePosition());
                if (!MapRect(map).Contains(p)) continue;

                Color c = PlayerColor(pc);
                if (CheatToggles.radarIcons)
                    Icon(p, c);
                else
                {
                    bool sq = pc.Data.Role != null && pc.Data.Role.IsImpostor;
                    Glyph(p, c, sq ? "■" : "●");
                }
            }
            catch { }
        }
    }

    private static void DrawBodies(RadarMap map)
    {
        DeadBody[] bodies = null;
        try { bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>(); } catch { }
        if (bodies == null) return;

        foreach (DeadBody body in bodies)
        {
            try
            {
                if (body == null) continue;
                Vector2 p = RadarPoint(map, body.TruePosition);
                if (!MapRect(map).Contains(p)) continue;
                if (CheatToggles.radarIcons && _bodyTex != null)
                    Icon(p, new Color(1f, 0.25f, 0.25f, 1f), _bodyTex);
                else
                    Glyph(p, new Color(1f, 0.25f, 0.25f, 1f), "✖");
            }
            catch { }
        }
    }

    private static Vector2 RadarPoint(RadarMap map, Vector2 pos)
    {
        Rect img = MapRect(map);
        float refW = Mathf.Max(1f, map.tex.width * ReferenceScale);
        float refH = Mathf.Max(1f, map.tex.height * ReferenceScale);
        float nativeX = map.x + pos.x * map.scale;
        float x = img.x + nativeX * (img.width / refW);
        float y = img.y + (map.y - pos.y * map.scale) * (img.height / refH);
        return new Vector2(x, y);
    }

    private static bool EnsureWorldRooms()
    {
        int map = CurrentMapId();
        if (_worldMap == map && _worldRooms.Count > 0) return true;

        _worldMap = map;
        _worldRooms.Clear();
        _worldMin = new Vector2(float.MaxValue, float.MaxValue);
        _worldMax = new Vector2(float.MinValue, float.MinValue);

        try
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllRooms == null) return false;
            foreach (PlainShipRoom room in ShipStatus.Instance.AllRooms)
            {
                Collider2D col = room != null ? room.roomArea : null;
                if (col == null) continue;
                Bounds bounds = col.bounds;
                if (bounds.size.x <= 0.01f || bounds.size.y <= 0.01f) continue;
                _worldRooms.Add(bounds);
                Grow(new Vector2(bounds.min.x, bounds.min.y));
                Grow(new Vector2(bounds.max.x, bounds.max.y));
            }
        }
        catch { _worldRooms.Clear(); return false; }

        if (_worldRooms.Count == 0 || _worldMin.x > _worldMax.x) return false;
        Vector2 size = _worldMax - _worldMin;
        Vector2 margin = size * 0.035f + Vector2.one * 0.35f;
        _worldMin -= margin;
        _worldMax += margin;
        return true;
    }

    private static void Grow(Vector2 point)
    {
        if (point.x < _worldMin.x) _worldMin.x = point.x;
        if (point.y < _worldMin.y) _worldMin.y = point.y;
        if (point.x > _worldMax.x) _worldMax.x = point.x;
        if (point.y > _worldMax.y) _worldMax.y = point.y;
    }

    private static Vector2 WorldPoint(Vector2 world, Rect img)
    {
        float x = (world.x - _worldMin.x) / Mathf.Max(0.01f, _worldMax.x - _worldMin.x);
        float y = (world.y - _worldMin.y) / Mathf.Max(0.01f, _worldMax.y - _worldMin.y);
        return new Vector2(img.x + x * img.width, img.y + (1f - y) * img.height);
    }

    private static void DrawWorldRooms(Rect img)
    {
        if (!EnsureWorldRooms()) return;
        Fill(img, new Color(0.035f, 0.04f, 0.055f, 0.82f * CheatToggles.radarAlpha));
        for (int i = 0; i < _worldRooms.Count; i++)
        {
            Bounds bounds = _worldRooms[i];
            Vector2 a = WorldPoint(new Vector2(bounds.min.x, bounds.min.y), img);
            Vector2 b = WorldPoint(new Vector2(bounds.max.x, bounds.max.y), img);
            Rect room = new Rect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
            Fill(room, new Color(0.42f, 0.46f, 0.54f, 0.34f * CheatToggles.radarAlpha));
            Stroke(room, new Color(0.4f, 0.85f, 1f, 0.72f * CheatToggles.radarAlpha));
        }
    }

    private static void Fill(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.Box(rect, GUIContent.none, _pixelStyle);
        GUI.color = old;
    }

    private static void Stroke(Rect rect, Color color)
    {
        Fill(new Rect(rect.x, rect.y, rect.width, 1f), color);
        Fill(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
        Fill(new Rect(rect.x, rect.y, 1f, rect.height), color);
        Fill(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
    }

    private static void Icon(Vector2 p, Color c, Texture2D tex = null)
    {
        tex ??= _playerTex;
        GUIStyle style = tex == _bodyTex ? IconStyle(tex, ref _bodyStyle) : IconStyle(tex, ref _playerStyle);
        if (tex == null || style == null) { Glyph(p, c, "●"); return; }
        try
        {
            float size = 18f * CheatToggles.radarScale;
            Rect rect = new Rect(p.x - size * 0.5f, p.y - size * 0.5f, size, size);
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.Box(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f), GUIContent.none, style);
            GUI.color = c;
            GUI.Box(rect, GUIContent.none, style);
            if (tex == _playerTex && _visorTex != null)
            {
                GUIStyle visorStyle = IconStyle(_visorTex, ref _visorStyle);
                if (visorStyle != null)
                {
                    GUI.color = new Color(0.72f, 0.86f, 0.96f, 1f);
                    GUI.Box(rect, GUIContent.none, visorStyle);
                }
            }
            GUI.color = old;
        }
        catch { Glyph(p, c, "●"); }
    }

    private static void Glyph(Vector2 p, Color c, string glyph)
    {
        float sz = 20f * CheatToggles.radarScale;
        _dotStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(18f * CheatToggles.radarScale));
        Rect r = new Rect(p.x - sz * 0.5f, p.y - sz * 0.5f, sz, sz);
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.9f);
        GUI.Label(new Rect(r.x + 1f, r.y + 1f, r.width, r.height), glyph, _dotStyle);
        GUI.color = new Color(c.r, c.g, c.b, 1f);
        GUI.Label(r, glyph, _dotStyle);
        GUI.color = old;
    }

    private static Color PlayerColor(PlayerControl pc)
    {
        try
        {
            int cid = pc.Data.DefaultOutfit.ColorId;
            if (Palette.PlayerColors != null && cid >= 0 && cid < Palette.PlayerColors.Length)
                return Palette.PlayerColors[cid];
        }
        catch { }
        return Color.white;
    }

    private static bool RadarBindMatches(Event e)
    {
        return CheatToggles.radarTeleportBind switch
        {
            CheatToggles.TeleportBind.Left => e.button == 0 && !e.shift && !e.control,
            CheatToggles.TeleportBind.Right => e.button == 1 && !e.shift && !e.control,
            CheatToggles.TeleportBind.ShiftLeft => e.button == 0 && e.shift,
            CheatToggles.TeleportBind.ShiftRight => e.button == 1 && e.shift,
            CheatToggles.TeleportBind.CtrlLeft => e.button == 0 && e.control,
            CheatToggles.TeleportBind.CtrlRight => e.button == 1 && e.control,
            _ => false,
        };
    }

    private static bool RadarDoorsBindMatches(Event e)
    {
        return CheatToggles.radarDoorsBind switch
        {
            CheatToggles.TeleportBind.Left => e.button == 0 && !e.shift && !e.control,
            CheatToggles.TeleportBind.Right => e.button == 1 && !e.shift && !e.control,
            CheatToggles.TeleportBind.ShiftLeft => e.button == 0 && e.shift,
            CheatToggles.TeleportBind.ShiftRight => e.button == 1 && e.shift,
            CheatToggles.TeleportBind.CtrlLeft => e.button == 0 && e.control,
            CheatToggles.TeleportBind.CtrlRight => e.button == 1 && e.control,
            _ => false,
        };
    }

    private static void ClickDoors(RadarMap map)
    {
        Event e = Event.current;
        if (e == null) return;
        if (e.type != EventType.MouseDown) return;
        if (ShipStatus.Instance == null || ShipStatus.Instance.AllRooms == null) return;
        if (!RadarDoorsBindMatches(e)) return;

        Rect img = MapRect(map);
        Vector2 m = e.mousePosition;
        if (!img.Contains(m)) return;

        Vector2 world = UnmapPoint(map, img, m);
        PlainShipRoom[] rooms = null;
        try { rooms = ShipStatus.Instance.AllRooms; } catch { }
        if (rooms == null) return;
        SystemTypes bestRoom = SystemTypes.Hallway;
        float bestDist = float.MaxValue;
        for (int i = 0; i < rooms.Length; i++)
        {
            PlainShipRoom room = null;
            try { room = rooms[i]; } catch { continue; }
            if (room == null) continue;
            try
            {
                if (room.roomArea == null) continue;
                if (DoorsHandler.GetDoorsInRoom(room.RoomId).Count == 0) continue;
                Bounds b = room.roomArea.bounds;
                Vector2 closest = new Vector2(Mathf.Clamp(world.x, b.min.x, b.max.x), Mathf.Clamp(world.y, b.min.y, b.max.y));
                float d = Vector2.Distance(world, closest);
                if (d >= bestDist) continue;
                bestDist = d;
                bestRoom = room.RoomId;
            }
            catch { }
        }
        if (bestDist < 30f)
        {
            DoorsHandler.CloseDoorsInRoom(bestRoom);
            e.Use();
        }
    }

    private static Vector2 UnmapPoint(RadarMap map, Rect img, Vector2 m)
    {
        float refW = Mathf.Max(1f, map.tex.width * ReferenceScale);
        float refH = Mathf.Max(1f, map.tex.height * ReferenceScale);
        float nativeX = (m.x - img.x) * refW / Mathf.Max(1f, img.width);
        float nativeY = (m.y - img.y) * refH / Mathf.Max(1f, img.height);
        return new Vector2((nativeX - map.x) / map.scale, (map.y - nativeY) / map.scale);
    }

    private static void ClickTp(RadarMap map)
    {
        Event e = Event.current;
        if (e == null) return;
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag) return;
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.NetTransform == null) return;
        if (!RadarBindMatches(e)) return;

        if (e.type == EventType.MouseDrag && Time.unscaledTime < _nextTpAt) return;
        _nextTpAt = Time.unscaledTime + 0.1f;

        Rect img = MapRect(map);
        Vector2 m = e.mousePosition;
        if (!img.Contains(m)) return;

        Vector2 target;
        if (CheatToggles.radarRealistic && EnsureWorldRooms())
        {
            float x = Mathf.Clamp01((m.x - img.x) / img.width);
            float y = 1f - Mathf.Clamp01((m.y - img.y) / img.height);
            target = new Vector2(Mathf.Lerp(_worldMin.x, _worldMax.x, x), Mathf.Lerp(_worldMin.y, _worldMax.y, y));
        }
        else
        {
            float refW = Mathf.Max(1f, map.tex.width * ReferenceScale);
            float refH = Mathf.Max(1f, map.tex.height * ReferenceScale);
            float nativeX = (m.x - img.x) * refW / Mathf.Max(1f, img.width);
            float nativeY = (m.y - img.y) * refH / Mathf.Max(1f, img.height);
            target = new Vector2((nativeX - map.x) / map.scale, (map.y - nativeY) / map.scale);
        }

        try { Teleporter.TeleportTo(target); }
        catch { try { PlayerControl.LocalPlayer.NetTransform.SnapTo(target); } catch { } }
        e.Use();
    }
}