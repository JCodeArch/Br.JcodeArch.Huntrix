using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class OptionsUiViewContractTests
    {
        private GameObject root;
        private OptionsUiView view;
        private Toggle jump;
        private Toggle rescue;
        private Text languageStatus;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Options", typeof(RectTransform));
            jump = Add<Toggle>("toggleJumpToggle");
            rescue = Add<Toggle>("toggleRescueToggle");
            languageStatus = Add<Text>("languageStatusText");
            Add<Button>("ptLanguageButton");
            Add<Button>("enLanguageButton");
            view = root.AddComponent<OptionsUiView>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        private T Add<T>(string name) where T : Component
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<T>();
        }

        [Test]
        public void HydrationPreservesMixedJumpAndRescueWithoutEmittingCommands()
        {
            int commands = 0;
            view.ToggleJumpChanged += _ => commands++;
            view.ToggleRescueChanged += _ => commands++;
            view.LanguageChanged += _ => commands++;
            view.SetPreferences(true, true, true, true, false, "EN");
            Assert.That(jump.isOn, Is.True);
            Assert.That(rescue.isOn, Is.False);
            Assert.That(view.CurrentLanguage, Is.EqualTo("EN"));
            Assert.That(languageStatus.text, Is.EqualTo("Idioma selecionado: English"));
            Assert.That(commands, Is.Zero);
        }

        [Test]
        public void ChangingJumpDoesNotIssueRescueOrCombinedCommands()
        {
            int jumpCommands = 0, rescueCommands = 0, combinedCommands = 0;
            view.SetPreferences(true, true, true, false, false, "PT");
            view.ToggleJumpChanged += _ => jumpCommands++;
            view.ToggleRescueChanged += _ => rescueCommands++;
            view.ToggleJumpRescueChanged += _ => combinedCommands++;
            jump.isOn = true;
            Assert.That(jumpCommands, Is.EqualTo(1));
            Assert.That(rescueCommands, Is.Zero);
            Assert.That(combinedCommands, Is.Zero);
            Assert.That(rescue.isOn, Is.False);
        }

        [Test]
        public void LegacyHydrationStillSetsBothToggleModes()
        {
            view.SetPreferences(true, true, true, true, "PT");
            Assert.That(jump.isOn, Is.True);
            Assert.That(rescue.isOn, Is.True);
        }

        [Test]
        public void LanguageRejectsUnsupportedValuesAndEmitsOnlyAnActualSelectionChange()
        {
            int commands = 0;
            view.LanguageChanged += _ => commands++;
            view.SetLanguage("FR");
            view.SetLanguage(null);
            view.SetLanguage("PT");
            Assert.That(view.CurrentLanguage, Is.EqualTo("PT"));
            Assert.That(commands, Is.Zero);
            view.SetLanguage("EN");
            view.SetLanguage("EN");
            Assert.That(commands, Is.EqualTo(1));
            Assert.That(languageStatus.text, Is.EqualTo("Idioma selecionado: English"));
        }
    }
}
