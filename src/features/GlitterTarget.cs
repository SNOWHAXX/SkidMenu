using UnityEngine;

namespace SkidMenu.features;

public class GlitterTarget : MonoBehaviour
{
    public static GlitterTarget Instance;
    public PlayerControl Target;
    public bool Enabled;
    public float Delay = 0.05f;

    private float _timer;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(Instance.gameObject);
        Instance = this;
    }

    private void Update()
    {
        if (!Enabled || Target == null || Target.Data == null || Target.Data.Disconnected) return;
        if (!(AmongUsClient.Instance?.AmHost ?? false)) return;

        _timer += Time.deltaTime;
        if (_timer < Mathf.Clamp(Delay, 0.01f, 2f)) return;
        _timer = 0f;

        try
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueAppear(Target);
            batch.QueueVanish(Target);
            batch.FinishBatch();

            if (AmongUsClient.Instance?.AmHost ?? false)
                Target.RpcSetColor((byte)Utilities.GetFreeColor());
        }
        catch { }
    }
}