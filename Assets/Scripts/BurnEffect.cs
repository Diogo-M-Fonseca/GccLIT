using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Burn")]
internal sealed class BurnEffect : SpellEffect
{
    [SerializeField] private int _turns = 3;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        string thaw = target.Thaw() ? $"{target.DisplayName} thaws out! " : "";

        target.ApplyBurn(_turns, power);

        string tick = target.StartTurn(false);
        string msg = $"{thaw}{target.DisplayName} catches fire ({power}/turn for {_turns} turns)!";
        return tick == null ? msg : $"{msg} {tick}";
    }
}
