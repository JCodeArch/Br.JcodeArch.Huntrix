using UnityEngine;

namespace HuntrX.Gameplay.Jump
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class JumpController2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float jumpVelocity;
        [SerializeField, Min(0f)] private float wallJumpHorizontalSpeed;
        [SerializeField, Min(0f)] private float gravityScale;
        [SerializeField, Min(0f)] private float coyoteTime;
        [SerializeField, Min(0f)] private float jumpBufferTime;
        [SerializeField, Min(0f)] private float jumpCutMultiplier;

        private Rigidbody2D body;
        private float coyoteTimeRemaining;
        private float jumpBufferTimeRemaining;
        private bool isGrounded;
        private bool jumpHeld;
        private bool hasBufferedJump;
        private bool hasActiveJump;
        private bool jumpCutApplied;
        private bool gravityConfigured;
        private bool invalidConfigurationReported;
        private bool jumpSuppressed;
        private bool hasWallContact;
        private Vector2 wallOutwardNormal;
        private bool wallJumpConsumed;
        private bool invalidWallJumpSpeedReported;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Reports grounded contact from a character or ground-sensing layer.</summary>
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
            if (grounded)
            {
                coyoteTimeRemaining = coyoteTime;
                hasActiveJump = false;
                jumpCutApplied = false;
            }
        }

        /// <summary>Reports the current wall contact and its outward-facing normal.</summary>
        public void SetWallContact(bool touching, Vector2 outwardNormal)
        {
            if (!touching)
            {
                hasWallContact = false;
                wallOutwardNormal = Vector2.zero;
                wallJumpConsumed = false;
                return;
            }

            if (!IsFinite(outwardNormal.x) || !IsFinite(outwardNormal.y) ||
                (outwardNormal.x == 0f && outwardNormal.y == 0f) ||
                Mathf.Abs(outwardNormal.x) <= Mathf.Abs(outwardNormal.y))
            {
                hasWallContact = false;
                wallOutwardNormal = Vector2.zero;
                return;
            }

            float largestComponent = Mathf.Max(Mathf.Abs(outwardNormal.x), Mathf.Abs(outwardNormal.y));
            Vector2 scaledNormal = outwardNormal / largestComponent;
            hasWallContact = true;
            wallOutwardNormal = scaledNormal.normalized;
        }

        /// <summary>Temporarily suppresses jump starts and cuts while a higher-priority action is active.</summary>
        public void SetJumpSuppressed(bool suppressed)
        {
            jumpSuppressed = suppressed;
            if (!suppressed)
            {
                return;
            }

            jumpHeld = false;
            hasBufferedJump = false;
            jumpBufferTimeRemaining = 0f;
            coyoteTimeRemaining = 0f;
            hasActiveJump = false;
            jumpCutApplied = true;
        }

        /// <summary>Queues the logical Jump action until grounded or its buffer expires.</summary>
        public void PressJump()
        {
            if (jumpSuppressed)
            {
                return;
            }

            jumpHeld = true;
            jumpBufferTimeRemaining = jumpBufferTime;
            hasBufferedJump = true;
        }

        /// <summary>Releases the logical Jump action; an active rising jump is cut in FixedUpdate.</summary>
        public void ReleaseJump()
        {
            jumpHeld = false;
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                ReportInvalidConfiguration("JumpController2D requires a Rigidbody2D component.");
                return;
            }

            if (!HasValidConfiguration())
            {
                ReportInvalidConfiguration(
                    "JumpController2D requires positive jump velocity, gravity scale, coyote time, and jump buffer, plus a jump cut multiplier between 0 and 1.");
                return;
            }

            if (!gravityConfigured)
            {
                body.gravityScale = gravityScale;
                gravityConfigured = true;
            }

            if (jumpSuppressed)
            {
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            if (isGrounded)
            {
                coyoteTimeRemaining = coyoteTime;
            }
            else
            {
                coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - fixedDeltaTime);
            }

            bool canJump = isGrounded || coyoteTimeRemaining > 0f;
            bool canWallJump = !isGrounded && hasWallContact && !wallJumpConsumed;
            if (hasBufferedJump && isGrounded)
            {
                StartJump();
            }
            else if (hasBufferedJump && canWallJump)
            {
                if (IsFinitePositive(wallJumpHorizontalSpeed))
                {
                    StartWallJump();
                }
                else
                {
                    ReportInvalidWallJumpSpeed();
                    ExpireBufferedJumpIfNeeded(fixedDeltaTime);
                }
            }
            else if (hasBufferedJump && canJump)
            {
                StartJump();
            }
            else if (hasBufferedJump)
            {
                ExpireBufferedJumpIfNeeded(fixedDeltaTime);
            }

            ApplyJumpCutIfReleased();
        }

        private void StartJump()
        {
            body.linearVelocityY = jumpVelocity;
            isGrounded = false;
            coyoteTimeRemaining = 0f;
            jumpBufferTimeRemaining = 0f;
            hasBufferedJump = false;
            hasActiveJump = true;
            jumpCutApplied = false;
        }

        private void StartWallJump()
        {
            body.linearVelocity = new Vector2(Mathf.Sign(wallOutwardNormal.x) * wallJumpHorizontalSpeed, jumpVelocity);
            isGrounded = false;
            coyoteTimeRemaining = 0f;
            jumpBufferTimeRemaining = 0f;
            hasBufferedJump = false;
            hasActiveJump = true;
            jumpCutApplied = false;
            wallJumpConsumed = true;
        }

        private void ExpireBufferedJumpIfNeeded(float fixedDeltaTime)
        {
            jumpBufferTimeRemaining = Mathf.Max(0f, jumpBufferTimeRemaining - fixedDeltaTime);
            if (jumpBufferTimeRemaining <= 0f)
            {
                hasBufferedJump = false;
            }
        }

        private void ApplyJumpCutIfReleased()
        {
            if (!hasActiveJump || jumpHeld || jumpCutApplied)
            {
                return;
            }

            if (body.linearVelocityY > 0f)
            {
                body.linearVelocityY *= jumpCutMultiplier;
            }

            jumpCutApplied = true;
            hasActiveJump = false;
        }

        private bool HasValidConfiguration()
        {
            return IsFinitePositive(jumpVelocity) &&
                IsFinitePositive(gravityScale) &&
                IsFinitePositive(coyoteTime) &&
                IsFinitePositive(jumpBufferTime) &&
                IsFinite(jumpCutMultiplier) &&
                jumpCutMultiplier > 0f &&
                jumpCutMultiplier < 1f;
        }

        private void ReportInvalidWallJumpSpeed()
        {
            if (invalidWallJumpSpeedReported)
            {
                return;
            }

            invalidWallJumpSpeedReported = true;
            Debug.LogError("JumpController2D requires a positive finite wall jump horizontal speed.", this);
        }

        private void ReportInvalidConfiguration(string message)
        {
            if (invalidConfigurationReported)
            {
                return;
            }

            invalidConfigurationReported = true;
            Debug.LogError(message, this);
        }

        private static bool IsFinitePositive(float value)
        {
            return IsFinite(value) && value > 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}