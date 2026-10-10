using UnityEngine;
namespace HuntrX.Data
{
    public enum SupportEnemyRole { Healer, Protector, SoulDrainer }
    [CreateAssetMenu(fileName = "SupportEnemyDefinition", menuName = "HUNTR/X/Data/Support Enemy")]
    public sealed class SupportEnemyDefinition : GameDataDefinition
    {
        [SerializeField] private SupportEnemyRole role;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float interval = 1f;
        [SerializeField] private float healingAmount = 5f;
        [SerializeField] private float protectionDuration = 1.2f;
        [SerializeField] private AttackDefinition drainAttack;
        public SupportEnemyRole Role => role;
        public float Radius => radius;
        public float Interval => interval;
        public float HealingAmount => healingAmount;
        public float ProtectionDuration => protectionDuration;
        public AttackDefinition DrainAttack => drainAttack;
        public bool IsValid(out string error)
        {
            bool valid = System.Enum.IsDefined(typeof(SupportEnemyRole), role) && Positive(radius) &&
                Positive(interval) && Positive(healingAmount) && Positive(protectionDuration) &&
                (role != SupportEnemyRole.SoulDrainer || (drainAttack != null && drainAttack.IsValid(out _)));
            error = valid ? string.Empty : "Support role, finite positive parameters and drainer attack must be valid.";
            return valid;
        }
        private static bool Positive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
