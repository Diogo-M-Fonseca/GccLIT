using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Poison")]
internal sealed class PoisonEffect : SpellEffect
{
    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyPoison(power);
        int total = target.PoisonStacks;
        return $"{target.DisplayName} is poisoned ({total} stack{(total == 1 ? "" : "s")})!";
    }
}
