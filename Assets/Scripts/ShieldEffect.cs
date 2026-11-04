using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Shield")]
internal sealed class ShieldEffect : SpellEffect
{
    internal override bool Reflectable => false;

    internal override bool SelfOnly => true;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        caster.AddShield(power);
        return $"{caster.DisplayName} gains {power} shield.";
    }
}
