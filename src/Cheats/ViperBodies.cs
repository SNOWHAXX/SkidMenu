using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkidMenu;

public static class ViperBodies
{
    private const float ReportGraceSeconds = 2f;

    private struct ViperState
    {
        public float maxTime;
        public float seededAt;
        public float dissolvedAt;
        public int bodyInstanceId;
        public bool logFired;
    }

    private static readonly Dictionary<byte, ViperState> _states = new();

    public static void RegisterViper(byte parentId, float maxTime)
    {
        if (maxTime <= 0f) return;
        try
        {
            if (_states.TryGetValue(parentId, out var existing) && existing.maxTime > 0f
                && Time.time - existing.seededAt < existing.maxTime)
                return;
            _states[parentId] = new ViperState
            {
                maxTime = maxTime,
                seededAt = Time.time,
                dissolvedAt = -1f,
                bodyInstanceId = -1,
                logFired = false
            };
        }
        catch { }
    }

    public static bool IsViper(byte parentId)
    {
        return _states.TryGetValue(parentId, out var st) && st.maxTime > 0f;
    }

    public static float Remaining(byte parentId)
    {
        if (!_states.TryGetValue(parentId, out var st) || st.maxTime <= 0f) return -1f;
        return Mathf.Max(0f, st.maxTime - (Time.time - st.seededAt));
    }

    public static float Remaining(DeadBody body)
    {
        if (body == null) return -1f;
        float reg = Remaining(body.ParentId);
        if (reg >= 0f) return reg;
        if (body is not ViperDeadBody viper) return -1f;
        try { return Mathf.Max(0f, viper.maxDissolveTime - viper.dissolveCurrentTime); }
        catch { return -1f; }
    }

    public static bool IsFullyDissolved(DeadBody body)
    {
        if (body == null) return false;
        byte pid = body.ParentId;
        if (_states.TryGetValue(pid, out var st) && st.maxTime > 0f)
            return Time.time - st.seededAt >= st.maxTime;
        if (body is not ViperDeadBody viper) return false;
        try { return viper.maxDissolveTime > 0f && viper.dissolveCurrentTime >= viper.maxDissolveTime; }
        catch { return false; }
    }

    public static void TickBodies(DeadBody[] bodies)
    {
        if (bodies == null) return;
        for (int i = 0; i < bodies.Length; i++)
        {
            try
            {
                DeadBody body = bodies[i];
                if (body == null || body.gameObject == null) continue;
                byte pid = body.ParentId;
                int bid = body.gameObject.GetInstanceID();
                if (body is not ViperDeadBody viper)
                    continue;
                float maxT = viper.maxDissolveTime;
                if (maxT <= 0f && _states.TryGetValue(pid, out var prev) && prev.maxTime > 0f)
                    maxT = prev.maxTime;
                if (maxT <= 0f) continue;
                if (_states.TryGetValue(pid, out var st))
                {
                    float now = Time.time;
                    bool clockExpired = now - st.seededAt >= st.maxTime;
                    if (st.bodyInstanceId >= 0 && st.bodyInstanceId != bid && clockExpired)
                    {
                        st.seededAt = now;
                        st.dissolvedAt = -1f;
                        st.logFired = false;
                    }
                    st.bodyInstanceId = bid;
                    st.maxTime = maxT;
                    _states[pid] = st;
                }
                else
                {
                    _states[pid] = new ViperState
                    {
                        maxTime = maxT,
                        seededAt = Time.time,
                        dissolvedAt = -1f,
                        bodyInstanceId = bid,
                        logFired = false
                    };
                }
            }
            catch { }
        }
        float t = Time.time;
        foreach (var key in new List<byte>(_states.Keys))
        {
            var st = _states[key];
            if (st.maxTime <= 0f) continue;
            if (t - st.seededAt >= st.maxTime && st.dissolvedAt < 0f)
            {
                st.dissolvedAt = t;
                _states[key] = st;
            }
        }
        Cleanup();
    }

    public static bool CanReport(byte parentId)
    {
        if (!_states.TryGetValue(parentId, out var st) || st.maxTime <= 0f) return true;
        float now = Time.time;
        float dissolveEnd = st.seededAt + st.maxTime;
        if (now < dissolveEnd) return true;
        return now - dissolveEnd <= ReportGraceSeconds;
    }

    public static bool CanReport(DeadBody body)
    {
        if (body == null) return false;
        byte pid = body.ParentId;
        if (_states.TryGetValue(pid, out var st) && st.maxTime > 0f)
            return CanReport(pid);
        if (body is ViperDeadBody viper)
        {
            try { return viper.maxDissolveTime - viper.dissolveCurrentTime > 0f; }
            catch { }
        }
        return true;
    }

    public static bool LogDissolvedOnce(byte parentId)
    {
        if (!_states.TryGetValue(parentId, out var st) || st.maxTime <= 0f) return false;
        if (Time.time - st.seededAt < st.maxTime) return false;
        if (st.logFired) return false;
        st.logFired = true;
        _states[parentId] = st;
        return true;
    }

    public static bool LogDissolvedOnce(DeadBody body)
    {
        if (body == null || body is not ViperDeadBody) return false;
        return LogDissolvedOnce(body.ParentId);
    }

    public static DeadBody FindBody(byte parentId)
    {
        DeadBody[] bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < bodies.Length; i++)
            if (bodies[i] != null && bodies[i].ParentId == parentId) return bodies[i];
        return null;
    }

    public static string AcidTag(byte parentId, bool includeRemaining = true)
    {
        if (!_states.TryGetValue(parentId, out var st) || st.maxTime <= 0f) return "";
        try
        {
            float remaining = Mathf.Max(0f, st.maxTime - (Time.time - st.seededAt));
            string suffix = includeRemaining
                ? remaining > 0f ? $" {remaining:F1}s" : " <color=#aaaaaa>gone</color>"
                : "";
            return $" <color=#00ff88>[ACID{suffix}]</color>";
        }
        catch { return " <color=#00ff88>[ACID]</color>"; }
    }

    public static string AcidTag(DeadBody body, bool includeRemaining = true)
    {
        if (body is not ViperDeadBody) return "";
        return AcidTag(body.ParentId, includeRemaining);
    }

    private static void Cleanup()
    {
        if (_states.Count >= 64)
        {
            var stale = new List<byte>();
            float now = Time.time;
            foreach (var kv in _states)
            {
                var st = kv.Value;
                if (now - (st.seededAt + st.maxTime) > 10f) stale.Add(kv.Key);
            }
            for (int i = 0; i < stale.Count; i++) _states.Remove(stale[i]);
            if (_states.Count >= 64) _states.Clear();
        }
    }
}
