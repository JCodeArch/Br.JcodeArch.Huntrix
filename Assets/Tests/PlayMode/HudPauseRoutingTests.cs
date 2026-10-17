using System.Collections;
using System.Reflection;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class HudPauseRoutingTests
    {
        [UnityTest]
        public IEnumerator RemappedPauseClosesWhileGameplayInputDisabledAndRestoresDefault()
        {
            float previousScale = Time.timeScale;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var root = new GameObject("Pause routing regression");
            try
            {
                var preferences = root.AddComponent<AccessibilityPreferences2D>();
                Assert.That(preferences.TryRemap(GameplayControl.Pause, "<Keyboard>/p"), Is.True);
                var input = root.AddComponent<SlicePlayerInput2D>();
                input.enabled = false;
                var session = root.AddComponent<VerticalSliceSession2D>();
                session.enabled = false; // isolate input ownership from full gameplay startup
                typeof(VerticalSliceSession2D).GetProperty("Input").SetValue(session, input);
                var hud = root.AddComponent<SliceHud2D>();
                typeof(SliceHud2D).GetField("preferences", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hud, preferences);
                typeof(SliceHud2D).GetField("session", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hud, session);
                yield return null; // apply the remap before queuing input
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
                yield return null;
                Assert.That(IsOpen(hud), Is.False, "Escape must stop pausing after remap");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.P));
                yield return null;
                Assert.That(IsOpen(hud), Is.True);
                Assert.That(Time.timeScale, Is.Zero);
                Assert.That(input.enabled, Is.False);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.P));
                yield return null;
                Assert.That(IsOpen(hud), Is.False, "Paused gameplay input must not block closing options");
                Assert.That(input.enabled, Is.False, "Previously disabled gameplay stays disabled");
                preferences.ClearRemap(GameplayControl.Pause);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
                yield return null;
                Assert.That(IsOpen(hud), Is.True);
                Assert.That((bool)typeof(SliceHud2D).GetField("inputOwned", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hud), Is.True);
                hud.enabled = false;
                Assert.That(IsOpen(hud), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
                InputSystem.RemoveDevice(keyboard);
                var service = Object.FindAnyObjectByType<HitStopService>();
                if (service != null) Object.DestroyImmediate(service.gameObject);
                Time.timeScale = previousScale;
            }
        }
        [UnityTest]
        public IEnumerator HudGamepadStartOpensAndClosesAcrossDisableEnable()
        {
            float previousScale = Time.timeScale;
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var root = new GameObject("HUD gamepad lifecycle");
            try
            {
                var hud = root.AddComponent<SliceHud2D>();
                yield return null;
                InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.Start));
                yield return null;
                Assert.That(IsOpen(hud), Is.True);
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                yield return null;
                InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.Start));
                yield return null;
                Assert.That(IsOpen(hud), Is.False);
                hud.enabled = false;
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                yield return null;
                hud.enabled = true;
                yield return null;
                InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.Start));
                yield return null;
                Assert.That(IsOpen(hud), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
                InputSystem.RemoveDevice(gamepad);
                var service = Object.FindAnyObjectByType<HitStopService>();
                if (service != null) Object.DestroyImmediate(service.gameObject);
                Time.timeScale = previousScale;
            }
        }
        [Test]
        public void GamepadStartPausesAndRightStickFeedsFanWithoutConflict()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var root = new GameObject("Gamepad routes");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                var read = typeof(SlicePlayerInput2D).GetMethod("ReadPhysical", BindingFlags.Instance | BindingFlags.NonPublic);
                InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.Start));
                InputSystem.Update();
                var frame = (SliceInputFrame)read.Invoke(input, null);
                Assert.That(frame.Pause, Is.True);
                Assert.That(frame.FoodFan, Is.False);
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.RightStick));
                InputSystem.Update();
                frame = (SliceInputFrame)read.Invoke(input, null);
                Assert.That(frame.Pause, Is.False);
                Assert.That(frame.FoodFan, Is.True);
            }
            finally { Object.DestroyImmediate(root); InputSystem.RemoveDevice(gamepad); }
        }
        private static bool IsOpen(SliceHud2D hud) => (bool)typeof(SliceHud2D).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hud);
    }
}
