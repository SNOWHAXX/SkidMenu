using UnityEngine;
using SkidMenu.features;

namespace SkidMenu;

public class ESPTab : ITab
{
    public string name => "ESP";

    public void Draw()
    {
        DrawGeneral();
        GUILayout.Space(10);
        DrawCamera();
        GUILayout.Space(10);
        DrawResizeHUD();
        GUILayout.Space(10);
        DrawTracers();
        GUILayout.Space(10);
        DrawMinimap();
        GUILayout.Space(10);
        DrawRadar();
    }

    private void DrawGeneral()
    {
        GUILayout.Label("Roles", GUIStylePreset.TabSubtitle);
        CtxRow(" Show Role", ref CheatToggles.espShowRole, ref ESPContexts.ShowRole, withSimple: true);
        if (CheatToggles.espShowRole)
        {
            CtxRow("   Kill Cooldown",        ref CheatToggles.espKillCooldown, ref ESPContexts.KillCooldown);
            CtxRow("   Tasks Done/Remaining", ref CheatToggles.espTasks,        ref ESPContexts.Tasks);
        }

        GUILayout.Space(6);
        GUILayout.Label("Player Info", GUIStylePreset.TabSubtitle);
        CtxRow(" Show Player Info", ref CheatToggles.espShowPlayerInfo, ref ESPContexts.ShowInfo);
        if (CheatToggles.espShowPlayerInfo)
        {
            CtxRow("   Is Host",     ref CheatToggles.espIsHost,     ref ESPContexts.IsHost);
            CtxRow("   Mod User",   ref CheatToggles.espModUser,   ref ESPContexts.ModUser);
            CtxRow("   Level",       ref CheatToggles.espLevel,       ref ESPContexts.Level);
            CtxRow("   Platform",    ref CheatToggles.espPlatform,    ref ESPContexts.Platform);
            CtxRow("   Votekicks",   ref CheatToggles.espVotekicks,   ref ESPContexts.Votekicks);
            CtxRow("   Friend Code", ref CheatToggles.espFriendCode,  ref ESPContexts.FriendCode);
            CtxRow("   PUID",        ref CheatToggles.espPuid,        ref ESPContexts.Puid);
            CtxRow("   Device ID",   ref CheatToggles.espDeviceId,    ref ESPContexts.DeviceId);
        }

        GUILayout.Space(6);
        GUILayout.Label("Visibility", GUIStylePreset.TabSubtitle);
        CheatToggles.seeGhosts            = GUIStylePreset.CustomToggle(CheatToggles.seeGhosts, " See Ghosts");
        SeePlayersInVents.Enabled         = GUIStylePreset.CustomToggle(SeePlayersInVents.Enabled, " See Players In Vents");
        SeePlayersInVents.SeePhantoms     = GUIStylePreset.CustomToggle(SeePlayersInVents.SeePhantoms, " See Phantoms");
        CheatToggles.revealVotes          = GUIStylePreset.CustomToggle(CheatToggles.revealVotes, " Reveal Votes");

        GUILayout.Space(6);
        GUILayout.Label("Notifications", GUIStylePreset.TabSubtitle);
        GUILayout.Label("  hide notifs from →", GUIStylePreset.ModernLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("", _w180);
        GUILayout.Label("Me",   _w35);
        GUILayout.Label("Host", _w40);
        GUILayout.EndHorizontal();

        NotifRow(ref CheatToggles.notifKill,       " Kill",              0);
        NotifRow(ref CheatToggles.notifSabotage,   " Sabotage",          1);
        NotifRow(ref CheatToggles.notifVent,       " Enter Vent",        2);
        NotifRow(ref CheatToggles.notifExitVent,   " Exit Vent",         3);
        NotifRow(ref CheatToggles.notifShapeshift,       " Shapeshift",        4);
        NotifRow(ref CheatToggles.notifShapeshiftRevert, " Shapeshift Revert", 15);
        NotifRow(ref CheatToggles.notifPhantom,          " Phantom Vanish",    5);
        NotifRow(ref CheatToggles.notifPhantomReappear,  " Phantom Reappear",  16);
        NotifRow(ref CheatToggles.notifMeeting,    " Emergency Meeting", 6);
        NotifRow(ref CheatToggles.notifBodyReport, " Body Report",       7);
        NotifRow(ref CheatToggles.notifVote,       " Vote Cast",         8);
        NotifRow(ref CheatToggles.notifVotekick,   " Votekick",          9);
        NotifRow(ref CheatToggles.notifChat,       " Chat Message",      10);
        NotifRow(ref CheatToggles.notifDisconnect, " Disconnect",        11);
        NotifRow(ref CheatToggles.notifRoleAssign, " Role Assigned",     12);
        NotifRow(ref CheatToggles.notifTask,       " Task Completed",    13);
        NotifRow(ref CheatToggles.notifJoin,           " Player Join",       14);
        NotifRow(ref CheatToggles.notifGuardianProtect, " Guardian Protect",  17);
        NotifRow(ref CheatToggles.notifKillAttempt,     " Kill Attempt",      18);
        NotifRow(ref CheatToggles.notifEjections,       " Ejection",          19);
        NotifRow(ref CheatToggles.notifVerdict,         " Judge Verdict",     22);
        NotifRow(ref CheatToggles.notifVerdictLive,     " Judge Gavel",       26);
        NotifRow(ref CheatToggles.notifSabotageFix,     " Sabotage Fix",      20);
        NotifRow(ref CheatToggles.notifGameOver,        " Round Start / Over",21);
        NotifRow(ref CheatToggles.notifZipline,          " Zipline",          23);
        NotifRow(ref CheatToggles.notifPlatform,         " Platform",         24);
        NotifRow(ref CheatToggles.notifLadder,           " Ladder",           25);

        GUILayout.Label("  — Extra Info —", GUIStylePreset.ModernLabel);
        CheatToggles.notifCameras    = GUIStylePreset.CustomToggle(CheatToggles.notifCameras, " Notify Cameras / Vitals");
        CheatToggles.notifRoomEntry  = GUIStylePreset.CustomToggle(CheatToggles.notifRoomEntry, " Notify Player Entered Room");
        CheatToggles.notifShowRoom      = GUIStylePreset.CustomToggle(CheatToggles.notifShowRoom, " Show Room Location");
        CheatToggles.notifShowTaskCount = GUIStylePreset.CustomToggle(CheatToggles.notifShowTaskCount, " Show Task Count");
        CheatToggles.notifShowDistance  = GUIStylePreset.CustomToggle(CheatToggles.notifShowDistance, " Show Distance");

        GUILayout.Space(6);
        GUILayout.Label("Misc", GUIStylePreset.TabSubtitle);
        CheatToggles.noShadows            = GUIStylePreset.CustomToggle(CheatToggles.noShadows, " No Shadows");
        CheatToggles.taskArrows           = GUIStylePreset.CustomToggle(CheatToggles.taskArrows, " Task Arrows");
        CheatToggles.seeLobbyInfo         = GUIStylePreset.CustomToggle(CheatToggles.seeLobbyInfo, " See Lobby Info");
        LobbyTimer.Enabled                = GUIStylePreset.CustomToggle(LobbyTimer.Enabled, " Always Show Lobby Timer");
        Visuals.ShowProtections.Enabled   = GUIStylePreset.CustomToggle(Visuals.ShowProtections.Enabled, " Show Guardian Angel Protections");
        Visuals.AccurateDisconnectReasons.Enabled = GUIStylePreset.CustomToggle(Visuals.AccurateDisconnectReasons.Enabled, " Accurate Disconnect Reasons");
    }

    private static readonly GUILayoutOption _w160 = GUILayout.Width(160);
    private static readonly GUILayoutOption _w55  = GUILayout.Width(55);
    private static readonly GUILayoutOption _w60  = GUILayout.Width(60);
    private static readonly GUILayoutOption _w72  = GUILayout.Width(72);
    private static readonly GUILayoutOption _w45  = GUILayout.Width(45);
    private static readonly GUILayoutOption _w180 = GUILayout.Width(180);
    private static readonly GUILayoutOption _w35  = GUILayout.Width(35);
    private static readonly GUILayoutOption _w40  = GUILayout.Width(40);
    private static readonly GUILayoutOption _w85  = GUILayout.Width(85);

    private static void CtxRow(string label, ref bool toggle, ref byte ctx, bool withSimple = false)
    {
        GUILayout.BeginHorizontal();
        toggle = GUIStylePreset.CustomToggle(toggle, label, _w160);
        bool g = (ctx & ESPContexts.InGame)    != 0;
        bool l = (ctx & ESPContexts.InLobby)   != 0;
        bool m = (ctx & ESPContexts.InMeeting) != 0;
        bool c = (ctx & ESPContexts.InChat)    != 0;
        bool d = (ctx & ESPContexts.InGuide)   != 0;
        bool ng = GUIStylePreset.CustomToggle(g, "Game",    _w55);
        bool nl = GUIStylePreset.CustomToggle(l, "Lobby",   _w60);
        bool nm = GUIStylePreset.CustomToggle(m, "Meeting", _w72);
        bool nc = GUIStylePreset.CustomToggle(c, "Chat",    _w55);
        bool nd = GUIStylePreset.CustomToggle(d, "Guide",   _w45);
        if (ng != g) ctx = (byte)(ng ? ctx | ESPContexts.InGame    : ctx & ~ESPContexts.InGame);
        if (nl != l) ctx = (byte)(nl ? ctx | ESPContexts.InLobby   : ctx & ~ESPContexts.InLobby);
        if (nm != m) ctx = (byte)(nm ? ctx | ESPContexts.InMeeting : ctx & ~ESPContexts.InMeeting);
        if (nc != c) ctx = (byte)(nc ? ctx | ESPContexts.InChat    : ctx & ~ESPContexts.InChat);
        if (nd != d) ctx = (byte)(nd ? ctx | ESPContexts.InGuide   : ctx & ~ESPContexts.InGuide);
        if (withSimple)
            CheatToggles.espShowRoleSimple = GUIStylePreset.CustomToggle(CheatToggles.espShowRoleSimple, "Simple Role", _w85);
        GUILayout.EndHorizontal();
    }

    private static void NotifRow(ref bool toggle, string label, int idx)
    {
        GUILayout.BeginHorizontal();
        toggle = GUIStylePreset.CustomToggle(toggle, label, _w180);
        CheatToggles.notifExSelf[idx] = GUIStylePreset.CustomToggle(CheatToggles.notifExSelf[idx], "", _w35);
        CheatToggles.notifExHost[idx] = GUIStylePreset.CustomToggle(CheatToggles.notifExHost[idx], "", _w35);
        GUILayout.EndHorizontal();
    }

    private void DrawCamera()
    {
        GUILayout.Label("Camera", GUIStylePreset.TabSubtitle);
        CheatToggles.zoomOut = GUIStylePreset.CustomToggle(CheatToggles.zoomOut, " Zoom Out");
        if (CheatToggles.zoomOut)
        {
            GUILayout.Label($"   Scroll Speed: {MalumESP.ZoomScrollSpeed:F1}");
            MalumESP.ZoomScrollSpeed = GUILayout.HorizontalSlider(MalumESP.ZoomScrollSpeed, 0.5f, 5f);
            GUILayout.Label($"   Smoothness: {MalumESP.ZoomSmoothness:F1}");
            MalumESP.ZoomSmoothness = GUILayout.HorizontalSlider(MalumESP.ZoomSmoothness, 0f, 20f);
            GUILayout.Label($"   Min Distance: {MalumESP.ZoomMinDistance:F0}");
            MalumESP.ZoomMinDistance = GUILayout.HorizontalSlider(MalumESP.ZoomMinDistance, 1f, 20f);
            GUILayout.Label($"   Max Distance: {MalumESP.ZoomMaxDistance:F0}");
            MalumESP.ZoomMaxDistance = GUILayout.HorizontalSlider(MalumESP.ZoomMaxDistance, 5f, 50f);
        }
        CheatToggles.freecam = GUIStylePreset.CustomToggle(CheatToggles.freecam, " Freecam");
        if (CheatToggles.freecam)
        {
            GUILayout.Label($"   Speed: {MalumESP.FreecamSpeed:F1}");
            MalumESP.FreecamSpeed = GUILayout.HorizontalSlider(MalumESP.FreecamSpeed, 1f, 50f);
            GUILayout.Label($"   Smoothness: {MalumESP.FreecamSmoothness:F1}");
            MalumESP.FreecamSmoothness = GUILayout.HorizontalSlider(MalumESP.FreecamSmoothness, 0f, 20f);
        }
        CheatToggles.spectate = GUIStylePreset.CustomToggle(CheatToggles.spectate, " Spectate");
    }

    private void DrawResizeHUD()
    {
        GUILayout.Label("Resize HUD", GUIStylePreset.TabSubtitle);
        CheatToggles.resizeHUD = GUIStylePreset.CustomToggle(CheatToggles.resizeHUD, " Resize Buttons/HUD");
        GUILayout.Label($"   HUD Size: {CheatToggles.hudScale:F0}%");
        CheatToggles.hudScale = GUILayout.HorizontalSlider(CheatToggles.hudScale, 25f, 200f);
        GUILayout.Label($"   Height: {CheatToggles.hudSizeV:F0}%");
        CheatToggles.hudSizeV = GUILayout.HorizontalSlider(CheatToggles.hudSizeV, 25f, 200f);
        GUILayout.Label($"   Offset X: {CheatToggles.hudOffsetX:F0}   Y: {CheatToggles.hudOffsetY:F0}");
        CheatToggles.hudOffsetX = GUILayout.HorizontalSlider(CheatToggles.hudOffsetX, -200f, 200f);
        CheatToggles.hudOffsetY = GUILayout.HorizontalSlider(CheatToggles.hudOffsetY, -200f, 200f);
        GUILayout.Label($"   Opacity: {CheatToggles.hudOpacity:F0}%");
        CheatToggles.hudOpacity = GUILayout.HorizontalSlider(CheatToggles.hudOpacity, 0f, 100f);
        CheatToggles.hideHUD = GUIStylePreset.CustomToggle(CheatToggles.hideHUD, " Hide HUD");
    }

    private void DrawTracers()
    {
        GUILayout.Label("Tracers", GUIStylePreset.TabSubtitle);
        CheatToggles.tracersCrew   = GUIStylePreset.CustomToggle(CheatToggles.tracersCrew,   " Crewmates");
        CheatToggles.tracersImps   = GUIStylePreset.CustomToggle(CheatToggles.tracersImps,   " Impostors");
        CheatToggles.tracersGhosts = GUIStylePreset.CustomToggle(CheatToggles.tracersGhosts, " Ghosts");
        CheatToggles.tracersBodies = GUIStylePreset.CustomToggle(CheatToggles.tracersBodies, " Dead Bodies");

        GUILayout.Space(4);
        GUILayout.Label("Color Mode", GUIStylePreset.TabSubtitle);

        if ((CheatToggles.colorBasedTracers ? 1 : 0) + (CheatToggles.distanceBasedTracers ? 1 : 0) + (CheatToggles.simpleRoleBasedTracers ? 1 : 0) > 1)
        {
            CheatToggles.colorBasedTracers = false;
            CheatToggles.distanceBasedTracers = false;
            CheatToggles.simpleRoleBasedTracers = false;
        }

        int mode = CheatToggles.colorBasedTracers ? 1 : CheatToggles.distanceBasedTracers ? 2 : CheatToggles.simpleRoleBasedTracers ? 3 : 0;
        bool r0 = GUIStylePreset.CustomToggle(mode == 0, " Role Color");
        bool r1 = GUIStylePreset.CustomToggle(mode == 1, " Player Color");
        bool r2 = GUIStylePreset.CustomToggle(mode == 2, " Distance-based");
        bool r3 = GUIStylePreset.CustomToggle(mode == 3, " Simple Role");
        int newMode = mode;
        if      (r0 && mode != 0) newMode = 0;
        else if (r1 && mode != 1) newMode = 1;
        else if (r2 && mode != 2) newMode = 2;
        else if (r3 && mode != 3) newMode = 3;
        if (newMode != mode)
        {
            CheatToggles.colorBasedTracers    = newMode == 1;
            CheatToggles.distanceBasedTracers = newMode == 2;
            CheatToggles.simpleRoleBasedTracers = newMode == 3;
        }
        if (CheatToggles.simpleRoleBasedTracers)
            GUILayout.Label("  light blue = crew, light red = imp, light yellow = body, white = ghost", GUIStylePreset.ModernLabel);
    }
    private void DrawMinimap()
    {
        GUILayout.Label("Minimap", GUIStylePreset.TabSubtitle);
        CheatToggles.mapCrew   = GUIStylePreset.CustomToggle(CheatToggles.mapCrew,   " Crewmates");
        CheatToggles.mapImps   = GUIStylePreset.CustomToggle(CheatToggles.mapImps,   " Impostors");
        CheatToggles.mapGhosts = GUIStylePreset.CustomToggle(CheatToggles.mapGhosts, " Ghosts");

        GUILayout.Space(4);
        GUILayout.Label("Color Mode", GUIStylePreset.TabSubtitle);

        if ((CheatToggles.colorBasedMap ? 1 : 0) + (CheatToggles.distanceBasedMap ? 1 : 0) + (CheatToggles.simpleRoleBasedMap ? 1 : 0) > 1)
        {
            CheatToggles.colorBasedMap = false;
            CheatToggles.distanceBasedMap = false;
            CheatToggles.simpleRoleBasedMap = false;
        }

        int mode = CheatToggles.colorBasedMap ? 1 : CheatToggles.distanceBasedMap ? 2 : CheatToggles.simpleRoleBasedMap ? 3 : 0;
        bool r0 = GUIStylePreset.CustomToggle(mode == 0, " Role Color");
        bool r1 = GUIStylePreset.CustomToggle(mode == 1, " Player Color");
        bool r2 = GUIStylePreset.CustomToggle(mode == 2, " Distance-based");
        bool r3 = GUIStylePreset.CustomToggle(mode == 3, " Simple Role");
        int newMode = mode;
        if      (r0 && mode != 0) newMode = 0;
        else if (r1 && mode != 1) newMode = 1;
        else if (r2 && mode != 2) newMode = 2;
        else if (r3 && mode != 3) newMode = 3;
        if (newMode != mode)
        {
            CheatToggles.colorBasedMap    = newMode == 1;
            CheatToggles.distanceBasedMap = newMode == 2;
            CheatToggles.simpleRoleBasedMap = newMode == 3;
        }
        if (CheatToggles.simpleRoleBasedMap)
            GUILayout.Label("  light blue = crew, light red = imp, white = ghost", GUIStylePreset.ModernLabel);
    }

    private void DrawRadar()
    {
        GUILayout.Label("Radar", GUIStylePreset.TabSubtitle);
        CheatToggles.radarShow   = GUIStylePreset.CustomToggle(CheatToggles.radarShow,   " Show Radar");
        CheatToggles.radarIcons  = GUIStylePreset.CustomToggle(CheatToggles.radarIcons,  " Draw Icons");
        CheatToggles.radarBodies = GUIStylePreset.CustomToggle(CheatToggles.radarBodies, " Show Dead Bodies");
        CheatToggles.radarGhosts = GUIStylePreset.CustomToggle(CheatToggles.radarGhosts, " Show Ghosts");
        string radarBindName = ((int)CheatToggles.radarTeleportBind) switch { 0 => "Left click", 1 => "Right click", 2 => "Shift + Left", 3 => "Shift + Right", 4 => "Ctrl + Left", _ => "Ctrl + Right" };
        CheatToggles.radarClickTp = GUIStylePreset.CustomToggle(CheatToggles.radarClickTp, $" Teleport on Radar ({radarBindName})");
        if (CheatToggles.radarClickTp)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind", GUILayout.Width(60));
            int radarSel = (int)CheatToggles.radarTeleportBind;
            int radarNext = radarSel;
            string[] radarBinds = { "Left click", "Right click", "Shift + Left", "Shift + Right", "Ctrl + Left", "Ctrl + Right" };
            for (int b = 0; b < radarBinds.Length; b++)
            {
                bool on = GUIStylePreset.CustomToggle(radarSel == b, " " + radarBinds[b]);
                if (on && radarSel != b) radarNext = b;
            }
            GUILayout.EndHorizontal();
            CheatToggles.radarTeleportBind = (CheatToggles.TeleportBind)radarNext;
        }
        string radarDoorsBindName = ((int)CheatToggles.radarDoorsBind) switch { 0 => "Left click", 1 => "Right click", 2 => "Shift + Left", 3 => "Shift + Right", 4 => "Ctrl + Left", _ => "Ctrl + Right" };
        CheatToggles.radarClickDoors = GUIStylePreset.CustomToggle(CheatToggles.radarClickDoors, $" Close Doors on Radar ({radarDoorsBindName})");
        if (CheatToggles.radarClickDoors)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bind", GUILayout.Width(60));
            int doorsSel = (int)CheatToggles.radarDoorsBind;
            int doorsNext = doorsSel;
            string[] doorsBinds = { "Left click", "Right click", "Shift + Left", "Shift + Right", "Ctrl + Left", "Ctrl + Right" };
            for (int b = 0; b < doorsBinds.Length; b++)
            {
                bool on = GUIStylePreset.CustomToggle(doorsSel == b, " " + doorsBinds[b]);
                if (on && doorsSel != b) doorsNext = b;
            }
            GUILayout.EndHorizontal();
            CheatToggles.radarDoorsBind = (CheatToggles.TeleportBind)doorsNext;
        }
        CheatToggles.radarHideMeeting = GUIStylePreset.CustomToggle(CheatToggles.radarHideMeeting, " Hide In Meeting");
        CheatToggles.radarLock   = GUIStylePreset.CustomToggle(CheatToggles.radarLock,   " Lock Position");
        CheatToggles.radarBorder = GUIStylePreset.CustomToggle(CheatToggles.radarBorder, " Border");
        CheatToggles.radarShowNoMenu = GUIStylePreset.CustomToggle(CheatToggles.radarShowNoMenu, " Show Even If Menu Hidden");

        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale", GUILayout.Width(80));
        CheatToggles.radarScale = GUILayout.HorizontalSlider(CheatToggles.radarScale, 0.65f, 1.6f);
        GUILayout.Label($"{CheatToggles.radarScale:0.00}", GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Alpha", GUILayout.Width(80));
        CheatToggles.radarAlpha = GUILayout.HorizontalSlider(CheatToggles.radarAlpha, 0.2f, 1f);
        GUILayout.Label($"{CheatToggles.radarAlpha:0.00}", GUILayout.Width(50));
        GUILayout.EndHorizontal();
    }
}
