using HuntrX.Data;

namespace HuntrX.Gameplay.Combat
{
    /// <summary>Describes an attack successfully parried by a defender.</summary>
    public readonly struct CombatParryEvent
    {
        public CombatParryEvent(DamageReceiver2D defender, DamageReceiver2D attacker,
            AttackDefinition attackDefinition, int comboStepIndex)
        {
            Defender = defender;
            Attacker = attacker;
            AttackDefinition = attackDefinition;
            ComboStepIndex = comboStepIndex;
        }

        public DamageReceiver2D Defender { get; }
        public DamageReceiver2D Attacker { get; }
        public AttackDefinition AttackDefinition { get; }
        public int ComboStepIndex { get; }
    }
}
