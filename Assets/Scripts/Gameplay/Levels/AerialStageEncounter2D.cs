using System.Collections.Generic;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    /// <summary>Owns independent-root flyers at the authored stage's varied aerial spawn points.</summary>
    [DisallowMultipleComponent]
    public sealed class AerialStageEncounter2D : MonoBehaviour
    {
        private StageBlockoutBuilder2D stage;
        private CharacterManager2D manager;
        private GameObject prefab;
        private readonly List<EnemyAgent2D> owned=new List<EnemyAgent2D>(16);
        private bool spawning,clearing;
        public int Count=>owned.Count;
        public EnemyAgent2D GetAt(int index)=>index>=0&&index<owned.Count?owned[index]:null;
        public bool Configure(StageBlockoutBuilder2D builder,CharacterManager2D owner,GameObject flyingPrefab)
        {
            if(spawning||clearing||owned.Count>0||builder==null||owner==null||flyingPrefab==null||flyingPrefab.scene.IsValid()||
                flyingPrefab.GetComponent<FlyingEnemyController2D>()==null||flyingPrefab.GetComponent<EnemyAgent2D>()==null)return false;
            if(manager!=null)manager.ActiveCharacterChanged-=HandleTarget;
            stage=builder;manager=owner;prefab=flyingPrefab;
            if(isActiveAndEnabled)manager.ActiveCharacterChanged+=HandleTarget;return true;
        }
        public bool TrySpawn()
        {
            DamageReceiver2D target=manager==null?null:manager.ActiveCharacter;
            if(!isActiveAndEnabled||spawning||clearing||owned.Count>0||stage==null||stage.RuntimeRoot==null||stage.Definition==null||
                !stage.Definition.IsValid(out _)||target==null||!target.isActiveAndEnabled||!target.IsAlive||prefab==null)return false;
            spawning=true;
            try
            {
                for(int i=0;i<stage.Definition.AerialSpawnCount;i++)
                {
                    GameObject instance=Instantiate(prefab,stage.Definition.GetAerialSpawn(i),Quaternion.identity);
                    instance.SetActive(true);EnemyAgent2D agent=instance.GetComponent<EnemyAgent2D>();
                    if(!isActiveAndEnabled||manager==null||manager.ActiveCharacter!=target||agent==null||!agent.IsConfigurationValid||
                        !agent.TrySetTarget(target)||!isActiveAndEnabled||manager.ActiveCharacter!=target||
                        !target.isActiveAndEnabled||!target.IsAlive||!agent.isActiveAndEnabled||agent.Self==null||!agent.Self.IsAlive)
                    {instance.SetActive(false);Destroy(instance);Clear();return false;}
                    owned.Add(agent);
                }
                return true;
            }
            finally{spawning=false;}
        }
        private void HandleTarget(DamageReceiver2D previous,DamageReceiver2D current)
        {
            EnemyAgent2D[] snapshot=owned.ToArray();
            foreach(EnemyAgent2D actor in snapshot)if(actor!=null&&owned.Contains(actor))
            {if(!actor.TrySetTarget(current)&&actor!=null)actor.Stop();}
        }
        public void Clear()
        {
            if(clearing)return;clearing=true;
            try
            {
                while(owned.Count>0)
                {
                    int index=owned.Count-1;EnemyAgent2D actor=owned[index];owned.RemoveAt(index);
                    if(actor==null)continue;
                    actor.Stop();if(actor!=null){actor.gameObject.SetActive(false);Destroy(actor.gameObject);}
                }
            }
            finally{clearing=false;}
        }
        private void OnEnable(){if(manager!=null)manager.ActiveCharacterChanged+=HandleTarget;}
        private void OnDisable(){if(manager!=null)manager.ActiveCharacterChanged-=HandleTarget;Clear();}
    }
}
