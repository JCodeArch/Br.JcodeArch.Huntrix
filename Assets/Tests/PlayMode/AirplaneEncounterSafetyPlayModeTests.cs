using System.Collections;
using System.Reflection;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AirplaneEncounterSafetyPlayModeTests
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
            SceneManager.SetActiveScene(SceneManager.CreateScene("AirplaneSafetyCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }

        private static void SetHealth(DamageReceiver2D receiver, float value) =>
            typeof(DamageReceiver2D).GetField("<CurrentHealth>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(receiver, value);

        [UnityTest] public IEnumerator OpeningViewportIsEmptyOfEnemiesAndAHeroineStandingStillIsNotHurt()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var cam = session.Manager.GetComponent<SliceCameraDirector2D>().View;
            var actor = session.Manager.ActiveCharacter;
            float halfWidth = cam.orthographicSize * Mathf.Max(cam.aspect, 16f / 9f); // headless aspect may be narrower than a real 16:9 player
            foreach (var enemy in Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None))
                Assert.That(Mathf.Abs(enemy.transform.position.x - actor.transform.position.x), Is.GreaterThan(halfWidth + 1f),
                    enemy.name + " starts inside or beside the opening viewport at " + enemy.transform.position);

            float health = actor.CurrentHealth;
            for (float t = 0f; t < 8f; t += Time.deltaTime) yield return null;
            Assert.That(actor.CurrentHealth, Is.EqualTo(health), "Standing still at the start must not cost health.");
        }

        [UnityTest] public IEnumerator EnemiesStandDownForTheBreathingRoomAfterARespawn()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var actor = session.Manager.ActiveCharacter;
            EnemyAgent2D enemy = null;
            foreach (var candidate in Object.FindObjectsByType<EnemyAgent2D>(FindObjectsSortMode.None))
                if (candidate.name.StartsWith("FlightCrew_")) { enemy = candidate; break; }
            Assert.That(enemy, Is.Not.Null);

            bool died = false;
            actor.Died += _ => died = true;
            SetHealth(actor, 1f);
            var enemyBody = enemy.GetComponent<Rigidbody2D>();
            for (float t = 0f; t < 6f && !died; t += Time.deltaTime)
            {
                enemyBody.position = (Vector2)actor.transform.position + new Vector2(0.9f, 0f);
                yield return null;
            }
            Assert.That(died, Is.True, "Setup: the adjacent enemy should defeat a 1 HP heroine.");

            for (int i = 0; i < 60 && !(session.Manager.ActiveCharacter != null && session.Manager.ActiveCharacter.IsAlive); i++) yield return null;
            var revived = session.Manager.ActiveCharacter;
            Assert.That(revived != null && revived.IsAlive, Is.True, "Heroine must return after death.");
            float health = revived.CurrentHealth;
            enemyBody.position = (Vector2)revived.transform.position + new Vector2(0.9f, 0f);

            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                enemyBody.position = (Vector2)revived.transform.position + new Vector2(0.9f, 0f);
                yield return null;
                Assert.That(enemy.State, Is.Not.EqualTo(EnemyState2D.Telegraph).And.Not.EqualTo(EnemyState2D.Attacking),
                    "Enemies must not start attacking during the respawn breathing room.");
            }
            Assert.That(revived.CurrentHealth, Is.EqualTo(health), "No damage during the respawn breathing room.");
        }
    }
}
