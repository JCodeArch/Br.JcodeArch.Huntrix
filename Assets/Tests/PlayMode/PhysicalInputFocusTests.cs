using System.Collections;
using System.Collections.Generic;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class PhysicalInputFocusTests
    {
        private Scene slice;
        private Keyboard keyboard;
        private Gamepad gamepad;
        private readonly List<SliceInputFrame> observed = new List<SliceInputFrame>();
        private SlicePlayerInput2D input;
        [UnitySetUp] public IEnumerator Load()
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            gamepad = InputSystem.AddDevice<Gamepad>();
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
#endif
            slice = SceneManager.GetActiveScene();
            yield return null;
            input = Object.FindFirstObjectByType<VerticalSliceSession2D>().Input;
            input.ActionsRequested += observed.Add;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (input != null) input.ActionsRequested -= observed.Add;
            SceneManager.SetActiveScene(SceneManager.CreateScene("PhysicalFocusCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
            InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(gamepad);
            observed.Clear();
        }
        [UnityTest] public IEnumerator KeyboardToggleIsReleasedOnFocusLossAndRequiresFreshContact()
        {
            var preferences = Object.FindFirstObjectByType<AccessibilityPreferences2D>();
            Assert.That(preferences.TrySetToggle(GameplayControl.Interact, true), Is.True);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            Assert.That(observed.Exists(frame => frame.Interact && frame.InteractHeld), Is.True);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.True);
            observed.Clear();
            input.SendMessage("OnApplicationFocus", false, SendMessageOptions.DontRequireReceiver);
            Assert.That(observed.Count, Is.EqualTo(1));
            Assert.That(observed[0].InteractReleased, Is.True);
            Assert.That(observed[0].JumpReleased, Is.True);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            input.SubmitInput(new SliceInputFrame { Attack = true });
            yield return null;
            Assert.That(observed.Count, Is.EqualTo(1), "Unfocused input must not dispatch commands.");
            input.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver);
            yield return null;
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.False);
            Assert.That(observed[observed.Count - 1].Attack, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            Assert.That(observed[observed.Count - 1].Interact, Is.True);
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.True);
        }
        [UnityTest] public IEnumerator HeldGamepadButtonAndStickMustReturnToNeutralAfterFocusLoss()
        {
            InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = Vector2.right }
                .WithButton(GamepadButton.North));
            yield return null;
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.True);
            input.SendMessage("OnApplicationFocus", false, SendMessageOptions.DontRequireReceiver);
            input.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver);
            yield return null;
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.False);
            Assert.That(observed[observed.Count - 1].Move, Is.EqualTo(Vector2.zero));
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;
            InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = Vector2.right }
                .WithButton(GamepadButton.North));
            yield return null;
            Assert.That(observed[observed.Count - 1].Interact, Is.True);
            Assert.That(observed[observed.Count - 1].InteractHeld, Is.True);
            Assert.That(observed[observed.Count - 1].Move.x, Is.GreaterThan(.5f));
        }
    }
}
