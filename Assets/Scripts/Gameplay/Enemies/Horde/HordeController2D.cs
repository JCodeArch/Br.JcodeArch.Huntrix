using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Pooling;
using UnityEngine;
namespace HuntrX.Gameplay.Enemies
{
    [DisallowMultipleComponent]
    public sealed class HordeController2D : MonoBehaviour
    {
        [SerializeField] private HordeDefinition definition;
        [SerializeField] private Transform[] spawnPoints = new Transform[0];
        [SerializeField] private CharacterManager2D targetManager;
        [SerializeField] private DamageReceiver2D explicitTarget;
        private readonly List<EnemyAgent2D> owned = new List<EnemyAgent2D>(64);
        private float remaining;
        private bool spawning;
        private bool stopping;
        private bool completed;
        private MonoBehaviour intensityOwner;
        private float intervalMultiplier = 1f;
        private int extraConcurrent;
        private BoundedPrefabPool enemyPool;
        private GameObject pooledPrefab;
        private bool usePooling;
        public bool IsRunning { get; private set; }
        public int SpawnedCount { get; private set; }
        public int AliveCount => owned.Count;
        public bool IsComplete => completed;
        public EnemyAgent2D GetOwnedAt(int index)
        {
            if (index < 0 || index >= owned.Count) return null;
            EnemyAgent2D actor = owned[index];
            return actor != null && actor.isActiveAndEnabled && actor.Self != null && actor.Self.IsAlive ? actor : null;
        }
        public int EffectiveMaximumConcurrent => definition == null ? 0 : Mathf.Min(64, definition.MaximumConcurrent + ActiveExtraConcurrent);
        public float EffectiveSpawnInterval => definition == null ? 0f : definition.SpawnInterval * ActiveIntervalMultiplier;
        private bool HasIntensity => intensityOwner != null && intensityOwner.isActiveAndEnabled;
        private int ActiveExtraConcurrent => HasIntensity ? extraConcurrent : 0;
        private float ActiveIntervalMultiplier => HasIntensity ? intervalMultiplier : 1f;
        public bool TrySetIntensity(MonoBehaviour source, float multiplier, int additionalConcurrent)
        {
            if (!isActiveAndEnabled || source == null || !source.isActiveAndEnabled ||
                (HasIntensity && intensityOwner != source) || !Finite(multiplier) || multiplier <= 0f ||
                multiplier > 1f || additionalConcurrent < 0 || additionalConcurrent > 64) return false;
            intensityOwner = source; intervalMultiplier = multiplier; extraConcurrent = additionalConcurrent;
            remaining = Mathf.Min(remaining, EffectiveSpawnInterval);
            return true;
        }
        public void ClearIntensity(MonoBehaviour source)
        {
            if (intensityOwner != source) return;
            intensityOwner = null; intervalMultiplier = 1f; extraConcurrent = 0;
        }
        private DamageReceiver2D Target => targetManager != null ? targetManager.ActiveCharacter : explicitTarget;
        public bool TryConfigure(HordeDefinition value, Transform[] points, CharacterManager2D manager, DamageReceiver2D target)
        {
            if (IsRunning || spawning || stopping || owned.Count > 0 || value == null || !value.IsValid(out _) ||
                points == null || points.Length == 0 || points.Length > 64 || (manager == null && target == null)) return false;
            foreach (Transform point in points) if (!ValidPoint(point)) return false;
            if(pooledPrefab!=value.EnemyPrefab)DisposePool();
            definition = value; spawnPoints = (Transform[])points.Clone(); targetManager = manager; explicitTarget = target;
            SpawnedCount = 0; completed = false;
            return true;
        }
        public bool TryStart()
        {
            if (!isActiveAndEnabled || IsRunning || spawning || stopping || owned.Count > 0 || definition == null ||
                !definition.IsValid(out _) || Target == null || !Target.isActiveAndEnabled || !Target.IsAlive ||
                spawnPoints == null || spawnPoints.Length == 0 || spawnPoints.Length > 64) return false;
            foreach (Transform point in spawnPoints) if (!ValidPoint(point)) return false;
            EnemyAgent2D template = definition.EnemyPrefab.GetComponent<EnemyAgent2D>();
            if (template == null || template.Definition == null || !template.Definition.IsValid(out _) ||
                definition.EnemyPrefab.GetComponent<DamageReceiver2D>() == null) return false;
            usePooling=definition.EnemyPrefab.GetComponent<GroundEnemyPoolLease>()!=null&&GroundEnemyPoolLease.Supports(definition.EnemyPrefab);
            if(!usePooling)DisposePool();
            if(usePooling&&enemyPool==null)
            {
                try{enemyPool=new BoundedPrefabPool(definition.EnemyPrefab,64,definition.MaximumConcurrent);pooledPrefab=definition.EnemyPrefab;}
                catch(System.Exception exception){Debug.LogException(exception,this);DisposePool();return false;}
            }
            SpawnedCount = 0; remaining = 0f; completed = false; IsRunning = true;
            return true;
        }
        private void Update()
        {
            DamageReceiver2D target = Target;
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                if (i >= owned.Count) continue; // callbacks may stop and clear the remaining owned actors
                EnemyAgent2D agent = owned[i];
                if (agent == null || agent.Self == null || !agent.Self.IsAlive || !agent.isActiveAndEnabled || !agent.gameObject.activeInHierarchy)
                { owned.RemoveAt(i); if (agent != null) Dispose(agent); }
                else if (target != null && target.isActiveAndEnabled && target.IsAlive) agent.TrySetTarget(target);
                else { agent.TrySetTarget(null); agent.Stop(); }
            }
            if (!IsRunning) return;
            if (definition == null || !definition.IsValid(out _)) { Stop(); return; }
            if (SpawnedCount >= definition.TotalSpawns)
            { if (owned.Count == 0) { IsRunning = false; completed = true; } return; }
            if (target == null || !target.isActiveAndEnabled || !target.IsAlive || Time.deltaTime <= 0f) return;
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            if (remaining > 0f || owned.Count >= EffectiveMaximumConcurrent) return;
            remaining = EffectiveSpawnInterval;
            SpawnOne(); // no catch-up burst: at most one bounded pool rent per rendered frame
        }
        private void SpawnOne()
        {
            if (spawning) return;
            spawning = true;
            GameObject instance = null;
            try
            {
                Transform point = spawnPoints[SpawnedCount % spawnPoints.Length];
                if (!ValidPoint(point)) { IsRunning = false; return; }
                bool capturedPooling=usePooling;BoundedPrefabPool capturedPool=enemyPool;
                if(capturedPooling)
                {if(capturedPool==null||!capturedPool.TryRent(point.position,point.rotation,out instance))return;}
                else{instance=Instantiate(definition.EnemyPrefab,point.position,point.rotation);instance.SetActive(true);}
                if(usePooling!=capturedPooling||(capturedPooling&&enemyPool!=capturedPool)||!isActiveAndEnabled||!IsRunning){ReturnCandidate(instance);return;}
                EnemyAgent2D agent = instance.GetComponent<EnemyAgent2D>();
                if (agent == null || agent.Self == null || !agent.Self.IsAlive ||
                    agent.Self.Faction != CombatFaction2D.Demon || !agent.TrySetTarget(Target) || !isActiveAndEnabled || !IsRunning||usePooling!=capturedPooling||(capturedPooling&&enemyPool!=capturedPool))
                { ReturnCandidate(instance); IsRunning = false; return; }
                owned.Add(agent); SpawnedCount++;
            }
            catch (System.Exception exception)
            {
                if (instance != null) ReturnCandidate(instance);
                IsRunning = false; Debug.LogException(exception, this);
            }
            finally { spawning = false; }
        }
        public void Stop()
        {
            if (stopping) return;
            stopping = true;
            IsRunning = false; completed = false;
            try
            {
                while (owned.Count > 0)
                {
                    int index = owned.Count - 1;
                    EnemyAgent2D agent = owned[index];
                    owned.RemoveAt(index); // relinquish ownership before publishing enemy state callbacks
                    if (agent != null) Dispose(agent);
                }
                remaining = 0f;
            }
            finally { stopping = false; }
        }
        private static void Dispose(EnemyAgent2D agent)
        {
            GameObject instance=agent.gameObject;agent.Stop();if(instance!=null)ReturnCandidate(instance);
        }
        private static void ReturnCandidate(GameObject instance)
        {
            if(instance==null)return;PooledInstance lease=instance.GetComponent<PooledInstance>();
            if(lease!=null&&lease.IsPooled){lease.TryReturn();return;}
            instance.SetActive(false);Destroy(instance);
        }
        private void DisposePool()
        {
            BoundedPrefabPool ownedPool=enemyPool;enemyPool=null;pooledPrefab=null;usePooling=false;ownedPool?.Dispose();
        }
        private static bool ValidPoint(Transform point) => point != null && point.gameObject.activeInHierarchy &&
            Finite(point.position.x) && Finite(point.position.y) && Finite(point.position.z) &&
            Finite(point.rotation.x) && Finite(point.rotation.y) && Finite(point.rotation.z) && Finite(point.rotation.w) &&
            (point.rotation.x != 0f || point.rotation.y != 0f || point.rotation.z != 0f || point.rotation.w != 0f);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable(){Stop();DisposePool();}
        private void OnDestroy(){Stop();DisposePool();}
    }
}
