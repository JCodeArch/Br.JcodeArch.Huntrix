using System;
using UnityEngine;

namespace HuntrX.Gameplay.Narrative
{
    public enum AirplaneCutsceneSequenceId
    {
        CockpitCaptainEntry = 0,
        PostVictoryDramaticExit = 1
    }

    public enum AirplaneCutsceneTransitionKind
    {
        HardCutWithFlash = 0,
        SmoothCrossfade = 1,
        ImpactHoldToGameplay = 2
    }

    [Serializable]
    public struct AirplaneCutsceneFrameSpec
    {
        public int FrameIndex;
        public string ResourcePath;
        public string BeatLabel;
        public float StartTimeSeconds;
        public float DurationSeconds;
        public float CrossfadeDurationSeconds;
        public AirplaneCutsceneTransitionKind TransitionKind;
        public float CameraShakeAmplitude;

        public float EndTimeSeconds => StartTimeSeconds + DurationSeconds;
    }

    [Serializable]
    public sealed class AirplaneCutsceneSequenceSpec
    {
        public AirplaneCutsceneSequenceId SequenceId;
        public string SequenceKey;
        public string DisplayTitle;
        public int TargetWidthPixels;
        public int TargetHeightPixels;
        public float RecommendedCaptainToHeroScaleRatio;
        public AirplaneCutsceneFrameSpec[] Frames;

        public float TotalDurationSeconds
        {
            get
            {
                if (Frames == null || Frames.Length == 0) return 0f;
                AirplaneCutsceneFrameSpec last = Frames[Frames.Length - 1];
                return last.StartTimeSeconds + last.DurationSeconds;
            }
        }
    }

    /// <summary>
    /// Self-contained catalog and timing/transition specification for Stage 1 (Airplane)
    /// narrative cutscene sequences:
    /// (a) Captain Dokkaebi Cockpit Entry (4 frames, 1376x768 16:9, 3.50s total)
    /// (b) Post-Victory Dramatic Airplane Exit (4 frames, 1376x768 16:9, 3.80s total)
    /// </summary>
    public static class AirplaneCutsceneArtCatalog
    {
        public const int StandardFrameWidth = 1376;
        public const int StandardFrameHeight = 768;
        public const float StandardPixelsPerUnit = 100f;
        public const float CaptainToRumiScaleRatio = 1.35f;

        public const string CockpitEntryFolder = "Art/Cutscenes/AirplaneCockpitEntry";
        public const string VictoryExitFolder = "Art/Cutscenes/AirplaneVictoryExit";

        private static readonly AirplaneCutsceneSequenceSpec CockpitEntrySpec = new AirplaneCutsceneSequenceSpec
        {
            SequenceId = AirplaneCutsceneSequenceId.CockpitCaptainEntry,
            SequenceKey = "airplane_cockpit_captain_entry",
            DisplayTitle = "Porta da Cabine: Entrada do Capitão Dokkaebi",
            TargetWidthPixels = StandardFrameWidth,
            TargetHeightPixels = StandardFrameHeight,
            RecommendedCaptainToHeroScaleRatio = CaptainToRumiScaleRatio,
            Frames = new[]
            {
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 0,
                    ResourcePath = CockpitEntryFolder + "/0",
                    BeatLabel = "Approach_Sealed_Cockpit_Bulkhead",
                    StartTimeSeconds = 0.00f,
                    DurationSeconds = 0.85f,
                    CrossfadeDurationSeconds = 0.14f,
                    TransitionKind = AirplaneCutsceneTransitionKind.SmoothCrossfade,
                    CameraShakeAmplitude = 0.05f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 1,
                    ResourcePath = CockpitEntryFolder + "/1",
                    BeatLabel = "BlastDoor_Breach_Disguise_Fracture",
                    StartTimeSeconds = 0.85f,
                    DurationSeconds = 0.85f,
                    CrossfadeDurationSeconds = 0.08f,
                    TransitionKind = AirplaneCutsceneTransitionKind.HardCutWithFlash,
                    CameraShakeAmplitude = 0.18f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 2,
                    ResourcePath = CockpitEntryFolder + "/2",
                    BeatLabel = "Red_Captain_Dokkaebi_Roar_Scale_Reveal",
                    StartTimeSeconds = 1.70f,
                    DurationSeconds = 0.95f,
                    CrossfadeDurationSeconds = 0.12f,
                    TransitionKind = AirplaneCutsceneTransitionKind.SmoothCrossfade,
                    CameraShakeAmplitude = 0.25f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 3,
                    ResourcePath = CockpitEntryFolder + "/3",
                    BeatLabel = "Cyan_Saingeom_Vs_Crimson_Claw_Clash_Engage",
                    StartTimeSeconds = 2.65f,
                    DurationSeconds = 0.85f,
                    CrossfadeDurationSeconds = 0.16f,
                    TransitionKind = AirplaneCutsceneTransitionKind.ImpactHoldToGameplay,
                    CameraShakeAmplitude = 0.30f
                }
            }
        };

        private static readonly AirplaneCutsceneSequenceSpec VictoryExitSpec = new AirplaneCutsceneSequenceSpec
        {
            SequenceId = AirplaneCutsceneSequenceId.PostVictoryDramaticExit,
            SequenceKey = "airplane_post_victory_dramatic_exit",
            DisplayTitle = "Saída Dramática: Salto do Jato HUNTR/X rumo a Seul",
            TargetWidthPixels = StandardFrameWidth,
            TargetHeightPixels = StandardFrameHeight,
            RecommendedCaptainToHeroScaleRatio = CaptainToRumiScaleRatio,
            Frames = new[]
            {
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 0,
                    ResourcePath = VictoryExitFolder + "/0",
                    BeatLabel = "Cyan_Saingeom_Finisher_And_Bulkhead_Unlock",
                    StartTimeSeconds = 0.00f,
                    DurationSeconds = 0.90f,
                    CrossfadeDurationSeconds = 0.15f,
                    TransitionKind = AirplaneCutsceneTransitionKind.HardCutWithFlash,
                    CameraShakeAmplitude = 0.22f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 1,
                    ResourcePath = VictoryExitFolder + "/1",
                    BeatLabel = "Huntrix_Trio_Regroup_At_Open_Cabin_Hatch",
                    StartTimeSeconds = 0.90f,
                    DurationSeconds = 0.90f,
                    CrossfadeDurationSeconds = 0.16f,
                    TransitionKind = AirplaneCutsceneTransitionKind.SmoothCrossfade,
                    CameraShakeAmplitude = 0.08f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 2,
                    ResourcePath = VictoryExitFolder + "/2",
                    BeatLabel = "Synchronized_Leap_Over_Swept_Jet_Wing",
                    StartTimeSeconds = 1.80f,
                    DurationSeconds = 0.95f,
                    CrossfadeDurationSeconds = 0.16f,
                    TransitionKind = AirplaneCutsceneTransitionKind.SmoothCrossfade,
                    CameraShakeAmplitude = 0.14f
                },
                new AirplaneCutsceneFrameSpec
                {
                    FrameIndex = 3,
                    ResourcePath = VictoryExitFolder + "/3",
                    BeatLabel = "High_Altitude_Skydive_Toward_Seoul_Dome",
                    StartTimeSeconds = 2.75f,
                    DurationSeconds = 1.05f,
                    CrossfadeDurationSeconds = 0.22f,
                    TransitionKind = AirplaneCutsceneTransitionKind.ImpactHoldToGameplay,
                    CameraShakeAmplitude = 0.12f
                }
            }
        };

        public static AirplaneCutsceneSequenceSpec GetSequence(AirplaneCutsceneSequenceId id)
        {
            if (id == AirplaneCutsceneSequenceId.CockpitCaptainEntry) return CockpitEntrySpec;
            if (id == AirplaneCutsceneSequenceId.PostVictoryDramaticExit) return VictoryExitSpec;
            return null;
        }

        public static bool TryResolveFrameAtTime(
            AirplaneCutsceneSequenceId id,
            float elapsedSeconds,
            out AirplaneCutsceneFrameSpec frame,
            out float normalizedFrameTime)
        {
            AirplaneCutsceneSequenceSpec seq = GetSequence(id);
            if (seq == null || seq.Frames == null || seq.Frames.Length == 0)
            {
                frame = default;
                normalizedFrameTime = 0f;
                return false;
            }

            float clamped = Mathf.Clamp(elapsedSeconds, 0f, seq.TotalDurationSeconds);
            for (int i = 0; i < seq.Frames.Length; i++)
            {
                AirplaneCutsceneFrameSpec candidate = seq.Frames[i];
                if (clamped <= candidate.EndTimeSeconds || i == seq.Frames.Length - 1)
                {
                    frame = candidate;
                    float denom = Mathf.Max(0.001f, candidate.DurationSeconds);
                    normalizedFrameTime = Mathf.Clamp01((clamped - candidate.StartTimeSeconds) / denom);
                    return true;
                }
            }

            frame = seq.Frames[seq.Frames.Length - 1];
            normalizedFrameTime = 1f;
            return true;
        }

        public static bool TryLoadSprites(AirplaneCutsceneSequenceId id, out Sprite[] sprites)
        {
            AirplaneCutsceneSequenceSpec seq = GetSequence(id);
            if (seq == null || seq.Frames == null || seq.Frames.Length == 0)
            {
                sprites = Array.Empty<Sprite>();
                return false;
            }

            sprites = new Sprite[seq.Frames.Length];
            for (int i = 0; i < seq.Frames.Length; i++)
            {
                Sprite s = UnityEngine.Resources.Load<Sprite>(seq.Frames[i].ResourcePath);
                if (s == null)
                {
                    sprites = Array.Empty<Sprite>();
                    return false;
                }
                sprites[i] = s;
            }
            return true;
        }

        public static bool TryLoadTextures(AirplaneCutsceneSequenceId id, out Texture2D[] textures)
        {
            AirplaneCutsceneSequenceSpec seq = GetSequence(id);
            if (seq == null || seq.Frames == null || seq.Frames.Length == 0)
            {
                textures = Array.Empty<Texture2D>();
                return false;
            }

            textures = new Texture2D[seq.Frames.Length];
            for (int i = 0; i < seq.Frames.Length; i++)
            {
                Texture2D tex = UnityEngine.Resources.Load<Texture2D>(seq.Frames[i].ResourcePath);
                if (tex == null)
                {
                    textures = Array.Empty<Texture2D>();
                    return false;
                }
                textures[i] = tex;
            }
            return true;
        }
    }
}
