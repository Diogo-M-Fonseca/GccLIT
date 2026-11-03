using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Heal")]
internal sealed class HealEffect : SpellEffect
{
    internal override bool Reflectable => false;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        caster.Heal(power);
        return $"{caster.DisplayName} heals {power}.";
    }
}
