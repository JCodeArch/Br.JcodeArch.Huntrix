using System;
using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>Static authored timing for the defensive parry window.</summary>
    [CreateAssetMenu(fileName = "ParryDefinition", menuName = "HUNTR/X/Data/Parry")]
    public sealed class ParryDefinition : GameDataDefinition
    {
        [SerializeField] private float windowDuration;

        public float WindowDuration => windowDuration;

        public bool IsValid(out string error)
        {
            if (float.IsNaN(windowDuration) || float.IsInfinity(windowDuration) || windowDuration <= 0f)
            {
                error = "ParryDefinition window duration must be finite and positive.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
