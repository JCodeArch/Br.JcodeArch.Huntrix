using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuntrX.Presentation.UI
{
    public enum UiScreen
    {
        MainMenu,
        TeamProfile,
        MobileHud,
        Options,
        ResultsReplay
    }

    /// <summary>
    /// Flow controller managing screen transitions, navigation stack, and lifecycle.
    /// </summary>
    public sealed class SliceUiFlow2D : MonoBehaviour
    {
        [SerializeField] private UiScreen initialScreen = UiScreen.MainMenu;

        private readonly Stack<UiScreen> navigationStack = new Stack<UiScreen>();
        private UiScreen currentScreen;

        public UiScreen CurrentScreen => currentScreen;
        public int HistoryCount => navigationStack.Count;

        public event Action<UiScreen, UiScreen> ScreenChanged;

        private void Awake()
        {
            currentScreen = initialScreen;
        }

        public bool CanTransitionTo(UiScreen target)
        {
            if (target == currentScreen)
            {
                return false;
            }

            switch (currentScreen)
            {
                case UiScreen.MainMenu:
                    // From main menu, player can go to TeamProfile or Options
                    return target == UiScreen.TeamProfile || target == UiScreen.Options || target == UiScreen.MobileHud;

                case UiScreen.TeamProfile:
                    // From profile selection, player can start gameplay (MobileHud) or return to MainMenu
                    return target == UiScreen.MobileHud || target == UiScreen.MainMenu || target == UiScreen.Options;

                case UiScreen.MobileHud:
                    // During gameplay, can open Options (Pause) or finish to ResultsReplay
                    return target == UiScreen.Options || target == UiScreen.ResultsReplay;

                case UiScreen.Options:
                    // Options can return to previous caller (MainMenu, TeamProfile, or MobileHud)
                    return true;

                case UiScreen.ResultsReplay:
                    // From results, replay restarts MobileHud or returns to MainMenu / TeamProfile
                    return target == UiScreen.MobileHud || target == UiScreen.MainMenu || target == UiScreen.TeamProfile;

                default:
                    return false;
            }
        }

        public bool TransitionTo(UiScreen target, bool rememberInHistory = true)
        {
            if (!CanTransitionTo(target))
            {
                return false;
            }

            UiScreen previous = currentScreen;
            if (rememberInHistory)
            {
                navigationStack.Push(previous);
            }

            currentScreen = target;
            ScreenChanged?.Invoke(previous, currentScreen);
            return true;
        }

        public bool TryGoBack()
        {
            if (navigationStack.Count == 0)
            {
                return false;
            }

            UiScreen previousTarget = navigationStack.Pop();
            UiScreen current = currentScreen;
            currentScreen = previousTarget;
            ScreenChanged?.Invoke(current, currentScreen);
            return true;
        }

        public void ClearHistory()
        {
            navigationStack.Clear();
        }
    }
}
