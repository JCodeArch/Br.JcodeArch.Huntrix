using System;
using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>Authored presentation timing. It never opens hitboxes or advances combat state.</summary>
    [Serializable]
    public sealed class SpriteAnimationClip2D
    {
        [SerializeField] private Sprite[] frames = Array.Empty<Sprite>();
        [SerializeField] private float[] durations = Array.Empty<float>();
        [SerializeField] private bool loop;

        public SpriteAnimationClip2D() { }

        public SpriteAnimationClip2D(Sprite[] frames, float[] durations, bool loop)
        {
            this.frames = frames == null ? null : (Sprite[])frames.Clone();
            this.durations = durations == null ? null : (float[])durations.Clone();
            this.loop = loop;
        }

        public bool IsValid => TryGetDuration(out _);
        public float Duration => TryGetDuration(out float duration) ? duration : 0f;
        public bool Loop => loop;
        public int FrameCount => frames == null ? 0 : frames.Length;
        internal Sprite FrameAtIndex(int index) => frames[index];

        public Sprite FrameAt(float elapsed)
        {
            if (!Finite(elapsed) || !TryGetDuration(out float duration)) return null;
            elapsed = Mathf.Max(0f, elapsed);
            if (loop) elapsed %= duration;
            else if (elapsed >= duration) return frames[frames.Length - 1];

            for (int i = 0; i < frames.Length; i++)
            {
                if (elapsed < durations[i]) return frames[i];
                elapsed -= durations[i];
            }
            // Floating point subtraction at the end of a clip must retain a valid pose.
            return frames[frames.Length - 1];
        }

        private bool TryGetDuration(out float duration)
        {
            duration = 0f;
            if (frames == null || durations == null || frames.Length < 1 || frames.Length > 128 ||
                frames.Length != durations.Length) return false;
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] == null || !Finite(durations[i]) || durations[i] <= 0f) return false;
                duration += durations[i];
                if (!Finite(duration)) return false;
            }
            return duration > 0f;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
