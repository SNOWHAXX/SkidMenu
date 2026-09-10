using UnityEngine;

namespace SkidMenu.features;

public class LagGhost : MonoBehaviour
{
    private static GameObject _ghost;
    private static SpriteRenderer _ghostBody;
    private static Vector3 _ghostOffset;
    private static float _checkTimer;
    private static bool _ghostActive;
    private static Vector2 _lastDrawnPos;

    private void Update()
    {
        _checkTimer += Time.unscaledDeltaTime;
        if (_checkTimer < 0.25f) return;
        _checkTimer = 0f;

        bool want = LagCompensation.Enabled && LagCompensation.ShowGhost && LagCompensation.HasServerPos
            && PlayerControl.LocalPlayer != null && !SkidMenu.isPanicked;

        if (!want)
        {
            if (_ghostActive && _ghost != null) { _ghost.SetActive(false); _ghostActive = false; }
            return;
        }

        try
        {
            if (_ghost == null) BuildGhost();
            if (_ghost == null) return;
            if (!_ghostActive) { _ghost.SetActive(true); _ghostActive = true; }
            Vector2 server = LagCompensation.LastServerPos;
            if (server != _lastDrawnPos)
            {
                _lastDrawnPos = server;
                _ghost.transform.position = new Vector3(server.x, server.y, 0f) + _ghostOffset + new Vector3(0f, 0.35f, 0f);
            }
        }
        catch { }
    }

    private static void BuildGhost()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        SpriteRenderer src = null;
        try
        {
            var body = local.cosmetics?.currentBodySprite;
            if (body != null) src = body.BodySprite;
        }
        catch { }
        if (src == null || src.sprite == null)
        {
            var srcRenderers = local.GetComponentsInChildren<SpriteRenderer>(true);
            if (srcRenderers == null || srcRenderers.Length == 0) return;
            foreach (var r in srcRenderers)
            {
                if (r == null || r.sprite == null) continue;
                string n = r.gameObject.name.ToLower();
                if (n.Contains("body"))
                {
                    src = r;
                    break;
                }
            }
            src ??= srcRenderers[0];
        }
        if (src == null || src.sprite == null) return;

        _ghost = new GameObject("LagGhost");
        _ghost.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(_ghost);

        _ghostBody = _ghost.AddComponent<SpriteRenderer>();
        _ghostBody.sprite = src.sprite;
        try { _ghostBody.material = new Material(Shader.Find("Sprites/Default")); } catch { }
        _ghostBody.color = new Color(0.4f, 0.85f, 1f, 0.5f);
        _ghostBody.sortingLayerName = src.sortingLayerName;
        _ghostBody.sortingOrder = src.sortingOrder + 5;
        try { _ghost.transform.localScale = src.gameObject.transform.lossyScale; }
        catch { _ghost.transform.localScale = Vector3.one * 0.5f; }
        _ghostOffset = src.gameObject.transform.position - local.transform.position;
    }

    private void OnDestroy()
    {
        try { if (_ghost != null) Object.Destroy(_ghost); } catch { }
        _ghost = null;
        _ghostBody = null;
    }
}