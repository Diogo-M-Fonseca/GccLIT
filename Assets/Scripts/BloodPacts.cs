using System.Collections.Generic;
using UnityEngine;

internal enum BloodPerk
{
    None,
    DoubleAll,
    Boost,
    FullHeal,
    StatusImmune,
    SpareSelf,
    PierceShield,
    LongGit,
    SureJackpot,
    StrongFamiliar,
    FearKills,
    WaterKills,
    SelfPoison,
    IceHeal,
    CastTwice,
    RainbowStart,
    ShieldEachTurn,
    AutoShatter,
    KeepScars
}

internal static class BloodPacts
{
    private static readonly Dictionary<BloodPerk, SpellEffect> Granted = new Dictionary<BloodPerk, SpellEffect>();
    private static int _doubles;

    internal const int FearLimit = 10;
    internal const int WaterLimit = 10;

    internal static int PermanentDoubles => _doubles;

    internal static void Reset()
    {
        Granted.Clear();
        _doubles = 0;
    }

    internal static bool Active(BloodPerk perk)
    {
        return Granted.ContainsKey(perk);
    }

    internal static SpellEffect SourceOf(BloodPerk perk)
    {
        Granted.TryGetValue(perk, out SpellEffect source);
        return source;
    }

    internal static void Grant(BloodPerk perk, SpellEffect source, Combatant player)
    {
        if (perk == BloodPerk.None || Granted.ContainsKey(perk)) return;
        Granted[perk] = source;

        switch (perk)
        {
            case BloodPerk.DoubleAll:
                _doubles++;
                player.DoubleDamage();
                break;
            case BloodPerk.StatusImmune:
                player.StatusImmune = true;
                player.ClearStatuses();
                break;
            case BloodPerk.StrongFamiliar:
                player.BloodFamiliar = true;
                break;
        }
    }

    internal static int Factor(SpellEffect effect)
    {
        if (effect == null || !Active(BloodPerk.Boost)) return 1;
        SpellEffect source = SourceOf(BloodPerk.Boost);
        return effect == source ? 2 : 1;
    }

    internal static string Describe(BloodPerk perk)
    {
        switch (perk)
        {
            case BloodPerk.DoubleAll: return "All your damage is doubled for the rest of the run.";
            case BloodPerk.Boost: return "It grows huge and hits twice as hard, in every variation, for the rest of the run.";
            case BloodPerk.FullHeal: return "Healing restores your whole life, in every variation, for the rest of the run.";
            case BloodPerk.StatusImmune: return "You are immune to status effects for the rest of the run.";
            case BloodPerk.SpareSelf: return "It never harms you again.";
            case BloodPerk.PierceShield: return "Every spell you cast now ignores the enemy's shield.";
            case BloodPerk.LongGit: return "Git is twice as long, with twice the hits, and the number becomes 171004042026.";
            case BloodPerk.SureJackpot: return "Gamble, in every variation, always hits the jackpot.";
            case BloodPerk.StrongFamiliar: return "Your familiars have double life and double damage.";
            case BloodPerk.FearKills: return $"An enemy with {FearLimit} or more fear dies of fright.";
            case BloodPerk.WaterKills: return $"An enemy with {WaterLimit} or more wet drowns.";
            case BloodPerk.SelfPoison: return "Everything the enemy does poisons the enemy.";
            case BloodPerk.IceHeal: return "Casting ice freezes you in blood ice and heals you; you thaw at the end of the turn.";
            case BloodPerk.CastTwice: return "Every spell you cast is cast twice.";
            case BloodPerk.RainbowStart: return "Whenever the enemy starts its turn, rainbow strikes it.";
            case BloodPerk.ShieldEachTurn: return "You gain a shield every turn.";
            case BloodPerk.AutoShatter: return "A frozen enemy shatters on its own, without costing you a turn.";
            case BloodPerk.KeepScars: return "Salt and poop no longer spend scars.";
            default: return "";
        }
    }
}
