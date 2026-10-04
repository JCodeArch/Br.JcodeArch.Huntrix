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

        public bool TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection,
            out DamageReceiver2D hitReceiver)
        {
            hitReceiver = receiver;
            return receiver != null && receiver.TryReceiveHit(attack, attacker, facingDirection);
        }
    }
}
