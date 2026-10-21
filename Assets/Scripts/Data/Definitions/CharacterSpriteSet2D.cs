using System;
using UnityEngine;

namespace HuntrX.Data
{
    [Serializable]
    public sealed class SpriteAnimationState2D
    {
        [SerializeField] private string id;
        [SerializeField] private SpriteAnimationClip2D clip;
        public string Id => id;
        public SpriteAnimationClip2D Clip => clip;
        public SpriteAnimationState2D(string id, SpriteAnimationClip2D clip)
        {
            this.id = id;
            this.clip = clip;
        }
    }

    /// <summary>One identity and layout across all states. No filename guessing or random appearance.</summary>
    [CreateAssetMenu(menuName = "HUNTR/X/Art/HD sprite set")]
    public sealed class CharacterSpriteSet2D : GameDataDefinition
    {
        [SerializeField] private string characterName;
        [SerializeField] private float pixelsPerUnit = 200f;
        [SerializeField] private float worldScale = 1f;
        [SerializeField] private Vector2 footAnchor = new Vector2(.5f, 0f);
        [SerializeField] private SpriteAnimationState2D[] states = Array.Empty<SpriteAnimationState2D>();

        public string CharacterName => characterName;
        public float WorldScale => worldScale;
        public float PixelsPerUnit => pixelsPerUnit;
        public Vector2 FootAnchor => footAnchor;

        public bool IsValid => Validate(characterName, pixelsPerUnit, worldScale, footAnchor, states);

        /// <summary>Creates a runtime catalog only after validation, without retaining the caller's array.</summary>
        public static bool TryCreate(string characterName, float pixelsPerUnit, float worldScale,
            Vector2 footAnchor, SpriteAnimationState2D[] states, out CharacterSpriteSet2D result)
        {
            result = null;
            var copiedStates = states == null ? null : (SpriteAnimationState2D[])states.Clone();
            if (!Validate(characterName, pixelsPerUnit, worldScale, footAnchor, copiedStates)) return false;
            result = CreateInstance<CharacterSpriteSet2D>();
            result.characterName = characterName;
            result.pixelsPerUnit = pixelsPerUnit;
            result.worldScale = worldScale;
            result.footAnchor = footAnchor;
            result.states = copiedStates;
            return true;
        }

        private static bool Validate(string characterName, float pixelsPerUnit, float worldScale,
            Vector2 footAnchor, SpriteAnimationState2D[] states)
        {
            if (string.IsNullOrWhiteSpace(characterName) || characterName.Length > 64 ||
                !Finite(pixelsPerUnit) || pixelsPerUnit <= 0f || pixelsPerUnit > 4096f ||
                !Finite(worldScale) || worldScale <= 0f || worldScale > 10f ||
                !Unit(footAnchor.x) || !Unit(footAnchor.y) || states == null ||
                states.Length < 1 || states.Length > 32) return false;

            Vector2 canvas = Vector2.zero;
            for (int i = 0; i < states.Length; i++)
            {
                SpriteAnimationState2D state = states[i];
                if (state == null || string.IsNullOrWhiteSpace(state.Id) || state.Id.Length > 64 ||
                    state.Clip == null || !state.Clip.IsValid) return false;
                for (int j = 0; j < i; j++)
                    if (string.Equals(states[j].Id, state.Id, StringComparison.Ordinal)) return false;

                for (int frame = 0; frame < state.Clip.FrameCount; frame++)
                {
                    Sprite sprite = state.Clip.FrameAtIndex(frame);
                    Vector2 size = sprite.rect.size;
                    if (i == 0 && frame == 0) canvas = size;
                    if (size != canvas || !Mathf.Approximately(sprite.pixelsPerUnit, pixelsPerUnit) ||
                        Vector2.Distance(sprite.pivot, Vector2.Scale(size, footAnchor)) > .01f) return false;
                }
            }
            return true;
        }

        public bool TryGetClip(string stateId, out SpriteAnimationClip2D clip)
        {
            clip = null;
            if (string.IsNullOrWhiteSpace(stateId) || !IsValid) return false;
            foreach (SpriteAnimationState2D state in states)
            {
                if (!string.Equals(state.Id, stateId, StringComparison.Ordinal)) continue;
                clip = state.Clip;
                return true;
            }
            return false;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Unit(float value) => Finite(value) && value >= 0f && value <= 1f;
    }
}
