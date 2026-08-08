using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using UnityEngine;

namespace SkidMenu.features;

public static class DummySpawner
{
    public static bool WalkToTasks   = true;
    public static bool FixSabotages  = true;
    public static bool ReportAndChat = false;
    public static bool UseKeybind    = false;
    public static KeyCode SpawnKey   = KeyCode.B;
    public static bool SpamEnabled   = false;
    public static float SpamDelay    = 3f;

    private static float _spamTimer = 0f;
    private static readonly List<PlayerControl> _dummies = new();

    public static void Tick()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        if (UseKeybind && Input.GetKeyDown(SpawnKey))
            SpawnDummy();

        if (SpamEnabled)
        {
            _spamTimer += Time.unscaledDeltaTime;
            if (_spamTimer >= SpamDelay) { _spamTimer = 0f; SpawnDummy(); }
        }
        else _spamTimer = 0f;
    }

    private static byte _dummyCounter = 100;

    public static void SpawnDummy()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
        if (PlayerControl.AllPlayerControls.Count >= 15) return;
        try
        {
            PlayerControl prefab = AmongUsClient.Instance.PlayerPrefab;
            if (prefab == null) return;

            var pos = PlayerControl.LocalPlayer != null
                ? (UnityEngine.Vector3)PlayerControl.LocalPlayer.transform.position + UnityEngine.Vector3.right * 1.5f
                : UnityEngine.Vector3.zero;

            PlayerControl dummy = UnityEngine.Object.Instantiate(prefab, pos, UnityEngine.Quaternion.identity);
            dummy.PlayerId = _dummyCounter;
            GameData.Instance.AddPlayerInfo(GameData.Instance.AddDummy(dummy));
            AmongUsClient.Instance.Spawn(dummy, -2, SpawnFlags.IsClientCharacter);
            ((UnityEngine.Behaviour)dummy.NetTransform).enabled = true;

            var hm = DestroyableSingleton<HatManager>.Instance;
            dummy.RpcSetColor((byte)UnityEngine.Random.Range(0, 18));
            dummy.RpcSetName("Bot" + UnityEngine.Random.Range(1000, 9999));
            dummy.RpcSetLevel((uint)UnityEngine.Random.Range(1, 150));
            if (hm != null)
            {
                if (hm.allHats.Count   > 0) dummy.RpcSetHat(hm.allHats[UnityEngine.Random.Range(0, hm.allHats.Count)].ProdId);
                if (hm.allSkins.Count  > 0) dummy.RpcSetSkin(hm.allSkins[UnityEngine.Random.Range(0, hm.allSkins.Count)].ProdId);
                if (hm.allVisors.Count > 0) dummy.RpcSetVisor(hm.allVisors[UnityEngine.Random.Range(0, hm.allVisors.Count)].ProdId);
            }
            dummy.RpcSetRole(RoleTypes.Crewmate, true);

            _dummies.Add(dummy);
            if (_dummyCounter >= 200) _dummyCounter = 100;
            else _dummyCounter++;

            if (WalkToTasks || FixSabotages || ReportAndChat)
                dummy.StartCoroutine(BotRoutine(dummy).WrapToIl2Cpp());
        }
        catch { }
    }

    public static void ClearDummies()
    {
        foreach (var d in _dummies)
            try { if (d != null) d.Despawn(); } catch { }
        _dummies.Clear();
    }

    private static IEnumerator BotRoutine(PlayerControl bot)
    {
        while (bot != null && bot.gameObject.activeInHierarchy)
        {
            yield return Effects.Wait(0.5f);

            if (bot == null || bot.Data == null || bot.Data.IsDead) yield break;

            bool fixedSabotage = false;
            if (FixSabotages && IsSabotageActive())
            {
                try { TryFixSabotage(); } catch { }
                fixedSabotage = true;
            }

            if (fixedSabotage)
            {
                yield return Effects.Wait(1f);
                continue;
            }

            if (ReportAndChat)
            {
                DeadBody bodyToReport = null;
                try
                {
                    foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
                    {
                        if (body == null) continue;
                        if (Vector2.Distance(bot.transform.position, body.transform.position) < 1.5f)
                        { bodyToReport = body; break; }
                    }
                }
                catch { }

                if (bodyToReport != null)
                {
                    try
                    {
                        var info = GameData.Instance.GetPlayerById(bodyToReport.ParentId);
                        if (info != null) bot.CmdReportDeadBody(info);
                    }
                    catch { }
                }
            }

            if (WalkToTasks && bot.myTasks != null)
            {
                PlayerTask taskToWalk = null;
                try
                {
                    foreach (PlayerTask task in bot.myTasks)
                    {
                        if (task != null && !task.IsComplete) { taskToWalk = task; break; }
                    }
                }
                catch { }

                if (taskToWalk != null)
                {
                    try { bot.NetTransform.RpcSnapTo(taskToWalk.transform.position); } catch { }
                    yield return Effects.Wait(0.3f);
                }
            }
        }
    }

    private static bool IsSabotageActive()
    {
        try
        {
            if (ShipStatus.Instance?.Systems == null) return false;
            return CheckSystem(SystemTypes.Reactor) || CheckSystem(SystemTypes.LifeSupp);
        }
        catch { return false; }
    }

    private static bool CheckSystem(SystemTypes type)
    {
        try
        {
            if (!ShipStatus.Instance.Systems.ContainsKey(type)) return false;
            var sys = ShipStatus.Instance.Systems[type];
            if (sys.TryCast<ReactorSystemType>() is { } r) return r.IsActive;
            if (sys.TryCast<LifeSuppSystemType>() is { } l) return l.IsActive;
            return false;
        }
        catch { return false; }
    }

    private static void TryFixSabotage()
    {
        Sabotage.FixSabotage(SystemTypes.Reactor);
        Sabotage.FixSabotage(SystemTypes.LifeSupp);
    }
}
