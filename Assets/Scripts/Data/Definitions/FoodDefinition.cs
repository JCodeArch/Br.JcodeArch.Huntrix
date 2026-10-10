using UnityEngine;
namespace HuntrX.Data
{
    public enum FoodKind { Ramen, Onigiri, Gyoza, Dango }
    [CreateAssetMenu(fileName="FoodDefinition",menuName="HUNTR/X/Data/Food")]
    public sealed class FoodDefinition : GameDataDefinition
    {
        [SerializeField] private FoodKind kind;
        [SerializeField] private float selfRecovery=10f;
        [SerializeField] private float fanRecovery=10f;
        public FoodKind Kind => kind;
        public float SelfRecovery => selfRecovery;
        public float FanRecovery => fanRecovery;
        public bool IsValid(out string error)
        {
            if(!System.Enum.IsDefined(typeof(FoodKind),kind) || !Positive(selfRecovery) || !Positive(fanRecovery))
            { error="Food requires a known kind and finite positive self/fan recovery."; return false; }
            error=string.Empty; return true;
        }
        private static bool Positive(float value) => value>0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
