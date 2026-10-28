using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Damage")]
internal sealed class DamageEffect : SpellEffect
{
    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        int lost = target.TakeDamage(power, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows the hit!";
        return $"{caster.DisplayName} hits {target.DisplayName} ({lost} HP lost).";
    }
}
