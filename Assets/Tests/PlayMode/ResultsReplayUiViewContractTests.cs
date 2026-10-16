using HuntrX.Presentation.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class ResultsReplayUiViewContractTests
    {
        private GameObject root;
        private ResultsReplayUiView view;
        private Text outcomeTitle;
        private Text outcomeDetails;
        private Text honmoonScore;
        private Text fansScore;
        private Slider funSlider;
        private Text funText;
        private Slider difficultySlider;
        private Text difficultyText;
        private InputField commentsInput;
        private Button submitSurveyBtn;
        private Text surveyStatus;
        private Button replayBtn;
        private Button mainMenuBtn;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("ResultsReplay", typeof(RectTransform));
            outcomeTitle = Add<Text>("outcomeTitleText");
            outcomeDetails = Add<Text>("outcomeDetailsText");
            honmoonScore = Add<Text>("honmoonScoreText");
            fansScore = Add<Text>("fansRescuedScoreText");
            funSlider = Add<Slider>("funRatingSlider");
            funText = Add<Text>("funRatingText");
            difficultySlider = Add<Slider>("difficultyRatingSlider");
            difficultyText = Add<Text>("difficultyRatingText");
            commentsInput = Add<InputField>("commentsInputField");
            submitSurveyBtn = Add<Button>("submitSurveyButton");
            surveyStatus = Add<Text>("surveyStatusText");
            replayBtn = Add<Button>("replayButton");
            mainMenuBtn = Add<Button>("mainMenuButton");

            view = root.AddComponent<ResultsReplayUiView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        private T Add<T>(string name) where T : Component
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<T>();
        }

        [Test]
        public void AwakeInitializesSliderRangesAndDefaults()
        {
            Assert.That(funSlider.minValue, Is.EqualTo(1));
            Assert.That(funSlider.maxValue, Is.EqualTo(5));
            Assert.That(funSlider.wholeNumbers, Is.True);
            Assert.That(view.CurrentFunRating, Is.EqualTo(3));

            Assert.That(difficultySlider.minValue, Is.EqualTo(1));
            Assert.That(difficultySlider.maxValue, Is.EqualTo(5));
            Assert.That(difficultySlider.wholeNumbers, Is.True);
            Assert.That(view.CurrentDifficultyRating, Is.EqualTo(3));
        }

        [Test]
        public void DisplayResultsFormatsOutcomeTextsAndScores()
        {
            view.DisplayResults("Show Concluído!", "Encontros superados com sucesso", 150, 12, 12);
            Assert.That(outcomeTitle.text, Is.EqualTo("Show Concluído!"));
            Assert.That(outcomeDetails.text, Is.EqualTo("Encontros superados com sucesso"));
            Assert.That(honmoonScore.text, Is.EqualTo("Honmoon: 150"));
            Assert.That(fansScore.text, Is.EqualTo("Fãs Protegidos: 12/12"));

            view.DisplayResults("Vitória", "Detalhe", 80, 5, 0);
            Assert.That(fansScore.text, Is.EqualTo("Fãs Protegidos: 5"));
        }

        [Test]
        public void ChangingSliderValuesUpdatesRatingLabelsAndCurrentRatings()
        {
            funSlider.value = 5;
            Assert.That(funText.text, Is.EqualTo("5 / 5"));
            Assert.That(view.CurrentFunRating, Is.EqualTo(5));

            difficultySlider.value = 2;
            Assert.That(difficultyText.text, Is.EqualTo("2 / 5"));
            Assert.That(view.CurrentDifficultyRating, Is.EqualTo(2));
        }

        [Test]
        public void ClickingSubmitSurveyEmitsSurveySubmittedEvent()
        {
            int submittedFun = 0;
            int submittedDiff = 0;
            string submittedComments = null;

            view.SurveySubmitted += (f, d, c) =>
            {
                submittedFun = f;
                submittedDiff = d;
                submittedComments = c;
            };

            funSlider.value = 4;
            difficultySlider.value = 3;
            commentsInput.text = "Ótimo combate e ritmo!";

            submitSurveyBtn.onClick.Invoke();

            Assert.That(submittedFun, Is.EqualTo(4));
            Assert.That(submittedDiff, Is.EqualTo(3));
            Assert.That(submittedComments, Is.EqualTo("Ótimo combate e ritmo!"));
        }

        [Test]
        public void ClickingReplayAndMainMenuEmitsExpectedEvents()
        {
            int replayCount = 0;
            int mainMenuCount = 0;

            view.ReplayRequested += () => replayCount++;
            view.MainMenuRequested += () => mainMenuCount++;

            replayBtn.onClick.Invoke();
            Assert.That(replayCount, Is.EqualTo(1));

            mainMenuBtn.onClick.Invoke();
            Assert.That(mainMenuCount, Is.EqualTo(1));
        }

        [Test]
        public void SetSurveyStatusConfiguresMessageAndTextColor()
        {
            view.SetSurveyStatus("Avaliação salva", isError: false);
            Assert.That(surveyStatus.text, Is.EqualTo("Avaliação salva"));
            Assert.That(surveyStatus.color, Is.EqualTo(Color.green));

            view.SetSurveyStatus("Não foi possível salvar", isError: true);
            Assert.That(surveyStatus.text, Is.EqualTo("Não foi possível salvar"));
            Assert.That(surveyStatus.color, Is.EqualTo(Color.red));
        }

        [Test]
        public void SetVisibleControlsGameObjectActive()
        {
            view.SetVisible(false);
            Assert.That(view.gameObject.activeSelf, Is.False);
            view.SetVisible(true);
            Assert.That(view.gameObject.activeSelf, Is.True);
        }
    }
}
