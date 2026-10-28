using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Swap Letters")]
internal sealed class SwapLettersFilter : LanguageFilter
{
    [SerializeField] private int _a = 0;
    [SerializeField] private int _b = 1;

    internal override string Transform(string word)
    {
        int a = _a < 0 ? word.Length + _a : _a;
        int b = _b < 0 ? word.Length + _b : _b;
        if (a < 0 || b < 0 || a >= word.Length || b >= word.Length || a == b) return word;
        char[] chars = word.ToCharArray();
        (chars[a], chars[b]) = (chars[b], chars[a]);
        return new string(chars);
    }
}
