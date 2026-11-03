using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Substitution")]
internal sealed class SubstitutionFilter : LanguageFilter
{
    [SerializeField] private string _from = "ear";
    [SerializeField] private string _to = "ire";
    [Tooltip("Also turns the 'to' text back into the 'from' text (ear <-> ire).")]
    [SerializeField] private bool _bothWays;

    internal override string Transform(string word)
    {
        if (string.IsNullOrEmpty(_from)) return word;
        if (!_bothWays) return word.Replace(_from, _to);
        if (string.IsNullOrEmpty(_to)) return word.Replace(_from, _to);

        var sb = new StringBuilder(word.Length);
        int i = 0;
        while (i < word.Length)
        {
            if (string.CompareOrdinal(word, i, _from, 0, _from.Length) == 0)
            {
                sb.Append(_to);
                i += _from.Length;
            }
            else if (string.CompareOrdinal(word, i, _to, 0, _to.Length) == 0)
            {
                sb.Append(_from);
                i += _to.Length;
            }
            else
            {
                sb.Append(word[i]);
                i++;
            }
        }
        return sb.ToString();
    }
}
