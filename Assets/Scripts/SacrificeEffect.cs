using UnityEngine;

internal enum SacrificeKind { Hp, Familiar, Spell }

[CreateAssetMenu(menuName = "SpellTypos/Effects/Sacrifice")]
internal sealed class SacrificeEffect : SpellEffect
{
    [Tooltip("On: the HP cost is multiplied by the damage bonus you already have (power 5: 5, then 10, then 20 HP...), so stacking gets dangerous. Off: it always costs the spell's power.")]
    [SerializeField] private bool _hpCostScalesWithBonus = true;

    [System.NonSerialized] internal SacrificeKind Pending = SacrificeKind.Hp;
    [System.NonSerialized] internal string PendingName;

    [System.NonSerialized] internal bool KilledCaster;

    internal override bool Reflectable => false;
    internal override bool IgnoresFear => true;

    internal int HpCost(Combatant caster, int power)
    {
        int cost = Mathf.Max(1, power);
        return _hpCostScalesWithBonus ? cost * caster.DamageMultiplier : cost;
    }

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        KilledCaster = false;
        string paid;
        switch (Pending)
        {
            case SacrificeKind.Hp:
            {
                int cost = HpCost(caster, power);
                caster.LoseHp(cost);
                KilledCaster = caster.IsDead;
                paid = $"{caster.DisplayName} gives {cost} HP";
                break;
            }
            case SacrificeKind.Familiar:
                paid = $"{caster.DisplayName} sacrifices {caster.FamiliarName}";
                caster.KillFamiliar();
                break;
            default:
                paid = $"{caster.DisplayName} seals \"{PendingName}\" for the rest of the run";
                break;
        }

        if (KilledCaster) return $"{paid}...";

        caster.DoubleDamage();
        return $"{paid}. Damage is now x{caster.DamageMultiplier} for this room!";
    }
}
