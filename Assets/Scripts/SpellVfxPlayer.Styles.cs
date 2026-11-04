using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal sealed partial class SpellVfxPlayer
{
    private VfxStyle _style = new VfxStyle();

    internal VfxStyle Style
    {
        get { return _style; }
        set { _style = value ?? new VfxStyle(); }
    }

    private static readonly System.Action Nothing = () => { };

    private static readonly Color FurColor = new Color(0.78f, 0.58f, 0.36f, 0.95f);

    private void ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam)
    {
        cam = Camera.main;
        halfH = cam != null && cam.orthographic ? cam.orthographicSize : 5.4f;
        halfW = cam != null ? halfH * cam.aspect : halfH * 16f / 9f;
        origin = cam != null ? cam.transform.position : Vector3.zero;
        origin.z = 0f;
    }

    internal IEnumerator Banner(string text, Color color, float time = 1.1f)
    {
        ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam);
        Vector3 center = origin + Vector3.up * (halfH * 0.15f);

        TextMeshPro label = NewWorldText(text, center, 14f, color);
        label.rectTransform.sizeDelta = new Vector2(halfW * 2f, 6f);
        label.enableWordWrapping = false;
        label.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder + 40;

        TextMeshPro shadow = NewWorldText(text, center + new Vector3(0.12f, -0.12f, 0f), 14f, new Color(0f, 0f, 0f, 0.6f));
        shadow.rectTransform.sizeDelta = label.rectTransform.sizeDelta;
        shadow.enableWordWrapping = false;
        shadow.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder + 39;

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            float pop = k < 0.15f ? Mathf.Lerp(0.2f, 1.25f, k / 0.15f) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((k - 0.15f) / 0.1f));
            float fade = k > 0.75f ? 1f - (k - 0.75f) / 0.25f : 1f;
            Vector3 jitter = (Vector3)(Random.insideUnitCircle * 0.05f * (1f - k));
            label.transform.position = center + jitter;
            shadow.transform.position = center + jitter + new Vector3(0.12f, -0.12f, 0f);
            label.transform.localScale = Vector3.one * pop;
            shadow.transform.localScale = Vector3.one * pop;
            Color c = color;
            c.a = fade;
            label.color = c;
            shadow.color = new Color(0f, 0f, 0f, 0.6f * fade);
            yield return null;
        }

        Destroy(label.gameObject);
        Destroy(shadow.gameObject);
    }

    internal IEnumerator ScreenWash(Color color, bool rainbow, float time)
    {
        ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam);
        SpriteRenderer wash = NewSprite(SquareSprite, color, origin, 1f);
        wash.transform.localScale = new Vector3(halfW * 2.2f, halfH * 2.2f, 1f);
        wash.sortingOrder = _sortingOrder + 30;
        _tintedSprites.Remove(wash);

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            Color c = rainbow ? Color.HSVToRGB(Mathf.Repeat(t * 0.8f, 1f), 0.8f, 1f) : color;
            c.a = 0.35f * Mathf.Sin(k * Mathf.PI);
            wash.color = c;
            yield return null;
        }

        Destroy(wash.gameObject);
    }

    private IEnumerator Scatter(Sprite sprite, Color color, int count, bool groundOnly, float size, float hold, bool flames, bool rainbow = false)
    {
        ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam);
        var parts = new SpriteRenderer[count];
        var spots = new Vector3[count];
        var delays = new float[count];
        var spin = new float[count];

        for (int i = 0; i < count; i++)
        {
            float y = groundOnly ? Random.Range(-halfH * 0.85f, -halfH * 0.1f) : Random.Range(-halfH * 0.85f, halfH * 0.85f);
            spots[i] = origin + new Vector3(Random.Range(-halfW * 0.9f, halfW * 0.9f), y, 0f);
            delays[i] = Random.Range(0f, 0.5f);
            spin[i] = flames ? 0f : Random.Range(-25f, 25f);
            parts[i] = flames ? NewFlame(color, spots[i], size * 0.6f, size * 1.5f) : NewSprite(sprite, color, spots[i], size);
            parts[i].enabled = false;
        }

        float total = 0.5f + 0.25f + hold + 0.4f;
        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                float k = t - delays[i];
                if (k < 0f) continue;
                parts[i].enabled = true;
                float grow = Mathf.Clamp01(k / 0.25f);
                float pop = 1f + 0.2f * Mathf.Sin(Mathf.Clamp01(k / 0.25f) * Mathf.PI);
                float fade = Mathf.Clamp01((total - t) / 0.4f);
                float flick = flames ? 1f + 0.15f * Mathf.Sin(t * 20f + i) : 1f;

                Color c = rainbow ? Color.HSVToRGB(Mathf.Repeat(t * 0.6f + i * 0.13f, 1f), 0.8f, 1f) : color;
                c.a = color.a * fade * grow;
                parts[i].color = c;

                if (flames) parts[i].transform.localScale = new Vector3(size * 0.6f * grow, size * 0.75f * grow * flick, 1f);
                else
                {
                    parts[i].transform.localScale = Vector3.one * (size * grow * pop);
                    parts[i].transform.rotation = Quaternion.Euler(0f, 0f, spin[i] * Mathf.Min(1f, k));
                }
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in parts) Destroy(sr.gameObject);
    }

    private IEnumerator SpriteBurst(Sprite sprite, Vector3 center, Color color, int count, float radius, float duration, float size)
    {
        var parts = new SpriteRenderer[count];
        var dirs = new Vector3[count];
        var spin = new float[count];
        for (int i = 0; i < count; i++)
        {
            parts[i] = NewSprite(sprite, color, center, size);
            float a = Random.value * Mathf.PI * 2f;
            dirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Random.Range(0.4f, 1f);
            spin[i] = Random.Range(-360f, 360f);
        }

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            for (int i = 0; i < count; i++)
            {
                parts[i].transform.position = center + dirs[i] * (radius * k);
                parts[i].transform.localScale = Vector3.one * (size * (1f - k));
                parts[i].transform.rotation = Quaternion.Euler(0f, 0f, spin[i] * k);
                Color c = color;
                c.a = color.a * (1f - k);
                parts[i].color = c;
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in parts) Destroy(sr.gameObject);
    }

    private IEnumerator FurSkin(SpellVfxKind kind, Vector3 from, Vector3 to)
    {
        bool moving = kind == SpellVfxKind.Fireball || kind == SpellVfxKind.Spark || kind == SpellVfxKind.Rainbow || kind == SpellVfxKind.Git;
        if (kind == SpellVfxKind.Meteor)
        {
            yield return Scatter(ExtraSprites.Puff, FurColor, 16, false, 0.9f, 0.6f, false);
            yield break;
        }

        const int count = 7;
        float duration = moving ? Mathf.Max(0.3f, _fireballTravelTime) : 0.8f;
        float radius = moving ? 0.55f : 1.1f;
        var puffs = new SpriteRenderer[count];
        for (int i = 0; i < count; i++)
        {
            puffs[i] = NewSprite(ExtraSprites.Puff, FurColor, from, 0.5f);
            puffs[i].sortingOrder = _sortingOrder + 3;
        }

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            Vector3 center = moving ? Vector3.Lerp(from, to, k) : to;
            float fade = moving ? 1f : Mathf.Clamp01((1f - k) * 3f);
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)count) * Mathf.PI * 2f + t * 7f;
                puffs[i].transform.position = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                puffs[i].transform.rotation = Quaternion.Euler(0f, 0f, a * 57f);
                puffs[i].color = new Color(FurColor.r, FurColor.g, FurColor.b, fade);
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in puffs) Destroy(sr.gameObject);
    }

    private IEnumerator Helix(Vector3 from, Vector3 to, System.Action impact)
    {
        Vector3 dir = (to - from).normalized;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
        float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg - 90f;
        float duration = Mathf.Max(0.4f, _fireballTravelTime * 1.3f);
        float scale = _style.Scale;

        SpriteRenderer fireOuter = NewFlame(FlameVisuals.Outer(0.95f), from, 0.7f * scale, 1.5f * scale);
        SpriteRenderer fireCore = NewFlame(FlameVisuals.Core(1f), from, 0.32f * scale, 0.8f * scale);
        SpriteRenderer waterGlow = NewSprite(CircleSprite, new Color(0.25f, 0.55f, 1f, 0.45f), from, 1.1f * scale);
        SpriteRenderer waterCore = NewSprite(CircleSprite, new Color(0.7f, 0.9f, 1f, 1f), from, 0.55f * scale);
        fireOuter.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        fireCore.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            Vector3 p = Vector3.Lerp(from, to, k);
            float swing = Mathf.Sin(k * Mathf.PI * 4f) * 0.7f * Mathf.Sin(k * Mathf.PI);
            Vector3 a = p + perp * swing;
            Vector3 b = p - perp * swing;

            fireOuter.transform.position = a;
            fireCore.transform.position = a;
            waterGlow.transform.position = b;
            waterCore.transform.position = b;
            StartCoroutine(Ember(a, dir));
            if (Time.frameCount % 2 == 0) StartCoroutine(Droplet(b));
            yield return null;
        }

        Destroy(fireOuter.gameObject);
        Destroy(fireCore.gameObject);
        Destroy(waterGlow.gameObject);
        Destroy(waterCore.gameObject);
        impact();
        StartCoroutine(Burst(to, new Color(0.35f, 0.65f, 1f, 1f), 14, 1.2f, 0.4f, 0.26f));
        yield return FlameBurst(to);
    }

    private IEnumerator Droplet(Vector3 position)
    {
        Vector3 start = position + (Vector3)(Random.insideUnitCircle * 0.1f);
        SpriteRenderer drop = NewSprite(CircleSprite, new Color(0.45f, 0.75f, 1f, 0.9f), start, 0.22f);
        const float life = 0.35f;
        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            drop.transform.position = start + Vector3.down * (0.5f * k * k);
            drop.transform.localScale = Vector3.one * (0.22f * (1f - k));
            drop.color = new Color(0.45f, 0.75f, 1f, 0.9f * (1f - k));
            yield return null;
        }
        Destroy(drop.gameObject);
    }

    private Sprite MeteorSprite()
    {
        switch (_style.Look)
        {
            case MeteorLook.Medical: return ExtraSprites.Cross;
            case MeteorLook.Bits: return ExtraSprites.One;
            case MeteorLook.Shatter: return ShardSprite;
            default: return CircleSprite;
        }
    }

    private Color RockColor(int index)
    {
        switch (_style.Look)
        {
            case MeteorLook.Medical: return new Color(1f, 0.25f, 0.3f, 1f);
            case MeteorLook.Salt: return new Color(0.95f, 0.95f, 1f, 1f);
            case MeteorLook.Bits: return new Color(0.25f, 1f, 0.45f, 1f);
            case MeteorLook.Shatter: return new Color(0.75f, 0.95f, 1f, 1f);
            case MeteorLook.Rainbow: return Color.HSVToRGB(Mathf.Repeat(Time.time * 0.8f + index * 0.17f, 1f), 0.85f, 1f);
            default: return new Color(0.45f, 0.25f, 0.15f, 1f);
        }
    }

    private Color RockGlow(int index)
    {
        Color c = RockColor(index);
        if (_style.Look == MeteorLook.Normal) return new Color(1f, 0.3f, 0.05f, 0.5f);
        c.a = 0.45f;
        return c;
    }

    private IEnumerator MeteorTrail(Vector3 position, int index)
    {
        if (_style.Look == MeteorLook.Normal)
        {
            yield return Ember(position);
            yield break;
        }

        Sprite sprite = CircleSprite;
        float size = 0.22f;
        if (_style.Look == MeteorLook.Medical) { sprite = ExtraSprites.Cross; size = 0.3f; }
        else if (_style.Look == MeteorLook.Bits) { sprite = Random.value < 0.5f ? ExtraSprites.One : ExtraSprites.Ring; size = 0.3f; }
        else if (_style.Look == MeteorLook.Shatter) { sprite = ShardSprite; size = 0.25f; }
        else if (_style.Look == MeteorLook.Salt) size = 0.14f;

        Color color = RockColor(index);
        Vector3 start = position + (Vector3)(Random.insideUnitCircle * 0.15f);
        SpriteRenderer bit = NewSprite(sprite, color, start, size);
        const float life = 0.4f;
        for (float t = 0f; t < life; t += Time.deltaTime)
        {
            float k = t / life;
            bit.transform.position = start + new Vector3(0.15f * k, 0.35f * k, 0f);
            bit.transform.localScale = Vector3.one * (size * (1f - 0.6f * k));
            Color c = color;
            c.a = 0.9f * (1f - k);
            bit.color = c;
            yield return null;
        }
        Destroy(bit.gameObject);
    }

    private IEnumerator MeteorLanding(Vector3 position)
    {
        switch (_style.Look)
        {
            case MeteorLook.Medical:
                yield return SpriteBurst(ExtraSprites.Cross, position, new Color(1f, 0.35f, 0.4f, 1f), 8, 1f, 0.45f, 0.35f);
                break;
            case MeteorLook.Salt:
                yield return Burst(position, new Color(0.95f, 0.95f, 1f, 1f), 14, 1f, 0.4f, 0.14f);
                break;
            case MeteorLook.Bits:
                yield return SpriteBurst(ExtraSprites.One, position, new Color(0.25f, 1f, 0.45f, 1f), 10, 1f, 0.45f, 0.35f);
                break;
            case MeteorLook.Shatter:
                yield return SpriteBurst(ShardSprite, position, new Color(0.75f, 0.95f, 1f, 1f), 12, 1.3f, 0.5f, 0.4f);
                break;
            case MeteorLook.Rainbow:
                yield return Burst(position, Color.HSVToRGB(Random.value, 0.8f, 1f), 12, 1f, 0.4f, 0.25f);
                break;
            default:
                yield return Burst(position, new Color(1f, 0.5f, 0.1f, 1f), 10, 0.9f, 0.35f, 0.25f);
                break;
        }
    }

    private IEnumerator GiantMeteor(Vector3 center)
    {
        Vector3 start = center + new Vector3(5f, 9f, 0f);
        SpriteRenderer glow = NewSprite(CircleSprite, new Color(1f, 0.3f, 0.05f, 0.5f), start, 4.2f);
        SpriteRenderer rock = NewSprite(CircleSprite, new Color(0.45f, 0.25f, 0.15f, 1f), start, 2f);
        rock.sortingOrder = _sortingOrder + 1;
        const float fall = 0.8f;

        for (float t = 0f; t < fall; t += Time.deltaTime)
        {
            float k = t / fall;
            Vector3 p = Vector3.Lerp(start, center, k * k);
            glow.transform.position = p;
            rock.transform.position = p;
            StartCoroutine(Ember(p));
            StartCoroutine(Ember(p + Vector3.right * 0.4f));
            yield return null;
        }

        Destroy(glow.gameObject);
        Destroy(rock.gameObject);
    }

    private IEnumerator Tsunami(Vector3 target)
    {
        ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam);
        float startX = origin.x - halfW - 4f;
        float endX = origin.x + halfW + 4f;
        const float duration = 1.4f;
        const int foam = 9;

        SpriteRenderer body = NewSprite(CircleSprite, new Color(0.15f, 0.45f, 0.95f, 0.85f), origin, 1f);
        body.transform.localScale = new Vector3(8f, halfH * 1.9f, 1f);
        body.sortingOrder = _sortingOrder + 20;
        var crest = new SpriteRenderer[foam];
        for (int i = 0; i < foam; i++)
        {
            crest[i] = NewSprite(CircleSprite, new Color(0.85f, 0.95f, 1f, 0.95f), origin, 1.6f);
            crest[i].sortingOrder = _sortingOrder + 21;
        }

        bool hit = false;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            float x = Mathf.Lerp(startX, endX, k);
            body.transform.position = new Vector3(x, origin.y - halfH * 0.1f, 0f);
            for (int i = 0; i < foam; i++)
            {
                float y = origin.y + halfH * 0.85f - i * (halfH * 1.7f / (foam - 1));
                float bob = Mathf.Sin(t * 12f + i) * 0.25f;
                crest[i].transform.position = new Vector3(x + 3.4f + bob, y, 0f);
            }
            if (!hit && Mathf.Abs(x - target.x) < 3f)
            {
                hit = true;
                StartCoroutine(Burst(target, new Color(0.7f, 0.9f, 1f, 1f), 24, 3f, 0.7f, 0.4f));
            }
            yield return null;
        }

        Destroy(body.gameObject);
        foreach (SpriteRenderer sr in crest) Destroy(sr.gameObject);
    }

    private IEnumerator Jackpot(JackpotFx kind, Vector3 center, Vector3 enemy)
    {
        ArenaBounds(out Vector3 origin, out float halfW, out float halfH, out Camera cam);
        switch (kind)
        {
            case JackpotFx.Fire:
                StartCoroutine(Scatter(null, FlameVisuals.Outer(0.9f), 12, true, 1.4f, 2.2f, true));
                yield return Fireball(enemy + new Vector3(0f, halfH, 0f), enemy, Nothing, 2.6f);
                break;
            case JackpotFx.Fur:
                yield return Scatter(ExtraSprites.Puff, FurColor, 26, false, 1.2f, 1.4f, false);
                break;
            case JackpotFx.Meteor:
                yield return MeteorWave(origin, halfW, halfH, cam, Nothing);
                break;
            case JackpotFx.Ice:
                StartCoroutine(Scatter(ShardSprite, new Color(0.6f, 0.9f, 1f, 0.95f), 12, true, 1.6f, 2f, false));
                yield return Scatter(SquareSprite, new Color(0.7f, 0.95f, 1f, 0.8f), 8, true, 0.8f, 2f, false);
                break;
            case JackpotFx.Water:
                yield return Tsunami(enemy);
                break;
            case JackpotFx.Salt:
                yield return Scatter(ExtraSprites.Mound, new Color(0.95f, 0.95f, 1f, 0.95f), 12, true, 1.5f, 1.8f, false);
                break;
            case JackpotFx.Poop:
                float saved = _style.Scale;
                _style.Scale = 2.4f;
                yield return Poop(enemy, Nothing);
                _style.Scale = saved;
                break;
            case JackpotFx.Shatter:
                yield return Scatter(ShardSprite, new Color(0.8f, 0.95f, 1f, 0.95f), 28, false, 1.1f, 0.6f, false);
                break;
            case JackpotFx.Rainbow:
                yield return ScreenWash(Color.white, true, 2f);
                break;
            case JackpotFx.Heal:
                yield return Scatter(ExtraSprites.Cross, new Color(0.35f, 1f, 0.5f, 0.95f), 16, false, 0.9f, 1.2f, false);
                break;
            case JackpotFx.Cleanse:
                yield return Scatter(ShardSprite, new Color(0.85f, 1f, 1f, 0.95f), 16, false, 0.8f, 1.2f, false);
                break;
            case JackpotFx.Fear:
                StartCoroutine(Scatter(ExtraSprites.Skull, new Color(0.85f, 0.8f, 0.95f, 0.9f), 8, false, 1.3f, 1.4f, false));
                yield return ScreenWash(new Color(0.3f, 0.05f, 0.45f), false, 1.8f);
                break;
            case JackpotFx.Shield:
                yield return Scatter(ExtraSprites.Ring, new Color(0.6f, 0.75f, 1f, 0.9f), 10, false, 1.3f, 1.2f, false);
                break;
            case JackpotFx.Mirror:
                yield return ScreenWash(new Color(0.85f, 0.9f, 1f), false, 1.2f);
                break;
            case JackpotFx.Poison:
                yield return Scatter(CircleSprite, new Color(0.5f, 0.9f, 0.2f, 0.7f), 18, false, 0.7f, 1.4f, false);
                break;
        }
    }

    private IEnumerator ExplosionStyled(Vector3 center, System.Action impact)
    {
        if (_style.Before == BeforeFx.Thunder)
        {
            float saved = _style.Scale;
            _style.Scale = 2.2f;
            yield return Thunder(center, Nothing);
            _style.Scale = saved;
        }
        else if (_style.Before == BeforeFx.Meteor) yield return GiantMeteor(center);

        if (_style.Explosions > 1)
        {
            for (int i = 0; i < _style.Explosions - 1; i++)
            {
                StartCoroutine(Explosion(center + (Vector3)(Random.insideUnitCircle * 1.8f), Nothing));
                yield return new WaitForSeconds(0.3f);
            }
        }
        yield return Explosion(center, impact);

        switch (_style.After)
        {
            case AfterFx.ArenaFire:
                yield return Scatter(null, FlameVisuals.Outer(0.9f), 14, true, 1.4f, 2.4f, true);
                break;
            case AfterFx.ArenaIce:
                StartCoroutine(Scatter(ShardSprite, new Color(0.6f, 0.9f, 1f, 0.95f), 12, true, 1.5f, 2f, false));
                yield return Scatter(SquareSprite, new Color(0.7f, 0.95f, 1f, 0.8f), 7, true, 0.8f, 2f, false);
                break;
            case AfterFx.ShardRain:
                yield return Scatter(ShardSprite, new Color(0.8f, 0.95f, 1f, 0.95f), 30, false, 1.1f, 0.6f, false);
                break;
        }
    }

    internal IEnumerator BloodIce(Combatant who)
    {
        Vector3 p = who.VfxPoint.position;
        SpriteRenderer block = NewSprite(CircleSprite, new Color(0.75f, 0.2f, 0.35f, 0.6f), p, 1f);
        block.transform.localScale = new Vector3(2.2f, 3f, 1f);
        block.sortingOrder = _sortingOrder + 8;
        SpriteRenderer shine = NewSprite(CircleSprite, new Color(0.8f, 0.95f, 1f, 0.5f), p + new Vector3(-0.3f, 0.5f, 0f), 1f);
        shine.transform.localScale = new Vector3(0.7f, 1.4f, 1f);
        shine.sortingOrder = _sortingOrder + 9;

        const float time = 1f;
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            float a = k < 0.2f ? k / 0.2f : 1f;
            block.color = new Color(0.75f, 0.2f, 0.35f, 0.6f * a);
            shine.color = new Color(0.8f, 0.95f, 1f, 0.5f * a);
            yield return null;
        }

        Destroy(block.gameObject);
        Destroy(shine.gameObject);
        yield return SpriteBurst(ShardSprite, p, new Color(0.9f, 0.4f, 0.5f, 1f), 12, 1.6f, 0.5f, 0.4f);
    }

    private Sprite SlotSprite(int symbol)
    {
        int s = symbol % SlotMachineEffect.SymbolCount;
        switch (_style.Theme)
        {
            case SlotTheme.Scary: return s % 2 == 0 ? ExtraSprites.Skull : ExtraSprites.Bat;
            case SlotTheme.Medical: return ExtraSprites.Cross;
            case SlotTheme.Bits: return s % 2 == 0 ? ExtraSprites.Ring : ExtraSprites.One;
            default: return SlotShape(symbol);
        }
    }

    private Color SlotColor(int symbol)
    {
        int s = symbol % SlotMachineEffect.SymbolCount;
        switch (_style.Theme)
        {
            case SlotTheme.Scary: return s % 2 == 0 ? new Color(0.92f, 0.9f, 0.85f) : new Color(0.65f, 0.35f, 0.9f);
            case SlotTheme.Medical:
                return new[] { new Color(1f, 0.25f, 0.3f), new Color(0.3f, 1f, 0.5f), new Color(0.35f, 0.6f, 1f), new Color(1f, 0.45f, 0.85f), new Color(1f, 0.9f, 0.3f) }[s];
            case SlotTheme.Bits: return new Color(0.2f, Mathf.Lerp(0.7f, 1f, s / 4f), 0.4f);
            case SlotTheme.Rainbow: return Color.HSVToRGB(Mathf.Repeat(Time.time * 0.8f + s * 0.2f, 1f), 0.85f, 1f);
            default: return SlotColors[s % SlotColors.Length];
        }
    }

    private Sprite JackpotSprite()
    {
        switch (_style.Jackpot)
        {
            case JackpotFx.Fire: return FlameVisuals.Flame;
            case JackpotFx.Fur: return ExtraSprites.Puff;
            case JackpotFx.Meteor: return CircleSprite;
            default: return null;
        }
    }

    private Color JackpotSymbolColor()
    {
        switch (_style.Jackpot)
        {
            case JackpotFx.Fire: return FlameVisuals.Mid(1f);
            case JackpotFx.Fur: return FurColor;
            default: return new Color(0.5f, 0.28f, 0.15f);
        }
    }
}
