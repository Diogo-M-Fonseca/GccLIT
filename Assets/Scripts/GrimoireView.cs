using TMPro;
using UnityEngine;

internal sealed class GrimoireView : MonoBehaviour
{
    [SerializeField] private SpellBook _book;
    [SerializeField] private TMP_Text _rowPrefab;
    [SerializeField] private Transform _container;

    private void Start()
    {
        foreach (SpellEntry e in _book.Entries)
        {
            if (e == null || e.Effect == null) continue;
            TMP_Text row = Instantiate(_rowPrefab, _container);
            row.text = $"<b>{e.Word}</b>  <color=#AAAAAA>{e.Effect.Label} {e.Power}</color>";
        }
    }
}
