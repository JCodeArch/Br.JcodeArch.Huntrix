using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Shared explicit-target combat state. Exactly one behaviour controller must drive Tick.</summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DamageReceiver2D), typeof(Rigidbody2D))]
    public sealed class EnemyAgent2D : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private DamageReceiver2D target;
        [SerializeField] private LayerMask collisionMask = ~0;
        private readonly Collider2D[] overlaps = new Collider2D[32];
        private readonly RaycastHit2D[] rays = new RaycastHit2D[32];
        private readonly HashSet<DamageReceiver2D> resolved = new HashSet<DamageReceiver2D>();
        private Rigidbody2D body;
        private float timer;
        private float facing = 1f;
        private bool resolving;
        private bool configuring;
        public EnemyDefinition Definition => definition;
        public DamageReceiver2D Self { get; private set; }
        public DamageReceiver2D Target => target;
        public EnemyState2D State { get; private set; }
        public bool HasValidTarget => Operational && target != null && target.isActiveAndEnabled &&
            target.IsAlive && target.Faction != Self.Faction && target.transform.root != transform.root &&
            Finite(target.transform.position.x) && Finite(target.transform.position.y);
        public bool IsConfigurationValid => Self != null && Self.IsConfigurationValid && body != null &&
            body.bodyType == RigidbodyType2D.Dynamic && definition != null && definition.IsValid(out _);
        private bool Operational => isActiveAndEnabled && IsConfigurationValid && Self.isActiveAndEnabled && Self.IsAlive &&
            Finite(body.position.x) && Finite(body.position.y);
        public event Action<EnemyState2D> StateChanged;
        public event Action<CombatImpactEvent> ImpactOccurred;
        public event Action<CombatParryEvent> ParryOccurred;

        private void Awake() { Self = GetComponent<DamageReceiver2D>(); body = GetComponent<Rigidbody2D>(); }
        private void OnEnable()
        {
            if (Self != null) Self.Died += HandleDeath;
            SetState(Self != null && Self.IsConfigurationValid && !Self.IsAlive ? EnemyState2D.Dead : EnemyState2D.Idle);
        }
        private void OnDisable()
        {
            if (Self != null) Self.Died -= HandleDeath;
            Stop();
        }
        private void HandleDeath(DamageReceiver2D actor) { timer = 0f; StopMotion(); SetState(EnemyState2D.Dead); }
        public bool TryConfigure(EnemyDefinition value, out string error)
        {
            error = string.Empty;
            if (value == null || !value.IsValid(out error)) { if (string.IsNullOrEmpty(error)) error = "Enemy definition is required."; return false; }
            if (!isActiveAndEnabled || Self == null || !Self.IsConfigurationValid || !Self.IsAlive ||
                resolving || configuring || State == EnemyState2D.Telegraph || State == EnemyState2D.Attacking || State == EnemyState2D.Recovering || State == EnemyState2D.Dead)
            { error = "Cannot configure an active or dead enemy action."; return false; }
            configuring = true;
            try
            {
                Stop();
                if (!isActiveAndEnabled || Self == null || !Self.isActiveAndEnabled || !Self.IsAlive || State != EnemyState2D.Idle) return false;
                definition = value; return true;
            }
            finally { configuring = false; }
        }
        public bool TrySetTarget(DamageReceiver2D value)
        {
            if (resolving || configuring || !Operational || (value != null && (!value.isActiveAndEnabled || !value.IsAlive ||
                value.Faction == Self.Faction || value.transform.root == transform.root))) return false;
            if (value == target) return true;
            configuring = true;
            try
            {
                Stop();
                if (!Operational) return false;
                target = value; return true;
            }
            finally { configuring = false; }
        }
        public void Stop()
        {
            timer = 0f; StopMotion();
            SetState(Self != null && Self.IsConfigurationValid && !Self.IsAlive ? EnemyState2D.Dead : EnemyState2D.Idle);
        }
        public void Tick(float deltaTime)
        {
            if (configuring || !Finite(deltaTime) || deltaTime < 0f) return;
            if (!Operational || !HasValidTarget) { Stop(); return; }
            if (State == EnemyState2D.Telegraph || State == EnemyState2D.Recovering)
            {
                timer = Mathf.Max(0f, timer - deltaTime);
                if (timer <= 0f) SetState(State == EnemyState2D.Telegraph ? EnemyState2D.Attacking : EnemyState2D.Idle);
            }
        }
        public void MoveTowardsTarget(bool flying = false)
        {
            if (HasValidTarget) MoveTowardsPoint(target.transform.position, flying); else Stop();
        }
        public void MoveTowardsPoint(Vector2 destination, bool flying = false)
        {
            if (configuring || !HasValidTarget || !Finite(destination.x) || !Finite(destination.y) ||
                (State != EnemyState2D.Idle && State != EnemyState2D.Chasing)) return;
            Vector2 offset = destination - body.position;
            if (offset.sqrMagnitude > definition.DetectionRange * definition.DetectionRange) { StopMotion(); SetState(EnemyState2D.Idle); return; }
            Vector2 velocity = offset.sqrMagnitude <= 0.01f ? Vector2.zero : offset.normalized * definition.MoveSpeed;
            body.linearVelocity = flying ? velocity : new Vector2(velocity.x, body.linearVelocity.y);
            SetState(EnemyState2D.Chasing);
        }
        public bool TryBeginTelegraph()
        {
            if (configuring || !HasValidTarget || (State != EnemyState2D.Idle && State != EnemyState2D.Chasing) ||
                Vector2.Distance(body.position, target.transform.position) > definition.AttackRange ||
                !HasLineOfSight(target.transform.position)) return false;
            facing = target.transform.position.x < transform.position.x ? -1f : 1f;
            StopMotion(); timer = definition.TelegraphDuration; SetState(EnemyState2D.Telegraph);
            return Operational && State == EnemyState2D.Telegraph;
        }
        /// <summary>Reserve recovery before callbacks; shared contact resolution keeps parry/protection semantics.</summary>
        public bool TryExecuteMelee()
        {
            if (configuring || !HasValidTarget || State != EnemyState2D.Attacking || resolving) return false;
            AttackDefinition attack = definition.Attack;
            Vector2 center = (Vector2)transform.position + new Vector2(attack.HitboxOffset.x * facing, attack.HitboxOffset.y);
            ContactFilter2D filter = Filter();
            int count = Physics2D.OverlapBox(center, attack.HitboxSize, 0f, filter, overlaps);
            resolving = true; resolved.Clear();
            try
            {
                if (!BeginRecovery() || count == overlaps.Length) return false;
                for (int i = 0; i < count; i++)
                {
                    if (!Operational || State != EnemyState2D.Recovering) break;
                    Collider2D collider = overlaps[i];
                    if (collider == null || collider.transform.root == transform.root) continue;
                    Hurtbox2D hurtbox = collider.GetComponentInParent<Hurtbox2D>();
                    if (hurtbox == null || !hurtbox.isActiveAndEnabled || hurtbox.Receiver == null ||
                        !hurtbox.Receiver.isActiveAndEnabled || !hurtbox.Receiver.IsAlive || !resolved.Add(hurtbox.Receiver) ||
                        !HasLineOfSight(hurtbox.Receiver.transform.position)) continue;
                    CombatContactResult result = hurtbox.ResolveHit(attack, Self, facing, 0,
                        out DamageReceiver2D receiver, out CombatParryEvent parryEvent);
                    if (result == CombatContactResult.Damaged) PublishImpact(new CombatImpactEvent(Self, receiver, attack, 0));
                    else if (result == CombatContactResult.Parried) PublishParry(parryEvent);
                }
                return true;
            }
            finally { resolving = false; resolved.Clear(); }
        }
        public bool BeginRecovery()
        {
            if (configuring || !Operational || State != EnemyState2D.Attacking) return false;
            timer = definition.RecoveryDuration; StopMotion(); SetState(EnemyState2D.Recovering);
            return Operational && State == EnemyState2D.Recovering;
        }
        public bool HasLineOfSight(Vector2 destination)
        {
            if (!Operational || !Finite(destination.x) || !Finite(destination.y)) return false;
            Vector2 offset = destination - body.position;
            int count = Physics2D.Raycast(body.position, offset.normalized, Filter(), rays, offset.magnitude);
            if (count == rays.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = rays[i].collider;
                if (collider == null || collider.transform.root == transform.root || collider.isTrigger ||
                    collider.GetComponentInParent<Hurtbox2D>() != null) continue;
                return false;
            }
            return true;
        }
        private ContactFilter2D Filter() { ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(collisionMask); filter.useTriggers = true; return filter; }
        private void StopMotion()
        {
            if (body != null) body.linearVelocity = Mathf.Approximately(body.gravityScale, 0f) ? Vector2.zero : new Vector2(0f, body.linearVelocity.y);
        }
        private void SetState(EnemyState2D state)
        {
            if (State == state) return;
            State = state;
            Action<EnemyState2D> handlers = StateChanged;
            if (handlers == null) return;
            foreach (Action<EnemyState2D> handler in handlers.GetInvocationList()) { try { handler(state); } catch (Exception e) { Debug.LogException(e, this); } }
        }
        private void PublishImpact(CombatImpactEvent impact)
        {
            Action<CombatImpactEvent> handlers = ImpactOccurred;
            if (handlers == null) return;
            foreach (Action<CombatImpactEvent> handler in handlers.GetInvocationList()) { try { handler(impact); } catch (Exception e) { Debug.LogException(e, this); } }
        }
        private void PublishParry(CombatParryEvent value)
        {
            Action<CombatParryEvent> handlers = ParryOccurred;
            if (handlers == null) return;
            foreach (Action<CombatParryEvent> handler in handlers.GetInvocationList()) { try { handler(value); } catch (Exception e) { Debug.LogException(e, this); } }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
