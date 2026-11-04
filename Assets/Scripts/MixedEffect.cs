using System.Collections.Generic;
using UnityEngine;

internal sealed class MixedEffect : SpellEffect
{
    private static readonly Dictionary<(SpellMix, SpellEffect, SpellEffect, int), MixedEffect> Cache =
        new Dictionary<(SpellMix, SpellEffect, SpellEffect, int), MixedEffect>();

    internal static readonly HashSet<SpellEffect> Buffed = new HashSet<SpellEffect>();

    internal static bool KeepScars { get; private set; }
    internal static bool NoSelfDamage { get; private set; }

    private SpellEffect _body;
    private SpellEffect _rider;
    private int _riderPower;
    private string _name;
    private SpellMix _mix;
    private int _hit;
    private int _total = 1;

    internal SpellMix Mix => _mix;
    internal SpellEffect Body => _body;
    internal SpellEffect RiderEffect => _rider;

    internal static void ResetRun()
    {
        BloodPacts.Reset();
        Buffed.Clear();
    }

    internal static MixedEffect Get(SpellMix mix, SpellEffect body, SpellEffect rider, int riderPower)
    {
        var key = (mix, body, rider, riderPower);
        if (Cache.TryGetValue(key, out MixedEffect found) && found != null) return found;

        MixedEffect m = CreateInstance<MixedEffect>();
        m.hideFlags = HideFlags.HideAndDontSave;
        m._body = body;
        m._rider = rider;
        m._riderPower = riderPower;
        m._name = mix.Name;
        m._mix = mix;
        Cache[key] = m;
        return m;
    }

    internal override string Label => _name;
    internal override bool WinsRun => _body.WinsRun || _rider.WinsRun;
    internal override bool WinsEvenIfCasterDies => _body.WinsEvenIfCasterDies || _rider.WinsEvenIfCasterDies;
    internal override int Impacts => _body.Impacts;
    internal override int MaxRepeats => _body.MaxRepeats;
    internal override bool IgnoresFear => _body.IgnoresFear && _rider.IgnoresFear;
    internal override bool Reflectable => _body.Reflectable && !_mix.Area;
    internal override bool SelfOnly => _body.SelfOnly && _rider.SelfOnly;

    internal override int[] Prepare()
    {
        _hit = 0;
        _total = 1;
        _rider.Prepare();
        return _body.Prepare();
    }

    internal override void BeginCast(int totalHits)
    {
        _total = Mathf.Max(1, totalHits);
        _body.BeginCast(totalHits);
    }

    private Combatant[] RiderTargets(Combatant caster, Combatant target)
    {
        switch (_mix.Target)
        {
            case MixTarget.Self: return new[] { caster };
            case MixTarget.Enemy: return new[] { target };
            case MixTarget.Everyone: return new[] { caster, target };
            default: return new[] { _rider.SelfOnly ? caster : target };
        }
    }

    private bool RiderDue()
    {
        switch (_mix.Timing)
        {
            case MixTiming.EveryHit: return true;
            case MixTiming.LastHit: return _hit >= _total;
            default: return _hit == 1;
        }
    }

    private string ApplyRider(Combatant caster, Combatant target, int amount)
    {
        if (_mix.Stack) amount = Mathf.Max(1, amount) * _hit;
        amount *= BloodPacts.Factor(_rider);

        string all = null;
        KeepScars = _mix.KeepScars;
        NoSelfDamage = _mix.NoSelfDamage;
        try
        {
            foreach (Combatant who in RiderTargets(caster, target))
            {
                if (who.IsDead && _rider.Reflectable) continue;
                string text;
                if (_rider is CleanseEffect && (_mix.Timing != MixTiming.Once || _mix.ByDamage))
                {
                    int removed = who.CleanseSome(_mix.ByDamage ? Mathf.Max(1, amount) : 1);
                    text = removed > 0
                        ? $"{who.DisplayName} sheds {removed} status effect{(removed == 1 ? "" : "s")}."
                        : $"{who.DisplayName} has nothing left to cleanse.";
                }
                else text = _rider.SelfOnly ? _rider.Apply(who, who, amount) : _rider.Apply(caster, who, amount);
                if (_mix.Permanent)
                {
                    who.SetPermanent(_rider is BurnEffect, _rider is WetEffect, _rider is FearEffect, _rider is FreezeEffect);
                    if (_rider is PoisonEffect) who.SetPoisonPermanent(true);
                }
                if (!string.IsNullOrEmpty(text)) all = string.IsNullOrEmpty(all) ? text : $"{all} {text}";
            }
        }
        finally { KeepScars = false; NoSelfDamage = false; }
        return all;
    }

    private static string Join(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b;
        if (string.IsNullOrEmpty(b)) return a;
        return $"{a} {b}";
    }

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        _hit++;
        power *= BloodPacts.Factor(_body);

        if (_body.AbsorbsRider)
        {
            if (_hit != 1) return null;
            if (_rider is ExplosionEffect)
            {
                target.Kill();
                return $"{_name}: the familiar blows up and takes {target.DisplayName} with it!";
            }
            if (_mix.IsBlood) return _body.Apply(caster, target, power);
            return _body.ApplyWithRider(caster, target, power, _rider, _riderPower, _mix);
        }

        if (_hit == 1) RegisterHooks(caster, target);

        string pre = null;
        string first = null;
        string second = null;
        bool hooked = _mix.Timing == MixTiming.EveryTick || _mix.Timing == MixTiming.OnReflect;
        bool due = RiderDue() && !hooked;
        if (_mix.RiderVisualOnly) due = false;

        if (_hit == 1 && _mix.PierceShield)
        {
            int broken = target.BreakShield();
            if (broken > 0) pre = $"{target.DisplayName}'s shield shatters ({broken}).";
        }

        if (due && _mix.RiderFirst && !_mix.ByDamage) pre = Join(pre, ApplyRider(caster, target, _riderPower));

        int before = target.Burden;
        int poisonBefore = target.PoisonStacks;
        NoSelfDamage = _mix.NoSelfDamage;
        try
        {
            if (!_mix.BodyVisualOnly && !_mix.Area) first = _body.Apply(caster, target, power);
        }
        finally { NoSelfDamage = false; }
        if (_mix.NoSelfDamage && _body is ExplosionEffect) caster.Survive();
        int dealt = Mathf.Max(0, before - target.Burden);

        if (due && !(_mix.RiderFirst && !_mix.ByDamage))
        {
            int amount = _riderPower;
            bool skip = false;
            if (_mix.ByDamage)
            {
                amount = dealt;
                skip = dealt <= 0;
            }
            bool changed = dealt > 0 || target.PoisonStacks > poisonBefore;
            if (_mix.OnlyIfBodyHit && !changed && !_mix.Area && !_mix.BodyVisualOnly) skip = true;
            if (!skip) second = ApplyRider(caster, target, amount);
        }

        if (_hit == 1 && _mix.ArenaBuff && _rider != null) Buffed.Add(_rider);

        return Join(Join(pre, first), second);
    }

    private void RegisterHooks(Combatant caster, Combatant target)
    {
        if (_mix.RiderVisualOnly) return;
        if (_mix.Timing == MixTiming.EveryTick)
        {
            SpellEffect rider = _rider;
            int amount = _riderPower;
            bool byDamage = _mix.ByDamage;
            target.AddPoisonTickHook(damage => rider.Apply(caster, rider.SelfOnly ? caster : target, byDamage ? damage : amount));
        }
        else if (_mix.Timing == MixTiming.OnReflect)
        {
            SpellEffect rider = _rider;
            int amount = _riderPower;
            bool byDamage = _mix.ByDamage;
            caster.ReflectHook = damage => byDamage && damage <= 0 ? null : rider.Apply(caster, rider.SelfOnly ? caster : target, byDamage ? damage : amount);
        }
    }
}
