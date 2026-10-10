using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "FanRescueDefinition", menuName = "HUNTR/X/Data/Fan Rescue")]
    public sealed class FanRescueDefinition : GameDataDefinition
    {
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private float recoveryDuration = 2f;
        [SerializeField] private int rewardAmount = 5;
        public float InteractionRange => interactionRange;
        public float RecoveryDuration => recoveryDuration;
        public int RewardAmount => rewardAmount;
        public bool IsValid(out string error)
        {
            bool valid = Positive(interactionRange) && Positive(recoveryDuration) && rewardAmount >= 0 && rewardAmount <= 1000;
            error = valid ? string.Empty : "Rescue needs finite positive range/duration and reward 0..1000.";
            return valid;
        }
        private static bool Positive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
