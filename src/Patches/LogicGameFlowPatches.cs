using HarmonyLib;

namespace SkidMenu;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormal_CheckEndCriteria
{
    // Prefix patch of LogicGameFlowNormal.CheckEndCriteria to prevent a running game from ending
    public static bool Prefix()
    {
        return !CheatToggles.noGameEnd && !features.FuckGame.Enabled;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class LogicGameFlowNormal_IsGameOverDueToDeath
{
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.noGameEnd || features.FuckGame.Enabled)
        {
            __result = false;
        }

    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
public static class LogicGameFlowHnS_CheckEndCriteria
{
    public static bool Prefix()
    {
        return !CheatToggles.noGameEnd && !features.FuckGame.Enabled;
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
public static class LogicGameFlowHnS_IsGameOverDueToDeath
{
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.noGameEnd || features.FuckGame.Enabled)
        {
            __result = false;
        }

    }
}
