using System.Collections.Generic;
using UnityEngine;

internal sealed class SpellVfxStyle
{
    internal SpellVfxKind Body;
    internal SpellVfxKind Rider;
    internal SpellEffect BodyEffect;
    internal SpellEffect RiderEffect;
    internal int[] RiderData;
    internal Color RiderColor = Color.white;
    internal bool Fur;
    internal float Boost = 1f;
}

internal static class VfxShapes
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    private static float Edge(float inside)
    {
        return inside > 0f ? Mathf.Clamp01(inside * 24f) : 0f;
    }

    private static Sprite Make(string key, System.Func<float, float, float> alpha)
    {
        if (Cache.TryGetValue(key, out Sprite found) && found != null) return found;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float a = Mathf.Clamp01(alpha((x + 0.5f) / size, (y + 0.5f) / size));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        Cache[key] = sprite;
        return sprite;
    }

    internal static Sprite Flame
    {
        get
        {
            return Make("flame", (nx, ny) =>
            {
                float cx = 0.5f + 0.07f * Mathf.Sin(ny * 7f) * ny;
                float half;
                if (ny < 0.32f)
                {
                    float u = (ny - 0.32f) / 0.32f;
                    half = 0.4f * Mathf.Sqrt(Mathf.Max(0f, 1f - u * u));
                }
                else half = 0.4f * Mathf.Pow(Mathf.Max(0f, 1f - (ny - 0.32f) / 0.66f), 1.25f);
                return Edge(half - Mathf.Abs(nx - cx));
            });
        }
    }

    internal static Sprite Cross
    {
        get
        {
            return Make("cross", (nx, ny) =>
            {
                float dx = Mathf.Abs(nx - 0.5f);
                float dy = Mathf.Abs(ny - 0.5f);
                float m = Mathf.Max(Mathf.Min(0.17f - dx, 0.45f - dy), Mathf.Min(0.17f - dy, 0.45f - dx));
                return Edge(m);
            });
        }
    }

    internal static Sprite Skull
    {
        get
        {
            return Make("skull", (nx, ny) =>
            {
                var p = new Vector2(nx, ny);
                float head = 0.33f - Vector2.Distance(p, new Vector2(0.5f, 0.58f));
                float jaw = Mathf.Min(0.17f - Mathf.Abs(nx - 0.5f), Mathf.Min(ny - 0.12f, 0.36f - ny));
                float inside = Mathf.Max(head, jaw);
                bool eyeL = Vector2.Distance(p, new Vector2(0.4f, 0.58f)) < 0.085f;
                bool eyeR = Vector2.Distance(p, new Vector2(0.6f, 0.58f)) < 0.085f;
                bool nose = ny > 0.4f && ny < 0.5f && Mathf.Abs(nx - 0.5f) < (0.5f - ny) * 0.6f;
                if (eyeL || eyeR || nose) return 0f;
                return Edge(inside);
            });
        }
    }

    internal static Sprite Bat
    {
        get
        {
            return Make("bat", (nx, ny) =>
            {
                float ax = Mathf.Abs(nx - 0.5f);
                float body = 0.09f - Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.5f));
                float top = 0.5f + 0.24f * Mathf.Cos(Mathf.Min(ax, 0.5f) * 3.1f);
                float bottom = 0.42f - 0.06f * Mathf.Abs(Mathf.Sin(ax * 28f));
                float wing = Mathf.Min(top - ny, ny - bottom);
                if (ax > 0.07f && ax < 0.48f && wing > 0f) return Edge(wing);
                return Edge(body);
            });
        }
    }

    internal static Sprite Drop
    {
        get
        {
            return Make("drop", (nx, ny) =>
            {
                float ax = Mathf.Abs(nx - 0.5f);
                float bulb = 0.28f - Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.34f));
                float tip = ny > 0.4f ? (0.95f - ny) / 0.55f * 0.26f - ax : -1f;
                return Edge(Mathf.Max(bulb, tip));
            });
        }
    }

    internal static Sprite Ring
    {
        get
        {
            return Make("ring", (nx, ny) =>
                Edge(0.09f - Mathf.Abs(Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.5f)) - 0.33f)));
        }
    }

    internal static Sprite Bar
    {
        get
        {
            return Make("bar", (nx, ny) =>
                Edge(Mathf.Min(0.09f - Mathf.Abs(nx - 0.5f), 0.42f - Mathf.Abs(ny - 0.5f))));
        }
    }

    internal static Sprite Puff
    {
        get
        {
            return Make("puff", (nx, ny) =>
            {
                Vector2 d = new Vector2(nx - 0.5f, ny - 0.5f);
                float angle = Mathf.Atan2(d.y, d.x);
                float r = 0.36f + 0.06f * Mathf.Sin(angle * 9f) + 0.03f * Mathf.Sin(angle * 17f);
                return Mathf.Clamp01((r - d.magnitude) * 14f);
            });
        }
    }

    internal static Sprite Crystal
    {
        get
        {
            return Make("crystal", (nx, ny) =>
                Edge(0.42f - (Mathf.Abs(nx - 0.5f) * 1.4f + Mathf.Abs(ny - 0.5f))));
        }
    }

    internal static Sprite Spike
    {
        get
        {
            return Make("spike", (nx, ny) =>
                Edge(Mathf.Min((0.98f - ny) * 0.2f - Mathf.Abs(nx - 0.5f), ny - 0.02f)));
        }
    }
}

internal sealed class FlameOverlay : MonoBehaviour
{
    private const int Count = 9;

    private SpriteRenderer _area;
    private readonly SpriteRenderer[] _outer = new SpriteRenderer[Count];
    private readonly SpriteRenderer[] _inner = new SpriteRenderer[Count];
    private readonly float[] _spread = new float[Count];
    private readonly float[] _seed = new float[Count];

    internal static void Attach(GameObject burnObject)
    {
        if (burnObject == null || burnObject.GetComponent<FlameOverlay>() != null) return;

        SpriteRenderer area = burnObject.GetComponent<SpriteRenderer>();
        if (area == null) area = burnObject.GetComponentInChildren<SpriteRenderer>(true);
        if (area == null || area.sprite == null) return;

        FlameOverlay overlay = burnObject.AddComponent<FlameOverlay>();
        overlay._area = area;
        area.enabled = false;

        for (int i = 0; i < Count; i++)
        {
            overlay._spread[i] = Count == 1 ? 0f : Mathf.Lerp(-0.42f, 0.42f, (float)i / (Count - 1));
            overlay._seed[i] = Random.value * 50f;
            overlay._outer[i] = overlay.NewFlame(new Color(1f, 0.42f, 0.08f, 0.9f), area.sortingOrder + 1);
            overlay._inner[i] = overlay.NewFlame(new Color(1f, 0.9f, 0.4f, 0.95f), area.sortingOrder + 2);
        }
    }

    private SpriteRenderer NewFlame(Color color, int order)
    {
        var go = new GameObject("Flame");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VfxShapes.Flame;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    private void Update()
    {
        if (_area == null) return;

        Vector3 center = _area.transform.TransformPoint(_area.sprite.bounds.center);
        Vector3 size = Vector3.Scale(_area.sprite.bounds.size, _area.transform.lossyScale);
        float t = Time.time;

        for (int i = 0; i < Count; i++)
        {
            float flick = 0.75f + 0.5f * Mathf.PerlinNoise(_seed[i], t * 7f);
            float sway = (Mathf.PerlinNoise(_seed[i] + 9f, t * 3f) - 0.5f) * 0.18f * size.x;
            float lift = Mathf.PerlinNoise(_seed[i] + 4f, t * 2f) * 0.12f * size.y;

            float width = size.x * 0.24f * flick;
            float height = size.y * 0.62f * flick;
            Vector3 position = new Vector3(center.x + _spread[i] * size.x + sway,
                center.y - size.y * 0.28f + height * 0.5f + lift, center.z);
            Quaternion tilt = Quaternion.Euler(0f, 0f, (Mathf.PerlinNoise(_seed[i] + 2f, t * 4f) - 0.5f) * 16f);

            _outer[i].transform.position = position;
            _outer[i].transform.rotation = tilt;
            _outer[i].transform.localScale = new Vector3(width, height, 1f);

            _inner[i].transform.position = position - new Vector3(0f, height * 0.12f, 0f);
            _inner[i].transform.rotation = tilt;
            _inner[i].transform.localScale = new Vector3(width * 0.55f, height * 0.58f, 1f);
        }
    }
}
