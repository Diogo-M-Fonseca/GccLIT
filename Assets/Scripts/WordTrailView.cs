using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal sealed class WordTrailView : MonoBehaviour
{
    [SerializeField] private TMP_Text _wordPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private TMP_Text _resultLabel;
    [SerializeField] private float _stepDelay = 0.7f;
    [SerializeField] private Color _spellColor = new Color(0.45f, 0.95f, 0.5f);
    [SerializeField] private Color _noiseColor = new Color(0.65f, 0.65f, 0.65f);

    private readonly List<GameObject> _spawned = new List<GameObject>();

    internal void Clear()
    {
        foreach (GameObject go in _spawned) Destroy(go);
        _spawned.Clear();
        _resultLabel.text = "";
    }

    internal IEnumerator Play(CastResult result)
    {
        Clear();

        if (result.Effective == null)
        {
            _resultLabel.text = "Not a spell...";
            yield break;
        }

        for (int i = 0; i < result.Steps.Count; i++)
        {
            CastStep step = result.Steps[i];
            TMP_Text t = Instantiate(_wordPrefab, _container);
            _spawned.Add(t.gameObject);

            string arrow = i == 0 ? "" : "<color=#888888>> </color>";
            string sub;
            if (i == 0) sub = "you typed";
            else if (step.IsSpell) sub = $"{step.FilterName}: new spell!";
            else sub = $"{step.FilterName}: just noise";

            t.color = step.IsSpell ? _spellColor : _noiseColor;
            t.text = $"{arrow}{step.Word}\n<size=45%>{sub}</size>";

            _resultLabel.text = $"{step.Effect.Effect.Label} {step.Effect.Power}";
            yield return new WaitForSeconds(_stepDelay);
        }

        SpellEntry final = result.Effective;
        _resultLabel.text = $"Cast: <b>{final.Word}</b> - {final.Effect.Label} {final.Power}";
    }
}
