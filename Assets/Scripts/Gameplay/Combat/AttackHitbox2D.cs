using System;
using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class AttackHitbox2D : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D hitboxCollider;

        private readonly HashSet<DamageReceiver2D> resolvedReceivers = new HashSet<DamageReceiver2D>();
        private AttackDefinition attack;
        private DamageReceiver2D attacker;
        private float facingDirection;
        private int comboStepIndex;
        private float hitStopDuration;
        private bool activationActive;

        internal event Action<CombatImpactEvent, float> AcceptedHit;
        internal event Action<CombatParryEvent> ParriedHit;

        public bool IsConfigurationValid { get; private set; }
        public int ComboStepIndex => comboStepIndex;
        public float HitStopDuration => hitStopDuration;

        private void Awake()
        {
            if (hitboxCollider == null)
            {
                hitboxCollider = GetComponentInChildren<BoxCollider2D>(true);
            }

            IsConfigurationValid = hitboxCollider != null &&
                hitboxCollider.transform != transform && hitboxCollider.transform.IsChildOf(transform) &&
                hitboxCollider.isTrigger;

            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
            }
        }

        public void BeginActivation(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection,
            int comboStepIndex = 0, float hitStopDuration = 0f)
        {
            EndActivation();
            if (!IsConfigurationValid || hitboxCollider == null || attack == null || attacker == null ||
                facingDirection == 0f || !IsFinite(facingDirection) || comboStepIndex < 0 ||
                !IsFiniteNonnegative(hitStopDuration))
            {
                return;
            }

            this.attack = attack;
            this.attacker = attacker;
            this.facingDirection = Mathf.Sign(facingDirection);
            this.comboStepIndex = comboStepIndex;
            this.hitStopDuration = hitStopDuration;
            resolvedReceivers.Clear();
            activationActive = true;
            hitboxCollider.size = attack.HitboxSize;
            hitboxCollider.offset = new Vector2(attack.HitboxOffset.x * this.facingDirection,
                attack.HitboxOffset.y);
            hitboxCollider.enabled = true;
        }

        public void EndActivation()
        {
            activationActive = false;
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
            }
            resolvedReceivers.Clear();
            attack = null;
            attacker = null;
            comboStepIndex = 0;
            hitStopDuration = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other) => ProcessContact(other);

        private void OnTriggerStay2D(Collider2D other) => ProcessContact(other);

        private void ProcessContact(Collider2D other)
        {
            if (!activationActive || attack == null || attacker == null || other == null)
            {
                return;
            }

            Hurtbox2D hurtbox = other.GetComponentInParent<Hurtbox2D>();
            if (hurtbox == null || hurtbox.Receiver == null || resolvedReceivers.Contains(hurtbox.Receiver))
            {
                return;
            }

            CombatContactResult result = hurtbox.ResolveHit(attack, attacker, facingDirection, comboStepIndex,
                out DamageReceiver2D receiver, out CombatParryEvent parryEvent);
            if (receiver == null ||
                (result != CombatContactResult.Damaged && result != CombatContactResult.Parried &&
                 result != CombatContactResult.Protected))
            {
                return;
            }

            // Reserve the receiver before callbacks so re-entry cannot apply another result in this activation.
            resolvedReceivers.Add(receiver);
            if (result == CombatContactResult.Protected)
            {
                return;
            }
            if (result == CombatContactResult.Parried)
            {
                DispatchParried(parryEvent);
                return;
            }

            DispatchAccepted(new CombatImpactEvent(attacker, receiver, attack, comboStepIndex), hitStopDuration);
        }

        private void DispatchAccepted(CombatImpactEvent impact, float duration) =>
            AcceptedHit?.Invoke(impact, duration);

        private void DispatchParried(CombatParryEvent parryEvent) =>
            ParriedHit?.Invoke(parryEvent);
        private void OnDisable() => EndActivation();

        private static bool IsFiniteNonnegative(float value) => IsFinite(value) && value >= 0f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
