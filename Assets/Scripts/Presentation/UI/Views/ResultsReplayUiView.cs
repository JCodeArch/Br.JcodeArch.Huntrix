using System;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI.Views
{
    /// <summary>
    /// Results and replay view implementing Figma frame 2:150.
    /// Presents actual run outcome, playtest rating form (fun/difficulty 1-5, comments),
    /// feedback save status, and actions to replay or return to main menu.
    /// </summary>
    public sealed class ResultsReplayUiView : MonoBehaviour
    {
        [Header("Outcome Summary")]
        [SerializeField] private Text outcomeTitleText;
        [SerializeField] private Text outcomeDetailsText;
        [SerializeField] private Text honmoonScoreText;
        [SerializeField] private Text fansRescuedScoreText;

        [Header("Playtest Survey (1-5 scale)")]
        [SerializeField] private Slider funRatingSlider;
        [SerializeField] private Text funRatingText;
        [SerializeField] private Slider difficultyRatingSlider;
        [SerializeField] private Text difficultyRatingText;
        [SerializeField] private InputField commentsInputField;
        [SerializeField] private Button submitSurveyButton;
        [SerializeField] private Text surveyStatusText;

        [Header("Flow Actions")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button mainMenuButton;

        public event Action<int, int, string> SurveySubmitted;
        public event Action ReplayRequested;
        public event Action MainMenuRequested;

        public int CurrentFunRating => funRatingSlider != null ? Mathf.RoundToInt(funRatingSlider.value) : 3;
        public int CurrentDifficultyRating => difficultyRatingSlider != null ? Mathf.RoundToInt(difficultyRatingSlider.value) : 3;
        public string CurrentComments => commentsInputField != null ? commentsInputField.text : string.Empty;

        private void Awake()
        {
            if (outcomeTitleText == null) outcomeTitleText = FindControl<Text>("outcomeTitleText");
            if (outcomeDetailsText == null) outcomeDetailsText = FindControl<Text>("outcomeDetailsText");
            if (honmoonScoreText == null) honmoonScoreText = FindControl<Text>("honmoonScoreText");
            if (fansRescuedScoreText == null) fansRescuedScoreText = FindControl<Text>("fansRescuedScoreText");
            if (funRatingSlider == null) funRatingSlider = FindControl<Slider>("funRatingSlider");
            if (funRatingText == null) funRatingText = FindControl<Text>("funRatingText");
            if (difficultyRatingSlider == null) difficultyRatingSlider = FindControl<Slider>("difficultyRatingSlider");
            if (difficultyRatingText == null) difficultyRatingText = FindControl<Text>("difficultyRatingText");
            if (commentsInputField == null) commentsInputField = FindControl<InputField>("commentsInputField");
            if (submitSurveyButton == null) submitSurveyButton = FindControl<Button>("submitSurveyButton");
            if (surveyStatusText == null) surveyStatusText = FindControl<Text>("surveyStatusText");
            if (replayButton == null) replayButton = FindControl<Button>("replayButton");
            if (mainMenuButton == null) mainMenuButton = FindControl<Button>("mainMenuButton");
            if (funRatingSlider != null)
            {
                funRatingSlider.minValue = 1;
                funRatingSlider.maxValue = 5;
                funRatingSlider.wholeNumbers = true;
                funRatingSlider.value = 3;
                funRatingSlider.onValueChanged.AddListener(val =>
                {
                    if (funRatingText != null) funRatingText.text = $"{Mathf.RoundToInt(val)} / 5";
                });
            }

            if (difficultyRatingSlider != null)
            {
                difficultyRatingSlider.minValue = 1;
                difficultyRatingSlider.maxValue = 5;
                difficultyRatingSlider.wholeNumbers = true;
                difficultyRatingSlider.value = 3;
                difficultyRatingSlider.onValueChanged.AddListener(val =>
                {
                    if (difficultyRatingText != null) difficultyRatingText.text = $"{Mathf.RoundToInt(val)} / 5";
                });
            }

            if (submitSurveyButton != null)
            {
                submitSurveyButton.onClick.AddListener(() =>
                {
                    SurveySubmitted?.Invoke(CurrentFunRating, CurrentDifficultyRating, CurrentComments);
                });
            }

            if (replayButton != null)
            {
                replayButton.onClick.AddListener(() => ReplayRequested?.Invoke());
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(() => MainMenuRequested?.Invoke());
            }
        }

        public void DisplayResults(string title, string details, int honmoon, int fansRescued, int totalFans)
        {
            if (outcomeTitleText != null) outcomeTitleText.text = title;
            if (outcomeDetailsText != null) outcomeDetailsText.text = details;
            if (honmoonScoreText != null) honmoonScoreText.text = $"Honmoon: {honmoon}";
            if (fansRescuedScoreText != null)
            {
                fansRescuedScoreText.text = totalFans > 0
                    ? $"Fãs Protegidos: {fansRescued}/{totalFans}"
                    : $"Fãs Protegidos: {fansRescued}";
            }
        }

        public void SetSurveyStatus(string status, bool isError = false)
        {
            if (surveyStatusText != null)
            {
                surveyStatusText.text = status;
                surveyStatusText.color = isError ? Color.red : Color.green;
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
