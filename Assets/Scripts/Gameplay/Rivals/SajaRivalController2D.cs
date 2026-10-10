using HuntrX.Data;
using HuntrX.Gameplay.Enemies;
using UnityEngine;

namespace HuntrX.Gameplay.Rivals
{
    /// <summary>Provisional distinct rival tactics; explicit target binding belongs to the encounter.</summary>
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent, RequireComponent(typeof(EnemyAgent2D))]
    public sealed class SajaRivalController2D : MonoBehaviour
    {
        [SerializeField] private SajaRivalDefinition definition;
        private EnemyAgent2D agent;
        private Rigidbody2D body;
        private float pause;
        private int burstRemaining;
        private bool configuring;
        public SajaRivalDefinition Definition => definition;
        public bool TrySetDefinition(SajaRivalDefinition profile, out string error)
        {
            if (configuring) { error = "Rival profile configuration is already in progress."; return false; }
            if (profile == null || !profile.IsValid(out error))
            { error = "A valid rival tactic profile is required."; return false; }
            if (agent == null) agent = GetComponent<EnemyAgent2D>();
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (agent == null || body == null)
            { error = "Rival requires an enemy agent and Rigidbody2D."; return false; }
            configuring = true;
            try
            {
                if (!agent.TryConfigure(profile.Enemy, out error)) return false;
                if (agent.Definition != profile.Enemy)
                { error = "Enemy profile changed during configuration."; return false; }
                definition = profile;
                pause = 0f;
                burstRemaining = 0;
                error = string.Empty;
                return true;
            }
            finally { configuring = false; }
        }
        private void Awake()
        {
            agent = GetComponent<EnemyAgent2D>();
            body = GetComponent<Rigidbody2D>();
            if (!TrySetDefinition(definition, out string error))
            { Debug.LogError(error, this); enabled = false; }
        }
        private void FixedUpdate()
        {
            if (definition == null || agent == null || !agent.isActiveAndEnabled) return;
            float dt = Time.fixedDeltaTime;
            agent.Tick(dt);
            if (!isActiveAndEnabled || !agent.HasValidTarget) return;
            if (agent.State == EnemyState2D.Attacking)
            {
                if (agent.TryExecuteMelee() && isActiveAndEnabled && agent.HasValidTarget)
                {
                    burstRemaining--;
                    if (burstRemaining <= 0) pause = definition.PauseBetweenBursts;
                }
                return;
            }
            if (agent.State == EnemyState2D.Telegraph || agent.State == EnemyState2D.Recovering || agent.State == EnemyState2D.Dead) return;
            pause = Mathf.Max(0f, pause - dt);
            float distance = Vector2.Distance(agent.Self.transform.position, agent.Target.transform.position);
            if (distance > definition.Enemy.DetectionRange) { agent.Stop(); return; }
            if (definition.Tactic == SajaTactic.KeepDistance && distance < definition.PreferredDistance)
            {
                float away = Mathf.Sign(agent.Self.transform.position.x - agent.Target.transform.position.x);
                body.linearVelocityX = (away == 0f ? 1f : away) * definition.Enemy.MoveSpeed;
                return;
            }
            if (distance > definition.Enemy.AttackRange)
            {
                if (definition.Tactic == SajaTactic.Ambush) agent.Stop();
                else agent.MoveTowardsTarget();
                return;
            }
            agent.Stop();
            if (!isActiveAndEnabled || !agent.HasValidTarget || pause > 0f) return;
            if (agent.TryBeginTelegraph())
            {
                if (burstRemaining <= 0) burstRemaining = definition.Tactic == SajaTactic.Burst ? definition.BurstCount : 1;
            }
        }
        private void OnDisable()
        {
            pause = 0f;
            burstRemaining = 0;
            if (agent != null) agent.Stop();
        }
    }
}
