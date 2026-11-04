using UnityEngine;

internal sealed class EnemyView : MonoBehaviour
{
    [SerializeField] private Combatant _combatant;
    [SerializeField] private SpriteRenderer _sprite;
    [Tooltip("Optional: fades together with the sprite when the enemy dies.")]
    [SerializeField] private CanvasGroup _hudGroup;
    [SerializeField] private float _bobHeight = 0.15f;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _hitTime = 0.25f;
    [SerializeField] private float _shakeSize = 0.15f;
    [Tooltip("Constant trembling while the enemy is under the Fear status.")]
    [SerializeField] private float _fearShakeSize = 0.06f;
    [SerializeField] private float _fadeSpeed = 3f;
    [SerializeField] private Color _hitColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private Color _burnColor = new Color(1f, 0.65f, 0.3f);
    [SerializeField] private Color _wetColor = new Color(0.45f, 0.7f, 1f);
    [SerializeField] private Color _poisonColor = new Color(0.55f, 0.9f, 0.35f);
    [Tooltip("Tint while frozen. The enemy also stops bobbing.")]
    [SerializeField] private Color _frozenColor = new Color(0.6f, 0.9f, 1f);

    [Header("Variants")]
    [Tooltip("Sprite of the regular enemy (the green elf). Empty = keep the sprite already on the renderer.")]
    [SerializeField] private Sprite _normalSprite;
    [Tooltip("Sprite of the rare enemy (the purple elf).")]
    [SerializeField] private Sprite _rareSprite;
    [Tooltip("Chance (0 to 1) that a new enemy is the rare variant. 0.05 = 5%.")]
    [SerializeField, Range(0f, 1f)] private float _rareChance = 0.05f;

    [Header("Attack (the elf has a single pose, so the attack is a lunge)")]
    [Tooltip("Total length of the lunge in seconds.")]
    [SerializeField] private float _attackTime = 0.4f;
    [Tooltip("How far the enemy leans back before striking.")]
    [SerializeField] private float _windUpDistance = 0.25f;
    [Tooltip("How far the enemy lunges towards its target.")]
    [SerializeField] private float _lungeDistance = 0.6f;

    private Transform _visual;
    private Vector3 _home;
    private Color _baseColor;
    private int _lastHp = -1;
    private float _hitTimer;
    private float _alpha = 1f;
    private float _attackTimer;
    private Vector2 _attackDir = Vector2.left;

    internal bool IsRare { get; private set; }

    private void Awake()
    {
        _visual = _sprite.transform;
        _home = _visual.localPosition;
        _baseColor = _sprite.color;
    }

    private void OnEnable()
    {
        _combatant.Changed += OnChanged;
        _combatant.AttackStarted += OnAttackStarted;
    }

    private void OnDisable()
    {
        _combatant.Changed -= OnChanged;
        _combatant.AttackStarted -= OnAttackStarted;
    }

    // Picks the sprite for a new enemy. Returns true when the rare variant was chosen.
    internal bool RollVariant()
    {
        IsRare = _rareSprite != null && Random.value < _rareChance;
        Sprite chosen = IsRare ? _rareSprite : _normalSprite;
        if (chosen != null) _sprite.sprite = chosen;
        return IsRare;
    }

    private void OnAttackStarted(Combatant target)
    {
        if (target != null)
        {
            Vector3 toTarget = target.VfxPoint.position - _visual.position;
            if (toTarget.sqrMagnitude > 0.0001f) _attackDir = ((Vector2)toTarget).normalized;
        }
        _attackTimer = _attackTime;
    }

    // Offset along the attack direction: lean back, lunge, recover.
    private float AttackAmount()
    {
        if (_attackTimer <= 0f || _attackTime <= 0f) return 0f;
        float t = 1f - _attackTimer / _attackTime;
        if (t < 0.4f) return Mathf.Lerp(0f, -_windUpDistance, Mathf.SmoothStep(0f, 1f, t / 0.4f));
        if (t < 0.6f) return Mathf.Lerp(-_windUpDistance, _lungeDistance, (t - 0.4f) / 0.2f);
        return Mathf.Lerp(_lungeDistance, 0f, Mathf.SmoothStep(0f, 1f, (t - 0.6f) / 0.4f));
    }

    private void OnChanged()
    {
        if (_lastHp >= 0 && _combatant.Hp < _lastHp) _hitTimer = _hitTime;
        _lastHp = _combatant.Hp;
    }

    private void Update()
    {
        bool frozen = _combatant.IsFrozen && !_combatant.IsDead;

        Vector3 offset = frozen ? Vector3.zero : new Vector3(0f, Mathf.Sin(Time.time * _bobSpeed) * _bobHeight, 0f);

        float hit = 0f;
        if (_hitTimer > 0f)
        {
            _hitTimer -= Time.deltaTime;
            hit = Mathf.Clamp01(_hitTimer / _hitTime);
            offset += (Vector3)(Random.insideUnitCircle * _shakeSize * hit);
        }

        if (_combatant.IsFeared && !_combatant.IsDead && !frozen)
            offset += (Vector3)(Random.insideUnitCircle * _fearShakeSize);

        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
            if (!frozen) offset += (Vector3)(_attackDir * AttackAmount());
        }

        _visual.localPosition = _home + offset;

        Color c = _baseColor;
        if (_combatant.IsWet) c = Color.Lerp(c, _wetColor, 0.4f);
        if (frozen) c = Color.Lerp(c, _frozenColor, 0.7f);
        if (_combatant.BurnTurns > 0) c = Color.Lerp(c, _burnColor, 0.6f);
        if (_combatant.IsPoisoned) c = Color.Lerp(c, _poisonColor, 0.4f);
        c = Color.Lerp(c, _hitColor, hit);

        _alpha = Mathf.MoveTowards(_alpha, _combatant.IsDead ? 0f : 1f, _fadeSpeed * Time.deltaTime);
        c.a = _baseColor.a * _alpha;
        _sprite.color = c;

        if (_hudGroup != null) _hudGroup.alpha = _alpha;
        _combatant.SetScarsAlpha(_alpha);
    }
}
