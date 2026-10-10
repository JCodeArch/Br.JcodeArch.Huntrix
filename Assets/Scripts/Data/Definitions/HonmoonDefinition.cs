using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "HonmoonDefinition", menuName = "HUNTR/X/Data/Honmoon")]
    public sealed class HonmoonDefinition : GameDataDefinition
    {
        [SerializeField] private float initialValue = 100f;
        [SerializeField] private float crisisEnterValue = 20f;
        [SerializeField] private float crisisExitValue = 40f;
        public float InitialValue => initialValue;
        public float CrisisEnterValue => crisisEnterValue;
        public float CrisisExitValue => crisisExitValue;
        public bool IsValid(out string error)
        {
            bool valid = Finite(initialValue) && initialValue >= 0f && initialValue <= 100f &&
                Finite(crisisEnterValue) && crisisEnterValue >= 0f && crisisEnterValue < 100f &&
                Finite(crisisExitValue) && crisisExitValue > crisisEnterValue && crisisExitValue <= 100f;
            error = valid ? string.Empty : "Honmoon requires values in 0..100 and an exit threshold above crisis entry.";
            return valid;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
