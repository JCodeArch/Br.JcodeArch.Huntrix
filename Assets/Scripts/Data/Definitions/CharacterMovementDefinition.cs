using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>Static, provisional movement tuning for a playable character.</summary>
    [CreateAssetMenu(fileName = "CharacterMovementDefinition", menuName = "HUNTR/X/Data/Character Movement")]
    public sealed class CharacterMovementDefinition : GameDataDefinition
    {
        [SerializeField, Min(0f)] private float maxHorizontalSpeed;
        [SerializeField, Min(0f)] private float acceleration;
        [SerializeField, Min(0f)] private float deceleration;
        [SerializeField, Min(0f)] private float jumpVelocity;
        [SerializeField, Min(0f)] private float wallJumpHorizontalSpeed;
        [SerializeField, Min(0f)] private float gravityScale;
        [SerializeField, Min(0f)] private float coyoteTime;
        [SerializeField, Min(0f)] private float jumpBufferTime;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier;
        [SerializeField, Min(0f)] private float dashSpeed;
        [SerializeField, Min(0f)] private float dashDuration;
        [SerializeField, Min(0f)] private float airDashSpeed;
        [SerializeField, Min(0f)] private float airDashDuration;

        public float MaxHorizontalSpeed => maxHorizontalSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float JumpVelocity => jumpVelocity;
        public float WallJumpHorizontalSpeed => wallJumpHorizontalSpeed;
        public float GravityScale => gravityScale;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public float AirDashSpeed => airDashSpeed;
        public float AirDashDuration => airDashDuration;

        public bool IsValid(out string error)
        {
            if (!IsPositive(maxHorizontalSpeed) || !IsPositive(acceleration) || !IsPositive(deceleration) ||
                !IsPositive(jumpVelocity) || !IsPositive(wallJumpHorizontalSpeed) || !IsPositive(gravityScale) ||
                !IsPositive(coyoteTime) || !IsPositive(jumpBufferTime) || !IsFinite(jumpCutMultiplier) ||
                jumpCutMultiplier <= 0f || jumpCutMultiplier >= 1f || !IsPositive(dashSpeed) ||
                !IsPositive(dashDuration) || !IsPositive(airDashSpeed) || !IsPositive(airDashDuration))
            {
                error = "CharacterMovementDefinition requires finite positive movement timings/speeds and a jump cut multiplier between 0 and 1.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool IsPositive(float value) => IsFinite(value) && value > 0f;
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
