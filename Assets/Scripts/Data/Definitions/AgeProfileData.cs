using UnityEngine;
namespace HuntrX.Data
{
    public enum AgeBand { Ages4To5, Ages6To8, Ages9To12, Ages13To15, Ages16Plus }
    [CreateAssetMenu(fileName="AgeProfile",menuName="HUNTR/X/Data/Age Profile")]
    public sealed class AgeProfileData : GameDataDefinition
    {
        [SerializeField] private AgeBand band;
        [SerializeField] private float visualIntensity=.3f;
        [SerializeField] private bool allowBlood;
        [SerializeField] private int particleBudget=4;
        [SerializeField] private float playerDamageTaken=.5f;
        [SerializeField] private float enemyDamageTaken=1.2f;
        [SerializeField] private float enemyTelegraphMultiplier=1.5f;
        [SerializeField] private int maximumBurstCount=1;
        public int MaximumBurstCount=>maximumBurstCount;
        [SerializeField] private bool checkpointAssistance=true;
        [SerializeField] private float checkpointRadius=1.5f;
        [SerializeField] private float narrativeIntensity=.25f;
        public AgeBand Band=>band;
        public float VisualIntensity=>visualIntensity;
        public bool AllowBlood=>allowBlood;
        public int ParticleBudget=>particleBudget;
        public float PlayerDamageTaken=>playerDamageTaken;
        public float EnemyDamageTaken=>enemyDamageTaken;
        public float EnemyTelegraphMultiplier=>enemyTelegraphMultiplier;
        public bool CheckpointAssistance=>checkpointAssistance;
        public float CheckpointRadius=>checkpointRadius;
        public float NarrativeIntensity=>narrativeIntensity;
        public bool IsValid(out string error)
        {
            if(!System.Enum.IsDefined(typeof(AgeBand),band) || !Unit(visualIntensity) || !Unit(narrativeIntensity) || particleBudget<0 || particleBudget>64 || maximumBurstCount<1 || maximumBurstCount>8 ||
                !Bounded(playerDamageTaken,.1f,2f) || !Bounded(enemyDamageTaken,.1f,2f) || !Bounded(enemyTelegraphMultiplier,1f,3f) || !Bounded(checkpointRadius,.1f,5f) ||
                ((band==AgeBand.Ages4To5 || band==AgeBand.Ages6To8) && allowBlood))
            { error="Age profile violates validated prototype intensity/difficulty bounds or child visual restrictions."; return false; }
            error=string.Empty; return true;
        }
        private static bool Unit(float value)=>Bounded(value,0f,1f);
        private static bool Bounded(float value,float minimum,float maximum)=>!float.IsNaN(value) && !float.IsInfinity(value) && value>=minimum && value<=maximum;
    }
}
