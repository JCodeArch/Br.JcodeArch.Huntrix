using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using HuntrX.Presentation.UI;
using HuntrX.Presentation.UI.Views;

namespace HuntrX.Tests.PlayMode
{
    public sealed class UiLayoutPlayModeTests
    {
        private GameObject rootGo;
        private Canvas canvas;
        private CanvasScaler scaler;
        private SliceSafeAreaFitter safeAreaFitter;
        private SliceUiController2D controller;

        [SetUp]
        public void SetUp()
        {
            rootGo = new GameObject("TestUiRoot");
            canvas = rootGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            scaler = rootGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SliceUiTokens.ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            rootGo.AddComponent<GraphicRaycaster>();

            // Safe area panel
            var safeAreaGo = new GameObject("SafeAreaContainer", typeof(RectTransform));
            safeAreaGo.transform.SetParent(rootGo.transform, false);
            safeAreaFitter = safeAreaGo.AddComponent<SliceSafeAreaFitter>();

            // Views
            var mainMenuGo = new GameObject("MainMenuPanel", typeof(RectTransform));
            mainMenuGo.transform.SetParent(safeAreaGo.transform, false);
            var mainMenuView = mainMenuGo.AddComponent<MainMenuUiView>();

            var teamProfileGo = new GameObject("TeamProfilePanel", typeof(RectTransform));
            teamProfileGo.transform.SetParent(safeAreaGo.transform, false);
            var teamProfileView = teamProfileGo.AddComponent<TeamProfileUiView>();

            var hudGo = new GameObject("HudPanel", typeof(RectTransform));
            hudGo.transform.SetParent(safeAreaGo.transform, false);
            var hudView = hudGo.AddComponent<MobileHudUiView>();

            var optionsGo = new GameObject("OptionsPanel", typeof(RectTransform));
            optionsGo.transform.SetParent(safeAreaGo.transform, false);
            var optionsView = optionsGo.AddComponent<OptionsUiView>();

            var resultsGo = new GameObject("ResultsPanel", typeof(RectTransform));
            resultsGo.transform.SetParent(safeAreaGo.transform, false);
            var resultsView = resultsGo.AddComponent<ResultsReplayUiView>();

            // Controller
            controller = rootGo.AddComponent<SliceUiController2D>();
            controller.Initialize(
                rootGo.GetComponent<SliceUiFlow2D>(),
                mainMenuView,
                teamProfileView,
                hudView,
                optionsView,
                resultsView
            );
        }

        [TearDown]
        public void TearDown()
        {
            if (rootGo != null)
            {
                Object.Destroy(rootGo);
            }
        }

        [UnityTest]
        public IEnumerator SafeAreaFitter_AppliesNormalizedAnchorsInRuntime()
        {
            yield return null;

            safeAreaFitter.ApplySafeArea();
            var rect = safeAreaFitter.GetComponent<RectTransform>();

            Assert.That(rect.anchorMin.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rect.anchorMin.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rect.anchorMax.x, Is.LessThanOrEqualTo(1f));
            Assert.That(rect.anchorMax.y, Is.LessThanOrEqualTo(1f));
        }

        [UnityTest]
        public IEnumerator UiFlow_TransitionsCorrectlyAcrossScreensInRuntime()
        {
            yield return null;

            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.MainMenu));

            // Move to profile selection
            bool navigatedToProfile = controller.Flow.TransitionTo(UiScreen.TeamProfile);
            Assert.That(navigatedToProfile, Is.True);
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.TeamProfile));

            yield return null;

            // Move to Mobile HUD
            bool navigatedToHud = controller.Flow.TransitionTo(UiScreen.MobileHud);
            Assert.That(navigatedToHud, Is.True);
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.MobileHud));

            // Render sample snapshot
            var state = SliceUiState2D.CreateDefault();
            state.HealthCurrent = 75;
            state.ActiveCharacter = "Mira";
            controller.MobileHudView.RenderState(state);

            yield return null;

            // Pause / Options
            bool navigatedToOptions = controller.Flow.TransitionTo(UiScreen.Options);
            Assert.That(navigatedToOptions, Is.True);

            // Go back to Hud
            bool backed = controller.Flow.TryGoBack();
            Assert.That(backed, Is.True);
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.MobileHud));

            // Finish to ResultsReplay
            bool completed = controller.Flow.TransitionTo(UiScreen.ResultsReplay);
            Assert.That(completed, Is.True);
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.ResultsReplay));
        }
    }
}
