using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal sealed class GrimoireView : MonoBehaviour
{
    private sealed class Row
    {
        internal TMP_Text Text;
        internal SpellEntry Entry;
    }

    [SerializeField] private SpellBook _book;
    [SerializeField] private TurnBattle _battle;
    [SerializeField] private TMP_Text _rowPrefab;
    [SerializeField] private Transform _container;

    private readonly HashSet<string> _known = new HashSet<string>();
    private readonly Dictionary<string, Row> _rows = new Dictionary<string, Row>();
    private TMP_Text _emptyHint;
    private bool _ready;

    private void Awake()
    {
        if (_battle == null) _battle = FindFirstObjectByType<TurnBattle>();
    }

    private void OnEnable()
    {
        if (_battle == null) return;
        _battle.SpellWritten += OnSpellWritten;
        _battle.UsesChanged += RefreshRows;
    }

    private void OnDisable()
    {
        if (_battle == null) return;
        _battle.SpellWritten -= OnSpellWritten;
        _battle.UsesChanged -= RefreshRows;
    }

    private void Start()
    {
        if (_rowPrefab == null || _container == null || _book == null || _battle == null)
        {
            Debug.LogError("[SpellTypos] GrimoireView: missing Row Prefab, Container, Spell Book or Battle.", this);
            return;
        }

        _emptyHint = Instantiate(_rowPrefab, _container);
        _emptyHint.text = "<i><color=#AAAAAA>Nothing written yet.\nCast a spell to learn it.</color></i>";
        _ready = true;
    }

    private void OnSpellWritten(string word)
    {
        if (!_ready || !_known.Add(word)) return;
        if (!_book.TryGet(word, out SpellEntry e) || e.Effect == null) return;

        if (_emptyHint != null)
        {
            Destroy(_emptyHint.gameObject);
            _emptyHint = null;
        }

        TMP_Text text = Instantiate(_rowPrefab, _container);
        var row = new Row { Text = text, Entry = e };
        _rows[word] = row;
        text.text = RowText(e);
    }

    private void RefreshRows()
    {
        foreach (Row row in _rows.Values)
            row.Text.text = RowText(row.Entry);
    }

    private string RowText(SpellEntry e)
    {
        string info = e.EffectText;
        if (_battle.IsSealed(e)) return $"<s><color=#777777>{e.Word}  {info}</color></s>  <color=#FF6666>sealed</color>";
        if (e.MaxUses <= 0) return $"<b>{e.Word}</b>  <color=#AAAAAA>{info}</color>";

        int left = _battle.UsesLeft(e);
        if (left <= 0) return $"<s><color=#777777>{e.Word}  {info}</color></s>  <color=#FF6666>spent</color>";
        return $"<b>{e.Word}</b>  <color=#AAAAAA>{info}  ({left} left)</color>";
    }
}
