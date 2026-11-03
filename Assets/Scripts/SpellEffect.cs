using UnityEngine;

internal abstract class SpellEffect : ScriptableObject
{
    [SerializeField] private string _label = "Effect";
    [SerializeField, TextArea] private string _description;

    internal virtual string Label => _label;
    internal string Description => _description;

    internal virtual bool WinsRun => false;

    internal virtual bool WinsEvenIfCasterDies => false;

    internal virtual int Impacts => 1;

    internal virtual int MaxRepeats => 1;

    internal virtual bool IgnoresFear => false;

    internal virtual bool Reflectable => true;

    internal virtual int[] Prepare() { return null; }

    internal virtual bool AbsorbsRider => false;

    internal virtual string ApplyWithRider(Combatant caster, Combatant target, int power,
        SpellEffect rider, int riderPower, SpellMix mix)
    {
        return Apply(caster, target, power);
    }

    internal abstract string Apply(Combatant caster, Combatant target, int power);
}
