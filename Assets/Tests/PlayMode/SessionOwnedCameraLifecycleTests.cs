using System.Collections;
using System.Reflection;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SessionOwnedCameraLifecycleTests
    {
        private Scene sliceScene;
        private VerticalSliceSession2D session;
        private float previousScale;
        private static readonly FieldInfo CameraField = typeof(VerticalSliceSession2D).GetField("camera", BindingFlags.Instance | BindingFlags.NonPublic);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousScale = Time.timeScale; Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Additive);
            sliceScene = SceneManager.GetSceneByName("VerticalSlice");
            foreach (GameObject root in sliceScene.GetRootGameObjects())
            {
                session = root.GetComponentInChildren<VerticalSliceSession2D>();
                if (session != null) break;
            }
            Assert.That(session, Is.Not.Null);
            yield return null; // let autoStart (Start) run before taking ownership of the session
            session.Stop();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (session != null) session.Stop();
            if (sliceScene.IsValid() && sliceScene.isLoaded) yield return SceneManager.UnloadSceneAsync(sliceScene);
            Time.timeScale = previousScale;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ImmediateRestartReplacesOwnedCameraBeforeDeferredDestruction()
        {
            Camera external = (Camera)CameraField.GetValue(session);
            external.gameObject.SetActive(false);
            CameraField.SetValue(session, null);
            Assert.That(session.TryBegin(), Is.True);
            Camera first = (Camera)CameraField.GetValue(session);
            Assert.That(first, Is.Not.Null);
            Assert.That(first.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
            session.Stop();
            Assert.That(first.gameObject.activeSelf, Is.False, "Stopped camera must stop rendering immediately.");
            Assert.That(session.TryBegin(), Is.True);
            Camera replacement = (Camera)CameraField.GetValue(session);
            Assert.That(replacement, Is.Not.SameAs(first));
            yield return null;
            Assert.That(first == null, Is.True);
            Assert.That(replacement != null && replacement.isActiveAndEnabled, Is.True);
            Assert.That(session.IsRunning, Is.True);
            Assert.That(replacement.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
        }

        [UnityTest]
        public IEnumerator StopPreservesExternallyComposedCameraForRestart()
        {
            Camera external = (Camera)CameraField.GetValue(session);
            Assert.That(external, Is.Not.Null);
            Assert.That(session.TryBegin(), Is.True);
            session.Stop();
            Assert.That(CameraField.GetValue(session), Is.SameAs(external));
            Assert.That(external.gameObject.activeSelf, Is.True);
            Assert.That(session.TryBegin(), Is.True);
            yield return null;
            Assert.That(external != null && external.isActiveAndEnabled, Is.True);
            Assert.That(CameraField.GetValue(session), Is.SameAs(external));
            Assert.That(external.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
        }
    }
}
