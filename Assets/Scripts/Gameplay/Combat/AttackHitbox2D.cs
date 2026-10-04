using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class AttackHitbox2D : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D hitboxCollider;

        private readonly HashSet<DamageReceiver2D> acceptedReceivers = new HashSet<DamageReceiver2D>();
        private AttackDefinition attack;
        private DamageReceiver2D attacker;
        private float facingDirection;
        private int comboStepIndex;
        private float hitStopDuration;
        private bool activationActive;

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
            acceptedReceivers.Clear();
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
            acceptedReceivers.Clear();
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
            if (hurtbox == null || hurtbox.Receiver == null || acceptedReceivers.Contains(hurtbox.Receiver))
            {
                return;
            }

            if (hurtbox.TryReceiveHit(attack, attacker, facingDirection, out DamageReceiver2D receiver) && receiver != null)
            {
                acceptedReceivers.Add(receiver);
            }
        }

        private void OnDisable() => EndActivation();

        private static bool IsFiniteNonnegative(float value) => IsFinite(value) && value >= 0f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
