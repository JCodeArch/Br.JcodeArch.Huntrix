using System;
using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageReceiver2D), typeof(AttackHitbox2D))]
    public sealed class AttackController2D : MonoBehaviour
    {
        [SerializeField] private CombatComboDefinition comboDefinition;

        private AttackHitbox2D attackHitbox;
        private DamageReceiver2D attacker;
        private IReadOnlyList<ComboStep> activeSequence;
        private float remainingPhaseDuration;
        private float stepElapsedTime;
        private float facingDirection;
        private bool invalidDefinitionReported;
        private bool invalidHitboxReported;
        private bool activeHitboxPendingEnable;
        private Action<CombatImpactEvent> impactOccurred;
        private Action<CombatImpactEvent>[] impactListeners = Array.Empty<Action<CombatImpactEvent>>();

        public AttackState2D State { get; private set; } = AttackState2D.Idle;
        public int CurrentComboStepIndex { get; private set; }

        public event Action<CombatImpactEvent> ImpactOccurred
        {
            add
            {
                if (value == null) return;
                impactOccurred += value;
                RefreshImpactListeners();
            }
            remove
            {
                if (value == null) return;
                impactOccurred -= value;
                RefreshImpactListeners();
            }
        }

        private void Awake()
        {
            attacker = GetComponent<DamageReceiver2D>();
            attackHitbox = GetComponent<AttackHitbox2D>();
        }

        private void OnEnable()
        {
            if (attackHitbox == null)
            {
                attackHitbox = GetComponent<AttackHitbox2D>();
            }

            if (attackHitbox != null)
            {
                attackHitbox.AcceptedHit -= HandleAcceptedHit;
                attackHitbox.AcceptedHit += HandleAcceptedHit;
            }
        }

        /// <summary>
        /// Starts a grounded combo. Retained for callers that do not provide a ground check.
        /// </summary>
        public bool TryStartAttack(float facingDirection) => TryStartAttack(facingDirection, true);

        /// <summary>
        /// Starts a combo while idle, or submits one logical Attack press while a step is active.
        /// The ground context is sampled only when an idle chain begins.
        /// </summary>
        public bool TryStartAttack(float facingDirection, bool isGrounded)
        {
            if (!isActiveAndEnabled || !IsFinite(facingDirection) || facingDirection == 0f ||
                attacker == null || !attacker.IsConfigurationValid || attackHitbox == null)
            {
                return false;
            }

            if (!attackHitbox.IsConfigurationValid)
            {
                ReportInvalidHitbox();
                return false;
            }

            if (State != AttackState2D.Idle)
            {
                return TryAdvanceCombo(facingDirection);
            }

            if (comboDefinition == null || !comboDefinition.IsValid(out _))
            {
                ReportInvalidDefinition();
                return false;
            }

            activeSequence = isGrounded ? comboDefinition.GroundedSteps : comboDefinition.AerialSteps;
            CurrentComboStepIndex = 0;
            stepElapsedTime = 0f;
            this.facingDirection = Mathf.Sign(facingDirection);
            BeginCurrentStep();
            return true;
        }

        private bool TryAdvanceCombo(float nextFacingDirection)
        {
            if (activeSequence == null || CurrentComboStepIndex >= activeSequence.Count - 1)
            {
                return false;
            }

            ComboStep currentStep = activeSequence[CurrentComboStepIndex];
            if (stepElapsedTime < currentStep.ChainWindowStart || stepElapsedTime >= currentStep.ChainWindowEnd)
            {
                return false;
            }

            // Close and clear this step before configuring the next step's independent activation.
            attackHitbox.EndActivation();
            activeHitboxPendingEnable = false;
            CurrentComboStepIndex++;
            stepElapsedTime = 0f;
            facingDirection = Mathf.Sign(nextFacingDirection);
            BeginCurrentStep();
            return true;
        }

        private void FixedUpdate()
        {
            if (State == AttackState2D.Idle)
            {
                return;
            }

            stepElapsedTime += Time.fixedDeltaTime;
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
                    FinishChain();
                    break;
            }

            if (activeHitboxPendingEnable)
            {
                EnableActiveHitbox();
            }
        }

        private void BeginCurrentStep()
        {
            ComboStep step = activeSequence[CurrentComboStepIndex];
            if (step.Attack.StartupDuration <= 0f)
            {
                BeginActivePhase();
            }
            else
            {
                State = AttackState2D.Startup;
                remainingPhaseDuration = step.Attack.StartupDuration;
            }
        }

        private void BeginActivePhase()
        {
            State = AttackState2D.Active;
            remainingPhaseDuration = activeSequence[CurrentComboStepIndex].Attack.ActiveDuration;
            activeHitboxPendingEnable = true;
        }

        private void EnableActiveHitbox()
        {
            activeHitboxPendingEnable = false;
            ComboStep step = activeSequence[CurrentComboStepIndex];
            attackHitbox.BeginActivation(step.Attack, attacker, facingDirection,
                CurrentComboStepIndex, step.HitStopDuration);
        }

        private void EndActivePhase()
        {
            attackHitbox.EndActivation();
            ComboStep step = activeSequence[CurrentComboStepIndex];
            if (step.Attack.RecoveryDuration <= 0f)
            {
                FinishChain();
                return;
            }

            State = AttackState2D.Recovery;
            remainingPhaseDuration = step.Attack.RecoveryDuration;
        }

        private void FinishChain()
        {
            attackHitbox.EndActivation();
            State = AttackState2D.Idle;
            activeSequence = null;
            CurrentComboStepIndex = 0;
            stepElapsedTime = 0f;
            remainingPhaseDuration = 0f;
            activeHitboxPendingEnable = false;
        }

        private void ReportInvalidDefinition()
        {
            if (invalidDefinitionReported)
            {
                return;
            }

            invalidDefinitionReported = true;
            Debug.LogError("AttackController2D requires a valid CombatComboDefinition.", this);
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
            if (attackHitbox != null)
            {
                attackHitbox.AcceptedHit -= HandleAcceptedHit;
            }
            activeSequence = null;
            activeHitboxPendingEnable = false;
            remainingPhaseDuration = 0f;
            stepElapsedTime = 0f;
            CurrentComboStepIndex = 0;
            State = AttackState2D.Idle;
        }

        private void HandleAcceptedHit(CombatImpactEvent impact, float hitStopDuration)
        {
            if (hitStopDuration > 0f)
            {
                HitStopService.EnsureInstance().RequestHitStop(hitStopDuration);
            }

            Action<CombatImpactEvent>[] listeners = impactListeners;
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    listeners[i](impact);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void RefreshImpactListeners()
        {
            if (impactOccurred == null)
            {
                impactListeners = Array.Empty<Action<CombatImpactEvent>>();
                return;
            }

            Delegate[] invocationList = impactOccurred.GetInvocationList();
            var listeners = new Action<CombatImpactEvent>[invocationList.Length];
            for (int i = 0; i < invocationList.Length; i++)
            {
                listeners[i] = (Action<CombatImpactEvent>)invocationList[i];
            }
            impactListeners = listeners;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
