using UnityEngine;

internal enum MeteorLook { Normal, Medical, Salt, Bits, Rainbow, Shatter }
internal enum SlotTheme { Default, Scary, Medical, Bits, Rainbow }
internal enum JackpotFx { None, Fire, Fur, Meteor, Ice, Water, Salt, Poop, Shatter, Rainbow, Heal, Fear, Shield, Mirror, Cleanse, Poison }
internal enum AfterFx { None, ArenaFire, ArenaIce, ShardRain }
internal enum BeforeFx { None, Thunder, Meteor }

internal sealed class VfxStyle
{
    internal float Scale = 1f;
    internal bool Fur;
    internal bool Helix;
    internal MeteorLook Look;
    internal SlotTheme Theme;
    internal JackpotFx Jackpot;
    internal AfterFx After;
    internal BeforeFx Before;
    internal int Explosions = 1;

    private static string RiderWord(SpellEntry entry)
    {
        if (entry == null || entry.RiderEntry == null) return "";
        return CastResolver.Normalize(entry.RiderEntry.Word);
    }

    private static JackpotFx JackpotFor(SpellEffect rider)
    {
        if (rider is BurnEffect) return JackpotFx.Fire;
        if (rider is FurBallEffect) return JackpotFx.Fur;
        if (rider is MeteorShowerEffect) return JackpotFx.Meteor;
        if (rider is FreezeEffect) return JackpotFx.Ice;
        if (rider is WetEffect) return JackpotFx.Water;
        if (rider is SaltEffect) return JackpotFx.Salt;
        if (rider is PoopEffect) return JackpotFx.Poop;
        if (rider is ShatterEffect) return JackpotFx.Shatter;
        if (rider is RainbowEffect) return JackpotFx.Rainbow;
        if (rider is HealEffect) return JackpotFx.Heal;
        if (rider is FearEffect) return JackpotFx.Fear;
        if (rider is ShieldEffect) return JackpotFx.Shield;
        if (rider is MirrorEffect) return JackpotFx.Mirror;
        if (rider is CleanseEffect) return JackpotFx.Cleanse;
        if (rider is PoisonEffect) return JackpotFx.Poison;
        return JackpotFx.None;
    }

    internal static VfxStyle For(SpellEntry entry, bool byPlayer)
    {
        var s = new VfxStyle();
        if (entry == null || entry.Effect == null) return s;

        SpellEffect body = entry.Effect;
        SpellEffect rider = null;
        if (entry.Effect is MixedEffect mix)
        {
            body = mix.Body;
            rider = mix.RiderEffect;
        }

        if (byPlayer)
        {
            SpellEffect boosted = BloodPacts.Active(BloodPerk.Boost) ? BloodPacts.SourceOf(BloodPerk.Boost) : null;
            if (boosted != null && (body == boosted || rider == boosted)) s.Scale = 2f;
            if (BloodPacts.Active(BloodPerk.KeepScars) && (body is PoopEffect || rider is PoopEffect)) s.Scale = 2f;
            if (BloodPacts.Active(BloodPerk.PierceShield)) s.Fur = true;
        }

        if (rider is FurBallEffect) s.Fur = true;
        if ((body is BurnEffect && rider is WetEffect) || (body is WetEffect && rider is BurnEffect)) s.Helix = true;

        string riderWord = RiderWord(entry);

        if (body is MeteorShowerEffect)
        {
            if (rider is HealEffect || rider is CleanseEffect) s.Look = MeteorLook.Medical;
            else if (rider is SaltEffect) s.Look = MeteorLook.Salt;
            else if (rider is GitEffect) s.Look = MeteorLook.Bits;
            else if (rider is RainbowEffect) s.Look = MeteorLook.Rainbow;
            else if (rider is ShatterEffect) s.Look = MeteorLook.Shatter;
        }

        if (body is SlotMachineEffect)
        {
            if (rider is FearEffect) s.Theme = SlotTheme.Scary;
            else if (rider is HealEffect) s.Theme = SlotTheme.Medical;
            else if (rider is GitEffect) s.Theme = SlotTheme.Bits;
            else if (rider is RainbowEffect) s.Theme = SlotTheme.Rainbow;
            s.Jackpot = JackpotFor(rider);
        }

        if (body is ExplosionEffect)
        {
            if (riderWord == "bolt" || riderWord == "lightning") s.Before = BeforeFx.Thunder;
            else if (rider is MeteorShowerEffect) s.Before = BeforeFx.Meteor;
            else if (rider is GitEffect) s.Explosions = 4;
            else if (rider is ShatterEffect) s.After = AfterFx.ShardRain;
            else if (rider is BurnEffect) s.After = AfterFx.ArenaFire;
            else if (rider is FreezeEffect) s.After = AfterFx.ArenaIce;
        }

        return s;
    }
}

internal static class ExtraSprites
{
    private static Sprite _cross, _skull, _bat, _puff, _ring, _one, _mound;

    internal static Sprite Cross { get { if (_cross == null) _cross = Make(CrossAlpha); return _cross; } }
    internal static Sprite Skull { get { if (_skull == null) _skull = Make(SkullAlpha); return _skull; } }
    internal static Sprite Bat { get { if (_bat == null) _bat = Make(BatAlpha); return _bat; } }
    internal static Sprite Puff { get { if (_puff == null) _puff = Make(PuffAlpha); return _puff; } }
    internal static Sprite Ring { get { if (_ring == null) _ring = Make(RingAlpha); return _ring; } }
    internal static Sprite One { get { if (_one == null) _one = Make(OneAlpha); return _one; } }
    internal static Sprite Mound { get { if (_mound == null) _mound = Make(MoundAlpha); return _mound; } }

    private static Sprite Make(System.Func<float, float, float> alpha)
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size - 0.5f;
                float v = (y + 0.5f) / size - 0.5f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha(u, v))));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static float Inside(float signedDistance)
    {
        return -signedDistance / 0.04f;
    }

    private static float Box(float u, float v, float cx, float cy, float hx, float hy)
    {
        return Mathf.Max(Mathf.Abs(u - cx) - hx, Mathf.Abs(v - cy) - hy);
    }

    private static float CrossAlpha(float u, float v)
    {
        return Inside(Mathf.Min(Box(u, v, 0f, 0f, 0.16f, 0.45f), Box(u, v, 0f, 0f, 0.45f, 0.16f)));
    }

    private static float SkullAlpha(float u, float v)
    {
        float head = Mathf.Sqrt(u * u + (v - 0.08f) * (v - 0.08f)) - 0.36f;
        float jaw = Box(u, v, 0f, -0.26f, 0.2f, 0.14f);
        float shape = Mathf.Min(head, jaw);
        float eyeL = 0.09f - Mathf.Sqrt((u + 0.14f) * (u + 0.14f) + (v - 0.08f) * (v - 0.08f));
        float eyeR = 0.09f - Mathf.Sqrt((u - 0.14f) * (u - 0.14f) + (v - 0.08f) * (v - 0.08f));
        float nose = 0.05f - Mathf.Sqrt(u * u + (v + 0.06f) * (v + 0.06f));
        float hole = Mathf.Max(eyeL, Mathf.Max(eyeR, nose));
        return Mathf.Min(Inside(shape), -hole / 0.04f + 0.5f);
    }

    private static float BatAlpha(float u, float v)
    {
        float au = Mathf.Abs(u);
        float body = Box(u, v, 0f, 0f, 0.07f, 0.2f);
        float ears = Box(au, v, 0.05f, 0.24f, 0.03f, 0.06f);
        float top = 0.3f - 0.35f * au;
        float bottom = -0.05f - 0.25f * au + 0.07f * Mathf.Cos(au * 22f);
        float wing = Mathf.Max(au - 0.5f, Mathf.Max(v - top, bottom - v));
        return Inside(Mathf.Min(body, Mathf.Min(ears, wing)));
    }

    private static float PuffAlpha(float u, float v)
    {
        float angle = Mathf.Atan2(v, u);
        float radius = 0.32f + 0.08f * Mathf.Sin(angle * 8f);
        float d = Mathf.Sqrt(u * u + v * v) - radius;
        return Inside(d) * 1.2f;
    }

    private static float RingAlpha(float u, float v)
    {
        float e = Mathf.Sqrt((u / 0.28f) * (u / 0.28f) + (v / 0.42f) * (v / 0.42f));
        float d = Mathf.Abs(e - 0.8f) - 0.22f;
        return Inside(d * 0.3f);
    }

    private static float OneAlpha(float u, float v)
    {
        float bar = Box(u, v, 0f, 0f, 0.07f, 0.4f);
        float flag = Box(u + 0.1f, v - 0.28f + (u + 0.1f) * 0.8f, 0f, 0f, 0.14f, 0.05f);
        float baseBar = Box(u, v, 0f, -0.38f, 0.2f, 0.04f);
        return Inside(Mathf.Min(bar, Mathf.Min(flag, baseBar)));
    }

    private static float MoundAlpha(float u, float v)
    {
        float t = 1f - (u / 0.45f) * (u / 0.45f);
        if (t < 0f) return 0f;
        float top = -0.35f + 0.65f * Mathf.Sqrt(t);
        return Mathf.Min((top - v) / 0.04f, (v + 0.38f) / 0.04f);
    }
}
