using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Heal")]
internal sealed class HealEffect : SpellEffect
{
    internal override bool Reflectable => false;

    internal override bool SelfOnly => true;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        if (BloodPacts.Active(BloodPerk.FullHeal)) power = caster.MaxHp;
        caster.Heal(power);
        return $"{caster.DisplayName} heals {power}.";
    }
}
