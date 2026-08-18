using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using UnityEngine;

namespace SkidMenu
{
    public static class GUIStylePreset
    {
        public static Font FontRegular
        {
            get
            {
                if (_fontRegular == null) _fontRegular = LoadFont("Roboto_Condensed-Regular.ttf");
                return _fontRegular;
            }
        }

        public static Font FontBold
        {
            get
            {
                if (_fontBold == null) _fontBold = LoadFont("Roboto_Condensed-Bold.ttf");
                return _fontBold;
            }
        }

        private static Font LoadFont(string filename)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resName = "SkidMenu.Assets." + filename;
                using (Stream stream = assembly.GetManifestResourceStream(resName))
                {
                    if (stream != null)
                    {
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        string path = Path.Combine(Paths.BepInExRootPath, filename);
                        File.WriteAllBytes(path, bytes);
                        Font font = Font.CreateDynamicFontFromOSFont(path, 14);
                        if (font != null) return font;
                        font = new Font(path);
                        font.hideFlags = HideFlags.HideAndDontSave;
                        return font;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public static Texture2D LoadEmbeddedTexture(string filename)
        {
            try
            {
                string path = Path.Combine(Paths.BepInExRootPath, filename);
                if (!File.Exists(path))
                {
                    Debug.LogError("[SkidMenu] File not found: " + path);
                    return null;
                }
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.hideFlags = (HideFlags)61;
                ImageConversion.LoadImage(tex, bytes, false);
                Debug.Log($"[SkidMenu] Loaded texture {filename}: {tex.width}x{tex.height}");
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogError("[SkidMenu] LoadEmbeddedTexture error: " + e);
            }
            return null;
        }

        private static Texture2D MakeTex(Color c)
        {
            if (_texCache.TryGetValue(c, out var existing) && existing != null) return existing;
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = (HideFlags)61;
            _texCache[c] = t;
            return t;
        }

        public static Texture2D MakeTex1x1(Color c) => MakeTex(c);

        public static Texture2D MakeRoundedSolid(int size, Color fill, int radius, float borderAlpha)
        {
            Color border = new Color(0f, 0f, 0f, borderAlpha);
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!InRoundRect(x, y, 0, 0, size, size, radius))
                    {
                        px[y * size + x] = new Color(0f, 0f, 0f, 0f);
                    }
                    else if (IsRoundedBorder(x, y, size, radius))
                    {
                        px[y * size + x] = border;
                    }
                    else
                    {
                        px[y * size + x] = fill;
                    }
                }
            }
            t.SetPixels(px);
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = (HideFlags)61;
            return t;
        }

        public static Texture2D MakeRoundedSolidInner(int size, Color fill, int radius, float borderAlpha, Color innerFill, int innerPad)
        {
            Color border = new Color(0f, 0f, 0f, borderAlpha);
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!InRoundRect(x, y, 0, 0, size, size, radius))
                    {
                        px[y * size + x] = new Color(0f, 0f, 0f, 0f);
                    }
                    else if (IsRoundedBorder(x, y, size, radius))
                    {
                        px[y * size + x] = border;
                    }
                    else if (innerFill.a > 0f && InRoundRect(x, y, innerPad, innerPad, size - innerPad, size - innerPad, Mathf.Max(1, radius - 1)))
                    {
                        px[y * size + x] = innerFill;
                    }
                    else
                    {
                        px[y * size + x] = fill;
                    }
                }
            }
            t.SetPixels(px);
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = (HideFlags)61;
            return t;
        }

        private static bool InSmoothRoundRect(int px, int py, int x0, int y0, int x1, int y1, int r)
        {
            if (px < x0 || px >= x1 || py < y0 || py >= y1) return false;
            if (px >= x0 + r && px < x1 - r) return true;
            if (py >= y0 + r && py < y1 - r) return true;
            int cx = px < x0 + r ? x0 + r : x1 - r - 1;
            int cy = py < y0 + r ? y0 + r : y1 - r - 1;
            return Dist(px, py, cx, cy) < r;
        }

        private static Texture2D MakeSmoothBand(int canvas, int y0, int y1, Color fill, int radius, float borderAlpha)
        {
            const int ss = 4;
            Color border = new Color(0f, 0f, 0f, borderAlpha);
            int N = canvas * ss;
            int D = N * ss;
            int R = radius * ss * ss;
            int by0 = y0 * ss * ss;
            int by1 = y1 * ss * ss;
            var cov = new float[N * N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    int subX = x * ss;
                    int subY = y * ss;
                    float c = 0f;
                    for (int sy = 0; sy < ss; sy++)
                    {
                        for (int sx = 0; sx < ss; sx++)
                        {
                            if (InSmoothRoundRect(subX + sx, subY + sy, 0, by0, D, by1, R)) c += 1f;
                        }
                    }
                    cov[y * N + x] = c / (ss * ss);
                }
            }
            var t = new Texture2D(N, N, TextureFormat.RGBA32, false);
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    float c = cov[y * N + x];
                    if (c <= 0f)
                    {
                        px[y * N + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }
                    bool edge = false;
                    for (int dy = -1; dy <= 1 && !edge; dy++)
                    {
                        for (int dx = -1; dx <= 1 && !edge; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= N || ny >= N || cov[ny * N + nx] <= 0f) edge = true;
                        }
                    }
                    Color col = edge ? border : fill;
                    px[y * N + x] = new Color(col.r, col.g, col.b, col.a * c);
                }
            }
            t.SetPixels(px);
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = (HideFlags)61;
            return t;
        }

        public static Texture2D MakeRoundedSolidSmooth(int size, Color fill, int radius, float borderAlpha)
        {
            return MakeSmoothBand(size, 0, size, fill, radius, borderAlpha);
        }

        public static Texture2D MakeSliderTrack(Color fill, int bandHeight, int radius, float borderAlpha)
        {
            return MakeSmoothBand(bandHeight + 6, 3, 3 + bandHeight, fill, radius, borderAlpha);
        }

        public static Texture2D MakeRoundedPanel(Color fill, int radius, float borderAlpha, float sheen, float shade)
        {
            const int size = 32;
            Color border = new Color(0f, 0f, 0f, borderAlpha);
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!InRoundRect(x, y, 0, 0, size, size, radius))
                    {
                        px[y * size + x] = new Color(0f, 0f, 0f, 0f);
                    }
                    else if (IsRoundedBorder(x, y, size, radius))
                    {
                        px[y * size + x] = border;
                    }
                    else
                    {
                        float t0 = (float)y / (size - 1);
                        px[y * size + x] = Color.Lerp(Lighten(fill, sheen), Darken(fill, shade), t0);
                    }
                }
            }
            t.SetPixels(px);
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = (HideFlags)61;
            return t;
        }

        private static Texture2D MakeWindowTex()
        {
            const int size = 32;
            const int radius = 10;
            Color fill = new Color(0.07f, 0.07f, 0.07f, 0.82f);
            Color border = new Color(0f, 0f, 0f, 0.6f);
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!InRoundRect(x, y, 0, 0, size, size, radius))
                    {
                        px[y * size + x] = new Color(0f, 0f, 0f, 0f);
                    }
                    else if (IsRoundedBorder(x, y, size, radius))
                    {
                        px[y * size + x] = border;
                    }
                    else
                    {
                        float t0 = (float)y / (size - 1);
                        px[y * size + x] = Color.Lerp(Lighten(fill, 0.05f), Darken(fill, 0.10f), t0);
                    }
                }
            }
            for (int x = radius; x < size - radius; x++)
            {
                px[1 * size + x] = _accent;
                px[2 * size + x] = _accent;
            }
            t.SetPixels(px);
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            t.hideFlags = (HideFlags)61;
            return t;
        }

        private static Color Lighten(Color c, float amt) => Color.Lerp(c, Color.white, amt);

        private static Color Darken(Color c, float amt) => Color.Lerp(c, Color.black, amt);

        private static bool IsRoundedBorder(int px, int py, int size, int radius)
        {
            if (!InRoundRect(px, py, 0, 0, size, size, radius)) return false;
            if (!InRoundRect(px - 1, py, 0, 0, size, size, radius)) return true;
            if (!InRoundRect(px + 1, py, 0, 0, size, size, radius)) return true;
            if (!InRoundRect(px, py - 1, 0, 0, size, size, radius)) return true;
            if (!InRoundRect(px, py + 1, 0, 0, size, size, radius)) return true;
            return false;
        }

        private static bool InRoundRect(int px, int py, int x0, int y0, int x1, int y1, int r)
        {
            if (px < x0 || px >= x1 || py < y0 || py >= y1) return false;
            if (px < x0 + r && py < y0 + r) return Dist(px, py, x0 + r, y0 + r) < r;
            if (px >= x1 - r && py < y0 + r) return Dist(px, py, x1 - r - 1, y0 + r) < r;
            if (px < x0 + r && py >= y1 - r) return Dist(px, py, x0 + r, y1 - r - 1) < r;
            return px < x1 - r || py < y1 - r || Dist(px, py, x1 - r - 1, y1 - r - 1) < r;
        }

        private static float Dist(int ax, int ay, int bx, int by)
        {
            return Mathf.Sqrt((float)((ax - bx) * (ax - bx) + (ay - by) * (ay - by)));
        }

        public static GUIStyle Separator
        {
            get
            {
                if (_separator == null)
                {
                    _separator = new GUIStyle(GUI.skin.box)
                    {
                        normal = { background = MakeTex(new Color(0.25f, 0.25f, 0.3f, 1f)) },
                        margin = new RectOffset { top = 6, bottom = 6, left = 2, right = 2 },
                        padding = new RectOffset(),
                        border = new RectOffset()
                    };
                }
                return _separator;
            }
        }

        public static GUIStyle DarkSeparator
        {
            get
            {
                if (_darkSeparator == null)
                {
                    _darkSeparator = new GUIStyle(GUI.skin.box)
                    {
                        normal = { background = MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f)) },
                        margin = new RectOffset { top = 4, bottom = 4 },
                        padding = new RectOffset(),
                        border = new RectOffset()
                    };
                }
                return _darkSeparator;
            }
        }

        public static GUIStyle NormalButton
        {
            get
            {
                if (_normalButton == null)
                {
                    _normalButton = new GUIStyle(GUI.skin.button)
                    {
                        font = FontRegular,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 14, right = 14, top = 5, bottom = 8 },
                        margin = new RectOffset { left = 3, right = 3, top = 4, bottom = 4 },
                        border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                        richText = true,
                        wordWrap = false,
                        normal = { background = MakeRoundedPanel(_bg, 6, 0.55f, 0.05f, 0.10f), textColor = _text },
                        hover = { background = MakeRoundedPanel(_bgHover, 6, 0.55f, 0.05f, 0.08f), textColor = Color.white },
                        active = { background = MakeRoundedPanel(_bgActive, 6, 0.55f, 0.04f, 0.06f), textColor = Color.white }
                    };
                }
                return _normalButton;
            }
        }

        public static GUIStyle NormalToggle
        {
            get
            {
                if (_normalToggle == null)
                {
                    _normalToggle = new GUIStyle(GUI.skin.toggle)
                    {
                        font = FontRegular,
                        fontSize = 13,
                        padding = new RectOffset { left = 6, right = 5, top = 2, bottom = 2 },
                        margin = new RectOffset { left = 2, right = 2, top = 4, bottom = 4 },
                        alignment = TextAnchor.MiddleLeft,
                        richText = true,
                        fixedWidth = 0f,
                        fixedHeight = 0f,
                        normal = { background = null, textColor = _textDim },
                        onNormal = { background = null, textColor = _text },
                        hover = { background = null, textColor = _text },
                        onHover = { background = null, textColor = Color.white }
                    };
                }
                return _normalToggle;
            }
        }

        public static GUIStyle TabButton
        {
            get
            {
                if (_tabButton == null)
                {
                    _tabButton = new GUIStyle(GUI.skin.button)
                    {
                        font = FontRegular,
                        fontSize = 13,
                        padding = new RectOffset { left = 12, right = 12, top = 8, bottom = 9 },
                        margin = new RectOffset { left = 3, right = 3, top = 3, bottom = 3 },
                        border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                        alignment = TextAnchor.MiddleLeft,
                        wordWrap = false,
                        richText = true,
                        clipping = TextClipping.Clip,
                        normal = { background = MakeRoundedPanel(_bg, 6, 0.5f, 0.04f, 0.12f), textColor = _textDim },
                        hover = { background = MakeRoundedPanel(_bgHover, 6, 0.5f, 0.05f, 0.09f), textColor = _text },
                        active = { background = MakeRoundedPanel(_bgActive, 6, 0.5f, 0.04f, 0.07f), textColor = Color.white }
                    };
                }
                return _tabButton;
            }
        }

        public static GUIStyle TabButtonSelected
        {
            get
            {
                if (_tabButtonSelected == null)
                {
                    _tabButtonSelected = new GUIStyle(GUI.skin.button)
                    {
                        font = FontBold,
                        fontSize = 13,
                        padding = new RectOffset { left = 12, right = 12, top = 8, bottom = 9 },
                        margin = new RectOffset { left = 3, right = 3, top = 3, bottom = 3 },
                        border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                        alignment = TextAnchor.MiddleLeft,
                        wordWrap = true,
                        richText = true,
                        normal = { background = MakeRoundedPanel(_accent, 6, 0.7f, 0f, 0.14f), textColor = new Color(0.05f, 0.05f, 0.07f, 1f) },
                        hover = { background = MakeRoundedPanel(_accentHov, 6, 0.7f, 0f, 0.12f), textColor = new Color(0.05f, 0.05f, 0.07f, 1f) },
                        active = { background = MakeRoundedPanel(_accentHov, 6, 0.7f, 0f, 0.12f), textColor = new Color(0.05f, 0.05f, 0.07f, 1f) }
                    };
                }
                return _tabButtonSelected;
            }
        }

        public static GUIStyle TabTitle
        {
            get
            {
                if (_tabTitle == null)
                {
                    _tabTitle = new GUIStyle(GUI.skin.label)
                    {
                        font = FontBold,
                        fontSize = 20,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 8, right = 8, top = 6, bottom = 6 },
                        margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 4 },
                        richText = true,
                        normal = { textColor = Color.white }
                    };
                }
                return _tabTitle;
            }
        }

        public static GUIStyle TabSubtitle
        {
            get
            {
                if (_tabSubtitle == null)
                {
                    _tabSubtitle = new GUIStyle(GUI.skin.label)
                    {
                        font = FontBold,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 8, right = 8, top = 4, bottom = 4 },
                        margin = new RectOffset { left = 0, right = 0, top = 2, bottom = 2 },
                        richText = true,
                        normal = { textColor = new Color(0.6f, 0.75f, 1f, 1f) }
                    };
                }
                return _tabSubtitle;
            }
        }

        public static GUIStyle ModernBox
        {
            get
            {
                if (_modernBox == null)
                {
                    _modernBox = new GUIStyle(GUI.skin.box)
                    {
                        normal = { background = MakeRoundedPanel(new Color(0.05f, 0.05f, 0.05f, 0.32f), 8, 0.35f, 0.06f, 0.05f) },
                        padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
                        margin = new RectOffset { left = 3, right = 3, top = 4, bottom = 4 },
                        border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
                    };
                }
                return _modernBox;
            }
        }

        public static GUIStyle SectionHeader
        {
            get
            {
                if (_sectionHeader == null)
                {
                    _sectionHeader = new GUIStyle(GUI.skin.label)
                    {
                        font = FontBold,
                        fontSize = 14,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 6, right = 6, top = 4, bottom = 4 },
                        margin = new RectOffset { left = 2, right = 2, top = 6, bottom = 4 },
                        richText = true,
                        normal = { textColor = Color.white }
                    };
                }
                return _sectionHeader;
            }
        }

        public static GUIStyle ModernLabel
        {
            get
            {
                if (_modernLabel == null)
                {
                    _modernLabel = new GUIStyle(GUI.skin.label)
                    {
                        font = FontRegular,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 4, right = 4, top = 3, bottom = 3 },
                        margin = new RectOffset { left = 2, right = 2, top = 1, bottom = 1 },
                        richText = true,
                        wordWrap = true,
                        normal = { textColor = _textDim }
                    };
                }
                return _modernLabel;
            }
        }

        public static Texture2D SliderTrack
        {
            get
            {
                if (_sliderTrack == null) _sliderTrack = MakeSliderTrack(new Color(0.18f, 0.18f, 0.18f, 1f), 8, 3, 0.4f);
                return _sliderTrack;
            }
        }

        public static Texture2D SliderThumb
        {
            get
            {
                if (_sliderThumb == null) _sliderThumb = MakeRoundedSolidSmooth(16, _accent, 8, 0.5f);
                return _sliderThumb;
            }
        }

        public static Texture2D SliderThumbHover
        {
            get
            {
                if (_sliderThumbHover == null) _sliderThumbHover = MakeRoundedSolidSmooth(16, _accentHov, 8, 0.5f);
                return _sliderThumbHover;
            }
        }

        public static Texture2D ActionButtonBg
        {
            get
            {
                if (_actionButtonBg == null)
                    _actionButtonBg = MakeRoundedPanel(new Color(0.22f, 0.22f, 0.22f, 1f), 6, 0.55f, 0.05f, 0.09f);
                return _actionButtonBg;
            }
        }

        public static Texture2D WhiteButtonBg
        {
            get
            {
                if (_whiteButtonBg == null)
                    _whiteButtonBg = MakeRoundedPanel(new Color(0.4f, 0.4f, 0.4f, 1f), 6, 0.5f, 0.03f, 0.05f);
                return _whiteButtonBg;
            }
        }

        public static GUIStyle WindowStyle
        {
            get
            {
                if (_windowStyle == null)
                {
                    if (_windowTex == null) _windowTex = MakeWindowTex();
                    _windowStyle = new GUIStyle(GUI.skin.window)
                    {
                        normal = { background = _windowTex, textColor = Color.white },
                        onNormal = { background = _windowTex, textColor = Color.white },
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.UpperCenter,
                        border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                        padding = new RectOffset { left = 8, right = 8, top = 34, bottom = 8 }
                    };
                }
                return _windowStyle;
            }
        }

        public static GUIStyle InfoLabel => ModernLabel;

        public static GUIStyle SectionBox
        {
            get
            {
                if (_sectionBox == null)
                {
                    _sectionBox = new GUIStyle(GUI.skin.box)
                    {
                        normal = { background = MakeRoundedPanel(new Color(0.13f, 0.13f, 0.13f, 1f), 8, 0.45f, 0.05f, 0.08f), textColor = _text },
                        border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
                        padding = new RectOffset { left = 8, right = 8, top = 6, bottom = 6 },
                        margin = new RectOffset { left = 2, right = 2, top = 4, bottom = 4 }
                    };
                }
                return _sectionBox;
            }
        }

        public static GUIStyle NormalTextField
        {
            get
            {
                if (_normalTextField == null)
                {
                    _normalTextField = new GUIStyle(GUI.skin.textField)
                    {
                        font = FontRegular,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset { left = 7, right = 7, top = 4, bottom = 4 },
                        margin = new RectOffset { left = 3, right = 3, top = 4, bottom = 4 },
                        border = new RectOffset { left = 6, right = 6, top = 6, bottom = 6 },
                        normal = { background = MakeRoundedPanel(new Color(0.15f, 0.15f, 0.15f, 1f), 6, 0.5f, 0.06f, 0.10f), textColor = _text },
                        focused = { background = MakeRoundedPanel(new Color(0.21f, 0.21f, 0.21f, 1f), 6, 0.0f, 0.05f, 0.08f), textColor = Color.white },
                        hover = { background = MakeRoundedPanel(new Color(0.18f, 0.18f, 0.18f, 1f), 6, 0.5f, 0.05f, 0.09f), textColor = _text }
                    };
                }
                return _normalTextField;
            }
        }

        private static GUIStyle ToggleBoxOff
        {
            get
            {
                if (_toggleBoxOff == null)
                {
                    _toggleBoxOff = new GUIStyle
                    {
                        normal = { background = MakeRoundedSolid(18, _bgHover, 5, 0.5f) },
                        fixedWidth = 18f,
                        fixedHeight = 18f,
                        margin = new RectOffset { left = 3, right = 5, top = 4, bottom = 4 },
                        padding = new RectOffset()
                    };
                }
                return _toggleBoxOff;
            }
        }

        private static GUIStyle ToggleBoxOn
        {
            get
            {
                if (_toggleBoxOn == null)
                {
                    _toggleBoxOn = new GUIStyle
                    {
                        normal = { background = MakeRoundedSolidInner(18, _bgHover, 5, 0.5f, _accent, 3) },
                        fixedWidth = 18f,
                        fixedHeight = 18f,
                        margin = new RectOffset { left = 3, right = 5, top = 4, bottom = 4 },
                        padding = new RectOffset()
                    };
                }
                return _toggleBoxOn;
            }
        }

        private static readonly GUILayoutOption[] _toggleIconOpts = { GUILayout.Width(18f), GUILayout.Height(18f) };
        private static readonly GUILayoutOption[] _toggleLabelOpts = { GUILayout.ExpandWidth(false) };

        public static bool CustomToggle(bool value, string label, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            bool clicked = GUILayout.Button("", value ? ToggleBoxOn : ToggleBoxOff, _toggleIconOpts);
            GUILayout.Label(label, NormalToggle, _toggleLabelOpts);
            GUILayout.EndHorizontal();
            if (!clicked) return value;
            return !value;
        }

        public static void Reset()
        {
            _separator = null;
            _darkSeparator = null;
            _normalButton = null;
            _normalToggle = null;
            _tabButton = null;
            _tabButtonSelected = null;
            _tabTitle = null;
            _tabSubtitle = null;
            _modernBox = null;
            _sectionHeader = null;
            _modernLabel = null;
            _normalTextField = null;
            _sectionBox = null;
            _windowStyle = null;
            _sliderTrack = null;
            _sliderThumb = null;
            _sliderThumbHover = null;
            _actionButtonBg = null;
            _whiteButtonBg = null;
            _toggleBoxOff = null;
            _toggleBoxOn = null;
            _windowTex = null;
            _cornerOverlayStyle = null;
            foreach (var tex in _texCache.Values)
            {
                if (tex != null) UnityEngine.Object.Destroy(tex);
            }
            _texCache.Clear();
        }

        private static GUIStyle _separator;
        private static GUIStyle _darkSeparator;
        private static GUIStyle _normalButton;
        private static GUIStyle _normalToggle;
        private static GUIStyle _tabButton;
        private static GUIStyle _tabButtonSelected;
        private static GUIStyle _tabTitle;
        private static GUIStyle _tabSubtitle;
        private static GUIStyle _modernBox;
        private static GUIStyle _sectionHeader;
        private static GUIStyle _modernLabel;
        private static GUIStyle _normalTextField;
        private static GUIStyle _sectionBox;
        private static GUIStyle _windowStyle;
        private static GUIStyle _toggleBoxOff;
        private static GUIStyle _toggleBoxOn;
        private static Texture2D _windowTex;

        private static readonly Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();
        private static Font _fontRegular;
        private static Font _fontBold;
        private static Texture2D _sliderTrack;
        private static Texture2D _sliderThumb;
        private static Texture2D _sliderThumbHover;
        private static Texture2D _actionButtonBg;
        private static Texture2D _whiteButtonBg;

        private static readonly Color _bg = new Color(0.1f, 0.1f, 0.1f, 1f);
        private static readonly Color _bgHover = new Color(0.16f, 0.16f, 0.16f, 1f);
        private static readonly Color _bgActive = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color _accent = new Color(0.6f, 1f, 0.99f, 1f);
        private static readonly Color _accentHov = new Color(0.75f, 1f, 0.99f, 1f);
        private static readonly Color _text = new Color(0.93f, 0.93f, 0.95f, 1f);
        private static readonly Color _textDim = new Color(0.7f, 0.7f, 0.73f, 1f);

        private static GUIStyle _cornerOverlayStyle;

        public static GUIStyle CornerOverlayStyle
        {
            get
            {
                if (_cornerOverlayStyle == null)
                {
                    _cornerOverlayStyle = new GUIStyle
                    {
                        normal = { background = GenerateCornerOverlay() },
                        border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                        padding = new RectOffset(),
                        margin = new RectOffset(),
                        overflow = new RectOffset()
                    };
                }
                return _cornerOverlayStyle;
            }
        }

        private static Texture2D GenerateCornerOverlay()
        {
            const int size = 32;
            const int radius = 10;
            Color fill = new Color(0.07f, 0.07f, 0.07f, 0.82f);

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (InRoundRect(x, y, 0, 0, size, size, radius))
                        px[y * size + x] = Color.clear;
                    else
                        px[y * size + x] = fill;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.hideFlags = (HideFlags)61;
            return tex;
        }
    }
}
