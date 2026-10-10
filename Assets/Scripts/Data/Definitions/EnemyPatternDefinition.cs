using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName="EnemyPattern", menuName="HUNTR/X/Data/Enemy Pattern")]
    public sealed class EnemyPatternDefinition : GameDataDefinition
    {
        [SerializeField] private EnemyDefinition primaryBehavior;
        [SerializeField] private EnemyDefinition alternateBehavior;
        public EnemyDefinition PrimaryBehavior => primaryBehavior;
        public EnemyDefinition AlternateBehavior => alternateBehavior;
        [SerializeField] private int burstCount = 2;
        [SerializeField] private int alternateBurstCount = 1;
        [SerializeField] private float recoveryDuration = 1f;
        [SerializeField] private float alternateRecoveryDuration = 1.5f;
        public int BurstCount => burstCount;
        public int AlternateBurstCount => alternateBurstCount;
        public float RecoveryDuration => recoveryDuration;
        public float AlternateRecoveryDuration => alternateRecoveryDuration;
        public bool IsValid(out string error)
        {
            if (primaryBehavior==null || alternateBehavior==null || !primaryBehavior.IsValid(out _) || !alternateBehavior.IsValid(out _) || burstCount < 1 || burstCount > 8 || alternateBurstCount < 1 || alternateBurstCount > 8 ||
                !Positive(recoveryDuration) || !Positive(alternateRecoveryDuration))
            { error="Pattern requires bursts of 1–8 attacks and finite positive recovery durations."; return false; }
            error=string.Empty; return true;
        }
        private static bool Positive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
