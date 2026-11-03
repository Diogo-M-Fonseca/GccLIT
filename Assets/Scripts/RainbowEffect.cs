using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Rainbow")]
internal sealed class RainbowEffect : SpellEffect
{
    [SerializeField] private int _burnTurns = 3;
    [SerializeField] private int _fearTurns = 2;
    [SerializeField] private int _wetTurns = 4;
    [SerializeField] private int _freezeTurns = 2;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.ApplyBurn(_burnTurns, power);
        target.ApplyFear(_fearTurns);
        target.ApplyWet(_wetTurns);
        target.ApplyFreeze(_freezeTurns);
        return $"A rainbow crashes into {target.DisplayName}: burning, terrified, soaked and frozen!";
    }
}
