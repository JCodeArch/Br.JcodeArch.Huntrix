using UnityEngine;

namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "HUNTR/X/Data/Enemy")]
    public sealed class EnemyDefinition : GameDataDefinition
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float telegraphDuration = 0.5f;
        [SerializeField] private float recoveryDuration = 0.7f;
        public AttackDefinition Attack => attack;
        public float MoveSpeed => moveSpeed;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float TelegraphDuration => telegraphDuration;
        public float RecoveryDuration => recoveryDuration;
        public bool IsValid(out string error)
        {
            if (attack == null || !attack.IsValid(out error)) { error = "Enemy requires a valid attack."; return false; }
            if (!Positive(moveSpeed) || !Positive(detectionRange) || !Positive(attackRange) ||
                attackRange > detectionRange || !Positive(telegraphDuration) || !Positive(recoveryDuration))
            { error = "Enemy speeds, ranges and readable telegraph/recovery durations must be finite and positive; attack range cannot exceed detection."; return false; }
            error = string.Empty; return true;
        }
        private static bool Positive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
