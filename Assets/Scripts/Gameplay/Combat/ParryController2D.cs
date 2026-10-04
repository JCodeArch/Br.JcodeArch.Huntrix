using HuntrX.Data;
using HuntrX.Gameplay.Dash;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageReceiver2D))]
    public sealed class ParryController2D : MonoBehaviour
    {
        [SerializeField] private ParryDefinition definition;

        private DamageReceiver2D receiver;
        private DashController2D dashController;
        private int activeWindowFixedStepsRemaining;

        public bool IsWindowActive { get; private set; }

        internal void SetValidatedDefinition(ParryDefinition nextDefinition)
        {
            definition = nextDefinition;
        }

        private void Awake()
        {
            receiver = GetComponent<DamageReceiver2D>();
            dashController = GetComponent<DashController2D>();
            receiver?.RegisterParryController(this);
        }

        /// <summary>Opens one parry window. Input edge handling belongs to the caller.</summary>
        public bool TryStartParry()
        {
            if (!isActiveAndEnabled || receiver == null || !receiver.IsAlive ||
                (dashController != null && dashController.IsInvulnerable) ||
                definition == null || !definition.IsValid(out _) ||
                !IsFinite(Time.fixedDeltaTime) || Time.fixedDeltaTime <= 0f)
            {
                return false;
            }

            if (IsWindowActive)
            {
                return false;
            }

            activeWindowFixedStepsRemaining = Mathf.Max(1,
                Mathf.CeilToInt(definition.WindowDuration / Time.fixedDeltaTime));
            IsWindowActive = true;
            return true;
        }

        internal bool TryConsumeParry(DamageReceiver2D attacker, AttackDefinition attack,
            int comboStepIndex, out CombatParryEvent parryEvent)
        {
            parryEvent = default;
            if (!IsWindowActive || definition == null || !definition.IsValid(out _) ||
                attacker == null || attack == null || comboStepIndex < 0)
            {
                return false;
            }

            IsWindowActive = false;
            activeWindowFixedStepsRemaining = 0;
            parryEvent = new CombatParryEvent(receiver, attacker, attack, comboStepIndex);
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsWindowActive)
            {
                return;
            }

            if (activeWindowFixedStepsRemaining > 0)
            {
                activeWindowFixedStepsRemaining--;
                return;
            }

            IsWindowActive = false;
            activeWindowFixedStepsRemaining = 0;
        }

        private void OnDisable()
        {
            IsWindowActive = false;
            activeWindowFixedStepsRemaining = 0;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
