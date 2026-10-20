using System.Collections;
using HuntrX.Gameplay.Narrative;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class NarrativeGameplayProtectionTests
    {
        private Scene slice;
        private float previous;
        [UnitySetUp] public IEnumerator SetUp()
        {
            previous = Time.timeScale;
            yield break;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(slice.IsValid() && slice.isLoaded)
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("NarrativeProtectionCleanup"));
                yield return SceneManager.UnloadSceneAsync(slice);
            }
            Time.timeScale = previous;
        }
        [UnityTest]
        public IEnumerator IntroBlocksCombatUntilExplicitCompletion()
        {
            {
                #if UNITY_EDITOR
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
                #else
                yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
                #endif
                slice = SceneManager.GetActiveScene();
                var narrative = Object.FindAnyObjectByType<SliceNarrativeComposition2D>();
                var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
                for (int frame = 0; frame < 30 && (narrative.Presenter == null || !narrative.Presenter.IsPresenting); frame++) yield return null;
                Assert.That(narrative.Presenter, Is.Not.Null);
                Assert.That(narrative.Presenter.IsPresenting, Is.True);
                Assert.That(Time.timeScale, Is.Zero, "Combat must stop while the intro covers the playfield.");
                Assert.That(session.Input.enabled, Is.False);
                var originalActor = session.Manager.ActiveCharacter;
                float health = originalActor.CurrentHealth;
                var actor = originalActor.transform;
                Vector3 position = actor.position;
                session.Input.SubmitInput(new SliceInputFrame { Move = Vector2.right, Attack = true, Switch = true });
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(actor.position, Is.EqualTo(position));
                Assert.That(session.Manager.ActiveCharacter, Is.SameAs(originalActor));
                Assert.That(originalActor.CurrentHealth, Is.EqualTo(health));
                var hud = Object.FindAnyObjectByType<SliceHud2D>();
                hud.SetOptionsOpen(true);
                Assert.That(narrative.Presenter.IsUiBlocked, Is.True);
                hud.SetOptionsOpen(false);
                Assert.That(Time.timeScale, Is.Zero, "Closing options cannot release an active dialogue.");
                Assert.That(session.Input.enabled, Is.False);
                Assert.That(narrative.Presenter.IsUiBlocked, Is.False);
                hud.SetOptionsOpen(true);
                for (int advance = 0; advance < 64 && narrative.Presenter.IsPresenting; advance++) narrative.Presenter.Advance();
                Assert.That(narrative.Presenter.IsPresenting, Is.False);
                yield return null;
                Assert.That(Time.timeScale, Is.Zero, "Finishing dialogue cannot release open options.");
                Assert.That(session.Input.enabled, Is.False);
                hud.SetOptionsOpen(false);
                Assert.That(Time.timeScale, Is.GreaterThan(0));
                Assert.That(session.Input.enabled, Is.True);
            }
        }
    }
}
