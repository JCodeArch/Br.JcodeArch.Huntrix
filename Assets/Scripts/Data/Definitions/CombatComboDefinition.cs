using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>
    /// Static authored data for grounded and aerial combo chains.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatComboDefinition", menuName = "HUNTR/X/Data/Combat Combo")]
    public sealed class CombatComboDefinition : GameDataDefinition
    {
        [SerializeField] private ComboStep[] groundedSteps = Array.Empty<ComboStep>();
        [SerializeField] private ComboStep[] aerialSteps = Array.Empty<ComboStep>();

        public IReadOnlyList<ComboStep> GroundedSteps => groundedSteps ?? Array.Empty<ComboStep>();
        public IReadOnlyList<ComboStep> AerialSteps => aerialSteps ?? Array.Empty<ComboStep>();

        /// <summary>
        /// Validates both authored chains without emitting Unity log messages.
        /// </summary>
        public bool IsValid(out string error)
        {
            if (!ValidateSequence(groundedSteps, "grounded", out error))
                return false;
            if (!ValidateSequence(aerialSteps, "aerial", out error))
                return false;

            error = string.Empty;
            return true;
        }

        private static bool ValidateSequence(ComboStep[] steps, string sequenceName, out string error)
        {
            if (steps == null || steps.Length < 2)
            {
                error = "Combat combo " + sequenceName + " sequence must contain at least two steps.";
                return false;
            }

            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                var stepName = "Combat combo " + sequenceName + " step " + index + ": ";
                if (step.Attack == null)
                {
                    error = stepName + "attack reference is required.";
                    return false;
                }

                if (!step.Attack.IsValid(out var attackError))
                {
                    error = stepName + attackError;
                    return false;
                }

                var attackDuration = step.Attack.StartupDuration + step.Attack.ActiveDuration + step.Attack.RecoveryDuration;
                if (!IsFinite(attackDuration))
                {
                    error = stepName + "attack total duration must be finite.";
                    return false;
                }

                if (!IsFiniteNonnegative(step.HitStopDuration))
                {
                    error = stepName + "hit-stop duration must be finite and nonnegative.";
                    return false;
                }

                // The last step has no outgoing link, so its serialized window is unused.
                if (index == steps.Length - 1)
                    continue;

                if (!IsFinite(step.ChainWindowStart) || !IsFinite(step.ChainWindowEnd))
                {
                    error = stepName + "chain window bounds must be finite.";
                    return false;
                }
                if (step.ChainWindowStart < 0f || step.ChainWindowStart >= step.ChainWindowEnd)
                {
                    error = stepName + "chain window must have a nonnegative start before its end.";
                    return false;
                }
                if (step.ChainWindowEnd > attackDuration)
                {
                    error = stepName + "chain window must fit within the attack total duration.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsFiniteNonnegative(float value) => IsFinite(value) && value >= 0f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
