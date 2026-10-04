using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>Static authored identity and gameplay profile references for one playable character.</summary>
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "HUNTR/X/Data/Character")]
    public sealed class CharacterDefinition : GameDataDefinition
    {
        [SerializeField] private string displayName;
        [SerializeField] private CharacterMovementDefinition movement;
        [SerializeField] private CombatComboDefinition combatCombo;
        [SerializeField] private ParryDefinition parry;

        public string DisplayName => displayName;
        public CharacterMovementDefinition Movement => movement;
        public CombatComboDefinition CombatCombo => combatCombo;
        public ParryDefinition Parry => parry;

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                error = "CharacterDefinition display name is required.";
                return false;
            }
            if (movement == null)
            {
                error = "CharacterDefinition movement profile is required.";
                return false;
            }
            if (!movement.IsValid(out error)) return false;
            if (combatCombo == null)
            {
                error = "CharacterDefinition combat combo is required.";
                return false;
            }
            if (!combatCombo.IsValid(out error)) return false;
            if (parry == null)
            {
                error = "CharacterDefinition parry profile is required.";
                return false;
            }
            if (!parry.IsValid(out error)) return false;

            error = string.Empty;
            return true;
        }
    }
}
