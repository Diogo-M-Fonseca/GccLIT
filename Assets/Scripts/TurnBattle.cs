using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal sealed class TurnBattle : MonoBehaviour
{
    [SerializeField] private SpellBook _book;
    [SerializeField] private Combatant _player;
    [SerializeField] private Combatant _enemy;
    [SerializeField] private SpellInput _input;
    [SerializeField] private BattleHud _hud;
    [Tooltip("Optional: created automatically at runtime if left empty.")]
    [SerializeField] private SpellVfxPlayer _vfx;
    [Tooltip("Optional: created automatically at runtime if left empty.")]
    [SerializeField] private SpellAudioPlayer _audio;
    [SerializeField] private float _beatTime = 0.6f;

    private RoomData _room;
    private bool _busy;

    private bool _awaitingChoice;
    private string _choiceBuffer;

    private sealed class SacrificeChoice
    {
        internal bool Done;
        internal bool Cancelled;
        internal SacrificeKind Kind;
        internal string Name;
        internal SpellEntry Spell;
    }

    private readonly List<SpellEntry> _sealed = new List<SpellEntry>();

    internal bool SacrificeDeath { get; private set; }

    internal event Action<bool> Ended;

    internal bool WinsRun { get; private set; }

    internal bool PyrrhicWin { get; private set; }

    internal event Action<string> SpellWritten;

    internal event Action UsesChanged;

    private readonly Dictionary<string, int> _uses = new Dictionary<string, int>();

    private static bool SameSpell(SpellEntry a, SpellEntry b)
    {
        return a.Effect == b.Effect && a.Power == b.Power && a.Vfx == b.Vfx;
    }

    internal bool IsSealed(SpellEntry entry)
    {
        foreach (SpellEntry s in _sealed)
            if (SameSpell(s, entry)) return true;
        return false;
    }

    private void Seal(SpellEntry entry)
    {
        if (IsSealed(entry)) return;
        _sealed.Add(entry);
        UsesChanged?.Invoke();
    }

    internal int UsesLeft(SpellEntry entry)
    {
        if (IsSealed(entry)) return 0;
        if (entry.MaxUses <= 0) return int.MaxValue;
        _uses.TryGetValue(CastResolver.Normalize(entry.Word), out int used);
        return Mathf.Max(0, entry.MaxUses - used);
    }

    internal bool IsExhausted(SpellEntry entry)
    {
        return IsSealed(entry) || (entry.MaxUses > 0 && UsesLeft(entry) <= 0);
    }

    internal void ResetUses()
    {
        _uses.Clear();
        _sealed.Clear();
        UsesChanged?.Invoke();
    }

    private void ConsumeUse(SpellEntry entry)
    {
        if (entry.MaxUses <= 0) return;
        string key = CastResolver.Normalize(entry.Word);
        _uses.TryGetValue(key, out int used);
        _uses[key] = used + 1;
        UsesChanged?.Invoke();
    }

    private void Awake()
    {
        if (_vfx == null) _vfx = FindFirstObjectByType<SpellVfxPlayer>();
        if (_vfx == null) _vfx = new GameObject("SpellVfxPlayer").AddComponent<SpellVfxPlayer>();

        if (_audio == null) _audio = FindFirstObjectByType<SpellAudioPlayer>();
        if (_audio == null) _audio = new GameObject("SpellAudioPlayer").AddComponent<SpellAudioPlayer>();
    }

    private void OnEnable()
    {
        _input.Submitted += OnSubmitted;
        _player.FamiliarChanged += OnPlayerFamiliarChanged;
        _enemy.FamiliarChanged += OnEnemyFamiliarChanged;
    }

    private void OnDisable()
    {
        _input.Submitted -= OnSubmitted;
        _player.FamiliarChanged -= OnPlayerFamiliarChanged;
        _enemy.FamiliarChanged -= OnEnemyFamiliarChanged;
    }

    private void OnPlayerFamiliarChanged() { if (_vfx != null) _vfx.SyncPet(_player, _enemy); }
    private void OnEnemyFamiliarChanged() { if (_vfx != null) _vfx.SyncPet(_enemy, _player); }

    internal void Begin(RoomData room)
    {
        _room = room;
        _busy = false;
        _audio.StopAll();
        _awaitingChoice = false;
        SacrificeDeath = false;
        _player.ResetDamageMultiplier();
        WinsRun = false;
        PyrrhicWin = false;
        _enemy.ResetFull(room.EnemyHp, room.EnemyName);
        if (_vfx != null) _vfx.SyncPet(_player, _enemy);
        _hud.ShowRoom(room);
        _hud.Log($"A {room.EnemyName} appears!");
        _input.Clear();
        _input.SetInteractable(true);
    }

    private void OnSubmitted(string typed)
    {
        if (_awaitingChoice)
        {
            string answer = CastResolver.Normalize(typed);
            if (string.IsNullOrEmpty(answer)) return;
            _input.Clear();
            _choiceBuffer = answer;
            return;
        }

        if (_busy || _room == null) return;

        string word = CastResolver.Normalize(typed);
        if (string.IsNullOrEmpty(word)) return;

        if (_book.TryGet(word, out _)) SpellWritten?.Invoke(word);
        else
            foreach (string part in word.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                if (_book.TryGet(part, out _)) SpellWritten?.Invoke(part);

        _input.Clear();

        CastResult result = ResolveRepeats(word)
            ?? CastResolver.ResolveMix(word, _book, _room.Filters)
            ?? CastResolver.Resolve(word, _book, _room.Filters);
        StartCoroutine(RunTurn(result));
    }

    private CastResult ResolveRepeats(string line)
    {
        string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2) return null;

        CastResult first = null;
        foreach (string w in words)
        {
            CastResult r = CastResolver.Resolve(w, _book, _room.Filters);
            if (r.Fizzled || r.Effective == null || r.Effective.Effect == null) return null;
            if (r.Effective.Effect.MaxRepeats <= 1) return null;
            if (first == null) first = r;
            else if (r.Effective.Effect != first.Effective.Effect || r.Effective.Power != first.Effective.Power)
                return null;
        }

        first.Repeats = Mathf.Min(words.Length, first.Effective.Effect.MaxRepeats);
        return first;
    }

    private IEnumerator RunTurn(CastResult result)
    {
        _busy = true;
        _input.SetInteractable(false);

        yield return _hud.PlayTrail(result);

        SpellEntry cast = result.Effective;
        if (_player.ConsumeFreeze())
        {
            _hud.Log(FrozenMessage(_player));
        }
        else if (result.Fizzled)
        {
            string lost = result.Steps[result.Steps.Count - 1].Word;
            _hud.Log($"\"{lost}\" is not a spell. Nothing happens and you lose your turn!");
        }
        else if (cast != null && cast.Effect != null)
        {
            if (IsExhausted(cast) || (result.MixRider != null && IsExhausted(result.MixRider)))
            {
                SpellEntry blocked = IsExhausted(cast) ? cast : result.MixRider;
                _hud.Log(IsSealed(blocked)
                    ? $"\"{blocked.Word}\" was sealed by your sacrifice. Nothing happens and you lose your turn!"
                    : $"\"{blocked.Word}\" is used up. Nothing happens and you lose your turn!");
            }
            else if (!cast.Effect.IgnoresFear && _player.ConsumeFear())
            {
                _hud.Log(MissMessage(_player));
            }
            else
            {
                ConsumeUse(cast);
                if (result.MixRider != null) ConsumeUse(result.MixRider);

                SacrificeChoice choice = null;
                if (cast.Effect is SacrificeEffect sacrifice)
                {
                    choice = new SacrificeChoice();
                    yield return AskSacrifice(sacrifice, cast, choice);
                    if (choice.Cancelled)
                        _hud.Log("You hold back the offering. The ritual fades and your turn is lost.");
                    else
                    {
                        sacrifice.Pending = choice.Kind;
                        sacrifice.PendingName = choice.Name;
                        if (choice.Spell != null) Seal(choice.Spell);
                    }
                }

                if (choice == null || !choice.Cancelled)
                {
                    yield return PlayVfx(cast, _player, _enemy,
                        () => _hud.Log(ApplySpell(cast, _player, _enemy)), result.Repeats);

                    if (cast.Effect is SacrificeEffect paid && paid.KilledCaster && _player.IsDead)
                    {
                        SacrificeDeath = true;
                        _hud.Log("You offered your very last drop of life... and the ritual takes it all.");
                    }
                }

                if (cast.Effect.WinsRun && _enemy.IsDead) WinsRun = true;

                if (cast.Effect.WinsEvenIfCasterDies && _enemy.IsDead && _player.IsDead)
                {
                    PyrrhicWin = true;
                    _hud.Log("You win, but at what cost...");
                }
            }
        }

        yield return new WaitForSeconds(_beatTime);

        if (PyrrhicWin) { Finish(true); yield break; }
        if (_player.IsDead) { Finish(false); yield break; }
        if (_enemy.IsDead) { Finish(true); yield break; }

        yield return PetTurn(_player, _enemy);
        if (_player.IsDead) { Finish(false); yield break; }
        if (_enemy.IsDead) { Finish(true); yield break; }

        string burn = _enemy.StartTurn();
        if (burn != null) { _hud.Log(burn); yield return new WaitForSeconds(_beatTime); }
        if (_enemy.IsDead) { Finish(true); yield break; }

        yield return EnemyCast();
        yield return new WaitForSeconds(_beatTime);
        if (_player.IsDead) { Finish(false); yield break; }
        if (_enemy.IsDead) { Finish(true); yield break; }

        yield return PetTurn(_enemy, _player);
        if (_player.IsDead) { Finish(false); yield break; }
        if (_enemy.IsDead) { Finish(true); yield break; }

        burn = _player.StartTurn();
        if (burn != null) { _hud.Log(burn); yield return new WaitForSeconds(_beatTime); }
        if (_player.IsDead) { Finish(false); yield break; }

        _busy = false;
        _input.SetInteractable(true);
    }

    private IEnumerator EnemyCast()
    {
        if (_enemy.ConsumeFreeze())
        {
            _hud.Log(FrozenMessage(_enemy));
            yield break;
        }

        SpellEntry entry = null;
        string word = null;
        if (_room.EnemyWords.Count > 0)
        {
            word = CastResolver.Normalize(_room.EnemyWords[UnityEngine.Random.Range(0, _room.EnemyWords.Count)]);
            if (!_book.TryGet(word, out entry))
            {
                CastResult mix = CastResolver.ResolveMix(word, _book, System.Array.Empty<LanguageFilter>());
                if (mix != null) entry = mix.Effective;
            }
        }

        bool ignoresFear = entry != null && entry.Effect != null && entry.Effect.IgnoresFear;
        if (!ignoresFear && _enemy.ConsumeFear())
        {
            _hud.Log(MissMessage(_enemy));
            yield break;
        }

        if (entry == null || entry.Effect == null) yield break;

        _hud.Log($"{_enemy.DisplayName} casts \"{word}\"!");
        yield return PlayVfx(entry, _enemy, _player,
            () => _hud.Log(ApplySpell(entry, _enemy, _player)));
    }

    private IEnumerator PlayVfx(SpellEntry entry, Combatant caster, Combatant target, Action onImpact, int repeats = 1)
    {
        int hits = Mathf.Max(1, entry.Effect.Impacts) * Mathf.Max(1, repeats);
        int landed = 0;

        int[] vfxData = entry.Effect.Prepare();

        bool animated = _vfx != null && entry.Vfx != SpellVfxKind.None;
        SpellVfxTiming timing = animated ? _vfx.GetTiming(entry.Vfx, hits, vfxData) : default(SpellVfxTiming);

        if (!entry.SoundPerHit) _audio.PlayCast(entry, timing);
        if (entry.Mix != null && entry.Mix.PlayRiderSound && entry.RiderEntry != null)
            _audio.PlayCast(entry.RiderEntry, timing);

        void Land()
        {
            landed++;
            if (entry.SoundPerHit) _audio.PlayHit(entry, timing, hits);
            onImpact();
        }

        if (animated)
        {
            if (entry.Mix != null) _vfx.SetTint(entry.Mix.Tint, entry.Mix.Rainbow);
            yield return _vfx.Play(entry.Vfx, caster, target, Land, hits, vfxData,
                (cue, index) => _audio.PlayCue(entry, cue, index, timing));
            _vfx.ClearTint();
        }

        while (landed < hits)
        {
            if (landed > 0) yield return new WaitForSeconds(_beatTime * 0.5f);
            Land();
        }
    }

    private string ApplySpell(SpellEntry entry, Combatant caster, Combatant target)
    {
        return ApplyEffect(entry.Effect, entry.Power, caster, target);
    }

    private string ApplyEffect(SpellEffect effect, int power, Combatant caster, Combatant target)
    {
        Combatant victim = target;
        if (effect.Reflectable && target.ConsumeMirror())
        {
            _hud.Log($"{target.DisplayName}'s mirror bounces the spell back!");
            victim = caster;
        }

        victim.IncomingBoost = caster.DamageMultiplier;
        string message;
        try { message = effect.Apply(caster, victim, power); }
        finally { victim.IncomingBoost = 1; }

        string notes = victim.TakeFamiliarNote();
        if (caster != victim)
        {
            string own = caster.TakeFamiliarNote();
            if (!string.IsNullOrEmpty(own)) notes = string.IsNullOrEmpty(notes) ? own : $"{notes} {own}";
        }
        if (string.IsNullOrEmpty(notes)) return message;
        return string.IsNullOrEmpty(message) ? notes : $"{message} {notes}";
    }

    private IEnumerator PetTurn(Combatant owner, Combatant foe)
    {
        if (!owner.HasFamiliar || owner.FamiliarEffect == null || owner.IsDead || foe.IsDead) yield break;

        SpellEffect effect = owner.FamiliarEffect;
        int power = owner.FamiliarPower;
        string name = owner.FamiliarName;

        if (_vfx != null) yield return _vfx.PetAttack(owner, foe);
        effect.Prepare();
        string message = ApplyEffect(effect, power, owner, foe);
        if (!string.IsNullOrEmpty(message)) _hud.Log($"{name}: {message}");
        yield return new WaitForSeconds(_beatTime * 0.5f);
    }

    private IEnumerator AskSacrifice(SacrificeEffect sacrifice, SpellEntry cast, SacrificeChoice choice)
    {
        int cost = sacrifice.HpCost(_player, cast.Power);
        string deadly = cost >= _player.Hp ? " - it would kill you" : "";
        string pet = _player.HasFamiliar ? " / familiar" : "";
        _hud.Log($"Sacrifice what? Type: hp ({cost} HP{deadly}){pet} / a spell word (sealed for the whole run) / cancel");

        _awaitingChoice = true;
        _input.SetInteractable(true);

        while (!choice.Done)
        {
            _choiceBuffer = null;
            while (_choiceBuffer == null) yield return null;
            string answer = _choiceBuffer;

            if (answer == "cancel" || answer == "back")
            {
                choice.Cancelled = true;
                choice.Done = true;
            }
            else if (answer == "hp" || answer == "life" || answer == "health" || answer == "blood")
            {
                choice.Kind = SacrificeKind.Hp;
                choice.Name = "HP";
                choice.Done = true;
            }
            else if (answer == "familiar" || answer == "pet")
            {
                if (!_player.HasFamiliar) _hud.Log("You have no familiar to sacrifice.");
                else
                {
                    choice.Kind = SacrificeKind.Familiar;
                    choice.Name = _player.FamiliarName;
                    choice.Done = true;
                }
            }
            else if (_book.TryGet(answer, out SpellEntry spell) && spell.Effect != null)
            {
                if (spell.Effect == cast.Effect) _hud.Log("The ritual cannot sacrifice itself.");
                else if (IsSealed(spell)) _hud.Log($"\"{answer}\" is already sealed.");
                else if (IsExhausted(spell)) _hud.Log($"\"{answer}\" is already used up.");
                else
                {
                    choice.Kind = SacrificeKind.Spell;
                    choice.Name = spell.Word;
                    choice.Spell = spell;
                    choice.Done = true;
                }
            }
            else _hud.Log($"\"{answer}\" is not something you can offer. Type hp, familiar, a spell word or cancel.");
        }

        _awaitingChoice = false;
        _input.SetInteractable(false);
    }

    private static string MissMessage(Combatant who)
    {
        return $"The {who.DisplayName} missed due to overwhelming fear.";
    }

    private static string FrozenMessage(Combatant who)
    {
        return who.IsFrozen
            ? $"{who.DisplayName} is frozen solid and can't act ({who.FreezeTurns} more)."
            : $"{who.DisplayName} is frozen solid and can't act. The ice finally cracks.";
    }

    private void Finish(bool playerWon)
    {
        _busy = true;
        _input.SetInteractable(false);
        Ended?.Invoke(playerWon);
    }
}
