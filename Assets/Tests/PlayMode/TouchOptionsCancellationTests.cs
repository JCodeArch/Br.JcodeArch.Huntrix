using System.Reflection;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Touch;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace HuntrX.Tests.PlayMode
{
    public sealed class TouchOptionsCancellationTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void EscapeOptionsRequiresFreshContactsForJumpRescueAndMovement(bool toggleJump, bool toggleRescue)
        {
            float scale = Time.timeScale;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var screen = InputSystem.AddDevice<Touchscreen>();
            var root = new GameObject("Touch options cancellation");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                var session = root.AddComponent<VerticalSliceSession2D>();
                session.enabled = false; // Composition is irrelevant to the producer/pause boundary.
                typeof(VerticalSliceSession2D).GetProperty("Input").SetValue(session, input);
                typeof(VerticalSliceSession2D).GetProperty("IsRunning").SetValue(session, true);
                var preferences = root.AddComponent<AccessibilityPreferences2D>();
                preferences.TrySetToggle(GameplayControl.Jump, toggleJump);
                preferences.TrySetToggle(GameplayControl.Interact, toggleRescue);
                var touch = root.AddComponent<SliceTouchControls2D>();
                Set(touch, "showInDesktop", false);
                Assert.That(touch.TryBind(session, preferences), Is.True);
                var hud = root.AddComponent<SliceHud2D>();
                Set(hud, "session", session);
                Tick(touch, "Layout");
                var buttons = (Rect[])typeof(SliceTouchControls2D).GetField("buttons", Private).GetValue(touch);
                var pad = (Rect)typeof(SliceTouchControls2D).GetField("pad", Private).GetValue(touch);
                Vector2 jump = ScreenPoint(buttons[0].center), rescue = ScreenPoint(buttons[6].center);
                Vector2 move = ScreenPoint(pad.center + new Vector2(pad.width * .35f, 0));
                Contact(screen, 1, jump, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 2, rescue, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 3, move, UnityEngine.InputSystem.TouchPhase.Began);
                InputSystem.Update();
                Tick(touch);
                var first = Submitted(input);
                Assert.That(first.JumpPressed && first.Interact && first.InteractHeld, Is.True);
                Assert.That(first.Move.x, Is.GreaterThan(.5f));

                Key(keyboard, Key.Escape); Tick(hud); Tick(touch);
                Assert.That(input.enabled, Is.False);
                // A finger first pressed inside the menu must also be quarantined.
                Contact(screen, 4, ScreenPoint(buttons[1].center), UnityEngine.InputSystem.TouchPhase.Began);
                Key(keyboard); Tick(hud); Tick(touch);
                Key(keyboard, Key.Escape); Tick(hud); Tick(touch);
                Assert.That(input.enabled, Is.True);
                AssertNeutral(Submitted(input));
                Key(keyboard); Tick(hud); Tick(touch);
                AssertNeutral(Submitted(input));

                Contact(screen, 1, jump, UnityEngine.InputSystem.TouchPhase.Ended);
                InputSystem.Update(); Tick(touch);
                AssertNeutral(Submitted(input)); // Releasing jump cannot unlock held rescue/pad/menu contact.
                Contact(screen, 2, rescue, UnityEngine.InputSystem.TouchPhase.Ended);
                Contact(screen, 3, move, UnityEngine.InputSystem.TouchPhase.Ended);
                Contact(screen, 4, ScreenPoint(buttons[1].center), UnityEngine.InputSystem.TouchPhase.Ended);
                InputSystem.Update(); Tick(touch);
                Tick(input, "ResetHeld");
                Contact(screen, 5, jump, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 6, rescue, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 7, move, UnityEngine.InputSystem.TouchPhase.Began);
                InputSystem.Update(); Tick(touch);
                var fresh = Submitted(input);
                Assert.That(fresh.JumpPressed && fresh.Interact && fresh.InteractHeld, Is.True);
                Assert.That(fresh.Move.x, Is.GreaterThan(.5f));
                Assert.That(preferences.ToggleModeFor(GameplayControl.Jump), Is.EqualTo(toggleJump));
                Assert.That(preferences.ToggleModeFor(GameplayControl.Interact), Is.EqualTo(toggleRescue));
            }
            finally
            {
                Object.DestroyImmediate(root);
                InputSystem.RemoveDevice(screen);
                InputSystem.RemoveDevice(keyboard);
                var service = Object.FindFirstObjectByType<HitStopService>();
                if (service != null) Object.DestroyImmediate(service.gameObject);
                Time.timeScale = scale;
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ContactsStartedWithoutFocusCannotLatchActions(bool toggle, bool releaseBeforeFocus)
        {
            var screen = InputSystem.AddDevice<Touchscreen>();
            var root = new GameObject("Unfocused contact cancellation");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                var session = root.AddComponent<VerticalSliceSession2D>();
                session.enabled = false;
                typeof(VerticalSliceSession2D).GetProperty("Input").SetValue(session, input);
                typeof(VerticalSliceSession2D).GetProperty("IsRunning").SetValue(session, true);
                var preferences = root.AddComponent<AccessibilityPreferences2D>();
                preferences.TrySetToggle(GameplayControl.Jump, toggle);
                preferences.TrySetToggle(GameplayControl.Interact, toggle);
                var touch = root.AddComponent<SliceTouchControls2D>();
                Set(touch, "showInDesktop", false);
                Assert.That(touch.TryBind(session, preferences), Is.True);
                Tick(touch, "Layout");
                var buttons = (Rect[])typeof(SliceTouchControls2D).GetField("buttons", Private).GetValue(touch);
                var pad = (Rect)typeof(SliceTouchControls2D).GetField("pad", Private).GetValue(touch);
                Vector2 jump = ScreenPoint(buttons[0].center), rescue = ScreenPoint(buttons[6].center);
                Vector2 move = ScreenPoint(pad.center + new Vector2(pad.width * .35f, 0));
                Focus(touch, false);
                Contact(screen, 1, jump, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 2, rescue, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 3, move, UnityEngine.InputSystem.TouchPhase.Began);
                InputSystem.Update(); Tick(touch);
                AssertNeutral(Submitted(input));
                if (releaseBeforeFocus)
                {
                    Contact(screen, 1, jump, UnityEngine.InputSystem.TouchPhase.Ended);
                    Contact(screen, 2, rescue, UnityEngine.InputSystem.TouchPhase.Ended);
                    Contact(screen, 3, move, UnityEngine.InputSystem.TouchPhase.Ended);
                    InputSystem.Update(); Tick(touch);
                }
                // Model the physical consumer dropping submitted frames while unfocused.
                Tick(input, "ResetHeld");
                Focus(touch, true); Tick(touch);
                AssertNeutral(Submitted(input));
                if (!releaseBeforeFocus)
                {
                    Contact(screen, 1, jump, UnityEngine.InputSystem.TouchPhase.Ended);
                    Contact(screen, 2, rescue, UnityEngine.InputSystem.TouchPhase.Ended);
                    Contact(screen, 3, move, UnityEngine.InputSystem.TouchPhase.Ended);
                    InputSystem.Update(); Tick(touch);
                }
                Tick(input, "ResetHeld");
                Contact(screen, 4, jump, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 5, rescue, UnityEngine.InputSystem.TouchPhase.Began);
                Contact(screen, 6, move, UnityEngine.InputSystem.TouchPhase.Began);
                InputSystem.Update(); Tick(touch);
                var fresh = Submitted(input);
                Assert.That(fresh.JumpPressed && fresh.Interact && fresh.InteractHeld, Is.True);
                Assert.That(fresh.Move.x, Is.GreaterThan(.5f));
            }
            finally { Object.DestroyImmediate(root); InputSystem.RemoveDevice(screen); }
        }

        private static void Focus(SliceTouchControls2D touch, bool focused) =>
            typeof(SliceTouchControls2D).GetMethod("OnApplicationFocus", Private).Invoke(touch, new object[] { focused });

        [TestCase("disable")]
        [TestCase("focus")]
        [TestCase("bind")]
        public void MouseContactNeedsReleaseAfterProducerCancellation(string cancellation)
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var root = new GameObject("Mouse cancellation");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                var session = root.AddComponent<VerticalSliceSession2D>();
                session.enabled = false;
                typeof(VerticalSliceSession2D).GetProperty("Input").SetValue(session, input);
                typeof(VerticalSliceSession2D).GetProperty("IsRunning").SetValue(session, true);
                var touch = root.AddComponent<SliceTouchControls2D>();
                Assert.That(touch.TryBind(session, null), Is.True);
                Tick(touch, "Layout");
                var buttons = (Rect[])typeof(SliceTouchControls2D).GetField("buttons", Private).GetValue(touch);
                Vector2 point = ScreenPoint(buttons[1].center);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                InputSystem.Update(); Tick(touch);
                Assert.That(Submitted(input).Attack, Is.True);
                Tick(input, "ResetHeld");
                if (cancellation == "disable") { touch.enabled = false; touch.enabled = true; }
                else if (cancellation == "focus")
                {
                    typeof(SliceTouchControls2D).GetMethod("OnApplicationFocus", Private).Invoke(touch, new object[] { false });
                    typeof(SliceTouchControls2D).GetMethod("OnApplicationFocus", Private).Invoke(touch, new object[] { true });
                }
                else Assert.That(touch.TryBind(session, null), Is.True);
                Tick(touch);
                AssertNeutral(Submitted(input));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                InputSystem.Update(); Tick(touch); Tick(input, "ResetHeld");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                InputSystem.Update(); Tick(touch);
                Assert.That(Submitted(input).Attack, Is.True);
            }
            finally { Object.DestroyImmediate(root); InputSystem.RemoveDevice(mouse); }
        }

        private static void AssertNeutral(SliceInputFrame frame)
        {
            Assert.That(frame.Move, Is.EqualTo(Vector2.zero));
            Assert.That(frame.JumpPressed || frame.Interact || frame.InteractHeld || frame.Attack || frame.Dash || frame.Parry || frame.Switch || frame.Special || frame.FoodSelf || frame.FoodFan, Is.False);
        }
        private static Vector2 ScreenPoint(Vector2 gui) => new Vector2(gui.x, Screen.height - gui.y);
        private static void Contact(Touchscreen screen, int id, Vector2 point, UnityEngine.InputSystem.TouchPhase phase) =>
            InputSystem.QueueStateEvent(screen, new TouchState { touchId = id, position = point, phase = phase });
        private static void Key(Keyboard keyboard, params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); InputSystem.Update(); }
        private static void Tick(object target, string method = "Update") => target.GetType().GetMethod(method, Private).Invoke(target, null);
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
        private static SliceInputFrame Submitted(SlicePlayerInput2D input) => (SliceInputFrame)typeof(SlicePlayerInput2D).GetField("submitted", Private).GetValue(input);
    }
}
