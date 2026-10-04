using HuntrX.Data;

namespace HuntrX.Gameplay.Combat
{
    /// <summary>
    /// Describes one accepted combat hit. Trigger overlaps do not provide a unique contact point.
    /// </summary>
    public readonly struct CombatImpactEvent
    {
        public CombatImpactEvent(DamageReceiver2D attacker, DamageReceiver2D receiver,
            AttackDefinition attackDefinition, int comboStepIndex)
        {
            Attacker = attacker;
            Receiver = receiver;
            AttackDefinition = attackDefinition;
            ComboStepIndex = comboStepIndex;
        }

        public DamageReceiver2D Attacker { get; }
        public DamageReceiver2D Receiver { get; }
        public AttackDefinition AttackDefinition { get; }
        public int ComboStepIndex { get; }
    }
}
