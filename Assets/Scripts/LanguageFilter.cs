using UnityEngine;

internal abstract class LanguageFilter : ScriptableObject
{
    [SerializeField] private string _displayName = "Strange tongue";
    [SerializeField, TextArea] private string _hint;

    internal string DisplayName => _displayName;
    internal string Hint => _hint;

    internal abstract string Transform(string word);
}
