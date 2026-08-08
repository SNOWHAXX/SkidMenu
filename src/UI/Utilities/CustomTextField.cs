using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SkidMenu;

public static class CustomTextField
{
    private static readonly Dictionary<string, bool> Focused = new();
    private static readonly Dictionary<string, float> LastBlinkTime = new();
    private static readonly Dictionary<string, bool> CursorVisible = new();
    private static readonly Dictionary<string, Rect> FieldRects = new();
    private static readonly Dictionary<string, int> CursorPositions = new();
    private static readonly float CursorBlinkTime = 0.5f;

    public static bool IsFocused(string fieldKey)
        => Focused.TryGetValue(fieldKey, out bool f) && f;

    public static void Draw(ref string content, string fieldKey, int width = 200, int height = 20, string placeholder = "")
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(width), GUILayout.Height(height));

        if (Event.current.type == EventType.Repaint)
            FieldRects[fieldKey] = GUILayoutUtility.GetLastRect();

        if (!Focused.ContainsKey(fieldKey))
            Focused[fieldKey] = false;

        if (Event.current.type == EventType.MouseDown && FieldRects.ContainsKey(fieldKey))
        {
            if (FieldRects[fieldKey].Contains(Event.current.mousePosition))
            {
                Focused[fieldKey] = true;
                LastBlinkTime[fieldKey] = Time.time;
                CursorVisible[fieldKey] = true;
                Event.current.Use();
            }
            else
            {
                Focused[fieldKey] = false;
            }
        }

        if (Focused[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (!CursorPositions.ContainsKey(fieldKey)) CursorPositions[fieldKey] = content.Length;
            int cp = CursorPositions[fieldKey];
            cp = Mathf.Clamp(cp, 0, content.Length);

            bool ctrl = Event.current.control || Event.current.command;

            if (ctrl && Event.current.keyCode == KeyCode.C)
            {
                GUIUtility.systemCopyBuffer = content;
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.X)
            {
                GUIUtility.systemCopyBuffer = content;
                content = "";
                cp = 0;
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.V)
            {
                string clip = GUIUtility.systemCopyBuffer ?? "";
                var sb = new StringBuilder();
                foreach (char c in clip) if (!char.IsControl(c)) sb.Append(c);
                clip = sb.ToString();
                content = content.Substring(0, cp) + clip + content.Substring(cp);
                cp = Mathf.Clamp(cp + clip.Length, 0, content.Length);
                Event.current.Use();
            }
            else if (ctrl && Event.current.keyCode == KeyCode.A)
            {
                GUIUtility.systemCopyBuffer = content;
                cp = content.Length;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; }
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Delete)
            {
                if (cp < content.Length) content = content.Substring(0, cp) + content.Substring(cp + 1);
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.LeftArrow)
            {
                if (cp > 0) cp--;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.RightArrow)
            {
                if (cp < content.Length) cp++;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Home)
            {
                cp = 0;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.End)
            {
                cp = content.Length;
                Event.current.Use();
            }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            {
                content = content.Substring(0, cp) + Event.current.character + content.Substring(cp);
                cp++;
                Event.current.Use();
            }

            CursorPositions[fieldKey] = Mathf.Clamp(cp, 0, content.Length);
        }

        if (FieldRects.ContainsKey(fieldKey))
        {
            Rect rect = FieldRects[fieldKey];
            string shown = content;
            bool showPlaceholder = content.Length == 0 && placeholder.Length > 0 && !Focused[fieldKey];
            if (showPlaceholder) shown = placeholder;

            Color prev = GUI.color;
            if (showPlaceholder)
                GUI.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            GUI.Label(new Rect(rect.x + 5, rect.y + 2, rect.width - 10, rect.height), shown);
            GUI.color = prev;

            if (Focused[fieldKey])
            {
                if (!LastBlinkTime.ContainsKey(fieldKey)) LastBlinkTime[fieldKey] = Time.time;
                if (Time.time - LastBlinkTime[fieldKey] > CursorBlinkTime)
                {
                    CursorVisible[fieldKey] = !CursorVisible[fieldKey];
                    LastBlinkTime[fieldKey] = Time.time;
                }
                if (CursorVisible[fieldKey])
                {
                    int cp2 = CursorPositions.ContainsKey(fieldKey) ? Mathf.Clamp(CursorPositions[fieldKey], 0, content.Length) : content.Length;
                    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp2)));
                    GUI.Label(new Rect(rect.x + textSize.x + 7, rect.y + 2, 10, rect.height - 4), "|");
                }
            }
        }
    }
}
