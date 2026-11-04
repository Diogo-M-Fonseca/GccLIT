using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Git")]
internal sealed class GitEffect : SpellEffect
{
    [Tooltip("The number whose binary code is hurled at the target. Every 1 bit hits, every 0 bit misses, and a run of 1s hits harder and harder.")]
    [SerializeField] private int _number = 1710;

    [System.NonSerialized] private int[] _streaks;
    [System.NonSerialized] private int _next;

    internal override int Impacts => Mathf.Max(1, CountOnes());

    internal override int[] Prepare()
    {
        string text = System.Convert.ToString(Number, 2);
        var bits = new int[text.Length];
        _streaks = new int[CountOnes()];
        _next = 0;

        int run = 0;
        int one = 0;
        for (int i = 0; i < text.Length; i++)
        {
            bool isOne = text[i] == '1';
            bits[i] = isOne ? 1 : 0;
            run = isOne ? run + 1 : 0;
            if (isOne) _streaks[one++] = run;
        }
        return bits;
    }

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        if (target.IsDead) return null;

        if (_streaks == null) Prepare();
        int streak = _next < _streaks.Length ? _streaks[_next] : 1;
        _next++;

        int lost = target.TakeDamage(power * streak, out bool absorbed);
        if (absorbed) return $"{target.DisplayName}'s fur ball swallows a 1!";
        if (streak > 1) return $"A 1 hits {target.DisplayName} (x{streak} streak, {lost} HP lost).";
        return $"A 1 hits {target.DisplayName} ({lost} HP lost).";
    }

    private long Number => BloodPacts.Active(BloodPerk.LongGit) ? 171004042026L : Mathf.Max(1, _number);

    private int CountOnes()
    {
        long n = Number;
        int ones = 0;
        while (n > 0)
        {
            ones += (int)(n & 1);
            n >>= 1;
        }
        return ones;
    }
}
