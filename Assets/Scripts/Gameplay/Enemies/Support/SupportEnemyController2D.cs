using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Protection;
using UnityEngine;
namespace HuntrX.Gameplay.Enemies
{
    [DisallowMultipleComponent, RequireComponent(typeof(EnemyAgent2D))]
    public sealed class SupportEnemyController2D : MonoBehaviour, IDamageProtectionSource2D
    {
        [SerializeField] private SupportEnemyDefinition definition;
        [SerializeField] private LayerMask detectionMask = ~0;
        private readonly Collider2D[] contacts = new Collider2D[32];
        private readonly HashSet<DamageReceiver2D> visited = new HashSet<DamageReceiver2D>();
        private readonly List<DamageProtection2D> protectedTargets = new List<DamageProtection2D>(32);
        private EnemyAgent2D agent;
        private float cooldown;
        private float protectionRemaining;
        private bool pulsing;
        public event Action<DamageReceiver2D, float> DrainAccepted;
        private void Awake() => agent = GetComponent<EnemyAgent2D>();
        private void Update()
        {
            if (agent == null || agent.Self == null || !agent.IsConfigurationValid || !agent.Self.IsAlive || definition == null || !definition.IsValid(out _))
            { ClearProtection(); return; }
            if (Time.deltaTime <= 0f) return;
            agent.Tick(Time.deltaTime);
            protectionRemaining = Mathf.Max(0f, protectionRemaining - Time.deltaTime);
            if (protectionRemaining <= 0f) ClearProtection();
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
            if (cooldown <= 0f) TryPulse();
        }
        public bool TryPulse()
        {
            if (!isActiveAndEnabled || pulsing || cooldown > 0f || agent == null || agent.Self == null ||
                !agent.IsConfigurationValid || !agent.Self.isActiveAndEnabled || !agent.Self.IsAlive || agent.Self.Faction != CombatFaction2D.Demon ||
                definition == null || !definition.IsValid(out _)) return false;
            cooldown = definition.Interval;
            pulsing = true;
            try
            {
                if (definition.Role == SupportEnemyRole.SoulDrainer) return DrainTarget();
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(detectionMask); filter.useTriggers = true;
                int count = Physics2D.OverlapCircle(transform.position, definition.Radius, filter, contacts);
                if (count == contacts.Length) return false; // reject incomplete candidate sets
                visited.Clear();
                if (definition.Role == SupportEnemyRole.Protector) ClearProtection();
                for (int i = 0; i < count; i++)
                {
                    DamageReceiver2D receiver = contacts[i].GetComponentInParent<DamageReceiver2D>();
                    if (!EligibleAlly(receiver) || receiver == agent.Self || !visited.Add(receiver)) continue;
                    if (definition.Role == SupportEnemyRole.Healer)
                        receiver.TryRestoreHealth(definition.HealingAmount, agent.Self);
                    else
                    {
                        DamageProtection2D protection = receiver.GetComponent<DamageProtection2D>();
                        if (protection != null && protection.RegisterExternalSource(this)) protectedTargets.Add(protection);
                    }
                }
                if (definition.Role == SupportEnemyRole.Protector) protectionRemaining = definition.ProtectionDuration;
                return true;
            }
            finally { pulsing = false; }
        }
        public bool Covers(DamageReceiver2D receiver) => isActiveAndEnabled && agent != null && agent.IsConfigurationValid && agent.Self != null &&
            agent.Self.isActiveAndEnabled && agent.Self.IsAlive && definition != null && definition.IsValid(out _) &&
            definition.Role == SupportEnemyRole.Protector && protectionRemaining > 0f && EligibleAlly(receiver);
        private bool EligibleAlly(DamageReceiver2D receiver) => receiver != null && receiver.isActiveAndEnabled &&
            receiver.IsAlive && receiver.Faction == agent.Self.Faction &&
            Vector2.Distance(transform.position, receiver.transform.position) <= definition.Radius;
        private bool DrainTarget()
        {
            DamageReceiver2D target = agent.Target;
            if (target == null || !target.isActiveAndEnabled || !target.IsAlive || target.Faction == agent.Self.Faction ||
                Vector2.Distance(transform.position, target.transform.position) > definition.Radius) return false;
            Vector3 sourcePosition = transform.position;
            Vector3 targetPosition = target.transform.position;
            if (!Finite(sourcePosition.x) || !Finite(sourcePosition.y) || !Finite(targetPosition.x) || !Finite(targetPosition.y)) return false;
            float before = target.CurrentHealth;
            float facing = target.transform.position.x < transform.position.x ? -1f : 1f;
            if (!target.TryReceiveHit(definition.DrainAttack, agent.Self, facing)) return false;
            float lost = Mathf.Max(0f, before - target.CurrentHealth);
            Action<DamageReceiver2D, float> handlers = DrainAccepted;
            if (handlers != null)
                foreach (Action<DamageReceiver2D, float> handler in handlers.GetInvocationList())
                    try { handler(target, lost); } catch (Exception exception) { Debug.LogException(exception, this); }
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void ClearProtection()
        {
            foreach (DamageProtection2D target in protectedTargets) if (target != null) target.UnregisterExternalSource(this);
            protectedTargets.Clear(); protectionRemaining = 0f;
        }
        private void OnDisable() { ClearProtection(); cooldown = 0f; visited.Clear(); }
        private void OnDestroy() => ClearProtection();
    }
}
