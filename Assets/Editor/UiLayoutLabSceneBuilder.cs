using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using HuntrX.Presentation.UI;
using HuntrX.Presentation.UI.Views;

namespace HuntrX.Editor
{
    public static class UiLayoutLabSceneBuilder
    {
        [MenuItem("HUNTR/X/Build UI Layout Lab Scene")]
        public static void BuildScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cameraGo = new GameObject("UiCamera");
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SliceUiTokens.Background;
            cam.orthographic = true;

            // Canvas Root
            var canvasGo = new GameObject("UiCanvasRoot");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SliceUiTokens.ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Safe Area Container
            var safeAreaGo = new GameObject("SafeAreaContainer", typeof(RectTransform));
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            var safeRect = safeAreaGo.GetComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;
            safeAreaGo.AddComponent<SliceSafeAreaFitter>();

            // 1. Main Menu View
            var mainMenuGo = CreateViewPanel("MainMenuPanel", safeAreaGo.transform);
            var mainMenuView = mainMenuGo.AddComponent<MainMenuUiView>();

            // 2. Team Profile View
            var profileGo = CreateViewPanel("TeamProfilePanel", safeAreaGo.transform);
            var profileView = profileGo.AddComponent<TeamProfileUiView>();

            // 3. Mobile HUD View
            var hudGo = CreateViewPanel("HudPanel", safeAreaGo.transform);
            var hudView = hudGo.AddComponent<MobileHudUiView>();

            // 4. Options View
            var optionsGo = CreateViewPanel("OptionsPanel", safeAreaGo.transform);
            var optionsView = optionsGo.AddComponent<OptionsUiView>();

            // 5. Results Replay View
            var resultsGo = CreateViewPanel("ResultsPanel", safeAreaGo.transform);
            var resultsView = resultsGo.AddComponent<ResultsReplayUiView>();

            // Flow & Controller
            var controller = canvasGo.AddComponent<SliceUiController2D>();
            var flow = canvasGo.GetComponent<SliceUiFlow2D>();
            controller.Initialize(flow, mainMenuView, profileView, hudView, optionsView, resultsView);

            // Event System
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Save Scene
            string scenePath = "Assets/Scenes/UiLayoutLab.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.Refresh();
            Debug.Log($"UI Layout Lab Scene successfully created at {scenePath}");
        }

        private static GameObject CreateViewPanel(string name, Transform parent)
        {
            var panelGo = new GameObject(name, typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);
            var rect = panelGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panelGo;
        }
    }
}
