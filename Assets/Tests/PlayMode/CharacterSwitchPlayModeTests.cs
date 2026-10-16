using System.Collections;
using System.Collections.Generic;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CharacterSwitchPlayModeTests
    {
        private readonly List<UnityEngine.Object> disposables = new List<UnityEngine.Object>();

        private T Spawn<T>(string path) where T : UnityEngine.Object
        {
            var prefab = Resources.Load<T>(path);
            Assert.That(prefab, Is.Not.Null, "Resource not found at: " + path);
            return prefab;
        }

        private void CreatePlayerSlot(out GameObject slotGo, out CharacterManager2D manager, out CharacterSwitchController2D switchController, out GameObject rumiPrefab, out GameObject miraPrefab, out GameObject zoeyPrefab)
        {
            var slotPrefab = Spawn<GameObject>("Characters/HuntrXPlayerSlot_Prototype");
            slotGo = UnityEngine.Object.Instantiate(slotPrefab);
            disposables.Add(slotGo);

            manager = slotGo.GetComponent<CharacterManager2D>();
            switchController = slotGo.GetComponent<CharacterSwitchController2D>();
            var flow = slotGo.GetComponent<CheckpointAttemptFlow2D>();

            rumiPrefab = Spawn<GameObject>("Characters/Rumi_Prototype");
            miraPrefab = Spawn<GameObject>("Characters/Mira_Prototype");
            zoeyPrefab = Spawn<GameObject>("Characters/Zoey_Prototype");

            Transform stageStart = slotGo.transform.Find("StageStart");
            Assert.That(stageStart, Is.Not.Null);

            Assert.That(manager.Configure(rumiPrefab, flow, stageStart, "start-01"), Is.True);
            Assert.That(switchController.Configure(manager, new[] { rumiPrefab, miraPrefab, zoeyPrefab }, out string error), Is.True, error);
            Assert.That(manager.TrySpawn(), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(disposables[i]);
                }
            }
            disposables.Clear();
        }

        [Test]
        public void ConfigureRoster_ValidatesInputsAndInitializesState()
        {
            var manager = new GameObject("ManagerGo").AddComponent<CharacterManager2D>();
            var switchController = manager.gameObject.AddComponent<CharacterSwitchController2D>();
            disposables.Add(manager.gameObject);

            var rumiPrefab = Spawn<GameObject>("Characters/Rumi_Prototype");
            var miraPrefab = Spawn<GameObject>("Characters/Mira_Prototype");

            Assert.That(switchController.Configure(null, new[] { rumiPrefab, miraPrefab }, out string error1), Is.False);
            Assert.That(switchController.Configure(manager, null, out string error2), Is.False);
            Assert.That(switchController.Configure(manager, new[] { rumiPrefab }, out string error3), Is.False);
            Assert.That(switchController.Configure(manager, new[] { rumiPrefab, null }, out string error4), Is.False);
            Assert.That(switchController.Configure(manager, new[] { rumiPrefab, rumiPrefab }, out string error5), Is.False);

            bool configured = switchController.Configure(manager, new[] { rumiPrefab, miraPrefab }, out string errorValid);
            Assert.That(configured, Is.True, errorValid);
            Assert.That(switchController.RosterCount, Is.EqualTo(2));
        }

        [Test]
        public void SwitchNext_CyclesSequentiallyThroughRoster()
        {
            CreatePlayerSlot(out GameObject slotGo, out CharacterManager2D manager, out CharacterSwitchController2D switchController, out GameObject rumiPrefab, out GameObject miraPrefab, out GameObject zoeyPrefab);

            Assert.That(switchController.ActiveIndex, Is.EqualTo(0));
            Assert.That(manager.ActivePrefab, Is.EqualTo(rumiPrefab));

            bool switchedToMira = switchController.TrySwitchNext(out string error1);
            Assert.That(switchedToMira, Is.True, error1);
            Assert.That(switchController.ActiveIndex, Is.EqualTo(1));
            Assert.That(manager.ActivePrefab, Is.EqualTo(miraPrefab));

            bool switchedToZoey = switchController.TrySwitchNext(out string error2);
            Assert.That(switchedToZoey, Is.True, error2);
            Assert.That(switchController.ActiveIndex, Is.EqualTo(2));
            Assert.That(manager.ActivePrefab, Is.EqualTo(zoeyPrefab));

            bool switchedBackToRumi = switchController.TrySwitchNext(out string error3);
            Assert.That(switchedBackToRumi, Is.True, error3);
            Assert.That(switchController.ActiveIndex, Is.EqualTo(0));
            Assert.That(manager.ActivePrefab, Is.EqualTo(rumiPrefab));
        }

        [Test]
        public void TrySwitchTo_SwitchesDirectlyToTargetOrRejectsInvalidIndices()
        {
            CreatePlayerSlot(out GameObject slotGo, out CharacterManager2D manager, out CharacterSwitchController2D switchController, out GameObject rumiPrefab, out GameObject miraPrefab, out GameObject zoeyPrefab);

            Assert.That(switchController.ActiveIndex, Is.EqualTo(0));

            bool switchedDirectlyToZoey = switchController.TrySwitchTo(2, out string error1);
            Assert.That(switchedDirectlyToZoey, Is.True, error1);
            Assert.That(switchController.ActiveIndex, Is.EqualTo(2));
            Assert.That(manager.ActivePrefab, Is.EqualTo(zoeyPrefab));

            bool switchSameRejected = switchController.TrySwitchTo(2, out string errorSame);
            Assert.That(switchSameRejected, Is.False);
            Assert.That(errorSame, Does.Contain("different character"));

            bool switchUnderflowRejected = switchController.TrySwitchTo(-1, out string errorUnderflow);
            Assert.That(switchUnderflowRejected, Is.False);
            Assert.That(errorUnderflow, Does.Contain("outside the roster"));

            bool switchOverflowRejected = switchController.TrySwitchTo(99, out string errorOverflow);
            Assert.That(switchOverflowRejected, Is.False);
            Assert.That(errorOverflow, Does.Contain("outside the roster"));
        }

        [Test]
        public void TrySwitchNext_RejectsWhenControllerOrManagerIsDisabled()
        {
            CreatePlayerSlot(out GameObject slotGo, out CharacterManager2D manager, out CharacterSwitchController2D switchController, out _, out _, out _);

            switchController.enabled = false;
            bool switchWhileDisabled = switchController.TrySwitchNext(out string errorDisabled);
            Assert.That(switchWhileDisabled, Is.False);
            Assert.That(errorDisabled, Does.Contain("disabled"));

            switchController.enabled = true;
            manager.enabled = false;
            bool switchWhileManagerDisabled = switchController.TrySwitchNext(out string errorManagerDisabled);
            Assert.That(switchWhileManagerDisabled, Is.False);
            Assert.That(errorManagerDisabled, Does.Contain("must own a live active character"));
        }

        [UnityTest]
        public IEnumerator TrySwitchNext_RejectsWhenCharacterIsAttacking()
        {
            CreatePlayerSlot(out GameObject slotGo, out CharacterManager2D manager, out CharacterSwitchController2D switchController, out _, out _, out _);

            var attack = manager.ActiveCharacter.GetComponent<AttackController2D>();
            Assert.That(attack, Is.Not.Null);

            attack.TryTriggerAttack();
            yield return null;

            Assert.That(attack.State, Is.Not.EqualTo(AttackState2D.Idle));

            bool switchRejected = switchController.TrySwitchNext(out string error);
            Assert.That(switchRejected, Is.False);
            Assert.That(error, Does.Contain("rejected the switch"));
        }
    }
}
