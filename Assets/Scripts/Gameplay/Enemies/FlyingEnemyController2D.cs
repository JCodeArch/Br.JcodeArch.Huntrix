using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyAgent2D))]
    public sealed class FlyingEnemyController2D : MonoBehaviour
    {
        [SerializeField] private FlyingEnemyDefinition definition;
        [SerializeField] private LayerMask collisionMask = ~0;
        private readonly List<EnemyProjectile2D> projectiles = new List<EnemyProjectile2D>(4);
        private EnemyAgent2D agent;
        private Rigidbody2D body;
        private float altitudePhase, diveRemaining;
        private Vector2 diveDirection;
        private bool diving, nextIsDive = true;
        public FlyingEnemyDefinition Definition => definition;
        public bool IsDiving => diving;
        private void Awake()
        {
            agent = GetComponent<EnemyAgent2D>(); body = GetComponent<Rigidbody2D>();
            if (definition != null && definition.IsValid(out _)) agent.TryConfigure(definition.Behavior, out _);
        }
        private void OnEnable() { if (agent != null && agent.Self != null) agent.Self.Died += HandleDeath; }
        private void OnDisable()
        {
            if (agent != null && agent.Self != null) agent.Self.Died -= HandleDeath;
            CancelActions();
        }
        private void HandleDeath(DamageReceiver2D actor) => CancelActions();
        private void CancelActions()
        {
            diving = false; diveRemaining = 0f;
            for (int i = projectiles.Count - 1; i >= 0; i--) if (projectiles[i] != null) projectiles[i].Cancel();
            projectiles.Clear();
            if (agent != null) agent.Stop();
        }
        private void FixedUpdate()
        {
            if (definition == null || !definition.IsValid(out _) || body == null || !agent.HasValidTarget)
            { CancelActions(); return; }
            for (int i = projectiles.Count - 1; i >= 0; i--) if (projectiles[i] == null || !projectiles[i].IsFlying) projectiles.RemoveAt(i);
            agent.Tick(Time.fixedDeltaTime);
            if (!isActiveAndEnabled || !agent.HasValidTarget) { CancelActions(); return; }
            if (diving && agent.State != EnemyState2D.Attacking)
            { diving = false; diveRemaining = 0f; body.linearVelocity = Vector2.zero; }
            if (diving)
            {
                diveRemaining = Mathf.Max(0f, diveRemaining - Time.fixedDeltaTime);
                Vector2 next = body.position + diveDirection * definition.DiveSpeed * Time.fixedDeltaTime;
                if (diveRemaining <= 0f || !agent.HasLineOfSight(next) ||
                    Vector2.Distance(body.position, agent.Target.transform.position) <=
                    Mathf.Min(definition.Behavior.Attack.HitboxSize.x, definition.Behavior.Attack.HitboxSize.y) * 0.5f)
                {
                    body.linearVelocity = Vector2.zero; diving = false;
                    if (agent.State == EnemyState2D.Attacking) agent.TryExecuteMelee();
                    return;
                }
                body.linearVelocity = diveDirection * definition.DiveSpeed; return;
            }
            if (agent.State == EnemyState2D.Attacking)
            {
                if (nextIsDive)
                {
                    Vector2 offset = (Vector2)agent.Target.transform.position - body.position;
                    diveDirection = offset.sqrMagnitude <= 0.0001f ? Vector2.down : offset.normalized;
                    diving = true; diveRemaining = definition.DiveDuration; nextIsDive = false;
                }
                else
                {
                    if (projectiles.Count < 4)
                    {
                        GameObject instance = Instantiate(definition.ProjectilePrefab, body.position, Quaternion.identity);
                        instance.SetActive(true);
                        if (!isActiveAndEnabled || !agent.HasValidTarget || agent.State != EnemyState2D.Attacking)
                        { instance.SetActive(false); Destroy(instance); return; }
                        EnemyProjectile2D projectile = instance.GetComponent<EnemyProjectile2D>();
                        if (projectile != null && projectile.TryLaunch(definition.Behavior.Attack, agent.Self, body.position,
                            (Vector2)agent.Target.transform.position - body.position, definition.ProjectileSpeed,
                            definition.ProjectileLifetime, definition.ProjectileRange, collisionMask)) projectiles.Add(projectile);
                        else Destroy(instance);
                    }
                    nextIsDive = true; agent.BeginRecovery();
                }
                return;
            }
            if (agent.State == EnemyState2D.Idle || agent.State == EnemyState2D.Chasing)
            {
                altitudePhase = Mathf.Repeat(altitudePhase + Time.fixedDeltaTime * definition.AltitudeFrequency, Mathf.PI * 2f);
                DamageReceiver2D chaseTarget = agent.Target;
                Vector2 destination = chaseTarget.transform.position;
                destination.y += definition.HoverHeight + Mathf.Sin(altitudePhase) * definition.AltitudeAmplitude;
                // Return to the hover band after a dive before telegraphing the next attack.
                bool inHoverBand = body.position.y >= agent.Target.transform.position.y +
                    definition.HoverHeight - definition.AltitudeAmplitude - 0.1f;
                bool began = inHoverBand && agent.TryBeginTelegraph();
                if (!began && isActiveAndEnabled && agent.HasValidTarget && agent.Target == chaseTarget)
                    agent.MoveTowardsPoint(destination, true);
            }
        }
    }
}
