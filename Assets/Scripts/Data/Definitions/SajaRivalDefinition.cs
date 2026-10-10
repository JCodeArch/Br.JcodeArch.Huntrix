using UnityEngine;

namespace HuntrX.Data
{
    public enum SajaIdentity { Abby, Baby, Mystery, Romance, Jinu }
    public enum SajaTactic { Pressure, Burst, Ambush, KeepDistance }

    [CreateAssetMenu(fileName = "SajaRival", menuName = "HUNTR/X/Data/Saja Rival")]
    public sealed class SajaRivalDefinition : GameDataDefinition
    {
        [SerializeField] private SajaIdentity identity;
        [SerializeField] private SajaTactic tactic;
        [SerializeField] private EnemyDefinition enemy;
        [SerializeField, Min(1)] private int burstCount = 1;
        [SerializeField, Min(0f)] private float pauseBetweenBursts = 1f;
        [SerializeField, Min(0f)] private float preferredDistance = 2f;
        public SajaIdentity Identity => identity;
        public SajaTactic Tactic => tactic;
        public EnemyDefinition Enemy => enemy;
        public int BurstCount => burstCount;
        public float PauseBetweenBursts => pauseBetweenBursts;
        public float PreferredDistance => preferredDistance;
        public bool IsValid(out string error)
        {
            error = "Rival requires a valid identity/tactic, enemy profile, burst count and finite positive distances/times.";
            if ((int)identity < 0 || (int)identity > 4 || (int)tactic < 0 || (int)tactic > 3 ||
                enemy == null || !enemy.IsValid(out _) || burstCount < 1 || burstCount > 8 ||
                !Finite(pauseBetweenBursts) || pauseBetweenBursts < 0f || !Finite(preferredDistance) ||
                preferredDistance <= 0f || preferredDistance > enemy.AttackRange) return false;
            error = string.Empty;
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
