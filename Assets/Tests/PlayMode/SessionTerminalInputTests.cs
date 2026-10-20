using System.Collections;
using System.Reflection;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SessionTerminalInputTests
    {
        private Scene slice;
        private VerticalSliceSession2D session;
        private float previousScale;
        private static readonly FieldInfo BeatsField = typeof(VerticalSliceSession2D).GetField("beats", BindingFlags.Instance | BindingFlags.NonPublic);

        [UnitySetUp]
        public IEnumerator Load()
        {
            previousScale = Time.timeScale; Time.timeScale = 1f;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
#endif
            slice = SceneManager.GetActiveScene();
            session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            Assert.That(session, Is.Not.Null);
            yield return null; // let autoStart (Start) run before taking ownership of the session
            session.Stop();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            if (session != null) session.Stop();
            SceneManager.SetActiveScene(SceneManager.CreateScene("SessionTerminalInputCleanup"));
            if (slice.IsValid() && slice.isLoaded) yield return SceneManager.UnloadSceneAsync(slice);
            Time.timeScale = previousScale;
        }

        [UnityTest]
        public IEnumerator CompletionSuspendsGameplayInputAndRetainsResults()
        {
            var authored = (SliceBeat2D[])BeatsField.GetValue(session);
            Assert.That(authored[0], Is.TypeOf<SliceAirplaneBeat2D>());
            BeatsField.SetValue(session, new[] { authored[0] });
            Assert.That(session.TryBegin(), Is.True);
            var manager = session.Manager;
            var performance = session.Performance;
            var stage = session.Stage;
            yield return ReachAirplaneExit();

            Assert.That(session.IsComplete, Is.True);
            Assert.That(session.Input.isActiveAndEnabled, Is.False, "Completed sessions must stop accepting gameplay actions.");
            Assert.That(session.Manager, Is.SameAs(manager));
            Assert.That(manager.isActiveAndEnabled, Is.True);
            Assert.That(session.Stage, Is.SameAs(stage));
            Assert.That(session.Performance, Is.SameAs(performance));
            Assert.That(performance.IsRunning, Is.False);
            yield return AssertSubmittedActionsIgnored();
        }

        [UnityTest]
        public IEnumerator LaterBeatFailureSuspendsGameplayInputWithoutDestroyingSessionState()
        {
            var authored = (SliceBeat2D[])BeatsField.GetValue(session);
            Assert.That(authored[0], Is.TypeOf<SliceAirplaneBeat2D>());
            Assert.That(authored[1], Is.TypeOf<SliceRamenBeat2D>());
            // Session composition validates that beats exist, but this later beat cannot initialize.
            typeof(SliceRamenBeat2D).GetField("pickupPrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(authored[1], null);
            BeatsField.SetValue(session, new[] { authored[0], authored[1] });
            Assert.That(session.TryBegin(), Is.True);
            var manager = session.Manager;
            var performance = session.Performance;
            LogAssert.Expect(LogType.Error, "Slice beat failed: " + authored[1].Title);
            yield return ReachAirplaneExit();

            Assert.That(session.IsComplete, Is.False);
            Assert.That(session.Input.isActiveAndEnabled, Is.False, "Failed transitions must stop accepting gameplay actions.");
            Assert.That(session.Manager, Is.SameAs(manager));
            Assert.That(manager.isActiveAndEnabled, Is.True);
            Assert.That(session.Performance, Is.SameAs(performance));
            yield return AssertSubmittedActionsIgnored();
        }

        private IEnumerator ReachAirplaneExit()
        {
            yield return NarrativeTestFlow.CompleteCurrentDialogue();
            session.ClearEnemies();
            var body = session.Manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            body.position = session.Stage.EndPoint.position;
            body.linearVelocity = Vector2.zero;
            yield return new WaitForFixedUpdate();
            for (int frame = 0; frame < 10 && session.IsRunning; frame++) yield return null;
            Assert.That(session.IsRunning, Is.False, "The real airplane beat should cross its terminal boundary.");
        }

        private IEnumerator AssertSubmittedActionsIgnored()
        {
            var actor = session.Manager.ActiveCharacter;
            var attack = actor.GetComponent<AttackController2D>();
            int requests = 0;
            session.Input.ActionsRequested += _ => requests++;
            session.Input.SubmitInput(new SliceInputFrame { Move = Vector2.right, Attack = true, Switch = true });
            yield return null;
            Assert.That(requests, Is.Zero);
            Assert.That(session.Manager.ActiveCharacter, Is.SameAs(actor));
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Idle));
        }
    }
}
