using UnityEngine;
using System;

namespace HuntrX.Data
{
    /// <summary>
    /// Static authored definition for attack content. Gameplay fields belong to the corresponding approved card.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "HUNTR/X/Data/Attack")]
    public sealed class AttackDefinition : GameDataDefinition
    {
        [SerializeField] private float damage;
        [SerializeField] private float startupDuration;
        [SerializeField] private float activeDuration;
        [SerializeField] private float recoveryDuration;
        [SerializeField] private Vector2 hitboxSize;
        [SerializeField] private Vector2 hitboxOffset;
        [SerializeField] private float horizontalKnockbackImpulse;
        [SerializeField] private float upwardKnockbackImpulse;

        public float Damage => damage;
        public float StartupDuration => startupDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float HorizontalKnockbackImpulse => horizontalKnockbackImpulse;
        public float UpwardKnockbackImpulse => upwardKnockbackImpulse;

        public bool IsValid(out string error)
        {
            if (!IsFinitePositive(damage))
            {
                error = "AttackDefinition damage must be finite and positive.";
                return false;
            }
            if (!IsFiniteNonnegative(startupDuration))
            {
                error = "AttackDefinition startup duration must be finite and nonnegative.";
                return false;
            }
            if (!IsFinitePositive(activeDuration))
            {
                error = "AttackDefinition active duration must be finite and positive.";
                return false;
            }
            if (!IsFiniteNonnegative(recoveryDuration))
            {
                error = "AttackDefinition recovery duration must be finite and nonnegative.";
                return false;
            }
            if (!IsFinitePositive(hitboxSize.x) || !IsFinitePositive(hitboxSize.y))
            {
                error = "AttackDefinition hitbox size components must be finite and positive.";
                return false;
            }
            if (!IsFinite(hitboxOffset.x) || !IsFinite(hitboxOffset.y))
            {
                error = "AttackDefinition hitbox offset components must be finite.";
                return false;
            }
            if (!IsFinitePositive(horizontalKnockbackImpulse))
            {
                error = "AttackDefinition horizontal knockback impulse must be finite and positive.";
                return false;
            }
            if (!IsFiniteNonnegative(upwardKnockbackImpulse))
            {
                error = "AttackDefinition upward knockback impulse must be finite and nonnegative.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool IsFinitePositive(float value) => IsFinite(value) && value > 0f;

        private static bool IsFiniteNonnegative(float value) => IsFinite(value) && value >= 0f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
