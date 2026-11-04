using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Slot Machine")]
internal sealed class SlotMachineEffect : SpellEffect
{
    internal const int SymbolCount = 5;

    [Tooltip("Chance that all three reels match.")]
    [SerializeField, Range(0f, 1f)] private float _jackpotChance = 0.2f;

    [System.NonSerialized] private int[] _reels;
    [System.NonSerialized] private bool _jackpot;

    internal override int[] Prepare()
    {
        _jackpot = Random.value < _jackpotChance || BloodPacts.Active(BloodPerk.SureJackpot);
        _reels = new int[3];

        if (_jackpot)
        {
            int symbol = Random.Range(0, SymbolCount);
            _reels[0] = symbol;
            _reels[1] = symbol;
            _reels[2] = symbol;
        }
        else
        {
            do
            {
                for (int i = 0; i < _reels.Length; i++) _reels[i] = Random.Range(0, SymbolCount);
            }
            while (_reels[0] == _reels[1] && _reels[1] == _reels[2]);
        }

        return _reels;
    }

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        if (_reels == null) Prepare();
        bool jackpot = _jackpot;
        _reels = null;

        if (!jackpot)
            return $"{caster.DisplayName} pulls the lever... no jackpot. Nothing happens!";

        int lost = target.TakeDamage(power, out bool absorbed);
        if (absorbed) return $"JACKPOT! But {target.DisplayName}'s fur ball swallows the coins!";
        return $"JACKPOT! Coins rain on {target.DisplayName} ({lost} HP lost)!";
    }
}
