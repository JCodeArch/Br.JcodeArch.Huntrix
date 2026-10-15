using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Main menu view implementing Figma frame 2:2.
    /// Start button navigates to character/profile selection.
    /// Continue button is strictly disabled until save/resume contracts exist.
    /// </summary>
    public sealed class MainMenuUiView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;

        public event Action StartRequested;
        public event Action OptionsRequested;

        public bool IsContinueInteractable => continueButton != null && continueButton.interactable;

        private void Awake()
        {
            // Figma specification: Continue must remain unavailable
            if (continueButton != null)
            {
                continueButton.interactable = false;
            }

            if (startButton != null)
            {
                startButton.onClick.AddListener(() => StartRequested?.Invoke());
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(() => OptionsRequested?.Invoke());
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveAllListeners();
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
