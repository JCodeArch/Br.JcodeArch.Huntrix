using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent, RequireComponent(typeof(EnemyAgent2D))]
    public sealed class FanSoulDrainController2D : MonoBehaviour
    {
        [SerializeField] private FanDrainDefinition definition;
        [SerializeField] private FanRegistry2D registry;
        [SerializeField] private LayerMask blockerMask = ~0;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        private EnemyAgent2D agent;
        private FanActor2D target;
        private float timer;
        private bool resolving;
        public bool IsTelegraphing { get; private set; }
        public event Action<FanActor2D, float> FanDrainAccepted;
        private void Awake() => agent = GetComponent<EnemyAgent2D>();
        public bool TryBindRegistry(FanRegistry2D value)
        {
            if (resolving || IsTelegraphing || value == null) return false;
            registry = value; return true;
        }
        private void Update()
        {
            if (!Operational) { Reset(); return; }
            if (Time.deltaTime <= 0f) return;
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            if (IsTelegraphing)
            {
                if (!Eligible(target)) { Reset(); timer = definition.Cooldown; return; }
                if (timer <= 0f) Resolve();
            }
            else if (timer <= 0f)
            {
                target = FindNearest();
                if (target == null) { timer = definition.Cooldown; return; }
                IsTelegraphing = true; timer = definition.TelegraphDuration;
            }
        }
        private bool Operational => isActiveAndEnabled && !resolving && agent != null && agent.IsConfigurationValid &&
            agent.Self != null && agent.Self.isActiveAndEnabled && agent.Self.IsAlive && agent.Self.Faction == CombatFaction2D.Demon &&
            definition != null && definition.IsValid(out _) && registry != null && registry.isActiveAndEnabled;
        private FanActor2D FindNearest()
        {
            FanActor2D nearest = null; float distance = float.PositiveInfinity;
            for (int i = 0; i < registry.Count; i++)
            {
                FanActor2D candidate = registry.GetAt(i);
                if (!Eligible(candidate)) continue;
                float squared = ((Vector2)(candidate.transform.position - transform.position)).sqrMagnitude;
                if (squared < distance) { nearest = candidate; distance = squared; }
            }
            return nearest;
        }
        private bool Eligible(FanActor2D fan) => fan != null && fan.isActiveAndEnabled && fan.IsConfigurationValid &&
            fan.Soul > 0f && !fan.IsProtected && Vector2.Distance(transform.position, fan.transform.position) <= definition.Range &&
            HasLineOfSight(fan);
        private bool HasLineOfSight(FanActor2D fan)
        {
            Vector2 origin = transform.position;
            Vector2 offset = (Vector2)fan.transform.position - origin;
            if (offset.sqrMagnitude <= 0.0001f) return true;
            ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(blockerMask); filter.useTriggers = false;
            int count = Physics2D.Raycast(origin, offset.normalized, filter, hits, offset.magnitude);
            if (count == hits.Length) return false;
            for (int i = 0; i < count; i++)
                if (hits[i].collider != null && hits[i].collider.transform.root != transform.root &&
                    hits[i].collider.GetComponentInParent<FanActor2D>() != fan) return false;
            return true;
        }
        private void Resolve()
        {
            if (resolving || !Operational || !Eligible(target)) return;
            resolving = true; IsTelegraphing = false; timer = definition.Cooldown;
            FanActor2D accepted = target; target = null;
            float before = accepted.Soul;
            try
            {
                if (!accepted.TryDrain(definition.SoulPerPulse)) return;
                float lost = Mathf.Max(0f, before - accepted.Soul);
                Action<FanActor2D, float> handlers = FanDrainAccepted;
                if (handlers != null)
                    foreach (Action<FanActor2D, float> handler in handlers.GetInvocationList())
                        try { handler(accepted, lost); } catch (Exception exception) { Debug.LogException(exception, this); }
            }
            finally { resolving = false; }
        }
        private void Reset() { IsTelegraphing = false; target = null; timer = 0f; }
        private void OnDisable() => Reset();
    }
}
