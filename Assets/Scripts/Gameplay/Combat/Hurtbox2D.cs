using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class Hurtbox2D : MonoBehaviour
    {
        private DamageReceiver2D receiver;

        public DamageReceiver2D Receiver => receiver;

        private void Awake()
        {
            receiver = GetComponentInParent<DamageReceiver2D>();
        }

        /// <summary>Compatibility API: returns true only when damage was applied.</summary>
        public bool TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection,
            out DamageReceiver2D hitReceiver)
        {
            hitReceiver = receiver;
            return receiver != null && receiver.TryReceiveHit(attack, attacker, facingDirection);
        }
        internal CombatContactResult ResolveHit(AttackDefinition attack, DamageReceiver2D attacker,
            float facingDirection, int comboStepIndex, out DamageReceiver2D hitReceiver,
            out CombatParryEvent parryEvent)
        {
            hitReceiver = receiver;
            parryEvent = default;
            return receiver == null
                ? CombatContactResult.Rejected
                : receiver.ResolveHit(attack, attacker, facingDirection, comboStepIndex, out parryEvent);
        }
    }
}
