using UnityEngine;

namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "ZoeyRangedDefinition", menuName = "HUNTR/X/Data/Zoey Ranged")]
    public sealed class ZoeyRangedDefinition : GameDataDefinition
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private float range = 12f;
        [SerializeField] private float cooldown = 0.3f;
        [SerializeField] private Vector2 muzzleOffset = Vector2.zero;

        public AttackDefinition Attack => attack;
        public float Range => range;
        public float Cooldown => cooldown;
        public Vector2 MuzzleOffset => muzzleOffset;

        public bool IsValid(out string error)
        {
            if (attack == null || !attack.IsValid(out error))
            {
                error = "Zoey requires a valid AttackDefinition.";
                return false;
            }
            if (!Finite(range) || range <= 0f || !Finite(cooldown) || cooldown <= 0f ||
                !Finite(muzzleOffset.x) || !Finite(muzzleOffset.y))
            {
                error = "Range and cooldown must be finite and positive; muzzle offset must be finite.";
                return false;
            }
            error = string.Empty;
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
