using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace HuntrX.Tests.PlayMode
{
    public sealed class SlicePlaytestRecorderPersistenceTests
    {
        private readonly List<GameObject> instances = new List<GameObject>();
        private readonly List<string> paths = new List<string>();
        private SlicePlaytestRecorder2D CreateRecorder(bool withReport = true)
        {
            var instance = new GameObject("recorder persistence test");
            instances.Add(instance);
            var recorder = instance.AddComponent<SlicePlaytestRecorder2D>();
            if (withReport) typeof(SlicePlaytestRecorder2D).GetProperty("Report").SetValue(recorder,
                new SlicePlaytestReport { StartedAtUtc = DateTime.UtcNow.ToString("O") });
            return recorder;
        }
        [TearDown] public void TearDown()
        {
            foreach (var instance in instances) if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            foreach (var path in paths) { if (File.Exists(path)) File.Delete(path); if (Directory.Exists(path)) Directory.Delete(path); }
            instances.Clear(); paths.Clear();
        }
        [Test] public void RepeatedSaveUpdatesSameFileAndSurvivesDisable()
        {
            var recorder = CreateRecorder();
            Assert.That(recorder.TrySubmitFeedback(2, 3, "first"), Is.True);
            Assert.That(recorder.TrySaveReport(out var first), Is.True); paths.Add(first);
            recorder.gameObject.SetActive(false); recorder.gameObject.SetActive(true);
            Assert.That(recorder.TrySubmitFeedback(5, 1, "updated"), Is.True);
            Assert.That(recorder.TrySaveReport(out var second), Is.True); paths.Add(second);
            Assert.That(second, Is.EqualTo(first));
            var saved = JsonUtility.FromJson<SlicePlaytestReport>(File.ReadAllText(first));
            Assert.That(saved.HumanFunRating, Is.EqualTo(5)); Assert.That(saved.HumanComments, Is.EqualTo("updated"));
        }
        [Test] public void NewSessionRecorderUsesSeparateFile()
        {
            Assert.That(CreateRecorder().TrySaveReport(out var first), Is.True); paths.Add(first);
            Assert.That(CreateRecorder().TrySaveReport(out var second), Is.True); paths.Add(second);
            Assert.That(second, Is.Not.EqualTo(first)); Assert.That(File.Exists(first), Is.True);
        }
        [Test] public void FailedSavePreservesFeedbackAndRetriesSamePath()
        {
            var recorder = CreateRecorder();
            string blocked = Path.Combine(Application.persistentDataPath, "Playtests", "slice-test-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(blocked); paths.Add(blocked);
            typeof(SlicePlaytestRecorder2D).GetField("reportPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(recorder, blocked);
            Assert.That(recorder.TrySubmitFeedback(4, 2, "keep on failure"), Is.True);
            LogAssert.Expect(LogType.Exception, new Regex(".*"));
            Assert.That(recorder.TrySaveReport(out var failed), Is.False); Assert.That(failed, Is.Empty);
            Assert.That(recorder.Report.HumanComments, Is.EqualTo("keep on failure"));
            Directory.Delete(blocked);
            Assert.That(recorder.TrySaveReport(out var saved), Is.True); Assert.That(saved, Is.EqualTo(blocked));
            Assert.That(JsonUtility.FromJson<SlicePlaytestReport>(File.ReadAllText(saved)).HumanComments, Is.EqualTo("keep on failure"));
        }
        [Test] public void MissingReportCannotSave()
        {
            Assert.That(CreateRecorder(false).TrySaveReport(out var path), Is.False); Assert.That(path, Is.Empty);
        }
        [Test] public void FailedUpdatePreservesPreviousFileAndCanRetry()
        {
            var recorder = CreateRecorder();
            Assert.That(recorder.TrySubmitFeedback(2, 3, "previous"), Is.True);
            Assert.That(recorder.TrySaveReport(out var path), Is.True); paths.Add(path);
            string previousJson = File.ReadAllText(path);
            string temporaryPath = path + ".tmp";
            Directory.CreateDirectory(temporaryPath); paths.Add(temporaryPath);
            Assert.That(recorder.TrySubmitFeedback(5, 1, "retry update"), Is.True);
            LogAssert.Expect(LogType.Exception, new Regex(".*"));
            Assert.That(recorder.TrySaveReport(out var failed), Is.False); Assert.That(failed, Is.Empty);
            Assert.That(File.ReadAllText(path), Is.EqualTo(previousJson));
            Assert.That(recorder.Report.HumanComments, Is.EqualTo("retry update"));
            Directory.Delete(temporaryPath);
            Assert.That(recorder.TrySaveReport(out var retried), Is.True); Assert.That(retried, Is.EqualTo(path));
            Assert.That(JsonUtility.FromJson<SlicePlaytestReport>(File.ReadAllText(path)).HumanComments, Is.EqualTo("retry update"));
            Assert.That(File.Exists(temporaryPath), Is.False);
        }
    }
}
