using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HuntrX.Presentation.UI.Views;

namespace HuntrX.Presentation.UI
{
    /// <summary>
    /// Orchestrator for the UI screens, binding views to the flow coordinator.
    /// Provides an isolated testing harness for visual layouts without requiring gameplay sessions.
    /// </summary>
    public sealed class SliceUiController2D : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private SliceUiFlow2D flow;

        [Header("Views")]
        [SerializeField] private MainMenuUiView mainMenuView;
        [SerializeField] private TeamProfileUiView teamProfileView;
        [SerializeField] private MobileHudUiView mobileHudView;
        [SerializeField] private OptionsUiView optionsView;
        [SerializeField] private ResultsReplayUiView resultsReplayView;

        private readonly Dictionary<Transform, Selectable> rememberedFocus = new Dictionary<Transform, Selectable>();
        private Transform focusedView;

        // The lab creates its EventSystem after composing this controller.
        private void Start() { if (flow != null) UpdateActiveViews(flow.CurrentScreen); }

        private void OnEnable() { if (flow != null) UpdateActiveViews(flow.CurrentScreen); }

        private void OnDisable()
        {
            RememberAndClearFocus();
        }

        private void RememberAndClearFocus()
        {
            var events = EventSystem.current;
            var selected = events != null ? events.currentSelectedGameObject : null;
            if (selected == null || !selected.transform.IsChildOf(transform)) return;
            var control = selected.GetComponent<Selectable>();
            if (focusedView != null && control != null && selected.transform.IsChildOf(focusedView))
                rememberedFocus[focusedView] = control;
            events.SetSelectedGameObject(null);
        }

        private void RestoreFocus(Transform view)
        {
            focusedView = view;
            if (view == null || EventSystem.current == null || !isActiveAndEnabled) return;
            rememberedFocus.TryGetValue(view, out var target);
            if (!IsValidFocus(target, view))
            {
                target = null;
                foreach (var candidate in view.GetComponentsInChildren<Selectable>(true))
                    if (IsValidFocus(candidate, view)) { target = candidate; break; }
            }
            EventSystem.current.SetSelectedGameObject(target != null ? target.gameObject : null);
        }

        private static bool IsValidFocus(Selectable control, Transform view)
        {
            return control != null && control.transform.IsChildOf(view) &&
                control.isActiveAndEnabled && control.IsInteractable() &&
                control.navigation.mode != Navigation.Mode.None;
        }

        public SliceUiFlow2D Flow => flow;
        public MainMenuUiView MainMenuView => mainMenuView;
        public TeamProfileUiView TeamProfileView => teamProfileView;
        public MobileHudUiView MobileHudView => mobileHudView;
        public OptionsUiView OptionsView => optionsView;
        public ResultsReplayUiView ResultsReplayView => resultsReplayView;

        private void Awake()
        {
            if (flow == null)
            {
                flow = GetComponent<SliceUiFlow2D>();
                if (flow == null)
                {
                    flow = gameObject.AddComponent<SliceUiFlow2D>();
                }
            }

            ResolveViewReferences();
            SubscribeViewEvents();
            flow.ScreenChanged += OnScreenChanged;
            UpdateActiveViews(flow.CurrentScreen);
        }

        public void Initialize(
            SliceUiFlow2D flowInstance,
            MainMenuUiView mainMenu,
            TeamProfileUiView teamProfile,
            MobileHudUiView mobileHud,
            OptionsUiView options,
            ResultsReplayUiView resultsReplay)
        {
            UnsubscribeViewEvents();
            if (flow != null) flow.ScreenChanged -= OnScreenChanged;
            flow = flowInstance;
            mainMenuView = mainMenu;
            teamProfileView = teamProfile;
            mobileHudView = mobileHud;
            optionsView = options;
            resultsReplayView = resultsReplay;

            SubscribeViewEvents();
            if (flow != null)
            {
                flow.ScreenChanged -= OnScreenChanged;
                flow.ScreenChanged += OnScreenChanged;
                UpdateActiveViews(flow.CurrentScreen);
            }
        }

        private void ResolveViewReferences()
        {
            if (mainMenuView == null) mainMenuView = GetComponentInChildren<MainMenuUiView>(true);
            if (teamProfileView == null) teamProfileView = GetComponentInChildren<TeamProfileUiView>(true);
            if (mobileHudView == null) mobileHudView = GetComponentInChildren<MobileHudUiView>(true);
            if (optionsView == null) optionsView = GetComponentInChildren<OptionsUiView>(true);
            if (resultsReplayView == null) resultsReplayView = GetComponentInChildren<ResultsReplayUiView>(true);
        }

        private void OnDestroy()
        {
            if (flow != null)
            {
                flow.ScreenChanged -= OnScreenChanged;
            }
            UnsubscribeViewEvents();
        }

        private void SubscribeViewEvents()
        {
            if (mainMenuView != null) { mainMenuView.StartRequested += OpenProfile; mainMenuView.OptionsRequested += OpenOptions; }
            if (teamProfileView != null) { teamProfileView.Confirmed += OpenHud; teamProfileView.BackRequested += GoBack; }
            if (mobileHudView != null) mobileHudView.PausePressed += OpenOptions;
            if (optionsView != null) optionsView.CloseRequested += GoBack;
            if (resultsReplayView != null) { resultsReplayView.ReplayRequested += Replay; resultsReplayView.MainMenuRequested += OpenMenu; }
        }

        private void UnsubscribeViewEvents()
        {
            if (mainMenuView != null) { mainMenuView.StartRequested -= OpenProfile; mainMenuView.OptionsRequested -= OpenOptions; }
            if (teamProfileView != null) { teamProfileView.Confirmed -= OpenHud; teamProfileView.BackRequested -= GoBack; }
            if (mobileHudView != null) mobileHudView.PausePressed -= OpenOptions;
            if (optionsView != null) optionsView.CloseRequested -= GoBack;
            if (resultsReplayView != null) { resultsReplayView.ReplayRequested -= Replay; resultsReplayView.MainMenuRequested -= OpenMenu; }
        }
        private void OpenProfile() { flow.TransitionTo(UiScreen.TeamProfile); }
        private void OpenOptions() { flow.TransitionTo(UiScreen.Options); }
        private void OpenHud() { flow.TransitionTo(UiScreen.MobileHud); flow.ClearHistory(); }
        private void GoBack() { flow.TryGoBack(); }
        private void Replay() { flow.TransitionTo(UiScreen.MobileHud); flow.ClearHistory(); }
        private void OpenMenu() { flow.TransitionTo(UiScreen.MainMenu); flow.ClearHistory(); }

        private void OnScreenChanged(UiScreen previous, UiScreen current)
        {
            UpdateActiveViews(current);
        }

        public void UpdateActiveViews(UiScreen screen)
        {
            RememberAndClearFocus();
            if (mainMenuView != null) mainMenuView.SetVisible(screen == UiScreen.MainMenu);
            if (teamProfileView != null) teamProfileView.SetVisible(screen == UiScreen.TeamProfile);
            if (mobileHudView != null) mobileHudView.SetVisible(screen == UiScreen.MobileHud);
            if (optionsView != null) optionsView.SetVisible(screen == UiScreen.Options);
            if (resultsReplayView != null) resultsReplayView.SetVisible(screen == UiScreen.ResultsReplay);
            Transform view = null;
            switch (screen)
            {
                case UiScreen.MainMenu: view = mainMenuView != null ? mainMenuView.transform : null; break;
                case UiScreen.TeamProfile: view = teamProfileView != null ? teamProfileView.transform : null; break;
                case UiScreen.MobileHud: view = mobileHudView != null ? mobileHudView.transform : null; break;
                case UiScreen.Options: view = optionsView != null ? optionsView.transform : null; break;
                case UiScreen.ResultsReplay: view = resultsReplayView != null ? resultsReplayView.transform : null; break;
            }
            RestoreFocus(view);
        }
    }
}
