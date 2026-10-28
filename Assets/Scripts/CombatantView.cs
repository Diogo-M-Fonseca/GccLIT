using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class CombatantView : MonoBehaviour
{
    [SerializeField] private Combatant _combatant;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _shieldText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Slider _hpBar;
    [SerializeField] private float _barSpeed = 2f;

    private float _shownValue;

    private void OnEnable()
    {
        _combatant.Changed += Refresh;
        Refresh();
        _shownValue = _hpBar.value;
    }

    private void OnDisable()
    {
        _combatant.Changed -= Refresh;
    }

    private void Update()
    {
        float target = _combatant.MaxHp > 0 ? (float)_combatant.Hp / _combatant.MaxHp : 0f;
        _shownValue = Mathf.MoveTowards(_shownValue, target, _barSpeed * Time.deltaTime);
        _hpBar.value = _shownValue;
    }

    private void Refresh()
    {
        _nameText.text = _combatant.DisplayName;
        _hpText.text = $"{_combatant.Hp} / {_combatant.MaxHp}";
        _shieldText.text = _combatant.Shield > 0 ? $"Shield {_combatant.Shield}" : "";

        string status = "";
        if (_combatant.BurnTurns > 0) status += $"<color=#FF7A2F>Burn {_combatant.BurnTurns}</color>  ";
        if (_combatant.FurCharges > 0) status += $"<color=#C9A36B>Fur x{_combatant.FurCharges}</color>";
        _statusText.text = status;
    }
}
