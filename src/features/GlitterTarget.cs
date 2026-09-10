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
        if (!Enabled) return;
        if (Target == null || Target.Data == null || Target.Data.Disconnected) return;
        if (!(AmongUsClient.Instance?.AmHost ?? false)) return;

        _timer += Time.deltaTime;
        float interval = Delay < 0.01f ? 0.01f : Delay > 2f ? 2f : Delay;
        if (_timer < interval) return;
        _timer = 0f;

        try
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueAppear(Target);
            batch.QueueVanish(Target);
            batch.FinishBatch();

            Target.RpcSetColor((byte)Utilities.GetFreeColor());
        }
        catch { }
    }
}