using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class TeamProfileUiViewContractTests
    {
        private GameObject root;
        private TeamProfileUiView view;
        private Button rumiBtn;
        private Button miraBtn;
        private Button zoeyBtn;
        private Button p4To5Btn;
        private Button p6To8Btn;
        private Button p9To12Btn;
        private Button p13To15Btn;
        private Button p16PlusBtn;
        private Button confirmBtn;
        private Button backBtn;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TeamProfile", typeof(RectTransform));
            rumiBtn = Add<Button>("rumiButton");
            miraBtn = Add<Button>("miraButton");
            zoeyBtn = Add<Button>("zoeyButton");
            p4To5Btn = Add<Button>("profile4To5Button");
            p6To8Btn = Add<Button>("profile6To8Button");
            p9To12Btn = Add<Button>("profile9To12Button");
            p13To15Btn = Add<Button>("profile13To15Button");
            p16PlusBtn = Add<Button>("profile16PlusButton");
            confirmBtn = Add<Button>("confirmButton");
            backBtn = Add<Button>("backButton");
            view = root.AddComponent<TeamProfileUiView>();
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
        public void InitialSelectionDefaultsToRumiAndNineToTwelve()
        {
            Assert.That(view.SelectedCharacter, Is.EqualTo("Rumi"));
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("9-12"));
        }

        [Test]
        public void ClickingCharacterButtonsUpdatesSelectionAndEmitsEvent()
        {
            string selected = null;
            view.CharacterSelected += charName => selected = charName;

            miraBtn.onClick.Invoke();
            Assert.That(view.SelectedCharacter, Is.EqualTo("Mira"));
            Assert.That(selected, Is.EqualTo("Mira"));

            zoeyBtn.onClick.Invoke();
            Assert.That(view.SelectedCharacter, Is.EqualTo("Zoey"));
            Assert.That(selected, Is.EqualTo("Zoey"));

            rumiBtn.onClick.Invoke();
            Assert.That(view.SelectedCharacter, Is.EqualTo("Rumi"));
            Assert.That(selected, Is.EqualTo("Rumi"));
        }

        [Test]
        public void ClickingProfileButtonsUpdatesSelectionAndEmitsEvent()
        {
            string profile = null;
            view.AgeProfileSelected += p => profile = p;

            p4To5Btn.onClick.Invoke();
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("4-5"));
            Assert.That(profile, Is.EqualTo("4-5"));

            p6To8Btn.onClick.Invoke();
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("6-8"));
            Assert.That(profile, Is.EqualTo("6-8"));

            p13To15Btn.onClick.Invoke();
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("13-15"));
            Assert.That(profile, Is.EqualTo("13-15"));

            p16PlusBtn.onClick.Invoke();
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("16+"));
            Assert.That(profile, Is.EqualTo("16+"));

            p9To12Btn.onClick.Invoke();
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("9-12"));
            Assert.That(profile, Is.EqualTo("9-12"));
        }

        [Test]
        public void ClickingConfirmAndBackEmitsExpectedEvents()
        {
            int confirmed = 0;
            int backRequested = 0;
            view.Confirmed += () => confirmed++;
            view.BackRequested += () => backRequested++;

            confirmBtn.onClick.Invoke();
            Assert.That(confirmed, Is.EqualTo(1));

            backBtn.onClick.Invoke();
            Assert.That(backRequested, Is.EqualTo(1));
        }

        [Test]
        public void SetSelectionDirectlySetsValuesWithoutEmittingEvents()
        {
            int charEvents = 0;
            int profileEvents = 0;
            view.CharacterSelected += _ => charEvents++;
            view.AgeProfileSelected += _ => profileEvents++;

            view.SetSelection("Zoey", "16+");

            Assert.That(view.SelectedCharacter, Is.EqualTo("Zoey"));
            Assert.That(view.SelectedAgeProfile, Is.EqualTo("16+"));
            Assert.That(charEvents, Is.Zero);
            Assert.That(profileEvents, Is.Zero);
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
