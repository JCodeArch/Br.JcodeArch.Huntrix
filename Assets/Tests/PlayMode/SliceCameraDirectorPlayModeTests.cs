using System.Collections;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SliceCameraDirectorPlayModeTests
    {
        private Scene slice;
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
            yield return NarrativeTestFlow.CompleteCurrentDialogue();
        }
        [UnityTearDown] public IEnumerator Unload()
        {
            SceneManager.SetActiveScene(SceneManager.CreateScene("CameraCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }

        private static SliceCameraDirector2D DirectorOf(VerticalSliceSession2D session)
        {
            // The slot is a root separate from the session; resolve through the real manager/slot.
            var director = session.Manager.GetComponent<SliceCameraDirector2D>();
            Assert.That(director, Is.Not.Null);
            Assert.That(session.Manager.GetComponents<SliceCameraDirector2D>().Length, Is.EqualTo(1));
            Assert.That(director.View, Is.Not.Null);
            return director;
        }

        private static void AssertInside(Rect b, SliceCameraDirector2D d)
        {
            float halfH = d.View.orthographicSize, halfW = halfH * d.View.aspect;
            Vector3 p = d.View.transform.position;
            if (b.width > 2f * halfW) { Assert.That(p.x - halfW, Is.GreaterThanOrEqualTo(b.xMin - 0.01f)); Assert.That(p.x + halfW, Is.LessThanOrEqualTo(b.xMax + 0.01f)); }
            else Assert.That(p.x, Is.EqualTo(b.center.x).Within(0.01f));
            if (b.height > 2f * halfH) { Assert.That(p.y - halfH, Is.GreaterThanOrEqualTo(b.yMin - 0.01f)); Assert.That(p.y + halfH, Is.LessThanOrEqualTo(b.yMax + 0.01f)); }
            else Assert.That(p.y, Is.EqualTo(b.center.y).Within(0.01f));
        }

        [UnityTest] public IEnumerator DirectorOwnsTheSessionCameraAndStaysInsideStageAtBothEdges()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var director = DirectorOf(session);
            session.Stage.Definition.TryGetWorldBounds(out Rect b);
            float size = director.View.orthographicSize;
            var body = session.Manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            foreach (float x in new[] { b.xMin - 20f, b.xMax + 20f })
            {
                body.position = new Vector2(x, body.position.y);
                body.linearVelocity = Vector2.zero;
                for (int i = 0; i < 90; i++) yield return null;
                AssertInside(b, director);
            }
            Assert.That(director.View.orthographicSize, Is.EqualTo(size), "The director must not rewrite the projection size.");
        }

        [UnityTest] public IEnumerator ZoomOutAtTheEdgeYieldsAValidPoseOnTheFirstFrame()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var director = DirectorOf(session);
            session.Stage.Definition.TryGetWorldBounds(out Rect b);
            var body = session.Manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            body.position = new Vector2(b.xMax + 20f, body.position.y);
            body.linearVelocity = Vector2.zero;
            for (int i = 0; i < 90; i++) yield return null;
            // Viewport grows (aspect/zoom change) while the smoothed pose still sits at the old edge.
            director.View.orthographicSize *= 1.5f;
            yield return null;
            AssertInside(b, director);
        }

        [UnityTest] public IEnumerator RestartUsesTheNewDirectorAndCameraAndSnapsToTheNewActor()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var oldDirector = DirectorOf(session);
            session.Stop();
            yield return null;
            Assert.That(session.TryBegin(), Is.True);
            yield return NarrativeTestFlow.CompleteCurrentDialogue();
            yield return null; yield return null;
            var director = DirectorOf(session);
            Assert.That(director, Is.Not.SameAs(oldDirector));
            Vector2 actor = session.Manager.ActiveCharacter.transform.position;
            session.Stage.Definition.TryGetWorldBounds(out Rect b);
            AssertInside(b, director);
            Assert.That(Mathf.Abs(director.View.transform.position.x - actor.x), Is.LessThan(b.width * 0.5f));
            Assert.That(director.View.transform.position.z, Is.EqualTo(-10f));
        }
    }
}