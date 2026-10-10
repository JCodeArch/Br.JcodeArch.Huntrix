using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "FanDefinition", menuName = "HUNTR/X/Data/Fan")]
    public sealed class FanDefinition : GameDataDefinition
    {
        [SerializeField] private float maximumSoul = 100f;
        [SerializeField] private float criticalFraction = 0.3f;
        public float MaximumSoul => maximumSoul;
        public float CriticalFraction => criticalFraction;
        public bool IsValid(out string error)
        {
            bool valid = Finite(maximumSoul) && maximumSoul > 0f && Finite(criticalFraction) && criticalFraction > 0f && criticalFraction < 1f;
            error = valid ? string.Empty : "Fan requires finite positive soul and critical fraction inside (0,1).";
            return valid;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
