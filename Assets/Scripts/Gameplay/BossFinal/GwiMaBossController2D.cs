using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.BossFinal
{
    public enum GwiMaPhase2D { Uninitialized, Opening, Pressure, Finale, Defeated }
    public readonly struct FinalBossOutcome2D
    {
        public FinalBossOutcome2D(string bossId,string endingCue,Vector3 position)
        { BossId=bossId;EndingCue=endingCue;Position=position; }
        public string BossId { get; }
        public string EndingCue { get; }
        public Vector3 Position { get; }
    }
    /// <summary>Monotonic final encounter phases and a once-only ending integration boundary.</summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent,RequireComponent(typeof(EnemyAgent2D),typeof(EnemyPatternController2D))]
    public sealed class GwiMaBossController2D : MonoBehaviour
    {
        [SerializeField] private GwiMaBossDefinition definition;
        private EnemyAgent2D agent;
        private EnemyPatternController2D pattern;
        private DamageReceiver2D self;
        private bool subscribed,endingPublished,suspendedPattern;
        public GwiMaPhase2D Phase { get; private set; }
        public GwiMaBossDefinition Definition=>definition;
        public event Action<GwiMaPhase2D> PhaseChanged;
        public event Action<FinalBossOutcome2D> EndingRequested;
        private void Awake()
        { agent=GetComponent<EnemyAgent2D>();pattern=GetComponent<EnemyPatternController2D>();self=GetComponent<DamageReceiver2D>(); }
        private void OnEnable()
        {
            if(self!=null&&!subscribed){self.Died+=HandleDeath;subscribed=true;}
            if(suspendedPattern&&self!=null&&self.IsAlive){pattern.enabled=true;suspendedPattern=false;}
            if(self!=null&&self.IsConfigurationValid&&!self.IsAlive)HandleDeath(self);
        }
        private void OnDisable()
        {
            if(subscribed&&self!=null)self.Died-=HandleDeath;
            subscribed=false;
            suspendedPattern=pattern!=null&&pattern.enabled;
            if(pattern!=null)pattern.enabled=false;
            agent?.Stop();
        }
        private void Update()
        {
            if(definition==null||!definition.IsValid(out _)||agent==null||!agent.isActiveAndEnabled||
                pattern==null||!pattern.isActiveAndEnabled||self==null||!self.isActiveAndEnabled||!self.IsAlive||endingPublished)return;
            if(agent.State!=EnemyState2D.Idle&&agent.State!=EnemyState2D.Chasing)return;
            float ratio=self.CurrentHealth/self.MaximumHealth;
            GwiMaPhase2D next=ratio<=definition.FinaleHealthFraction?GwiMaPhase2D.Finale:
                ratio<=definition.PressureHealthFraction?GwiMaPhase2D.Pressure:GwiMaPhase2D.Opening;
            if(next<=Phase)return;
            EnemyPatternDefinition nextPattern=next==GwiMaPhase2D.Opening?definition.Opening:
                next==GwiMaPhase2D.Pressure?definition.Pressure:definition.Finale;
            if(!pattern.TrySetDefinition(nextPattern,out _)||!isActiveAndEnabled||!self.IsAlive)return;
            Phase=next;PublishPhase(next);
        }
        private void HandleDeath(DamageReceiver2D actor)
        {
            if(actor!=self||endingPublished||definition==null||!definition.IsValid(out _))return;
            endingPublished=true;Phase=GwiMaPhase2D.Defeated;
            if(pattern!=null)pattern.enabled=false;
            agent?.Stop();
            FinalBossOutcome2D outcome=new FinalBossOutcome2D(definition.Id,definition.EndingCue,transform.position);
            PublishPhase(Phase);
            var handlers=EndingRequested;
            if(handlers!=null)foreach(Action<FinalBossOutcome2D> handler in handlers.GetInvocationList())
                try{handler(outcome);}catch(Exception error){Debug.LogException(error,this);}
        }
        private void PublishPhase(GwiMaPhase2D phase)
        {
            var handlers=PhaseChanged;
            if(handlers!=null)foreach(Action<GwiMaPhase2D> handler in handlers.GetInvocationList())
                try{handler(phase);}catch(Exception error){Debug.LogException(error,this);}
        }
    }
}
