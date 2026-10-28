using System.Collections.Generic;

internal sealed class CastStep
{
    internal CastStep(string filterName, string word, bool isSpell, SpellEntry effect)
    {
        FilterName = filterName;
        Word = word;
        IsSpell = isSpell;
        Effect = effect;
    }

    internal string FilterName { get; }
    internal string Word { get; }
    internal bool IsSpell { get; }
    internal SpellEntry Effect { get; }
}

internal sealed class CastResult
{
    internal readonly List<CastStep> Steps = new List<CastStep>();
    internal SpellEntry Effective;
}

internal static class CastResolver
{
    internal static string Normalize(string raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim().ToLowerInvariant();
    }

    internal static CastResult Resolve(string typed, SpellBook book, IReadOnlyList<LanguageFilter> filters)
    {
        var result = new CastResult();
        string word = Normalize(typed);
        if (!book.TryGet(word, out SpellEntry current)) return result;

        result.Steps.Add(new CastStep(null, word, true, current));

        foreach (LanguageFilter filter in filters)
        {
            if (filter == null) continue;
            word = filter.Transform(word);
            bool isSpell = book.TryGet(word, out SpellEntry found);
            if (isSpell) current = found;
            result.Steps.Add(new CastStep(filter.DisplayName, word, isSpell, current));
        }

        result.Effective = current;
        return result;
    }
}
