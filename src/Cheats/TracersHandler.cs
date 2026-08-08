using UnityEngine;

namespace SkidMenu;

public static class TracersHandler
{
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        try
        {
            var data = playerPhysics.myPlayer.Data;
            if (data == null) return;

            bool anyEnabled = CheatToggles.tracersCrew || CheatToggles.tracersImps || CheatToggles.tracersGhosts;

            var color = Color.clear;

            if (!anyEnabled)
            {
                // still call DrawTracer so LineRenderer gets disabled
            }
            else if (!data.IsDead)
            {
                if (data.Role != null)
                {
                    bool isImp = data.Role.IsImpostor;
                    bool shouldDraw = (CheatToggles.tracersImps && isImp) || (CheatToggles.tracersCrew && !isImp);
                    if (shouldDraw)
                        color = CheatToggles.distanceBasedTracers
                            ? GetDistanceColor(playerPhysics.myPlayer.transform.position)
                            : CheatToggles.colorBasedTracers
                                ? WithAlpha(Palette.PlayerColors[data.DefaultOutfit.ColorId], 1f)
                                : WithAlpha(Utils.GetCustomRoleColor(data), 1f);
                }
            }
            else if (CheatToggles.tracersGhosts)
            {
                color = CheatToggles.distanceBasedTracers
                    ? GetDistanceColor(playerPhysics.myPlayer.transform.position)
                    : CheatToggles.colorBasedTracers
                        ? WithAlpha(Palette.PlayerColors[data.DefaultOutfit.ColorId], 1f)
                        : Palette.White;
            }

            Utils.DrawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        }
        catch { }
    }

    public static void DrawBodyTracer(DeadBody deadBody)
    {
        try
        {
            if (!deadBody || !deadBody.gameObject) return;

            if (!deadBody.gameObject.activeInHierarchy || ViperBodies.IsFullyDissolved(deadBody))
            {
                Utils.HideTracer(deadBody.gameObject);
                return;
            }

            if (deadBody is ViperDeadBody viperBody)
            {
                try
                {
                    var acid = viperBody.acidRenderer;
                    var splash = viperBody.splashRenderer;
                    bool acidGone   = acid   == null || !acid.enabled   || acid.color.a   < 0.01f;
                    bool splashGone = splash == null || !splash.enabled || splash.color.a < 0.01f;
                    if (acidGone && splashGone)
                    {
                        Utils.HideTracer(deadBody.gameObject);
                        return;
                    }
                }
                catch { }
            }

            var color = Color.clear;

            if (CheatToggles.tracersBodies)
            {
                var info = GameData.Instance.GetPlayerById(deadBody.ParentId);
                color = CheatToggles.distanceBasedTracers
                    ? GetDistanceColor(deadBody.transform.position)
                    : CheatToggles.colorBasedTracers && info != null
                        ? WithAlpha(Palette.PlayerColors[info.DefaultOutfit.ColorId], 1f)
                        : Color.white;
            }

            Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color, 0.07f);
        }
        catch { }
    }

    private static Color WithAlpha(Color c, float a) { c.a = a; return c; }

    public static Color GetDistanceColor(Vector3 targetPosition)
    {
        const float maxDistSqr = 20f * 20f;
        const float minDistSqr = 2f  * 2f;
        var sqrDist = (targetPosition - PlayerControl.LocalPlayer.transform.position).sqrMagnitude;
        var normalized = Mathf.InverseLerp(minDistSqr, maxDistSqr, sqrDist);
        return normalized < 0.5f
            ? Color.Lerp(Color.red, Color.yellow, normalized * 2f)
            : Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f);
    }
}
