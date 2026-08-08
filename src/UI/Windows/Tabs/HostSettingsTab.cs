using AmongUs.GameOptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SkidMenu;

public class HostSettingsTab : ITab
{
    public string name => "Host Settings";

    private Vector2 _scroll;

    private static readonly string FolderPath = System.IO.Path.Combine(
        BepInEx.Paths.BepInExRootPath, "SkidMenu", "HostSettings");

    private static IGameOptions Opts => GameOptionsManager.Instance?.currentGameOptions;
    private static IRoleOptionsCollection Roles => GameOptionsManager.Instance?.currentGameOptions?.RoleOptions;
    private static bool IsHost => AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;

    private static void Sync()
    {
        if (!IsHost || Opts == null) return;
        try { GameManager.Instance?.LogicOptions?.SyncOptions(); } catch { }
    }

    private static void EnsureFolder()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
    }

    private static volatile string _pendingSavePath = null;
    private static volatile string _pendingLoadPath = null;
    private static volatile bool   _dialogRunning   = false;

    private static string ShowSaveDialog()
    {
        try
        {
            EnsureFolder();
            string ps = $@"Add-Type -AssemblyName System.Windows.Forms; $d = New-Object System.Windows.Forms.SaveFileDialog; $d.Filter = 'Text files (*.txt)|*.txt'; $d.InitialDirectory = '{FolderPath}'; $d.FileName = 'hostsettings'; if ($d.ShowDialog() -eq 'OK') {{ Write-Output $d.FileName }} else {{ Write-Output '' }}";
            var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"{ps}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            p.WaitForExit();
            string path = p.StandardOutput.ReadToEnd().Trim();
            return string.IsNullOrEmpty(path) ? null : path;
        }
        catch { return null; }
    }

    private static string ShowOpenDialog()
    {
        try
        {
            EnsureFolder();
            string ps = $@"Add-Type -AssemblyName System.Windows.Forms; $d = New-Object System.Windows.Forms.OpenFileDialog; $d.Filter = 'Text files (*.txt)|*.txt'; $d.InitialDirectory = '{FolderPath}'; if ($d.ShowDialog() -eq 'OK') {{ Write-Output $d.FileName }} else {{ Write-Output '' }}";
            var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"{ps}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            p.WaitForExit();
            string path = p.StandardOutput.ReadToEnd().Trim();
            return string.IsNullOrEmpty(path) ? null : path;
        }
        catch { return null; }
    }

    private static void SaveAsync()
    {
        if (_dialogRunning) return;
        _dialogRunning = true;
        new System.Threading.Thread(() =>
        {
            string path = ShowSaveDialog();
            _pendingSavePath = path ?? "";
            _dialogRunning = false;
        }) { IsBackground = true }.Start();
    }

    private static void LoadAsync()
    {
        if (_dialogRunning) return;
        _dialogRunning = true;
        new System.Threading.Thread(() =>
        {
            string path = ShowOpenDialog();
            _pendingLoadPath = path ?? "";
            _dialogRunning = false;
        }) { IsBackground = true }.Start();
    }

    private static void SaveToFile(string path)
    {
        if (Opts == null) return;
        try
        {
            var lines = new List<string>
            {
                $"MapId={(int)Opts.GetByte(ByteOptionNames.MapId)}",
                $"NumImpostors={Opts.GetInt(Int32OptionNames.NumImpostors)}",
                $"PlayerSpeedMod={Opts.GetFloat(FloatOptionNames.PlayerSpeedMod)}",
                $"CrewLightMod={Opts.GetFloat(FloatOptionNames.CrewLightMod)}",
                $"ImpostorLightMod={Opts.GetFloat(FloatOptionNames.ImpostorLightMod)}",
                $"KillCooldown={Opts.GetFloat(FloatOptionNames.KillCooldown)}",
                $"KillDistance={Opts.GetInt(Int32OptionNames.KillDistance)}",
                $"ConfirmImpostor={Opts.GetBool(BoolOptionNames.ConfirmImpostor)}",
                $"VisualTasks={Opts.GetBool(BoolOptionNames.VisualTasks)}",
                $"AnonymousVotes={Opts.GetBool(BoolOptionNames.AnonymousVotes)}",
                $"TaskBarMode={Opts.GetInt(Int32OptionNames.TaskBarMode)}",
                $"DiscussionTime={Opts.GetInt(Int32OptionNames.DiscussionTime)}",
                $"VotingTime={Opts.GetInt(Int32OptionNames.VotingTime)}",
                $"NumEmergencyMeetings={Opts.GetInt(Int32OptionNames.NumEmergencyMeetings)}",
                $"EmergencyCooldown={Opts.GetInt(Int32OptionNames.EmergencyCooldown)}",
                $"NumCommonTasks={Opts.GetInt(Int32OptionNames.NumCommonTasks)}",
                $"NumLongTasks={Opts.GetInt(Int32OptionNames.NumLongTasks)}",
                $"NumShortTasks={Opts.GetInt(Int32OptionNames.NumShortTasks)}",
                $"ShapeshifterCooldown={Opts.GetFloat(FloatOptionNames.ShapeshifterCooldown)}",
                $"ShapeshifterDuration={Opts.GetFloat(FloatOptionNames.ShapeshifterDuration)}",
                $"ShapeshifterLeaveSkin={Opts.GetBool(BoolOptionNames.ShapeshifterLeaveSkin)}",
                $"ScientistCooldown={Opts.GetFloat(FloatOptionNames.ScientistCooldown)}",
                $"ScientistBatteryCharge={Opts.GetFloat(FloatOptionNames.ScientistBatteryCharge)}",
                $"EngineerCooldown={Opts.GetFloat(FloatOptionNames.EngineerCooldown)}",
                $"EngineerInVentMaxTime={Opts.GetFloat(FloatOptionNames.EngineerInVentMaxTime)}",
                $"GuardianAngelCooldown={Opts.GetFloat(FloatOptionNames.GuardianAngelCooldown)}",
                $"ProtectionDurationSeconds={Opts.GetFloat(FloatOptionNames.ProtectionDurationSeconds)}",
                $"ImpostorsCanSeeProtect={Opts.GetBool(BoolOptionNames.ImpostorsCanSeeProtect)}",
                $"PhantomCooldown={Opts.GetFloat(FloatOptionNames.PhantomCooldown)}",
                $"PhantomDuration={Opts.GetFloat(FloatOptionNames.PhantomDuration)}",
            };
            lines.Add($"TrackerCooldown={Opts.GetFloat(FloatOptionNames.TrackerCooldown)}");
            lines.Add($"TrackerDuration={Opts.GetFloat(FloatOptionNames.TrackerDuration)}");
            lines.Add($"TrackerDelay={Opts.GetFloat(FloatOptionNames.TrackerDelay)}");
            lines.Add($"NoisemakerAlertDuration={Opts.GetFloat(FloatOptionNames.NoisemakerAlertDuration)}");
            lines.Add($"NoisemakerImpostorAlert={Opts.GetBool(BoolOptionNames.NoisemakerImpostorAlert)}");
            lines.Add($"ViperDissolveTime={Opts.GetFloat(FloatOptionNames.ViperDissolveTime)}");
            lines.Add($"DetectiveSuspectLimit={Opts.GetFloat(FloatOptionNames.DetectiveSuspectLimit)}");
            if (Roles != null)
                foreach (RoleTypes role in new[] { RoleTypes.Scientist, RoleTypes.Engineer, RoleTypes.GuardianAngel,
                    RoleTypes.Shapeshifter, RoleTypes.Noisemaker, RoleTypes.Tracker,
                    RoleTypes.Phantom, RoleTypes.Viper, RoleTypes.Detective })
                {
                    lines.Add($"Role_{role}_Chance={Roles.GetChancePerGame(role)}");
                    lines.Add($"Role_{role}_Count={Roles.GetNumPerGame(role)}");
                }
            File.WriteAllLines(path, lines);
        }
        catch { }
    }

    private static void LoadFromFile(string path)
    {
        if (!IsHost || Opts == null || !File.Exists(path)) return;
        try
        {
            foreach (var line in File.ReadAllLines(path))
            {
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                try
                {
                    if (k == "MapId")                     { Opts.SetByte(ByteOptionNames.MapId, byte.Parse(v)); continue; }
                    if (k == "NumImpostors")              { Opts.SetInt(Int32OptionNames.NumImpostors, int.Parse(v)); continue; }
                    if (k == "PlayerSpeedMod")            { Opts.SetFloat(FloatOptionNames.PlayerSpeedMod, float.Parse(v)); continue; }
                    if (k == "CrewLightMod")              { Opts.SetFloat(FloatOptionNames.CrewLightMod, float.Parse(v)); continue; }
                    if (k == "ImpostorLightMod")          { Opts.SetFloat(FloatOptionNames.ImpostorLightMod, float.Parse(v)); continue; }
                    if (k == "KillCooldown")              { Opts.SetFloat(FloatOptionNames.KillCooldown, float.Parse(v)); continue; }
                    if (k == "KillDistance")              { Opts.SetInt(Int32OptionNames.KillDistance, int.Parse(v)); continue; }
                    if (k == "ConfirmImpostor")           { Opts.SetBool(BoolOptionNames.ConfirmImpostor, bool.Parse(v)); continue; }
                    if (k == "VisualTasks")               { Opts.SetBool(BoolOptionNames.VisualTasks, bool.Parse(v)); continue; }
                    if (k == "AnonymousVotes")            { Opts.SetBool(BoolOptionNames.AnonymousVotes, bool.Parse(v)); continue; }
                    if (k == "TaskBarMode")               { Opts.SetInt(Int32OptionNames.TaskBarMode, int.Parse(v)); continue; }
                    if (k == "DiscussionTime")            { Opts.SetInt(Int32OptionNames.DiscussionTime, int.Parse(v)); continue; }
                    if (k == "VotingTime")                { Opts.SetInt(Int32OptionNames.VotingTime, int.Parse(v)); continue; }
                    if (k == "NumEmergencyMeetings")      { Opts.SetInt(Int32OptionNames.NumEmergencyMeetings, int.Parse(v)); continue; }
                    if (k == "EmergencyCooldown")         { Opts.SetInt(Int32OptionNames.EmergencyCooldown, int.Parse(v)); continue; }
                    if (k == "NumCommonTasks")            { Opts.SetInt(Int32OptionNames.NumCommonTasks, int.Parse(v)); continue; }
                    if (k == "NumLongTasks")              { Opts.SetInt(Int32OptionNames.NumLongTasks, int.Parse(v)); continue; }
                    if (k == "NumShortTasks")             { Opts.SetInt(Int32OptionNames.NumShortTasks, int.Parse(v)); continue; }
                    if (k == "ShapeshifterCooldown")      { Opts.SetFloat(FloatOptionNames.ShapeshifterCooldown, float.Parse(v)); continue; }
                    if (k == "ShapeshifterDuration")      { Opts.SetFloat(FloatOptionNames.ShapeshifterDuration, float.Parse(v)); continue; }
                    if (k == "ShapeshifterLeaveSkin")     { Opts.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, bool.Parse(v)); continue; }
                    if (k == "ScientistCooldown")         { Opts.SetFloat(FloatOptionNames.ScientistCooldown, float.Parse(v)); continue; }
                    if (k == "ScientistBatteryCharge")    { Opts.SetFloat(FloatOptionNames.ScientistBatteryCharge, float.Parse(v)); continue; }
                    if (k == "EngineerCooldown")          { Opts.SetFloat(FloatOptionNames.EngineerCooldown, float.Parse(v)); continue; }
                    if (k == "EngineerInVentMaxTime")     { Opts.SetFloat(FloatOptionNames.EngineerInVentMaxTime, float.Parse(v)); continue; }
                    if (k == "GuardianAngelCooldown")     { Opts.SetFloat(FloatOptionNames.GuardianAngelCooldown, float.Parse(v)); continue; }
                    if (k == "ProtectionDurationSeconds") { Opts.SetFloat(FloatOptionNames.ProtectionDurationSeconds, float.Parse(v)); continue; }
                    if (k == "ImpostorsCanSeeProtect")    { Opts.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, bool.Parse(v)); continue; }
                    if (k == "PhantomCooldown")           { Opts.SetFloat(FloatOptionNames.PhantomCooldown, float.Parse(v)); continue; }
                    if (k == "PhantomDuration")           { Opts.SetFloat(FloatOptionNames.PhantomDuration, float.Parse(v)); continue; }
                    if (k == "TrackerCooldown")           { Opts.SetFloat(FloatOptionNames.TrackerCooldown, float.Parse(v)); continue; }
                    if (k == "TrackerDuration")           { Opts.SetFloat(FloatOptionNames.TrackerDuration, float.Parse(v)); continue; }
                    if (k == "TrackerDelay")              { Opts.SetFloat(FloatOptionNames.TrackerDelay, float.Parse(v)); continue; }
                    if (k == "NoisemakerAlertDuration")   { Opts.SetFloat(FloatOptionNames.NoisemakerAlertDuration, float.Parse(v)); continue; }
                    if (k == "NoisemakerImpostorAlert")   { Opts.SetBool(BoolOptionNames.NoisemakerImpostorAlert, bool.Parse(v)); continue; }
                    if (k == "ViperDissolveTime")         { Opts.SetFloat(FloatOptionNames.ViperDissolveTime, float.Parse(v)); continue; }
                    if (k == "DetectiveSuspectLimit")     { Opts.SetFloat(FloatOptionNames.DetectiveSuspectLimit, float.Parse(v)); continue; }
                    if (k.StartsWith("Role_") && Roles != null)
                    {
                        var parts = k.Split('_');
                        if (parts.Length == 3 && System.Enum.TryParse<RoleTypes>(parts[1], out var role))
                        {
                            if (parts[2] == "Chance") Roles.SetRoleRate(role, Roles.GetNumPerGame(role), int.Parse(v));
                            else if (parts[2] == "Count") Roles.SetRoleRate(role, int.Parse(v), Roles.GetChancePerGame(role));
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    private static readonly Dictionary<FloatOptionNames,   float>             _floats = new();
    private static readonly Dictionary<Int32OptionNames,   int>               _ints   = new();
    private static readonly Dictionary<BoolOptionNames,    bool>              _bools  = new();
    private static readonly Dictionary<RoleTypes, (int chance, int count)>    _roles  = new();
    private static byte _mapId = 0;
    private static bool _shadowInit = false;

    private static void InitShadow()
    {
        if (Opts == null) return;
        _floats.Clear(); _ints.Clear(); _bools.Clear(); _roles.Clear();
        try { _mapId = Opts.GetByte(ByteOptionNames.MapId); } catch { }
        foreach (var n in new[] { FloatOptionNames.PlayerSpeedMod, FloatOptionNames.CrewLightMod, FloatOptionNames.ImpostorLightMod,
            FloatOptionNames.KillCooldown, FloatOptionNames.ShapeshifterCooldown, FloatOptionNames.ShapeshifterDuration,
            FloatOptionNames.ScientistCooldown, FloatOptionNames.ScientistBatteryCharge, FloatOptionNames.EngineerCooldown,
            FloatOptionNames.EngineerInVentMaxTime, FloatOptionNames.GuardianAngelCooldown, FloatOptionNames.ProtectionDurationSeconds,
            FloatOptionNames.PhantomCooldown, FloatOptionNames.PhantomDuration, FloatOptionNames.TrackerCooldown,
            FloatOptionNames.TrackerDuration, FloatOptionNames.TrackerDelay, FloatOptionNames.NoisemakerAlertDuration,
            FloatOptionNames.ViperDissolveTime, FloatOptionNames.DetectiveSuspectLimit })
            try { _floats[n] = Opts.GetFloat(n); } catch { }
        foreach (var n in new[] { Int32OptionNames.NumImpostors, Int32OptionNames.KillDistance, Int32OptionNames.TaskBarMode,
            Int32OptionNames.DiscussionTime, Int32OptionNames.VotingTime, Int32OptionNames.NumEmergencyMeetings,
            Int32OptionNames.EmergencyCooldown, Int32OptionNames.NumCommonTasks, Int32OptionNames.NumLongTasks,
            Int32OptionNames.NumShortTasks })
            try { _ints[n] = Opts.GetInt(n); } catch { }
        foreach (var n in new[] { BoolOptionNames.ConfirmImpostor, BoolOptionNames.VisualTasks, BoolOptionNames.AnonymousVotes,
            BoolOptionNames.ShapeshifterLeaveSkin, BoolOptionNames.ImpostorsCanSeeProtect, BoolOptionNames.NoisemakerImpostorAlert })
            try { _bools[n] = Opts.GetBool(n); } catch { }
        if (Roles != null)
            foreach (RoleTypes r in new[] { RoleTypes.Scientist, RoleTypes.Engineer, RoleTypes.GuardianAngel,
                RoleTypes.Shapeshifter, RoleTypes.Noisemaker, RoleTypes.Tracker,
                RoleTypes.Phantom, RoleTypes.Viper, RoleTypes.Detective })
                try { _roles[r] = (Roles.GetChancePerGame(r), Roles.GetNumPerGame(r)); } catch { }
        _shadowInit = true;
    }

    private static void ApplyAll()
    {
        if (!IsHost || Opts == null) return;
        try {
            foreach (var kv in _floats)   try { Opts.SetFloat(kv.Key, kv.Value); } catch { }
            foreach (var kv in _ints)     try { Opts.SetInt(kv.Key, kv.Value); } catch { }
            foreach (var kv in _bools)    try { Opts.SetBool(kv.Key, kv.Value); } catch { }
            try { Opts.SetByte(ByteOptionNames.MapId, _mapId); } catch { }
            if (Roles != null)
                foreach (var kv in _roles) try { Roles.SetRoleRate(kv.Key, kv.Value.count, kv.Value.chance); } catch { }
            Sync();
        } catch { }
    }

    private static readonly string[] _killDistLabels  = { "Short", "Medium", "Long" };
    private static readonly string[] _taskBarLabels   = { "Always", "Meetings", "Never" };
    private static readonly Color    _mapSelected     = new Color(0.2f, 0.7f, 0.4f, 1f);
    private static readonly Color    _mapUnselected   = new Color(0.25f, 0.25f, 0.25f, 1f);

    private static void FloatRow(string label, FloatOptionNames opt, float step, bool host)
    {
        _floats.TryGetValue(opt, out float val);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {val:F1}", GUILayout.Width(220));
        if (host)
        {
            if (GUILayout.Button("-", GUILayout.Width(24))) _floats[opt] = Mathf.Round((val - step) * 10f) / 10f;
            if (GUILayout.Button("+", GUILayout.Width(24))) _floats[opt] = Mathf.Round((val + step) * 10f) / 10f;
        }
        GUILayout.EndHorizontal();
    }

    private static void IntRow(string label, Int32OptionNames opt, int min = -9999, int max = 9999, bool host = false)
    {
        _ints.TryGetValue(opt, out int val);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {val}", GUILayout.Width(220));
        if (host)
        {
            if (GUILayout.Button("-", GUILayout.Width(24))) _ints[opt] = System.Math.Clamp(val - 1, min, max);
            if (GUILayout.Button("+", GUILayout.Width(24))) _ints[opt] = System.Math.Clamp(val + 1, min, max);
        }
        GUILayout.EndHorizontal();
    }

    private static void LabeledIntRow(string label, Int32OptionNames opt, string[] labels, bool host)
    {
        _ints.TryGetValue(opt, out int val);
        string display = (val >= 0 && val < labels.Length) ? labels[val] : val.ToString();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {display}", GUILayout.Width(220));
        if (host)
        {
            if (GUILayout.Button("-", GUILayout.Width(24))) _ints[opt] = System.Math.Clamp(val - 1, 0, labels.Length - 1);
            if (GUILayout.Button("+", GUILayout.Width(24))) _ints[opt] = System.Math.Clamp(val + 1, 0, labels.Length - 1);
        }
        GUILayout.EndHorizontal();
    }

    private static void BoolRow(string label, BoolOptionNames opt, bool host)
    {
        _bools.TryGetValue(opt, out bool val);
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(220));
        if (host) { bool n = GUIStylePreset.CustomToggle(val, ""); if (n != val) _bools[opt] = n; }
        else GUILayout.Label(val ? "On" : "Off");
        GUILayout.EndHorizontal();
    }

    private void RoleRow(string label, RoleTypes role, bool host)
    {
        if (!_roles.TryGetValue(role, out var rv)) rv = (0, 0);
        int chance = rv.chance, count = rv.count;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}:", GUILayout.Width(130));
        GUILayout.Label("Chance:", GUILayout.Width(55));
        if (host && GUILayout.Button("-", GUILayout.Width(24))) _roles[role] = (System.Math.Clamp(chance - 1, 0, 100), count);
        GUILayout.Label($"{chance}%", GUILayout.Width(35));
        if (host && GUILayout.Button("+", GUILayout.Width(24))) _roles[role] = (System.Math.Clamp(chance + 1, 0, 100), count);
        GUILayout.Space(10);
        GUILayout.Label("Count:", GUILayout.Width(45));
        if (host && GUILayout.Button("-", GUILayout.Width(24))) _roles[role] = (chance, System.Math.Clamp(count - 1, 0, 9999));
        GUILayout.Label($"{count}", GUILayout.Width(25));
        if (host && GUILayout.Button("+", GUILayout.Width(24))) _roles[role] = (chance, System.Math.Clamp(count + 1, 0, 9999));
        GUILayout.EndHorizontal();
    }

    private static readonly (string name, byte id)[] Maps = {
        ("Skeld",   0), ("Mira",    1), ("Polus",   2),
        ("Airship", 4), ("Fungus",  5),
    };

    private void DrawMapSelection(bool host)
    {
        GUILayout.Label("Map", GUIStylePreset.TabSubtitle);
        GUILayout.BeginHorizontal();
        foreach (var (mapName, mapId) in Maps)
        {
            bool selected = _mapId == mapId;
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = selected ? _mapSelected : _mapUnselected;
            GUI.enabled = host;
            if (GUILayout.Button(mapName, GUILayout.ExpandWidth(true), GUILayout.Height(26)))
                _mapId = mapId;
            GUI.enabled = true;
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();
    }

    public void Draw()
    {
        if (Opts == null) { GUILayout.Label("No game options available."); return; }
        if (!_shadowInit) InitShadow();
        bool host = IsHost;

        if (_pendingSavePath != null)
        {
            string path = _pendingSavePath;
            _pendingSavePath = null;
            if (path.Length > 0) SaveToFile(path);
        }
        if (_pendingLoadPath != null)
        {
            string path = _pendingLoadPath;
            _pendingLoadPath = null;
            if (path.Length > 0) LoadFromFile(path);
        }

        var prevBg = GUI.backgroundColor;
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.15f, 0.55f, 0.15f, 1f);
        if (GUILayout.Button(_dialogRunning ? "..." : "Save to File", GUILayout.Height(28))) SaveAsync();

        GUI.backgroundColor = new Color(0.15f, 0.35f, 0.75f, 1f);
        GUI.enabled = host && !_dialogRunning;
        if (GUILayout.Button(_dialogRunning ? "..." : "Load from File", GUILayout.Height(28))) LoadAsync();
        GUI.enabled = true;

        GUI.backgroundColor = new Color(0.7f, 0.15f, 0.15f, 1f);
        GUI.enabled = host;
        if (GUILayout.Button("Apply Settings", GUILayout.Height(28))) ApplyAll();
        GUI.enabled = true;

        GUI.backgroundColor = new Color(0.4f, 0.4f, 0.1f, 1f);
        if (GUILayout.Button("Refresh", GUILayout.Height(28))) { _shadowInit = false; InitShadow(); }

        GUI.backgroundColor = new Color(0.65f, 0.55f, 0.05f, 1f);
        if (GUILayout.Button("Open Folder", GUILayout.Height(28))) { EnsureFolder(); try { System.Diagnostics.Process.Start("explorer.exe", FolderPath); } catch { } }
        GUI.backgroundColor = prevBg;
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        _scroll = GUILayout.BeginScrollView(_scroll);
        DrawMapSelection(host);

        GUILayout.Space(6);
        GUILayout.Label("General", GUIStylePreset.TabSubtitle);
        IntRow("Impostors",         Int32OptionNames.NumImpostors,     1, 3, host);
        FloatRow("Player Speed",    FloatOptionNames.PlayerSpeedMod,   0.1f, host);
        FloatRow("Crewmate Vision", FloatOptionNames.CrewLightMod,     0.1f, host);
        FloatRow("Impostor Vision", FloatOptionNames.ImpostorLightMod, 0.1f, host);
        FloatRow("Kill Cooldown",   FloatOptionNames.KillCooldown,     0.1f, host);
        LabeledIntRow("Kill Distance",    Int32OptionNames.KillDistance, _killDistLabels, host);
        BoolRow("Confirm Ejects",   BoolOptionNames.ConfirmImpostor, host);
        BoolRow("Visual Tasks",     BoolOptionNames.VisualTasks, host);
        BoolRow("Anonymous Votes",  BoolOptionNames.AnonymousVotes, host);
        LabeledIntRow("Task Bar Updates", Int32OptionNames.TaskBarMode, _taskBarLabels, host);

        GUILayout.Space(6);
        GUILayout.Label("Meetings", GUIStylePreset.TabSubtitle);
        IntRow("Discussion Time",    Int32OptionNames.DiscussionTime,       host: host);
        IntRow("Voting Time",        Int32OptionNames.VotingTime,           host: host);
        IntRow("Emergency Meetings", Int32OptionNames.NumEmergencyMeetings, -9999, 9999, host);
        IntRow("Emergency Cooldown", Int32OptionNames.EmergencyCooldown,    -9999, 9999, host);

        GUILayout.Space(6);
        GUILayout.Label("Tasks", GUIStylePreset.TabSubtitle);
        IntRow("Common Tasks", Int32OptionNames.NumCommonTasks, host: host);
        IntRow("Long Tasks",   Int32OptionNames.NumLongTasks,   host: host);
        IntRow("Short Tasks",  Int32OptionNames.NumShortTasks,  host: host);

        GUILayout.Space(6);
        GUILayout.Label("Roles  (C=Chance +-1%  N=Count +-1)", GUIStylePreset.TabSubtitle);
        RoleRow("Scientist",    RoleTypes.Scientist, host);
        RoleRow("Engineer",     RoleTypes.Engineer, host);
        RoleRow("Guardian",     RoleTypes.GuardianAngel, host);
        RoleRow("Shapeshifter", RoleTypes.Shapeshifter, host);
        RoleRow("Noisemaker",   RoleTypes.Noisemaker, host);
        RoleRow("Tracker",      RoleTypes.Tracker, host);
        RoleRow("Phantom",      RoleTypes.Phantom, host);
        RoleRow("Viper",        RoleTypes.Viper, host);
        RoleRow("Detective",    RoleTypes.Detective, host);

        GUILayout.Space(6);
        GUILayout.Label("Shapeshifter", GUIStylePreset.TabSubtitle);
        FloatRow("Shift Cooldown", FloatOptionNames.ShapeshifterCooldown, 0.1f, host);
        FloatRow("Shift Duration", FloatOptionNames.ShapeshifterDuration, 0.1f, host);
        BoolRow("Leave Skin",      BoolOptionNames.ShapeshifterLeaveSkin, host);

        GUILayout.Space(6);
        GUILayout.Label("Scientist", GUIStylePreset.TabSubtitle);
        FloatRow("Vitals Cooldown", FloatOptionNames.ScientistCooldown,      0.1f, host);
        FloatRow("Battery Charge",  FloatOptionNames.ScientistBatteryCharge, 1f, host);

        GUILayout.Space(6);
        GUILayout.Label("Engineer", GUIStylePreset.TabSubtitle);
        FloatRow("Vent Cooldown", FloatOptionNames.EngineerCooldown,      0.1f, host);
        FloatRow("Vent Duration", FloatOptionNames.EngineerInVentMaxTime, 0.1f, host);

        GUILayout.Space(6);
        GUILayout.Label("Guardian Angel", GUIStylePreset.TabSubtitle);
        FloatRow("Protect Cooldown",     FloatOptionNames.GuardianAngelCooldown,     0.1f, host);
        FloatRow("Protect Duration",     FloatOptionNames.ProtectionDurationSeconds, 0.1f, host);
        BoolRow("Impostors See Protect", BoolOptionNames.ImpostorsCanSeeProtect, host);

        GUILayout.Space(6);
        GUILayout.Label("Phantom", GUIStylePreset.TabSubtitle);
        FloatRow("Invisibility Cooldown", FloatOptionNames.PhantomCooldown, 0.1f, host);
        FloatRow("Invisibility Duration", FloatOptionNames.PhantomDuration, 0.1f, host);

        GUILayout.Space(6);
        GUILayout.Label("Tracker", GUIStylePreset.TabSubtitle);
        FloatRow("Track Cooldown", FloatOptionNames.TrackerCooldown, 0.1f, host);
        FloatRow("Track Duration", FloatOptionNames.TrackerDuration, 0.1f, host);
        FloatRow("Track Delay",    FloatOptionNames.TrackerDelay,    0.1f, host);

        GUILayout.Space(6);
        GUILayout.Label("Noisemaker", GUIStylePreset.TabSubtitle);
        FloatRow("Alert Duration", FloatOptionNames.NoisemakerAlertDuration, 0.1f, host);
        BoolRow("Impostor Alert",  BoolOptionNames.NoisemakerImpostorAlert, host);

        GUILayout.Space(6);
        GUILayout.Label("Viper", GUIStylePreset.TabSubtitle);
        FloatRow("Dissolve Time", FloatOptionNames.ViperDissolveTime, 0.1f, host);

        GUILayout.Space(6);
        GUILayout.Label("Detective", GUIStylePreset.TabSubtitle);
        FloatRow("Suspects Per Case", FloatOptionNames.DetectiveSuspectLimit, 1f, host);

        GUILayout.EndScrollView();
    }
}
