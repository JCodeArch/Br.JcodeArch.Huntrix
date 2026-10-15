using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Landscape Mobile HUD view implementing Figma frame 2:71.
    /// Exposes read-only telemetry displays (Health, Honmoon, Special, Food, Fans, Beat)
    /// and 9 distinct mobile action button bindings plus movement touch zone.
    /// </summary>
    public sealed class MobileHudUiView : MonoBehaviour
    {
        [Header("Status & Resources (Top Left)")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider honmoonBar;
        [SerializeField] private Text honmoonText;

        [Header("Character Switch (Top Center)")]
        [SerializeField] private Text activeCharacterText;
        [SerializeField] private Button switchButton;

        [Header("Pause / Options (Top Right)")]
        [SerializeField] private Button pauseButton;

        [Header("Progress & Beat Information")]
        [SerializeField] private Text beatTitleText;
        [SerializeField] private Text fansRescuedText;
        [SerializeField] private Text foodCountText;

        [Header("Action Buttons (Bottom Right)")]
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button dashButton;
        [SerializeField] private Button parryButton;
        [SerializeField] private Button specialButton;
        [SerializeField] private Button rescueButton;
        [SerializeField] private Button foodSelfButton;
        [SerializeField] private Button foodFanButton;

        [Header("Special Indicator")]
        [SerializeField] private Slider specialChargeBar;
        [SerializeField] private Image specialReadyGlow;

        // Action Events
        public event Action JumpPressed;
        public event Action AttackPressed;
        public event Action DashPressed;
        public event Action ParryPressed;
        public event Action SwitchPressed;
        public event Action SpecialPressed;
        public event Action RescuePressed;
        public event Action FoodSelfPressed;
        public event Action FoodFanPressed;
        public event Action PausePressed;

        private void Awake()
        {
            if (healthBar == null) healthBar = FindControl<Slider>("healthBar");
            if (healthText == null) healthText = FindControl<Text>("healthText");
            if (honmoonBar == null) honmoonBar = FindControl<Slider>("honmoonBar");
            if (honmoonText == null) honmoonText = FindControl<Text>("honmoonText");
            if (activeCharacterText == null) activeCharacterText = FindControl<Text>("activeCharacterText");
            if (switchButton == null) switchButton = FindControl<Button>("switchButton");
            if (pauseButton == null) pauseButton = FindControl<Button>("pauseButton");
            if (beatTitleText == null) beatTitleText = FindControl<Text>("beatTitleText");
            if (fansRescuedText == null) fansRescuedText = FindControl<Text>("fansRescuedText");
            if (foodCountText == null) foodCountText = FindControl<Text>("foodCountText");
            if (jumpButton == null) jumpButton = FindControl<Button>("jumpButton");
            if (attackButton == null) attackButton = FindControl<Button>("attackButton");
            if (dashButton == null) dashButton = FindControl<Button>("dashButton");
            if (parryButton == null) parryButton = FindControl<Button>("parryButton");
            if (specialButton == null) specialButton = FindControl<Button>("specialButton");
            if (rescueButton == null) rescueButton = FindControl<Button>("rescueButton");
            if (foodSelfButton == null) foodSelfButton = FindControl<Button>("foodSelfButton");
            if (foodFanButton == null) foodFanButton = FindControl<Button>("foodFanButton");
            if (specialChargeBar == null) specialChargeBar = FindControl<Slider>("specialChargeBar");
            if (specialReadyGlow == null) specialReadyGlow = FindControl<Image>("specialReadyGlow");
            BindButton(jumpButton, () => JumpPressed?.Invoke());
            BindButton(attackButton, () => AttackPressed?.Invoke());
            BindButton(dashButton, () => DashPressed?.Invoke());
            BindButton(parryButton, () => ParryPressed?.Invoke());
            BindButton(switchButton, () => SwitchPressed?.Invoke());
            BindButton(specialButton, () => SpecialPressed?.Invoke());
            BindButton(rescueButton, () => RescuePressed?.Invoke());
            BindButton(foodSelfButton, () => FoodSelfPressed?.Invoke());
            BindButton(foodFanButton, () => FoodFanPressed?.Invoke());
            BindButton(pauseButton, () => PausePressed?.Invoke());
        }

        private void BindButton(Button button, Action action)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => action());
            }
        }

        public void RenderState(SliceUiState2D state)
        {
            // Health
            if (healthBar != null)
            {
                healthBar.maxValue = Mathf.Max(1, state.HealthMaximum);
                healthBar.value = state.HealthCurrent;
            }

            if (healthText != null)
            {
                healthText.text = $"{state.HealthCurrent} / {state.HealthMaximum}";
            }

            // Honmoon
            if (honmoonBar != null)
            {
                honmoonBar.value = Mathf.Clamp(state.HonmoonCurrent, 0, 100);
            }

            if (honmoonText != null)
            {
                honmoonText.text = state.HonmoonCurrent.ToString();
            }

            // Character
            if (activeCharacterText != null)
            {
                activeCharacterText.text = state.ActiveCharacter;
            }

            // Beat & Progress
            if (beatTitleText != null)
            {
                beatTitleText.text = state.BeatTitle;
            }

            if (fansRescuedText != null)
            {
                fansRescuedText.text = state.FansTotal > 0
                    ? $"Fãs: {state.FansRescued}/{state.FansTotal}"
                    : $"Fãs: {state.FansRescued}";
            }

            if (foodCountText != null)
            {
                foodCountText.text = $"Comida: {state.FoodCount}";
            }

            // Special
            if (specialChargeBar != null)
            {
                specialChargeBar.value = Mathf.Clamp01(state.SpecialCharge);
            }

            if (specialReadyGlow != null)
            {
                specialReadyGlow.enabled = state.SpecialReady;
            }

            if (specialButton != null)
            {
                specialButton.interactable = state.SpecialReady;
            }
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
