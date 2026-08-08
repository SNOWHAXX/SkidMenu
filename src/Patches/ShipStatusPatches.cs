using HarmonyLib;

namespace SkidMenu;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public static class ShipStatus_Start
{
    public static void Postfix()
    {
        features.KillAura.GameStartTime = UnityEngine.Time.time;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate
{
    public static void Postfix()
    {
        if (ShipStatus.Instance == null) return;

        MalumSabotageCheats.Process(ShipStatus.Instance);
        MalumCheats.OpenSabotageMapCheat();

        MalumCheats.CloseMeetingCheat();
        MalumCheats.SkipMeetingCheat();
        MalumCheats.CallMeetingCheat();
        MalumCheats.WalkInVentCheat();
        MalumCheats.KickVentsCheat();

        MalumCheats.DoAnyTaskCheat();

        MalumPPMCheats.ReportBodyPPM();

        if (ShipStatus.Instance is FungleShipStatus f)
            MalumSabotageCheats.ProcessFungle(f);
    }
}


