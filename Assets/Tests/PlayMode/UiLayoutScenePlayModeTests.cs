using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HuntrX.Presentation.UI;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.VerticalSlice;
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
            controller = Object.FindAnyObjectByType<SliceUiController2D>();
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
        public IEnumerator NavigationStartsWithoutPointerAndSubmitOpensProfile()
        {
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("startButton").gameObject));
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            AssertScreen(UiScreen.TeamProfile);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("rumiButton").gameObject));
            yield return null;
        }

        [UnityTest]
        public IEnumerator NavigationMoveSkipsUnavailableContinueAndSubmitOpensOptions()
        {
            var move = new AxisEventData(EventSystem.current)
            {
                moveDir = MoveDirection.Down,
                moveVector = Vector2.down
            };
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, move, ExecuteEvents.moveHandler);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("optionsButton").gameObject));
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            AssertScreen(UiScreen.Options);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReturningToScreenRestoresFocusAndRejectsDisabledRememberedControl()
        {
            EventSystem.current.SetSelectedGameObject(Find<Button>("optionsButton").gameObject);
            Click("optionsButton");
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Toggle>("reduceFlashesToggle").gameObject));
            EventSystem.current.SetSelectedGameObject(Find<Button>("enLanguageButton").gameObject);
            Click("closeButton");
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("optionsButton").gameObject));
            Click("optionsButton");
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("enLanguageButton").gameObject));
            Click("closeButton");
            Find<Button>("enLanguageButton").interactable = false;
            Click("optionsButton");
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Toggle>("reduceFlashesToggle").gameObject));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReturningToScreenRejectsHiddenAndNavigationDisabledRememberedControls()
        {
            var remembered = Find<Button>("enLanguageButton");
            foreach (bool hide in new[] { true, false })
            {
                Click("optionsButton");
                EventSystem.current.SetSelectedGameObject(remembered.gameObject);
                Click("closeButton");
                if (hide) remembered.gameObject.SetActive(false);
                else
                {
                    var navigation = remembered.navigation;
                    navigation.mode = Navigation.Mode.None;
                    remembered.navigation = navigation;
                }
                Click("optionsButton");
                AssertScreen(UiScreen.Options);
                Assert.That(EventSystem.current.currentSelectedGameObject,
                    Is.EqualTo(Find<Toggle>("reduceFlashesToggle").gameObject),
                    hide ? "Hidden remembered target must be discarded" : "Navigation.None target must be discarded");
                Click("closeButton");
                remembered.gameObject.SetActive(true);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator HidingControllerClearsOwnedFocus()
        {
            EventSystem.current.SetSelectedGameObject(Find<Button>("startButton").gameObject);
            controller.enabled = false;
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.Null);
            controller.enabled = true;
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("startButton").gameObject));
            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryScreenSelectsAnInteractableControlInItsVisibleView()
        {
            foreach (UiScreen screen in new[] { UiScreen.MainMenu, UiScreen.TeamProfile,
                UiScreen.MobileHud, UiScreen.Options, UiScreen.ResultsReplay })
            {
                controller.Flow.TransitionTo(screen);
                AssertScreen(screen);
                Transform expectedView = null;
                switch (screen)
                {
                    case UiScreen.MainMenu: expectedView = controller.MainMenuView.transform; break;
                    case UiScreen.TeamProfile: expectedView = controller.TeamProfileView.transform; break;
                    case UiScreen.MobileHud: expectedView = controller.MobileHudView.transform; break;
                    case UiScreen.Options: expectedView = controller.OptionsView.transform; break;
                    case UiScreen.ResultsReplay: expectedView = controller.ResultsReplayView.transform; break;
                }
                Assert.That(expectedView.gameObject.activeInHierarchy, Is.True, screen.ToString());
                var selected = EventSystem.current.currentSelectedGameObject;
                Assert.That(selected, Is.Not.Null, screen.ToString());
                var control = selected.GetComponent<Selectable>();
                Assert.That(control.isActiveAndEnabled && control.IsInteractable(), Is.True, screen.ToString());
                Assert.That(selected.transform.IsChildOf(expectedView), Is.True, screen.ToString());
                Assert.That(selected, Is.Not.EqualTo(Find<Button>("continueButton").gameObject));
                Assert.That(selected, Is.Not.EqualTo(Find<Button>("remapKeysButton").gameObject));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneBindsEveryViewControlAndContainsLegibleLabels()
        {
            var viewTypes = new[] { typeof(MainMenuUiView), typeof(TeamProfileUiView),
                typeof(MobileHudUiView), typeof(OptionsUiView), typeof(RemapPanelUiView), typeof(ResultsReplayUiView) };
            foreach (var type in viewTypes)
            {
                Component view = controller.GetComponentInChildren(type, true);
                Assert.That(view, Is.Not.Null, type.Name);
                foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!field.IsDefined(typeof(SerializeField), true) ||
                        !typeof(Object).IsAssignableFrom(field.FieldType)) continue;
                    // Legacy combined switch is optional; the authored layout exposes independent jump/rescue toggles.
                    if (type == typeof(OptionsUiView) && field.Name == "toggleJumpRescueToggle") continue;
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
        public IEnumerator RemapPanelOpensFromOptionsAndBackRestoresOptionsFocus()
        {
            Click("optionsButton");
            Assert.That(controller.RemapPanelView.gameObject.activeInHierarchy, Is.False);

            Click("remapKeysButton");
            Assert.That(controller.Flow.CurrentScreen, Is.EqualTo(UiScreen.Options));
            Assert.That(controller.OptionsView.gameObject.activeInHierarchy, Is.True);
            Assert.That(controller.RemapPanelView.gameObject.activeInHierarchy, Is.True);
            Assert.That(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(controller.RemapPanelView.transform), Is.True);

            Click("remapBackButton");
            Assert.That(controller.RemapPanelView.gameObject.activeInHierarchy, Is.False);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Find<Button>("remapKeysButton").gameObject));
            yield return null;
        }

        [UnityTest]
        public IEnumerator OptionsControlsUpdateDemonstrationSessionPreferences()
        {
            Click("optionsButton");
            var preferences = controller.GetComponent<AccessibilityPreferences2D>();
            Assert.That(preferences, Is.Not.Null);

            Find<Toggle>("reduceFlashesToggle").isOn = false;
            Find<Toggle>("subtitlesToggle").isOn = false;
            Find<Toggle>("aimAssistToggle").isOn = false;
            Find<Toggle>("toggleJumpToggle").isOn = true;
            Find<Toggle>("toggleRescueToggle").isOn = true;

            Assert.That(preferences.ReduceFlashes, Is.False);
            Assert.That(preferences.CaptionsEnabled, Is.False);
            Assert.That(preferences.AssistedAim, Is.False);
            Assert.That(preferences.ToggleModeFor(GameplayControl.Jump), Is.True);
            Assert.That(preferences.ToggleModeFor(GameplayControl.Interact), Is.True);
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
        public IEnumerator SurveySlidersMeetReferenceTouchMinimumAndAcceptEdgeTaps()
        {
            Click("startButton");
            Click("confirmButton");
            Click("finishPreviewButton");
            Canvas.ForceUpdateCanvases();
            var panel = (RectTransform)controller.ResultsReplayView.transform;
            foreach (string name in new[] { "funRatingSlider", "difficultyRatingSlider" })
            {
                Slider slider = Find<Slider>(name);
                var rect = (RectTransform)slider.transform;
                // Compare in the declared design reference, independently of the test runner's window.
                Assert.That(rect.rect.height / panel.rect.height * SliceUiTokens.ReferenceResolution.y,
                    Is.GreaterThanOrEqualTo(SliceUiTokens.MinimumTouchTargetSize), name);
                var below = name == "funRatingSlider"
                    ? (RectTransform)Find<Text>("difficultyLabel").transform
                    : (RectTransform)Find<InputField>("commentsInputField").transform;
                var above = (RectTransform)Find<Text>(name == "funRatingSlider"
                    ? "funLabel" : "difficultyLabel").transform;
                Assert.That(rect.anchorMin.y, Is.GreaterThanOrEqualTo(below.anchorMax.y),
                    "Slider must not overlap the content below: " + name);
                Assert.That(rect.anchorMax.y, Is.LessThanOrEqualTo(above.anchorMin.y),
                    "Slider must not overlap its label: " + name);
                Assert.That(slider.handleRect.anchorMin.y, Is.GreaterThanOrEqualTo(0f), name);
                Assert.That(slider.handleRect.anchorMax.y, Is.LessThanOrEqualTo(1f), name);
                foreach (float edge in new[] { .05f, .95f })
                {
                    slider.value = 3;
                    Canvas.ForceUpdateCanvases();
                    var local = new Vector2(Mathf.Lerp(rect.rect.xMin, rect.rect.xMax, .9f),
                        Mathf.Lerp(rect.rect.yMin, rect.rect.yMax, edge));
                    var pointer = new PointerEventData(EventSystem.current)
                    {
                        position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(local)),
                        button = PointerEventData.InputButton.Left
                    };
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    Assert.That(hits.Count, Is.GreaterThan(0), name);
                    GameObject target = ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject);
                    Assert.That(target, Is.EqualTo(slider.gameObject), "Obstructed slider edge: " + name);
                    ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerUpHandler);
                    Assert.That(slider.value, Is.EqualTo(5), "Edge tap must update rating: " + name);
                }
            }
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
            Assert.That(hits.Count, Is.GreaterThan(0), "Control must be raycastable: " + name +
                " pointer=" + pointer.position + " screen=" + Screen.width + "x" + Screen.height);
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
