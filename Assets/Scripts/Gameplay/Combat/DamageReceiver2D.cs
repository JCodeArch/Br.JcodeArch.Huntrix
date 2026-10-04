using UnityEngine;
using HuntrX.Data;
using HuntrX.Gameplay.Dash;

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
        private bool configurationReported;

        public CombatFaction2D Faction => faction;
        public float MaximumHealth => maximumHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => IsConfigurationValid && CurrentHealth > 0f;
        public bool IsConfigurationValid { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            dashController = GetComponent<DashController2D>();
            InitializeConfiguration();
        }

        public bool TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)
        {
            if (!IsConfigurationValid || !IsAlive || attacker == null || !attacker.IsConfigurationValid ||
                attack == null || !attack.IsValid(out _) || attacker.transform.root == transform.root ||
                attacker.Faction == Faction || !IsFinite(facingDirection) || facingDirection == 0f ||
                (dashController != null && dashController.IsInvulnerable))
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - attack.Damage);
            float horizontalDistance = transform.position.x - attacker.transform.position.x;
            float horizontalDirection = Mathf.Approximately(horizontalDistance, 0f)
                ? Mathf.Sign(facingDirection)
                : Mathf.Sign(horizontalDistance);

            Vector2 impulse = new Vector2(
                horizontalDirection * attack.HorizontalKnockbackImpulse,
                attack.UpwardKnockbackImpulse);
            body.AddForce(impulse, ForceMode2D.Impulse);
            return true;
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
