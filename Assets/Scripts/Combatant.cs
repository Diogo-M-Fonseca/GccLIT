using System;
using System.Collections.Generic;
using UnityEngine;

internal sealed class Combatant : MonoBehaviour
{
    [SerializeField] private string _displayName = "Wizard";
    [SerializeField] private int _maxHp = 30;
    [Header("Optional visuals (toggled on/off)")]
    [SerializeField] private GameObject _burnVfx;
    [SerializeField] private GameObject _furBallVfx;
    [Tooltip("Where spell animations start and land. Defaults to this object's transform. Must be in world space.")]
    [SerializeField] private Transform _vfxAnchor;

    private int _hp;
    private int _shield;
    private int _burnTurns;
    private int _burnDamage;
    private int _furCharges;
    private int _fearTurns;
    private int _wetTurns;
    private int _freezeTurns;
    private int _mirrorCharges;
    private int _poisonStacks;

    private int _familiarHp;
    private int _familiarMaxHp;
    private int _familiarPower;
    private SpellEffect _familiarEffect;
    private Color _familiarTint = Color.white;
    private bool _familiarRainbow;
    private string _familiarName = "Familiar";
    private string _familiarNote;

    private int _damageMultiplier = 1;

    internal event Action Changed;
    internal event Action FamiliarChanged;

    internal string DisplayName => _displayName;
    internal int Hp => _hp;
    internal int MaxHp => _maxHp;
    internal int Shield => _shield;
    internal int BurnTurns => _burnTurns;
    internal int FurCharges => _furCharges;
    internal int FearTurns => _fearTurns;
    internal int WetTurns => _wetTurns;
    internal int FreezeTurns => _freezeTurns;
    internal int MirrorCharges => _mirrorCharges;
    internal int PoisonStacks => _poisonStacks;
    internal bool IsFeared => _fearTurns > 0;
    internal bool IsWet => _wetTurns > 0;
    internal bool IsPoisoned => _poisonStacks > 0;
    internal bool IsFrozen => _freezeTurns > 0;
    internal bool IsDead => _hp <= 0;
    internal bool HasFamiliar => _familiarHp > 0;
    internal int FamiliarHp => _familiarHp;
    internal int FamiliarMaxHp => _familiarMaxHp;
    internal int FamiliarPower => _familiarPower;
    internal SpellEffect FamiliarEffect => _familiarEffect;
    internal Color FamiliarTint => _familiarTint;
    internal bool FamiliarRainbow => _familiarRainbow;
    internal string FamiliarName => _familiarName;

    internal int DamageMultiplier => _damageMultiplier;
    internal int IncomingBoost { get; set; } = 1;

    internal Transform VfxPoint => _vfxAnchor != null ? _vfxAnchor : transform;

    private sealed class Scar
    {
        internal SpriteRenderer[] Parts;
        internal float[] Alphas;
    }

    private const int MaxScars = 40;
    private readonly List<Scar> _scars = new List<Scar>();
    private float _scarAlpha = 1f;

    internal Transform ScarParent
    {
        get
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.transform : transform;
        }
    }

    internal int ScarCount => _scars.Count;

    internal void AddScar(params SpriteRenderer[] parts)
    {
        var scar = new Scar { Parts = parts, Alphas = new float[parts.Length] };
        for (int i = 0; i < parts.Length; i++) scar.Alphas[i] = parts[i].color.a;
        _scars.Add(scar);

        while (_scars.Count > MaxScars)
        {
            DestroyScar(_scars[0]);
            _scars.RemoveAt(0);
        }
    }

    internal void ClearScars()
    {
        foreach (Scar scar in _scars) DestroyScar(scar);
        _scars.Clear();
        _scarAlpha = 1f;
    }

    internal void SetScarsAlpha(float alpha)
    {
        if (Mathf.Approximately(alpha, _scarAlpha)) return;
        _scarAlpha = alpha;

        foreach (Scar scar in _scars)
        {
            for (int i = 0; i < scar.Parts.Length; i++)
            {
                if (scar.Parts[i] == null) continue;
                Color c = scar.Parts[i].color;
                c.a = scar.Alphas[i] * alpha;
                scar.Parts[i].color = c;
            }
        }
    }

    private static void DestroyScar(Scar scar)
    {
        foreach (SpriteRenderer part in scar.Parts)
            if (part != null) Destroy(part.gameObject);
    }

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
        _damageMultiplier = 1;
        IncomingBoost = 1;
        ClearFamiliar();
        ClearScars();
        ClearStatuses();
    }

    internal void ClearStatuses()
    {
        _burnTurns = 0;
        _burnDamage = 0;
        _furCharges = 0;
        _mirrorCharges = 0;
        _fearTurns = 0;
        _wetTurns = 0;
        _freezeTurns = 0;
        _poisonStacks = 0;
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

        amount *= Mathf.Max(1, IncomingBoost);

        if (_familiarHp > 0)
        {
            int taken = Mathf.Min(_familiarHp, amount);
            _familiarHp -= taken;
            amount -= taken;
            string note = _familiarHp <= 0
                ? $"{_familiarName} falls protecting {_displayName}!"
                : $"{_familiarName} takes {taken} ({_familiarHp}/{_familiarMaxHp} HP left).";
            _familiarNote = _familiarNote == null ? note : $"{_familiarNote} {note}";
            if (_familiarHp <= 0) _familiarEffect = null;
            FamiliarChanged?.Invoke();
            if (amount <= 0)
            {
                Notify();
                return 0;
            }
        }

        int soaked = Mathf.Min(_shield, amount);
        _shield -= soaked;
        int lost = amount - soaked;
        _hp = Mathf.Max(0, _hp - lost);
        Notify();
        return lost;
    }

    internal string TakeFamiliarNote()
    {
        string note = _familiarNote;
        _familiarNote = null;
        return note;
    }

    internal void SummonFamiliar(int hp, SpellEffect effect, int power, Color tint, bool rainbow, string name)
    {
        _familiarMaxHp = Mathf.Max(1, hp);
        _familiarHp = _familiarMaxHp;
        _familiarEffect = effect;
        _familiarPower = power;
        _familiarTint = tint;
        _familiarRainbow = rainbow;
        _familiarName = string.IsNullOrEmpty(name) ? "Familiar" : name;
        _familiarNote = null;
        FamiliarChanged?.Invoke();
        Notify();
    }

    internal void KillFamiliar()
    {
        if (_familiarHp <= 0) return;
        _familiarHp = 0;
        _familiarEffect = null;
        FamiliarChanged?.Invoke();
        Notify();
    }

    private void ClearFamiliar()
    {
        _familiarHp = 0;
        _familiarMaxHp = 0;
        _familiarEffect = null;
        _familiarNote = null;
        FamiliarChanged?.Invoke();
    }

    internal void LoseHp(int amount)
    {
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        Notify();
    }

    internal void DoubleDamage()
    {
        _damageMultiplier *= 2;
        Notify();
    }

    internal void ResetDamageMultiplier()
    {
        _damageMultiplier = 1;
        Notify();
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
        _burnDamage = Mathf.Max(_burnDamage, damagePerTurn * Mathf.Max(1, IncomingBoost));
        Notify();
    }

    internal void AddFurBall(int charges)
    {
        _furCharges += charges;
        Notify();
    }

    internal void ApplyPoison(int stacks)
    {
        _poisonStacks += Mathf.Max(0, stacks) * Mathf.Max(1, IncomingBoost);
        Notify();
    }

    internal void AddMirror(int charges)
    {
        _mirrorCharges += charges;
        Notify();
    }

    internal bool ConsumeMirror()
    {
        if (_mirrorCharges <= 0) return false;
        _mirrorCharges--;
        Notify();
        return true;
    }

    internal void ApplyFear(int turns)
    {
        _fearTurns = Mathf.Max(_fearTurns, turns);
        Notify();
    }

    internal void ApplyWet(int turns)
    {
        _wetTurns = Mathf.Max(_wetTurns, turns);
        Notify();
    }

    internal void ApplyFreeze(int turns)
    {
        _freezeTurns = Mathf.Max(_freezeTurns, turns);
        Notify();
    }

    internal bool ConsumeFreeze()
    {
        if (_freezeTurns <= 0) return false;
        _freezeTurns--;
        Notify();
        return true;
    }

    internal bool Cleanse()
    {
        bool hadAny = _burnTurns > 0 || _fearTurns > 0 || _wetTurns > 0 || _freezeTurns > 0
                      || _furCharges > 0 || _mirrorCharges > 0 || _poisonStacks > 0;
        ClearStatuses();
        return hadAny;
    }

    internal bool Thaw()
    {
        if (_freezeTurns <= 0) return false;
        _freezeTurns = 0;
        Notify();
        return true;
    }

    internal void Kill()
    {
        _hp = 0;
        Notify();
    }

    internal bool ConsumeFear()
    {
        if (_fearTurns <= 0) return false;
        _fearTurns--;
        Notify();
        return true;
    }

    internal string StartTurn(bool tickPoison = true)
    {
        bool changed = false;
        if (_wetTurns > 0)
        {
            _wetTurns--;
            changed = true;
        }

        string msg = null;
        if (_burnTurns > 0)
        {
            _burnTurns--;
            _hp = Mathf.Max(0, _hp - _burnDamage);
            msg = $"{_displayName} burns for {_burnDamage}.";
            if (_burnTurns == 0) _burnDamage = 0;
            changed = true;
        }

        if (tickPoison && _poisonStacks > 0 && _hp > 0)
        {
            int damage = _poisonStacks;
            _hp = Mathf.Max(0, _hp - damage);
            _poisonStacks--;
            string poison = $"{_displayName} suffers {damage} poison damage.";
            msg = msg == null ? poison : $"{msg} {poison}";
            changed = true;
        }

        if (changed) Notify();
        return msg;
    }

    private void Notify()
    {
        if (_burnVfx != null) _burnVfx.SetActive(_burnTurns > 0);
        if (_furBallVfx != null) _furBallVfx.SetActive(_furCharges > 0);
        Changed?.Invoke();
    }
}
