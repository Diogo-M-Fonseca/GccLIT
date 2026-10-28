using UnityEngine;
[CreateAssetMenu(menuName = "SpellTypos/Effects/Burn")]
internal sealed class BurnEffect : SpellEffect
{
    [SerializeField] private int _turns = 3;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyBurn(_turns, power);
        return $"{target.DisplayName} catches fire ({power}/turn for {_turns} turns)!";
    }
}
