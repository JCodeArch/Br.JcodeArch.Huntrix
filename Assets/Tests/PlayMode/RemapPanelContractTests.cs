using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class RemapPanelContractTests
    {
        private GameObject root;
        private RemapPanelUiView view;
        private AccessibilityPreferences2D preferences;
        private Dropdown actionDropdown;
        private Text currentBindingText;
        private InputField keyInputField;
        private Button applyButton;
        private Button restoreButton;
        private Text statusText;
        private Text helpText;
        private Button backButton;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("RemapPanel", typeof(RectTransform));
            actionDropdown = Add<Dropdown>("actionDropdown");
            currentBindingText = Add<Text>("currentBindingText");
            keyInputField = Add<InputField>("keyInputField");
            applyButton = Add<Button>("applyButton");
            restoreButton = Add<Button>("restoreButton");
            statusText = Add<Text>("statusText");
            helpText = Add<Text>("helpText");
            backButton = Add<Button>("backButton");
            Add<Text>("titleText");

            view = root.AddComponent<RemapPanelUiView>();

            var prefObj = new GameObject("Preferences");
            preferences = prefObj.AddComponent<AccessibilityPreferences2D>();
        }

        [TearDown]
        public void TearDown()
        {
            if (preferences != null) Object.DestroyImmediate(preferences.gameObject);
            if (root != null) Object.DestroyImmediate(root);
        }

        private T Add<T>(string name) where T : Component
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<T>();
        }

        [Test]
        public void InitialStatePresentsCorrectContractDefaults()
        {
            view.BindPreferences(preferences);
            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.InitialStatusMessage));
            Assert.That(helpText.text, Is.EqualTo(RemapPanelUiView.DefaultHelpMessage));
            Assert.That(keyInputField.characterLimit, Is.EqualTo(24));
            Assert.That(restoreButton.interactable, Is.True);
        }

        [Test]
        public void UnavailablePreferencesReflectsUnavailableState()
        {
            view.BindPreferences(null);
            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.UnavailableMessage));
            Assert.That(view.CurrentBindingDisplay, Is.EqualTo("Indisponível"));
            Assert.That(applyButton.interactable, Is.False);
            Assert.That(restoreButton.interactable, Is.False);
        }

        [Test]
        public void EmptyInputFailsWithEmptyFieldStatus()
        {
            view.BindPreferences(preferences);
            view.SetInputText("");
            view.ApplyRemap();
            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.EmptyFieldMessage));
        }

        [Test]
        public void InvalidKeyFailsValidationAndPreservesExistingBinding()
        {
            view.BindPreferences(preferences);
            view.SelectControl(GameplayControl.Jump);
            string originalBinding = view.CurrentBindingDisplay;

            view.SetInputText("invalid_key_name!@#");
            view.ApplyRemap();

            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.InvalidKeyMessage));
            Assert.That(view.CurrentBindingDisplay, Is.EqualTo(originalBinding));
            Assert.That(preferences.KeyboardBinding(GameplayControl.Jump), Is.Null);
        }

        [Test]
        public void ValidRemapUpdatesPreferencesAndBindingDisplay()
        {
            view.BindPreferences(preferences);
            view.SelectControl(GameplayControl.Attack);

            int eventCount = 0;
            GameplayControl emittedControl = GameplayControl.Jump;
            string emittedKey = null;

            view.RemapApplied += (c, k) =>
            {
                eventCount++;
                emittedControl = c;
                emittedKey = k;
            };

            view.SetInputText("u");
            view.ApplyRemap();

            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.AppliedMessage));
            Assert.That(preferences.KeyboardBinding(GameplayControl.Attack), Is.EqualTo("<Keyboard>/u"));
            Assert.That(view.CurrentBindingDisplay, Is.EqualTo("u"));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(emittedControl, Is.EqualTo(GameplayControl.Attack));
            Assert.That(emittedKey, Is.EqualTo("u"));
        }

        [Test]
        public void RestoringDefaultClearsCustomBindingOnlyForSelectedAction()
        {
            view.BindPreferences(preferences);
            preferences.TryRemap(GameplayControl.Attack, "<Keyboard>/u");
            preferences.TryRemap(GameplayControl.Dash, "<Keyboard>/space");

            view.SelectControl(GameplayControl.Attack);
            int restoreCount = 0;
            view.RemapRestored += _ => restoreCount++;

            view.RestoreDefault();

            Assert.That(view.Status, Is.EqualTo(RemapPanelUiView.RestoredMessage));
            Assert.That(preferences.KeyboardBinding(GameplayControl.Attack), Is.Null);
            Assert.That(preferences.KeyboardBinding(GameplayControl.Dash), Is.EqualTo("<Keyboard>/space"));
            Assert.That(view.CurrentBindingDisplay, Is.EqualTo(RemapPanelUiView.GetDefaultBindingDisplay(GameplayControl.Attack)));
            Assert.That(restoreCount, Is.EqualTo(1));
        }

        [Test]
        public void BackButtonRaisesBackRequestedEvent()
        {
            int backCount = 0;
            view.BackRequested += () => backCount++;
            backButton.onClick.Invoke();
            Assert.That(backCount, Is.EqualTo(1));
        }
    }
}
