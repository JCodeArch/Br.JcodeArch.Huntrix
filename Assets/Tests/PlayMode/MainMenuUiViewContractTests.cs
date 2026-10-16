using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class MainMenuUiViewContractTests
    {
        private GameObject root;
        private MainMenuUiView view;
        private Button startBtn;
        private Button continueBtn;
        private Button optionsBtn;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("MainMenu", typeof(RectTransform));
            startBtn = Add<Button>("startButton");
            continueBtn = Add<Button>("continueButton");
            optionsBtn = Add<Button>("optionsButton");
            continueBtn.interactable = true;
            view = root.AddComponent<MainMenuUiView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        private T Add<T>(string name) where T : Component
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<T>();
        }

        [Test]
        public void AwakeInitializesAndKeepsContinueButtonDisabled()
        {
            Assert.That(view.IsContinueInteractable, Is.False);
            Assert.That(continueBtn.interactable, Is.False);
        }

        [Test]
        public void ClickingStartEmitsStartRequested()
        {
            int count = 0;
            view.StartRequested += () => count++;
            startBtn.onClick.Invoke();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ClickingOptionsEmitsOptionsRequested()
        {
            int count = 0;
            view.OptionsRequested += () => count++;
            optionsBtn.onClick.Invoke();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void SetVisibleControlsGameObjectActive()
        {
            view.SetVisible(false);
            Assert.That(view.gameObject.activeSelf, Is.False);
            view.SetVisible(true);
            Assert.That(view.gameObject.activeSelf, Is.True);
        }
    }
}
