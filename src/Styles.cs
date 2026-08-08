using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu.ui
{
    internal class Styles
    {
        public enum UIColors
        {
            Azure,
            Carbon,
            Cardinal,
            Pesto,
            Pumpkin,
            White,
            Violet
        }

        public static Dictionary<UIColors, Color> ColorValues = new Dictionary<UIColors, Color>()
        {
            { UIColors.Azure, new Color(0.0f, 0.50f, 1f) }, // #007FFF
			{ UIColors.Carbon, new Color(0.07f, 0.07f, 0.07f) }, // #222222
			{ UIColors.Cardinal, new Color(0.77f, 0.12f, 0.23f) }, // #C41E3A
			{ UIColors.Pesto, new Color(0.05f, 0.5f, 0.13f) }, // #119922
			{ UIColors.Pumpkin, new Color(1.0f, 0.18f, 0.04f) }, // #FF7518
			{ UIColors.White, new Color(0.95f, 0.95f, 0.97f) }, // #F0EFDF
			{ UIColors.Violet, new Color(0.5f, 0f, 1f) } // #7F00FF
		};

        public static float menuOpacity = 0.85f;
        public static UIColors primaryColor = UIColors.Azure;

        private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();

        private static GUIStyle _mainBox;
        private static GUIStyle _sectionBox;
        private static GUIStyle _sectionBoxActive;
        private static GUIStyle _playerBox;
        private static GUIStyle _playerBoxActive;

        public static GUIStyle MainBox
        {
            get
            {
                if (_mainBox == null)
                {
                    _mainBox = new GUIStyle();
                    _mainBox.normal.background = CreateColoredTexture("MainBox", ColorValues[UIColors.Carbon], menuOpacity);
                    _mainBox.normal.textColor = Color.white;
                    _mainBox.alignment = TextAnchor.MiddleCenter;
                    _mainBox.padding.top = 2;
                }
                return _mainBox;
            }
        }

        public static GUIStyle SectionBox
        {
            get
            {
                if (_sectionBox == null)
                {
                    _sectionBox = new GUIStyle();
                    _sectionBox.normal.textColor = ColorValues[UIColors.White];
                    _sectionBox.alignment = TextAnchor.MiddleLeft;
                    _sectionBox.padding.bottom = 1;
                    _sectionBox.padding.left = 8;
                    _sectionBox.fontSize = 14;
                }
                return _sectionBox;
            }
        }

        public static GUIStyle SectionBoxActive
        {
            get
            {
                if (_sectionBoxActive == null)
                {
                    _sectionBoxActive = new GUIStyle();
                    _sectionBoxActive.normal.background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
                    _sectionBoxActive.normal.textColor = ColorValues[UIColors.White];
                    _sectionBoxActive.alignment = TextAnchor.MiddleLeft;
                    _sectionBoxActive.padding.bottom = 1;
                    _sectionBoxActive.padding.left = 13;
                    _sectionBoxActive.fontSize = 14;
                }
                return _sectionBoxActive;
            }
        }

        public static GUIStyle PlayerBox
        {
            get
            {
                if (_playerBox == null)
                {
                    _playerBox = new GUIStyle();
                    _playerBox.normal.textColor = ColorValues[UIColors.White];
                    _playerBox.alignment = TextAnchor.MiddleLeft;
                    _playerBox.clipping = TextClipping.Clip;
                    _playerBox.padding.left = 10;
                    _playerBox.richText = true;
                }
                return _playerBox;
            }
        }

        public static GUIStyle PlayerBoxActive
        {
            get
            {
                if (_playerBoxActive == null)
                {
                    _playerBoxActive = new GUIStyle();
                    _playerBoxActive.normal.background = CreateColoredTexture("SectionBoxActive", ColorValues[primaryColor]);
                    _playerBoxActive.normal.textColor = ColorValues[UIColors.White];
                    _playerBoxActive.alignment = TextAnchor.MiddleLeft;
                    _playerBoxActive.clipping = TextClipping.Clip;
                    _playerBoxActive.padding.left = 10;
                    _playerBoxActive.richText = true;
                }
                return _playerBoxActive;
            }
        }

        public static GUIStyle CreateCrewmateColorBox(string colorName, Color color)
        {
            GUIStyle style = new GUIStyle();

            Texture2D background = CreateColoredTexture(colorName, color);
            style.normal.background = background;

            return style;
        }

        private static Texture2D CreateColoredTexture(string textureName, Color color, float opacity = 1.0f)
        {
            CachedTextures.TryGetValue(textureName, out Texture2D background);
            if (background != null) return background;

            SkidMenu.Log.LogInfo($"Cache lookup for texture {textureName} returned a miss, creating the required texture...");

            background = new Texture2D(1, 1);
            background.SetPixel(0, 0, color.SetAlpha(opacity));
            background.Apply();

            CachedTextures[textureName] = background;
            return background;
        }

        public static void ClearCache()
        {
            foreach (Texture2D texture in CachedTextures.Values)
            {
                Texture2D.Destroy(texture);
            }
            CachedTextures.Clear();
            _mainBox = null;
            _sectionBox = null;
            _sectionBoxActive = null;
            _playerBox = null;
            _playerBoxActive = null;
        }
    }
}