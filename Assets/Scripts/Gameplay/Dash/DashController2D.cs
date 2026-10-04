using System;
using HuntrX.Gameplay.Movement;
using UnityEngine;

namespace HuntrX.Gameplay.Dash
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HorizontalMovement2D))]
    public sealed class DashController2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float dashSpeed;
        [SerializeField, Min(0f)] private float dashDuration;

        private HorizontalMovement2D horizontalMovement;
        private bool isGrounded;
        private bool invalidSettingsReported;
        private bool invalidDirectionReported;
        private float remainingDuration;

        public event Action<bool> DashStateChanged;

        public bool IsDashing { get; private set; }

        public bool IsInvulnerable => IsDashing;

        private void Awake()
        {
            horizontalMovement = GetComponent<HorizontalMovement2D>();
        }

        /// <summary>
        /// Supplies the current grounded state from the character/grounding layer.
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            isGrounded = grounded;
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