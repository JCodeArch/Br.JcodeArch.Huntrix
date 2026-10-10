using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "HordeDefinition", menuName = "HUNTR/X/Data/Horde")]
    public sealed class HordeDefinition : GameDataDefinition
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int maximumConcurrent = 8;
        [SerializeField] private int totalSpawns = 20;
        [SerializeField] private float spawnInterval = 0.5f;
        public GameObject EnemyPrefab => enemyPrefab;
        public int MaximumConcurrent => maximumConcurrent;
        public int TotalSpawns => totalSpawns;
        public float SpawnInterval => spawnInterval;
        public bool IsValid(out string error)
        {
            bool valid = enemyPrefab != null && !enemyPrefab.scene.IsValid() && maximumConcurrent > 0 &&
                maximumConcurrent <= 64 && totalSpawns > 0 && totalSpawns <= 256 &&
                !float.IsNaN(spawnInterval) && !float.IsInfinity(spawnInterval) && spawnInterval > 0f;
            error = valid ? string.Empty : "Horde requires prefab asset, concurrency 1..64, total 1..256 and finite positive cadence.";
            return valid;
        }
    }
}
