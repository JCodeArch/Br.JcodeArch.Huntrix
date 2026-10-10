using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName="GwiMaBoss",menuName="HUNTR/X/Data/Gwi-Ma Boss")]
    public sealed class GwiMaBossDefinition : GameDataDefinition
    {
        [SerializeField] private EnemyPatternDefinition opening;
        [SerializeField] private EnemyPatternDefinition pressure;
        [SerializeField] private EnemyPatternDefinition finale;
        [SerializeField] private float pressureHealthFraction=.7f;
        [SerializeField] private float finaleHealthFraction=.35f;
        [SerializeField] private string endingCue="ending.gwi-ma.defeated";
        public EnemyPatternDefinition Opening=>opening;
        public EnemyPatternDefinition Pressure=>pressure;
        public EnemyPatternDefinition Finale=>finale;
        public float PressureHealthFraction=>pressureHealthFraction;
        public float FinaleHealthFraction=>finaleHealthFraction;
        public string EndingCue=>endingCue;
        public bool IsValid(out string error)
        {
            if(opening==null || pressure==null || finale==null || opening==pressure || opening==finale || pressure==finale ||
                !opening.IsValid(out _) || !pressure.IsValid(out _) || !finale.IsValid(out _) ||
                !Finite(pressureHealthFraction) || !Finite(finaleHealthFraction) || finaleHealthFraction<=0f ||
                pressureHealthFraction>=1f || finaleHealthFraction>=pressureHealthFraction || string.IsNullOrWhiteSpace(endingCue))
            { error="Gwi-Ma requires three distinct valid patterns, ordered health thresholds and an ending cue ID.";return false; }
            error=string.Empty;return true;
        }
        private static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
    }
}
