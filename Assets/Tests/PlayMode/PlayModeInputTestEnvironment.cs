using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HuntrX.Tests.PlayMode
{
    /// <summary>
    /// Keeps injected Input System events deterministic in unattended (batchmode/CI) runs.
    /// Without window focus the Input System discards or resets device state, which makes
    /// queued keyboard/gamepad/touch events invisible to the tests. A temporary in-memory
    /// settings object is used so no project asset is modified; the original is restored.
    /// </summary>
    [SetUpFixture]
    public sealed class PlayModeInputTestEnvironment
    {
        private InputSettings previousSettings;
        private InputSettings testSettings;

        [OneTimeSetUp]
        public void ConfigureInput()
        {
            previousSettings = InputSystem.settings;
            testSettings = ScriptableObject.CreateInstance<InputSettings>();
            testSettings.hideFlags = HideFlags.HideAndDontSave;
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings = testSettings;
        }

        [OneTimeTearDown]
        public void RestoreInput()
        {
            if (previousSettings != null)
            {
                InputSystem.settings = previousSettings;
            }

            if (testSettings != null)
            {
                Object.DestroyImmediate(testSettings);
            }
        }
    }
}
