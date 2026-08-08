using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace SkidMenu;

public class ProtectUI : MonoBehaviour
{
    public static int windowHeight = 300;
    public static int windowWidth = 620;
    private Rect _windowRect;
    public static Rect LastWindowRect;

    private Vector2 _scrollPosition = Vector2.zero;
    public static List<PlayerControl> playersToProtect = new();
    private bool _keepEveryoneProtected;

    private void Start()
    {
        // Instantiate 2D area of ProtectUI
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showProtectMenu || !(MenuUI.isGUIActive || SkidMenu.menuKeepSubwindowsOpen.Value) || SkidMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();

        Matrix4x4 prev = GUI.matrix;
        if (CheatToggles.protectScaleH != 100f || CheatToggles.protectScaleV != 100f)
        {
            Vector2 pivot = new Vector2(_windowRect.x + _windowRect.width * 0.5f, _windowRect.y + _windowRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(new Vector2(CheatToggles.protectScaleH / 100f, CheatToggles.protectScaleV / 100f), pivot);
        }
        _windowRect = GUI.Window((int)WindowId.ProtectUI, _windowRect, (GUI.WindowFunction)ProtectWindow, "Protect Players", GUIStylePreset.WindowStyle);
        LastWindowRect = _windowRect;
        GUI.matrix = prev;
    }

    private void ProtectWindow(int windowID)
    {
        GUI.skin = MenuUI.GetCustomSkin();
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName))
            {
                if (playersToProtect.Contains(player))  // Ensure to remove invalid players from the list
                {
                    playersToProtect.Remove(player);
                }

                continue;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label($"<color=#{ColorCache.ToHex(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));

            if (player.protectedByGuardianId == -1)
            {
                GUILayout.Label("<color=#FF0000>Unprotected</color>", GUILayout.Width(135));
            }
            else
            {
                NetworkedPlayerInfo guardianInfo = GameData.Instance.GetPlayerById((byte)player.protectedByGuardianId);
                GUILayout.Label($"<color=#00FF00>Protected</color> by <color=#{ColorCache.ToHex(guardianInfo.Color)}>{guardianInfo._object.Data.PlayerName}</color>", GUILayout.Width(135));
            }

            if (GUILayout.Button("Protect", GUIStylePreset.NormalButton) && Utils.isHost && !Utils.isLobby)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }

            var keepProtected = playersToProtect.Contains(player);
            var newKeepProtected = GUIStylePreset.CustomToggle(keepProtected, "Keep protected");

            if (newKeepProtected != keepProtected)
            {
                if (newKeepProtected) playersToProtect.Add(player);
                else playersToProtect.Remove(player);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Protect Everyone", GUIStylePreset.NormalButton, GUILayout.Height(30)) && Utils.isHost && !Utils.isLobby)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }
        }

        GUILayout.FlexibleSpace();

        _keepEveryoneProtected = GUIStylePreset.CustomToggle(_keepEveryoneProtected, "Keep Everyone Protected");

        if (_keepEveryoneProtected)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!playersToProtect.Contains(player))
                {
                    playersToProtect.Add(player);
                }
            }
        }
        else
        {
            if (PlayerControl.AllPlayerControls.Count == playersToProtect.Count)  // Only clear the list if all players were being kept protected
            {
                playersToProtect.Clear();
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        MenuUI.DrawBgAndOverlay(windowWidth, windowHeight);
        GUI.DragWindow();
    }
}




