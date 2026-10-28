using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal sealed class SpellEntry
{
    [SerializeField] private string _word;
    [SerializeField] private SpellEffect _effect;
    [SerializeField] private int _power = 1;

    internal string Word => _word;
    internal SpellEffect Effect => _effect;
    internal int Power => _power;
}

[CreateAssetMenu(menuName = "SpellTypos/Spell Book")]
internal sealed class SpellBook : ScriptableObject
{
    [SerializeField] private List<SpellEntry> _entries = new List<SpellEntry>();

    private Dictionary<string, SpellEntry> _lookup;

    internal IReadOnlyList<SpellEntry> Entries => _entries;

    internal bool TryGet(string word, out SpellEntry entry)
    {
        if (_lookup == null) Build();
        return _lookup.TryGetValue(word, out entry);
    }

    private void OnValidate() { _lookup = null; }

    private void Build()
    {
        _lookup = new Dictionary<string, SpellEntry>();
        foreach (SpellEntry e in _entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Word)) continue;
            _lookup[e.Word.Trim().ToLowerInvariant()] = e;
        }
    }
}
