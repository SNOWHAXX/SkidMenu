using UnityEngine;

namespace SkidMenu.features;

public class RainbowTarget : MonoBehaviour
{
    public static RainbowTarget Instance;
    public PlayerControl Target;
    public bool Enabled;
    public float Delay = 0.2f;

    private float _timer;
    private int _colorIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(Instance.gameObject);
        Instance = this;
    }

    private void Update()
    {
        if (!Enabled || Target == null || Target.Data == null || Target.Data.Disconnected) return;

        _timer += Time.deltaTime;
        if (_timer < Mathf.Clamp(Delay, 0.01f, 2f)) return;
        _timer = 0f;

        _colorIndex = (_colorIndex + 1) % 18;
        ApplyColor(_colorIndex);
    }

    private void ApplyColor(int colorId)
    {
        try { Target.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId; } catch { }
        try { Target.cosmetics.SetColor(colorId); } catch { }
        if (AmongUsClient.Instance?.AmHost ?? false)
            try { Target.RpcSetColor((byte)colorId); } catch { }
    }
}
