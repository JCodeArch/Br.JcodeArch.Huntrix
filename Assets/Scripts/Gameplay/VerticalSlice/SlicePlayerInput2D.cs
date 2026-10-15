using System;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
namespace HuntrX.Gameplay.VerticalSlice
{
    [DisallowMultipleComponent]
    public sealed class SlicePlayerInput2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private CharacterSwitchController2D characterSwitch;
        [SerializeField] private MonoBehaviour inputPreferences;
        [SerializeField] private LayerMask groundMask = ~0;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        private readonly InputAction[] actions = new InputAction[14];
        private readonly bool[] toggles = new bool[14];
        private readonly bool[] previousHeld = new bool[14];
        private readonly bool[] pressed = new bool[14], released = new bool[14], held = new bool[14];
        private readonly string[] paths = new string[14];
        private static readonly string[] defaults = { "a", "d", "w", "s", "space", "leftShift", "j", "k", "q", "e", "r", "f", "l", "escape" };
        private static readonly string[] keyboardPaths = Array.ConvertAll(defaults, key => "<Keyboard>/" + key);
        private static readonly string[] gamepad = { "dpad/left", "dpad/right", "dpad/up", "dpad/down", "buttonSouth", "rightShoulder", "buttonWest", "buttonEast", "leftShoulder", "buttonNorth", "select", "rightStickPress", "rightTrigger", "start" };
        private DamageReceiver2D aimTarget;
        public void SetAimTarget(DamageReceiver2D target) => aimTarget = target;
        private SliceInputFrame submitted;
        private bool hasSubmitted;
        private float facing = 1f;
        private bool switchingAllowed = true;
        public void SetCharacterSwitchAllowed(bool value) => switchingAllowed = value;
        public CharacterManager2D Manager => manager;
        public bool BindPreferences(MonoBehaviour source) { if (source != null && !(source is IInputPreferenceSource)) return false; inputPreferences = source; return true; }
        public bool IsGrounded { get; private set; }
        public event Action<SliceInputFrame> ActionsRequested;
        private void OnEnable()
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] == null)
                {
                    actions[i] = new InputAction(((GameplayControl)i).ToString(), InputActionType.Button);
                    paths[i] = keyboardPaths[i];
                    actions[i].AddBinding(paths[i]); actions[i].AddBinding("<Gamepad>/" + gamepad[i]);
                }
                actions[i].Enable();
            }
        }
        public bool TryBind(CharacterManager2D owner, CharacterSwitchController2D switching)
        {
            if (owner == null || switching == null) return false;
            manager = owner; characterSwitch = switching; return true;
        }
        public void SubmitInput(SliceInputFrame frame)
        {
            if (!isActiveAndEnabled || !Finite(frame.Move.x) || !Finite(frame.Move.y)) return;
            submitted.Move = Vector2.ClampMagnitude(frame.Move, 1f);
            submitted.JumpPressed |= frame.JumpPressed; submitted.JumpReleased |= frame.JumpReleased;
            submitted.Dash |= frame.Dash; submitted.Attack |= frame.Attack; submitted.Parry |= frame.Parry;
            submitted.Switch |= frame.Switch; submitted.Interact |= frame.Interact; submitted.InteractHeld |= frame.InteractHeld; submitted.InteractReleased |= frame.InteractReleased;
            submitted.FoodSelf |= frame.FoodSelf; submitted.FoodFan |= frame.FoodFan; submitted.Special |= frame.Special;
            submitted.Pause |= frame.Pause;
            hasSubmitted = true;
        }
        private void Update()
        {
            if (!isActiveAndEnabled || manager == null || !manager.isActiveAndEnabled || manager.State != CharacterManagerState2D.Alive || manager.ActiveCharacter == null || !manager.ActiveCharacter.isActiveAndEnabled || !manager.ActiveCharacter.IsAlive) { ResetHeld(); return; }
            SliceInputFrame frame = ReadPhysical();
            if (hasSubmitted)
            {
                if (submitted.Move.sqrMagnitude > frame.Move.sqrMagnitude) frame.Move = submitted.Move;
                frame.JumpPressed |= submitted.JumpPressed; frame.JumpReleased |= submitted.JumpReleased;
                frame.Dash |= submitted.Dash; frame.Attack |= submitted.Attack; frame.Parry |= submitted.Parry;
                frame.Switch |= submitted.Switch; frame.Interact |= submitted.Interact; frame.InteractHeld |= submitted.InteractHeld; frame.InteractReleased |= submitted.InteractReleased;
                frame.FoodSelf |= submitted.FoodSelf; frame.FoodFan |= submitted.FoodFan; frame.Special |= submitted.Special;
                frame.Pause |= submitted.Pause;
                submitted = default; hasSubmitted = false;
            }
            if (Time.timeScale <= 0f) { manager.ActiveCharacter.GetComponent<HorizontalMovement2D>()?.SetMovementInput(Vector2.zero); return; }
            DamageReceiver2D actor = manager.ActiveCharacter;
            UpdateSensors(actor);
            if (Mathf.Abs(frame.Move.x) > 0.01f) facing = Mathf.Sign(frame.Move.x);
            actor.GetComponent<HorizontalMovement2D>()?.SetMovementInput(frame.Move);
            JumpController2D jump = actor.GetComponent<JumpController2D>();
            if (frame.JumpPressed) jump?.PressJump(); if (frame.JumpReleased) jump?.ReleaseJump();
            DashController2D dash = actor.GetComponent<DashController2D>();
            if (frame.Dash) { if (IsGrounded) dash?.TryStartDash(facing); else dash?.TryStartAirDash(frame.Move.sqrMagnitude > 0.01f ? frame.Move : new Vector2(facing, 0f)); }
            if (!CanApply(actor)) return;
            if (frame.Attack)
            {
                var ranged = actor.GetComponent<ZoeyRangedAttack2D>();
                if (ranged != null)
                {
                    Vector2 aim = frame.Move.sqrMagnitude > 0.01f ? frame.Move : new Vector2(facing, 0f);
                    if (inputPreferences != null && inputPreferences.isActiveAndEnabled && inputPreferences is IInputAimPreferenceSource preferences && preferences.AssistedAim &&
                        aimTarget != null && aimTarget.isActiveAndEnabled && aimTarget.IsAlive && aimTarget.Faction == CombatFaction2D.Demon)
                    {
                        Vector2 offset = aimTarget.transform.position - actor.transform.position;
                        if (offset.sqrMagnitude > 0.001f && offset.sqrMagnitude <= 64f && Vector2.Angle(aim, offset) <= 45f) aim = offset.normalized;
                    }
                    ranged.TryFire(aim);
                }
                else actor.GetComponent<AttackController2D>()?.TryStartAttack(facing, IsGrounded);
            }
            if (!CanApply(actor)) return;
            if (frame.Parry) actor.GetComponent<ParryController2D>()?.TryStartParry();
            if (!CanApply(actor)) return;
            if (frame.Switch && switchingAllowed && characterSwitch != null) characterSwitch.TrySwitchNext(out _);
            if (!CanApply(actor)) return;
            Action<SliceInputFrame> handlers = ActionsRequested;
            if (handlers != null) foreach (Action<SliceInputFrame> handler in handlers.GetInvocationList())
                try { if (!CanApply(actor)) break; handler(frame); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        private SliceInputFrame ReadPhysical()
        {
            IInputPreferenceSource preferences = inputPreferences != null && inputPreferences.isActiveAndEnabled ? inputPreferences as IInputPreferenceSource : null;
            for (int i = 0; i < actions.Length; i++)
            {
                string next = preferences != null ? preferences.KeyboardBinding((GameplayControl)i) : null;
                if (string.IsNullOrWhiteSpace(next)) next = keyboardPaths[i];
                if (!string.IsNullOrWhiteSpace(next) && next != paths[i]) { actions[i].ApplyBindingOverride(0, next); paths[i] = next; }
                bool value = actions[i].IsPressed();
                if (preferences != null && preferences.ToggleModeFor((GameplayControl)i))
                { if (actions[i].WasPressedThisFrame()) toggles[i] = !toggles[i]; value = toggles[i]; }
                bool toggle = preferences != null && preferences.ToggleModeFor((GameplayControl)i);
                held[i] = value; pressed[i] = toggle ? value && !previousHeld[i] : actions[i].WasPressedThisFrame();
                released[i] = toggle ? !value && previousHeld[i] : actions[i].WasReleasedThisFrame(); previousHeld[i] = value;
            }
            Vector2 move = new Vector2((held[1] ? 1f : 0f) - (held[0] ? 1f : 0f), (held[2] ? 1f : 0f) - (held[3] ? 1f : 0f));
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > move.sqrMagnitude) move = Gamepad.current.leftStick.ReadValue();
            return new SliceInputFrame { Move = Vector2.ClampMagnitude(move, 1f), JumpPressed = pressed[4], JumpReleased = released[4], Dash = pressed[5], Attack = pressed[6], Parry = pressed[7], Switch = pressed[8], Interact = pressed[9], InteractHeld = held[9], InteractReleased = released[9], FoodSelf = pressed[10], FoodFan = pressed[11], Special = pressed[12], Pause = pressed[13] };
        }
        private bool CanApply(DamageReceiver2D actor) => isActiveAndEnabled && manager != null && manager.isActiveAndEnabled && manager.State == CharacterManagerState2D.Alive && manager.ActiveCharacter == actor && actor != null && actor.isActiveAndEnabled && actor.IsAlive;
        private void UpdateSensors(DamageReceiver2D actor)
        {
            Collider2D collider = actor.GetComponent<Collider2D>();
            if (collider == null) { IsGrounded = false; return; }
            Bounds bounds = collider.bounds;
            IsGrounded = Contact(actor, bounds.center, Vector2.down, bounds.extents.y + 0.08f, out _);
            bool wall = Contact(actor, bounds.center, new Vector2(facing, 0f), bounds.extents.x + 0.08f, out Vector2 normal);
            var jump = actor.GetComponent<JumpController2D>(); jump?.SetGrounded(IsGrounded); jump?.SetWallContact(wall, normal);
            actor.GetComponent<DashController2D>()?.SetGrounded(IsGrounded);
        }
        private bool Contact(DamageReceiver2D actor, Vector2 origin, Vector2 direction, float distance, out Vector2 normal)
        {
            normal = Vector2.zero;
            ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(groundMask); filter.useTriggers = false;
            int count = Physics2D.Raycast(origin, direction, filter, hits, distance); if (count == hits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider == null || hits[i].collider.transform.root == actor.transform.root) continue;
                if (direction.y < 0f && hits[i].normal.y < 0.5f) continue;
                if (direction.x != 0f && Mathf.Abs(hits[i].normal.x) < 0.5f) continue;
                normal = hits[i].normal; return true;
            }
            return false;
        }
        private void ResetHeld() { Array.Clear(toggles, 0, toggles.Length); Array.Clear(previousHeld, 0, previousHeld.Length); submitted = default; hasSubmitted = false; }
        private void OnDisable()
        {
            foreach (InputAction action in actions) action?.Disable(); ResetHeld();
            if (manager != null && manager.ActiveCharacter != null) { manager.ActiveCharacter.GetComponent<HorizontalMovement2D>()?.SetMovementInput(Vector2.zero); manager.ActiveCharacter.GetComponent<JumpController2D>()?.ReleaseJump(); }
        }
        private void OnDestroy() { foreach (InputAction action in actions) action?.Dispose(); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
