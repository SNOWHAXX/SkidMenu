using UnityEngine;
using System.Collections.Generic;
using SkidMenu.routines;

namespace SkidMenu;

public class FullyRandomizeRoutine : IRoutine
{
    private readonly Queue<System.Action> _steps = new();
    private float _timer = 0f;
    private float _delay = 0.10f;

    public FullyRandomizeRoutine()
    {
        RoutineName = "FullyRandomize";
    }

    public void Schedule(List<System.Action> steps, float delay)
    {
        _steps.Clear();
        foreach (var s in steps)
            _steps.Enqueue(s);
        _delay = delay;
        _timer = 0f;
        Enabled = _steps.Count > 0;
    }

    public override void Run()
    {
        if (_steps.Count == 0) { Enabled = false; return; }

        if (PlayerControl.LocalPlayer == null) { _steps.Clear(); Enabled = false; return; }

        _timer += Time.deltaTime;
        if (_timer < _delay) return;
        _timer = 0f;

        try { _steps.Dequeue()?.Invoke(); }
        catch { _steps.Clear(); Enabled = false; return; }

        if (_steps.Count == 0) Enabled = false;
    }
}
