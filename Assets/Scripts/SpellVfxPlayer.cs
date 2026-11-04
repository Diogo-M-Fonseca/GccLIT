using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal enum SpellVfxKind
{
    None,
    Fireball,
    Thunder,
    Heal,
    Spark,
    DarkAura,
    Splash,
    Frost,
    Hearts,
    Meteor,
    Explosion,
    Mirror,
    Slots,
    Cut,
    Salt,
    Shatter,
    Git,
    Rainbow,
    Poison,
    Poop,
    Familiar,
    Sacrifice
}

internal enum SpellCue
{
    ReelStop,
    Jackpot
}

internal struct SpellVfxTiming
{
    internal float Duration;
    internal float FirstImpact;
    internal float SoundSpan;
    internal float FinaleSpan;
}

internal sealed partial class SpellVfxPlayer : MonoBehaviour
{
    [SerializeField] private float _fireballTravelTime = 0.55f;
    [SerializeField] private float _sparkTravelTime = 0.25f;
    [SerializeField] private float _healTime = 0.9f;
    [SerializeField] private float _auraTime = 0.9f;
    [SerializeField] private float _splashTime = 0.8f;
    [SerializeField] private float _frostTime = 0.7f;
    [Tooltip("Hearts animation: total time and how many hearts fill the screen.")]
    [SerializeField] private float _heartsTime = 2.6f;
    [SerializeField] private int _heartCount = 45;
    [Tooltip("Meteor shower: fall time of one meteor, meteors per wave and pause between waves.")]
    [SerializeField] private float _meteorFallTime = 0.6f;
    [SerializeField] private int _meteorsPerWave = 10;
    [SerializeField] private float _meteorWavePause = 0.25f;
    [Tooltip("Explosion: total time of the blast.")]
    [SerializeField] private float _explosionTime = 1.4f;
    [Tooltip("Mirror: total time of the shimmer.")]
    [SerializeField] private float _mirrorTime = 0.9f;
    [Tooltip("Slot machine: how long the first reel spins, and the extra time each next reel keeps spinning.")]
    [SerializeField] private float _slotsSpinTime = 0.9f;
    [SerializeField] private float _slotsStagger = 0.35f;
    [Tooltip("Cut: time one slash takes to cross the target, and the pause before the next slash.")]
    [SerializeField] private float _cutSlashTime = 0.12f;
    [SerializeField] private float _cutPause = 0.08f;
    [Tooltip("Salt: how many grains fall and how long one grain takes to fall.")]
    [SerializeField] private int _saltGrains = 40;
    [SerializeField] private float _saltFallTime = 0.45f;
    [Tooltip("Shatter: time the cracks take to spread over the glass, and time the shards fly after it breaks.")]
    [SerializeField] private float _shatterCrackTime = 0.4f;
    [SerializeField] private float _shatterTime = 0.8f;
    [Tooltip("Git: seconds one digit takes to fly, pause between digits, font size of the digits (TMP 3D: roughly 0.1 world units per point) and how many extra decorative digits swarm along.")]
    [SerializeField] private float _gitTravelTime = 0.45f;
    [SerializeField] private float _gitDigitGap = 0.12f;
    [SerializeField] private float _gitDigitSize = 6f;
    [SerializeField] private int _gitCloud = 30;
    [Tooltip("Git: how many digits of Matrix-style code rain fall over the screen.")]
    [SerializeField] private int _gitRain = 60;
    [Tooltip("Git: seconds between the fading dots a flying digit leaves behind (smaller = longer, denser trail).")]
    [SerializeField] private float _gitTrailInterval = 0.03f;
    [Tooltip("Poison: total time of the bubbling.")]
    [SerializeField] private float _poisonTime = 1.1f;
    [Tooltip("Poop: seconds the pile takes to fall, and how long it stays (stinking) before fading.")]
    [SerializeField] private float _poopFallTime = 0.45f;
    [SerializeField] private float _poopHold = 0.7f;
    [Tooltip("Rainbow: time it takes to shoot across to the target, and how long it stays before fading.")]
    [SerializeField] private float _rainbowTravelTime = 0.5f;
    [SerializeField] private float _rainbowHold = 0.35f;
    [SerializeField] private float _thunderHeight = 4f;
    [SerializeField] private int _sortingOrder = 200;

    private bool _tintOn;
    private Color _tintColor = Color.white;
    private bool _tintRainbow;
    private readonly List<SpriteRenderer> _tintedSprites = new List<SpriteRenderer>();
    private readonly List<TextMeshPro> _tintedTexts = new List<TextMeshPro>();
    private const float TintStrength = 0.8f;

    private bool _tintShades;

    internal void SetTint(Color color, bool rainbow, bool shades = false)
    {
        _tintOn = true;
        _tintShades = shades;
        _tintColor = color;
        _tintRainbow = rainbow;
    }

    internal void ClearTint()
    {
        _tintOn = false;
        _tintedSprites.Clear();
        _tintedTexts.Clear();
    }

    private Color TintFor(Vector3 position)
    {
        if (_tintShades)
        {
            Color.RGBToHSV(_tintColor, out float h, out float s, out float v);
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f + position.x * 1.5f + position.y * 1.1f);
            return Color.HSVToRGB(h, Mathf.Lerp(0.35f, 1f, wave), Mathf.Lerp(0.55f, 1f, wave));
        }
        if (!_tintRainbow) return _tintColor;
        float hue = Mathf.Repeat(Time.time * 0.9f + position.x * 0.12f + position.y * 0.08f, 1f);
        return Color.HSVToRGB(hue, 0.85f, 1f);
    }

    private void LateUpdate()
    {
        UpdatePets();
        if (!_tintOn) return;

        for (int i = _tintedSprites.Count - 1; i >= 0; i--)
        {
            SpriteRenderer sr = _tintedSprites[i];
            if (sr == null) { _tintedSprites.RemoveAt(i); continue; }
            Color c = sr.color;
            Color t = TintFor(sr.transform.position);
            sr.color = new Color(Mathf.Lerp(c.r, t.r, TintStrength), Mathf.Lerp(c.g, t.g, TintStrength),
                Mathf.Lerp(c.b, t.b, TintStrength), c.a);
        }

        for (int i = _tintedTexts.Count - 1; i >= 0; i--)
        {
            TextMeshPro tmp = _tintedTexts[i];
            if (tmp == null) { _tintedTexts.RemoveAt(i); continue; }
            Color c = tmp.color;
            Color t = TintFor(tmp.transform.position);
            tmp.color = new Color(Mathf.Lerp(c.r, t.r, TintStrength), Mathf.Lerp(c.g, t.g, TintStrength),
                Mathf.Lerp(c.b, t.b, TintStrength), c.a);
        }
    }

    private static Sprite _circleSprite;
    private static Sprite _squareSprite;
    private static Sprite _heartSprite;
    private static Sprite _shardSprite;

    internal IEnumerator Play(SpellVfxKind kind, Combatant caster, Combatant target, System.Action onImpact,
        int impacts = 1, int[] data = null, System.Action<SpellCue, int> onCue = null)
    {
        bool fired = false;
        void Fire()
        {
            if (fired) return;
            fired = true;
            if (onImpact != null) onImpact();
        }

        void Hit()
        {
            fired = true;
            if (onImpact != null) onImpact();
        }

        if (kind == SpellVfxKind.None || caster == null || target == null)
        {
            Fire();
            yield break;
        }

        Vector3 from = caster.VfxPoint.position;
        Vector3 to = target.VfxPoint.position;

        if (_style.Helix && (kind == SpellVfxKind.Fireball || kind == SpellVfxKind.Splash))
        {
            if (_style.Fur) StartCoroutine(FurSkin(kind, from, to));
            yield return Helix(from, to, Fire);
            Fire();
            yield break;
        }

        if (_style.Fur) StartCoroutine(FurSkin(kind, from, to));

        switch (kind)
        {
            case SpellVfxKind.Fireball: yield return Fireball(from, to, Fire, _style.Scale); break;
            case SpellVfxKind.Thunder: yield return Thunder(to, Fire); break;
            case SpellVfxKind.Heal: yield return Heal(from, Fire); break;
            case SpellVfxKind.Spark: yield return Spark(from, to, Fire); break;
            case SpellVfxKind.DarkAura: yield return DarkAura(to, Fire); break;
            case SpellVfxKind.Splash: yield return Splash(to, Fire); break;
            case SpellVfxKind.Frost: yield return Frost(to, Fire); break;
            case SpellVfxKind.Hearts: yield return Hearts(Fire); break;
            case SpellVfxKind.Meteor: yield return Meteors(Hit, impacts); break;
            case SpellVfxKind.Explosion: yield return ExplosionStyled(Vector3.Lerp(from, to, 0.5f), Fire); break;
            case SpellVfxKind.Mirror: yield return Mirror(from, Fire); break;
            case SpellVfxKind.Slots: yield return Slots(data, Fire, onCue, from, to); break;
            case SpellVfxKind.Cut: yield return Cuts(target, Hit, impacts); break;
            case SpellVfxKind.Salt: yield return Salt(target, Fire); break;
            case SpellVfxKind.Shatter: yield return Shatter(target, Fire); break;
            case SpellVfxKind.Git: yield return Git(from, to, data, Hit); break;
            case SpellVfxKind.Rainbow: yield return Rainbow(from, to, Fire); break;
            case SpellVfxKind.Poison: yield return Poison(to, Fire); break;
            case SpellVfxKind.Poop: yield return Poop(to, Fire); break;
            case SpellVfxKind.Familiar: yield return Summon(caster, target, Fire); break;
            case SpellVfxKind.Sacrifice: yield return Ritual(from, Fire); break;
        }

        Fire();
    }

    internal SpellVfxTiming GetTiming(SpellVfxKind kind, int impacts = 1, int[] data = null)
    {
        float duration = 0f;
        float first = 0f;
        float span = -1f;
        float finale = 0f;

        switch (kind)
        {
            case SpellVfxKind.Fireball:
                first = Mathf.Max(0f, _fireballTravelTime);
                duration = first + 0.35f;
                break;
            case SpellVfxKind.Thunder:
                duration = 3f * 0.11f + 0.1f + 0.25f;
                break;
            case SpellVfxKind.Heal:
                first = _healTime * 0.4f;
                duration = _healTime;
                break;
            case SpellVfxKind.Spark:
                first = Mathf.Max(0f, _sparkTravelTime);
                duration = first + 0.2f;
                break;
            case SpellVfxKind.DarkAura:
                first = _auraTime * 0.5f;
                duration = _auraTime;
                break;
            case SpellVfxKind.Splash:
                first = _splashTime * 0.5f;
                duration = _splashTime + 0.25f;
                break;
            case SpellVfxKind.Frost:
                first = _frostTime * 0.45f;
                duration = _frostTime + 0.25f;
                break;
            case SpellVfxKind.Hearts:
                first = _heartsTime * 0.3f;
                duration = _heartsTime;
                break;
            case SpellVfxKind.Meteor:
            {
                int waves = Mathf.Max(1, impacts);
                duration = waves * (0.4f + _meteorFallTime) + (waves - 1) * _meteorWavePause;
                first = _meteorFallTime + 0.05f;
                break;
            }
            case SpellVfxKind.Explosion:
                first = _explosionTime * 0.35f;
                duration = _explosionTime;
                break;
            case SpellVfxKind.Mirror:
                first = _mirrorTime * 0.4f;
                duration = _mirrorTime;
                break;
            case SpellVfxKind.Slots:
            {
                float lastStop = _slotsSpinTime + 2f * _slotsStagger;
                first = lastStop + 0.2f;
                duration = first + 0.7f;
                span = lastStop;
                finale = duration - lastStop;
                break;
            }
            case SpellVfxKind.Cut:
            {
                int slashes = Mathf.Max(1, impacts);
                duration = slashes * _cutSlashTime + (slashes - 1) * _cutPause;
                first = _cutSlashTime;
                break;
            }
            case SpellVfxKind.Salt:
            {
                float fall = Mathf.Max(0.05f, _saltFallTime);
                duration = 0.5f + fall + 0.1f;
                first = 0.3f + fall;
                break;
            }
            case SpellVfxKind.Shatter:
            {
                float crack = Mathf.Max(0.05f, _shatterCrackTime);
                first = crack;
                duration = crack + Mathf.Max(0.05f, _shatterTime);
                break;
            }
            case SpellVfxKind.Git:
            {
                int[] bits = data != null && data.Length > 0 ? data : new[] { 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0 };
                float gap = Mathf.Max(0.01f, _gitDigitGap);
                float travel = Mathf.Max(0.05f, _gitTravelTime);
                int firstOne = 0;
                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i] != 1) continue;
                    firstOne = i;
                    break;
                }
                first = firstOne * gap + travel;
                duration = (bits.Length - 1) * gap + travel + 0.1f;
                break;
            }
            case SpellVfxKind.Rainbow:
            {
                float travel = Mathf.Max(0.05f, _rainbowTravelTime);
                first = travel;
                duration = travel + Mathf.Max(0f, _rainbowHold) + 0.3f;
                break;
            }
            case SpellVfxKind.Poison:
            {
                float time = Mathf.Max(0.2f, _poisonTime);
                first = time * 0.3f;
                duration = time;
                break;
            }
            case SpellVfxKind.Familiar:
                first = SummonTime * 0.7f;
                duration = SummonTime;
                break;
            case SpellVfxKind.Sacrifice:
                first = RitualTime * 0.65f;
                duration = RitualTime;
                break;
            case SpellVfxKind.Poop:
            {
                float fall = Mathf.Max(0.05f, _poopFallTime);
                first = fall;
                duration = fall + 0.18f + Mathf.Max(0f, _poopHold) + 0.3f;
                break;
            }
        }

        return new SpellVfxTiming
        {
            Duration = duration,
            FirstImpact = first,
            SoundSpan = span >= 0f ? span : duration,
            FinaleSpan = finale
        };
    }

    private SpriteRenderer NewFlame(Color color, Vector3 position, float width, float length)
    {
        SpriteRenderer sr = NewSprite(FlameVisuals.Flame, color, position, 1f);
        sr.transform.localScale = new Vector3(width, length / 2f, 1f);
        return sr;
    }

    private IEnumerator Fireball(Vector3 from, Vector3 to, System.Action impact, float scale = 1f)
    {
        Vector3 dir = (to - from).normalized;
        float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion tail = Quaternion.Euler(0f, 0f, angle);

        SpriteRenderer glow = NewSprite(CircleSprite, new Color(1f, 0.3f, 0.05f, 0.35f), from, 1.0f * scale);
        SpriteRenderer outer = NewFlame(FlameVisuals.Outer(0.95f), from, 0.8f * scale, 1.7f * scale);
        SpriteRenderer mid = NewFlame(FlameVisuals.Mid(0.95f), from, 0.58f * scale, 1.3f * scale);
        SpriteRenderer core = NewFlame(FlameVisuals.Core(1f), from, 0.34f * scale, 0.85f * scale);
        outer.transform.rotation = tail;
        mid.transform.rotation = tail;
        core.transform.rotation = tail;

        for (float t = 0f; t < _fireballTravelTime; t += Time.deltaTime)
        {
            Vector3 p = Vector3.Lerp(from, to, t / _fireballTravelTime);
            float flicker = 1f + 0.14f * Mathf.Sin(t * 45f);
            float wobble = 6f * Mathf.Sin(t * 30f);

            glow.transform.position = p;
            outer.transform.position = p;
            mid.transform.position = p;
            core.transform.position = p;
            outer.transform.rotation = Quaternion.Euler(0f, 0f, angle + wobble);
            mid.transform.rotation = Quaternion.Euler(0f, 0f, angle - wobble);
            outer.transform.localScale = new Vector3(0.8f * scale, 0.85f * scale * flicker, 1f);
            mid.transform.localScale = new Vector3(0.58f * scale, 0.65f * scale * flicker, 1f);
            StartCoroutine(Ember(p, dir));
            yield return null;
        }

        Destroy(glow.gameObject);
        Destroy(outer.gameObject);
        Destroy(mid.gameObject);
        Destroy(core.gameObject);
        impact();
        yield return FlameBurst(to);
    }

    private IEnumerator Ember(Vector3 position)
    {
        return Ember(position, Vector3.down);
    }

    private IEnumerator Ember(Vector3 position, Vector3 travel)
    {
        Vector3 start = position - travel * 0.25f + (Vector3)(Random.insideUnitCircle * 0.12f);
        SpriteRenderer tongue = NewFlame(FlameVisuals.Outer(0.8f), start, 0.3f, 0.7f);
        const float life = 0.35f;
        float drift = Random.Range(-0.2f, 0.2f);

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            tongue.transform.position = start + new Vector3(drift * k, 0.55f * k, 0f);
            tongue.transform.localScale = new Vector3(0.3f * (1f - k), 0.35f * (1f - 0.6f * k), 1f);
            tongue.color = Color.Lerp(FlameVisuals.Mid(0.8f), FlameVisuals.Outer(0f), k);
            yield return null;
        }

        Destroy(tongue.gameObject);
    }

    private IEnumerator FlameBurst(Vector3 center)
    {
        const int count = 9;
        const float life = 0.5f;
        var parts = new SpriteRenderer[count];
        var angles = new float[count];
        var reach = new float[count];

        for (int i = 0; i < count; i++)
        {
            angles[i] = (i + Random.value * 0.5f) / count * 360f;
            reach[i] = Random.Range(0.6f, 1.2f);
            parts[i] = NewFlame(i % 2 == 0 ? FlameVisuals.Outer(0.95f) : FlameVisuals.Mid(0.95f), center, 0.5f, 1f);
        }
        SpriteRenderer flash = NewSprite(CircleSprite, new Color(1f, 0.85f, 0.4f, 0.8f), center, 0.5f);

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            float ease = 1f - (1f - k) * (1f - k);
            for (int i = 0; i < count; i++)
            {
                float rad = (angles[i] + 90f) * Mathf.Deg2Rad;
                Vector3 outward = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
                parts[i].transform.position = center + outward * (reach[i] * ease * 0.6f);
                parts[i].transform.rotation = Quaternion.Euler(0f, 0f, angles[i]);
                parts[i].transform.localScale = new Vector3(0.5f * (1f - k), 0.6f * reach[i] * (1f - 0.4f * k), 1f);
                Color c = parts[i].color;
                c.a = 0.95f * (1f - k);
                parts[i].color = c;
            }
            flash.transform.localScale = Vector3.one * (0.5f + 1.4f * ease);
            flash.color = new Color(1f, 0.8f, 0.35f, 0.8f * (1f - k));
            yield return null;
        }

        foreach (SpriteRenderer sr in parts) Destroy(sr.gameObject);
        Destroy(flash.gameObject);
    }

    private IEnumerator Thunder(Vector3 target, System.Action impact)
    {
        var parts = new List<SpriteRenderer>();
        Vector3 top = target + Vector3.up * _thunderHeight;
        const int segments = 7;

        Vector3 previous = top;
        for (int i = 1; i <= segments; i++)
        {
            Vector3 next = Vector3.Lerp(top, target, (float)i / segments);
            if (i < segments) next.x += Random.Range(-0.4f, 0.4f) * _style.Scale;
            parts.Add(Segment(previous, next, 0.22f * _style.Scale, new Color(1f, 0.9f, 0.3f, 0.6f)));
            parts.Add(Segment(previous, next, 0.08f * _style.Scale, Color.white));
            previous = next;
        }
        parts.Add(NewSprite(CircleSprite, new Color(1f, 0.95f, 0.6f, 0.6f), target, 2f * _style.Scale));

        impact();

        for (int blink = 0; blink < 3; blink++)
        {
            SetVisible(parts, true);
            yield return new WaitForSeconds(0.07f);
            SetVisible(parts, false);
            yield return new WaitForSeconds(0.04f);
        }

        SetVisible(parts, true);
        yield return new WaitForSeconds(0.1f);

        foreach (SpriteRenderer sr in parts) Destroy(sr.gameObject);
        yield return Burst(target, new Color(1f, 0.95f, 0.4f, 1f), 8, 0.7f, 0.25f, 0.2f);
    }

    private IEnumerator Heal(Vector3 center, System.Action impact)
    {
        SpriteRenderer glow = NewSprite(CircleSprite, new Color(0.4f, 1f, 0.55f, 0f), center, 1.8f);

        const int count = 14;
        var sparks = new SpriteRenderer[count];
        var starts = new Vector3[count];
        var speeds = new float[count];
        for (int i = 0; i < count; i++)
        {
            starts[i] = center + (Vector3)(Random.insideUnitCircle * 0.7f) + Vector3.down * 0.3f;
            speeds[i] = Random.Range(0.8f, 1.6f);
            sparks[i] = NewSprite(CircleSprite, new Color(0.7f, 1f, 0.75f, 0f), starts[i], Random.Range(0.12f, 0.22f));
        }

        bool landed = false;
        for (float t = 0f; t < _healTime; t += Time.deltaTime)
        {
            float k = t / _healTime;
            if (!landed && k >= 0.4f)
            {
                landed = true;
                impact();
            }
            float fade = Mathf.Sin(k * Mathf.PI);
            glow.color = new Color(0.4f, 1f, 0.55f, 0.45f * fade);
            for (int i = 0; i < count; i++)
            {
                sparks[i].transform.position = starts[i] + Vector3.up * (speeds[i] * k);
                sparks[i].color = new Color(0.7f, 1f, 0.75f, fade);
            }
            yield return null;
        }

        Destroy(glow.gameObject);
        foreach (SpriteRenderer sr in sparks) Destroy(sr.gameObject);
    }

    private IEnumerator Spark(Vector3 from, Vector3 to, System.Action impact)
    {
        var color = new Color(1f, 0.95f, 0.4f, 1f);
        SpriteRenderer spark = NewSprite(CircleSprite, color, from, 0.25f);

        for (float t = 0f; t < _sparkTravelTime; t += Time.deltaTime)
        {
            spark.transform.position = Vector3.Lerp(from, to, t / _sparkTravelTime);
            yield return null;
        }

        Destroy(spark.gameObject);
        impact();
        yield return Burst(to, color, 6, 0.45f, 0.2f, 0.15f);
    }

    private IEnumerator DarkAura(Vector3 center, System.Action impact)
    {
        SpriteRenderer rim = NewSprite(CircleSprite, new Color(0.5f, 0.1f, 0.7f, 0f), center, 1f);
        SpriteRenderer core = NewSprite(CircleSprite, new Color(0.08f, 0f, 0.15f, 0f), center, 1f);

        const int count = 10;
        var wisps = new SpriteRenderer[count];
        var angles = new float[count];
        for (int i = 0; i < count; i++)
        {
            angles[i] = (Mathf.PI * 2f / count) * i;
            wisps[i] = NewSprite(CircleSprite, new Color(0.35f, 0.05f, 0.5f, 0f), center, 0.25f);
        }

        bool landed = false;
        for (float t = 0f; t < _auraTime; t += Time.deltaTime)
        {
            float k = t / _auraTime;
            if (!landed && k >= 0.5f)
            {
                landed = true;
                impact();
            }
            float fade = Mathf.Sin(k * Mathf.PI);
            float size = Mathf.Lerp(0.8f, 2.6f, k) * (1f + 0.08f * Mathf.Sin(t * 25f));

            rim.transform.localScale = Vector3.one * (size * 1.25f);
            rim.color = new Color(0.5f, 0.1f, 0.7f, 0.5f * fade);
            core.transform.localScale = Vector3.one * size;
            core.color = new Color(0.08f, 0f, 0.15f, 0.8f * fade);

            for (int i = 0; i < count; i++)
            {
                float a = angles[i] + k * 2f;
                float radius = Mathf.Lerp(1.8f, 0.2f, k);
                wisps[i].transform.position = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                wisps[i].color = new Color(0.35f, 0.05f, 0.5f, fade);
            }
            yield return null;
        }

        Destroy(rim.gameObject);
        Destroy(core.gameObject);
        foreach (SpriteRenderer sr in wisps) Destroy(sr.gameObject);
    }

    private IEnumerator Meteors(System.Action hit, int waves)
    {
        Camera cam = Camera.main;
        float halfH = cam != null && cam.orthographic ? cam.orthographicSize : 5.4f;
        float halfW = cam != null ? halfH * cam.aspect : halfH * 16f / 9f;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        origin.z = 0f;

        waves = Mathf.Max(1, waves);
        for (int w = 0; w < waves; w++)
        {
            yield return MeteorWave(origin, halfW, halfH, cam, hit);
            if (w < waves - 1) yield return new WaitForSeconds(_meteorWavePause);
        }
    }

    private IEnumerator MeteorWave(Vector3 origin, float halfW, float halfH, Camera cam, System.Action hit)
    {
        int count = Mathf.Max(1, _meteorsPerWave);
        var rocks = new SpriteRenderer[count];
        var glows = new SpriteRenderer[count];
        var starts = new Vector3[count];
        var ends = new Vector3[count];
        var delays = new float[count];
        var landed = new bool[count];
        const float maxDelay = 0.4f;

        for (int i = 0; i < count; i++)
        {
            ends[i] = origin + new Vector3(Random.Range(-halfW, halfW), Random.Range(-halfH * 0.9f, halfH * 0.5f), 0f);
            starts[i] = ends[i] + new Vector3(4f, 8f, 0f) * Random.Range(0.9f, 1.15f);
            delays[i] = Random.Range(0f, maxDelay);
            float size = Random.Range(0.35f, 0.8f) * _style.Scale;
            glows[i] = NewSprite(CircleSprite, new Color(1f, 0.3f, 0.05f, 0f), starts[i], size * 2.2f);
            rocks[i] = NewSprite(MeteorSprite(), new Color(0.45f, 0.25f, 0.15f, 0f), starts[i], size);
            rocks[i].sortingOrder = _sortingOrder + 1;
        }

        float total = maxDelay + _meteorFallTime;
        float hitTime = _meteorFallTime + 0.05f;
        bool hitDone = false;
        float shake = 0f;
        Vector3 camHome = cam != null ? cam.transform.position : Vector3.zero;

        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (landed[i]) continue;
                float k = (t - delays[i]) / _meteorFallTime;
                if (k < 0f) continue;

                if (k >= 1f)
                {
                    landed[i] = true;
                    rocks[i].enabled = false;
                    glows[i].enabled = false;
                    StartCoroutine(MeteorLanding(ends[i]));
                    continue;
                }

                Vector3 p = Vector3.Lerp(starts[i], ends[i], k);
                rocks[i].transform.position = p;
                glows[i].transform.position = p;
                rocks[i].color = RockColor(i);
                glows[i].color = RockGlow(i);
                if ((Time.frameCount + i) % 3 == 0) StartCoroutine(MeteorTrail(p, i));
            }

            if (!hitDone && t >= hitTime)
            {
                hitDone = true;
                shake = 0.35f;
                hit();
            }

            if (cam != null)
            {
                shake = Mathf.MoveTowards(shake, 0f, Time.deltaTime * 0.8f);
                cam.transform.position = camHome + (Vector3)(Random.insideUnitCircle * shake);
            }
            yield return null;
        }

        if (!hitDone) hit();
        if (cam != null) cam.transform.position = camHome;
        foreach (SpriteRenderer sr in rocks) Destroy(sr.gameObject);
        foreach (SpriteRenderer sr in glows) Destroy(sr.gameObject);
    }

    private IEnumerator Explosion(Vector3 center, System.Action impact)
    {
        Camera cam = Camera.main;
        Vector3 camHome = cam != null ? cam.transform.position : Vector3.zero;

        SpriteRenderer flash = NewSprite(CircleSprite, new Color(1f, 0.85f, 0.5f, 0f), center, 1f);
        flash.sortingOrder = _sortingOrder + 5;
        SpriteRenderer core = NewSprite(CircleSprite, new Color(1f, 1f, 1f, 0f), center, 1f);
        core.sortingOrder = _sortingOrder + 6;

        bool landed = false;
        for (float t = 0f; t < _explosionTime; t += Time.deltaTime)
        {
            float k = t / _explosionTime;
            if (!landed && k >= 0.35f)
            {
                landed = true;
                impact();
                StartCoroutine(Burst(center, new Color(1f, 0.55f, 0.1f, 1f), 28, 4f, 0.7f, 0.5f));
                StartCoroutine(Burst(center, new Color(0.35f, 0.3f, 0.3f, 1f), 16, 3f, 0.9f, 0.4f));
            }

            float fade = Mathf.Sin(k * Mathf.PI);
            flash.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 32f, k);
            flash.color = new Color(1f, 0.85f, 0.5f, 0.95f * fade);
            core.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 20f, k);
            core.color = new Color(1f, 1f, 1f, fade);

            if (cam != null)
            {
                float shake = landed ? 0.5f * (1f - k) : 0f;
                cam.transform.position = camHome + (Vector3)(Random.insideUnitCircle * shake);
            }
            yield return null;
        }

        if (cam != null) cam.transform.position = camHome;
        Destroy(flash.gameObject);
        Destroy(core.gameObject);
    }

    private IEnumerator Mirror(Vector3 center, System.Action impact)
    {
        SpriteRenderer rim = NewSprite(CircleSprite, new Color(1f, 1f, 1f, 0f), center, 1f);
        rim.sortingOrder = _sortingOrder - 1;
        SpriteRenderer glass = NewSprite(CircleSprite, new Color(0.75f, 0.92f, 1f, 0f), center, 1f);

        const int count = 8;
        var sparkles = new SpriteRenderer[count];
        for (int i = 0; i < count; i++)
            sparkles[i] = NewSprite(CircleSprite, new Color(1f, 1f, 1f, 0f), center, 0.2f);

        bool landed = false;
        for (float t = 0f; t < _mirrorTime; t += Time.deltaTime)
        {
            float k = t / _mirrorTime;
            if (!landed && k >= 0.4f)
            {
                landed = true;
                impact();
            }

            float fade = Mathf.Sin(k * Mathf.PI);
            float size = Mathf.Lerp(1.2f, 2.6f, Mathf.Min(1f, k * 2f));

            rim.transform.localScale = Vector3.one * (size * 1.2f);
            rim.color = new Color(1f, 1f, 1f, 0.25f * fade);
            glass.transform.localScale = Vector3.one * size;
            glass.color = new Color(0.75f, 0.92f, 1f, 0.4f * fade);

            for (int i = 0; i < count; i++)
            {
                float a = (Mathf.PI * 2f / count) * i + k * 3f;
                sparkles[i].transform.position = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (size * 0.5f);
                sparkles[i].transform.localScale = Vector3.one * (0.2f * (0.6f + 0.4f * Mathf.Sin(t * 20f + i)));
                sparkles[i].color = new Color(1f, 1f, 1f, fade);
            }
            yield return null;
        }

        Destroy(rim.gameObject);
        Destroy(glass.gameObject);
        foreach (SpriteRenderer sr in sparkles) Destroy(sr.gameObject);
    }

    private static readonly Color[] SlotColors =
    {
        new Color(1f, 0.3f, 0.35f), new Color(1f, 0.85f, 0.25f), new Color(0.35f, 0.65f, 1f),
        new Color(0.4f, 0.9f, 0.45f), new Color(0.75f, 0.45f, 1f)
    };

    private static Sprite SlotShape(int symbol)
    {
        switch (symbol % SlotMachineEffect.SymbolCount)
        {
            case 0:
            case 3: return HeartSprite;
            case 2: return SquareSprite;
            default: return CircleSprite;
        }
    }

    private IEnumerator Slots(int[] reels, System.Action impact, System.Action<SpellCue, int> cue, Vector3 casterPos, Vector3 enemyPos)
    {
        Camera cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        origin.z = 0f;
        origin.y += 1.2f;

        if (reels == null || reels.Length < 3) reels = new[] { 0, 1, 2 };
        bool jackpot = reels[0] == reels[1] && reels[1] == reels[2];

        const int count = 3;
        const float spacing = 2.4f;
        var frames = new SpriteRenderer[count];
        var faces = new SpriteRenderer[count];
        var symbols = new SpriteRenderer[count];
        var shown = new int[count];
        var stopTimes = new float[count];
        var locked = new bool[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 start = origin + new Vector3((i - 1) * spacing, 0f, 0f);
            frames[i] = NewSprite(SquareSprite, Color.white, start, 2.1f);
            frames[i].sortingOrder = _sortingOrder + 10;
            faces[i] = NewSprite(SquareSprite, Color.white, start, 1.8f);
            faces[i].sortingOrder = _sortingOrder + 11;
            symbols[i] = NewSprite(CircleSprite, Color.white, start, 1.1f);
            symbols[i].sortingOrder = _sortingOrder + 12;
            shown[i] = Random.Range(0, SlotMachineEffect.SymbolCount);
            stopTimes[i] = _slotsSpinTime + i * _slotsStagger;
        }

        float landTime = stopTimes[count - 1] + 0.2f;
        float total = landTime + 0.7f;
        bool landed = false;
        int lastTick = -1;
        var gold = new Color(0.9f, 0.72f, 0.2f);
        Sprite jackpotSprite = JackpotSprite();

        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            float a = Mathf.Min(1f, t / 0.2f) * Mathf.Clamp01((total - t) / 0.3f);
            int tick = Mathf.FloorToInt(t / 0.07f);
            bool newTick = tick != lastTick;
            lastTick = tick;

            Color frameColor = gold;
            if (landed)
            {
                frameColor = jackpot
                    ? Color.Lerp(gold, Color.white, 0.5f + 0.5f * Mathf.Sin(t * 30f))
                    : new Color(0.45f, 0.45f, 0.5f);
            }
            frameColor.a = a;

            for (int i = 0; i < count; i++)
            {
                if (!locked[i] && t >= stopTimes[i])
                {
                    locked[i] = true;
                    shown[i] = reels[i];
                    if (cue != null)
                    {
                        cue(SpellCue.ReelStop, i);
                        if (jackpot && i == count - 1) cue(SpellCue.Jackpot, 0);
                    }
                }
                else if (!locked[i] && newTick)
                {
                    shown[i] = Random.Range(0, SlotMachineEffect.SymbolCount);
                }

                float pop = locked[i] ? Mathf.Clamp01(1f - (t - stopTimes[i]) / 0.2f) : 0f;
                float wobble = locked[i] ? 0f : Random.Range(-0.04f, 0.04f);
                Vector3 p = origin + new Vector3((i - 1) * spacing, wobble, 0f);

                frames[i].transform.position = p;
                frames[i].color = frameColor;
                faces[i].transform.position = p;
                faces[i].color = new Color(0.08f, 0.06f, 0.12f, a);

                symbols[i].transform.position = p;
                bool swapped = landed && jackpot && jackpotSprite != null;
                symbols[i].sprite = swapped ? jackpotSprite : SlotSprite(shown[i]);
                Color c = swapped ? JackpotSymbolColor() : SlotColor(shown[i]);
                c.a = a;
                symbols[i].color = c;
                symbols[i].transform.localScale = Vector3.one * (1.1f * (1f + 0.35f * pop));
            }

            if (!landed && t >= landTime)
            {
                landed = true;
                impact();
                if (jackpot) StartCoroutine(Burst(origin, new Color(1f, 0.85f, 0.25f, 1f), 30, 5f, 0.8f, 0.3f));
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in frames) Destroy(sr.gameObject);
        foreach (SpriteRenderer sr in faces) Destroy(sr.gameObject);
        foreach (SpriteRenderer sr in symbols) Destroy(sr.gameObject);

        if (jackpot && _style.Jackpot != JackpotFx.None) yield return Jackpot(_style.Jackpot, casterPos, enemyPos);
    }

    private IEnumerator Hearts(System.Action impact)
    {
        Camera cam = Camera.main;
        float halfH = cam != null && cam.orthographic ? cam.orthographicSize : 5.4f;
        float halfW = cam != null ? halfH * cam.aspect : halfH * 16f / 9f;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        origin.z = 0f;

        int count = Mathf.Max(1, _heartCount);
        Color[] palette =
        {
            new Color(1f, 0.35f, 0.55f), new Color(0.95f, 0.15f, 0.3f),
            new Color(1f, 0.65f, 0.8f), new Color(0.85f, 0.2f, 0.6f)
        };

        var hearts = new SpriteRenderer[count];
        var starts = new Vector3[count];
        var sizes = new float[count];
        var delays = new float[count];
        var phases = new float[count];
        var tints = new Color[count];
        float life = _heartsTime * 0.55f;

        for (int i = 0; i < count; i++)
        {
            sizes[i] = Random.Range(0.3f, 2.4f);
            starts[i] = origin + new Vector3(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH), 0f);
            delays[i] = Random.Range(0f, _heartsTime - life);
            phases[i] = Random.value * Mathf.PI * 2f;
            tints[i] = palette[Random.Range(0, palette.Length)];
            hearts[i] = NewSprite(HeartSprite, new Color(tints[i].r, tints[i].g, tints[i].b, 0f), starts[i], sizes[i]);
            hearts[i].sortingOrder = _sortingOrder + Mathf.RoundToInt((2.4f - sizes[i]) * 10f);
        }

        bool landed = false;
        for (float t = 0f; t < _heartsTime; t += Time.deltaTime)
        {
            if (!landed && t >= _heartsTime * 0.3f)
            {
                landed = true;
                impact();
            }

            for (int i = 0; i < count; i++)
            {
                float hk = Mathf.Clamp01((t - delays[i]) / life);
                float pop = Mathf.Min(1f, hk * 6f);
                float pulse = 1f + 0.1f * Mathf.Sin(t * 10f + phases[i]);
                hearts[i].transform.localScale = Vector3.one * (sizes[i] * pop * pulse);
                hearts[i].transform.position = starts[i]
                    + new Vector3(Mathf.Sin(t * 2f + phases[i]) * 0.2f, hk * 1.4f, 0f);

                float a = (hk <= 0f || hk >= 1f) ? 0f : Mathf.Min(1f, (1f - hk) * 3f) * 0.9f;
                hearts[i].color = new Color(tints[i].r, tints[i].g, tints[i].b, a);
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in hearts) Destroy(sr.gameObject);
    }

    private IEnumerator Splash(Vector3 center, System.Action impact)
    {
        const int count = 14;
        var drops = new SpriteRenderer[count];
        var starts = new Vector3[count];
        var ends = new Vector3[count];
        var delays = new float[count];
        for (int i = 0; i < count; i++)
        {
            starts[i] = center + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(2.2f, 3f), 0f);
            ends[i] = center + (Vector3)(Random.insideUnitCircle * 0.6f);
            delays[i] = Random.Range(0f, 0.3f);
            drops[i] = NewSprite(CircleSprite, new Color(0.3f, 0.65f, 1f, 0f), starts[i], Random.Range(0.14f, 0.24f));
        }
        SpriteRenderer pool = NewSprite(CircleSprite, new Color(0.3f, 0.6f, 1f, 0f), center, 1.8f);

        float fall = _splashTime * 0.6f;
        bool landed = false;
        for (float t = 0f; t < _splashTime; t += Time.deltaTime)
        {
            float k = t / _splashTime;
            if (!landed && k >= 0.5f)
            {
                landed = true;
                impact();
            }

            for (int i = 0; i < count; i++)
            {
                float dk = Mathf.Clamp01((t - delays[i]) / fall);
                drops[i].transform.position = Vector3.Lerp(starts[i], ends[i], dk);
                float a = (dk <= 0f || dk >= 1f) ? 0f : 0.9f;
                drops[i].color = new Color(0.3f, 0.65f, 1f, a);
            }
            pool.color = new Color(0.3f, 0.6f, 1f, 0.4f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }

        Destroy(pool.gameObject);
        foreach (SpriteRenderer sr in drops) Destroy(sr.gameObject);
        yield return Burst(center, new Color(0.35f, 0.7f, 1f, 1f), 8, 0.6f, 0.25f, 0.15f);
    }

    private IEnumerator Frost(Vector3 center, System.Action impact)
    {
        SpriteRenderer glow = NewSprite(CircleSprite, new Color(0.6f, 0.9f, 1f, 0f), center, 1f);

        const int count = 8;
        var shards = new SpriteRenderer[count];
        var dirs = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float a = (Mathf.PI * 2f / count) * i;
            dirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
            shards[i] = NewSprite(SquareSprite, new Color(0.75f, 0.95f, 1f, 0f), center, 1f);
            shards[i].transform.localScale = new Vector3(0.45f, 0.12f, 1f);
            shards[i].transform.rotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg);
        }

        bool landed = false;
        for (float t = 0f; t < _frostTime; t += Time.deltaTime)
        {
            float k = t / _frostTime;
            if (!landed && k >= 0.45f)
            {
                landed = true;
                impact();
            }

            float fade = Mathf.Sin(k * Mathf.PI);
            glow.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 2.4f, k);
            glow.color = new Color(0.6f, 0.9f, 1f, 0.5f * fade);

            for (int i = 0; i < count; i++)
            {
                shards[i].transform.position = center + dirs[i] * Mathf.Lerp(0.2f, 1.1f, k);
                shards[i].color = new Color(0.75f, 0.95f, 1f, fade);
            }
            yield return null;
        }

        Destroy(glow.gameObject);
        foreach (SpriteRenderer sr in shards) Destroy(sr.gameObject);
        yield return Burst(center, new Color(0.8f, 0.97f, 1f, 1f), 8, 0.7f, 0.25f, 0.14f);
    }

    private static readonly Color[] RainbowColors =
    {
        new Color(1f, 0.15f, 0.15f), new Color(1f, 0.55f, 0.1f), new Color(1f, 0.95f, 0.2f),
        new Color(0.25f, 0.9f, 0.3f), new Color(0.25f, 0.55f, 1f), new Color(0.3f, 0.25f, 0.85f),
        new Color(0.7f, 0.3f, 0.9f)
    };

    private IEnumerator Rainbow(Vector3 from, Vector3 to, System.Action impact)
    {
        int bands = RainbowColors.Length;
        const int pieces = 22;
        const float bandWidth = 0.13f;

        Vector3 control = (from + to) * 0.5f + Vector3.up * (Vector3.Distance(from, to) * 0.35f);

        var strips = new SpriteRenderer[bands * pieces];
        for (int b = 0; b < bands; b++)
        {
            float offset = ((bands - 1) * 0.5f - b) * bandWidth;
            for (int p = 0; p < pieces; p++)
            {
                Vector3 a = RainbowPoint(from, control, to, (float)p / pieces, offset);
                Vector3 c = RainbowPoint(from, control, to, (float)(p + 1) / pieces, offset);
                Color hidden = new Color(RainbowColors[b].r, RainbowColors[b].g, RainbowColors[b].b, 0f);
                SpriteRenderer sr = NewSprite(SquareSprite, hidden, a, 1f);
                PlaceLine(sr, a, c, bandWidth * 1.1f);
                strips[b * pieces + p] = sr;
            }
        }

        float travel = Mathf.Max(0.05f, _rainbowTravelTime);
        int shown = 0;
        for (float t = 0f; t < travel; t += Time.deltaTime)
        {
            int upTo = Mathf.Min(pieces, Mathf.CeilToInt(t / travel * pieces));
            ShowRainbowPieces(strips, pieces, shown, upTo, 0.95f);
            shown = upTo;
            yield return null;
        }
        ShowRainbowPieces(strips, pieces, shown, pieces, 0.95f);

        impact();
        for (int b = 0; b < bands; b++)
            StartCoroutine(Burst(to, RainbowColors[b], 4, 0.9f, 0.35f, 0.12f));

        float hold = Mathf.Max(0f, _rainbowHold);
        for (float t = 0f; t < hold; t += Time.deltaTime)
        {
            ShowRainbowPieces(strips, pieces, 0, pieces, 0.8f + 0.15f * Mathf.Sin(t * 30f));
            yield return null;
        }

        const float fadeTime = 0.3f;
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            ShowRainbowPieces(strips, pieces, 0, pieces, 0.9f * (1f - t / fadeTime));
            yield return null;
        }

        foreach (SpriteRenderer sr in strips) Destroy(sr.gameObject);
    }

    private static Vector3 RainbowPoint(Vector3 from, Vector3 control, Vector3 to, float s, float offset)
    {
        float u = 1f - s;
        Vector3 position = u * u * from + 2f * u * s * control + s * s * to;
        Vector3 direction = 2f * u * (control - from) + 2f * s * (to - control);
        Vector3 normal = new Vector3(-direction.y, direction.x, 0f).normalized;
        if (normal.y < 0f) normal = -normal;
        return position + normal * offset;
    }

    private static void ShowRainbowPieces(SpriteRenderer[] strips, int pieces, int fromPiece, int toPiece, float alpha)
    {
        for (int i = 0; i < strips.Length; i++)
        {
            int piece = i % pieces;
            if (piece < fromPiece || piece >= toPiece) continue;
            Color band = RainbowColors[i / pieces];
            strips[i].color = new Color(band.r, band.g, band.b, alpha);
        }
    }

    private IEnumerator Poison(Vector3 center, System.Action impact)
    {
        const int count = 18;
        float time = Mathf.Max(0.2f, _poisonTime);
        float life = time * 0.65f;
        float maxDelay = time - life;

        SpriteRenderer glow = NewSprite(CircleSprite, new Color(0.55f, 0.15f, 0.85f, 0f), center, 1f);
        glow.sortingOrder = _sortingOrder - 1;

        var bubbles = new SpriteRenderer[count];
        var xs = new float[count];
        var delays = new float[count];
        var wobbles = new float[count];
        for (int i = 0; i < count; i++)
        {
            xs[i] = Random.Range(-0.7f, 0.7f);
            delays[i] = Random.Range(0f, maxDelay);
            wobbles[i] = Random.Range(0f, Mathf.PI * 2f);
            bubbles[i] = NewSprite(CircleSprite, new Color(0.75f, 0.35f, 1f, 0f),
                center + new Vector3(xs[i], -0.6f, 0f), Random.Range(0.15f, 0.35f));
        }

        bool landed = false;
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            if (!landed && t >= time * 0.3f)
            {
                landed = true;
                impact();
            }

            glow.transform.localScale = Vector3.one * (1.6f + 0.3f * Mathf.Sin(t * 14f));
            glow.color = new Color(0.55f, 0.15f, 0.85f, 0.35f * Mathf.Sin(t / time * Mathf.PI));

            for (int i = 0; i < count; i++)
            {
                float k = (t - delays[i]) / life;
                if (k < 0f) continue;
                if (k > 1f)
                {
                    bubbles[i].enabled = false;
                    continue;
                }

                bubbles[i].transform.position = center
                    + new Vector3(xs[i] + Mathf.Sin(k * 6f + wobbles[i]) * 0.15f, -0.6f + 1.6f * k, 0f);
                bubbles[i].color = new Color(0.75f, 0.35f, 1f, 0.8f * Mathf.Sin(k * Mathf.PI));
            }
            yield return null;
        }

        Destroy(glow.gameObject);
        foreach (SpriteRenderer sr in bubbles) Destroy(sr.gameObject);
    }

    private IEnumerator Poop(Vector3 center, System.Action impact)
    {
        float scale = 1.2f * _style.Scale;
        Vector3 top = center + Vector3.up * 4f;
        var brown = new Color(0.42f, 0.26f, 0.12f, 1f);
        var lightBrown = new Color(0.52f, 0.33f, 0.16f, 1f);

        Transform root = new GameObject("SpellVfx").transform;
        root.position = top;
        root.localScale = Vector3.one * scale;

        var parts = new List<SpriteRenderer>();
        void Piece(Color color, Vector2 offset, Vector2 size, int order)
        {
            SpriteRenderer sr = NewSprite(CircleSprite, color, top + (Vector3)(offset * scale), 1f);
            sr.transform.localScale = new Vector3(size.x * scale, size.y * scale, 1f);
            sr.sortingOrder = _sortingOrder + order;
            sr.transform.SetParent(root, true);
            parts.Add(sr);
        }

        Piece(brown, new Vector2(0f, 0f), new Vector2(1.1f, 0.55f), 0);
        Piece(lightBrown, new Vector2(0f, 0.36f), new Vector2(0.8f, 0.5f), 1);
        Piece(brown, new Vector2(0f, 0.68f), new Vector2(0.5f, 0.4f), 2);
        Piece(lightBrown, new Vector2(0.05f, 0.92f), new Vector2(0.22f, 0.28f), 3);
        Piece(Color.white, new Vector2(-0.15f, 0.38f), new Vector2(0.2f, 0.2f), 4);
        Piece(Color.white, new Vector2(0.15f, 0.38f), new Vector2(0.2f, 0.2f), 4);
        Piece(Color.black, new Vector2(-0.14f, 0.36f), new Vector2(0.09f, 0.09f), 5);
        Piece(Color.black, new Vector2(0.16f, 0.36f), new Vector2(0.09f, 0.09f), 5);

        var baseColors = new Color[parts.Count];
        for (int i = 0; i < parts.Count; i++) baseColors[i] = parts[i].color;

        float fall = Mathf.Max(0.05f, _poopFallTime);
        for (float t = 0f; t < fall; t += Time.deltaTime)
        {
            float k = t / fall;
            root.position = Vector3.Lerp(top, center, k * k);
            root.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * 9f) * 12f * (1f - k));
            yield return null;
        }

        root.position = center;
        root.rotation = Quaternion.identity;
        impact();
        StartCoroutine(Burst(center, brown, 14, 1.1f, 0.4f, 0.22f));

        float hold = Mathf.Max(0f, _poopHold);
        for (int i = 0; i < 4; i++)
            StartCoroutine(StinkWisp(center + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0f), i * hold / 4f));

        const float squash = 0.18f;
        for (float t = 0f; t < squash; t += Time.deltaTime)
        {
            float k = t / squash;
            root.localScale = new Vector3(Mathf.Lerp(scale * 1.35f, scale, k), Mathf.Lerp(scale * 0.6f, scale, k), 1f);
            yield return null;
        }
        root.localScale = Vector3.one * scale;

        for (float t = 0f; t < hold; t += Time.deltaTime)
        {
            root.localScale = Vector3.one * (scale * (1f + 0.03f * Mathf.Sin(t * 12f)));
            yield return null;
        }

        const float fade = 0.3f;
        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            float a = 1f - t / fade;
            for (int i = 0; i < parts.Count; i++)
                parts[i].color = new Color(baseColors[i].r, baseColors[i].g, baseColors[i].b, baseColors[i].a * a);
            yield return null;
        }

        Destroy(root.gameObject);
    }

    private IEnumerator StinkWisp(Vector3 start, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        var tint = new Color(0.5f, 0.85f, 0.25f, 1f);
        SpriteRenderer wisp = NewSprite(CircleSprite, new Color(tint.r, tint.g, tint.b, 0f), start, 0.3f);
        float phase = Random.value * Mathf.PI * 2f;
        const float life = 0.9f;

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            wisp.transform.position = start + new Vector3(Mathf.Sin(k * 8f + phase) * 0.15f, 1.2f * k, 0f);
            wisp.transform.localScale = Vector3.one * (0.3f + 0.3f * k);
            wisp.color = new Color(tint.r, tint.g, tint.b, 0.5f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }

        Destroy(wisp.gameObject);
    }

    private IEnumerator Git(Vector3 from, Vector3 to, int[] bits, System.Action hit)
    {
        if (bits == null || bits.Length == 0) bits = new[] { 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0 };

        var bright = new Color(0.35f, 1f, 0.45f, 1f);
        var dim = new Color(0.2f, 0.6f, 0.3f, 1f);

        int count = bits.Length;
        var digits = new TextMeshPro[count];
        var starts = new Vector3[count];
        var ends = new Vector3[count];
        var delays = new float[count];
        var arcs = new float[count];
        var spins = new float[count];
        var trailTimers = new float[count];
        var streaks = new int[count];
        var launched = new bool[count];
        var done = new bool[count];

        float travel = Mathf.Max(0.05f, _gitTravelTime);
        float gap = Mathf.Max(0.01f, _gitDigitGap);

        int run = 0;
        for (int i = 0; i < count; i++)
        {
            bool one = bits[i] == 1;
            run = one ? run + 1 : 0;
            streaks[i] = run;

            starts[i] = from + (Vector3)(Random.insideUnitCircle * 0.3f);
            ends[i] = one
                ? to + (Vector3)(Random.insideUnitCircle * 0.5f)
                : to + new Vector3(Random.Range(-0.8f, 0.8f), (Random.value < 0.5f ? -1f : 1f) * Random.Range(1.3f, 1.9f), 0f);
            delays[i] = i * gap;
            arcs[i] = Random.Range(0.2f, 0.9f) * (Random.value < 0.5f ? -1f : 1f);
            spins[i] = Random.Range(-540f, 540f);
            digits[i] = NewDigit(one ? '1' : '0', starts[i], _gitDigitSize, new Color(bright.r, bright.g, bright.b, 0f));
        }

        Camera cam = Camera.main;
        Vector3 camHome = cam != null ? cam.transform.position : Vector3.zero;
        float shake = 0f;
        float trailInterval = Mathf.Max(0.01f, _gitTrailInterval);

        float stream = (count - 1) * gap + travel;
        StartCoroutine(GitCloud(from, to, stream));
        StartCoroutine(GitRain(cam, stream));

        float total = stream + 0.1f;
        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (done[i]) continue;
                float k = (t - delays[i]) / travel;
                if (k < 0f) continue;

                bool one = bits[i] == 1;

                if (!launched[i])
                {
                    launched[i] = true;
                    StartCoroutine(Burst(starts[i], bright, 3, 0.35f, 0.15f, 0.08f));
                }

                if (k >= 1f)
                {
                    done[i] = true;
                    Destroy(digits[i].gameObject);
                    if (one)
                    {
                        hit();
                        GitImpact(ends[i], streaks[i], bright);
                        shake = Mathf.Max(shake, Mathf.Min(0.1f + 0.06f * streaks[i], 0.45f));
                    }
                    continue;
                }

                Vector3 position = Vector3.Lerp(starts[i], ends[i], k)
                    + Vector3.up * (Mathf.Sin(k * Mathf.PI) * arcs[i]);
                digits[i].transform.position = position;

                digits[i].transform.localScale = Vector3.one * (one ? Mathf.Lerp(0.8f, 1.5f, k) : 1f);
                if (!one) digits[i].transform.rotation = Quaternion.Euler(0f, 0f, k * spins[i]);

                Color c = one ? Color.Lerp(bright, Color.white, Mathf.Clamp01((streaks[i] - 1) * 0.25f)) : dim;
                float appear = Mathf.Min(1f, k * 6f);
                c.a = one ? appear : appear * (1f - k * 0.8f);
                digits[i].color = c;

                trailTimers[i] -= Time.deltaTime;
                if (trailTimers[i] <= 0f)
                {
                    trailTimers[i] = trailInterval;
                    StartCoroutine(GitTrail(position, c, one ? 0.22f : 0.14f));
                }
            }

            if (cam != null)
            {
                shake = Mathf.MoveTowards(shake, 0f, Time.deltaTime * 1.5f);
                cam.transform.position = camHome + (Vector3)(Random.insideUnitCircle * shake);
            }
            yield return null;
        }

        if (cam != null) cam.transform.position = camHome;

        for (int i = 0; i < count; i++)
            if (!done[i]) Destroy(digits[i].gameObject);
    }

    private void GitImpact(Vector3 point, int streak, Color color)
    {
        int level = Mathf.Min(streak, 6);
        Color hot = Color.Lerp(color, Color.white, Mathf.Clamp01((streak - 1) * 0.25f));

        StartCoroutine(Burst(point, hot, 5 + 3 * level, 0.5f + 0.12f * level, 0.25f + 0.03f * level, 0.1f));
        StartCoroutine(GitFlash(point, hot, 0.8f + 0.25f * level));
        if (streak > 1) StartCoroutine(GitStreakLabel(point, streak, hot));
    }

    private IEnumerator GitFlash(Vector3 center, Color color, float size)
    {
        SpriteRenderer flash = NewSprite(CircleSprite, new Color(color.r, color.g, color.b, 0.7f), center, size * 0.3f);
        flash.sortingOrder = _sortingOrder + 4;
        const float life = 0.2f;

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            flash.transform.localScale = Vector3.one * Mathf.Lerp(size * 0.3f, size, k);
            flash.color = new Color(color.r, color.g, color.b, 0.7f * (1f - k));
            yield return null;
        }

        Destroy(flash.gameObject);
    }

    private IEnumerator GitStreakLabel(Vector3 point, int streak, Color color)
    {
        Vector3 start = point + Vector3.up * 0.6f;
        TextMeshPro label = NewWorldText($"x{streak}", start, _gitDigitSize * 0.8f, color);
        label.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder + 7;
        const float life = 0.6f;

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            label.transform.position = start + Vector3.up * (1.2f * k);
            label.transform.localScale = Vector3.one * Mathf.Lerp(1.5f, 1f, Mathf.Min(1f, k * 5f));
            Color c = color;
            c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            label.color = c;
            yield return null;
        }

        Destroy(label.gameObject);
    }

    private IEnumerator GitTrail(Vector3 position, Color color, float size)
    {
        SpriteRenderer dot = NewSprite(CircleSprite, new Color(color.r, color.g, color.b, 0.5f), position, size);
        const float life = 0.25f;

        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            dot.transform.localScale = Vector3.one * (size * (1f - k));
            dot.color = new Color(color.r, color.g, color.b, 0.5f * (1f - k));
            yield return null;
        }

        Destroy(dot.gameObject);
    }

    private IEnumerator GitRain(Camera cam, float duration)
    {
        int count = Mathf.Max(0, _gitRain);
        if (count == 0) yield break;

        float halfH = cam != null && cam.orthographic ? cam.orthographicSize : 5.4f;
        float halfW = cam != null ? halfH * cam.aspect : halfH * 16f / 9f;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        origin.z = 0f;

        float topY = origin.y + halfH + 0.5f;
        float distance = halfH * 2f + 1f;
        var digits = new TextMeshPro[count];
        var xs = new float[count];
        var falls = new float[count];
        var delays = new float[count];
        var tint = new Color(0.2f, 0.75f, 0.35f, 1f);
        float longest = 0f;

        for (int i = 0; i < count; i++)
        {
            xs[i] = origin.x + Random.Range(-halfW, halfW);
            falls[i] = distance / Random.Range(7f, 12f);
            delays[i] = Random.Range(0f, Mathf.Max(0.01f, duration));
            longest = Mathf.Max(longest, falls[i]);
            digits[i] = NewDigit(Random.value < 0.5f ? '0' : '1', new Vector3(xs[i], topY, 0f),
                _gitDigitSize * 0.6f, new Color(tint.r, tint.g, tint.b, 0f));
            digits[i].GetComponent<MeshRenderer>().sortingOrder = _sortingOrder - 5;
        }

        float total = duration + longest;
        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                float k = (t - delays[i]) / falls[i];
                if (k < 0f) continue;
                if (k > 1f)
                {
                    digits[i].gameObject.SetActive(false);
                    continue;
                }

                digits[i].transform.position = new Vector3(xs[i], topY - distance * k, 0f);
                digits[i].color = new Color(tint.r, tint.g, tint.b, 0.45f * Mathf.Min(1f, k * 10f) * (1f - k * 0.5f));
                if (Random.value < 0.03f) digits[i].text = Random.value < 0.5f ? "0" : "1";
            }
            yield return null;
        }

        for (int i = 0; i < count; i++) Destroy(digits[i].gameObject);
    }

    private IEnumerator GitCloud(Vector3 from, Vector3 to, float duration)
    {
        int count = Mathf.Max(0, _gitCloud);
        var digits = new TextMeshPro[count];
        var starts = new Vector3[count];
        var ends = new Vector3[count];
        var delays = new float[count];
        const float travel = 0.5f;
        var tint = new Color(0.2f, 0.6f, 0.3f, 1f);

        for (int i = 0; i < count; i++)
        {
            starts[i] = from + (Vector3)(Random.insideUnitCircle * 0.5f);
            ends[i] = to + (Vector3)(Random.insideUnitCircle * 1.2f);
            delays[i] = Random.Range(0f, Mathf.Max(0.01f, duration));
            digits[i] = NewDigit(Random.value < 0.5f ? '0' : '1', starts[i], _gitDigitSize * 0.5f,
                new Color(tint.r, tint.g, tint.b, 0f));
        }

        float total = duration + travel;
        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                float k = (t - delays[i]) / travel;
                if (k < 0f) continue;
                if (k > 1f)
                {
                    digits[i].gameObject.SetActive(false);
                    continue;
                }

                digits[i].transform.position = Vector3.Lerp(starts[i], ends[i], k);
                digits[i].color = new Color(tint.r, tint.g, tint.b, 0.55f * Mathf.Sin(k * Mathf.PI));
            }
            yield return null;
        }

        for (int i = 0; i < count; i++) Destroy(digits[i].gameObject);
    }

    private TextMeshPro NewDigit(char digit, Vector3 position, float fontSize, Color color)
    {
        return NewWorldText(digit.ToString(), position, fontSize, color);
    }

    private TextMeshPro NewWorldText(string text, Vector3 position, float fontSize, Color color)
    {
        var go = new GameObject("SpellVfx");
        go.transform.position = position;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(4f, 4f);

        go.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder + 5;
        if (_tintOn) _tintedTexts.Add(tmp);
        return tmp;
    }

    private IEnumerator Salt(Combatant target, System.Action impact)
    {
        Vector3 center = target.VfxPoint.position;
        float w = 1.8f, h = 2f;
        SpriteRenderer body = target.ScarParent.GetComponent<SpriteRenderer>();
        if (body != null)
        {
            Bounds bounds = body.bounds;
            center = bounds.center;
            w = bounds.size.x;
            h = bounds.size.y;
        }

        int count = Mathf.Max(1, _saltGrains);
        var grains = new SpriteRenderer[count];
        var starts = new Vector3[count];
        var ends = new Vector3[count];
        var delays = new float[count];
        var spins = new float[count];
        var landed = new bool[count];
        const float maxDelay = 0.5f;

        for (int i = 0; i < count; i++)
        {
            ends[i] = center + new Vector3(Random.Range(-w * 0.45f, w * 0.45f), Random.Range(-h * 0.45f, h * 0.45f), 0f);
            starts[i] = new Vector3(ends[i].x + Random.Range(-0.15f, 0.15f), center.y + h * 0.5f + Random.Range(2f, 3.5f), 0f);
            delays[i] = Random.Range(0f, maxDelay);
            spins[i] = Random.Range(-720f, 720f);
            grains[i] = NewSprite(SquareSprite, new Color(0.97f, 0.97f, 1f, 0f), starts[i], Random.Range(0.07f, 0.15f));
        }

        float fall = Mathf.Max(0.05f, _saltFallTime);
        float total = maxDelay + fall + 0.1f;
        float impactTime = maxDelay * 0.6f + fall;
        bool hitDone = false;

        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                if (landed[i]) continue;
                float k = (t - delays[i]) / fall;
                if (k < 0f) continue;

                if (k >= 1f)
                {
                    landed[i] = true;
                    grains[i].enabled = false;
                    if (i % 3 == 0) StartCoroutine(Burst(ends[i], Color.white, 3, 0.35f, 0.18f, 0.07f));
                    continue;
                }

                grains[i].transform.position = Vector3.Lerp(starts[i], ends[i], k * k);
                grains[i].transform.rotation = Quaternion.Euler(0f, 0f, spins[i] * t);
                grains[i].color = new Color(0.97f, 0.97f, 1f, 0.95f);
            }

            if (!hitDone && t >= impactTime)
            {
                hitDone = true;
                impact();
                StartCoroutine(Burst(center, new Color(1f, 0.55f, 0.5f, 1f), 10, Mathf.Max(w, h) * 0.5f, 0.3f, 0.12f));
            }
            yield return null;
        }

        if (!hitDone) impact();
        foreach (SpriteRenderer sr in grains) Destroy(sr.gameObject);
    }

    private IEnumerator Shatter(Combatant target, System.Action impact)
    {
        Camera cam = Camera.main;
        Vector3 camHome = cam != null ? cam.transform.position : Vector3.zero;

        Vector3 center = target.VfxPoint.position;
        float w = 2f, h = 2.5f;
        SpriteRenderer body = target.ScarParent.GetComponent<SpriteRenderer>();
        if (body != null)
        {
            Bounds bounds = body.bounds;
            center = bounds.center;
            w = bounds.size.x * 1.1f;
            h = bounds.size.y * 1.1f;
        }

        var glass = new Color(0.7f, 0.92f, 1f);
        SpriteRenderer pane = NewSprite(SquareSprite, new Color(glass.r, glass.g, glass.b, 0f), center, 1f);
        pane.transform.localScale = new Vector3(w, h, 1f);

        const int cracks = 10;
        var mains = new SpriteRenderer[cracks];
        var branches = new SpriteRenderer[cracks];
        var angles = new float[cracks];
        var branchAngles = new float[cracks];
        var lengths = new float[cracks];
        for (int i = 0; i < cracks; i++)
        {
            angles[i] = Mathf.PI * 2f / cracks * i + Random.Range(-0.2f, 0.2f);
            branchAngles[i] = angles[i] + (Random.value < 0.5f ? -1f : 1f) * Random.Range(0.5f, 0.9f);
            lengths[i] = Mathf.Max(w, h) * Random.Range(0.3f, 0.55f);
            mains[i] = NewSprite(SquareSprite, new Color(1f, 1f, 1f, 0f), center, 1f);
            branches[i] = NewSprite(SquareSprite, new Color(1f, 1f, 1f, 0f), center, 1f);
            mains[i].sortingOrder = _sortingOrder + 1;
            branches[i].sortingOrder = _sortingOrder + 1;
        }

        float crackTime = Mathf.Max(0.05f, _shatterCrackTime);
        for (float t = 0f; t < crackTime; t += Time.deltaTime)
        {
            float k = t / crackTime;
            pane.color = new Color(glass.r, glass.g, glass.b, 0.22f * Mathf.Min(1f, k * 3f));

            float kb = Mathf.Clamp01((k - 0.5f) * 2f);
            for (int i = 0; i < cracks; i++)
            {
                Vector3 dir = new Vector3(Mathf.Cos(angles[i]), Mathf.Sin(angles[i]), 0f);
                PlaceLine(mains[i], center, center + dir * (lengths[i] * k), 0.04f);
                mains[i].color = new Color(0.9f, 0.98f, 1f, 0.95f);

                Vector3 forkStart = center + dir * (lengths[i] * 0.55f);
                Vector3 forkDir = new Vector3(Mathf.Cos(branchAngles[i]), Mathf.Sin(branchAngles[i]), 0f);
                PlaceLine(branches[i], forkStart, forkStart + forkDir * (lengths[i] * 0.4f * kb), 0.03f);
                branches[i].color = new Color(0.9f, 0.98f, 1f, kb > 0f ? 0.9f : 0f);
            }
            yield return null;
        }

        impact();
        Destroy(pane.gameObject);
        for (int i = 0; i < cracks; i++)
        {
            Destroy(mains[i].gameObject);
            Destroy(branches[i].gameObject);
        }
        StartCoroutine(Burst(center, new Color(0.9f, 0.98f, 1f, 1f), 14, Mathf.Max(w, h) * 0.6f, 0.35f, 0.12f));

        const int count = 26;
        var shards = new SpriteRenderer[count];
        var velocities = new Vector3[count];
        var spins = new float[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 p = center + new Vector3(Random.Range(-w * 0.5f, w * 0.5f), Random.Range(-h * 0.5f, h * 0.5f), 0f);
            Vector3 outward = p - center;
            outward.z = 0f;
            if (outward.sqrMagnitude < 0.0001f) outward = Vector3.up;

            velocities[i] = outward.normalized * Random.Range(1.5f, 4f) + Vector3.up * Random.Range(0.5f, 2.5f);
            spins[i] = Random.Range(-540f, 540f);

            float size = Random.Range(0.25f, 0.6f);
            shards[i] = NewSprite(ShardSprite, new Color(glass.r, glass.g, glass.b, 0.9f), p, size);
            shards[i].transform.localScale = new Vector3(size * Random.Range(0.6f, 1.2f), size * Random.Range(0.8f, 1.4f), 1f);
            shards[i].transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        float life = Mathf.Max(0.05f, _shatterTime);
        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            for (int i = 0; i < count; i++)
            {
                velocities[i] += Vector3.down * (9f * Time.deltaTime);
                shards[i].transform.position += velocities[i] * Time.deltaTime;
                shards[i].transform.Rotate(0f, 0f, spins[i] * Time.deltaTime);
                shards[i].color = new Color(glass.r, glass.g, glass.b, 0.9f * (1f - k));
            }

            if (cam != null)
                cam.transform.position = camHome + (Vector3)(Random.insideUnitCircle * (0.25f * (1f - k)));
            yield return null;
        }

        if (cam != null) cam.transform.position = camHome;
        foreach (SpriteRenderer sr in shards) Destroy(sr.gameObject);
    }

    private static void PlaceLine(SpriteRenderer sr, Vector3 a, Vector3 b, float thickness)
    {
        Vector3 d = b - a;
        sr.transform.position = (a + b) * 0.5f;
        sr.transform.localScale = new Vector3(Mathf.Max(0.001f, d.magnitude), thickness, 1f);
        sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    private IEnumerator Cuts(Combatant target, System.Action hit, int slashes)
    {
        slashes = Mathf.Max(1, slashes);
        float startAngle = Random.Range(0f, 180f);

        for (int i = 0; i < slashes; i++)
        {
            float angle = (startAngle + i * 137.5f + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            bool flip = Random.value < 0.5f;
            yield return Slash(target, hit, angle, flip);

            if (target.IsDead)
            {
                for (int j = i + 1; j < slashes; j++) hit();
                yield break;
            }

            if (i < slashes - 1) yield return new WaitForSeconds(_cutPause);
        }
    }

    private IEnumerator Slash(Combatant target, System.Action hit, float angle, bool flip)
    {
        Vector3 center = target.VfxPoint.position;
        float span = 2f;

        SpriteRenderer body = target.ScarParent.GetComponent<SpriteRenderer>();
        if (body != null)
        {
            Bounds bounds = body.bounds;
            center = bounds.center + new Vector3(Random.Range(-0.25f, 0.25f) * bounds.size.x,
                Random.Range(-0.25f, 0.25f) * bounds.size.y, 0f);
            span = Mathf.Min(bounds.size.x, bounds.size.y);
        }

        float length = span * Random.Range(0.6f, 0.95f);
        Vector3 half = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (length * 0.5f);
        Vector3 start = flip ? center + half : center - half;
        Vector3 end = flip ? center - half : center + half;

        SpriteRenderer glow = NewSprite(CircleSprite, new Color(1f, 0.25f, 0.3f, 0.55f), start, 1f);
        SpriteRenderer core = NewSprite(CircleSprite, Color.white, start, 1f);
        core.sortingOrder = _sortingOrder + 1;

        for (float t = 0f; t < _cutSlashTime; t += Time.deltaTime)
        {
            Vector3 head = Vector3.Lerp(start, end, t / _cutSlashTime);
            PlaceBlade(glow, start, head, 0.24f);
            PlaceBlade(core, start, head, 0.08f);
            yield return null;
        }
        PlaceBlade(glow, start, end, 0.24f);
        PlaceBlade(core, start, end, 0.08f);

        hit();
        AddScar(target, start, end);
        StartCoroutine(FadeBlade(glow, core));
        StartCoroutine(Burst(end, new Color(1f, 0.85f, 0.85f, 1f), 5, 0.5f, 0.2f, 0.1f));
    }

    private IEnumerator FadeBlade(SpriteRenderer glow, SpriteRenderer core)
    {
        const float time = 0.18f;
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = 1f - t / time;
            glow.color = new Color(1f, 0.25f, 0.3f, 0.55f * k);
            core.color = new Color(1f, 1f, 1f, k);
            yield return null;
        }

        Destroy(glow.gameObject);
        Destroy(core.gameObject);
    }

    private void AddScar(Combatant target, Vector3 a, Vector3 b)
    {
        SpriteRenderer wound = NewSprite(CircleSprite, new Color(0.5f, 0.06f, 0.1f, 0.95f), a, 1f);
        SpriteRenderer edge = NewSprite(CircleSprite, new Color(1f, 0.6f, 0.6f, 0.55f), a, 1f);
        wound.name = "Scar";
        edge.name = "Scar";
        PlaceBlade(wound, a, b, 0.09f);
        PlaceBlade(edge, a, b, 0.03f);

        wound.sortingOrder = 3;
        edge.sortingOrder = 4;

        Transform parent = target.ScarParent;
        wound.transform.SetParent(parent, true);
        edge.transform.SetParent(parent, true);
        target.AddScar(wound, edge);
    }

    private static void PlaceBlade(SpriteRenderer sr, Vector3 a, Vector3 b, float thickness)
    {
        Vector3 d = b - a;
        float length = Mathf.Max(0.001f, d.magnitude * 1.25f);
        sr.transform.position = (a + b) * 0.5f;
        sr.transform.localScale = new Vector3(length, thickness, 1f);
        sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    private const float SummonTime = 0.75f;
    private const float RitualTime = 1f;

    private sealed class PetView
    {
        internal Combatant Owner;
        internal Combatant Foe;
        internal Transform Root;
        internal readonly List<SpriteRenderer> BodyParts = new List<SpriteRenderer>();
        internal SpriteRenderer[] Eyes;
        internal TextMeshPro Label;
        internal int LastHp;
        internal float Pop;
        internal float Flash;
        internal Vector3 AttackOffset;
        internal float Seed;
    }

    private readonly Dictionary<Combatant, PetView> _pets = new Dictionary<Combatant, PetView>();

    private static Vector3 PetHome(Combatant owner, Combatant foe)
    {
        Vector3 o = owner.VfxPoint.position;
        float side = foe != null && foe.VfxPoint.position.x > o.x ? -1f : 1f;
        return o + new Vector3(side * 1.6f, -0.45f, 0f);
    }

    internal void SyncPet(Combatant owner, Combatant foe)
    {
        if (owner == null) return;
        _pets.TryGetValue(owner, out PetView view);

        if (!owner.HasFamiliar)
        {
            if (view == null) return;
            _pets.Remove(owner);
            StartCoroutine(PetDie(view));
            return;
        }

        if (view == null)
        {
            view = CreatePet(owner, foe);
            _pets[owner] = view;
            StartCoroutine(PetPop(view));
        }
        else
        {
            view.Foe = foe;
            if (owner.FamiliarHp < view.LastHp) StartCoroutine(PetFlash(view));
            else if (owner.FamiliarHp > view.LastHp) StartCoroutine(PetPop(view));
        }

        view.LastHp = owner.FamiliarHp;
        view.Label.text = $"{owner.FamiliarHp}/{owner.FamiliarMaxHp}";
    }

    private PetView CreatePet(Combatant owner, Combatant foe)
    {
        var view = new PetView { Owner = owner, Foe = foe, Seed = Random.value * 10f };
        var root = new GameObject("SpellPet");
        root.transform.position = PetHome(owner, foe);
        view.Root = root.transform;

        SpriteRenderer Part(Sprite sprite, Vector2 at, float size, int order)
        {
            SpriteRenderer sr = NewSprite(sprite, Color.white, root.transform.position, size);
            if (_tintOn) _tintedSprites.Remove(sr);
            sr.transform.SetParent(root.transform, false);
            sr.transform.localPosition = at;
            sr.sortingOrder = _sortingOrder + order;
            return sr;
        }

        view.BodyParts.Add(Part(CircleSprite, new Vector2(-0.5f, -0.15f), 0.28f, 1));
        view.BodyParts.Add(Part(CircleSprite, new Vector2(-0.24f, 0.4f), 0.3f, 1));
        view.BodyParts.Add(Part(CircleSprite, new Vector2(0.24f, 0.4f), 0.3f, 1));
        view.BodyParts.Add(Part(CircleSprite, Vector2.zero, 0.85f, 2));
        SpriteRenderer eyeL = Part(CircleSprite, new Vector2(-0.16f, 0.1f), 0.18f, 3);
        SpriteRenderer eyeR = Part(CircleSprite, new Vector2(0.16f, 0.1f), 0.18f, 3);
        SpriteRenderer pupilL = Part(CircleSprite, new Vector2(-0.16f, 0.09f), 0.08f, 4);
        SpriteRenderer pupilR = Part(CircleSprite, new Vector2(0.16f, 0.09f), 0.08f, 4);
        eyeL.color = eyeR.color = Color.white;
        pupilL.color = pupilR.color = new Color(0.1f, 0.08f, 0.12f);
        view.Eyes = new[] { eyeL, eyeR, pupilL, pupilR };

        view.Label = NewWorldText("", root.transform.position, 3f, Color.white);
        if (_tintOn) _tintedTexts.Remove(view.Label);
        view.Label.transform.SetParent(root.transform, false);
        view.Label.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        root.transform.localScale = Vector3.zero;
        return view;
    }

    private void UpdatePets()
    {
        if (_pets.Count == 0) return;

        List<Combatant> gone = null;
        foreach (KeyValuePair<Combatant, PetView> pair in _pets)
        {
            PetView v = pair.Value;
            if (v.Root == null || v.Owner == null)
            {
                if (gone == null) gone = new List<Combatant>();
                gone.Add(pair.Key);
                continue;
            }

            Vector3 home = PetHome(v.Owner, v.Foe);
            float bob = Mathf.Sin(Time.time * 3.2f + v.Seed) * 0.08f;
            v.Root.position = home + new Vector3(0f, bob, 0f) + v.AttackOffset;
            v.Root.localScale = Vector3.one * (v.Pop * (v.Owner.BloodFamiliar ? 1.8f : 1f));

            Color baseColor = new Color(1f, 0.9f, 0.65f);
            if (v.Owner.FamiliarEffect != null)
                baseColor = Color.Lerp(baseColor, v.Owner.FamiliarRainbow ? RainbowAt(v.Seed) : v.Owner.FamiliarTint, 0.85f);
            Color shown = Color.Lerp(baseColor, new Color(1f, 0.15f, 0.15f), v.Flash);
            foreach (SpriteRenderer sr in v.BodyParts) if (sr != null) sr.color = shown;
        }

        if (gone != null) foreach (Combatant c in gone) _pets.Remove(c);
    }

    private static Color RainbowAt(float offset)
    {
        return Color.HSVToRGB(Mathf.Repeat(Time.time * 0.9f + offset * 0.1f, 1f), 0.85f, 1f);
    }

    private IEnumerator PetPop(PetView v)
    {
        const float time = 0.35f;
        for (float t = 0f; t < time && v.Root != null; t += Time.deltaTime)
        {
            float k = t / time;
            v.Pop = Mathf.Sin(k * Mathf.PI * 0.5f) * (1f + 0.25f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        v.Pop = 1f;
    }

    private IEnumerator PetFlash(PetView v)
    {
        const float time = 0.35f;
        for (float t = 0f; t < time && v.Root != null; t += Time.deltaTime)
        {
            float k = 1f - t / time;
            v.Flash = k;
            v.AttackOffset = new Vector3(Mathf.Sin(t * 70f) * 0.12f * k, 0f, 0f);
            yield return null;
        }
        v.Flash = 0f;
        v.AttackOffset = Vector3.zero;
    }

    private IEnumerator PetDie(PetView v)
    {
        if (v.Root == null) yield break;
        Vector3 at = v.Root.position;
        v.Label.text = "";
        StartCoroutine(Burst(at, new Color(1f, 0.9f, 0.65f, 0.9f), 10, 0.9f, 0.4f, 0.2f));

        const float time = 0.3f;
        for (float t = 0f; t < time && v.Root != null; t += Time.deltaTime)
        {
            v.Flash = 1f;
            v.Pop = 1f - t / time;
            yield return null;
        }
        if (v.Root != null) Destroy(v.Root.gameObject);
    }

    internal IEnumerator PetAttack(Combatant owner, Combatant foe)
    {
        if (owner == null || !_pets.TryGetValue(owner, out PetView v) || v.Root == null) yield break;

        Vector3 reach = foe.VfxPoint.position - PetHome(owner, foe);
        reach *= 0.8f;
        const float out1 = 0.2f;
        const float back = 0.25f;

        for (float t = 0f; t < out1 && v.Root != null; t += Time.deltaTime)
        {
            float k = t / out1;
            v.AttackOffset = reach * (k * k);
            yield return null;
        }
        if (v.Root == null) yield break;
        v.AttackOffset = reach;
        StartCoroutine(Burst(foe.VfxPoint.position, Color.white, 6, 0.6f, 0.25f, 0.14f));

        for (float t = 0f; t < back && v.Root != null; t += Time.deltaTime)
        {
            v.AttackOffset = reach * (1f - t / back);
            yield return null;
        }
        if (v.Root != null) v.AttackOffset = Vector3.zero;
    }

    private IEnumerator Summon(Combatant owner, Combatant foe, System.Action impact)
    {
        Vector3 c = PetHome(owner, foe);
        const int count = 12;

        SpriteRenderer glow = NewSprite(CircleSprite, new Color(1f, 0.9f, 0.5f, 0f), c, 0.5f);
        var motes = new SpriteRenderer[count];
        for (int i = 0; i < count; i++)
            motes[i] = NewSprite(CircleSprite, new Color(1f, 0.95f, 0.7f, 0f), c, 0.14f);

        bool landed = false;
        for (float t = 0f; t < SummonTime; t += Time.deltaTime)
        {
            float k = t / SummonTime;
            float radius = Mathf.Lerp(1.1f, 0.05f, k * k);
            float alpha = Mathf.Sin(k * Mathf.PI);

            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f + k * 7f;
                motes[i].transform.position = c + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                motes[i].color = new Color(1f, 0.95f, 0.7f, alpha);
            }
            glow.transform.localScale = Vector3.one * (0.5f + 1.6f * k);
            glow.color = new Color(1f, 0.9f, 0.5f, 0.5f * alpha);

            if (!landed && k >= 0.7f)
            {
                landed = true;
                impact();
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in motes) Destroy(sr.gameObject);
        Destroy(glow.gameObject);
        if (!landed) impact();
    }

    private IEnumerator Ritual(Vector3 center, System.Action impact)
    {
        const int ring = 16;
        const int drops = 8;

        SpriteRenderer aura = NewSprite(CircleSprite, new Color(0.5f, 0f, 0.05f, 0f), center, 2.2f);
        var embers = new SpriteRenderer[ring];
        for (int i = 0; i < ring; i++)
            embers[i] = NewSprite(CircleSprite, new Color(0.85f, 0.05f, 0.1f, 0f), center, 0.16f);
        var blood = new SpriteRenderer[drops];
        for (int i = 0; i < drops; i++)
            blood[i] = NewSprite(CircleSprite, new Color(0.7f, 0f, 0.05f, 0f), center, 0.12f);

        bool landed = false;
        for (float t = 0f; t < RitualTime; t += Time.deltaTime)
        {
            float k = t / RitualTime;
            float alpha = Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI);
            float radius = Mathf.Lerp(1.3f, 0.35f, k);

            for (int i = 0; i < ring; i++)
            {
                float a = i / (float)ring * Mathf.PI * 2f + k * 5f;
                embers[i].transform.position = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                embers[i].color = new Color(0.85f, 0.05f, 0.1f, alpha);
            }
            for (int i = 0; i < drops; i++)
            {
                float rise = Mathf.Repeat(k * 1.5f + i / (float)drops, 1f);
                float sway = Mathf.Sin(rise * 6f + i) * 0.25f;
                blood[i].transform.position = center + new Vector3(sway, Mathf.Lerp(-0.4f, 1.3f, rise), 0f);
                blood[i].color = new Color(0.7f, 0f, 0.05f, alpha * (1f - rise));
            }
            aura.color = new Color(0.5f, 0f, 0.05f, 0.45f * alpha);

            if (!landed && k >= 0.65f)
            {
                landed = true;
                StartCoroutine(Burst(center, new Color(0.9f, 0.05f, 0.1f, 0.9f), 14, 1.2f, 0.35f, 0.2f));
                impact();
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in embers) Destroy(sr.gameObject);
        foreach (SpriteRenderer sr in blood) Destroy(sr.gameObject);
        Destroy(aura.gameObject);
        if (!landed) impact();
    }

    private IEnumerator Burst(Vector3 center, Color color, int count, float radius, float duration, float size)
    {
        var parts = new SpriteRenderer[count];
        var dirs = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            parts[i] = NewSprite(CircleSprite, color, center, size);
            float a = Random.value * Mathf.PI * 2f;
            dirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Random.Range(0.4f, 1f);
        }

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            for (int i = 0; i < count; i++)
            {
                parts[i].transform.position = center + dirs[i] * (radius * k);
                parts[i].transform.localScale = Vector3.one * (size * (1f - k));
                Color c = color;
                c.a = color.a * (1f - k);
                parts[i].color = c;
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in parts) Destroy(sr.gameObject);
    }

    private static void SetVisible(List<SpriteRenderer> parts, bool visible)
    {
        foreach (SpriteRenderer sr in parts) sr.enabled = visible;
    }

    private SpriteRenderer Segment(Vector3 a, Vector3 b, float thickness, Color color)
    {
        Vector3 d = b - a;
        SpriteRenderer sr = NewSprite(SquareSprite, color, (a + b) * 0.5f, 1f);
        sr.transform.localScale = new Vector3(d.magnitude, thickness, 1f);
        sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        return sr;
    }

    private SpriteRenderer NewSprite(Sprite sprite, Color color, Vector3 position, float size)
    {
        var go = new GameObject("SpellVfx");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * size;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = _sortingOrder;
        if (_tintOn) _tintedSprites.Add(sr);
        return sr;
    }

    private static Sprite CircleSprite
    {
        get
        {
            if (_circleSprite == null) _circleSprite = MakeCircle();
            return _circleSprite;
        }
    }

    private static Sprite SquareSprite
    {
        get
        {
            if (_squareSprite == null) _squareSprite = MakeSquare();
            return _squareSprite;
        }
    }
    private static Sprite HeartSprite
    {
        get
        {
            if (_heartSprite == null) _heartSprite = MakeHeart();
            return _heartSprite;
        }
    }

    private static Sprite ShardSprite
    {
        get
        {
            if (_shardSprite == null) _shardSprite = MakeShard();
            return _shardSprite;
        }
    }

    private static Sprite MakeShard()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size;
                float ny = (y + 0.5f) / size;
                bool inside = ny <= 1f - Mathf.Abs(nx * 2f - 1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, inside ? 1f : 0f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite MakeHeart()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = ((x + 0.5f) / size * 2f - 1f) * 1.3f;
                float ny = ((y + 0.5f) / size * 2f - 1f) * 1.3f + 0.15f;
                float q = nx * nx + ny * ny - 1f;
                float v = q * q * q - nx * nx * ny * ny * ny;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, v <= 0f ? 1f : 0f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite MakeCircle()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float r = size * 0.5f;
        var center = new Vector2(r, r);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) / r;
                float a = Mathf.Clamp01((1f - d) * 3f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    private static Sprite MakeSquare()
    {
        const int size = 4;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
