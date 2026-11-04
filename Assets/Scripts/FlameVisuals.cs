using UnityEngine;

internal static class FlameVisuals
{
    private static Sprite _flame;

    internal static Sprite Flame
    {
        get
        {
            if (_flame == null) _flame = MakeFlame();
            return _flame;
        }
    }

    private static Sprite MakeFlame()
    {
        const int w = 64;
        const int h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float v = (y + 0.5f) / h;
            float body = Mathf.Sin(Mathf.PI * Mathf.Pow(v, 0.62f));
            float half = 0.48f * Mathf.Pow(Mathf.Clamp01(body), 0.85f);
            float bend = 0.06f * Mathf.Sin(v * 5f) * v;
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w - 0.5f - bend;
                float edge = (half - Mathf.Abs(u)) / 0.14f;
                float a = Mathf.Clamp01(edge);
                a *= Mathf.Lerp(1f, 0.55f, Mathf.Pow(v, 2f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 64f);
    }

    internal static Color Outer(float alpha) { return new Color(1f, 0.22f, 0.04f, alpha); }
    internal static Color Mid(float alpha) { return new Color(1f, 0.55f, 0.1f, alpha); }
    internal static Color Core(float alpha) { return new Color(1f, 0.93f, 0.55f, alpha); }
}

internal sealed class BurnFlames : MonoBehaviour
{
    private const int Count = 12;

    private SpriteRenderer[] _outer;
    private SpriteRenderer[] _inner;
    private float[] _phase;
    private float[] _lane;
    private float[] _speed;
    private Vector2 _size = Vector2.one;
    private int _order = 1;
    private bool _ready;

    private void Build()
    {
        var panel = GetComponent<SpriteRenderer>();
        if (panel != null)
        {
            _size = panel.size;
            _order = panel.sortingOrder;
            panel.enabled = false;
        }

        _outer = new SpriteRenderer[Count];
        _inner = new SpriteRenderer[Count];
        _phase = new float[Count];
        _lane = new float[Count];
        _speed = new float[Count];

        for (int i = 0; i < Count; i++)
        {
            _outer[i] = NewFlame("Flame", _order + 1);
            _inner[i] = NewFlame("FlameCore", _order + 2);
            _phase[i] = Random.value;
            _lane[i] = Mathf.Lerp(-0.5f, 0.5f, (i + Random.value * 0.6f) / Count);
            _speed[i] = Random.Range(1.1f, 1.8f);
        }
        _ready = true;
    }

    private SpriteRenderer NewFlame(string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FlameVisuals.Flame;
        sr.sortingOrder = order;
        return sr;
    }

    private void OnEnable()
    {
        if (!_ready) Build();
    }

    private void Update()
    {
        if (!_ready) return;

        float baseY = -_size.y * 0.5f;
        float length = _size.y * 0.55f;
        float width = _size.x * 0.2f;

        for (int i = 0; i < Count; i++)
        {
            float k = Mathf.Repeat(Time.time * _speed[i] + _phase[i], 1f);
            float sway = Mathf.Sin(Time.time * 7f + i * 1.7f) * width * 0.35f * k;
            float x = _lane[i] * _size.x * (1f - 0.35f * k) + sway;
            float y = baseY + k * _size.y * 0.75f;
            float scale = Mathf.Lerp(1f, 0.25f, k);
            float flicker = 1f + 0.12f * Mathf.Sin(Time.time * 23f + i * 2.3f);
            float alpha = Mathf.Clamp01(Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI) * 1.6f) * 0.85f;

            Place(_outer[i], x, y, width * 1.6f * scale, length * scale * flicker, FlameVisuals.Outer(alpha));
            Place(_inner[i], x, y, width * 0.85f * scale, length * 0.62f * scale * flicker, FlameVisuals.Core(alpha));
        }
    }

    private static void Place(SpriteRenderer sr, float x, float y, float width, float height, Color color)
    {
        Transform t = sr.transform;
        t.localPosition = new Vector3(x, y, 0f);
        t.localScale = new Vector3(width, height / 2f, 1f);
        sr.color = color;
    }
}
