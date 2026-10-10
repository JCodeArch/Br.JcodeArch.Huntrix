using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "HonmoonCrisisDefinition", menuName = "HUNTR/X/Data/Honmoon Crisis")]
    public sealed class HonmoonCrisisDefinition : GameDataDefinition
    {
        [SerializeField] private float spawnIntervalMultiplier = 0.5f;
        [SerializeField] private int extraConcurrentEnemies = 4;
        [SerializeField] private float unionRecoveryValue = 40f;
        [SerializeField] private float unionProtectionDuration = 2f;
        public float SpawnIntervalMultiplier => spawnIntervalMultiplier;
        public int ExtraConcurrentEnemies => extraConcurrentEnemies;
        public float UnionRecoveryValue => unionRecoveryValue;
        public float UnionProtectionDuration => unionProtectionDuration;
        public bool IsValid(out string error)
        {
            bool valid = !float.IsNaN(spawnIntervalMultiplier) && !float.IsInfinity(spawnIntervalMultiplier) &&
                spawnIntervalMultiplier > 0f && spawnIntervalMultiplier <= 1f && extraConcurrentEnemies >= 0 &&
                extraConcurrentEnemies <= 64 && !float.IsNaN(unionRecoveryValue) && !float.IsInfinity(unionRecoveryValue) &&
                unionRecoveryValue > 0f && unionRecoveryValue <= 100f && !float.IsNaN(unionProtectionDuration) &&
                !float.IsInfinity(unionProtectionDuration) && unionProtectionDuration > 0f && unionProtectionDuration <= 30f;
            error = valid ? string.Empty : "Crisis intensity must be bounded and union recovery must be in (0,100].";
            return valid;
        }
    }
}
