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

    private Transform _visual;
    private Vector3 _home;
    private Color _baseColor;
    private int _lastHp = -1;
    private float _hitTimer;
    private float _alpha = 1f;

    private void Awake()
    {
        _visual = _sprite.transform;
        _home = _visual.localPosition;
        _baseColor = _sprite.color;
    }

    private void OnEnable() { _combatant.Changed += OnChanged; }
    private void OnDisable() { _combatant.Changed -= OnChanged; }

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
