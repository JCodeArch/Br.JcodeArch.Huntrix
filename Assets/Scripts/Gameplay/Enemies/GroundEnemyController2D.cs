using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyAgent2D))]
    public sealed class GroundEnemyController2D : MonoBehaviour
    {
        private EnemyAgent2D agent;
        private void Awake() => agent = GetComponent<EnemyAgent2D>();
        private void FixedUpdate()
        {
            agent.Tick(Time.fixedDeltaTime);
            if (!agent.HasValidTarget) return;
            if (agent.State == EnemyState2D.Attacking) agent.TryExecuteMelee();
            else if (agent.State == EnemyState2D.Idle || agent.State == EnemyState2D.Chasing)
            {
                if (!agent.TryBeginTelegraph()) agent.MoveTowardsTarget();
            }
        }
        private void OnDisable() { if (agent != null) agent.Stop(); }
    }
}
