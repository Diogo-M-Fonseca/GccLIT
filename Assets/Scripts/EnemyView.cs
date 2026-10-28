using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
internal sealed class EnemyView : MonoBehaviour
{
    [SerializeField] private Combatant _combatant;
    [SerializeField] private Image _image;
    [SerializeField] private float _bobHeight = 14f;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _hitTime = 0.25f;
    [SerializeField] private float _shakeSize = 16f;
    [SerializeField] private Color _hitColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private Color _burnColor = new Color(1f, 0.65f, 0.3f);

    private RectTransform _rect;
    private CanvasGroup _group;
    private Vector2 _home;
    private Color _baseColor;
    private int _lastHp = -1;
    private float _hitTimer;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _group = GetComponent<CanvasGroup>();
        _home = _rect.anchoredPosition;
        _baseColor = _image.color;
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
        Vector2 offset = new Vector2(0f, Mathf.Sin(Time.time * _bobSpeed) * _bobHeight);

        float hit = 0f;
        if (_hitTimer > 0f)
        {
            _hitTimer -= Time.deltaTime;
            hit = Mathf.Clamp01(_hitTimer / _hitTime);
            offset += Random.insideUnitCircle * _shakeSize * hit;
        }
        _rect.anchoredPosition = _home + offset;

        Color c = _baseColor;
        if (_combatant.BurnTurns > 0) c = Color.Lerp(c, _burnColor, 0.6f);
        _image.color = Color.Lerp(c, _hitColor, hit);

        float targetAlpha = _combatant.IsDead ? 0f : 1f;
        _group.alpha = Mathf.MoveTowards(_group.alpha, targetAlpha, 3f * Time.deltaTime);
    }
}
