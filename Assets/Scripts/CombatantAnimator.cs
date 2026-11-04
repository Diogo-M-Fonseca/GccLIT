using UnityEngine;

// Plays the attack animation of a combatant that has an Animator (the wizard).
// Add it to the root of the prefab, next to Combatant. The Animator itself goes on the object
// that has the SpriteRenderer (Visual), because the clips animate that renderer's sprite.
internal sealed class CombatantAnimator : MonoBehaviour
{
    [SerializeField] private Combatant _combatant;
    [Tooltip("Leave empty to use the Animator found on this object or its children.")]
    [SerializeField] private Animator _animator;
    [Tooltip("Name of the Trigger parameter that starts the attack animation.")]
    [SerializeField] private string _attackTrigger = "Attacking";

    private int _attackHash;

    private void Awake()
    {
        if (_combatant == null) _combatant = GetComponent<Combatant>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        _attackHash = Animator.StringToHash(_attackTrigger);
    }

    private void OnEnable()
    {
        if (_combatant != null) _combatant.AttackStarted += OnAttackStarted;
    }

    private void OnDisable()
    {
        if (_combatant != null) _combatant.AttackStarted -= OnAttackStarted;
    }

    private void OnAttackStarted(Combatant target)
    {
        if (_animator == null) return;
        _animator.ResetTrigger(_attackHash);
        _animator.SetTrigger(_attackHash);
    }
}
