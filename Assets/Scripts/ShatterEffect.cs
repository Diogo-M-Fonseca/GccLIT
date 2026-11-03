using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Shatter")]
internal sealed class ShatterEffect : SpellEffect
{
    [Tooltip("Damage multiplier when the target is frozen. 1 = no bonus.")]
    [SerializeField] private float _frozenMultiplier = 5f;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        bool frozen = target.IsFrozen;
        int amount = frozen ? Mathf.RoundToInt(power * _frozenMultiplier) : power;

        int lost = target.TakeDamage(amount, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows the shards!";

        if (!frozen)
            return $"{caster.DisplayName} hurls glass shards at {target.DisplayName} ({lost} HP lost). Nothing frozen to shatter.";

        target.Thaw();
        return $"{target.DisplayName} shatters like glass ({lost} HP lost, frozen: x{_frozenMultiplier:0.##}). The ice is gone.";
    }
}
