using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using SkidMenu.features;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SkidMenu;

public class HostOnlyTab : ITab
{
    public string name => "Host";

    private string _protScaleHInput = "100", _protScaleVInput = "100";
    private Dictionary<string, bool> _focusedFields = new();
    private Dictionary<string, float> _lastBlinkTime = new();
    private Dictionary<string, bool> _cursorVisible = new();
    private Dictionary<string, Rect> _fieldRects = new();
    private Dictionary<string, int> _cursorPositions = new();
    private float _cursorBlinkTime = 0.5f;

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 50, int height = 20)
    {
        GUILayout.Box("", GUIStylePreset.NormalTextField, GUILayout.Width(width), GUILayout.Height(height));
        if (Event.current.type == EventType.Repaint) _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();
        if (!_focusedFields.ContainsKey(fieldKey)) _focusedFields[fieldKey] = false;
        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey)) { if (_fieldRects[fieldKey].Contains(Event.current.mousePosition)) { _focusedFields[fieldKey] = true; _lastBlinkTime[fieldKey] = Time.time; _cursorVisible[fieldKey] = true; Event.current.Use(); } else _focusedFields[fieldKey] = false; }
        if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey] && Event.current.type == EventType.KeyDown) { if (!_cursorPositions.ContainsKey(fieldKey)) _cursorPositions[fieldKey] = content.Length; int cp = System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length); if (Event.current.keyCode == KeyCode.Backspace && cp > 0) { content = content.Substring(0, cp - 1) + content.Substring(cp); cp--; Event.current.Use(); } else if (char.IsDigit(Event.current.character) && content.Length < 3) { content = content.Substring(0, cp) + Event.current.character + content.Substring(cp); cp++; Event.current.Use(); } _cursorPositions[fieldKey] = System.Math.Clamp(cp, 0, content.Length); }
        if (_fieldRects.ContainsKey(fieldKey)) { GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content); if (_focusedFields.ContainsKey(fieldKey) && _focusedFields[fieldKey]) { if (!_lastBlinkTime.ContainsKey(fieldKey)) _lastBlinkTime[fieldKey] = Time.time; if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime) { _cursorVisible[fieldKey] = !_cursorVisible[fieldKey]; _lastBlinkTime[fieldKey] = Time.time; } if (_cursorVisible.ContainsKey(fieldKey) && _cursorVisible[fieldKey]) { int cp2 = _cursorPositions.ContainsKey(fieldKey) ? System.Math.Clamp(_cursorPositions[fieldKey], 0, content.Length) : content.Length; Vector2 ts = GUI.skin.label.CalcSize(new GUIContent(content.Substring(0, cp2))); GUI.Label(new Rect(_fieldRects[fieldKey].x + ts.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|"); } } }
    }
    private byte _selectedMap = 0;
    private Vector2 _scroll = Vector2.zero;


    public void Draw()
    {
        _scroll = GUILayout.BeginScrollView(_scroll);

        bool actualHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
        string status = PlayerControl.LocalPlayer == null ? "Not in a game"
                      : actualHost                        ? "Host"
                      : CheatToggles.bypassHostOnly       ? "Not host — bypass active"
                      :                                     "Not host";
        GUILayout.Label($"[ {status} ]");
        GUILayout.Space(4);

        DrawGeneral();
        GUILayout.Space(6);
        DrawProtections();
        GUILayout.Space(6);
        DrawMurder();
        GUILayout.Space(6);
        DrawGameState();
        GUILayout.Space(6);
        DrawRoleAssigner();
        GUILayout.Space(6);
        DrawMeetings();
        GUILayout.Space(6);
        DrawPlayers();
        GUILayout.Space(6);
        DrawEndGame();
        GUILayout.Space(6);
        DrawRoleManager();
        GUILayout.Space(6);
        DrawMapSpawner();
        GUILayout.Space(6);
        DrawDiscoParty();

        GUILayout.EndScrollView();
    }

    private void DrawGeneral()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("General");
        CheatToggles.bypassHostOnly  = GUIStylePreset.CustomToggle(CheatToggles.bypassHostOnly, " Bypass Host-Only Checks");
        CheatToggles.killVanished    = GUIStylePreset.CustomToggle(CheatToggles.killVanished, " Kill While Vanished");
CheatToggles.killAnyone = GUIStylePreset.CustomToggle(CheatToggles.killAnyone, " Kill Anyone");
CheatToggles.killGhosts = GUIStylePreset.CustomToggle(CheatToggles.killGhosts, " Kill Ghosts");
        CheatToggles.noKillCd        = GUIStylePreset.CustomToggle(CheatToggles.noKillCd, " No Kill Cooldown");
        CheatToggles.noTaskMode      = GUIStylePreset.CustomToggle(CheatToggles.noTaskMode, " No Task Mode");
        CheatToggles.noSettingLimit  = GUIStylePreset.CustomToggle(CheatToggles.noSettingLimit, " No Setting Limit");
        Host.BanMidGame.Enabled      = GUIStylePreset.CustomToggle(Host.BanMidGame.Enabled, " Ban Players Mid-Game");
        Host.FlippedSkeld            = GUIStylePreset.CustomToggle(Host.FlippedSkeld, " Use Flipped Skeld Map");
        bool newColor = GUIStylePreset.CustomToggle(ChatEnhancements.EnableColorCommand, " Enable /c Command");
        if (newColor != ChatEnhancements.EnableColorCommand) ChatEnhancements.EnableColorCommand = newColor;
        GUILayout.EndVertical();
    }

    private void DrawProtections()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Protection");
        CheatToggles.bypassShield = GUIStylePreset.CustomToggle(CheatToggles.bypassShield, " Bypass Angel Shield");
        CheatToggles.godMode      = GUIStylePreset.CustomToggle(CheatToggles.godMode, " God Mode");
        CheatToggles.godModeAll   = GUIStylePreset.CustomToggle(CheatToggles.godModeAll, " God Mode: Everyone");
        CheatToggles.autoAngel    = GUIStylePreset.CustomToggle(CheatToggles.autoAngel, " Auto Angel");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Auto Angel Delay: {CheatToggles.autoAngelInterval:F2}s", GUILayout.Width(150));
        CheatToggles.autoAngelInterval = GUILayout.HorizontalSlider(CheatToggles.autoAngelInterval, 0.1f, 2.0f);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Protect Everyone", GUIStylePreset.NormalButton))
            HostProtection.ProtectEveryone(true);
        GUILayout.Space(4);
        CheatToggles.showProtectMenu = GUIStylePreset.CustomToggle(CheatToggles.showProtectMenu, " Show Protect Menu");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Horizontal:", GUILayout.Width(150));
        HandleCustomTextField(ref _protScaleHInput, "protScaleH");
        GUILayout.Label("%  Vertical:", GUILayout.Width(70));
        HandleCustomTextField(ref _protScaleVInput, "protScaleV");
        GUILayout.Label("%", GUILayout.Width(20));
        if (GUILayout.Button("Apply", GUILayout.Width(60))) { if (float.TryParse(_protScaleHInput, out var h)) CheatToggles.protectScaleH = System.Math.Clamp(h, 50f, 300f); if (float.TryParse(_protScaleVInput, out var v)) CheatToggles.protectScaleV = System.Math.Clamp(v, 50f, 300f); }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawMurder()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Murder");
        CheatToggles.killPlayer     = GUIStylePreset.CustomToggle(CheatToggles.killPlayer, " Kill Player");
        CheatToggles.telekillPlayer = GUIStylePreset.CustomToggle(CheatToggles.telekillPlayer, " Telekill Player");
        if (GUILayout.Button("Kill All Crewmates", GUIStylePreset.NormalButton)) CheatToggles.killAllCrew = true;
        if (GUILayout.Button("Kill All Impostors", GUIStylePreset.NormalButton)) CheatToggles.killAllImps = true;
        if (GUILayout.Button("Kill Everyone",      GUIStylePreset.NormalButton)) CheatToggles.killAll     = true;
        GUILayout.EndVertical();
    }

    private void DrawGameState()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Game State");
        if (GUILayout.Button("Force Start Game", GUIStylePreset.NormalButton)) CheatToggles.forceStartGame = true;
        CheatToggles.noGameEnd             = GUIStylePreset.CustomToggle(CheatToggles.noGameEnd, " No Game End");
        Host.DisableMeetings.Enabled       = GUIStylePreset.CustomToggle(Host.DisableMeetings.Enabled, " Disable Meetings");
        Host.DisableSabotages.Enabled      = GUIStylePreset.CustomToggle(Host.DisableSabotages.Enabled, " Disable Sabotages");
        Host.DisableCloseDoors.Enabled     = GUIStylePreset.CustomToggle(Host.DisableCloseDoors.Enabled, " Disable Close Doors");
        Host.DisableCameras.Enabled        = GUIStylePreset.CustomToggle(Host.DisableCameras.Enabled, " Disable Security Cameras");
        GUILayout.EndVertical();
    }

    private void DrawMeetings()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Meetings");
        CheatToggles.skipMeeting = GUIStylePreset.CustomToggle(CheatToggles.skipMeeting, " Skip Meeting");
                CheatToggles.voteImmune  = GUIStylePreset.CustomToggle(CheatToggles.voteImmune, " Vote Immune");
        CheatToggles.judgeImmune = GUIStylePreset.CustomToggle(CheatToggles.judgeImmune, " Judge Immune");
        CheatToggles.ejectPlayer = GUIStylePreset.CustomToggle(CheatToggles.ejectPlayer, " Eject Player");
        GUILayout.EndVertical();
    }

    private void DrawPlayers()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Players");
        SkidMenu.routines.reportBodySpam.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.reportBodySpam.Enabled, " Spam Report Bodies");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        GUILayout.Label("Delay", GUILayout.Width(80));
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        SkidMenu.routines.reportBodySpam.reportDelay = GUILayout.HorizontalSlider(SkidMenu.routines.reportBodySpam.reportDelay, 0.001f, 1.0f);
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        GUILayout.Label($"{SkidMenu.routines.reportBodySpam.reportDelay:0.000}s", GUILayout.Width(60));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        Host.BlockLowLevels.Enabled = GUIStylePreset.CustomToggle(Host.BlockLowLevels.Enabled, $" Kick below level {Host.BlockLowLevels.MinLevel}");
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Space(8);
        Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawEndGame()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("End Game");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Crewmate Win"))
        { Host.DisableGameEnd.Enabled = false; GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false); }
        if (GUILayout.Button("Impostor Win"))
        { Host.DisableGameEnd.Enabled = false; GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false); }
        if (GUILayout.Button("Imp Disconnect"))
        { HostFeatures.ForceEndImpDisconnect(); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Kick All", GUILayout.ExpandWidth(true))) HostFeatures.KickAll();
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        GUILayout.Label("Shapeshift Controls:");
        if (GUILayout.Button("Shapeshift Everyone Into Me"))
            AmongUsClient.Instance?.StartCoroutine(ShapeshiftAllCoroutine(PlayerControl.LocalPlayer).WrapToIl2Cpp());
        if (GUILayout.Button("Shapeshift Everyone Into Random"))
        {
            var all = PlayerControl.AllPlayerControls.ToArray();
            if (all.Length > 0)
            {
                var target = all[new System.Random().Next(all.Length)];
                AmongUsClient.Instance?.StartCoroutine(ShapeshiftAllCoroutine(target).WrapToIl2Cpp());
            }
        }
        if (GUILayout.Button("Revert All Shapeshifts"))
            AmongUsClient.Instance?.StartCoroutine(RevertAllShapeshiftsCoroutine().WrapToIl2Cpp());

        GUILayout.EndVertical();
    }

    private void DrawRoleManager()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Role Manager");

        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Pre-Game");
        HostFeatures.preGameRoleForce = GUIStylePreset.CustomToggle(HostFeatures.preGameRoleForce, " Role Forcing");
        if (HostFeatures.preGameRoleForce)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(28))) HostFeatures.preGameImpCount = Mathf.Max(1, HostFeatures.preGameImpCount - 1);
            GUILayout.Label($"Random imps: {HostFeatures.preGameImpCount}", GUILayout.Width(120f));
            if (GUILayout.Button(">", GUILayout.Width(28))) HostFeatures.preGameImpCount++;
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(4);
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Live Role Distributor (Host)");

        var roles = HostFeatures.ValidRoles;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(30f)))
            HostFeatures.selectedRoleIndex = (HostFeatures.selectedRoleIndex - 1 + roles.Length) % roles.Length;
        GUILayout.Label(roles[HostFeatures.selectedRoleIndex].ToString(), GUILayout.ExpandWidth(true));
        if (GUILayout.Button(">", GUILayout.Width(30f)))
            HostFeatures.selectedRoleIndex = (HostFeatures.selectedRoleIndex + 1) % roles.Length;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Set All Players Role"))
            HostFeatures.SetAllPlayersRole(roles[HostFeatures.selectedRoleIndex]);

        if (PlayerControl.AllPlayerControls != null)
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.Data == null || pc.Data.Disconnected) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(pc.Data.PlayerName, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Set", GUILayout.Width(40f)))
                    pc.RpcSetRole(roles[HostFeatures.selectedRoleIndex], true);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

    private void DrawMapSpawner()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Map Spawner");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Despawn Lobby"))
        { if (LobbyBehaviour.Instance != null) LobbyBehaviour.Instance.Despawn(); }
        if (GUILayout.Button("Spawn Lobby"))
        {
            LobbyBehaviour.Instance = Object.Instantiate<LobbyBehaviour>(GameStartManager.Instance.LobbyPrefab);
            AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"Map: {(MapNames)_selectedMap}");
        _selectedMap = (byte)GUILayout.HorizontalSlider(_selectedMap, 0, 5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Despawn Map"))
        { if (ShipStatus.Instance != null) ShipStatus.Instance.Despawn(); }
        if (GUILayout.Button("Spawn Map"))
            AmongUsClient.Instance.StartCoroutine(SpawnMap(_selectedMap).WrapToIl2Cpp());
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawDiscoParty()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.Label("Disco Party");
        SkidMenu.routines.discoHost.Enabled = GUIStylePreset.CustomToggle(SkidMenu.routines.discoHost.Enabled, " Enabled");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Delay: {SkidMenu.routines.discoHost.randomizationDelay:F2}s", GUILayout.Width(80f));
        SkidMenu.routines.discoHost.randomizationDelay = GUILayout.HorizontalSlider(SkidMenu.routines.discoHost.randomizationDelay, 0.1f, 2.0f);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private static void ShapeshiftPlayer(PlayerControl victim, PlayerControl target)
    {
        var batch = new Network.BatchedMessage();
        if (victim.Data.RoleType != RoleTypes.Shapeshifter)
        {
            var prev = victim.Data.RoleType;
            batch.QueueSetRole(victim, RoleTypes.Shapeshifter, true);
            batch.QueueShapeshift(victim, target, true);
            batch.QueueSetRole(victim, prev, true);
        }
        else batch.QueueShapeshift(victim, target, true);
        batch.FinishBatch();
    }

    public static IEnumerator ShapeshiftAllCoroutine(PlayerControl target)
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.AmOwner) continue;
            ShapeshiftPlayer(pc, target);
            yield return new WaitForSeconds(0.15f);
        }
    }

    public static IEnumerator RevertAllShapeshiftsCoroutine()
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.AmOwner) continue;
            ShapeshiftPlayer(pc, pc);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private static readonly string[] _roleNames = {        "Random", "Crewmate", "Impostor", "Scientist", "Engineer",
        "Guardian Angel", "Shapeshifter", "Noisemaker", "Phantom",
        "Tracker", "Detective", "Viper", "Judge"
    };

    private static readonly RoleTypes[] _roleTypes = {
        (RoleTypes)255,          // sentinel for "Random"
        RoleTypes.Crewmate,
        RoleTypes.Impostor,
        RoleTypes.Scientist,
        RoleTypes.Engineer,
        RoleTypes.GuardianAngel,
        RoleTypes.Shapeshifter,
        RoleTypes.Noisemaker,
        RoleTypes.Phantom,
        RoleTypes.Tracker,
        RoleTypes.Detective,
        RoleTypes.Viper,
        RoleTypes.Judge,
    };

    private void DrawRoleAssigner()
    {
        GUILayout.BeginVertical(GUIStylePreset.SectionBox);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Role Assigner (next round)", GUIStylePreset.SectionHeader);
        RoleAssigner.Enabled = GUIStylePreset.CustomToggle(RoleAssigner.Enabled, " Enabled");
        GUILayout.EndHorizontal();

        if (!RoleAssigner.Enabled)
        {
            GUILayout.EndVertical();
            return;
        }

        if (GUILayout.Button("Clear All"))
            RoleAssigner.ClearAll();

        if (GameData.Instance == null || PlayerControl.AllPlayerControls == null)
        {
            GUILayout.Label("Not in a lobby.");
            GUILayout.EndVertical();
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null) continue;
            byte pid = pc.PlayerId;
            string pname = pc.Data.PlayerName;

            GUILayout.BeginHorizontal();
            GUILayout.Label(pname, GUILayout.Width(140));

            bool hasRole = RoleAssigner.TryGetRole(pid, out RoleTypes current);
            int curIdx = 0;
            if (hasRole)
                for (int i = 1; i < _roleTypes.Length; i++)
                    if (_roleTypes[i] == current) { curIdx = i; break; }

            if (GUILayout.Button("◀", GUILayout.Width(26)))
                curIdx = (curIdx - 1 + _roleNames.Length) % _roleNames.Length;
            GUILayout.Label(_roleNames[curIdx], GUILayout.Width(110));
            if (GUILayout.Button("▶", GUILayout.Width(26)))
                curIdx = (curIdx + 1) % _roleNames.Length;

            if (curIdx == 0)
                RoleAssigner.ClearRole(pid);
            else
                RoleAssigner.SetRole(pid, _roleTypes[curIdx]);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private static IEnumerator SpawnMap(byte mapId)
    {
        AsyncOperationHandle<GameObject> handle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
        yield return handle;
        ShipStatus ship = handle.Result.GetComponent<ShipStatus>();
        AmongUsClient.Instance.Spawn(ship, -2, SpawnFlags.None);
    }
}




