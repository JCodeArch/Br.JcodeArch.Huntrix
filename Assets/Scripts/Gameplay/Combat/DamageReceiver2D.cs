using System;
using UnityEngine;
using HuntrX.Data;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Protection;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DamageReceiver2D : MonoBehaviour
    {
        [SerializeField] private CombatFaction2D faction;
        [SerializeField] private float maximumHealth;

        private Rigidbody2D body;
        private DashController2D dashController;
        private ParryController2D parryController;
        private DamageProtection2D damageProtection;
        private bool configurationReported;

        public CombatFaction2D Faction => faction;
        public float MaximumHealth => maximumHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => IsConfigurationValid && CurrentHealth > 0f;
        public bool IsConfigurationValid { get; private set; }

        /// <summary>Raised once after accepted damage changes this receiver from alive to dead.</summary>
        public event Action<DamageReceiver2D> Died;
        /// <summary>Accepted damage only; published after health/impulse commit, before death notification.</summary>
        public event Action<CombatImpactEvent> DamageDealt;
        public event Action<CombatImpactEvent> DamageReceived;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            dashController = GetComponent<DashController2D>();
            parryController = GetComponent<ParryController2D>();
            damageProtection = GetComponent<DamageProtection2D>();
            InitializeConfiguration();
        }

        /// <summary>Compatibility API: returns true only when damage was applied.</summary>
        public bool TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection) =>
            ResolveHit(attack, attacker, facingDirection, 0, out _) == CombatContactResult.Damaged;

        /// <summary>Restores a living same-faction receiver without reviving or exceeding authored maximum health.</summary>
        public bool TryRestoreHealth(float amount, DamageReceiver2D source)
        {
            if (!isActiveAndEnabled || !IsAlive || source == null || !source.isActiveAndEnabled ||
                !source.IsAlive || source.Faction != Faction || !IsFinite(amount) || amount <= 0f ||
                CurrentHealth >= MaximumHealth) return false;
            CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + amount);
            return true;
        }

        internal void RegisterProtection(DamageProtection2D protection) => damageProtection = protection;

        internal void RegisterParryController(ParryController2D controller)
        {
            if (controller != null)
            {
                parryController = controller;
            }
        }
        internal CombatContactResult ResolveHit(AttackDefinition attack, DamageReceiver2D attacker,
            float facingDirection, int comboStepIndex, out CombatParryEvent parryEvent)
        {
            parryEvent = default;
            if (!IsConfigurationValid || !IsAlive || attacker == null || !attacker.IsConfigurationValid ||
                attack == null || !attack.IsValid(out _) || attacker.transform.root == transform.root ||
                attacker.Faction == Faction || !IsFinite(facingDirection) || facingDirection == 0f ||
                (dashController != null && dashController.IsInvulnerable))
            {
                return CombatContactResult.Rejected;
            }

            if (parryController != null &&
                parryController.TryConsumeParry(attacker, attack, comboStepIndex, out parryEvent))
            {
                return CombatContactResult.Parried;
            }

            if (damageProtection != null && damageProtection.IsProtected)
            {
                return CombatContactResult.Protected;
            }

            float healthBeforeHit = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - attack.Damage);
            bool killedByThisHit = healthBeforeHit > 0f && CurrentHealth <= 0f;
            float horizontalDistance = transform.position.x - attacker.transform.position.x;
            float horizontalDirection = Mathf.Approximately(horizontalDistance, 0f)
                ? Mathf.Sign(facingDirection)
                : Mathf.Sign(horizontalDistance);

            Vector2 impulse = new Vector2(
                horizontalDirection * attack.HorizontalKnockbackImpulse,
                attack.UpwardKnockbackImpulse);
            body.AddForce(impulse, ForceMode2D.Impulse);

            CombatImpactEvent impact = new CombatImpactEvent(attacker, this, attack, comboStepIndex);
            // Capture both subscriptions at the accepted commit boundary; a dealt listener may rebind actors.
            Action<CombatImpactEvent> dealtHandlers = attacker.DamageDealt;
            Action<CombatImpactEvent> receivedHandlers = DamageReceived;
            PublishDamage(dealtHandlers, impact);
            PublishDamage(receivedHandlers, impact);
            if (killedByThisHit) PublishDied();

            return CombatContactResult.Damaged;
        }

        private static void PublishDamage(Action<CombatImpactEvent> handlers, CombatImpactEvent impact)
        {
            if (handlers == null) return;
            foreach (Action<CombatImpactEvent> handler in handlers.GetInvocationList())
                try { handler(impact); } catch (Exception exception) { Debug.LogException(exception); }
        }

        private void PublishDied()
        {
            Action<DamageReceiver2D> handlers = Died;
            if (handlers == null)
            {
                return;
            }

            foreach (Action<DamageReceiver2D> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(this);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
        private void InitializeConfiguration()
        {
            if (faction != CombatFaction2D.HuntrX && faction != CombatFaction2D.Demon)
            {
                IsConfigurationValid = false;
                CurrentHealth = 0f;
                ReportInvalidConfiguration("DamageReceiver2D requires a valid CombatFaction2D value.");
                return;
            }

            if (!IsFinite(maximumHealth) || maximumHealth <= 0f)
            {
                IsConfigurationValid = false;
                CurrentHealth = 0f;
                ReportInvalidConfiguration("DamageReceiver2D requires finite positive maximum health.");
                return;
            }

            if (body == null || body.bodyType != RigidbodyType2D.Dynamic)
            {
                IsConfigurationValid = false;
                CurrentHealth = 0f;
                ReportInvalidConfiguration("DamageReceiver2D requires a dynamic Rigidbody2D component.");
                return;
            }

            IsConfigurationValid = true;
            CurrentHealth = maximumHealth;
        }

        private void ReportInvalidConfiguration(string message)
        {
            if (configurationReported)
            {
                return;
            }

            configurationReported = true;
            Debug.LogError(message, this);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
