using HuntrX.Gameplay.Combat;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AgeDifficultyPolicy2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        private float assistance=1f;
        public float Assistance=>assistance;
        public bool TrySetAssistance(float value)
        {
            if(float.IsNaN(value) || float.IsInfinity(value) || value<.6f || value>1f) return false;
            assistance=value; return true;
        }
        private AgeProfileData Profile=>isActiveAndEnabled && profiles!=null && profiles.isActiveAndEnabled && profiles.Current!=null && profiles.Current.IsValid(out _)?profiles.Current:null;
        public static float CalculateDamage(AttackDefinition attack,DamageReceiver2D attacker,DamageReceiver2D receiver)
        {
            if(attack==null || attacker==null || receiver==null) return float.NaN;
            var sourcePolicy=attacker.GetComponent<AgeDifficultyPolicy2D>();
            var targetPolicy=receiver.GetComponent<AgeDifficultyPolicy2D>();
            var sourceProfile=sourcePolicy!=null?sourcePolicy.Profile:null;
            var targetProfile=targetPolicy!=null?targetPolicy.Profile:null;
            float outgoing=attacker.Faction==CombatFaction2D.HuntrX && sourceProfile!=null?sourceProfile.EnemyDamageTaken:1f;
            float incoming=receiver.Faction==CombatFaction2D.HuntrX && targetProfile!=null?targetProfile.PlayerDamageTaken*targetPolicy.assistance:1f;
            return attack.Damage*outgoing*incoming;
        }
        public static float TelegraphDuration(Component enemy,float authored)
        {
            var policy=enemy!=null?enemy.GetComponent<AgeDifficultyPolicy2D>():null;
            var profile=policy!=null?policy.Profile:null;
            return profile!=null?(float)System.Math.Min(60d,(double)authored*profile.EnemyTelegraphMultiplier/System.Math.Max(.6d,policy.assistance)):authored;
        }
        public static int BurstCount(Component enemy,int authored)
        {
            var policy=enemy!=null?enemy.GetComponent<AgeDifficultyPolicy2D>():null;
            var profile=policy!=null?policy.Profile:null;
            return profile!=null?Mathf.Clamp(authored,1,profile.MaximumBurstCount):authored;
        }
        public static float RecoveryDuration(Component enemy,float authored)=>TelegraphDuration(enemy,authored);
    }
}
