using System;
using System.Collections.Generic;
using UnityEngine;

internal enum SpellSoundFit
{
    FitToAnimation,
    FitAndLoop,
    Natural
}

[Serializable]
internal sealed class SpellEntry
{
    [SerializeField] private string _word;
    [SerializeField] private SpellEffect _effect;
    [SerializeField] private int _power = 1;
    [Tooltip("Animation played when this spell is cast.")]
    [SerializeField] private SpellVfxKind _vfx = SpellVfxKind.None;
    [Tooltip("How many times the player can cast this spell per run. 0 = unlimited. Spent spells are crossed out in the grimoire.")]
    [SerializeField] private int _maxUses = 0;
    [Tooltip("Sound played when this spell is cast (by the player or the enemy). Empty = silent.")]
    [SerializeField] private AudioClip _sound;
    [SerializeField, Range(0f, 1f)] private float _soundVolume = 1f;
    [Tooltip("Off: the sound plays once when the cast starts. On: it plays on every hit instead (cut cut cut, git, meteor waves...).")]
    [SerializeField] private bool _soundPerHit;
    [Tooltip("How the sound is matched to the animation: FitToAnimation (speeds the clip up or slows it down so it ends with the animation), FitAndLoop (same, but a short clip repeats) or Natural (plays as it is). The gamble spell always loops its background sound.")]
    [SerializeField] private SpellSoundFit _soundFit = SpellSoundFit.FitToAnimation;
    [Tooltip("Seconds into the clip where its hit happens (the boom, the zap, the splash). 0 = the clip has no marked hit. The clip is started early or late enough that this moment lines up with the moment the animation lands.")]
    [SerializeField, Min(0f)] private float _soundImpactTime;
    [Tooltip("Gamble only: played every time a reel locks. Short and percussive works best; the pitch rises with each reel.")]
    [SerializeField] private AudioClip _reelStopSound;
    [SerializeField, Range(0f, 1f)] private float _reelStopVolume = 1f;
    [Tooltip("Gamble only: played on a jackpot together with the last reel stop. It is faded out when the animation ends.")]
    [SerializeField] private AudioClip _jackpotSound;
    [SerializeField, Range(0f, 1f)] private float _jackpotVolume = 1f;

    [NonSerialized] private SpellMix _mix;
    [NonSerialized] private SpellEntry _riderEntry;
    [NonSerialized] private string _displayName;

    internal string Word => _word;
    internal string DisplayName => string.IsNullOrEmpty(_displayName) ? _word : _displayName;
    internal SpellMix Mix => _mix;
    internal SpellEntry RiderEntry => _riderEntry;
    internal int MaxUses => _maxUses;
    internal SpellEffect Effect => _effect;
    internal int Power => _power;
    internal SpellVfxKind Vfx => _vfx;
    internal AudioClip Sound => _sound;
    internal float SoundVolume => _soundVolume;
    internal bool SoundPerHit => _soundPerHit;
    internal SpellSoundFit SoundFit => _soundFit;
    internal float SoundImpactTime => _soundImpactTime;
    internal AudioClip ReelStopSound => _reelStopSound;
    internal float ReelStopVolume => _reelStopVolume;
    internal AudioClip JackpotSound => _jackpotSound;
    internal float JackpotVolume => _jackpotVolume;

    internal static SpellEntry CreateMix(SpellMix mix, SpellEntry body, SpellEntry rider)
    {
        var entry = (SpellEntry)body.MemberwiseClone();
        entry._effect = MixedEffect.Get(mix, body._effect, rider._effect, rider._power);
        entry._mix = mix;
        entry._riderEntry = rider;
        entry._displayName = mix.Name;
        return entry;
    }

    internal string EffectText
    {
        get
        {
            if (_effect == null) return "?";
            return _power == 1 ? _effect.Label : $"{_effect.Label} {_power}";
        }
    }
}

[Serializable]
internal sealed class SpellMix
{
    [Tooltip("The spell that is cast: its animation, sound, power and repeats (cut cut fire = 2 flaming slashes).")]
    [SerializeField] private SpellEffect _body;
    [Tooltip("The spell mixed into the body: its effect is added to the body's and its colour tints the animation.")]
    [SerializeField] private SpellEffect _rider;
    [Tooltip("Name shown when the mix is cast.")]
    [SerializeField] private string _name = "Mix";
    [Tooltip("Colour the animation is tinted with.")]
    [SerializeField] private Color _tint = Color.white;
    [Tooltip("Instead of one colour, the animation cycles through every colour of the rainbow.")]
    [SerializeField] private bool _rainbow;
    [Tooltip("Off: the rider's effect lands once, with the first hit. On: it lands with every hit.")]
    [SerializeField] private bool _riderEveryHit;
    [Tooltip("Also play the rider spell's sound along with the body's.")]
    [SerializeField] private bool _playRiderSound = true;

    internal SpellEffect Body => _body;
    internal SpellEffect Rider => _rider;
    internal string Name => string.IsNullOrEmpty(_name) ? "Mix" : _name;
    internal Color Tint => _tint;
    internal bool Rainbow => _rainbow;
    internal bool RiderEveryHit => _riderEveryHit;
    internal bool PlayRiderSound => _playRiderSound;
}

[CreateAssetMenu(menuName = "SpellTypos/Spell Book")]
internal sealed class SpellBook : ScriptableObject
{
    [SerializeField] private List<SpellEntry> _entries = new List<SpellEntry>();

    [Tooltip("Spell mixes: two spells typed in one line combine into a new one.")]
    [SerializeField] private List<SpellMix> _mixes = new List<SpellMix>();

    private Dictionary<string, SpellEntry> _lookup;

    internal IReadOnlyList<SpellEntry> Entries => _entries;
    internal IReadOnlyList<SpellMix> Mixes => _mixes ?? (_mixes = new List<SpellMix>());

    internal bool TryGetMix(SpellEffect a, SpellEffect b, out SpellMix mix)
    {
        mix = null;
        if (_mixes == null || a == null || b == null) return false;
        foreach (SpellMix m in _mixes)
        {
            if (m == null || m.Body == null || m.Rider == null) continue;
            if ((m.Body == a && m.Rider == b) || (m.Body == b && m.Rider == a))
            {
                mix = m;
                return true;
            }
        }
        return false;
    }

    internal bool TryGet(string word, out SpellEntry entry)
    {
        if (_lookup == null) Build();
        return _lookup.TryGetValue(word, out entry);
    }

    private void OnValidate() { _lookup = null; }

    private void Build()
    {
        _lookup = new Dictionary<string, SpellEntry>();
        foreach (SpellEntry e in _entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Word)) continue;
            _lookup[e.Word.Trim().ToLowerInvariant()] = e;
        }
    }
}
