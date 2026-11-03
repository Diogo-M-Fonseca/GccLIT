using System.Collections.Generic;
using UnityEngine;

internal sealed class SpellAudioPlayer : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;

    [Header("Fitting a clip to its animation")]
    [Tooltip("Slowest a clip may be played to stretch it over a long animation (1 = normal speed). Lower sounds deeper and slower.")]
    [SerializeField, Range(0.25f, 1f)] private float _minPitch = 0.6f;
    [Tooltip("Fastest a clip may be played to squeeze it into a short animation (1 = normal speed). Higher sounds squeakier and quicker.")]
    [SerializeField, Range(1f, 4f)] private float _maxPitch = 1.6f;
    [Tooltip("Seconds of fade at the start of every sound (avoids clicks).")]
    [SerializeField] private float _fadeInTime = 0.02f;
    [Tooltip("Seconds of fade when a sound is cut off because the animation ended.")]
    [SerializeField] private float _fadeOutTime = 0.15f;
    [Tooltip("Hit sounds closer together than this many seconds are skipped (stops a burst of hits from becoming a single loud blast).")]
    [SerializeField] private float _hitSoundGap = 0.03f;

    [Header("Gamble (slot machine)")]
    [Tooltip("Pitch of the reel stop sound for reel 1, 2 and 3, in semitones. 0, 4, 7 climbs a major chord.")]
    [SerializeField] private float[] _reelStopSemitones = { 0f, 4f, 7f };
    [Tooltip("Pitch of the jackpot sound in semitones. Tune it until it sounds right with the reel stop sound.")]
    [SerializeField] private float _jackpotSemitones = 0f;

    [Tooltip("Most sounds that can play at once. The oldest one is cut when this is reached.")]
    [SerializeField, Min(1)] private int _maxVoices = 24;

    private struct Plan
    {
        internal float Pitch;
        internal float Delay;
        internal bool Loop;
        internal float Limit;
        internal bool Cut;
    }

    private sealed class Voice
    {
        internal AudioSource Source;
        internal bool Active;
        internal bool Started;
        internal float StartAt;
        internal float Limit;
        internal bool Cut;
        internal float Volume;
    }

    private readonly List<Voice> _voices = new List<Voice>();
    private float _lastHitTime = -10f;

    internal void PlayCast(SpellEntry entry, SpellVfxTiming timing)
    {
        if (entry == null || entry.Sound == null) return;

        SpellSoundFit mode = entry.SoundFit;
        if (entry.Vfx == SpellVfxKind.Slots && mode == SpellSoundFit.FitToAnimation) mode = SpellSoundFit.FitAndLoop;

        Plan plan = BuildPlan(entry.Sound, mode, timing.SoundSpan, entry.SoundImpactTime, timing.FirstImpact);
        Begin(entry.Sound, entry.SoundVolume * _masterVolume, plan);
    }

    internal void PlayHit(SpellEntry entry, SpellVfxTiming timing, int hits)
    {
        if (entry == null || entry.Sound == null) return;

        float now = Time.time;
        if (now - _lastHitTime < _hitSoundGap) return;
        _lastHitTime = now;

        float interval = 0f;
        if (timing.Duration > 0f)
        {
            float afterFirst = timing.Duration - timing.FirstImpact;
            interval = hits > 1 ? afterFirst / (hits - 1) : afterFirst;
        }

        Plan plan = BuildPlan(entry.Sound, entry.SoundFit, interval, 0f, 0f);
        plan.Loop = false;
        Begin(entry.Sound, entry.SoundVolume * _masterVolume, plan);
    }

    internal void PlayCue(SpellEntry entry, SpellCue cue, int index, SpellVfxTiming timing)
    {
        if (entry == null) return;

        switch (cue)
        {
            case SpellCue.ReelStop:
            {
                AudioClip clip = entry.ReelStopSound;
                if (clip == null) return;

                float semitones = 0f;
                if (_reelStopSemitones != null && _reelStopSemitones.Length > 0)
                    semitones = _reelStopSemitones[Mathf.Clamp(index, 0, _reelStopSemitones.Length - 1)];

                Begin(clip, entry.ReelStopVolume * _masterVolume, new Plan { Pitch = SemitonesToPitch(semitones) });
                break;
            }
            case SpellCue.Jackpot:
            {
                AudioClip clip = entry.JackpotSound;
                if (clip == null) return;

                var plan = new Plan { Pitch = SemitonesToPitch(_jackpotSemitones) };
                float room = timing.FinaleSpan;
                if (room > 0.05f && clip.length / plan.Pitch > room + 0.02f)
                {
                    plan.Limit = room;
                    plan.Cut = true;
                }
                Begin(clip, entry.JackpotVolume * _masterVolume, plan);
                break;
            }
        }
    }

    internal void StopAll()
    {
        foreach (Voice v in _voices)
            if (v.Active) Release(v);
    }

    private static float SemitonesToPitch(float semitones)
    {
        return Mathf.Pow(2f, semitones / 12f);
    }

    private Plan BuildPlan(AudioClip clip, SpellSoundFit mode, float span, float clipHit, float animHit)
    {
        var plan = new Plan { Pitch = 1f };
        float length = clip.length;
        bool hasHit = clipHit > 0f && clipHit < length && animHit > 0f;

        if (mode == SpellSoundFit.Natural || span <= 0.05f)
        {
            if (hasHit) plan.Delay = Mathf.Max(0f, animHit - clipHit);
            return plan;
        }

        float ratio = length / span;

        if (mode == SpellSoundFit.FitAndLoop && ratio < 1f)
        {
            plan.Loop = true;
            plan.Limit = span;
            plan.Cut = true;
            return plan;
        }

        float pitch = Mathf.Clamp(ratio, _minPitch, _maxPitch);
        float delay = 0f;

        if (hasHit && span > animHit)
        {
            pitch = Mathf.Clamp((length - clipHit) / (span - animHit), _minPitch, _maxPitch);
            delay = animHit - clipHit / pitch;

            if (delay < 0f)
            {
                pitch = Mathf.Clamp(clipHit / animHit, _minPitch, _maxPitch);
                delay = Mathf.Max(0f, animHit - clipHit / pitch);
            }
        }

        plan.Pitch = pitch;
        plan.Delay = delay;

        float room = span - delay;
        if (length / pitch > room + 0.02f)
        {
            plan.Limit = Mathf.Max(0.05f, room);
            plan.Cut = true;
        }
        return plan;
    }

    private Voice Acquire()
    {
        foreach (Voice v in _voices)
            if (!v.Active) return v;

        if (_voices.Count < Mathf.Max(1, _maxVoices))
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            var created = new Voice { Source = source };
            _voices.Add(created);
            return created;
        }

        Voice oldest = _voices[0];
        foreach (Voice v in _voices)
            if (v.StartAt < oldest.StartAt) oldest = v;
        Release(oldest);
        return oldest;
    }

    private void Begin(AudioClip clip, float volume, Plan plan)
    {
        Voice v = Acquire();
        AudioSource s = v.Source;
        s.Stop();
        s.clip = clip;
        s.pitch = plan.Pitch;
        s.loop = plan.Loop;
        s.volume = _fadeInTime > 0f ? 0f : volume;

        v.Active = true;
        v.Started = false;
        v.StartAt = Time.time + plan.Delay;
        v.Limit = plan.Limit;
        v.Cut = plan.Cut;
        v.Volume = volume;

        if (plan.Delay <= 0f)
        {
            s.Play();
            v.Started = true;
        }
    }

    private static void Release(Voice v)
    {
        v.Source.Stop();
        v.Source.clip = null;
        v.Active = false;
    }

    private void Update()
    {
        float now = Time.time;
        foreach (Voice v in _voices)
        {
            if (!v.Active) continue;

            float t = now - v.StartAt;
            if (!v.Started)
            {
                if (t < 0f) continue;
                v.Source.Play();
                v.Started = true;
            }

            if (v.Limit > 0f && t >= v.Limit)
            {
                Release(v);
                continue;
            }

            if (!v.Source.isPlaying && t > 0.05f)
            {
                Release(v);
                continue;
            }

            float gain = _fadeInTime > 0f ? Mathf.Clamp01(t / _fadeInTime) : 1f;
            if (v.Cut && v.Limit > 0f)
            {
                float fade = Mathf.Min(_fadeOutTime, v.Limit * 0.5f);
                if (fade > 0f) gain *= Mathf.Clamp01((v.Limit - t) / fade);
            }
            v.Source.volume = v.Volume * gain;
        }
    }

    private void OnDisable()
    {
        StopAll();
    }
}
