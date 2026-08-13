using UnityEngine;

namespace SkidMenu;

internal class DatingShitTab : ITab
{
    private string _hostNameInput = "";
    public string name => "Lobby Finding";
    private Vector2 _scrollPosition = Vector2.zero;

    private void AddHostNameKeyword()
    {
        if (string.IsNullOrWhiteSpace(_hostNameInput)) return;
        var t = _hostNameInput.Trim();
        if (!FindDatersLobbyPatch.hostNameKeywords.Contains(t)) { FindDatersLobbyPatch.hostNameKeywords.Add(t); FindDatersLobbyPatch.forceReapply = true; }
        _hostNameInput = "";
    }

    public void Draw()
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Label("Tools");
        GUILayout.Space(6);

        bool newFindDaters = GUIStylePreset.CustomToggle(CheatToggles.findDaters, "Filter Lobby Search");
        if (newFindDaters != CheatToggles.findDaters)
        {
            CheatToggles.findDaters = newFindDaters;
            FindDatersLobbyPatch.forceReapply = true;
        }

        if (CheatToggles.findDaters)
        {
            GUILayout.Space(8);
            GUILayout.BeginVertical(GUIStylePreset.SectionBox);
            GUILayout.Label("Filter Settings");
            GUILayout.Space(8);

            bool newUseImp = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.useImpostorFilter, "Filter by impostor count");
            if (newUseImp != FindDatersLobbyPatch.useImpostorFilter) { FindDatersLobbyPatch.useImpostorFilter = newUseImp; FindDatersLobbyPatch.forceReapply = true; }
            if (FindDatersLobbyPatch.useImpostorFilter)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Impostors: {FindDatersLobbyPatch.impostorCount}", GUILayout.Width(110f));
                int newImp = Mathf.RoundToInt(GUILayout.HorizontalSlider(FindDatersLobbyPatch.impostorCount, 1f, 3f));
                if (newImp != FindDatersLobbyPatch.impostorCount) { FindDatersLobbyPatch.impostorCount = newImp; FindDatersLobbyPatch.forceReapply = true; }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            bool newUsePlayers = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.usePlayerFilter, "Filter by player count");
            if (newUsePlayers != FindDatersLobbyPatch.usePlayerFilter) { FindDatersLobbyPatch.usePlayerFilter = newUsePlayers; FindDatersLobbyPatch.forceReapply = true; }
            if (FindDatersLobbyPatch.usePlayerFilter)
            {
                GUILayout.Space(4);
                int newMin = Mathf.RoundToInt(GUILayout.HorizontalSlider(FindDatersLobbyPatch.minPlayers, 1f, 15f));
                int newMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(FindDatersLobbyPatch.maxPlayers, 1f, 15f));
                newMin = Mathf.Clamp(newMin, 1, newMax);
                newMax = Mathf.Clamp(newMax, newMin, 15);
                GUILayout.Label($"Players: {newMin} – {newMax}");
                if (newMin != FindDatersLobbyPatch.minPlayers || newMax != FindDatersLobbyPatch.maxPlayers)
                {
                    FindDatersLobbyPatch.minPlayers = newMin;
                    FindDatersLobbyPatch.maxPlayers = newMax;
                    FindDatersLobbyPatch.forceReapply = true;
                }
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            bool newUseChat = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.useChatFilter, "Free chat only");
            if (newUseChat != FindDatersLobbyPatch.useChatFilter) { FindDatersLobbyPatch.useChatFilter = newUseChat; FindDatersLobbyPatch.forceReapply = true; }

            GUILayout.Space(6);
            bool newUseLang = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.useLangFilter, "Filter by chat language");
            if (newUseLang != FindDatersLobbyPatch.useLangFilter) { FindDatersLobbyPatch.useLangFilter = newUseLang; FindDatersLobbyPatch.forceReapply = true; }
            if (FindDatersLobbyPatch.useLangFilter)
            {
                GUILayout.Space(4);
                var langs = (SupportedLangs[])System.Enum.GetValues(typeof(SupportedLangs));
                for (int i = 0; i < langs.Length; i += 3)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = i; j < i + 3 && j < langs.Length; j++)
                    {
                        bool sel = FindDatersLobbyPatch.selectedLangs.Contains(langs[j]);
                        bool newSel = GUIStylePreset.CustomToggle(sel, langs[j].ToString(), GUILayout.Width(100f));
                        if (newSel != sel) { if (newSel) FindDatersLobbyPatch.selectedLangs.Add(langs[j]); else FindDatersLobbyPatch.selectedLangs.Remove(langs[j]); FindDatersLobbyPatch.forceReapply = true; }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            bool newUseHostPlatform = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.useHostPlatformFilter, "Filter by host platform");
            if (newUseHostPlatform != FindDatersLobbyPatch.useHostPlatformFilter) { FindDatersLobbyPatch.useHostPlatformFilter = newUseHostPlatform; FindDatersLobbyPatch.forceReapply = true; }
            if (FindDatersLobbyPatch.useHostPlatformFilter)
            {
                GUILayout.Space(4);
                var platforms = (Platforms[])System.Enum.GetValues(typeof(Platforms));
                for (int i = 0; i < platforms.Length; i += 3)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = i; j < i + 3 && j < platforms.Length; j++)
                    {
                        bool sel = FindDatersLobbyPatch.selectedPlatforms.Contains(platforms[j]);
                        bool newSel = GUIStylePreset.CustomToggle(sel, platforms[j].ToString(), GUILayout.Width(100f));
                        if (newSel != sel) { if (newSel) FindDatersLobbyPatch.selectedPlatforms.Add(platforms[j]); else FindDatersLobbyPatch.selectedPlatforms.Remove(platforms[j]); FindDatersLobbyPatch.forceReapply = true; }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            bool newUseHostName = GUIStylePreset.CustomToggle(FindDatersLobbyPatch.useHostNameFilter, "Filter by host name contains");
            if (newUseHostName != FindDatersLobbyPatch.useHostNameFilter) { FindDatersLobbyPatch.useHostNameFilter = newUseHostName; FindDatersLobbyPatch.forceReapply = true; }
            if (FindDatersLobbyPatch.useHostNameFilter)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                CustomTextField.Draw(ref _hostNameInput, "hostName", 190, 20, "enter host name");
                if (GUILayout.Button("Add", GUIStylePreset.NormalButton, GUILayout.Width(55f)))
                    AddHostNameKeyword();
                GUILayout.EndHorizontal();

                bool enterHit = CustomTextField.IsFocused("hostName")
                    && Event.current.type == EventType.KeyDown
                    && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
                if (enterHit)
                {
                    AddHostNameKeyword();
                    Event.current.Use();
                }

                GUILayout.Space(4);
                for (int i = FindDatersLobbyPatch.hostNameKeywords.Count - 1; i >= 0; i--)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(FindDatersLobbyPatch.hostNameKeywords[i]);
                    if (GUILayout.Button("X", GUIStylePreset.NormalButton, GUILayout.Width(30f))) { FindDatersLobbyPatch.hostNameKeywords.RemoveAt(i); FindDatersLobbyPatch.forceReapply = true; }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save", GUIStylePreset.NormalButton)) FindDatersLobbyPatch.SaveHostNameFilter();
                if (GUILayout.Button("Clear All", GUIStylePreset.NormalButton)) { FindDatersLobbyPatch.hostNameKeywords.Clear(); FindDatersLobbyPatch.forceReapply = true; }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            GUILayout.Space(6);
            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        bool newExtended = GUIStylePreset.CustomToggle(CheatToggles.extendedLobbyList, "Extended Lobby List");
        if (newExtended != CheatToggles.extendedLobbyList)
        {
            CheatToggles.extendedLobbyList = newExtended;
        }
        if (CheatToggles.extendedLobbyList)
        {
            GUILayout.Space(6);
            GUILayout.BeginVertical(GUIStylePreset.SectionBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Extra slots: {ExtendedLobbyListPatch.extraSlots}", GUILayout.Width(120f));
            ExtendedLobbyListPatch.extraSlots = Mathf.RoundToInt(GUILayout.HorizontalSlider(ExtendedLobbyListPatch.extraSlots, 5f, 30f));
            GUILayout.EndHorizontal();
            GUILayout.Label("<size=11>Toggle off/on to apply changes</size>");
            GUILayout.EndVertical();
        }

        GUILayout.Space(15);

        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Info");
        GUILayout.Space(4);
        GUILayout.Label("Lobby Finding: Filters lobby browser for more rooms (customizable)");
        GUILayout.Space(2);
        GUILayout.Label("Extended Lobby List: Expands lobby browser with extra scrollable slots");
        GUILayout.EndVertical();

        GUILayout.EndScrollView();
    }
}
