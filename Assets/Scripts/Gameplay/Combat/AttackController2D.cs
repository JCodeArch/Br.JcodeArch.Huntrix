using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageReceiver2D), typeof(AttackHitbox2D))]
    public sealed class AttackController2D : MonoBehaviour
    {
        [SerializeField] private AttackDefinition attackDefinition;

        private AttackHitbox2D attackHitbox;
        private DamageReceiver2D attacker;
        private float remainingPhaseDuration;
        private float facingDirection;
        private bool invalidDefinitionReported;
        private bool invalidHitboxReported;
        private bool activeHitboxPendingEnable;

        public AttackState2D State { get; private set; } = AttackState2D.Idle;

        private void Awake()
        {
            attacker = GetComponent<DamageReceiver2D>();
            attackHitbox = GetComponent<AttackHitbox2D>();
        }

        public bool TryStartAttack(float facingDirection)
        {
            if (!isActiveAndEnabled || State != AttackState2D.Idle || !IsFinite(facingDirection) ||
                facingDirection == 0f || attacker == null || !attacker.IsConfigurationValid ||
                attackHitbox == null)
            {
                return false;
            }

            if (!attackHitbox.IsConfigurationValid)
            {
                ReportInvalidHitbox();
                return false;
            }

            if (attackDefinition == null || !attackDefinition.IsValid(out _))
            {
                ReportInvalidDefinition();
                return false;
            }

            this.facingDirection = Mathf.Sign(facingDirection);
            if (attackDefinition.StartupDuration <= 0f)
            {
                BeginActivePhase();
            }
            else
            {
                State = AttackState2D.Startup;
                remainingPhaseDuration = attackDefinition.StartupDuration;
            }
            return true;
        }

        private void FixedUpdate()
        {
            if (State == AttackState2D.Idle)
            {
                return;
            }

            if (activeHitboxPendingEnable)
            {
                EnableActiveHitbox();
                return;
            }

            remainingPhaseDuration -= Time.fixedDeltaTime;
            if (remainingPhaseDuration > 0f)
            {
                return;
            }

            switch (State)
            {
                case AttackState2D.Startup:
                    BeginActivePhase();
                    break;
                case AttackState2D.Active:
                    EndActivePhase();
                    break;
                case AttackState2D.Recovery:
                    State = AttackState2D.Idle;
                    remainingPhaseDuration = 0f;
                    break;
            }

            if (activeHitboxPendingEnable)
            {
                EnableActiveHitbox();
            }
        }

        private void BeginActivePhase()
        {
            State = AttackState2D.Active;
            remainingPhaseDuration = attackDefinition.ActiveDuration;
            activeHitboxPendingEnable = true;
        }

        private void EnableActiveHitbox()
        {
            activeHitboxPendingEnable = false;
            attackHitbox.BeginActivation(attackDefinition, attacker, facingDirection);
        }

        private void EndActivePhase()
        {
            attackHitbox.EndActivation();
            if (attackDefinition.RecoveryDuration <= 0f)
            {
                State = AttackState2D.Idle;
                remainingPhaseDuration = 0f;
                return;
            }

            State = AttackState2D.Recovery;
            remainingPhaseDuration = attackDefinition.RecoveryDuration;
        }

        private void ReportInvalidDefinition()
        {
            if (invalidDefinitionReported)
            {
                return;
            }

            invalidDefinitionReported = true;
            Debug.LogError("AttackController2D requires a valid AttackDefinition.", this);
        }

        private void ReportInvalidHitbox()
        {
            if (invalidHitboxReported)
            {
                return;
            }

            invalidHitboxReported = true;
            Debug.LogError("AttackController2D requires a child BoxCollider2D trigger.", this);
        }

        private void OnDisable()
        {
            attackHitbox?.EndActivation();
            activeHitboxPendingEnable = false;
            remainingPhaseDuration = 0f;
            State = AttackState2D.Idle;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
