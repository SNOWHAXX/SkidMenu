using System;
using System.Collections;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace SkidMenu;

public static class MalumRandomizer
{
    // Kept for LobbyBehaviour_Start guard compatibility.
    public static bool isRejoinInProgress = false;
    private static bool isLobbyApplyInProgress;

    private const float LobbyLoadWaitTimeoutSeconds = 8.0f;
    private const float LobbyLoadPollSeconds = 0.2f;
    private const float LobbyPostLoadDelaySeconds = 0.5f;

    // Randomly sets a new name, color, and cosmetics for the local player.
    public static void Randomize()
    {
        try
        {
            if (isRejoinInProgress) return;

            var randomizeName = !Utils.isLobby && !Utils.isInGame && !Utils.isPlayer;

            // Apply random cosmetics to DataManager (safe at any time, no network calls).
            ApplyRandomCosmetics(randomizeName);

            if (Utils.isLobby)
            {
                if (!Utils.isPlayer) return;

                if (!isLobbyApplyInProgress && AmongUsClient.Instance != null)
                {
                    AmongUsClient.Instance.StartCoroutine(ApplyLobbyCosmeticsWhenReady());
                }

                return;
            }

            if (Utils.isInGame)
            {
                // During an active game, skip RPC calls entirely.
                // The updated DataManager values (including name) will apply outside the round.
                return;
            }

            if (!Utils.isPlayer) return;

            // Offline or in free-play: broadcast cosmetics directly via RPC.
            OutfitBypass.SetColor(AmongUsClient.Instance.AmHost ? DataManager.Player.Customization.Color : Utilities.GetFreeColor());
            if (randomizeName)
            {
                OutfitBypass.SetName(DataManager.Player.Customization.Name);
            }
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
        }
        catch (Exception e)
        {
            SkidMenu.Log.LogError($"Randomizer error: {e.Message}");
        }
    }

    // Applies random cosmetics to DataManager only (no network calls).
    private static void ApplyRandomCosmetics(bool randomizeName)
    {
        if (randomizeName)
        {
            DataManager.Player.Customization.Name = Utils.GetRandomName();
        }

        DataManager.Player.Customization.Color = (byte)UnityEngine.Random.Range(0, Palette.PlayerColors.Length);

        var hatManager = DestroyableSingleton<HatManager>.Instance;
        if (hatManager != null)
        {
            var allHats = hatManager.allHats;
            var allSkins = hatManager.allSkins;
            var allVisors = hatManager.allVisors;
            var allPets = hatManager.allPets;

            if (allHats != null && allHats.Count > 0)
                DataManager.Player.Customization.Hat = allHats[UnityEngine.Random.Range(0, allHats.Count)].ProdId;

            if (allSkins != null && allSkins.Count > 0)
                DataManager.Player.Customization.Skin = allSkins[UnityEngine.Random.Range(0, allSkins.Count)].ProdId;

            if (allVisors != null && allVisors.Count > 0)
                DataManager.Player.Customization.Visor = allVisors[UnityEngine.Random.Range(0, allVisors.Count)].ProdId;

            if (allPets != null && allPets.Count > 0)
                DataManager.Player.Customization.Pet = allPets[UnityEngine.Random.Range(0, allPets.Count)].ProdId;
        }

        DataManager.Player.Save();
    }

    private static IEnumerator ApplyLobbyCosmeticsWhenReady()
    {
        isLobbyApplyInProgress = true;

        float waited = 0f;
        while (waited < LobbyLoadWaitTimeoutSeconds)
        {
            if (Utils.isLobby &&
                Utils.isPlayer &&
                PlayerControl.LocalPlayer.Data != null &&
                !PlayerControl.LocalPlayer.Data.Disconnected &&
                PlayerControl.LocalPlayer.Collider != null &&
                PlayerControl.LocalPlayer.NetTransform != null)
            {
                break;
            }

            waited += LobbyLoadPollSeconds;
            yield return new WaitForSeconds(LobbyLoadPollSeconds);
        }

        if (Utils.isLobby && Utils.isPlayer)
        {
            yield return new WaitForSeconds(LobbyPostLoadDelaySeconds);

            OutfitBypass.SetColor(AmongUsClient.Instance.AmHost ? DataManager.Player.Customization.Color : Utilities.GetFreeColor());
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
        }

        isLobbyApplyInProgress = false;
    }

}
