using System.Collections;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SpecialOwnerSwitchLifecycleTests
    {
        private Scene slice;
        private GameObject target;

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

        [UnityTearDown] public IEnumerator Unload()
        {
            if (target != null) Object.DestroyImmediate(target);
            SceneManager.SetActiveScene(SceneManager.CreateScene("SpecialOwnerSwitchCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }

        [UnityTest] public IEnumerator SwitchingDuringSpecialCancelsOnceAndRebindsForNextActivation() => VerifySwitch(false);

        [UnityTest] public IEnumerator SwitchingInStageCallbackRebindsAfterDispatchAndAllowsNextActivation() => VerifySwitch(true);

        private IEnumerator VerifySwitch(bool fromStageCallback)
        {
            var session = Object.FindFirstObjectByType<VerticalSliceSession2D>();
            Assert.That(session.IsRunning, Is.True);
            var special = session.Special;
            var previous = session.Manager.ActiveCharacter;
            target = Object.Instantiate(Resources.Load<GameObject>("Enemies/GroundDemon_Prototype"),
                previous.transform.position + Vector3.right, Quaternion.identity);
            var receiver = target.GetComponent<DamageReceiver2D>();
            Assert.That(special.TrySetTargets(new[] { receiver }, out string error), Is.True, error);
            Assert.That(special.TryAddCharge(special.Definition.MaximumCharge), Is.True);
            int completions = 0;
            bool cancelled = false;
            special.Completed += value => { completions++; cancelled = value; };
            var switching = session.Manager.GetComponent<CharacterSwitchController2D>();
            bool switched = false;
            System.Action<HuntrX.Gameplay.Special.SpecialStage2D> callback = stage =>
            {
                switched = switching.TrySwitchNext(out string switchError);
                Assert.That(switched, Is.True, switchError);
            };
            if (fromStageCallback) special.StageChanged += callback;
            bool activated = special.TryActivate();
            if (fromStageCallback) special.StageChanged -= callback;
            Assert.That(activated, Is.EqualTo(!fromStageCallback));
            float committedHealth = receiver.CurrentHealth;
            if (fromStageCallback) Assert.That(committedHealth, Is.EqualTo(receiver.MaximumHealth),
                "Switching during stage publication must cancel before its first hit.");
            if (!fromStageCallback) switched = switching.TrySwitchNext(out error);
            Assert.That(switched, Is.True, error);
            var current = session.Manager.ActiveCharacter;
            Assert.That(current, Is.Not.SameAs(previous));
            Assert.That(previous.gameObject.activeSelf, Is.False);
            yield return null;
            Assert.That(special.IsActive, Is.False);
            Assert.That(special.Owner, Is.SameAs(current));
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(cancelled, Is.True);
            Assert.That(special.Charge, Is.Zero, "Switch cancellation must not refund reserved charge.");
            Assert.That(receiver.CurrentHealth, Is.EqualTo(committedHealth), "The cancelled sequence must stop dealing damage.");
            Assert.That(special.TrySetTargets(new[] { receiver }, out error), Is.True, error);
            Assert.That(special.TryAddCharge(special.Definition.MaximumCharge), Is.True);
            Assert.That(special.TryActivate(), Is.True, "The new actor must be able to activate after earning a fresh charge.");
            special.Cancel();
        }
    }
}
