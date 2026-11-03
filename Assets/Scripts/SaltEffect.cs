using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Salt")]
internal sealed class SaltEffect : SpellEffect
{
    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        int scars = target.ScarCount;
        if (scars <= 0) return $"{target.DisplayName} has no scars. The salt just bounces off.";

        int lost = target.TakeDamage(scars * power, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows the salt!";

        target.ClearScars();
        return $"Salt pours into {scars} scar{(scars == 1 ? "" : "s")} on {target.DisplayName} ({lost} HP lost). The scars close.";
    }
}
