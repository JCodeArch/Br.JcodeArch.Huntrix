using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using HuntrX.Presentation.UI;
using HuntrX.Presentation.UI.Views;

namespace HuntrX.Tests.EditMode
{
    [TestFixture]
    public sealed class UiLayoutTests
    {
        [Test]
        public void SliceUiTokens_MatchFigmaSpecifications()
        {
            Assert.That(SliceUiTokens.ReferenceResolution, Is.EqualTo(new Vector2(1280f, 720f)));
            Assert.That(SliceUiTokens.MinimumTouchTargetSize, Is.EqualTo(64f));

            // Verify color conversions
            Color primary = SliceUiTokens.PrimaryAction;
            Assert.That(primary.r, Is.GreaterThan(0.1f));
            Assert.That(primary.b, Is.GreaterThan(0.7f));

            Color bg = SliceUiTokens.Background;
            Assert.That(bg.r, Is.GreaterThan(0.9f));
        }

        [Test]
        public void SliceSafeAreaFitter_CalculatesNormalizedAnchorsCorrectly()
        {
            // Case 1: Standard screen without notch
            Vector2 screen = new Vector2(1920f, 1080f);
            Rect fullSafe = new Rect(0f, 0f, 1920f, 1080f);
            var (min, max) = SliceSafeAreaFitter.CalculateNormalizedSafeArea(fullSafe, screen);

            Assert.That(min, Is.EqualTo(Vector2.zero));
            Assert.That(max, Is.EqualTo(Vector2.one));

            // Case 2: Asymmetric left notch (e.g., 100px inset on left)
            Rect notchedSafe = new Rect(100f, 0f, 1820f, 1080f);
            var (notchMin, notchMax) = SliceSafeAreaFitter.CalculateNormalizedSafeArea(notchedSafe, screen);

            Assert.That(notchMin.x, Is.EqualTo(100f / 1920f).Within(0.0001f));
            Assert.That(notchMin.y, Is.EqualTo(0f));
            Assert.That(notchMax.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(notchMax.y, Is.EqualTo(1f));

            // Case 3: Zero or invalid screen size gracefully returns full anchors
            var (fallbackMin, fallbackMax) = SliceSafeAreaFitter.CalculateNormalizedSafeArea(Rect.zero, Vector2.zero);
            Assert.That(fallbackMin, Is.EqualTo(Vector2.zero));
            Assert.That(fallbackMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void SliceUiState2D_DefaultValuesAndEquality()
        {
            var defaultState = SliceUiState2D.CreateDefault();

            Assert.That(defaultState.HealthCurrent, Is.EqualTo(100));
            Assert.That(defaultState.HealthMaximum, Is.EqualTo(100));
            Assert.That(defaultState.HonmoonCurrent, Is.EqualTo(100));
            Assert.That(defaultState.ActiveCharacter, Is.EqualTo("Rumi"));
            Assert.That(defaultState.SpecialReady, Is.False);

            var clonedState = defaultState;
            Assert.That(clonedState, Is.EqualTo(defaultState));
            Assert.That(clonedState == defaultState, Is.True);

            clonedState.HealthCurrent = 50;
            Assert.That(clonedState != defaultState, Is.True);
        }

        [Test]
        public void SliceUiFlow2D_ManagesTransitionsAndHistoryCorrectly()
        {
            var go = new GameObject("TestUiFlow");
            try
            {
                var flow = go.AddComponent<SliceUiFlow2D>();
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.MainMenu));

                // Cannot jump straight from MainMenu to ResultsReplay
                Assert.That(flow.CanTransitionTo(UiScreen.ResultsReplay), Is.False);
                Assert.That(flow.TransitionTo(UiScreen.ResultsReplay), Is.False);

                // MainMenu -> TeamProfile is valid
                Assert.That(flow.CanTransitionTo(UiScreen.TeamProfile), Is.True);
                Assert.That(flow.TransitionTo(UiScreen.TeamProfile), Is.True);
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.TeamProfile));
                Assert.That(flow.HistoryCount, Is.EqualTo(1));

                // TeamProfile -> MobileHud is valid
                Assert.That(flow.TransitionTo(UiScreen.MobileHud), Is.True);
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.MobileHud));

                // MobileHud -> Options (Pause)
                Assert.That(flow.TransitionTo(UiScreen.Options), Is.True);
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.Options));

                // Back returns to MobileHud
                Assert.That(flow.TryGoBack(), Is.True);
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.MobileHud));

                // MobileHud -> ResultsReplay is valid
                Assert.That(flow.TransitionTo(UiScreen.ResultsReplay), Is.True);
                Assert.That(flow.CurrentScreen, Is.EqualTo(UiScreen.ResultsReplay));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MainMenuUiView_ContinueButtonIsStrictlyUnavailable()
        {
            var go = new GameObject("TestMainMenu");
            try
            {
                var view = go.AddComponent<MainMenuUiView>();
                Assert.That(view.IsContinueInteractable, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
