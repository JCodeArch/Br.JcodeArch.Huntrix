using System;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Enemies
{
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent, RequireComponent(typeof(EnemyPatternController2D),typeof(EnemyAgent2D))]
    public sealed class MinibossPhaseController2D : MonoBehaviour
    {
        [SerializeField] private EnemyPatternDefinition openingPattern;
        [SerializeField] private EnemyPatternDefinition enragedPattern;
        [SerializeField, Range(.01f,.99f)] private float phaseHealthFraction=.5f;
        private EnemyAgent2D agent;
        private EnemyPatternController2D pattern;
        public int Phase { get; private set; }
        public event Action<int> PhaseChanged;
        private void Awake() { agent=GetComponent<EnemyAgent2D>(); pattern=GetComponent<EnemyPatternController2D>(); }
        private void Update()
        {
            if(agent==null || !agent.isActiveAndEnabled || pattern==null || !pattern.isActiveAndEnabled || agent.Self==null || !agent.Self.IsAlive || float.IsNaN(phaseHealthFraction) || float.IsInfinity(phaseHealthFraction) || phaseHealthFraction<=0f || phaseHealthFraction>=1f) return;
            if(Phase==0) Apply(agent.Self.CurrentHealth<=agent.Self.MaximumHealth*phaseHealthFraction?2:1,agent.Self.CurrentHealth<=agent.Self.MaximumHealth*phaseHealthFraction?enragedPattern:openingPattern);
            else if(Phase==1 && agent.Self.CurrentHealth<=agent.Self.MaximumHealth*phaseHealthFraction) Apply(2,enragedPattern);
        }
        private void Apply(int phase,EnemyPatternDefinition next)
        {
            if(pattern==null || !pattern.TrySetDefinition(next,out _)) return;
            Phase=phase;
            var handlers=PhaseChanged;
            if(handlers!=null) foreach(Action<int> handler in handlers.GetInvocationList())
                try { handler(phase); } catch(Exception exception) { Debug.LogException(exception,this); }
        }
    }
}
