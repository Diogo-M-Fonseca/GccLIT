using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Mirror")]
internal sealed class MirrorEffect : SpellEffect
{
    internal override bool Reflectable => false;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        caster.AddMirror(power);
        string what = power == 1 ? "the next spell" : $"the next {power} spells";
        return $"A mirror shimmers around {caster.DisplayName} (bounces back {what}).";
    }
}
