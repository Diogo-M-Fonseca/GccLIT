using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Familiar")]
internal sealed class FamiliarEffect : SpellEffect
{
    internal override bool Reflectable => false;
    internal override bool IgnoresFear => true;
    internal override bool AbsorbsRider => true;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        caster.SummonFamiliar(power, null, 0, new Color(1f, 0.9f, 0.65f), false, "Familiar");
        return $"A familiar appears at {caster.DisplayName}'s side ({Mathf.Max(1, power)} HP). It takes hits for them.";
    }

    internal override string ApplyWithRider(Combatant caster, Combatant target, int power,
        SpellEffect rider, int riderPower, SpellMix mix)
    {
        string name = mix != null ? mix.Name : "Familiar";
        Color tint = mix != null ? mix.Tint : Color.white;
        bool rainbow = mix != null && mix.Rainbow;

        bool allowed = rider != null && !rider.WinsRun && !rider.WinsEvenIfCasterDies && !rider.AbsorbsRider
                       && !(rider is SacrificeEffect) && !(rider is MixedEffect);
        if (!allowed)
        {
            caster.SummonFamiliar(power, null, 0, tint, rainbow, name);
            return $"{name} appears at {caster.DisplayName}'s side ({Mathf.Max(1, power)} HP), but it cannot use that spell.";
        }

        caster.SummonFamiliar(power, rider, riderPower, tint, rainbow, name);
        return $"{name} appears at {caster.DisplayName}'s side ({Mathf.Max(1, power)} HP). Every turn it casts {rider.Label}!";
    }
}
