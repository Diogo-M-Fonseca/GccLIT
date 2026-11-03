using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Fear")]
internal sealed class FearEffect : SpellEffect
{
    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyFear(power);
        return $"{target.DisplayName} is gripped by fear ({power} turn{(power == 1 ? "" : "s")})!";
    }
}
