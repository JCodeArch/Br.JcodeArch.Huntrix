using System;
using System.Collections.Generic;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Remap panel view implementing the presentation contract from docs/ui/REMAP_PANEL_CONTRACT.md.
    /// Manages keyboard remapping selection, input editing, validation status, and integration with AccessibilityPreferences2D.
    /// </summary>
    public sealed class RemapPanelUiView : MonoBehaviour
    {
        [Header("Selection & Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;

        [Header("Control Selection")]
        [SerializeField] private Dropdown actionDropdown;
        [SerializeField] private Text currentBindingText;

        [Header("Key Input & Editing")]
        [SerializeField] private InputField keyInputField;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button restoreButton;

        [Header("Feedback & Status")]
        [SerializeField] private Text helpText;
        [SerializeField] private Text statusText;

        private AccessibilityPreferences2D preferences;
        private GameplayControl selectedControl = GameplayControl.Attack;

        public const string DefaultHelpMessage = "A mesma tecla pode acionar mais de uma ação";
        public const string InitialStatusMessage = "Ajustes valem nesta partida";
        public const string EmptyFieldMessage = "Informe uma tecla";
        public const string InvalidKeyMessage = "Tecla inválida. Use uma letra, número ou nome como space";
        public const string AppliedMessage = "Tecla atualizada para esta partida";
        public const string RestoredMessage = "Tecla padrão restaurada";
        public const string UnavailableMessage = "Controles indisponíveis";

        public event Action BackRequested;
        public event Action<GameplayControl, string> RemapApplied;
        public event Action<GameplayControl> RemapRestored;

        public GameplayControl SelectedControl => selectedControl;
        public string Status => statusText != null ? statusText.text : string.Empty;
        public string CurrentBindingDisplay => currentBindingText != null ? currentBindingText.text : string.Empty;

        private void Awake()
        {
            if (backButton == null) backButton = FindControl<Button>("backButton") ?? FindControl<Button>("remapBackButton");
            if (titleText == null) titleText = FindControl<Text>("titleText");
            if (actionDropdown == null) actionDropdown = FindControl<Dropdown>("actionDropdown");
            if (currentBindingText == null) currentBindingText = FindControl<Text>("currentBindingText");
            if (keyInputField == null) keyInputField = FindControl<InputField>("keyInputField");
            if (applyButton == null) applyButton = FindControl<Button>("applyButton");
            if (restoreButton == null) restoreButton = FindControl<Button>("restoreButton");
            if (helpText == null) helpText = FindControl<Text>("helpText");
            if (statusText == null) statusText = FindControl<Text>("statusText");

            if (keyInputField != null)
            {
                keyInputField.characterLimit = 24;
                keyInputField.onValueChanged.AddListener(OnInputChanged);
            }

            if (actionDropdown != null)
            {
                PopulateActionDropdown();
                actionDropdown.onValueChanged.AddListener(OnActionSelected);
            }

            if (applyButton != null)
            {
                applyButton.onClick.AddListener(ApplyRemap);
            }

            if (restoreButton != null)
            {
                restoreButton.onClick.AddListener(RestoreDefault);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => BackRequested?.Invoke());
            }

            ResetDisplayState();
        }

        public void BindPreferences(AccessibilityPreferences2D source)
        {
            preferences = source;
            // Awake runs before preferences exist and leaves the "unavailable" status; clear it once bound.
            if (preferences != null && Status == UnavailableMessage) SetStatus(InitialStatusMessage);
            RefreshCurrentBinding();
            UpdateControlAvailability();
        }

        public void SelectControl(GameplayControl control)
        {
            selectedControl = control;
            if (actionDropdown != null)
            {
                int index = (int)control;
                if (index >= 0 && index < actionDropdown.options.Count)
                {
                    actionDropdown.SetValueWithoutNotify(index);
                }
            }
            RefreshCurrentBinding();
        }

        public void SetInputText(string text)
        {
            if (keyInputField != null)
            {
                keyInputField.text = text;
            }
            OnInputChanged(text);
        }

        public void ApplyRemap()
        {
            if (preferences == null)
            {
                SetStatus(UnavailableMessage);
                return;
            }

            string input = keyInputField != null ? keyInputField.text : null;
            if (string.IsNullOrWhiteSpace(input))
            {
                SetStatus(EmptyFieldMessage);
                return;
            }

            string cleanKey = input.Trim();
            string path = cleanKey.StartsWith("<Keyboard>/", StringComparison.Ordinal)
                ? cleanKey
                : "<Keyboard>/" + cleanKey;

            bool success = preferences.TryRemap(selectedControl, path);
            if (success)
            {
                SetStatus(AppliedMessage);
                RefreshCurrentBinding();
                RemapApplied?.Invoke(selectedControl, cleanKey);
            }
            else
            {
                SetStatus(InvalidKeyMessage);
            }
        }

        public void RestoreDefault()
        {
            if (preferences == null)
            {
                SetStatus(UnavailableMessage);
                return;
            }

            preferences.ClearRemap(selectedControl);
            SetStatus(RestoredMessage);
            RefreshCurrentBinding();
            RemapRestored?.Invoke(selectedControl);
        }

        private void OnActionSelected(int index)
        {
            selectedControl = (GameplayControl)index;
            RefreshCurrentBinding();
        }

        private void OnInputChanged(string text)
        {
            if (applyButton != null)
            {
                applyButton.interactable = preferences != null && !string.IsNullOrWhiteSpace(text);
            }
        }

        private void RefreshCurrentBinding()
        {
            if (currentBindingText == null) return;

            if (preferences == null)
            {
                currentBindingText.text = "Indisponível";
                return;
            }

            string bound = preferences.KeyboardBinding(selectedControl);
            if (string.IsNullOrWhiteSpace(bound))
            {
                currentBindingText.text = GetDefaultBindingDisplay(selectedControl);
            }
            else
            {
                currentBindingText.text = bound.StartsWith("<Keyboard>/", StringComparison.Ordinal)
                    ? bound.Substring(11)
                    : bound;
            }
        }

        public static string GetDefaultBindingDisplay(GameplayControl control)
        {
            switch (control)
            {
                case GameplayControl.MoveLeft: return "A";
                case GameplayControl.MoveRight: return "D";
                case GameplayControl.MoveUp: return "W";
                case GameplayControl.MoveDown: return "S";
                case GameplayControl.Jump: return "space";
                case GameplayControl.Dash: return "leftShift";
                case GameplayControl.Attack: return "J";
                case GameplayControl.Parry: return "K";
                case GameplayControl.SwitchCharacter: return "Q";
                case GameplayControl.Interact: return "E";
                case GameplayControl.FoodSelf: return "R";
                case GameplayControl.FoodFan: return "F";
                case GameplayControl.Special: return "L";
                case GameplayControl.Pause: return "escape";
                default: return "—";
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void ResetDisplayState()
        {
            if (helpText != null) helpText.text = DefaultHelpMessage;
            SetStatus(preferences == null ? UnavailableMessage : InitialStatusMessage);
            RefreshCurrentBinding();
            UpdateControlAvailability();
        }

        private void UpdateControlAvailability()
        {
            bool available = preferences != null;
            if (applyButton != null)
            {
                string input = keyInputField != null ? keyInputField.text : null;
                applyButton.interactable = available && !string.IsNullOrWhiteSpace(input);
            }
            if (restoreButton != null) restoreButton.interactable = available;
            if (keyInputField != null) keyInputField.interactable = available;
            if (actionDropdown != null) actionDropdown.interactable = available;
        }

        private void PopulateActionDropdown()
        {
            if (actionDropdown == null) return;
            actionDropdown.options.Clear();
            var values = (GameplayControl[])Enum.GetValues(typeof(GameplayControl));
            for (int i = 0; i < values.Length; i++)
            {
                actionDropdown.options.Add(new Dropdown.OptionData(values[i].ToString()));
            }
            actionDropdown.value = (int)selectedControl;
            actionDropdown.RefreshShownValue();
        }

        private T FindControl<T>(string controlName) where T : Component
        {
            foreach (var control in GetComponentsInChildren<T>(true))
            {
                if (control.name == controlName) return control;
            }
            return null;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible)
            {
                ResetDisplayState();
            }
        }
    }
}
