using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Wet")]
internal sealed class WetEffect : SpellEffect
{
    [SerializeField] private int _turns = 4;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyWet(_turns);
        return $"{target.DisplayName} is soaked!";
    }
}
