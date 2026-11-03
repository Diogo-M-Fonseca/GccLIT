using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Cut")]
internal sealed class CutEffect : SpellEffect
{
    [Tooltip("Most slashes one line can make. Extra \"cut\" words beyond this are ignored.")]
    [SerializeField] private int _maxHits = 20;

    internal override int MaxRepeats => Mathf.Max(1, _maxHits);

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        if (target.IsDead) return null;

        int lost = target.TakeDamage(power, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows a slash!";
        return $"{caster.DisplayName} slashes {target.DisplayName} ({lost} HP lost).";
    }
}
