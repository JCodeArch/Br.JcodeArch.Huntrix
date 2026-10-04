using System;
using HuntrX.Gameplay.Movement;
using HuntrX.Gameplay.Jump;
using UnityEngine;

namespace HuntrX.Gameplay.Dash
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HorizontalMovement2D))]
    public sealed class DashController2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float dashSpeed;
        [SerializeField, Min(0f)] private float dashDuration;
        [SerializeField, Min(0f)] private float airDashSpeed;
        [SerializeField, Min(0f)] private float airDashDuration;

        private HorizontalMovement2D horizontalMovement;
        private bool isGrounded;
        private bool invalidSettingsReported;
        private bool invalidDirectionReported;
        private bool invalidAirSettingsReported;
        private bool invalidAirDirectionReported;
        private bool airDashAvailable = true;
        private bool isAirDashing;
        private Rigidbody2D body;
        private JumpController2D jumpController;
        private float remainingDuration;

        public event Action<bool> DashStateChanged;

        public bool IsDashing { get; private set; }

        public bool IsInvulnerable => IsDashing;

        private void Awake()
        {
            horizontalMovement = GetComponent<HorizontalMovement2D>();
            body = GetComponent<Rigidbody2D>();
            jumpController = GetComponent<JumpController2D>();
        }

        /// <summary>
        /// Supplies the current grounded state from the character/grounding layer.
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
            if (grounded)
            {
                airDashAvailable = true;
            }
        }

        /// <summary>
        /// Starts a ground dash in the sign of the supplied logical horizontal direction.
        /// </summary>
        public bool TryStartDash(float horizontalDirection)
        {
            if (!isActiveAndEnabled || IsDashing || !isGrounded)
            {
                return false;
            }

            if (!IsFinite(horizontalDirection) || horizontalDirection == 0f)
            {
                ReportInvalidDirection();
                return false;
            }
            if (!HasValidSettings())
            {
                ReportInvalidSettings();
                return false;
            }

            float dashVelocity = Mathf.Sign(horizontalDirection) * dashSpeed;
            if (horizontalMovement == null ||
                !horizontalMovement.TrySetHorizontalVelocityOverride(this, dashVelocity))
            {
                return false;
            }

            remainingDuration = dashDuration;
            IsDashing = true;
            DashStateChanged?.Invoke(true);
            return true;
        }

        /// <summary>
        /// Starts one directional air dash using the logical Move direction.
        /// </summary>
        public bool TryStartAirDash(Vector2 direction)
        {
            if (!isActiveAndEnabled || IsDashing || isGrounded || !airDashAvailable)
            {
                return false;
            }

            if (!TryNormalizeDirection(direction, out Vector2 normalizedDirection))
            {
                ReportInvalidAirDirection();
                return false;
            }
            if (!HasValidAirSettings())
            {
                ReportInvalidAirSettings();
                return false;
            }
            if (body == null || horizontalMovement == null)
            {
                return false;
            }

            Vector2 dashVelocity = normalizedDirection * airDashSpeed;
            if (!horizontalMovement.TrySetHorizontalVelocityOverride(this, dashVelocity.x))
            {
                return false;
            }

            body.linearVelocity = dashVelocity;
            remainingDuration = airDashDuration;
            airDashAvailable = false;
            isAirDashing = true;
            if (jumpController == null)
            {
                jumpController = GetComponent<JumpController2D>();
            }
            jumpController?.SetJumpSuppressed(true);
            IsDashing = true;
            DashStateChanged?.Invoke(true);
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsDashing)
            {
                return;
            }

            remainingDuration -= Time.fixedDeltaTime;
            if (remainingDuration <= 0f)
            {
                EndDash();
            }
        }

        private void OnDisable()
        {
            if (IsDashing)
            {
                EndDash();
            }
            else if (horizontalMovement != null)
            {
                horizontalMovement.ClearHorizontalVelocityOverride(this);
            }
        }

        private void EndDash()
        {
            horizontalMovement?.ClearHorizontalVelocityOverride(this);
            remainingDuration = 0f;
            IsDashing = false;
            if (isAirDashing)
            {
                jumpController?.SetJumpSuppressed(false);
                isAirDashing = false;
            }
            DashStateChanged?.Invoke(false);
        }

        private void ReportInvalidDirection()
        {
            if (invalidDirectionReported)
            {
                return;
            }

            invalidDirectionReported = true;
            Debug.LogError("DashController2D requires a finite, non-zero horizontal direction.", this);
        }
        private bool HasValidAirSettings()
        {
            return IsFinitePositive(airDashSpeed) && IsFinitePositive(airDashDuration);
        }

        private void ReportInvalidAirSettings()
        {
            if (invalidAirSettingsReported)
            {
                return;
            }

            invalidAirSettingsReported = true;
            Debug.LogError("DashController2D requires positive air dash speed and duration values.", this);
        }

        private void ReportInvalidAirDirection()
        {
            if (invalidAirDirectionReported)
            {
                return;
            }

            invalidAirDirectionReported = true;
            Debug.LogError("DashController2D requires a finite, non-zero air dash direction.", this);
        }

        private static bool TryNormalizeDirection(Vector2 direction, out Vector2 normalizedDirection)
        {
            normalizedDirection = Vector2.zero;
            if (!IsFinite(direction.x) || !IsFinite(direction.y))
            {
                return false;
            }

            float largestComponent = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            if (largestComponent <= 0f)
            {
                return false;
            }

            Vector2 scaledDirection = direction / largestComponent;
            normalizedDirection = scaledDirection.normalized;
            return normalizedDirection.sqrMagnitude > 0f;
        }

        private bool HasValidSettings()
        {
            return IsFinitePositive(dashSpeed) && IsFinitePositive(dashDuration);
        }

        private void ReportInvalidSettings()
        {
            if (invalidSettingsReported)
            {
                return;
            }

            invalidSettingsReported = true;
            Debug.LogError("DashController2D requires positive dash speed and duration values.", this);
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