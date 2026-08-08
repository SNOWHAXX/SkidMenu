using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using UnityEngine;

namespace SkidMenu
{
	// Token: 0x020000CC RID: 204
	public static class GUIStylePreset
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x00018FBF File Offset: 0x000171BF
		public static Font FontRegular
		{
			get
			{
				Font result;
				if ((result = GUIStylePreset._fontRegular) == null)
				{
					result = (GUIStylePreset._fontRegular = GUIStylePreset.LoadFont("Roboto_Condensed-Regular.ttf"));
				}
				return result;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x060001B1 RID: 433 RVA: 0x00018FDA File Offset: 0x000171DA
		public static Font FontBold
		{
			get
			{
				Font result;
				if ((result = GUIStylePreset._fontBold) == null)
				{
					result = (GUIStylePreset._fontBold = GUIStylePreset.LoadFont("Roboto_Condensed-Bold.ttf"));
				}
				return result;
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00018FF8 File Offset: 0x000171F8
		private static Font LoadFont(string filename)
		{
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string resName = "SkidMenu.Assets." + filename;
				using (Stream stream = executingAssembly.GetManifestResourceStream(resName))
				{
					if (stream != null)
					{
						byte[] bytes = new byte[stream.Length];
						stream.Read(bytes, 0, bytes.Length);
						string text = Path.Combine(Paths.BepInExRootPath, filename);
						File.WriteAllBytes(text, bytes);
						Font font = Font.CreateDynamicFontFromOSFont(text, 14);
						if (font != null) return font;
						font = new Font(text);
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

		// Token: 0x060001B3 RID: 435 RVA: 0x00019084 File Offset: 0x00017284
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
				tex.hideFlags = (UnityEngine.HideFlags)61;
				ImageConversion.LoadImage(tex, bytes, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 3);
				defaultInterpolatedStringHandler.AppendLiteral("[SkidMenu] Loaded texture ");
				defaultInterpolatedStringHandler.AppendFormatted(filename);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tex.width);
				defaultInterpolatedStringHandler.AppendLiteral("x");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tex.height);
				Debug.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				return tex;
			}
			catch (Exception e)
			{
				string str = "[SkidMenu] LoadEmbeddedTexture error: ";
				Exception ex = e;
				Debug.LogError(str + ((ex != null) ? ex.ToString() : null));
			}
			return null;
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00019188 File Offset: 0x00017388
		private static Font LoadFontFromBytes(byte[] bytes, string name)
		{
			try
			{
				string text = Path.Combine(Paths.BepInExRootPath, name);
				File.WriteAllBytes(text, bytes);
				return new Font(text);
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000191C8 File Offset: 0x000173C8
		private static Texture2D MakeTex(Color c)
		{
			Texture2D existing;
			if (GUIStylePreset._texCache.TryGetValue(c, out existing) && existing != null)
			{
				return existing;
			}
			Texture2D t = new Texture2D(1, 1);
			t.SetPixel(0, 0, c);
			t.Apply();
			t.hideFlags = (UnityEngine.HideFlags)61;
			GUIStylePreset._texCache[c] = t;
			return t;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0001921B File Offset: 0x0001741B
		public static Texture2D MakeTex1x1(Color c)
		{
			return GUIStylePreset.MakeTex(c);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00019224 File Offset: 0x00017424
		private static Texture2D GetToggleOff()
		{
			if (GUIStylePreset._toggleOff != null)
			{
				return GUIStylePreset._toggleOff;
			}
			GUIStylePreset._toggleOff = GUIStylePreset.MakeRoundedTex(16, GUIStylePreset._bgHover, new Color(0f, 0f, 0f, 0f), 4, 0);
			return GUIStylePreset._toggleOff;
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00019275 File Offset: 0x00017475
		private static Texture2D GetToggleOn()
		{
			if (GUIStylePreset._toggleOn != null)
			{
				return GUIStylePreset._toggleOn;
			}
			GUIStylePreset._toggleOn = GUIStylePreset.MakeRoundedTex(16, GUIStylePreset._bgHover, GUIStylePreset._accent, 4, 4);
			return GUIStylePreset._toggleOn;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x000192A8 File Offset: 0x000174A8
		private static Texture2D MakeRoundedTex(int size, Color fill, Color innerFill, int radius, int innerPad)
		{
			Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
			Color clear;
			clear = new Color(0f, 0f, 0f, 0f);
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					if (!GUIStylePreset.InRoundRect(x, y, 0, 0, size, size, radius))
					{
						t.SetPixel(x, y, clear);
					}
					else
					{
						bool inner = innerFill.a > 0f && GUIStylePreset.InRoundRect(x, y, innerPad, innerPad, size - innerPad, size - innerPad, Mathf.Max(1, radius - 1));
						t.SetPixel(x, y, inner ? innerFill : fill);
					}
				}
			}
			t.Apply();
			t.filterMode = FilterMode.Bilinear;
			t.hideFlags = (UnityEngine.HideFlags)61;
			return t;
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00019360 File Offset: 0x00017560
		private static bool InRoundRect(int px, int py, int x0, int y0, int x1, int y1, int r)
		{
			if (px < x0 || px >= x1 || py < y0 || py >= y1)
			{
				return false;
			}
			if (px < x0 + r && py < y0 + r)
			{
				return GUIStylePreset.Dist(px, py, x0 + r, y0 + r) < (float)r;
			}
			if (px >= x1 - r && py < y0 + r)
			{
				return GUIStylePreset.Dist(px, py, x1 - r - 1, y0 + r) < (float)r;
			}
			if (px < x0 + r && py >= y1 - r)
			{
				return GUIStylePreset.Dist(px, py, x0 + r, y1 - r - 1) < (float)r;
			}
			return px < x1 - r || py < y1 - r || GUIStylePreset.Dist(px, py, x1 - r - 1, y1 - r - 1) < (float)r;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0001941E File Offset: 0x0001761E
		private static float Dist(int ax, int ay, int bx, int by)
		{
			return Mathf.Sqrt((float)((ax - bx) * (ax - bx) + (ay - by) * (ay - by)));
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00019438 File Offset: 0x00017638
		public static GUIStyle Separator
		{
			get
			{
				if (GUIStylePreset._separator == null)
				{
					GUIStylePreset._separator = new GUIStyle(GUI.skin.box)
					{
						normal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0.25f, 0.25f, 0.3f, 1f))
						},
						margin = new RectOffset
						{
							top = 6,
							bottom = 6,
							left = 2,
							right = 2
						},
						padding = new RectOffset(),
						border = new RectOffset()
					};
				}
				return GUIStylePreset._separator;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060001BD RID: 445 RVA: 0x000194CC File Offset: 0x000176CC
		public static GUIStyle DarkSeparator
		{
			get
			{
				if (GUIStylePreset._darkSeparator == null)
				{
					GUIStylePreset._darkSeparator = new GUIStyle(GUI.skin.box)
					{
						normal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f))
						},
						margin = new RectOffset
						{
							top = 4,
							bottom = 4
						},
						padding = new RectOffset(),
						border = new RectOffset()
					};
				}
				return GUIStylePreset._darkSeparator;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00019554 File Offset: 0x00017754
		public static GUIStyle NormalButton
		{
			get
			{
				if (GUIStylePreset._normalButton == null)
				{
					GUIStylePreset._normalButton = new GUIStyle(GUI.skin.button)
					{
						font = GUIStylePreset.FontRegular,
						fontSize = 13,
						alignment = (UnityEngine.TextAnchor)4,
						padding = new RectOffset
						{
							left = 12,
							right = 12,
							top = 4,
							bottom = 8
						},
						margin = new RectOffset
						{
							left = 3,
							right = 3,
							top = 3,
							bottom = 3
						},
						richText = true,
						wordWrap = false,
						normal = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bg),
							textColor = GUIStylePreset._text
						},
						hover = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bgHover),
							textColor = Color.white
						},
						active = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bgActive),
							textColor = Color.white
						}
					};
				}
				return GUIStylePreset._normalButton;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060001BF RID: 447 RVA: 0x0001966C File Offset: 0x0001786C
		public static GUIStyle NormalToggle
		{
			get
			{
				if (GUIStylePreset._normalToggle == null)
				{
					GUIStylePreset._normalToggle = new GUIStyle(GUI.skin.toggle)
					{
						font = GUIStylePreset.FontRegular,
						fontSize = 13,
						padding = new RectOffset
						{
							left = 4,
							right = 5,
							top = 2,
							bottom = 2
						},
						margin = new RectOffset
						{
							left = 3,
							right = 3,
							top = 3,
							bottom = 3
						},
						alignment = (UnityEngine.TextAnchor)3,
						richText = true,
						fixedWidth = 0f,
						fixedHeight = 0f,
						normal = 
						{
							background = null,
							textColor = GUIStylePreset._textDim
						},
						onNormal = 
						{
							background = null,
							textColor = GUIStylePreset._text
						},
						hover = 
						{
							background = null,
							textColor = GUIStylePreset._text
						},
						onHover = 
						{
							background = null,
							textColor = Color.white
						}
					};
				}
				return GUIStylePreset._normalToggle;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x00019794 File Offset: 0x00017994
		public static GUIStyle TabButton
		{
			get
			{
				if (GUIStylePreset._tabButton == null)
				{
					GUIStylePreset._tabButton = new GUIStyle(GUI.skin.button)
					{
						font = GUIStylePreset.FontRegular,
						fontSize = 13,
						padding = new RectOffset
						{
							left = 10,
							right = 10,
							top = 6,
							bottom = 10
						},
						margin = new RectOffset
						{
							left = 2,
							right = 2,
							top = 2,
							bottom = 2
						},
						alignment = (UnityEngine.TextAnchor)4,
						wordWrap = false,
						richText = true,
						clipping = UnityEngine.TextClipping.Clip,
						normal = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bg),
							textColor = GUIStylePreset._textDim
						},
						hover = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bgHover),
							textColor = GUIStylePreset._text
						},
						active = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._bgActive),
							textColor = Color.white
						}
					};
				}
				return GUIStylePreset._tabButton;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x000198AC File Offset: 0x00017AAC
		public static GUIStyle TabButtonSelected
		{
			get
			{
				if (GUIStylePreset._tabButtonSelected == null)
				{
					GUIStylePreset._tabButtonSelected = new GUIStyle(GUI.skin.button)
					{
						font = GUIStylePreset.FontBold,
						fontSize = 13,
						padding = new RectOffset
						{
							left = 10,
							right = 10,
							top = 6,
							bottom = 10
						},
						margin = new RectOffset
						{
							left = 2,
							right = 2,
							top = 2,
							bottom = 2
						},
						alignment = (UnityEngine.TextAnchor)4,
						wordWrap = true,
						richText = true,
						normal = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._accent),
							textColor = new Color(0.05f, 0.05f, 0.07f, 1f)
						},
						hover = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._accentHov),
							textColor = new Color(0.05f, 0.05f, 0.07f, 1f)
						},
						active = 
						{
							background = GUIStylePreset.MakeTex(GUIStylePreset._accentHov),
							textColor = new Color(0.05f, 0.05f, 0.07f, 1f)
						}
					};
				}
				return GUIStylePreset._tabButtonSelected;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x00019A00 File Offset: 0x00017C00
		public static GUIStyle TabTitle
		{
			get
			{
				if (GUIStylePreset._tabTitle == null)
				{
					GUIStylePreset._tabTitle = new GUIStyle(GUI.skin.label)
					{
						font = GUIStylePreset.FontBold,
						fontSize = 20,
						alignment = (UnityEngine.TextAnchor)3,
						padding = new RectOffset
						{
							left = 8,
							right = 8,
							top = 6,
							bottom = 6
						},
						margin = new RectOffset
						{
							left = 0,
							right = 0,
							top = 0,
							bottom = 4
						},
						richText = true,
						normal = 
						{
							textColor = Color.white
						}
					};
				}
				return GUIStylePreset._tabTitle;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060001C3 RID: 451 RVA: 0x00019AB0 File Offset: 0x00017CB0
		public static GUIStyle TabSubtitle
		{
			get
			{
				if (GUIStylePreset._tabSubtitle == null)
				{
					GUIStylePreset._tabSubtitle = new GUIStyle(GUI.skin.label)
					{
						font = GUIStylePreset.FontBold,
						fontSize = 13,
						alignment = (UnityEngine.TextAnchor)3,
						padding = new RectOffset
						{
							left = 8,
							right = 8,
							top = 4,
							bottom = 4
						},
						margin = new RectOffset
						{
							left = 0,
							right = 0,
							top = 2,
							bottom = 2
						},
						richText = true,
						normal = 
						{
							textColor = new Color(0.6f, 0.75f, 1f, 1f)
						}
					};
				}
				return GUIStylePreset._tabSubtitle;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x00019B74 File Offset: 0x00017D74
		public static GUIStyle ModernBox
		{
			get
			{
				if (GUIStylePreset._modernBox == null)
				{
					GUIStylePreset._modernBox = new GUIStyle(GUI.skin.box)
					{
						normal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0f, 0f, 0f, 0f))
						},
						padding = new RectOffset
						{
							left = 8,
							right = 8,
							top = 8,
							bottom = 8
						},
						margin = new RectOffset
						{
							left = 3,
							right = 3,
							top = 4,
							bottom = 4
						},
						border = new RectOffset
						{
							left = 1,
							right = 1,
							top = 1,
							bottom = 1
						}
					};
				}
				return GUIStylePreset._modernBox;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x00019C44 File Offset: 0x00017E44
		public static GUIStyle SectionHeader
		{
			get
			{
				if (GUIStylePreset._sectionHeader == null)
				{
					GUIStylePreset._sectionHeader = new GUIStyle(GUI.skin.label)
					{
						font = GUIStylePreset.FontBold,
						fontSize = 14,
						alignment = (UnityEngine.TextAnchor)3,
						padding = new RectOffset
						{
							left = 6,
							right = 6,
							top = 4,
							bottom = 4
						},
						margin = new RectOffset
						{
							left = 2,
							right = 2,
							top = 6,
							bottom = 4
						},
						richText = true,
						normal = 
						{
							textColor = Color.white
						}
					};
				}
				return GUIStylePreset._sectionHeader;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x00019CF4 File Offset: 0x00017EF4
		public static GUIStyle ModernLabel
		{
			get
			{
				if (GUIStylePreset._modernLabel == null)
				{
					GUIStylePreset._modernLabel = new GUIStyle(GUI.skin.label)
					{
						font = GUIStylePreset.FontRegular,
						fontSize = 13,
						alignment = (UnityEngine.TextAnchor)3,
						padding = new RectOffset
						{
							left = 4,
							right = 4,
							top = 3,
							bottom = 3
						},
						margin = new RectOffset
						{
							left = 2,
							right = 2,
							top = 1,
							bottom = 1
						},
						richText = true,
						wordWrap = true,
						normal = 
						{
							textColor = GUIStylePreset._textDim
						}
					};
				}
				return GUIStylePreset._modernLabel;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x00019DAA File Offset: 0x00017FAA
		public static Texture2D SliderTrack
		{
			get
			{
				Texture2D result;
				if ((result = GUIStylePreset._sliderTrack) == null)
				{
					result = (GUIStylePreset._sliderTrack = GUIStylePreset.MakeTex(new Color(0.18f, 0.18f, 0.18f, 1f)));
				}
				return result;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x00019DD9 File Offset: 0x00017FD9
		public static Texture2D SliderThumb
		{
			get
			{
				Texture2D result;
				if ((result = GUIStylePreset._sliderThumb) == null)
				{
					result = (GUIStylePreset._sliderThumb = GUIStylePreset.MakeTex(new Color(0.6f, 1f, 0.99f, 1f)));
				}
				return result;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x00019E08 File Offset: 0x00018008
		public static GUIStyle WindowStyle
		{
			get
			{
				if (GUIStylePreset._windowStyle == null)
				{
					GUIStylePreset._windowStyle = new GUIStyle(GUI.skin.window)
					{
						normal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0.07f, 0.07f, 0.07f, 0.8f)),
							textColor = Color.white
						},
						onNormal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0.07f, 0.07f, 0.07f, 0.8f)),
							textColor = Color.white
						},
						fontSize = 13,
						fontStyle = (UnityEngine.FontStyle)1,
						alignment = (UnityEngine.TextAnchor)1,
						border = new RectOffset
						{
							left = 0,
							right = 0,
							top = 0,
							bottom = 0
						},
						padding = new RectOffset
						{
							left = 6,
							right = 6,
							top = 26,
							bottom = 6
						}
					};
				}
				return GUIStylePreset._windowStyle;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060001CA RID: 458 RVA: 0x00019F0F File Offset: 0x0001810F
		public static GUIStyle InfoLabel
		{
			get
			{
				return GUIStylePreset.ModernLabel;
			}
		}

		public static GUIStyle SectionBox
		{
			get
			{
				if (GUIStylePreset._sectionBox == null)
				{
					GUIStylePreset._sectionBox = new GUIStyle(GUI.skin.box)
					{
						normal =
						{
							background = GUIStylePreset.MakeRoundedTex(16, new Color(0.13f, 0.13f, 0.13f, 1f), new Color(0.13f, 0.13f, 0.13f, 1f), 4, 0),
							textColor = GUIStylePreset._text
						},
						border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 },
						padding = new RectOffset { left = 8, right = 8, top = 6, bottom = 6 },
						margin = new RectOffset { left = 2, right = 2, top = 4, bottom = 4 }
					};
				}
				return GUIStylePreset._sectionBox;
			}
		}

		public static GUIStyle NormalTextField
		{
			get
			{
				if (GUIStylePreset._normalTextField == null)
				{
					GUIStylePreset._normalTextField = new GUIStyle(GUI.skin.textField)
					{
						font = GUIStylePreset.FontRegular,
						fontSize = 13,
						alignment = (UnityEngine.TextAnchor)3,
						padding = new RectOffset
						{
							left = 6,
							right = 6,
							top = 3,
							bottom = 3
						},
						margin = new RectOffset
						{
							left = 3,
							right = 3,
							top = 3,
							bottom = 3
						},
						normal = 
						{
							background = GUIStylePreset.MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f)),
							textColor = GUIStylePreset._text
						},
						focused =
						{
							background = GUIStylePreset.MakeTex(new Color(0.21f, 0.21f, 0.21f, 1f)),
							textColor = Color.white
						},
						hover =
						{
							background = GUIStylePreset.MakeTex(new Color(0.18f, 0.18f, 0.18f, 1f)),
							textColor = GUIStylePreset._text
						}
					};
				}
				return GUIStylePreset._normalTextField;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060001CB RID: 459 RVA: 0x00019F18 File Offset: 0x00018118
		private static GUIStyle ToggleBoxOff
		{
			get
			{
				if (GUIStylePreset._toggleBoxOff == null)
				{
					GUIStylePreset._toggleBoxOff = new GUIStyle
					{
						normal = 
						{
							background = GUIStylePreset.GetToggleOff()
						},
						fixedWidth = 16f,
						fixedHeight = 16f,
						margin = new RectOffset
						{
							left = 3,
							right = 4,
							top = 3,
							bottom = 3
						},
						padding = new RectOffset()
					};
				}
				return GUIStylePreset._toggleBoxOff;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060001CC RID: 460 RVA: 0x00019F94 File Offset: 0x00018194
		private static GUIStyle ToggleBoxOn
		{
			get
			{
				if (GUIStylePreset._toggleBoxOn == null)
				{
					GUIStylePreset._toggleBoxOn = new GUIStyle
					{
						normal = 
						{
							background = GUIStylePreset.GetToggleOn()
						},
						fixedWidth = 16f,
						fixedHeight = 16f,
						margin = new RectOffset
						{
							left = 3,
							right = 4,
							top = 3,
							bottom = 3
						},
						padding = new RectOffset()
					};
				}
				return GUIStylePreset._toggleBoxOn;
			}
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0001A010 File Offset: 0x00018210
		private static readonly GUILayoutOption[] _toggleIconOpts  = { GUILayout.Width(16f), GUILayout.Height(16f) };
		private static readonly GUILayoutOption[] _toggleLabelOpts = { GUILayout.ExpandWidth(false) };

		public static bool CustomToggle(bool value, string label, params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal(options);
			bool clicked = GUILayout.Button("", value ? GUIStylePreset.ToggleBoxOn : GUIStylePreset.ToggleBoxOff, _toggleIconOpts);
			GUILayout.Label(label, GUIStylePreset.NormalToggle, _toggleLabelOpts);
			GUILayout.EndHorizontal();
			if (!clicked)
			{
				return value;
			}
			return !value;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0001A088 File Offset: 0x00018288
		public static void Reset()
		{
			GUIStylePreset._toggleOff = (GUIStylePreset._toggleOn = null);
			GUIStylePreset._toggleBoxOff = (GUIStylePreset._toggleBoxOn = null);
			GUIStylePreset._separator = (GUIStylePreset._darkSeparator = (GUIStylePreset._normalButton = (GUIStylePreset._normalToggle = null)));
			GUIStylePreset._tabButton = (GUIStylePreset._tabButtonSelected = (GUIStylePreset._tabTitle = (GUIStylePreset._tabSubtitle = null)));
			GUIStylePreset._modernBox = (GUIStylePreset._sectionHeader = (GUIStylePreset._modernLabel = (GUIStylePreset._normalTextField = (GUIStylePreset._sectionBox = (GUIStylePreset._windowStyle = null)))));
			GUIStylePreset._sliderTrack = (GUIStylePreset._sliderThumb = null);
			GUIStylePreset._fontRegular = (GUIStylePreset._fontBold = null);
			if (GUIStylePreset._toggleOff != null) { UnityEngine.Object.Destroy(GUIStylePreset._toggleOff); GUIStylePreset._toggleOff = null; }
			if (GUIStylePreset._toggleOn  != null) { UnityEngine.Object.Destroy(GUIStylePreset._toggleOn);  GUIStylePreset._toggleOn  = null; }
			if (GUIStylePreset._fontRegular != null) { UnityEngine.Object.Destroy(GUIStylePreset._fontRegular); }
			if (GUIStylePreset._fontBold    != null) { UnityEngine.Object.Destroy(GUIStylePreset._fontBold); }
			foreach (Texture2D tex in GUIStylePreset._texCache.Values)
			{
				if (tex != null) UnityEngine.Object.Destroy(tex);
			}
			GUIStylePreset._texCache.Clear();
		}

		// Token: 0x04000227 RID: 551
		private static GUIStyle _separator;

		// Token: 0x04000228 RID: 552
		private static GUIStyle _darkSeparator;

		// Token: 0x04000229 RID: 553
		private static GUIStyle _normalButton;

		// Token: 0x0400022A RID: 554
		private static GUIStyle _normalToggle;

		// Token: 0x0400022B RID: 555
		private static GUIStyle _tabButton;

		// Token: 0x0400022C RID: 556
		private static GUIStyle _tabButtonSelected;

		// Token: 0x0400022D RID: 557
		private static GUIStyle _tabTitle;

		// Token: 0x0400022E RID: 558
		private static GUIStyle _tabSubtitle;

		// Token: 0x0400022F RID: 559
		private static GUIStyle _modernBox;

		// Token: 0x04000230 RID: 560
		private static GUIStyle _sectionHeader;

		// Token: 0x04000231 RID: 561
		private static GUIStyle _modernLabel;
		private static GUIStyle _normalTextField;
		private static GUIStyle _sectionBox;

		// Token: 0x04000232 RID: 562
		private static readonly Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();

		// Token: 0x04000233 RID: 563
		private static Font _fontRegular;

		// Token: 0x04000234 RID: 564
		private static Font _fontBold;

		// Token: 0x04000235 RID: 565
		private static readonly Color _bg = new Color(0.1f, 0.1f, 0.1f, 1f);

		// Token: 0x04000236 RID: 566
		private static readonly Color _bgHover = new Color(0.16f, 0.16f, 0.16f, 1f);

		// Token: 0x04000237 RID: 567
		private static readonly Color _bgActive = new Color(0.22f, 0.22f, 0.22f, 1f);

		// Token: 0x04000238 RID: 568
		private static readonly Color _accent = new Color(0.6f, 1f, 0.99f, 1f);

		// Token: 0x04000239 RID: 569
		private static readonly Color _accentHov = new Color(0.75f, 1f, 0.99f, 1f);

		// Token: 0x0400023A RID: 570
		private static readonly Color _text = new Color(0.93f, 0.93f, 0.95f, 1f);

		// Token: 0x0400023B RID: 571
		private static readonly Color _textDim = new Color(0.7f, 0.7f, 0.73f, 1f);

		// Token: 0x0400023C RID: 572
		private static Texture2D _toggleOff;

		// Token: 0x0400023D RID: 573
		private static Texture2D _toggleOn;

		// Token: 0x0400023E RID: 574
		private static Texture2D _sliderTrack;

		// Token: 0x0400023F RID: 575
		private static Texture2D _sliderThumb;

		// Token: 0x04000240 RID: 576
		private static GUIStyle _windowStyle;

		// Token: 0x04000241 RID: 577
		private static GUIStyle _toggleBoxOff;

		// Token: 0x04000242 RID: 578
		private static GUIStyle _toggleBoxOn;
	}
}
