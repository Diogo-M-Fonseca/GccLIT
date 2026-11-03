using UnityEngine;

[CreateAssetMenu(menuName = "SpellTypos/Effects/Poop")]
internal sealed class PoopEffect : SpellEffect
{
    [Tooltip("How many scars it takes to get one group of poison stacks.")]
    [SerializeField] private int _scarsPerStack = 2;

    internal override string Apply(Combatant caster, Combatant target, int power)
    {
        int perGroup = Mathf.Max(1, _scarsPerStack);
        int scars = target.ScarCount;
        int stacks = (scars / perGroup) * power;

        if (stacks <= 0)
            return $"{target.DisplayName} has too few scars ({scars}). The poop just sits there and stinks.";

        target.ApplyPoison(stacks);
        return $"Filth seeps into {scars} scars: {target.DisplayName} gets {stacks} poison ({target.PoisonStacks} in total)!";
    }
}
