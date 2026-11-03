using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Damage")]
internal sealed class DamageEffect : SpellEffect
{
    [Tooltip("Damage multiplier when the target is wet. 1 = no bonus.")]
    [SerializeField] private float _wetMultiplier = 1f;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        bool soaked = _wetMultiplier > 1f && target.IsWet;
        int amount = soaked ? Mathf.RoundToInt(power * _wetMultiplier) : power;

        int lost = target.TakeDamage(amount, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows the hit!";
        if (soaked) return $"{caster.DisplayName} hits {target.DisplayName} ({lost} HP lost, soaked: x{_wetMultiplier:0.##}).";
        return $"{caster.DisplayName} hits {target.DisplayName} ({lost} HP lost).";
    }
}
