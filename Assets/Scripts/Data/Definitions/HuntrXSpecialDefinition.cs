using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName="HuntrXSpecial",menuName="HUNTR/X/Data/HUNTR-X Special")]
    public sealed class HuntrXSpecialDefinition : GameDataDefinition
    {
        [SerializeField] private AttackDefinition rumiAttack,miraAttack,zoeyAttack,combinedAttack;
        [SerializeField] private float maximumCharge=100f;
        [SerializeField] private float chargePerDefeat=10f;
        [SerializeField] private float targetFraction=.4f;
        [SerializeField] private float range=10f;
        [SerializeField] private float stageDuration=.3f;
        public AttackDefinition RumiAttack=>rumiAttack;
        public AttackDefinition MiraAttack=>miraAttack;
        public AttackDefinition ZoeyAttack=>zoeyAttack;
        public AttackDefinition CombinedAttack=>combinedAttack;
        public float MaximumCharge=>maximumCharge;
        public float ChargePerDefeat=>chargePerDefeat;
        public float TargetFraction=>targetFraction;
        public float Range=>range;
        public float StageDuration=>stageDuration;
        public bool IsValid(out string error)
        {
            if(rumiAttack==null||miraAttack==null||zoeyAttack==null||combinedAttack==null||
                !rumiAttack.IsValid(out _)||!miraAttack.IsValid(out _)||!zoeyAttack.IsValid(out _)||!combinedAttack.IsValid(out _)||
                !Positive(maximumCharge)||!Positive(chargePerDefeat)||!Positive(targetFraction)||targetFraction>1f||
                !Positive(range)||!Positive(stageDuration))
            {error="Special requires four valid attacks and finite positive charge, range, durations and fraction in (0,1].";return false;}
            error=string.Empty;return true;
        }
        private static bool Positive(float x)=>x>0f&&!float.IsNaN(x)&&!float.IsInfinity(x);
    }
}
