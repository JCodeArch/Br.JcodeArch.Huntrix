using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HuntrX.Presentation.UI;
using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    // These tests intentionally open the shipped scene, not test-created empty views.
    public sealed class UiLayoutScenePlayModeTests
    {
        private SliceUiController2D controller;

        [UnitySetUp]
        public IEnumerator LoadAuthoredScene()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/UiLayoutLab.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("UiLayoutLab", LoadSceneMode.Single);
#endif
            yield return null;
            controller = Object.FindFirstObjectByType<SliceUiController2D>();
            Assert.That(controller, Is.Not.Null, "The actual lab scene must compose a UI controller.");
            Assert.That(EventSystem.current, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator UnloadLab()
        {
            // Avoid leaking scene UI/EventSystem into unrelated PlayMode suites.
            Scene lab = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("UiLayoutTestCleanup"));
            yield return SceneManager.UnloadSceneAsync(lab);
        }

        [UnityTest]
        public IEnumerator SceneBindsEveryViewControlAndContainsLegibleLabels()
        {
            var viewTypes = new[] { typeof(MainMenuUiView), typeof(TeamProfileUiView),
                typeof(MobileHudUiView), typeof(OptionsUiView), typeof(ResultsReplayUiView) };
            foreach (var type in viewTypes)
            {
                Component view = controller.GetComponentInChildren(type, true);
                Assert.That(view, Is.Not.Null, type.Name);
                foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!field.IsDefined(typeof(SerializeField), true) ||
                        !typeof(Object).IsAssignableFrom(field.FieldType)) continue;
                    Assert.That(field.GetValue(view) as Object, Is.Not.Null, type.Name + "." + field.Name);
                }
                foreach (Button button in view.GetComponentsInChildren<Button>(true))
                    Assert.That(button.GetComponentInChildren<Text>(true)?.text, Is.Not.Null.And.Not.Empty,
                        "Every visible button needs a readable label: " + button.name);
            }
            Assert.That(Find<Button>("continueButton").interactable, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PointerClicksNavigateAllFiveScreensAndReturnToCaller()
        {
            AssertScreen(UiScreen.MainMenu);
            Click("optionsButton");
            AssertScreen(UiScreen.Options);
            Click("closeButton");
            AssertScreen(UiScreen.MainMenu);
            Click("startButton");
            AssertScreen(UiScreen.TeamProfile);
            Click("miraButton");
            Click("profile6To8Button");
            var profile = controller.GetComponentInChildren<TeamProfileUiView>(true);
            Assert.That(profile.SelectedCharacter, Is.EqualTo("Mira"));
            Assert.That(profile.SelectedAgeProfile, Is.EqualTo("6-8"));
            Click("backButton");
            AssertScreen(UiScreen.MainMenu);
            Click("startButton");
            Click("confirmButton");
            AssertScreen(UiScreen.MobileHud);
            Click("pauseButton");
            AssertScreen(UiScreen.Options);
            Click("closeButton");
            AssertScreen(UiScreen.MobileHud);
            Click("finishPreviewButton");
            AssertScreen(UiScreen.ResultsReplay);
            Click("replayButton");
            AssertScreen(UiScreen.MobileHud);
            Click("finishPreviewButton");
            Click("mainMenuButton");
            AssertScreen(UiScreen.MainMenu);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AuthoredHudDisplaysTelemetryAndSpecialReadiness()
        {
            Click("startButton");
            Click("confirmButton");
            var state = SliceUiState2D.CreateDefault();
            state.HealthCurrent = 42;
            state.HealthMaximum = 120;
            state.ActiveCharacter = "Zoey";
            state.SpecialCharge = 0.4f;
            state.SpecialReady = false;
            controller.MobileHudView.RenderState(state);
            Assert.That(Find<Text>("healthText").text, Is.EqualTo("42 / 120"));
            Assert.That(Find<Text>("activeCharacterText").text, Is.EqualTo("Zoey"));
            Assert.That(Find<Button>("specialButton").interactable, Is.False);
            Assert.That(Find<Slider>("specialChargeBar").value, Is.EqualTo(.4f).Within(.001f));
            state.SpecialReady = true;
            state.SpecialCharge = 1f;
            controller.MobileHudView.RenderState(state);
            Assert.That(Find<Button>("specialButton").interactable, Is.True);
            Assert.That(Find<Image>("specialReadyGlow").enabled, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OptionsAndSurveyControlsEmitActualUserValues()
        {
            Click("optionsButton");
            bool? reduceFlashes = null;
            string language = null;
            controller.OptionsView.ReduceFlashesChanged += value => reduceFlashes = value;
            controller.OptionsView.LanguageChanged += value => language = value;
            Toggle toggle = Find<Toggle>("reduceFlashesToggle");
            bool requestedValue = !toggle.isOn;
            toggle.isOn = requestedValue;
            Assert.That(reduceFlashes, Is.EqualTo(requestedValue));
            Click("enLanguageButton");
            Assert.That(language, Is.EqualTo("EN"));
            Click("ptLanguageButton");
            Assert.That(language, Is.EqualTo("PT"));
            Click("closeButton");
            Click("startButton");
            Click("confirmButton");
            Click("finishPreviewButton");
            var results = controller.ResultsReplayView;
            int fun = 0;
            int difficulty = 0;
            string comments = null;
            results.SurveySubmitted += (f, d, c) => { fun = f; difficulty = d; comments = c; };
            Find<Slider>("funRatingSlider").value = 1;
            Find<Slider>("difficultyRatingSlider").value = 5;
            Find<InputField>("commentsInputField").text = "Teste da cena";
            Click("submitSurveyButton");
            Assert.That(fun, Is.EqualTo(1));
            Assert.That(difficulty, Is.EqualTo(5));
            Assert.That(comments, Is.EqualTo("Teste da cena"));
            Assert.That(Find<Text>("funRatingText").text, Does.Contain("1"));
            Assert.That(Find<Text>("difficultyRatingText").text, Does.Contain("5"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllViewRootsAndControlsRemainInsideActualSafeArea()
        {
            SliceSafeAreaFitter fitter = controller.GetComponentInChildren<SliceSafeAreaFitter>(true);
            Assert.That(fitter, Is.Not.Null);
            fitter.ApplySafeArea();
            var expected = SliceSafeAreaFitter.CalculateNormalizedSafeArea(Screen.safeArea,
                new Vector2(Screen.width, Screen.height));
            var safeRect = (RectTransform)fitter.transform;
            Assert.That(Vector2.Distance(safeRect.anchorMin, expected.anchorMin), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(safeRect.anchorMax, expected.anchorMax), Is.LessThan(.001f));
            Canvas.ForceUpdateCanvases();
            foreach (Selectable control in fitter.GetComponentsInChildren<Selectable>(true))
            {
                var corners = new Vector3[4];
                ((RectTransform)control.transform).GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    Vector2 local = safeRect.InverseTransformPoint(corner);
                    Assert.That(local.x, Is.InRange(safeRect.rect.xMin - 1, safeRect.rect.xMax + 1), control.name);
                    Assert.That(local.y, Is.InRange(safeRect.rect.yMin - 1, safeRect.rect.yMax + 1), control.name);
                }
            }
            yield return null;
        }

        private T Find<T>(string name) where T : Component
        {
            var matches = controller.GetComponentsInChildren<T>(true).Where(c => c.name == name).ToArray();
            Assert.That(matches.Length, Is.EqualTo(1), "Expected exactly one real control: " + name);
            return matches[0];
        }

        private void Click(string name)
        {
            Button button = Find<Button>(name);
            Assert.That(button.isActiveAndEnabled && button.IsInteractable(), Is.True, name);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                button = PointerEventData.InputButton.Left
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0), "Control must be raycastable: " + name);
            GameObject target = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Assert.That(target, Is.EqualTo(button.gameObject), "Another graphic obstructs " + name);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerClickHandler);
        }

        private void AssertScreen(UiScreen expected)
        {
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(expected));
            var roots = new[] { "MainMenuPanel", "TeamProfilePanel", "HudPanel", "OptionsPanel", "ResultsPanel" };
            for (int i = 0; i < roots.Length; i++)
            {
                var root = controller.GetComponentsInChildren<Transform>(true).Single(t => t.name == roots[i]);
                Assert.That(root.gameObject.activeInHierarchy, Is.EqualTo(i == (int)expected), roots[i]);
            }
        }
    }
}
