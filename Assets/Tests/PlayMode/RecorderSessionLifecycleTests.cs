using System.Collections;
using System.IO;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class RecorderSessionLifecycleTests
    {
        private Scene slice;
        private string firstPath, secondPath;
        [UnitySetUp] public IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
#endif
            slice = SceneManager.GetActiveScene();
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            SceneManager.SetActiveScene(SceneManager.CreateScene("RecorderSessionCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
            if (firstPath != null && File.Exists(firstPath)) File.Delete(firstPath);
            if (secondPath != null && File.Exists(secondPath)) File.Delete(secondPath);
        }
        [UnityTest] public IEnumerator NewSessionResetsObservationAndKeepsPreviousSavedReport()
        {
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            var recorder = Object.FindFirstObjectByType<SlicePlaytestRecorder2D>();
            var previous = recorder.Report;
            Assert.That(previous, Is.Not.Null);
            Assert.That(recorder.TrySubmitFeedback(2, 4, "first session"), Is.True);
            Assert.That(recorder.TrySaveReport(out firstPath), Is.True);
            string saved = File.ReadAllText(firstPath);
            previous.DurationSeconds = 45;
            previous.JumpInputs = 7;
            session.Stop();
            yield return null;
            Assert.That(session.TryBegin(), Is.True);
            Assert.That(recorder.Report, Is.Not.SameAs(previous));
            Assert.That(recorder.Report.DurationSeconds, Is.Zero);
            Assert.That(recorder.Report.JumpInputs, Is.Zero);
            Assert.That(recorder.Report.VisitedBeats, Is.EqualTo(1));
            Assert.That(recorder.Report.HumanFunRating, Is.EqualTo(-1));
            Assert.That(recorder.Report.HumanComments, Is.Empty);
            Assert.That(recorder.TrySaveReport(out secondPath), Is.True);
            Assert.That(secondPath, Is.Not.EqualTo(firstPath));
            Assert.That(File.ReadAllText(firstPath), Is.EqualTo(saved));
        }
        [UnityTest] public IEnumerator RecorderDisableEnableKeepsCurrentSessionAndReportIdentity()
        {
            var recorder = Object.FindFirstObjectByType<SlicePlaytestRecorder2D>();
            var previous = recorder.Report;
            Assert.That(recorder.TrySubmitFeedback(4, 3, "same session"), Is.True);
            Assert.That(recorder.TrySaveReport(out firstPath), Is.True);
            int visited = previous.VisitedBeats;
            recorder.enabled = false;
            yield return null;
            recorder.enabled = true;
            Assert.That(recorder.Report, Is.SameAs(previous));
            Assert.That(recorder.Report.VisitedBeats, Is.EqualTo(visited));
            Assert.That(recorder.Report.HumanComments, Is.EqualTo("same session"));
            Assert.That(recorder.TrySaveReport(out secondPath), Is.True);
            Assert.That(secondPath, Is.EqualTo(firstPath));
        }
    }
}
