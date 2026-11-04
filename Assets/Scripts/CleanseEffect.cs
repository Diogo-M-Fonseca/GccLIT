using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Cleanse")]
internal sealed class CleanseEffect : SpellEffect
{
    internal override bool Reflectable => false;

    internal override bool IgnoresFear => true;

    internal override bool SelfOnly => true;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        return caster.Cleanse()
            ? $"{caster.DisplayName} is cleansed of every status effect."
            : $"{caster.DisplayName} is already clean. The spell fizzles harmlessly.";
    }
}
