using System;
using UnityEngine;

internal sealed class Combatant : MonoBehaviour
{
    [SerializeField] private string _displayName = "Wizard";
    [SerializeField] private int _maxHp = 30;
    [Header("Optional visuals (toggled on/off)")]
    [SerializeField] private GameObject _burnVfx;
    [SerializeField] private GameObject _furBallVfx;

    private int _hp;
    private int _shield;
    private int _burnTurns;
    private int _burnDamage;
    private int _furCharges;

    internal event Action Changed;

    internal string DisplayName => _displayName;
    internal int Hp => _hp;
    internal int MaxHp => _maxHp;
    internal int Shield => _shield;
    internal int BurnTurns => _burnTurns;
    internal int FurCharges => _furCharges;
    internal bool IsDead => _hp <= 0;

    private void Awake()
    {
        ResetFull(_maxHp);
    }

    internal void ResetFull(int maxHp, string displayName = null)
    {
        _maxHp = maxHp;
        _hp = maxHp;
        _shield = 0;
        if (!string.IsNullOrEmpty(displayName)) _displayName = displayName;
        ClearStatuses();
    }

    internal void ClearStatuses()
    {
        _burnTurns = 0;
        _burnDamage = 0;
        _furCharges = 0;
        Notify();
    }

    internal int TakeDamage(int amount, out bool absorbedByFur)
    {
        absorbedByFur = false;
        if (_furCharges > 0)
        {
            _furCharges--;
            absorbedByFur = true;
            Notify();
            return 0;
        }

        int soaked = Mathf.Min(_shield, amount);
        _shield -= soaked;
        int lost = amount - soaked;
        _hp = Mathf.Max(0, _hp - lost);
        Notify();
        return lost;
    }

    internal void Heal(int amount)
    {
        _hp = Mathf.Min(_maxHp, _hp + amount);
        Notify();
    }

    internal void AddShield(int amount)
    {
        _shield += amount;
        Notify();
    }

    internal void ApplyBurn(int turns, int damagePerTurn)
    {
        _burnTurns = Mathf.Max(_burnTurns, turns);
        _burnDamage = Mathf.Max(_burnDamage, damagePerTurn);
        Notify();
    }

    internal void AddFurBall(int charges)
    {
        _furCharges += charges;
        Notify();
    }

    internal string StartTurn()
    {
        if (_burnTurns <= 0) return null;
        _burnTurns--;
        _hp = Mathf.Max(0, _hp - _burnDamage);
        string msg = $"{_displayName} burns for {_burnDamage}.";
        if (_burnTurns == 0) _burnDamage = 0;
        Notify();
        return msg;
    }

    private void Notify()
    {
        if (_burnVfx != null) _burnVfx.SetActive(_burnTurns > 0);
        if (_furBallVfx != null) _furBallVfx.SetActive(_furCharges > 0);
        Changed?.Invoke();
    }
}
