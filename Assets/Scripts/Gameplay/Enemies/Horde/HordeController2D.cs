using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
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
        public bool IsRunning { get; private set; }
        public int SpawnedCount { get; private set; }
        public int AliveCount => owned.Count;
        public bool IsComplete => completed;
        private DamageReceiver2D Target => targetManager != null ? targetManager.ActiveCharacter : explicitTarget;
        public bool TryConfigure(HordeDefinition value, Transform[] points, CharacterManager2D manager, DamageReceiver2D target)
        {
            if (IsRunning || spawning || stopping || owned.Count > 0 || value == null || !value.IsValid(out _) ||
                points == null || points.Length == 0 || points.Length > 64 || (manager == null && target == null)) return false;
            foreach (Transform point in points) if (!ValidPoint(point)) return false;
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
            if (remaining > 0f || owned.Count >= definition.MaximumConcurrent) return;
            remaining = definition.SpawnInterval;
            SpawnOne(); // no catch-up burst: at most one allocation/spawn per rendered frame
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
                instance = Instantiate(definition.EnemyPrefab, point.position, point.rotation); // independent root
                instance.SetActive(true);
                EnemyAgent2D agent = instance.GetComponent<EnemyAgent2D>();
                if (agent == null || agent.Self == null || !agent.Self.IsAlive ||
                    agent.Self.Faction != CombatFaction2D.Demon || !agent.TrySetTarget(Target) || !isActiveAndEnabled || !IsRunning)
                { instance.SetActive(false); Destroy(instance); IsRunning = false; return; }
                owned.Add(agent); SpawnedCount++;
            }
            catch (System.Exception exception)
            {
                if (instance != null) { instance.SetActive(false); Destroy(instance); }
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
        private static void Dispose(EnemyAgent2D agent) { agent.Stop(); agent.gameObject.SetActive(false); Destroy(agent.gameObject); }
        private static bool ValidPoint(Transform point) => point != null && point.gameObject.activeInHierarchy &&
            Finite(point.position.x) && Finite(point.position.y) && Finite(point.position.z) &&
            Finite(point.rotation.x) && Finite(point.rotation.y) && Finite(point.rotation.z) && Finite(point.rotation.w) &&
            (point.rotation.x != 0f || point.rotation.y != 0f || point.rotation.z != 0f || point.rotation.w != 0f);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable() => Stop();
        private void OnDestroy() => Stop();
    }
}
