using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class SpellInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField _field;
    [SerializeField] private Button _castButton;

    internal event Action<string> Submitted;

    private void OnEnable()
    {
        _field.onSubmit.AddListener(OnFieldSubmit);
        _castButton.onClick.AddListener(OnCastClicked);
    }

    private void OnDisable()
    {
        _field.onSubmit.RemoveListener(OnFieldSubmit);
        _castButton.onClick.RemoveListener(OnCastClicked);
    }

    internal void SetInteractable(bool value)
    {
        _field.interactable = value;
        _castButton.interactable = value;
        if (value) _field.ActivateInputField();
    }

    internal void Clear()
    {
        _field.text = "";
    }

    private void OnFieldSubmit(string _) { Send(); }
    private void OnCastClicked() { Send(); }

    private void Send()
    {
        if (!_field.interactable || string.IsNullOrWhiteSpace(_field.text)) return;
        Submitted?.Invoke(_field.text);
        _field.ActivateInputField();
    }
}
