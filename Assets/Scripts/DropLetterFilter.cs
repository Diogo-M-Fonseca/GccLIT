using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Drop Letter")]
internal sealed class DropLetterFilter : LanguageFilter
{
    [SerializeField] private int _index = -1;

    internal override string Transform(string word)
    {
        if (word.Length <= 1) return word;
        int i = _index < 0 ? word.Length + _index : _index;
        if (i < 0 || i >= word.Length) return word;
        return word.Remove(i, 1);
    }
}
