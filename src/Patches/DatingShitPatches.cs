using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using AmongUs.QuickChat;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace SkidMenu;

[HarmonyPatch(typeof(FindAGameManager))]
public static class FindDatersLobbyPatch
{
    private static bool _lastState;
    public static bool forceReapply = false;

    public static bool useImpostorFilter = true;
    public static int  impostorCount    = 1;
    public static bool usePlayerFilter  = true;
    public static int  minPlayers       = 4;
    public static int  maxPlayers       = 9;
    public static bool useChatFilter    = true;
    public static bool useLangFilter    = false;
    public static readonly HashSet<SupportedLangs> selectedLangs = new();
    public static bool useMapFilter     = false;
    public static MapNames mapFilter    = MapNames.Skeld;

    public static bool useHostPlatformFilter = false;
    public static readonly HashSet<Platforms> selectedPlatforms = new();
    public static bool useHostNameFilter   = false;
    public static List<string> hostNameKeywords = new();

    private static readonly string HostNameFilterPath = "SkidMenu/HostNameFilter.txt";

    public static void SaveHostNameFilter()
    {
        try { System.IO.Directory.CreateDirectory("SkidMenu"); System.IO.File.WriteAllLines(HostNameFilterPath, hostNameKeywords); } catch { }
    }

    public static void LoadHostNameFilter()
    {
        try
        {
            if (!System.IO.File.Exists(HostNameFilterPath)) return;
            hostNameKeywords = new List<string>(System.IO.File.ReadAllLines(HostNameFilterPath)
                .Select(l => l.Trim()).Where(l => l.Length > 0).ToList());
        }
        catch { }
    }

    public static bool ShouldShowListing(GameListing listing)
    {
        try
        {
            if (string.IsNullOrEmpty(listing.HostName)) return true;
            if (useHostPlatformFilter && selectedPlatforms.Count > 0)
                if (!selectedPlatforms.Contains(listing.Platform)) return false;
            if (useHostNameFilter && hostNameKeywords.Count > 0)
            {
                bool anyMatch = false;
                foreach (var kw in hostNameKeywords)
                    if (listing.HostName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) { anyMatch = true; break; }
                if (!anyMatch) return false;
            }
        }
        catch { }
        return true;
    }

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    public static void Update_Postfix(FindAGameManager __instance)
    {
        try
        {
            bool stateChanged = CheatToggles.findDaters != _lastState;
            if (stateChanged || forceReapply)
            {
                if (CheatToggles.findDaters)
                    ApplyFilters(__instance);
                else
                    ClearFilters(__instance);

                _lastState    = CheatToggles.findDaters;
                forceReapply  = false;
            }
        }
        catch (Exception ex)
        {
            BepInEx.Logging.Logger.CreateLogSource("SkidMenu").LogError("FindDatersLobby error: " + ex.Message);
        }
    }

    private static void ApplyFilters(FindAGameManager instance)
    {
        try
        {
            if ((UnityEngine.Object)(object)instance == (UnityEngine.Object)null) return;
            instance.ClearAllFilters();
            if (useImpostorFilter)
                instance.AddIntFilterValue(impostorCount, "NumImpostors", (Int32OptionNames)1);
            if (usePlayerFilter)
                for (int i = minPlayers; i <= maxPlayers; i++)
                    instance.AddIntFilterValue(i, "MaxPlayers", (Int32OptionNames)9);
            if (useChatFilter)
                instance.AddChatFilterValue((QuickChatModes)1, false);
            if (useLangFilter)
                foreach (var lang in selectedLangs)
                    instance.AddLangFilterValue((uint)lang);
            if (useMapFilter)
                instance.SetMapFilter((byte)mapFilter);
        }
        catch (Exception ex)
        {
            BepInEx.Logging.Logger.CreateLogSource("SkidMenu").LogError("FindDaters ApplyFilters error: " + ex.Message);
        }
    }

    private static void ClearFilters(FindAGameManager instance)
    {
        try
        {
            if ((UnityEngine.Object)(object)instance == (UnityEngine.Object)null) return;
            instance.ClearAllFilters();
        }
        catch (Exception ex)
        {
            BepInEx.Logging.Logger.CreateLogSource("SkidMenu").LogError("FindDaters ClearFilters error: " + ex.Message);
        }
    }

    public static void Reset() => _lastState = false;
}

[HarmonyPatch(typeof(FindAGameManager), "Start")]
public static class ExtendedLobbyListPatch
{
    private static Scroller _scroller;
    private static bool _hasSetup;
    public static int extraSlots = 15;

    [HarmonyPrefix]
    public static bool Prefix(FindAGameManager __instance)
    {
        if (!CheatToggles.extendedLobbyList)
        {
            Reset();
            return true;
        }

        try
        {
            if (_hasSetup) { _hasSetup = false; _scroller = null; }

            GameContainer baseContainer = __instance.gameContainers[4];

            GameObject scrollRoot = new GameObject("GameListScroller");
            scrollRoot.transform.SetParent(((Component)baseContainer).transform.parent);

            _scroller = scrollRoot.AddComponent<Scroller>();
            _scroller.Inner = scrollRoot.transform;
            _scroller.MouseMustBeOverToScroll = true;

            BoxCollider2D mask = ((Component)((Component)baseContainer).transform.parent).gameObject.AddComponent<BoxCollider2D>();
            mask.size = new Vector2(100f, 100f);
            ((PassiveUiElement)_scroller).ClickMask = (Collider2D)(object)mask;

            _scroller.ScrollWheelSpeed = 0.3f;
            _scroller.SetYBoundsMin(0f);
            _scroller.SetYBoundsMax(3.5f);
            _scroller.allowY = true;

            foreach (GameContainer container in __instance.gameContainers)
            {
                ((Component)container).transform.SetParent(scrollRoot.transform);
                Vector3 pos = ((Component)container).transform.position;
                ((Component)container).transform.position = new Vector3(pos.x, pos.y, 25f);
            }

            var expanded = new List<GameContainer>(__instance.gameContainers.ToArray().Cast<GameContainer>());
            for (int i = 0; i < extraSlots; i++)
            {
                GameContainer clone = UnityEngine.Object.Instantiate(baseContainer, scrollRoot.transform);
                Vector3 clonePos = ((Component)clone).transform.position;
                ((Component)clone).transform.position = new Vector3(clonePos.x, clonePos.y - 0.75f * (i + 1), 25f);
                expanded.Add(clone);
            }

            __instance.gameContainers = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GameContainer>(expanded.ToArray());

            GameObject cutoff = new GameObject("CutOffTop");
            SpriteRenderer sr = cutoff.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(100, 100);
            Color[] pixels = tex.GetPixels();
            for (int j = 0; j < pixels.Length; j++) pixels[j] = Color.black;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
            cutoff.transform.SetParent(scrollRoot.transform.parent);
            cutoff.transform.localPosition = new Vector3(0f, 3f, 1f);
            cutoff.transform.localScale = new Vector3(1500f, 200f, 100f);

            _hasSetup = true;
        }
        catch (Exception ex)
        {
            BepInEx.Logging.Logger.CreateLogSource("SkidMenu").LogError("ExtendedLobbyList error: " + ex.Message);
            Reset();
        }

        return true;
    }

    [HarmonyPatch(typeof(FindAGameManager), "RefreshList")]
    [HarmonyPostfix]
    public static void RefreshList_Postfix()
    {
        try
        {
            if (CheatToggles.extendedLobbyList && (UnityEngine.Object)(object)_scroller != (UnityEngine.Object)null)
                _scroller.ScrollRelative(new Vector2(0f, -100f));
        }
        catch (Exception ex)
        {
            BepInEx.Logging.Logger.CreateLogSource("SkidMenu").LogError("ExtendedLobbyList RefreshList error: " + ex.Message);
        }
    }

    public static void Reset()
    {
        _hasSetup = false;
        _scroller = null;
    }
}

[HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetGameListing))]
public static class GameContainerSetGamePatch
{
    public static void Postfix(GameContainer __instance, GameListing gameL)
    {
        if (!CheatToggles.findDaters || gameL == null) return;
        if (!FindDatersLobbyPatch.ShouldShowListing(gameL))
            ((UnityEngine.Component)__instance).gameObject.SetActive(false);
    }
}

