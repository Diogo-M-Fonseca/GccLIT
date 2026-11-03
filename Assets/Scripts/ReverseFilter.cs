using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Reverse")]
internal sealed class ReverseFilter : LanguageFilter
{
    internal override string Transform(string word)
    {
        if (word.Length < 2) return word;
        char[] chars = word.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}
