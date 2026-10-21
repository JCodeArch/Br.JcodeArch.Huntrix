using System;
using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Presentation.Art
{
    /// <summary>Owns a sprite clock only. Physics and transform scale belong to their explicit owners.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterSpritePlayback2D : MonoBehaviour
    {
        private SpriteRenderer target;
        private CharacterSpriteSet2D definition;
        private SpriteAnimationClip2D clip;
        private Sprite lastOwnedFrame;
        private double elapsed;
        private bool externallySampled;

        public string CurrentState { get; private set; }
        public float Elapsed => (float)elapsed;

        public bool TryBind(SpriteRenderer renderer, CharacterSpriteSet2D set)
        {
            if (!isActiveAndEnabled || renderer == null || !renderer.gameObject.scene.IsValid() ||
                set == null || !set.TryGetClip("Idle", out SpriteAnimationClip2D initial)) return false;
            Unbind();
            target = renderer;
            definition = set;
            clip = initial;
            CurrentState = "Idle";
            Present();
            return true;
        }

        public bool SetState(string stateId, bool restart)
        {
            if (!isActiveAndEnabled || target == null || definition == null) return false;
            if (!definition.TryGetClip(stateId, out SpriteAnimationClip2D next)) return false;
            // Returning to an automatic pose also releases external clock ownership for the same state.
            externallySampled = false;
            if (!restart && string.Equals(CurrentState, stateId, StringComparison.Ordinal)) return true;
            clip = next;
            CurrentState = stateId;
            elapsed = 0d;
            Present();
            return true;
        }

        /// <summary>Samples an absolute presentation time; automatic advancement stays suspended until SetState.</summary>
        public bool SampleStateAt(string stateId, float seconds)
        {
            if (!isActiveAndEnabled || target == null || definition == null ||
                float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f ||
                !definition.TryGetClip(stateId, out SpriteAnimationClip2D next)) return false;
            double duration = next.Duration;
            if (duration <= 0d) return false;
            clip = next;
            CurrentState = stateId;
            externallySampled = true;
            elapsed = clip.Loop ? seconds % duration : Math.Min(seconds, duration);
            Present();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (externallySampled || !isActiveAndEnabled || target == null || clip == null ||
                float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return;
            double duration = clip.Duration;
            if (duration <= 0d) { Unbind(); return; }
            double next = elapsed + deltaTime;
            elapsed = clip.Loop ? next % duration : Math.Min(next, duration);
            Present();
        }

        public void Unbind()
        {
            // Another presenter may already have taken this renderer; do not clear its replacement.
            if (target != null && target.sprite == lastOwnedFrame) target.sprite = null;
            target = null;
            definition = null;
            clip = null;
            lastOwnedFrame = null;
            CurrentState = null;
            elapsed = 0d;
            externallySampled = false;
        }

        private void Present()
        {
            lastOwnedFrame = clip.FrameAt((float)elapsed);
            target.sprite = lastOwnedFrame;
        }

        private void Update() => Tick(Time.deltaTime);
        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();
    }
}
