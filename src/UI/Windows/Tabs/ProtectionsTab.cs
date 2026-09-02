using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class ProtectionsTab : ITab
{
    public string name => "Protections";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        // Network
        Protections.ForceDTLS.Enabled = GUIStylePreset.CustomToggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

        GUILayout.BeginHorizontal();
        Protections.BlockServerTeleports.Enabled = GUIStylePreset.CustomToggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server", GUILayout.Width(260));
        GUILayout.Label("<color=red>Can cause desyncs, known for weird bugs with you venting and other people venting</color>", new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true });
        GUILayout.EndHorizontal();

        // Overloads
        Protections.HardenedReadPackedUInt.Enabled = GUIStylePreset.CustomToggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packed int deserializer");
        Protections.BlockInvalidLadderOverload = GUIStylePreset.CustomToggle(Protections.BlockInvalidLadderOverload, "Protect against invalid ladder overload");
        Protections.BlockLargeGameMessages = GUIStylePreset.CustomToggle(Protections.BlockLargeGameMessages, "Block large game messages");
        Protections.BlockInvalidGameDataMessages = GUIStylePreset.CustomToggle(Protections.BlockInvalidGameDataMessages, "Block invalid game data message types");
        Protections.BlockUnauthorizedSystemUpdates = GUIStylePreset.CustomToggle(Protections.BlockUnauthorizedSystemUpdates, "Block unauthorized system updates");
        Protections.ProtectAgainstNonHostKickExploit = GUIStylePreset.CustomToggle(Protections.ProtectAgainstNonHostKickExploit, "Protect against non-host kick exploit");
        Protections.BlockZiplineForce = GUIStylePreset.CustomToggle(Protections.BlockZiplineForce, "Block forced ziplines");
        Protections.BlockVentTpForce = GUIStylePreset.CustomToggle(Protections.BlockVentTpForce, "Block forced vent teleports");

        Protections.Votekicks.Enabled = GUIStylePreset.CustomToggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");
        Protections.MemoryAllocationOverload.Enabled = GUIStylePreset.CustomToggle(Protections.MemoryAllocationOverload.Enabled, "Protect against VotingComplete overloads");
        Protections.BypassShapeshiftRatelimits.Enabled = GUIStylePreset.CustomToggle(Protections.BypassShapeshiftRatelimits.Enabled, "Bypass ratelimits for Shapeshift RPC");
        Protections.AntiCrash.Enabled = GUIStylePreset.CustomToggle(Protections.AntiCrash.Enabled, "Protect against report-body crash exploit");
        

        GUILayout.BeginHorizontal();
        Protections.AntiExploits = GUIStylePreset.CustomToggle(Protections.AntiExploits, "Anti-Exploits", GUILayout.Width(260));
        GUILayout.Label("<color=red>This function is unstable and is known to make you unable to rejoin a lobby after the game ended</color>", new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true });
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}