using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Explosion")]
internal sealed class ExplosionEffect : SpellEffect
{
    internal override bool WinsEvenIfCasterDies => true;
    internal override bool Reflectable => false;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.Kill();
        caster.Kill();
        return $"{caster.DisplayName} detonates! The blast takes {target.DisplayName} down too.";
    }
}
