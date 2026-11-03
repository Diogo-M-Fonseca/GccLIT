using System.Collections.Generic;
using UnityEngine;

internal sealed class MixedEffect : SpellEffect
{
    private static readonly Dictionary<(SpellMix, SpellEffect, SpellEffect, int), MixedEffect> Cache =
        new Dictionary<(SpellMix, SpellEffect, SpellEffect, int), MixedEffect>();

    private SpellEffect _body;
    private SpellEffect _rider;
    private int _riderPower;
    private bool _everyHit;
    private string _name;
    private SpellMix _mix;
    private int _hit;

    internal static MixedEffect Get(SpellMix mix, SpellEffect body, SpellEffect rider, int riderPower)
    {
        var key = (mix, body, rider, riderPower);
        if (Cache.TryGetValue(key, out MixedEffect found) && found != null) return found;

        MixedEffect m = CreateInstance<MixedEffect>();
        m.hideFlags = HideFlags.HideAndDontSave;
        m._body = body;
        m._rider = rider;
        m._riderPower = riderPower;
        m._everyHit = mix.RiderEveryHit;
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
    internal override bool Reflectable => _body.Reflectable;

    internal override int[] Prepare()
    {
        _hit = 0;
        _rider.Prepare();
        return _body.Prepare();
    }

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        _hit++;

        if (_body.AbsorbsRider)
            return _hit == 1 ? _body.ApplyWithRider(caster, target, power, _rider, _riderPower, _mix) : null;

        string first = _body.Apply(caster, target, power);

        string second = null;
        bool riderTurn = _hit == 1 || _everyHit;
        if (riderTurn && (!target.IsDead || !_rider.Reflectable))
            second = _rider.Apply(caster, target, _riderPower);

        if (string.IsNullOrEmpty(first)) return second;
        if (string.IsNullOrEmpty(second)) return first;
        return $"{first} {second}";
    }
}
