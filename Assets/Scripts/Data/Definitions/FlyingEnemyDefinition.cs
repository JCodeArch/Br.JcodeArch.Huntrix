using UnityEngine;

namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "FlyingEnemyDefinition", menuName = "HUNTR/X/Data/Flying Enemy")]
    public sealed class FlyingEnemyDefinition : GameDataDefinition
    {
        [SerializeField] private EnemyDefinition behavior;
        [SerializeField] private float hoverHeight = 3f;
        [SerializeField] private float altitudeAmplitude = 0.75f;
        [SerializeField] private float altitudeFrequency = 0.5f;
        [SerializeField] private float diveSpeed = 8f;
        [SerializeField] private float diveDuration = 0.5f;
        [SerializeField] private float projectileSpeed = 7f;
        [SerializeField] private float projectileLifetime = 2f;
        [SerializeField] private float projectileRange = 14f;
        [SerializeField] private GameObject projectilePrefab;
        public EnemyDefinition Behavior => behavior;
        public float HoverHeight => hoverHeight;
        public float AltitudeAmplitude => altitudeAmplitude;
        public float AltitudeFrequency => altitudeFrequency;
        public float DiveSpeed => diveSpeed;
        public float DiveDuration => diveDuration;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float ProjectileRange => projectileRange;
        public GameObject ProjectilePrefab => projectilePrefab;
        public bool IsValid(out string error)
        {
            if (behavior == null || !behavior.IsValid(out error)) { error = "Flying enemy requires a valid base behavior."; return false; }
            if (!Positive(hoverHeight) || !Nonnegative(altitudeAmplitude) || altitudeAmplitude >= hoverHeight ||
                !Positive(altitudeFrequency) || !Positive(diveSpeed) || !Positive(diveDuration) ||
                !Positive(projectileSpeed) || !Positive(projectileLifetime) || !Positive(projectileRange) ||
                projectilePrefab == null || projectilePrefab.scene.IsValid())
            { error = "Flying movement/projectile settings must be finite, positive and reference a prefab; altitude amplitude must remain below hover height."; return false; }
            error = string.Empty; return true;
        }
        private static bool Positive(float value) => Finite(value) && value > 0f;
        private static bool Nonnegative(float value) => Finite(value) && value >= 0f;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
