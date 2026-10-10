using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "FanDrainDefinition", menuName = "HUNTR/X/Data/Fan Drain")]
    public sealed class FanDrainDefinition : GameDataDefinition
    {
        [SerializeField] private float range = 6f;
        [SerializeField] private float soulPerPulse = 10f;
        [SerializeField] private float telegraphDuration = 0.6f;
        [SerializeField] private float cooldown = 1f;
        public float Range => range;
        public float SoulPerPulse => soulPerPulse;
        public float TelegraphDuration => telegraphDuration;
        public float Cooldown => cooldown;
        public bool IsValid(out string error)
        {
            bool valid = Positive(range) && Positive(soulPerPulse) && Positive(telegraphDuration) && Positive(cooldown);
            error = valid ? string.Empty : "Fan drain requires finite positive range, amount, telegraph and cooldown.";
            return valid;
        }
        private static bool Positive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
