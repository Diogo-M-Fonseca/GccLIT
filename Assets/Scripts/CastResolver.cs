using System;
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
    internal bool Fizzled;
    internal int Repeats = 1;
    internal SpellEntry MixRider;
}

internal static class CastResolver
{
    internal static string Normalize(string raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim().ToLowerInvariant();
    }

    internal static CastResult ResolveMix(string line, SpellBook book, IReadOnlyList<LanguageFilter> filters)
    {
        if (book == null || book.Mixes.Count == 0) return null;
        string normalized = Normalize(line);
        if (normalized.Length == 0 || book.TryGet(normalized, out _)) return null;

        string[] words = normalized.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2) return null;

        var parts = new List<SpellEntry>();
        var finalWords = new List<string>();
        foreach (string w in words)
        {
            CastResult r = Resolve(w, book, filters);
            if (r.Fizzled || r.Effective == null || r.Effective.Effect == null) return null;
            parts.Add(r.Effective);
            finalWords.Add(r.Steps[r.Steps.Count - 1].Word);
        }

        SpellEffect a = parts[0].Effect;
        SpellEffect b = null;
        foreach (SpellEntry p in parts)
        {
            if (p.Effect == a) continue;
            if (b == null) b = p.Effect;
            else if (p.Effect != b) return null;
        }
        if (b == null || !book.TryGetMix(a, b, out SpellMix mix)) return null;

        SpellEntry body = null, rider = null;
        int bodyCount = 0, riderCount = 0;
        foreach (SpellEntry p in parts)
        {
            if (p.Effect == mix.Body) { bodyCount++; if (body == null) body = p; }
            else { riderCount++; if (rider == null) rider = p; }
        }
        if (riderCount != 1) return null;
        if (bodyCount > 1 && body.Effect.MaxRepeats <= 1) return null;

        SpellEntry mixed = SpellEntry.CreateMix(mix, body, rider);
        var result = new CastResult { Effective = mixed, MixRider = rider };
        result.Repeats = Math.Min(bodyCount, Math.Max(1, body.Effect.MaxRepeats));
        result.Steps.Add(new CastStep(null, normalized, true, mixed));
        string changed = string.Join(" ", finalWords);
        if (changed != normalized) result.Steps.Add(new CastStep("Tongue", changed, true, mixed));
        return result;
    }

    internal static CastResult Resolve(string typed, SpellBook book, IReadOnlyList<LanguageFilter> filters)
    {
        var result = new CastResult();
        string word = Normalize(typed);

        bool lastIsSpell = book.TryGet(word, out SpellEntry current);
        result.Steps.Add(new CastStep(null, word, lastIsSpell, current));

        foreach (LanguageFilter filter in filters)
        {
            if (filter == null) continue;
            word = filter.Transform(word);
            bool isSpell = book.TryGet(word, out SpellEntry found);
            if (isSpell) current = found;
            lastIsSpell = isSpell;
            result.Steps.Add(new CastStep(filter.DisplayName, word, isSpell, current));
        }

        result.Fizzled = !lastIsSpell;
        result.Effective = lastIsSpell ? current : null;
        return result;
    }
}
