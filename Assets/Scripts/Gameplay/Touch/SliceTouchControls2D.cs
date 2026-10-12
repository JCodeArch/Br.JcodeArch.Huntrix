using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
using UnityEngine.InputSystem;
namespace HuntrX.Gameplay.Touch
{
    /// <summary>Independent touch contacts drive the same logical frames as keyboard/gamepad.</summary>
    [DefaultExecutionOrder(-50), DisallowMultipleComponent]
    public sealed class SliceTouchControls2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private MonoBehaviour preferences;
        [SerializeField] private bool showInDesktop = true;
        private readonly bool[] held = new bool[9], previous = new bool[9], toggled = new bool[9];
        private readonly Rect[] buttons = new Rect[9];
        private static readonly string[] labels = { "Pulo", "Ataque", "Dash", "Parry", "Troca", "SP", "Salvar", "Eu", "Fã" };
        private static readonly GameplayControl[] controls = { GameplayControl.Jump, GameplayControl.Attack, GameplayControl.Dash, GameplayControl.Parry, GameplayControl.SwitchCharacter, GameplayControl.Special, GameplayControl.Interact, GameplayControl.FoodSelf, GameplayControl.FoodFan };
        private Rect pad;
        private Vector2 movement;
        private bool previousJump, previousInteract;
        public bool TryBind(VerticalSliceSession2D owner, MonoBehaviour source)
        {
            if (owner == null || (source != null && !(source is IInputPreferenceSource))) return false;
            Release(); session = owner; preferences = source; return true;
        }
        private void Layout()
        {
            Rect safe = Screen.safeArea; float unit = Mathf.Clamp(Mathf.Min(safe.width / 12f, safe.height / 7f), 45f, 90f);
            float bottom = Screen.height - safe.yMin;
            pad = new Rect(safe.xMin + unit * .25f, bottom - unit * 2.5f, unit * 2.2f, unit * 2.2f);
            for (int i = 0; i < 6; i++) buttons[i] = new Rect(safe.xMax - unit * (3.35f - i % 3), bottom - unit * (2.4f - i / 3), unit * .9f, unit * .9f);
            for (int i = 6; i < 9; i++) buttons[i] = new Rect(safe.xMin + unit * (.25f + i - 6), bottom - unit * 3.6f, unit * .9f, unit * .9f);
        }
        private void Update()
        {
            if (session == null || !session.IsRunning || session.Input == null || !session.Input.isActiveAndEnabled) { Release(); return; }
            Layout(); movement = Vector2.zero; System.Array.Clear(held, 0, held.Length);
            Touchscreen touch = Touchscreen.current;
            if (touch != null) for (int i = 0; i < touch.touches.Count; i++) if (touch.touches[i].press.isPressed) Contact(touch.touches[i].position.ReadValue());
            if (showInDesktop && Mouse.current != null && Mouse.current.leftButton.isPressed) Contact(Mouse.current.position.ReadValue());
            var source = preferences != null && preferences.isActiveAndEnabled ? preferences as IInputPreferenceSource : null;
            for (int i = 0; i < held.Length; i++)
            {
                bool pressed = held[i] && !previous[i];
                if (source == null || !source.ToggleModeFor(controls[i])) toggled[i] = false;
                else if (pressed) toggled[i] = !toggled[i];
            }
            bool jump = Held(0, source), interact = Held(6, source);
            var frame = new SliceInputFrame { Move = movement, JumpPressed = held[0] && !previous[0], JumpReleased = !jump && previousJump,
                Attack = held[1] && !previous[1], Dash = held[2] && !previous[2], Parry = held[3] && !previous[3], Switch = held[4] && !previous[4],
                Special = held[5] && !previous[5], Interact = held[6] && !previous[6], InteractHeld = interact,
                InteractReleased = !interact && previousInteract, FoodSelf = held[7] && !previous[7], FoodFan = held[8] && !previous[8] };
            // Toggle jump's off transition must release even when the contact is still down.
            if (source != null && source.ToggleModeFor(GameplayControl.Jump)) { frame.JumpPressed = held[0] && !previous[0] && jump; frame.JumpReleased = held[0] && !previous[0] && !jump; }
            if (source != null && source.ToggleModeFor(GameplayControl.Interact)) frame.Interact = held[6] && !previous[6] && interact;
            session.Input.SubmitInput(frame); previousJump = jump; previousInteract = interact; System.Array.Copy(held, previous, held.Length);
        }
        private bool Held(int i, IInputPreferenceSource source) => source != null && source.ToggleModeFor(controls[i]) ? toggled[i] : held[i];
        private void Contact(Vector2 screen)
        {
            Vector2 point = new Vector2(screen.x, Screen.height - screen.y);
            if (pad.Contains(point)) { Vector2 next = new Vector2((point.x-pad.center.x)/(pad.width*.4f), -(point.y-pad.center.y)/(pad.height*.4f)); if (next.sqrMagnitude > movement.sqrMagnitude) movement = Vector2.ClampMagnitude(next,1f); }
            for (int i = 0; i < buttons.Length; i++) if (buttons[i].Contains(point)) held[i] = true;
        }
        private void Release()
        {
            if (session != null && session.Input != null) session.Input.SubmitInput(new SliceInputFrame { JumpReleased = previousJump, InteractReleased = previousInteract });
            previousJump = false; previousInteract = false; System.Array.Clear(previous,0,previous.Length); System.Array.Clear(toggled,0,toggled.Length); movement=Vector2.zero;
        }
        private void OnGUI()
        {
            if ((!showInDesktop && Touchscreen.current == null) || session == null || !session.IsRunning || session.Input == null || !session.Input.isActiveAndEnabled) return;
            Layout(); GUI.Box(pad,"Mover"); for (int i=0;i<buttons.Length;i++) GUI.Box(buttons[i],labels[i]);
        }
        private void OnDisable() => Release();
    }
}
