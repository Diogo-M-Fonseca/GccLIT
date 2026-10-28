using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Filters/Substitution")]
internal sealed class SubstitutionFilter : LanguageFilter
{
    [SerializeField] private string _from = "ear";
    [SerializeField] private string _to = "ire";

    internal override string Transform(string word)
    {
        return string.IsNullOrEmpty(_from) ? word : word.Replace(_from, _to);
    }
}
