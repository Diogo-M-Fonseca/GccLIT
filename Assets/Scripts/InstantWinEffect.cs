using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Instant Win")]
internal sealed class InstantWinEffect : SpellEffect
{
    [Tooltip("When on, defeating the enemy with this spell wins the whole run instead of just the room.")]
    [SerializeField] private bool _winsWholeRun = true;

    internal override bool WinsRun => _winsWholeRun;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        target.Kill();
        return $"{target.DisplayName} is overwhelmed by love. {caster.DisplayName} wins!";
    }
}
