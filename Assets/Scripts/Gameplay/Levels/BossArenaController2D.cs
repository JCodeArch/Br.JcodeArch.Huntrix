using System;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Performance;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    /// <summary>Boss + authored arena + enrolled fans + Honmoon + run-performance composition.</summary>
    [DisallowMultipleComponent]
    public sealed class BossArenaController2D : MonoBehaviour
    {
        [SerializeField] private float fanDrainInterval=2.5f;
        [SerializeField] private float fanDrainAmount=8f;
        [SerializeField] private float arenaRadius=12f;
        [SerializeField] private float honmoonLossPerSoul=.25f;
        private StageBlockoutBuilder2D stage;
        private CharacterManager2D manager;
        private GameObject prefab;
        private FanRegistry2D fans;
        private HonmoonController2D honmoon;
        private CombatPerformanceController2D performance;
        private float remaining;
        private bool spawning,clearing,completed,ownsRun;
        public EnemyAgent2D Boss {get;private set;}
        public bool IsComplete=>completed;
        public event Action<PerformanceSnapshot> Completed;
        public bool Configure(StageBlockoutBuilder2D builder,CharacterManager2D player,GameObject bossPrefab,
            FanRegistry2D registry,HonmoonController2D resource,CombatPerformanceController2D metrics)
        {
            if(spawning||clearing||Boss!=null||builder==null||player==null||bossPrefab==null||bossPrefab.scene.IsValid()||
                bossPrefab.GetComponent<EnemyAgent2D>()==null||registry==null||resource==null||metrics==null)return false;
            if(manager!=null)manager.ActiveCharacterChanged-=HandleTarget;
            stage=builder;manager=player;prefab=bossPrefab;fans=registry;honmoon=resource;performance=metrics;
            completed=false;if(isActiveAndEnabled)manager.ActiveCharacterChanged+=HandleTarget;return true;
        }
        public bool TrySpawn()
        {
            DamageReceiver2D target=manager==null?null:manager.ActiveCharacter;
            if(!isActiveAndEnabled||spawning||clearing||Boss!=null||completed||stage==null||stage.RuntimeRoot==null||stage.Definition==null||
                !stage.Definition.IsValid(out _)||target==null||!target.isActiveAndEnabled||!target.IsAlive||prefab==null||
                fans==null||!fans.isActiveAndEnabled||fans.Count==0||honmoon==null||!honmoon.isActiveAndEnabled||
                performance==null||!performance.isActiveAndEnabled||!Positive(fanDrainInterval)||!Positive(fanDrainAmount)||
                !Positive(arenaRadius)||!Finite(honmoonLossPerSoul)||honmoonLossPerSoul<0f)return false;
            spawning=true;
            try
            {
                GameObject instance=Instantiate(prefab,stage.Definition.ArenaCenter,Quaternion.identity);instance.SetActive(true);
                EnemyAgent2D agent=instance.GetComponent<EnemyAgent2D>();
                if(!isActiveAndEnabled||manager.ActiveCharacter!=target||agent==null||!agent.IsConfigurationValid||!agent.TrySetTarget(target)||!isActiveAndEnabled||manager.ActiveCharacter!=target||
                    !target.isActiveAndEnabled||!target.IsAlive||!agent.isActiveAndEnabled||agent.Self==null||!agent.Self.IsAlive)
                {instance.SetActive(false);Destroy(instance);return false;}
                bool startRun=!performance.IsRunning;
                if(startRun&&!performance.TryBeginRun()){instance.SetActive(false);Destroy(instance);return false;}
                if(!isActiveAndEnabled||manager==null||manager.ActiveCharacter!=target||!target.isActiveAndEnabled||
                    !target.IsAlive||!agent.isActiveAndEnabled||agent.Self==null||!agent.Self.IsAlive||!performance.isActiveAndEnabled)
                {if(startRun)performance.CancelRun();instance.SetActive(false);Destroy(instance);return false;}
                Boss=agent;ownsRun=startRun;remaining=fanDrainInterval;
                Boss.Self.Died+=HandleDefeated;return true;
            }
            finally{spawning=false;}
        }
        private void Update()
        {
            if(completed||Boss==null||Boss.Self==null||!Boss.Self.IsAlive||!Boss.isActiveAndEnabled||
                fans==null||!fans.isActiveAndEnabled||honmoon==null||!honmoon.isActiveAndEnabled)return;
            remaining=Mathf.Max(0f,remaining-Time.deltaTime);if(remaining>0f)return;remaining=fanDrainInterval;
            FanActor2D nearest=null;float distance=float.PositiveInfinity;
            for(int i=0;i<fans.Count;i++)
            {
                FanActor2D fan=fans.GetAt(i);if(fan==null||!fan.isActiveAndEnabled||fan.Soul<=0f||fan.IsProtected)continue;
                float candidate=Vector2.Distance(fan.transform.position,Boss.transform.position);
                if(candidate<=arenaRadius&&candidate<distance){distance=candidate;nearest=fan;}
            }
            if(nearest==null)return;
            float before=nearest.Soul;
            if(nearest.TryDrain(fanDrainAmount)&&isActiveAndEnabled&&honmoon!=null)
                honmoon.TryChange(-Mathf.Max(0f,before-nearest.Soul)*honmoonLossPerSoul);
        }
        private void HandleDefeated(DamageReceiver2D source)
        {
            if(Boss==null||source!=Boss.Self||completed)return;
            completed=true;remaining=0f;
            source.Died-=HandleDefeated;
            PerformanceSnapshot result=performance==null?default:performance.Snapshot;
            if(ownsRun&&performance!=null)performance.TryFinishRun(out result);
            ownsRun=false;var handlers=Completed;
            if(handlers!=null)foreach(Action<PerformanceSnapshot> handler in handlers.GetInvocationList())
                try{handler(result);}catch(Exception error){Debug.LogException(error,this);}
        }
        private void HandleTarget(DamageReceiver2D previous,DamageReceiver2D current)
        {EnemyAgent2D actor=Boss;if(actor!=null&&!actor.TrySetTarget(current)&&actor!=null)actor.Stop();}
        public void Clear()
        {
            if(clearing)return;clearing=true;
            EnemyAgent2D actor=Boss;Boss=null;
            bool cancelRun=ownsRun;ownsRun=false;remaining=0f;completed=false;
            try
            {
                if(actor!=null)
                {
                    if(actor.Self!=null)actor.Self.Died-=HandleDefeated;
                    actor.Stop();if(actor!=null){actor.gameObject.SetActive(false);Destroy(actor.gameObject);}
                }
                if(cancelRun&&performance!=null)performance.CancelRun();
            }
            finally{clearing=false;}
        }
        private void OnEnable(){if(manager!=null)manager.ActiveCharacterChanged+=HandleTarget;}
        private void OnDisable(){if(manager!=null)manager.ActiveCharacterChanged-=HandleTarget;Clear();}
        private static bool Positive(float x)=>Finite(x)&&x>0f;
        private static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
    }
}
