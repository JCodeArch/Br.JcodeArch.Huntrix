using System;
using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>
    /// One authored attack in a grounded or aerial combo sequence.
    /// </summary>
    [Serializable]
    public struct ComboStep
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private float chainWindowStart;
        [SerializeField] private float chainWindowEnd;
        [SerializeField] private float hitStopDuration;

        public AttackDefinition Attack => attack;
        public float ChainWindowStart => chainWindowStart;
        public float ChainWindowEnd => chainWindowEnd;
        public float HitStopDuration => hitStopDuration;

        public ComboStep(AttackDefinition attack, float chainWindowStart, float chainWindowEnd, float hitStopDuration)
        {
            this.attack = attack;
            this.chainWindowStart = chainWindowStart;
            this.chainWindowEnd = chainWindowEnd;
            this.hitStopDuration = hitStopDuration;
        }
    }
}
