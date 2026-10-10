using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.Performance
{
    /// <summary>Per-slot bounded run metrics; presentation consumes typed crowd/music states.</summary>
    [DisallowMultipleComponent]
    public sealed class CombatPerformanceController2D : MonoBehaviour
    {
        [SerializeField] private CombatPerformanceDefinition definition;
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private FanRescueController2D[] rescueSources=new FanRescueController2D[0];
        private readonly HashSet<int> rescued=new HashSet<int>();
        private DamageReceiver2D boundActor;
        private Action<CombatImpactEvent> dealtListener,receivedListener;
        private long generation;
        private Action<FanActor2D,int>[] rescueListeners=new Action<FanActor2D,int>[0];
        private bool subscribed;
        private bool running;
        private bool publishing;
        private bool notificationPending;
        private float points,elapsed,comboRemaining;
        private int combo,maximumCombo;
        private bool noDamage=true;
        public bool IsRunning=>running;
        public event Action<PerformanceSnapshot> SnapshotChanged;
        public PerformanceSnapshot Snapshot
        {
            get
            {
                float normalized=definition==null||definition.MaximumPoints<=0f?0f:points/definition.MaximumPoints;
                PerformanceBand band=definition==null?PerformanceBand.Calm:
                    normalized>=definition.SpectacularThreshold?PerformanceBand.Spectacular:
                    normalized>=definition.EnergizedThreshold?PerformanceBand.Energized:PerformanceBand.Calm;
                return new PerformanceSnapshot(points,normalized,combo,maximumCombo,rescued.Count,elapsed,noDamage,band);
            }
        }
        public bool Configure(CombatPerformanceDefinition profile,CharacterManager2D owner,FanRescueController2D[] sources)
        {
            if(publishing||running||profile==null||!profile.IsValid(out _)||owner==null||sources==null||sources.Length>1024) return false;
            for(int i=0;i<sources.Length;i++)
            { if(sources[i]==null)return false;for(int j=0;j<i;j++)if(sources[j]==sources[i])return false; }
            InvalidateGeneration();Unsubscribe();definition=profile;manager=owner;rescueSources=(FanRescueController2D[])sources.Clone();
            points=elapsed=comboRemaining=0f;combo=maximumCombo=0;noDamage=true;rescued.Clear();notificationPending=false;
            if(isActiveAndEnabled)Subscribe();return true;
        }
        public bool TryBeginRun()
        {
            if(!isActiveAndEnabled||publishing||running||generation==long.MaxValue||!ValidConfiguration()||!manager.isActiveAndEnabled||
                manager.State!=CharacterManagerState2D.Alive||manager.ActiveCharacter==null||
                !manager.ActiveCharacter.isActiveAndEnabled||!manager.ActiveCharacter.IsAlive) return false;
            generation++;
            Unsubscribe();Subscribe();Bind(manager.ActiveCharacter);
            points=elapsed=comboRemaining=0f;combo=maximumCombo=0;noDamage=true;rescued.Clear();notificationPending=false;running=true;
            Publish();return running&&isActiveAndEnabled;
        }
        public bool TryFinishRun(out PerformanceSnapshot result)
        {
            result=Snapshot;if(publishing||!running)return false;running=false;return true;
        }
        public bool ResetRun()
        {
            if(publishing)return false;
            InvalidateGeneration();running=false;points=elapsed=comboRemaining=0f;combo=maximumCombo=0;noDamage=true;rescued.Clear();Publish();return true;
        }
        public void CancelRun(){InvalidateGeneration();running=false;}
        private void InvalidateGeneration(){if(generation<long.MaxValue)generation++;}
        private void LateUpdate()
        {
            if(notificationPending&&!publishing){notificationPending=false;Publish();}
        }
        private void Update()
        {
            if(!running)return;
            elapsed=Mathf.Min(86400f,elapsed+Time.deltaTime);
            if(comboRemaining<=0f)return;
            comboRemaining=Mathf.Max(0f,comboRemaining-Time.deltaTime);
            if(comboRemaining<=0f&&combo!=0){combo=0;Publish();}
        }
        private void OnEnable()=>Subscribe();
        private void OnDisable(){InvalidateGeneration();running=false;notificationPending=false;Unsubscribe();}
        private void Subscribe()
        {
            if(subscribed||!ValidConfiguration())return;
            manager.ActiveCharacterChanged+=HandleActorChanged;
            rescueListeners=new Action<FanActor2D,int>[rescueSources.Length];
            for(int i=0;i<rescueSources.Length;i++)
            {
                long acceptedGeneration=generation;
                rescueListeners[i]=(fan,reward)=>{if(generation==acceptedGeneration)HandleRescue(fan,reward);};
                rescueSources[i].RescueCompleted+=rescueListeners[i];
            }
            subscribed=true;Bind(manager.ActiveCharacter);
        }
        private bool ValidConfiguration()
        {
            if(definition==null||!definition.IsValid(out _)||manager==null||rescueSources==null||rescueSources.Length>1024)return false;
            for(int i=0;i<rescueSources.Length;i++)
            {if(rescueSources[i]==null)return false;for(int j=0;j<i;j++)if(rescueSources[i]==rescueSources[j])return false;}
            return true;
        }
        private void Unsubscribe()
        {
            if(subscribed)
            {
                if(manager!=null)manager.ActiveCharacterChanged-=HandleActorChanged;
                if(rescueSources!=null)
                    for(int i=0;i<rescueSources.Length&&i<rescueListeners.Length;i++)
                        if(rescueSources[i]!=null)rescueSources[i].RescueCompleted-=rescueListeners[i];
            }
            subscribed=false;rescueListeners=new Action<FanActor2D,int>[0];Bind(null);
        }
        private void HandleActorChanged(DamageReceiver2D previous,DamageReceiver2D current)=>Bind(current);
        private void Bind(DamageReceiver2D actor)
        {
            if(boundActor!=null){boundActor.DamageDealt-=dealtListener;boundActor.DamageReceived-=receivedListener;}
            boundActor=actor;dealtListener=receivedListener=null;
            if(boundActor!=null)
            {
                long acceptedGeneration=generation;
                dealtListener=impact=>{if(generation==acceptedGeneration)HandleDealt(impact);};
                receivedListener=impact=>{if(generation==acceptedGeneration)HandleReceived(impact);};
                boundActor.DamageDealt+=dealtListener;boundActor.DamageReceived+=receivedListener;
            }
        }
        private void HandleDealt(CombatImpactEvent impact)
        {
            // Subscription snapshots identify the controlled actor at hit acceptance, even after a callback switches it.
            if(!running)return;
            combo=Math.Min(10000,combo+1);maximumCombo=Math.Max(maximumCombo,combo);comboRemaining=definition.ComboWindow;
            points=Mathf.Min(definition.MaximumPoints,points+definition.HitPoints+definition.ComboBonus*(combo-1));Publish();
        }
        private void HandleReceived(CombatImpactEvent impact)
        {
            if(!running)return;
            noDamage=false;combo=0;comboRemaining=0f;points=Mathf.Max(0f,points-definition.DamagePenalty);Publish();
        }
        private void HandleRescue(FanActor2D fan,int reward)
        {
            if(!running||ReferenceEquals(fan,null)||rescued.Count>=1024||!rescued.Add(fan.GetInstanceID()))return;
            points=Mathf.Min(definition.MaximumPoints,points+definition.RescuePoints);Publish();
        }
        private void Publish()
        {
            if(publishing){notificationPending=true;return;}
            Action<PerformanceSnapshot> listeners=SnapshotChanged;if(listeners==null)return;
            PerformanceSnapshot snapshot=Snapshot;publishing=true;
            try
            {
                foreach(Action<PerformanceSnapshot> listener in listeners.GetInvocationList())
                    try{listener(snapshot);}catch(Exception error){Debug.LogException(error,this);}
            }
            finally{publishing=false;}
        }
    }
}
