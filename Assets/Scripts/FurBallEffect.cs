using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Fur Ball")]
internal sealed class FurBallEffect : SpellEffect
{
    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        caster.AddFurBall(power);
        return $"A fur ball puffs up around {caster.DisplayName} (blocks {power} hit{(power == 1 ? "" : "s")}).";
    }
}
