using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Options & Accessibility settings view implementing Figma frame 2:110.
    /// Exposes toggles for flashes, subtitles, aim assist, toggle jump/rescue,
    /// narrative language selection (PT/EN), and key remapping subpanel trigger.
    /// </summary>
    public sealed class OptionsUiView : MonoBehaviour
    {
        [Header("Accessibility & Gameplay Toggles")]
        [SerializeField] private Toggle reduceFlashesToggle;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Toggle aimAssistToggle;
        // Kept for existing layouts that expose the original combined switch.
        [SerializeField] private Toggle toggleJumpRescueToggle;
        [SerializeField] private Toggle toggleJumpToggle;
        [SerializeField] private Toggle toggleRescueToggle;

        [Header("Narrative Language")]
        [SerializeField] private Button ptLanguageButton;
        [SerializeField] private Button enLanguageButton;
        [SerializeField] private Text languageStatusText;

        [Header("Remapping & Actions")]
        [SerializeField] private Button remapKeysButton;
        [SerializeField] private Button closeButton;

        private string currentLanguage = "PT";
        public string CurrentLanguage => currentLanguage;

        public event Action<bool> ReduceFlashesChanged;
        public event Action<bool> SubtitlesChanged;
        public event Action<bool> AimAssistChanged;
        public event Action<bool> ToggleJumpRescueChanged;
        public event Action<bool> ToggleJumpChanged;
        public event Action<bool> ToggleRescueChanged;
        public event Action<string> LanguageChanged;
        public event Action RemapKeysRequested;
        public event Action CloseRequested;

        private void Awake()
        {
            if (reduceFlashesToggle == null) reduceFlashesToggle = FindControl<Toggle>("reduceFlashesToggle");
            if (subtitlesToggle == null) subtitlesToggle = FindControl<Toggle>("subtitlesToggle");
            if (aimAssistToggle == null) aimAssistToggle = FindControl<Toggle>("aimAssistToggle");
            if (toggleJumpRescueToggle == null) toggleJumpRescueToggle = FindControl<Toggle>("toggleJumpRescueToggle");
            if (toggleJumpToggle == null) toggleJumpToggle = FindControl<Toggle>("toggleJumpToggle");
            if (toggleRescueToggle == null) toggleRescueToggle = FindControl<Toggle>("toggleRescueToggle");
            if (languageStatusText == null) languageStatusText = FindControl<Text>("languageStatusText");
            if (ptLanguageButton == null) ptLanguageButton = FindControl<Button>("ptLanguageButton");
            if (enLanguageButton == null) enLanguageButton = FindControl<Button>("enLanguageButton");
            if (remapKeysButton == null) remapKeysButton = FindControl<Button>("remapKeysButton");
            if (closeButton == null) closeButton = FindControl<Button>("closeButton");
            if (reduceFlashesToggle != null)
            {
                reduceFlashesToggle.onValueChanged.AddListener(val => ReduceFlashesChanged?.Invoke(val));
            }

            if (subtitlesToggle != null)
            {
                subtitlesToggle.onValueChanged.AddListener(val => SubtitlesChanged?.Invoke(val));
            }

            if (aimAssistToggle != null)
            {
                aimAssistToggle.onValueChanged.AddListener(val => AimAssistChanged?.Invoke(val));
            }

            if (toggleJumpRescueToggle != null)
            {
                toggleJumpRescueToggle.onValueChanged.AddListener(val => ToggleJumpRescueChanged?.Invoke(val));
            }

            if (toggleJumpToggle != null)
                toggleJumpToggle.onValueChanged.AddListener(val => ToggleJumpChanged?.Invoke(val));
            if (toggleRescueToggle != null)
                toggleRescueToggle.onValueChanged.AddListener(val => ToggleRescueChanged?.Invoke(val));

            RenderLanguage();
            if (ptLanguageButton != null)
            {
                ptLanguageButton.onClick.AddListener(() => SetLanguage("PT"));
            }

            if (enLanguageButton != null)
            {
                enLanguageButton.onClick.AddListener(() => SetLanguage("EN"));
            }

            if (remapKeysButton != null)
            {
                remapKeysButton.onClick.AddListener(() =>
                {
                    if (UnityEngine.EventSystems.EventSystem.current != null)
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(remapKeysButton.gameObject);
                    RemapKeysRequested?.Invoke();
                });
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => CloseRequested?.Invoke());
            }
        }

        public void SetLanguage(string language)
        {
            if (!IsSupportedLanguage(language) || currentLanguage == language) return;
            currentLanguage = language;
            RenderLanguage();
            LanguageChanged?.Invoke(language);
        }

        public void SetPreferences(bool reduceFlashes, bool subtitles, bool aimAssist, bool toggleJumpRescue, string language)
        {
            SetPreferences(reduceFlashes, subtitles, aimAssist, toggleJumpRescue, toggleJumpRescue, language);
        }

        /// <summary>Hydrates independent runtime preferences without issuing user commands.</summary>
        public void SetPreferences(bool reduceFlashes, bool subtitles, bool aimAssist, bool toggleJump, bool toggleRescue, string language)
        {
            if (reduceFlashesToggle != null) reduceFlashesToggle.SetIsOnWithoutNotify(reduceFlashes);
            if (subtitlesToggle != null) subtitlesToggle.SetIsOnWithoutNotify(subtitles);
            if (aimAssistToggle != null) aimAssistToggle.SetIsOnWithoutNotify(aimAssist);
            if (toggleJumpToggle != null) toggleJumpToggle.SetIsOnWithoutNotify(toggleJump);
            if (toggleRescueToggle != null) toggleRescueToggle.SetIsOnWithoutNotify(toggleRescue);
            if (toggleJumpRescueToggle != null) toggleJumpRescueToggle.SetIsOnWithoutNotify(toggleJump && toggleRescue);
            if (IsSupportedLanguage(language)) currentLanguage = language;
            RenderLanguage();
        }

        private static bool IsSupportedLanguage(string language) => language == "PT" || language == "EN";

        private void RenderLanguage()
        {
            if (languageStatusText != null)
                languageStatusText.text = "Idioma selecionado: " + (currentLanguage == "EN" ? "English" : "Português");
        }

        private T FindControl<T>(string controlName) where T : Component
        {
            foreach (var control in GetComponentsInChildren<T>(true))
                if (control.name == controlName) return control;
            return null;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
