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
            if (!restart && string.Equals(CurrentState, stateId, StringComparison.Ordinal)) return true;
            if (!definition.TryGetClip(stateId, out SpriteAnimationClip2D next)) return false;
            clip = next;
            CurrentState = stateId;
            elapsed = 0d;
            Present();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || target == null || clip == null ||
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
