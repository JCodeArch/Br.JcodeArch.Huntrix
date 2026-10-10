using System;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Enemies
{
    [DisallowMultipleComponent, RequireComponent(typeof(EnemyAgent2D))]
    public sealed class EnemyPatternController2D : MonoBehaviour
    {
        [SerializeField] private EnemyPatternDefinition definition;
        private EnemyAgent2D agent;
        private int attacksRemaining;
        private bool alternate;
        private float rest;
        private bool cycleStarted;
        private int revision;
        public EnemyPatternDefinition Definition => definition;
        public event Action<int> PatternStarted;
        private void Awake() => agent=GetComponent<EnemyAgent2D>();
        private void OnDisable() { ResetPattern(); agent?.Stop(); }
        public bool TrySetDefinition(EnemyPatternDefinition next, out string error)
        {
            if(next==null) { error="Pattern is required."; return false; }
            if(!next.IsValid(out error)) return false;
            definition=next; revision++; ResetPattern(); agent?.Stop(); return true;
        }
        private void ResetPattern() { attacksRemaining=0; alternate=false; rest=0f; cycleStarted=false; }
        private void Update()
        {
            if(agent==null || !agent.isActiveAndEnabled || definition==null || !definition.IsValid(out _) || agent.Definition==null || !agent.Definition.IsValid(out _)) return;
            agent.Tick(Time.deltaTime);
            if(!isActiveAndEnabled || !agent.isActiveAndEnabled || agent.Self==null || !agent.Self.IsAlive || !agent.HasValidTarget) { ResetPattern(); return; }
            if(rest>0f) { rest=Mathf.Max(0f,rest-Time.deltaTime); return; }
            if(agent.State==EnemyState2D.Attacking) { agent.TryExecuteMelee(); return; }
            if(agent.State==EnemyState2D.Telegraph || agent.State==EnemyState2D.Recovering || agent.State==EnemyState2D.Dead) return;
            float distance=Vector2.Distance(agent.Self.transform.position,agent.Target.transform.position);
            if(distance>agent.Definition.AttackRange) { agent.MoveTowardsTarget(); return; }
            if(!cycleStarted)
            {
                if(!agent.TryConfigure(alternate?definition.AlternateBehavior:definition.PrimaryBehavior,out _)) return;
                attacksRemaining=alternate?definition.AlternateBurstCount:definition.BurstCount;
                cycleStarted=true; int startedRevision=revision; int startedCount=attacksRemaining; var handlers=PatternStarted;
                if(handlers!=null) foreach(Action<int> handler in handlers.GetInvocationList())
                    try { handler(startedCount); } catch(Exception exception) { Debug.LogException(exception,this); }
                if(startedRevision!=revision || !isActiveAndEnabled || agent.Self==null || !agent.Self.IsAlive) return;
            }
            if(attacksRemaining>0)
            {
                if(agent.TryBeginTelegraph()) attacksRemaining--;
            }
            else
            {
                rest=alternate?definition.AlternateRecoveryDuration:definition.RecoveryDuration;
                alternate=!alternate; cycleStarted=false;
            }
        }
    }
}
