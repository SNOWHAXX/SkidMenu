using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;
public static class MinimapHandler
{
    public static bool minimapActive;
    public static List<HerePoint> herePoints = new List<HerePoint>();
    public static List<HerePoint> herePointsToRemove = new List<HerePoint>();

    public static bool IsCheatEnabled()
    {
        return CheatToggles.mapCrew || CheatToggles.mapGhosts || CheatToggles.mapImps;
    }

    public static void HandleHerePoint(HerePoint herePoint)
    {
        Color herePointColor = new Color();

        try // try-catch to fix issues caused by player disconnection
        {
            herePoint.sprite.gameObject.SetActive(false); // Initally make player icon invisible

            // Crewmate, alive
            if (CheatToggles.mapCrew && !herePoint.player.Data.Role.IsImpostor)
            {
                if (!herePoint.player.Data.IsDead)
                {
                    herePoint.sprite.gameObject.SetActive(true);
                    herePointColor = CheatToggles.distanceBasedTracers
                        ? TracersHandler.GetDistanceColor(herePoint.player.transform.position)
                        : CheatToggles.colorBasedTracers
                            ? Palette.PlayerColors[herePoint.player.Data.DefaultOutfit.ColorId]
                            : Utils.GetCustomRoleColor(herePoint.player.Data);
                }
            }
            // Impostor, alive
            else if (CheatToggles.mapImps && herePoint.player.Data.Role.IsImpostor)
            {
                if (!herePoint.player.Data.IsDead)
                {
                    herePoint.sprite.gameObject.SetActive(true);
                    herePointColor = CheatToggles.distanceBasedTracers
                        ? TracersHandler.GetDistanceColor(herePoint.player.transform.position)
                        : CheatToggles.colorBasedTracers
                            ? Palette.PlayerColors[herePoint.player.Data.DefaultOutfit.ColorId]
                            : Utils.GetCustomRoleColor(herePoint.player.Data);
                }
            }
            // Any Role, dead
            if (CheatToggles.mapGhosts && herePoint.player.Data.IsDead)
            {
                herePoint.sprite.gameObject.SetActive(true);
                if (CheatToggles.colorBasedTracers)
                {
                    herePointColor = herePoint.player.Data.Color;
                }
                else
                {
                    herePointColor = Palette.White;
                }
            }

            if (herePoint.sprite.gameObject.active)
            {
                // Set the right colors for active herePoint icons
                herePoint.sprite.material.SetColor(PlayerMaterial.BackColor, herePointColor);
                herePoint.sprite.material.SetColor(PlayerMaterial.BodyColor, herePointColor);
                herePoint.sprite.material.SetColor(PlayerMaterial.VisorColor, Palette.VisorColor);

                // Sync the position of active herePoint icons with their players
                var vector = herePoint.player.transform.position;
                vector /= ShipStatus.Instance.MapScale;
                vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                vector.z = -1f;
                herePoint.sprite.transform.localPosition = vector;
            }
        }
        catch
        {
            // Remove icons that are causing problems
            Object.Destroy(herePoint.sprite.gameObject);
            herePointsToRemove.Add(herePoint);
        }
    }
}
