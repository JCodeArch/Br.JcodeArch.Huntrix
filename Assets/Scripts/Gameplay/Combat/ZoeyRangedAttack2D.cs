using System;
using HuntrX.Data;
using HuntrX.Gameplay.Dash;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    /// <summary>Immediate precise ray shot. Input supplies world-space aim, including aerial directions.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageReceiver2D))]
    public sealed class ZoeyRangedAttack2D : MonoBehaviour
    {
        [SerializeField] private ZoeyRangedDefinition definition;
        [SerializeField] private LayerMask collisionMask = ~0;
        private readonly RaycastHit2D[] contacts = new RaycastHit2D[64];
        private DamageReceiver2D attacker;
        private DashController2D dash;
        private AttackController2D melee;
        private ParryController2D parry;
        private bool firing;
        public ZoeyRangedDefinition Definition => definition;
        public float CooldownRemaining { get; private set; }
        public bool IsConfigurationValid => attacker != null && attacker.IsConfigurationValid &&
            definition != null && definition.IsValid(out _);
        public event Action<Vector2, Vector2> ShotFired;
        public event Action<CombatImpactEvent> HitConfirmed;
        public event Action<CombatParryEvent> HitParried;

        private void Awake()
        {
            attacker = GetComponent<DamageReceiver2D>();
            dash = GetComponent<DashController2D>();
            melee = GetComponent<AttackController2D>();
            parry = GetComponent<ParryController2D>();
        }
        private void Update() => CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);
        private void OnDisable() { CooldownRemaining = 0f; }

        public bool TrySetDefinition(ZoeyRangedDefinition value, out string error)
        {
            error = string.Empty;
            if (value == null || !value.IsValid(out error))
            {
                if (string.IsNullOrEmpty(error)) error = "A valid ZoeyRangedDefinition is required.";
                return false;
            }
            if (firing || CooldownRemaining > 0f) { error = "Cannot replace a ranged definition during a shot or cooldown."; return false; }
            definition = value;
            return true;
        }

        public bool TryFire(Vector2 aimDirection)
        {
            if (!isActiveAndEnabled || firing || !IsConfigurationValid || !attacker.IsAlive ||
                CooldownRemaining > 0f || (dash != null && dash.IsDashing) ||
                (melee != null && melee.State != AttackState2D.Idle) ||
                (parry != null && parry.IsWindowActive) || !Finite(aimDirection.x) ||
                !Finite(aimDirection.y)) return false;
            // Scale first so even very large finite directions can be normalized safely.
            float scale = Mathf.Max(Mathf.Abs(aimDirection.x), Mathf.Abs(aimDirection.y));
            if (scale <= 0f) return false;
            Vector2 direction = (aimDirection / scale).normalized;
            Vector2 origin = (Vector2)transform.position + definition.MuzzleOffset;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(collisionMask);
            filter.useTriggers = true;
            int count = Physics2D.Raycast(origin, direction, filter, contacts, definition.Range);
            // Saturation is conservatively rejected: a missing blocker must never permit a shot through walls.
            if (count == contacts.Length) return false;
            firing = true;
            CooldownRemaining = definition.Cooldown;
            Vector2 end = origin + direction * definition.Range;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Collider2D collider = contacts[i].collider;
                    if (collider == null || collider.transform.root == transform.root) continue;
                    Hurtbox2D hurtbox = collider.GetComponentInParent<Hurtbox2D>();
                    if (hurtbox != null && hurtbox.Receiver != null)
                    {
                        end = contacts[i].point;
                        float facing = direction.x < 0f ? -1f : 1f;
                        CombatContactResult result = hurtbox.ResolveHit(definition.Attack, attacker, facing, 0,
                            out DamageReceiver2D receiver, out CombatParryEvent parryEvent);
                        if (result == CombatContactResult.Damaged)
                            HitConfirmed?.Invoke(new CombatImpactEvent(attacker, receiver, definition.Attack, 0));
                        else if (result == CombatContactResult.Parried) HitParried?.Invoke(parryEvent);
                        break;
                    }
                    if (!collider.isTrigger) { end = contacts[i].point; break; }
                }
                ShotFired?.Invoke(origin, end);
                return true;
            }
            finally { firing = false; }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
