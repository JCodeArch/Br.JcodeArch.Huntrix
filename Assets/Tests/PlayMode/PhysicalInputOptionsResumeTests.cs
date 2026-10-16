using System.Reflection;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace HuntrX.Tests.PlayMode
{
    public sealed class PhysicalInputOptionsResumeTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void OptionsResumeRequiresReleaseForHeldOrMenuStartedContacts(bool useGamepad, bool startInMenu)
        {
            float priorScale = Time.timeScale;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var root = new GameObject("Physical options resume regression");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                var preferences = root.AddComponent<AccessibilityPreferences2D>();
                Assert.That(preferences.TrySetToggle(GameplayControl.Interact, true), Is.True);
                Assert.That(input.BindPreferences(preferences), Is.True);
                var session = root.AddComponent<VerticalSliceSession2D>();
                session.enabled = false; // Exercise the real pause owner without starting gameplay.
                typeof(VerticalSliceSession2D).GetProperty("Input").SetValue(session, input);
                var hud = root.AddComponent<SliceHud2D>();
                typeof(SliceHud2D).GetField("session", Private).SetValue(hud, session);
                State(keyboard, gamepad, useGamepad, !startInMenu, false);
                if (!startInMenu) AssertActive(Read(input));

                State(keyboard, gamepad, useGamepad, !startInMenu, true);
                TickHud(hud);
                Assert.That(hud.IsOptionsOpen, Is.True);
                Assert.That(input.enabled, Is.False);
                Assert.That(Time.timeScale, Is.Zero);
                State(keyboard, gamepad, useGamepad, true, false);
                TickHud(hud);
                State(keyboard, gamepad, useGamepad, true, true);
                TickHud(hud);
                Assert.That(hud.IsOptionsOpen, Is.False);
                Assert.That(input.enabled, Is.True);
                AssertNeutral(Read(input));
                State(keyboard, gamepad, useGamepad, true, false);
                AssertNeutral(Read(input));

                State(keyboard, gamepad, useGamepad, false, false);
                AssertNeutral(Read(input));
                State(keyboard, gamepad, useGamepad, true, false);
                AssertActive(Read(input));
            }
            finally
            {
                Object.DestroyImmediate(root);
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(gamepad);
                var service = Object.FindFirstObjectByType<HuntrX.Gameplay.Combat.HitStopService>();
                if (service != null) Object.DestroyImmediate(service.gameObject);
                Time.timeScale = priorScale;
            }
        }

        [Test]
        public void FirstEnableAcceptsInputWithoutAnEarlierNeutralSample()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var root = new GameObject("Initial input regression");
            try
            {
                var input = root.AddComponent<SlicePlayerInput2D>();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.E, Key.Space));
                InputSystem.Update();
                AssertActive(Read(input));
            }
            finally { Object.DestroyImmediate(root); InputSystem.RemoveDevice(keyboard); }
        }

        private static void State(Keyboard keyboard, Gamepad gamepad, bool useGamepad, bool held, bool pause)
        {
            if (useGamepad)
            {
                var state = new GamepadState { leftStick = held ? Vector2.right : Vector2.zero };
                if (held) state = state.WithButton(GamepadButton.North).WithButton(GamepadButton.South);
                if (pause) state = state.WithButton(GamepadButton.Start);
                InputSystem.QueueStateEvent(gamepad, state);
            }
            else
            {
                var keys = held ? (pause ? new[] { Key.D, Key.E, Key.Space, Key.Escape } : new[] { Key.D, Key.E, Key.Space })
                    : (pause ? new[] { Key.Escape } : new Key[0]);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            }
            InputSystem.Update();
        }
        private static SliceInputFrame Read(SlicePlayerInput2D input) =>
            (SliceInputFrame)typeof(SlicePlayerInput2D).GetMethod("ReadPhysical", Private).Invoke(input, null);
        private static void TickHud(SliceHud2D hud) => typeof(SliceHud2D).GetMethod("Update", Private).Invoke(hud, null);
        private static void AssertNeutral(SliceInputFrame frame)
        {
            Assert.That(frame.Move, Is.EqualTo(Vector2.zero));
            Assert.That(frame.Interact || frame.InteractHeld || frame.JumpPressed, Is.False);
        }
        private static void AssertActive(SliceInputFrame frame)
        {
            Assert.That(frame.Move.x, Is.GreaterThan(.5f));
            Assert.That(frame.Interact && frame.InteractHeld && frame.JumpPressed, Is.True);
        }
    }
}
