using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Team profile & character selection view implementing Figma frame 2:29.
    /// Supports selecting initial hero (Rumi, Mira, Zoey) and age accessibility profile (4-5, 6-8, 9-12, 13-15, 16+).
    /// </summary>
    public sealed class TeamProfileUiView : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField] private Button rumiButton;
        [SerializeField] private Button miraButton;
        [SerializeField] private Button zoeyButton;

        [Header("Age Profile Selection")]
        [SerializeField] private Button profile4To5Button;
        [SerializeField] private Button profile6To8Button;
        [SerializeField] private Button profile9To12Button;
        [SerializeField] private Button profile13To15Button;
        [SerializeField] private Button profile16PlusButton;

        [Header("Navigation")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        private string selectedCharacter = "Rumi";
        private string selectedAgeProfile = "9-12";

        public string SelectedCharacter => selectedCharacter;
        public string SelectedAgeProfile => selectedAgeProfile;

        public event Action<string> CharacterSelected;
        public event Action<string> AgeProfileSelected;
        public event Action Confirmed;
        public event Action BackRequested;

        private void Awake()
        {
            if (rumiButton == null) rumiButton = FindControl<Button>("rumiButton");
            if (miraButton == null) miraButton = FindControl<Button>("miraButton");
            if (zoeyButton == null) zoeyButton = FindControl<Button>("zoeyButton");
            if (profile4To5Button == null) profile4To5Button = FindControl<Button>("profile4To5Button");
            if (profile6To8Button == null) profile6To8Button = FindControl<Button>("profile6To8Button");
            if (profile9To12Button == null) profile9To12Button = FindControl<Button>("profile9To12Button");
            if (profile13To15Button == null) profile13To15Button = FindControl<Button>("profile13To15Button");
            if (profile16PlusButton == null) profile16PlusButton = FindControl<Button>("profile16PlusButton");
            if (confirmButton == null) confirmButton = FindControl<Button>("confirmButton");
            if (backButton == null) backButton = FindControl<Button>("backButton");
            BindCharacterButton(rumiButton, "Rumi");
            BindCharacterButton(miraButton, "Mira");
            BindCharacterButton(zoeyButton, "Zoey");

            BindProfileButton(profile4To5Button, "4-5");
            BindProfileButton(profile6To8Button, "6-8");
            BindProfileButton(profile9To12Button, "9-12");
            BindProfileButton(profile13To15Button, "13-15");
            BindProfileButton(profile16PlusButton, "16+");

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(() => Confirmed?.Invoke());
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => BackRequested?.Invoke());
            }
        }

        private void BindCharacterButton(Button button, string character)
        {
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    selectedCharacter = character;
                    CharacterSelected?.Invoke(character);
                });
            }
        }

        private void BindProfileButton(Button button, string profile)
        {
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    selectedAgeProfile = profile;
                    AgeProfileSelected?.Invoke(profile);
                });
            }
        }

        public void SetSelection(string character, string profile)
        {
            selectedCharacter = character;
            selectedAgeProfile = profile;
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
