using UnityEngine;
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
            if (mainMenuView != null)
            {
                mainMenuView.StartRequested += () => flow.TransitionTo(UiScreen.TeamProfile);
                mainMenuView.OptionsRequested += () => flow.TransitionTo(UiScreen.Options);
            }

            if (teamProfileView != null)
            {
                teamProfileView.Confirmed += () => flow.TransitionTo(UiScreen.MobileHud);
                teamProfileView.BackRequested += () => flow.TryGoBack();
            }

            if (mobileHudView != null)
            {
                mobileHudView.PausePressed += () => flow.TransitionTo(UiScreen.Options);
            }

            if (optionsView != null)
            {
                optionsView.CloseRequested += () => flow.TryGoBack();
            }

            if (resultsReplayView != null)
            {
                resultsReplayView.ReplayRequested += () => flow.TransitionTo(UiScreen.MobileHud);
                resultsReplayView.MainMenuRequested += () => flow.TransitionTo(UiScreen.MainMenu);
            }
        }

        private void UnsubscribeViewEvents()
        {
            // View listeners cleanup
        }

        private void OnScreenChanged(UiScreen previous, UiScreen current)
        {
            UpdateActiveViews(current);
        }

        public void UpdateActiveViews(UiScreen screen)
        {
            if (mainMenuView != null) mainMenuView.SetVisible(screen == UiScreen.MainMenu);
            if (teamProfileView != null) teamProfileView.SetVisible(screen == UiScreen.TeamProfile);
            if (mobileHudView != null) mobileHudView.SetVisible(screen == UiScreen.MobileHud);
            if (optionsView != null) optionsView.SetVisible(screen == UiScreen.Options);
            if (resultsReplayView != null) resultsReplayView.SetVisible(screen == UiScreen.ResultsReplay);
        }
    }
}
