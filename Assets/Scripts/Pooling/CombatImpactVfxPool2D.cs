using System;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Pooling
{
    /// <summary>Explicit actor-bound accepted-hit visuals; capacity and expiry bookkeeping are bounded.</summary>
    [DisallowMultipleComponent]
    public sealed class CombatImpactVfxPool2D : MonoBehaviour
    {
        [SerializeField] private DamageReceiver2D actor;
        [SerializeField] private GameObject effectPrefab;
        [SerializeField,Range(1,32)] private int capacity=8;
        [SerializeField,Range(.05f,2f)] private float lifetime=.2f;
        private BoundedPrefabPool pool;
        private GameObject configuredPrefab;
        private GameObject[] active;
        private float[] remaining;
        private CombatImpactEvent[] impacts;
        private int[] budgets;
        private float[] intensities;
        private DamageReceiver2D boundActor;
        private Action<CombatImpactEvent> listener;
        private long generation;
        private bool clearing,destroying,enablePending;
        private void OnEnable(){if(clearing){enablePending=true;return;}Bind();}
        private void Bind()
        {
            if(destroying||!isActiveAndEnabled||actor==null||effectPrefab==null||capacity<1||capacity>32||float.IsNaN(lifetime)||float.IsInfinity(lifetime)||lifetime<=0f)return;
            generation++;
            if(pool!=null&&(configuredPrefab!=effectPrefab||active.Length!=capacity))DisposePool();
            if(pool==null)
            {
                pool=new BoundedPrefabPool(effectPrefab,capacity,capacity);configuredPrefab=effectPrefab;
                active=new GameObject[capacity];remaining=new float[capacity];
                impacts=new CombatImpactEvent[capacity];budgets=new int[capacity];intensities=new float[capacity];
            }
            boundActor=actor;long epoch=generation;
            listener=impact=>{if(generation==epoch)HandleImpact(impact);};boundActor.DamageReceived+=listener;
        }
        private void Unbind(){if(boundActor!=null&&listener!=null)boundActor.DamageReceived-=listener;boundActor=null;listener=null;}
        private void HandleImpact(CombatImpactEvent impact)
        {
            if(!isActiveAndEnabled||clearing||pool==null||impact.Receiver==null)return;
            if(!TryPresentation(impact,out int particleBudget,out float intensity))return;
            GameObject[] captured=active;BoundedPrefabPool owned=pool;long epoch=generation;
            for(int i=0;i<captured.Length;i++)if(captured[i]==null)
            {
                if(owned.TryRent(impact.Receiver.transform.position,Quaternion.identity,out GameObject effect))
                {
                    if(isActiveAndEnabled&&pool==owned&&active==captured&&generation==epoch&&TryPresentation(impact,out particleBudget,out intensity)&&
                        effect.GetComponent<PooledImpactSprite2D>() is PooledImpactSprite2D visual&&visual.TryPresent(particleBudget,intensity))
                    {captured[i]=effect;remaining[i]=lifetime;impacts[i]=impact;budgets[i]=particleBudget;intensities[i]=intensity;}
                    else effect.GetComponent<PooledInstance>()?.TryReturn();
                }
                return;
            }
        }
        private static bool TryPresentation(CombatImpactEvent impact,out int particleBudget,out float intensity)
        {
            particleBudget=0;intensity=0f;
            DamageReceiver2D context=impact.Attacker!=null&&impact.Attacker.Faction==CombatFaction2D.HuntrX?impact.Attacker:
                impact.Receiver!=null&&impact.Receiver.Faction==CombatFaction2D.HuntrX?impact.Receiver:null;
            if(context==null||!context.isActiveAndEnabled||!context.IsAlive)return false;
            AgeProfileSelection2D profiles=context.GetComponent<AgeProfileSelection2D>();
            AgeVisualFeedback2D visual=context.GetComponent<AgeVisualFeedback2D>();
            if(profiles==null||!profiles.isActiveAndEnabled||profiles.Current==null||!profiles.Current.IsValid(out _)||
                visual==null||!visual.isActiveAndEnabled||visual.IsReducedFlashes)return false;
            particleBudget=Mathf.Min(8,profiles.Current.ParticleBudget);intensity=profiles.Current.VisualIntensity;
            return intensity>0f;
        }
        private void Update()
        {
            GameObject[] captured=active;float[] timers=remaining;long epoch=generation;if(captured==null)return;
            for(int i=0;i<captured.Length;i++)
            {
                if(active!=captured||remaining!=timers||!isActiveAndEnabled||generation!=epoch)return;
                if(captured[i]==null)continue;
                if(!TryPresentation(impacts[i],out int budget,out float intensity)||budget!=budgets[i]||intensity!=intensities[i])timers[i]=0f;
                else timers[i]-=Time.deltaTime;
                if(timers[i]>0f)continue;
                GameObject effect=captured[i];captured[i]=null;impacts[i]=default;effect.GetComponent<PooledInstance>()?.TryReturn();
            }
        }
        private void OnDisable()
        {
            generation++;Unbind();
            if(clearing)return;clearing=true;
            try
            {
                GameObject[] captured=active;BoundedPrefabPool owned=pool;
                if(captured!=null)for(int i=0;i<captured.Length;i++)
                {
                    if(pool!=owned||active!=captured||destroying)break;
                    GameObject effect=captured[i];captured[i]=null;remaining[i]=0f;impacts[i]=default;
                    if(effect!=null)effect.GetComponent<PooledInstance>()?.TryReturn();
                }
            }
            finally{clearing=false;if(enablePending){enablePending=false;if(isActiveAndEnabled&&!destroying)Bind();}}
        }
        private void DisposePool()
        {
            BoundedPrefabPool owned=pool;pool=null;configuredPrefab=null;active=null;remaining=null;impacts=null;budgets=null;intensities=null;owned?.Dispose();
        }
        private void OnDestroy(){destroying=true;generation++;Unbind();DisposePool();}
    }
}
