using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Meteor Shower")]
internal sealed class MeteorShowerEffect : SpellEffect
{
    [Tooltip("How many waves of meteors one cast brings.")]
    [SerializeField] private int _waves = 3;

    internal override int Impacts => Mathf.Max(1, _waves);

    internal override bool Reflectable => false;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        bool spare = MixedEffect.NoSelfDamage || BloodPacts.Active(BloodPerk.SpareSelf);
        int casterLost = 0;
        bool casterAbsorbed = false;
        if (!spare) casterLost = caster.TakeDamage(power, out casterAbsorbed);
        int targetLost = target.TakeDamage(power, out bool targetAbsorbed);

        string casterPart = spare
            ? $"{caster.DisplayName} is spared"
            : casterAbsorbed
                ? $"{caster.DisplayName}'s fur ball swallows a meteor"
                : $"{caster.DisplayName} loses {casterLost} HP";
        string targetPart = targetAbsorbed
            ? $"{target.DisplayName}'s fur ball swallows a meteor"
            : $"{target.DisplayName} loses {targetLost} HP";

        return $"Meteors crash down! {casterPart}, {targetPart}.";
    }
}
