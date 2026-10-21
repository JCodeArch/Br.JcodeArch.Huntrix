using System.Collections;
using System.Linq;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using HuntrX.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SessionHudLifecycleTests
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
            SceneManager.SetActiveScene(SceneManager.CreateScene("SessionHudCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }
        [UnityTest] public IEnumerator StopReleasesTelemetryAndNextFrameRestartRendersNewResources()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var hud = Object.FindAnyObjectByType<SliceReadOnlyHud2D>();
            var owner = Object.FindAnyObjectByType<SliceHud2D>();
            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.Honmoon.TrySetValue(23), Is.True);
            yield return null;
            Assert.That(Label(hud, "Honmoon"), Is.EqualTo("Honmoon: 23"));
            var previousManager = session.Manager;
            session.Stop();
            yield return null;
            Assert.That(hud.GetComponentInChildren<Canvas>(true).enabled, Is.False,
                "A stopped session must not retain an overlay or its telemetry claim.");
            Assert.That(owner.TelemetryPresentedExternally, Is.False);
            // Waited one frame so this tests presentation rebinding, independent of same-frame camera teardown.
            Assert.That(session.TryBegin(), Is.True);
            Assert.That(session.Manager, Is.Not.SameAs(previousManager));
            Assert.That(session.Honmoon.TrySetValue(47), Is.True);
            yield return null;
            Assert.That(hud.GetComponentInChildren<Canvas>(true).enabled, Is.True);
            Assert.That(owner.TelemetryPresentedExternally, Is.True);
            Assert.That(Label(hud, "Honmoon"), Is.EqualTo("Honmoon: 47"));
            Assert.That(Label(hud, "Beat"), Is.EqualTo(session.BeatTitle));
        }
        [UnityTest] public IEnumerator CompletingAirplaneBeatReplacesTitleWithoutRecreatingHud()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var hud = Object.FindAnyObjectByType<SliceReadOnlyHud2D>();
            var canvas = hud.GetComponentInChildren<Canvas>(true);
            string previous = Label(hud, "Beat");
            Assert.That(session.BeatIndex, Is.Zero);
            session.ClearEnemies();
            var body = session.Manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            body.position = session.Stage.EndPoint.position;
            body.linearVelocity = Vector2.zero;
            // A render-frame count does not guarantee a physics step in a fast batch run.
            yield return new WaitForFixedUpdate();
            for (int frame = 0; frame < 10 && session.BeatIndex == 0; frame++) yield return null;
            Assert.That(session.BeatIndex, Is.EqualTo(1),
                $"actor={session.Manager.ActiveCharacter.transform.position}, body={body.position}, " +
                $"alive={session.Manager.ActiveCharacter.IsAlive}, running={session.IsRunning}, " +
                $"canEnter={session.Manager.CanEnterStage}, enemies={session.LivingEnemies}, aerial={session.Aerial.Count}");
            yield return null;
            Assert.That(Label(hud, "Beat"), Is.EqualTo(session.BeatTitle).And.Not.EqualTo(previous));
            Assert.That(hud.GetComponentInChildren<Canvas>(true), Is.SameAs(canvas));
            Assert.That(canvas.enabled, Is.True);
        }
        [UnityTest] public IEnumerator AirplaneIntroReleasesAllSeatedCharacterArtBeforeCombatPresentation()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            var beat = Object.FindAnyObjectByType<SliceAirplaneBeat2D>();
            var intro = session.Stage.RuntimeRoot.transform.Find("VIP_RearLounge_EatingScene");
            Assert.That(intro, Is.Not.Null);
            Assert.That(intro.GetComponentsInChildren<SpriteRenderer>().Length, Is.EqualTo(2));
            beat.Tick(3.3f);
            beat.Tick(0f);
            Assert.That(intro.GetComponentsInChildren<SpriteRenderer>().All(renderer => !renderer.enabled), Is.True,
                "Seated heroines must not remain as giant duplicate characters over live combat.");
            Assert.That(session.Manager.ActiveCharacter, Is.Not.Null);
            yield return null;
        }
        [UnityTest] public IEnumerator AirplaneRuntimeEnemiesStartOutsideSolidPlatforms()
        {
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            session.Stop();
            yield return null;
            Assert.That(session.TryBegin(), Is.True);
            Physics2D.SyncTransforms();
            var platforms = session.Stage.RuntimeRoot.GetComponentsInChildren<BoxCollider2D>();
            var enemies = Object.FindObjectsByType<HuntrX.Gameplay.Enemies.EnemyAgent2D>(FindObjectsSortMode.None);
            Assert.That(enemies.Length, Is.GreaterThanOrEqualTo(6), "Include the extra corridor guards, not only authored spawn points.");
            foreach (var enemy in enemies)
            {
                var capsule = enemy.GetComponent<CapsuleCollider2D>();
                Assert.That(capsule, Is.Not.Null);
                foreach (var platform in platforms)
                {
                    var a = capsule.bounds;
                    var b = platform.bounds;
                    bool overlaps = a.min.x < b.max.x && a.max.x > b.min.x && a.min.y < b.max.y && a.max.y > b.min.y;
                    Assert.That(overlaps, Is.False, enemy.name + " starts inside " + platform.name + " at " + enemy.transform.position);
                }
            }
        }
        private static string Label(Component hud, string name) =>
            hud.GetComponentsInChildren<Text>(true).Single(text => text.name == name).text;
    }
}
