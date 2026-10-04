using UnityEngine;

namespace HuntrX.Gameplay.Movement
{
    public enum HorizontalMovementState
    {
        Idle,
        Moving
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class HorizontalMovement2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxHorizontalSpeed;
        [SerializeField, Min(0f)] private float acceleration;
        [SerializeField, Min(0f)] private float deceleration;

        private Rigidbody2D body;
        private float horizontalInput;
        private bool invalidSettingsReported;

        public HorizontalMovementState State { get; private set; } = HorizontalMovementState.Idle;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Supplies the horizontal axis of the logical Move action. Device bindings stay outside this component.
        /// </summary>
        public void SetMovementInput(Vector2 moveInput)
        {
            float horizontal = moveInput.x;
            horizontalInput = IsFinite(horizontal) ? Mathf.Clamp(horizontal, -1f, 1f) : 0f;
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                ReportInvalidSettings("HorizontalMovement2D requires a Rigidbody2D component.");
                return;
            }

            if (!HasValidSettings())
            {
                ReportInvalidSettings(
                    "HorizontalMovement2D requires positive maximum speed, acceleration, and deceleration values.");
                State = Mathf.Approximately(body.linearVelocityX, 0f)
                    ? HorizontalMovementState.Idle
                    : HorizontalMovementState.Moving;
                return;
            }

            float currentSpeed = body.linearVelocityX;
            float targetSpeed = horizontalInput * maxHorizontalSpeed;
            bool targetDirectionMatches = !Mathf.Approximately(currentSpeed, 0f) &&
                Mathf.Sign(currentSpeed) == Mathf.Sign(targetSpeed);
            bool accelerating = !Mathf.Approximately(targetSpeed, 0f) &&
                (Mathf.Approximately(currentSpeed, 0f) ||
                 (targetDirectionMatches && Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed)));
            float rate = accelerating ? acceleration : deceleration;
            float nextSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);

            body.linearVelocityX = nextSpeed;
            State = Mathf.Approximately(nextSpeed, 0f)
                ? HorizontalMovementState.Idle
                : HorizontalMovementState.Moving;
        }

        private bool HasValidSettings()
        {
            return IsFinitePositive(maxHorizontalSpeed) &&
                IsFinitePositive(acceleration) &&
                IsFinitePositive(deceleration);
        }

        private void ReportInvalidSettings(string message)
        {
            if (invalidSettingsReported)
            {
                return;
            }

            invalidSettingsReported = true;
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