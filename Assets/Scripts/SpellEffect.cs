using UnityEngine;

internal abstract class SpellEffect : ScriptableObject
{
    [SerializeField] private string _label = "Effect";
    [SerializeField, TextArea] private string _description;

    internal string Label => _label;
    internal string Description => _description;

    internal abstract string Apply(Combatant caster, Combatant target, int power);
}
