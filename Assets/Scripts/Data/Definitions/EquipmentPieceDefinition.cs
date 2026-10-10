using UnityEngine;
namespace HuntrX.Data
{
    public enum EquipmentMechanic { AirStep, ResonanceBurst }
    [CreateAssetMenu(fileName="EquipmentPiece",menuName="HUNTR/X/Data/Equipment Piece")]
    public sealed class EquipmentPieceDefinition : GameDataDefinition
    {
        [SerializeField] private EquipmentMechanic mechanic;
        [SerializeField] private string narrativePointId;
        [SerializeField] private float airStepVelocity=9f;
        [SerializeField] private AttackDefinition burstAttack;
        [SerializeField] private float burstRadius=2.5f;
        [SerializeField] private float cooldown=3f;
        public EquipmentMechanic Mechanic=>mechanic;
        public string NarrativePointId=>narrativePointId;
        public float AirStepVelocity=>airStepVelocity;
        public AttackDefinition BurstAttack=>burstAttack;
        public float BurstRadius=>burstRadius;
        public float Cooldown=>cooldown;
        public bool IsValid(out string error)
        {
            if(string.IsNullOrWhiteSpace(narrativePointId) || !System.Enum.IsDefined(typeof(EquipmentMechanic),mechanic) || !Positive(cooldown) ||
                (mechanic==EquipmentMechanic.AirStep && !Positive(airStepVelocity)) ||
                (mechanic==EquipmentMechanic.ResonanceBurst && (!Positive(burstRadius) || burstAttack==null || !burstAttack.IsValid(out _))))
            { error="Piece requires a narrative point and a valid distinct mechanical effect."; return false; }
            error=string.Empty; return true;
        }
        private static bool Positive(float value)=>value>0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
