using System;
using UnityEngine;

namespace HuntrX.Gameplay.Audio
{
    public enum AudioCue
    {
        Attack = 0,
        Hit = 1,
        Dash = 2,
        Parry = 3,
        Enemy = 4,
        Rescue = 5,
        Honmoon = 6,
        RumiBladeSlash1 = 7,
        RumiBladeSlash2 = 8,
        RumiBladeSlash3 = 9,
        RumiTaekwondoKick = 10,
        ZoeyShinkalSlash = 11,
        ZoeyShinkalThrow = 12,
        ZoeyLegChokeholdSlam = 13,
        MiraGokdoSweep = 14,
        MiraBrawlerPunch = 15,
        MiraCoffeePotClang = 16,
        SaberChargeLoop = 17,
        SaberChargeRelease = 18,
        EnergyTankTick = 19,
        SubTankFull = 20,
        BossWarningSiren = 21
    }

    public enum AudioLayer { Exploration, Combat, Combo, Crowd, Crisis, Special }

    /// <summary>Bounded reusable voices, Capcom-grade DSP blade/CQC synthesis, and 6-layer synchronized K-Pop music.</summary>
    [DisallowMultipleComponent]
    public sealed class GameAudioManager2D : MonoBehaviour
    {
        private const int CueCount = 22;
        [SerializeField] private AudioClip[] cues = new AudioClip[CueCount];
        [SerializeField] private AudioClip[] layers = new AudioClip[6];
        [SerializeField, Range(1, 32)] private int voiceCount = 16;
        [SerializeField, Range(0, 1)] private float masterVolume = .72f;
        [SerializeField, Min(.01f)] private float fadeSpeed = 2.5f;

        private static AudioClip[] sharedSynthesizedCues;
        private static AudioClip[] sharedSynthesizedLayers;

        public float MasterVolume => masterVolume;

        public bool TrySetMasterVolume(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return false;
            masterVolume = Mathf.Clamp01(value);
            if (dialogue != null) dialogue.volume = masterVolume;
            return true;
        }

        private AudioSource[] voices, music;
        private AudioSource dialogue;
        private readonly float[] targets = new float[6];

        private void Awake()
        {
            EnsureSynthesizedAudioBank();
            if (cues == null || cues.Length < CueCount)
            {
                var expanded = new AudioClip[CueCount];
                if (cues != null) Array.Copy(cues, expanded, Mathf.Min(cues.Length, CueCount));
                cues = expanded;
            }
            for (int i = 0; i < CueCount; i++)
            {
                if (cues[i] == null) cues[i] = sharedSynthesizedCues[i];
            }

            if (layers == null || layers.Length < 6)
            {
                var expandedLayers = new AudioClip[6];
                if (layers != null) Array.Copy(layers, expandedLayers, Mathf.Min(layers.Length, 6));
                layers = expandedLayers;
            }
            for (int i = 0; i < 6; i++)
            {
                if (layers[i] == null) layers[i] = sharedSynthesizedLayers[i];
            }

            voices = new AudioSource[Mathf.Clamp(voiceCount, 1, 32)];
            music = new AudioSource[6];
            for (int i = 0; i < voices.Length; i++) voices[i] = Create(false);
            for (int i = 0; i < 6; i++)
            {
                music[i] = Create(true);
                music[i].clip = layers[i];
            }
            dialogue = Create(false);
            targets[0] = 1f;
            targets[1] = 0.35f;
        }

        private AudioSource Create(bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        private void OnEnable()
        {
            if (music != null)
            {
                foreach (AudioSource source in music)
                {
                    if (source != null && source.clip != null && !source.isPlaying) source.Play();
                }
            }
        }

        private void Update()
        {
            if (music == null) return;
            for (int i = 0; i < 6; i++)
            {
                if (music[i] != null)
                {
                    music[i].volume = Mathf.MoveTowards(music[i].volume, targets[i] * masterVolume * 0.45f, fadeSpeed * Time.unscaledDeltaTime);
                }
            }
        }

        public bool TryPlay(AudioCue cue, Vector3 position)
        {
            int index = (int)cue;
            if (!isActiveAndEnabled || voices == null || cues == null || index < 0) return false;
            AudioClip clip = index < cues.Length ? cues[index] : null;
            if (clip == null && sharedSynthesizedCues != null && index < sharedSynthesizedCues.Length)
            {
                clip = sharedSynthesizedCues[index];
            }
            if (clip == null) return false;

            foreach (AudioSource source in voices)
            {
                if (source != null && !source.isPlaying)
                {
                    source.volume = masterVolume;
                    source.clip = clip;
                    source.Play();
                    return true;
                }
            }
            return false;
        }

        public AudioClip GetLayerClip(AudioLayer layer)
        {
            int index = (int)layer;
            return layers != null && index >= 0 && index < layers.Length ? layers[index] : null;
        }

        public bool SetLayer(AudioLayer layer, float intensity)
        {
            int i = (int)layer;
            if (i < 0 || i >= 6 || float.IsNaN(intensity) || float.IsInfinity(intensity)) return false;
            targets[i] = Mathf.Clamp01(intensity);
            return true;
        }

        public bool TryPlayVoice(AudioClip clip)
        {
            if (!isActiveAndEnabled || dialogue == null || clip == null) return false;
            dialogue.Stop();
            dialogue.clip = clip;
            dialogue.volume = masterVolume;
            dialogue.Play();
            return true;
        }

        public void StopVoice()
        {
            if (dialogue != null) dialogue.Stop();
        }

        private void OnDisable()
        {
            if (voices != null) foreach (AudioSource source in voices) if (source != null) source.Stop();
            if (music != null) foreach (AudioSource source in music) if (source != null) source.Stop();
            StopVoice();
        }

        private static void EnsureSynthesizedAudioBank()
        {
            if (sharedSynthesizedCues != null && sharedSynthesizedCues[0] != null) return;
            sharedSynthesizedCues = new AudioClip[CueCount];
            const int sr = 22050;

            // 0..6: Core cues
            sharedSynthesizedCues[(int)AudioCue.Attack] = SynthBladeSlash("Cue_Attack", sr, 0.16f, 1600f, 520f, 0.45f);
            sharedSynthesizedCues[(int)AudioCue.Hit] = SynthImpact("Cue_Hit", sr, 0.14f, 180f, 920f, true);
            sharedSynthesizedCues[(int)AudioCue.Dash] = SynthWhoosh("Cue_Dash", sr, 0.18f, 380f, 1450f);
            sharedSynthesizedCues[(int)AudioCue.Parry] = SynthMetallicClang("Cue_Parry", sr, 0.28f, 1320f, 2640f, 3960f, false);
            sharedSynthesizedCues[(int)AudioCue.Enemy] = SynthImpact("Cue_EnemyHit", sr, 0.20f, 110f, 310f, false);
            sharedSynthesizedCues[(int)AudioCue.Rescue] = SynthChime("Cue_Rescue", sr, 0.35f, new[] { 523.25f, 659.25f, 783.99f, 1046.5f });
            sharedSynthesizedCues[(int)AudioCue.Honmoon] = SynthChime("Cue_Honmoon", sr, 0.45f, new[] { 440f, 554.37f, 659.25f, 880f });

            // 7..10: Rumi Saingeom (Zero Z-Saber 3-step rising pitch + Taekwondo High Roundhouse Kick)
            sharedSynthesizedCues[(int)AudioCue.RumiBladeSlash1] = SynthBladeSlash("Cue_RumiSlash1", sr, 0.15f, 1750f, 620f, 0.5f);
            sharedSynthesizedCues[(int)AudioCue.RumiBladeSlash2] = SynthBladeSlash("Cue_RumiSlash2", sr, 0.16f, 2100f, 740f, 0.55f);
            sharedSynthesizedCues[(int)AudioCue.RumiBladeSlash3] = SynthBladeSlash("Cue_RumiSlash3", sr, 0.22f, 2550f, 880f, 0.7f);
            sharedSynthesizedCues[(int)AudioCue.RumiTaekwondoKick] = SynthMartialKick("Cue_RumiKick", sr, 0.22f);

            // 11..13: Zoey Shinkal + Flying Leg-Scissor Chokehold Slam
            sharedSynthesizedCues[(int)AudioCue.ZoeyShinkalSlash] = SynthShinkalBellSlash("Cue_ZoeyShinkal", sr, 0.13f, 2900f);
            sharedSynthesizedCues[(int)AudioCue.ZoeyShinkalThrow] = SynthShinkalBellSlash("Cue_ZoeyThrow", sr, 0.16f, 3400f);
            sharedSynthesizedCues[(int)AudioCue.ZoeyLegChokeholdSlam] = SynthLegChokeholdSlam("Cue_ZoeyChokehold", sr, 0.32f);

            // 14..16: Mira Gokdo Polearm + Brawler Punches + Airplane Stainless-Steel Coffee Pot CLANG!
            sharedSynthesizedCues[(int)AudioCue.MiraGokdoSweep] = SynthBladeSlash("Cue_MiraGokdo", sr, 0.24f, 920f, 240f, 0.65f);
            sharedSynthesizedCues[(int)AudioCue.MiraBrawlerPunch] = SynthImpact("Cue_MiraPunch", sr, 0.15f, 135f, 540f, true);
            sharedSynthesizedCues[(int)AudioCue.MiraCoffeePotClang] = SynthMetallicClang("Cue_MiraCoffeePot", sr, 0.38f, 880f, 1345f, 2110f, true);

            // 17..21: Capcom Arcade Classics (Z-Saber Charge, Wave Release, E-Tank Refill Tick, Sub-Tank Full, Boss Warning Siren)
            sharedSynthesizedCues[(int)AudioCue.SaberChargeLoop] = SynthChargePulse("Cue_ChargeLoop", sr, 0.25f);
            sharedSynthesizedCues[(int)AudioCue.SaberChargeRelease] = SynthBladeSlash("Cue_ChargeRelease", sr, 0.34f, 2800f, 420f, 0.85f);
            sharedSynthesizedCues[(int)AudioCue.EnergyTankTick] = SynthChime("Cue_TankTick", sr, 0.07f, new[] { 1174.66f, 1567.98f });
            sharedSynthesizedCues[(int)AudioCue.SubTankFull] = SynthChime("Cue_SubTankFull", sr, 0.40f, new[] { 587.33f, 880f, 1174.66f, 1760f });
            sharedSynthesizedCues[(int)AudioCue.BossWarningSiren] = SynthWarningSiren("Cue_WarningSiren", sr, 0.55f);

            // 6 Synchronized K-Pop Synth-Rock Loops (2.0s seamless loop @ 120 BPM)
            sharedSynthesizedLayers = new AudioClip[6];
            for (int i = 0; i < 6; i++)
            {
                sharedSynthesizedLayers[i] = SynthKpopMusicLayer((AudioLayer)i, sr);
            }
        }

        private static AudioClip SynthBladeSlash(string name, int sr, float duration, float ringFreq, float sweepStart, float brightness)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0x9E3779B9u;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                float env = Mathf.Sin(Mathf.Clamp01(u * 4f) * Mathf.PI * 0.5f) * Mathf.Exp(-u * 6.5f);
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                float sweep = Mathf.Lerp(sweepStart, sweepStart * 0.3f, u);
                float body = Mathf.Sin(2f * Mathf.PI * sweep * t);
                float metallic = Mathf.Sin(2f * Mathf.PI * ringFreq * t + Mathf.Sin(2f * Mathf.PI * (ringFreq * 1.414f) * t) * 1.5f);
                data[i] = Mathf.Clamp((noise * 0.45f + body * 0.35f + metallic * brightness) * env, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthMartialKick(string name, int sr, float duration)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0x12345678u;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                float whipSnap = u < 0.18f ? noise * Mathf.Exp(-u * 28f) * 0.9f : 0f;
                float bassFreq = Mathf.Lerp(210f, 52f, Mathf.Pow(u, 0.5f));
                float subKick = Mathf.Sin(2f * Mathf.PI * bassFreq * t) * Mathf.Exp(-u * 7.5f);
                data[i] = Mathf.Clamp(whipSnap + subKick * 0.85f, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthShinkalBellSlash(string name, int sr, float duration, float bellFreq)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0xABCDEF01u;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                float bell = (Mathf.Sin(2f * Mathf.PI * bellFreq * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * (bellFreq * 1.5f) * t)) * Mathf.Exp(-u * 8f);
                float slice = noise * Mathf.Exp(-u * 12f) * 0.45f;
                data[i] = Mathf.Clamp(bell * 0.6f + slice, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthLegChokeholdSlam(string name, int sr, float duration)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                float sample;
                if (u < 0.4f)
                {
                    // Airborne scissor spin whoosh
                    float spinFreq = Mathf.Lerp(260f, 680f, u / 0.4f);
                    sample = Mathf.Sin(2f * Mathf.PI * spinFreq * t) * 0.5f;
                }
                else
                {
                    // Heavy ground slam impact
                    float v = (u - 0.4f) / 0.6f;
                    float slamFreq = Mathf.Lerp(160f, 42f, v);
                    sample = Mathf.Sin(2f * Mathf.PI * slamFreq * t) * Mathf.Exp(-v * 6f) * 0.95f;
                }
                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthMetallicClang(string name, int sr, float duration, float f1, float f2, float f3, bool steamHiss)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0xCAFEBABEu;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                // Inharmonic hollow stainless-steel resonance (Mira's Airplane Coffee Pot CLANG!)
                float metal = (Mathf.Sin(2f * Mathf.PI * f1 * t)
                             + 0.75f * Mathf.Sin(2f * Mathf.PI * f2 * t)
                             + 0.55f * Mathf.Sin(2f * Mathf.PI * f3 * t)
                             + 0.4f * Mathf.Sin(2f * Mathf.PI * (f1 * 2.76f) * t)) * Mathf.Exp(-u * 5.2f) * 0.4f;
                float dentImpact = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(260f, 75f, u) * t) * Mathf.Exp(-u * 14f) * 0.5f;
                float steam = steamHiss && u > 0.15f ? noise * Mathf.Exp(-(u - 0.15f) * 4.5f) * 0.32f : 0f;
                data[i] = Mathf.Clamp(metal + dentImpact + steam, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthImpact(string name, int sr, float duration, float lowFreq, float highFreq, bool crisp)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0x55AA55AAu;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                float f = Mathf.Lerp(highFreq, lowFreq, Mathf.Sqrt(u));
                float tone = Mathf.Sin(2f * Mathf.PI * f * t);
                float env = Mathf.Exp(-u * 9f);
                data[i] = Mathf.Clamp((tone * 0.7f + (crisp ? noise * 0.35f : 0f)) * env, -0.95f, 0.95f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthWhoosh(string name, int sr, float duration, float fStart, float fEnd)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            uint seed = 0x77777777u;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed & 0xFFFF) / 32768f) - 1f;
                float sweep = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(fStart, fEnd, u) * t);
                float env = Mathf.Sin(u * Mathf.PI);
                data[i] = Mathf.Clamp((noise * 0.5f + sweep * 0.35f) * env, -0.9f, 0.9f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthChime(string name, int sr, float duration, float[] notes)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                int noteIdx = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(u * notes.Length));
                float f = notes[noteIdx];
                float env = Mathf.Exp(-((u * notes.Length) % 1f) * 3.5f);
                data[i] = Mathf.Clamp(Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.65f, -0.9f, 0.9f);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthChargePulse(string name, int sr, float duration)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                float f = Mathf.Lerp(520f, 1040f, u);
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Sin(u * Mathf.PI) * 0.55f;
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthWarningSiren(string name, int sr, float duration)
        {
            int len = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / len;
                float f = u < 0.5f ? Mathf.Lerp(620f, 980f, u * 2f) : Mathf.Lerp(980f, 620f, (u - 0.5f) * 2f);
                float sq = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.5f;
                data[i] = sq * Mathf.Sin(u * Mathf.PI);
            }
            AudioClip clip = AudioClip.Create(name, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthKpopMusicLayer(AudioLayer layer, int sr)
        {
            int len = sr * 2; // 2.0 second bar @ 120 BPM
            float[] data = new float[len];
            float[] bassNotes = { 110f, 110f, 130.81f, 146.83f, 110f, 110f, 164.81f, 146.83f };
            float[] leadNotes = { 440f, 523.25f, 659.25f, 587.33f, 440f, 659.25f, 783.99f, 659.25f };

            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                int step = Mathf.FloorToInt((t / 2f) * 8f) % 8;
                float stepPhase = ((t / 2f) * 8f) % 1f;
                float env = Mathf.Exp(-stepPhase * 3.2f);
                float sample = 0f;

                switch (layer)
                {
                    case AudioLayer.Exploration:
                        sample = Mathf.Sin(2f * Mathf.PI * (bassNotes[step] * 2f) * t) * env * 0.22f;
                        break;
                    case AudioLayer.Combat:
                        sample = Mathf.Sin(2f * Mathf.PI * bassNotes[step] * t) * env * 0.28f;
                        break;
                    case AudioLayer.Combo:
                        sample = Mathf.Sin(2f * Mathf.PI * leadNotes[step] * t) * env * 0.22f;
                        break;
                    case AudioLayer.Crowd:
                        sample = (Mathf.Sin(2f * Mathf.PI * 440f * t) + Mathf.Sin(2f * Mathf.PI * 554.37f * t) + Mathf.Sin(2f * Mathf.PI * 659.25f * t)) * 0.08f;
                        break;
                    case AudioLayer.Crisis:
                        sample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (bassNotes[step] * 0.5f) * t)) * env * 0.18f;
                        break;
                    case AudioLayer.Special:
                        sample = (Mathf.Sin(2f * Mathf.PI * leadNotes[step] * t) + Mathf.Sin(2f * Mathf.PI * (leadNotes[step] * 1.5f) * t)) * env * 0.25f;
                        break;
                }
                data[i] = Mathf.Clamp(sample, -0.8f, 0.8f);
            }
            AudioClip clip = AudioClip.Create("Layer_" + layer, len, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
