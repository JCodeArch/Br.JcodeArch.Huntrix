using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
namespace HuntrX.Gameplay.Levels
{
    /// <summary>Blockout keyboard/gamepad/lab buttons; final multiplatform input remains separate.</summary>
    public sealed class BlockoutPlayerInput2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D player;
        [SerializeField] private CharacterSwitchController2D switching;
        [SerializeField] private Camera followCamera;
        private readonly Collider2D[] groundContacts = new Collider2D[8];
        private DamageReceiver2D actor;
        private HorizontalMovement2D move;
        private JumpController2D jump;
        private DashController2D dash;
        private AttackController2D attack;
        private ParryController2D parry;
        private float facing = 1f;
        private bool grounded;
        private float touchAxis;
        private bool touchJump;
        private void Update()
        {
            DamageReceiver2D next = player != null && player.isActiveAndEnabled ? player.ActiveCharacter : null;
            if (next != actor) Bind(next);
            if (actor == null || !actor.isActiveAndEnabled || !actor.IsAlive) return;
            Keyboard keys = Keyboard.current; Gamepad pad = Gamepad.current;
            float axis = touchAxis;
            if (keys != null) axis += (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f) -
                (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
            if (pad != null) axis += pad.leftStick.ReadValue().x;
            axis = Mathf.Clamp(axis, -1f, 1f); touchAxis = 0f;
            if (Mathf.Abs(axis) > .05f) facing = Mathf.Sign(axis);
            move?.SetMovementInput(new Vector2(axis, 0f));
            if (touchJump || (keys != null && keys.spaceKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame)) jump?.PressJump();
            touchJump = false;
            if ((keys != null && keys.spaceKey.wasReleasedThisFrame) || (pad != null && pad.buttonSouth.wasReleasedThisFrame)) jump?.ReleaseJump();
            if ((keys != null && keys.leftShiftKey.wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame))
            { if (grounded) dash?.TryStartDash(facing); else dash?.TryStartAirDash(new Vector2(facing, 0f)); }
            if (!CurrentActor()) { Bind(player != null ? player.ActiveCharacter : null); return; }
            if ((keys != null && keys.jKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame)) attack?.TryStartAttack(facing, grounded);
            if (!CurrentActor()) { Bind(player != null ? player.ActiveCharacter : null); return; }
            if (keys != null && keys.kKey.wasPressedThisFrame) parry?.TryStartParry();
            if (!CurrentActor()) { Bind(player != null ? player.ActiveCharacter : null); return; }
            if (keys != null && keys.qKey.wasPressedThisFrame && switching != null) switching.TrySwitchNext(out _);
            if (!CurrentActor()) { Bind(player != null ? player.ActiveCharacter : null); return; }
            if (followCamera != null)
            {
                Vector3 position = followCamera.transform.position;
                position.x = actor.transform.position.x + 3f; position.y = Mathf.Max(2f, actor.transform.position.y + 1.5f);
                followCamera.transform.position = position;
            }
        }
        private void FixedUpdate()
        {
            if (actor == null) return;
            Collider2D collider = actor.GetComponent<Collider2D>();
            if (collider == null) return;
            Bounds bounds = collider.bounds;
            ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers = false;
            int count = Physics2D.OverlapBox(new Vector2(bounds.center.x, bounds.min.y - .04f), new Vector2(bounds.size.x * .8f, .12f), 0f, filter, groundContacts);
            grounded = false;
            if (count < groundContacts.Length) for (int i = 0; i < count; i++)
                if (groundContacts[i] != null && groundContacts[i].transform.root != actor.transform.root) { grounded = true; break; }
            jump?.SetGrounded(grounded); dash?.SetGrounded(grounded);
        }
        private void Bind(DamageReceiver2D value)
        {
            move?.SetMovementInput(Vector2.zero); jump?.ReleaseJump();
            actor = value; grounded = false;
            move = actor != null ? actor.GetComponent<HorizontalMovement2D>() : null;
            jump = actor != null ? actor.GetComponent<JumpController2D>() : null;
            dash = actor != null ? actor.GetComponent<DashController2D>() : null;
            attack = actor != null ? actor.GetComponent<AttackController2D>() : null;
            parry = actor != null ? actor.GetComponent<ParryController2D>() : null;
            if (actor != null && actor.GetComponent<BlockoutPlayerSilhouette2D>() == null) actor.gameObject.AddComponent<BlockoutPlayerSilhouette2D>();
        }
        private bool CurrentActor() => isActiveAndEnabled && player != null && player.isActiveAndEnabled &&
            actor != null && actor.isActiveAndEnabled && actor.IsAlive && player.ActiveCharacter == actor;
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12, Screen.height - 80, 380, 70), GUI.skin.box);
            GUILayout.Label("Blockout: A/D move, Space jump, Shift dash, J attack, K parry, Q switch");
            GUILayout.BeginHorizontal();
            if (GUILayout.RepeatButton("Left")) touchAxis = -1f;
            if (GUILayout.RepeatButton("Right")) touchAxis = 1f;
            if (GUILayout.Button("Jump")) touchJump = true;
            GUILayout.EndHorizontal(); GUILayout.EndArea();
        }
        private void OnDisable() { Bind(null); touchAxis = 0f; touchJump = false; }
    }
}
