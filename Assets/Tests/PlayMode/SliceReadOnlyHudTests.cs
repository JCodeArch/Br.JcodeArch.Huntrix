using System.Collections;
using System.Linq;
using System.Reflection;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Gameplay.Touch;
using HuntrX.Presentation.Hud;
using HuntrX.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SliceReadOnlyHudTests
    {
        private Scene lab;
        [UnitySetUp]
        public IEnumerator LoadSlice()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
#endif
            lab = SceneManager.GetActiveScene();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            SceneManager.SetActiveScene(SceneManager.CreateScene("ReadOnlyHudCleanup"));
            yield return SceneManager.UnloadSceneAsync(lab);
        }
        [UnityTest]
        public IEnumerator SceneShowsRealResourcesAndDoesNotCreateInputOrTouchProducers()
        {
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var owner = Object.FindFirstObjectByType<SliceHud2D>();
            Assert.That(session.IsRunning, Is.True);
            Assert.That(presenter, Is.Not.Null);
            Assert.That(owner.TelemetryPresentedExternally, Is.True);
            Assert.That(owner.enabled, Is.True);
            Assert.That(Object.FindObjectsByType<SliceTouchControls2D>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(presenter.GetComponentsInChildren<Selectable>(true), Is.Empty);
            Assert.That(presenter.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(presenter.GetComponentsInChildren<GraphicRaycaster>(true), Is.Empty);
            Assert.That(presenter.GetComponentsInChildren<Graphic>(true).All(g => !g.raycastTarget), Is.True);
            var state = SliceReadOnlyHud2D.Capture(session);
            Assert.That(state.HealthCurrent, Is.EqualTo(Mathf.CeilToInt(session.Manager.ActiveCharacter.CurrentHealth)));
            Assert.That(state.FoodCount, Is.EqualTo(session.Inventory.TotalCount));
            Assert.That(state.SpecialCharge, Is.EqualTo(session.Special.Charge));
            Assert.That(state.BeatTitle, Is.EqualTo(session.BeatTitle));
            Assert.That(Text(presenter, "Health"), Does.Contain(state.HealthCurrent.ToString()));
            Assert.That(Text(presenter, "Beat"), Is.EqualTo(session.BeatTitle));
            bool priorInput = session.Input.enabled;
            float priorScale = Time.timeScale;
            presenter.enabled = false;
            Assert.That(owner.TelemetryPresentedExternally, Is.False);
            Assert.That(owner.enabled, Is.True);
            Assert.That(session.Input.enabled, Is.EqualTo(priorInput));
            Assert.That(Time.timeScale, Is.EqualTo(priorScale));
            yield return null;
        }
        [UnityTest]
        public IEnumerator MissingSessionClearsValuesAndOwnerDisableHidesOverlay()
        {
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var owner = Object.FindFirstObjectByType<SliceHud2D>();
            owner.enabled = false;
            yield return null;
            Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.False);
            Assert.That(owner.TelemetryPresentedExternally, Is.False);
            var empty = SliceReadOnlyHud2D.Capture(null);
            presenter.GetComponentInChildren<SliceReadOnlyHudView2D>(true).RenderState(empty);
            Assert.That(Text(presenter, "Health"), Does.Contain("indisponível"));
            Assert.That(Text(presenter, "Fans"), Does.Contain("indisponível"));
            Assert.That(Text(presenter, "Special"), Does.Contain("indisponível"));
        }
        [UnityTest]
        public IEnumerator PresentationRefreshesChangedSourceAndOptionsWithoutOwningPause()
        {
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            var owner = Object.FindFirstObjectByType<SliceHud2D>();
            var actor = session.Manager.ActiveCharacter;
            typeof(HuntrX.Gameplay.Combat.DamageReceiver2D).GetProperty("CurrentHealth")
                .SetValue(actor, 37f);
            yield return null;
            Assert.That(Text(presenter, "Health"), Does.StartWith("Vida: 37 /"));
            typeof(SliceHud2D).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, true);
            yield return null;
            Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.False);
            Assert.That(owner.IsOptionsOpen, Is.True);
            typeof(SliceHud2D).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, false);
            yield return null;
            Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.True);
            Assert.That(owner.TelemetryPresentedExternally, Is.True);
        }
        [UnityTest]
        public IEnumerator RealSwitchAndResourceChangesReplacePreviouslyRenderedValues()
        {
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var previous = session.Manager.ActiveCharacter;
            Assert.That(session.Manager.GetComponent<HuntrX.Gameplay.Characters.CharacterSwitchController2D>()
                .TrySwitchNext(out _), Is.True);
            Assert.That(session.Honmoon.TrySetValue(23f), Is.True);
            Assert.That(session.Inventory.TryAdd(session.Ramen), Is.True);
            yield return null;
            Assert.That(session.Manager.ActiveCharacter, Is.Not.SameAs(previous));
            Assert.That(Text(presenter, "Character"), Is.EqualTo(session.Manager.ActivePrefab.name));
            Assert.That(Text(presenter, "Honmoon"), Is.EqualTo("Honmoon: 23"));
            Assert.That(Text(presenter, "Food"), Is.EqualTo("Comida: " + session.Inventory.TotalCount));
            session.Honmoon.enabled = false;
            yield return null;
            Assert.That(Text(presenter, "Honmoon"), Is.EqualTo("Honmoon: indisponível"));
        }
        [UnityTest]
        public IEnumerator PresenterCannotStealOrReleaseAnotherPresentersClaim()
        {
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var owner = Object.FindFirstObjectByType<SliceHud2D>();
            presenter.enabled = false;
            var other = new GameObject("Other presenter").AddComponent<SliceUiFlow2D>();
            try
            {
                Assert.That(owner.TryClaimTelemetry(other), Is.True);
                presenter.enabled = true;
                yield return null;
                Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.False);
                presenter.enabled = false;
                Assert.That(owner.TelemetryPresentedExternally, Is.True);
                owner.ReleaseTelemetry(other);
                presenter.enabled = true;
                yield return null;
                Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.True);
            }
            finally { Object.Destroy(other.gameObject); }
        }
        [UnityTest]
        public IEnumerator MissingOwnerRebindDisableEnableAndDestroyRestoreLegacyWithoutLeakingCanvas()
        {
            var presenter = Object.FindFirstObjectByType<SliceReadOnlyHud2D>();
            var owner = Object.FindFirstObjectByType<SliceHud2D>();
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            var binding = typeof(SliceReadOnlyHud2D).GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic);
            bool priorInput = session.Input.enabled;
            float priorScale = Time.timeScale;
            binding.SetValue(presenter, null);
            yield return null;
            Assert.That(presenter.GetComponentInChildren<Canvas>(true).enabled, Is.False);
            Assert.That(owner.TelemetryPresentedExternally, Is.False);
            binding.SetValue(presenter, owner);
            yield return null;
            Assert.That(owner.TelemetryPresentedExternally, Is.True);
            presenter.enabled = false;
            presenter.enabled = true;
            yield return null;
            Assert.That(presenter.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(owner.TelemetryPresentedExternally, Is.True);
            var root = presenter.gameObject;
            Object.Destroy(presenter);
            yield return null;
            yield return null; // OnDestroy schedules destruction of its owned child Canvas.
            Assert.That(root.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(owner.TelemetryPresentedExternally, Is.False);
            Assert.That(owner.enabled, Is.True);
            Assert.That(session.Input.enabled, Is.EqualTo(priorInput));
            Assert.That(Time.timeScale, Is.EqualTo(priorScale));
        }
        private static string Text(Component root, string name) =>
            root.GetComponentsInChildren<Text>(true).Single(t => t.name == name).text;
    }
}
