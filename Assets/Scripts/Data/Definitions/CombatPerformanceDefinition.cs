using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName="CombatPerformance",menuName="HUNTR/X/Data/Combat Performance")]
    public sealed class CombatPerformanceDefinition : GameDataDefinition
    {
        [SerializeField] private float hitPoints=5f;
        [SerializeField] private float comboBonus=1f;
        [SerializeField] private float rescuePoints=20f;
        [SerializeField] private float damagePenalty=15f;
        [SerializeField] private float comboWindow=2f;
        [SerializeField] private float maximumPoints=100f;
        [SerializeField] private float energizedThreshold=.3f;
        [SerializeField] private float spectacularThreshold=.7f;
        public float HitPoints=>hitPoints;
        public float ComboBonus=>comboBonus;
        public float RescuePoints=>rescuePoints;
        public float DamagePenalty=>damagePenalty;
        public float ComboWindow=>comboWindow;
        public float MaximumPoints=>maximumPoints;
        public float EnergizedThreshold=>energizedThreshold;
        public float SpectacularThreshold=>spectacularThreshold;
        public bool IsValid(out string error)
        {
            error="Performance requires finite nonnegative rewards/penalty, positive window/cap and ordered thresholds in (0,1).";
            if(!Nonnegative(hitPoints)||!Nonnegative(comboBonus)||!Nonnegative(rescuePoints)||!Nonnegative(damagePenalty)||
                !Finite(comboWindow)||comboWindow<=0f||!Finite(maximumPoints)||maximumPoints<=0f||maximumPoints>1000000f||
                !Finite(energizedThreshold)||!Finite(spectacularThreshold)||energizedThreshold<=0f||
                spectacularThreshold<=energizedThreshold||spectacularThreshold>=1f) return false;
            error=string.Empty;return true;
        }
        private static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        private static bool Nonnegative(float v)=>Finite(v)&&v>=0f;
    }
}
