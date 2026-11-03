using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Freeze")]
internal sealed class FreezeEffect : SpellEffect
{
    [SerializeField] private int _turns = 5;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyFreeze(_turns);
        return $"{target.DisplayName} is frozen solid for {_turns} turns!";
    }
}
