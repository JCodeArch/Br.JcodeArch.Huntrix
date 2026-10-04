using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.Movement;
using UnityEngine;

namespace HuntrX.Gameplay
{
    /// <summary>Applies a validated static character profile to the existing shared gameplay controllers.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class CharacterDefinitionApplier2D : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition definition;
        private HorizontalMovement2D movement;
        private JumpController2D jump;
        private DashController2D dash;
        private AttackController2D attack;
        private ParryController2D parry;

        public CharacterDefinition Definition => definition;

        private void Awake()
        {
            movement = GetComponent<HorizontalMovement2D>();
            jump = GetComponent<JumpController2D>();
            dash = GetComponent<DashController2D>();
            attack = GetComponent<AttackController2D>();
            parry = GetComponent<ParryController2D>();
            if (definition != null && !TryApply(definition, out string error))
                Debug.LogError("CharacterDefinitionApplier2D could not apply " + definition.name + ": " + error, this);
        }

        public bool TryApply(CharacterDefinition character, out string error)
        {
            error = string.Empty;
            if (character == null || !character.IsValid(out error))
            {
                if (string.IsNullOrEmpty(error)) error = "A valid CharacterDefinition is required.";
                return false;
            }

            CacheControllers();
            if (movement == null || jump == null || dash == null || attack == null || parry == null)
            {
                error = "CharacterDefinitionApplier2D requires movement, jump, dash, attack, and parry controllers on the same GameObject.";
                return false;
            }
            if (dash.IsDashing || attack.State != AttackState2D.Idle || parry.IsWindowActive)
            {
                error = "CharacterDefinition cannot be applied while dash, attack, or parry is active.";
                return false;
            }

            CharacterMovementDefinition profile = character.Movement;
            // All values and controller availability are checked before any component is changed.
            movement.ConfigureValidated(profile.MaxHorizontalSpeed, profile.Acceleration, profile.Deceleration);
            jump.ConfigureValidated(profile.JumpVelocity, profile.WallJumpHorizontalSpeed, profile.GravityScale,
                profile.CoyoteTime, profile.JumpBufferTime, profile.JumpCutMultiplier);
            dash.ConfigureValidated(profile.DashSpeed, profile.DashDuration, profile.AirDashSpeed, profile.AirDashDuration);
            attack.SetValidatedDefinition(character.CombatCombo);
            parry.SetValidatedDefinition(character.Parry);
            definition = character;
            error = string.Empty;
            return true;
        }

        private void CacheControllers()
        {
            if (movement == null) movement = GetComponent<HorizontalMovement2D>();
            if (jump == null) jump = GetComponent<JumpController2D>();
            if (dash == null) dash = GetComponent<DashController2D>();
            if (attack == null) attack = GetComponent<AttackController2D>();
            if (parry == null) parry = GetComponent<ParryController2D>();
        }
    }
}
