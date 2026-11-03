using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Echo")]
internal sealed class EchoFilter : LanguageFilter
{
    internal override string Transform(string word)
    {
        if (word.Length < 2) return word;
        return word.Substring(0, word.Length / 2);
    }
}
