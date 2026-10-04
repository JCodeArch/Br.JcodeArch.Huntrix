using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>Static authored timing and area for Mira's defensive field.</summary>
    [CreateAssetMenu(fileName = "MiraProtectionDefinition", menuName = "HUNTR/X/Data/Mira Protection")]
    public sealed class MiraProtectionDefinition : GameDataDefinition
    {
        [SerializeField] private float durationSeconds;
        [SerializeField] private float radius;

        public float DurationSeconds => durationSeconds;
        public float Radius => radius;

        public bool IsValid(out string error)
        {
            if (!IsFinitePositive(durationSeconds))
            {
                error = "MiraProtectionDefinition duration must be finite and positive.";
                return false;
            }

            if (!IsFinitePositive(radius))
            {
                error = "MiraProtectionDefinition radius must be finite and positive.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool IsFinitePositive(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }
}
