using System;
using System.Collections;
using System.Collections.Generic;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CharacterManagerPlayModeTests
    {
        private readonly List<UnityEngine.Object> disposables = new List<UnityEngine.Object>();

        private T Spawn<T>(string path) where T : UnityEngine.Object
        {
            var prefab = Resources.Load<T>(path);
            Assert.That(prefab, Is.Not.Null, "Resource not found at: " + path);
            return prefab;
        }

        private CharacterManager2D CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out GameObject rumiPrefab, out GameObject miraPrefab, out GameObject zoeyPrefab)
        {
            var slotPrefab = Spawn<GameObject>("Characters/HuntrXPlayerSlot_Prototype");
            slotGo = UnityEngine.Object.Instantiate(slotPrefab);
            disposables.Add(slotGo);

            var manager = slotGo.GetComponent<CharacterManager2D>();
            flow = slotGo.GetComponent<CheckpointAttemptFlow2D>();
            rumiPrefab = Spawn<GameObject>("Characters/Rumi_Prototype");
            miraPrefab = Spawn<GameObject>("Characters/Mira_Prototype");
            zoeyPrefab = Spawn<GameObject>("Characters/Zoey_Prototype");

            Transform stageStart = slotGo.transform.Find("StageStart");
            Assert.That(stageStart, Is.Not.Null, "StageStart transform must exist under slot.");
            Assert.That(manager.Configure(rumiPrefab, flow, stageStart, "stage-start-01"), Is.True);
            return manager;
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
        public void HuntrXPlayerSlotPrototype_LoadsComponentsAndConfiguresManagerSuccessfully()
        {
            var manager = CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out GameObject rumiPrefab, out _, out _);

            Assert.That(manager, Is.Not.Null);
            Assert.That(flow, Is.Not.Null);
            Assert.That(manager.State, Is.EqualTo(CharacterManagerState2D.Uninitialized));

            bool spawned = manager.TrySpawn();
            Assert.That(spawned, Is.True);
            Assert.That(manager.State, Is.EqualTo(CharacterManagerState2D.Alive));
            Assert.That(manager.ActiveCharacter, Is.Not.Null);
            Assert.That(manager.ActivePrefab, Is.EqualTo(rumiPrefab));
            Assert.That(flow.IsAttemptInProgress, Is.True);
            Assert.That(flow.BoundActor, Is.EqualTo(manager.ActiveCharacter));
            Assert.That(manager.CanEnterStage, Is.True);
        }

        [UnityTest]
        public IEnumerator SwitchCharacter_TransfersPositionVelocityAndRebindsAttemptFlow()
        {
            var manager = CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out GameObject rumiPrefab, out GameObject miraPrefab, out GameObject zoeyPrefab);
            Assert.That(manager.TrySpawn(), Is.True);

            DamageReceiver2D rumiActor = manager.ActiveCharacter;
            Assert.That(rumiActor, Is.Not.Null);

            Vector3 testPosition = new Vector3(3.5f, 1.2f, 0f);
            Vector2 testVelocity = new Vector2(4.5f, -1.0f);
            rumiActor.transform.position = testPosition;
            var rumiBody = rumiActor.GetComponent<Rigidbody2D>();
            rumiBody.linearVelocity = testVelocity;

            yield return null;

            bool switchedToMira = manager.TrySwitch(miraPrefab);
            Assert.That(switchedToMira, Is.True);
            Assert.That(manager.ActivePrefab, Is.EqualTo(miraPrefab));
            Assert.That(manager.ActiveCharacter, Is.Not.Null);
            Assert.That(manager.ActiveCharacter, Is.Not.EqualTo(rumiActor));
            Assert.That(flow.BoundActor, Is.EqualTo(manager.ActiveCharacter));
            Assert.That(rumiActor.gameObject.activeInHierarchy, Is.False);
            Assert.That(manager.ActiveCharacter.gameObject.activeInHierarchy, Is.True);

            Assert.That(manager.ActiveCharacter.transform.position, Is.EqualTo(testPosition));
            var miraBody = manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            Assert.That(miraBody.linearVelocity, Is.EqualTo(testVelocity));

            bool switchedToZoey = manager.TrySwitch(zoeyPrefab);
            Assert.That(switchedToZoey, Is.True);
            Assert.That(manager.ActivePrefab, Is.EqualTo(zoeyPrefab));
            Assert.That(flow.BoundActor, Is.EqualTo(manager.ActiveCharacter));

            bool switchedBackToRumi = manager.TrySwitch(rumiPrefab);
            Assert.That(switchedBackToRumi, Is.True);
            Assert.That(manager.ActivePrefab, Is.EqualTo(rumiPrefab));
            Assert.That(manager.ActiveCharacter, Is.EqualTo(rumiActor));
            Assert.That(rumiActor.gameObject.activeInHierarchy, Is.True);
        }

        [UnityTest]
        public IEnumerator SwitchCharacter_RejectsWhenActiveCharacterIsAttacking()
        {
            var manager = CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out GameObject rumiPrefab, out GameObject miraPrefab, out _);
            Assert.That(manager.TrySpawn(), Is.True);

            var attackController = manager.ActiveCharacter.GetComponent<AttackController2D>();
            Assert.That(attackController, Is.Not.Null);

            attackController.TryStartAttack(1f);
            yield return null;

            Assert.That(attackController.State, Is.Not.EqualTo(AttackState2D.Idle));

            bool switchResult = manager.TrySwitch(miraPrefab);
            Assert.That(switchResult, Is.False);
            Assert.That(manager.ActivePrefab, Is.EqualTo(rumiPrefab));
        }

        [Test]
        public void TryEnterStage_TransfersActorToNewStageStartAndResetsAttempt()
        {
            var manager = CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out _, out _, out _);
            Assert.That(manager.TrySpawn(), Is.True);

            var nextStartGo = new GameObject("StageStart_Level2");
            nextStartGo.transform.position = new Vector3(12f, 0f, 0f);
            disposables.Add(nextStartGo);

            bool entered = manager.TryEnterStage(nextStartGo.transform, "stage-02");
            Assert.That(entered, Is.True);
            Assert.That(manager.ActiveCharacter.transform.position, Is.EqualTo(new Vector3(12f, 0f, 0f)));
            var body = manager.ActiveCharacter.GetComponent<Rigidbody2D>();
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(flow.IsAttemptInProgress, Is.True);
            Assert.That(flow.BoundActor, Is.EqualTo(manager.ActiveCharacter));
        }

        [Test]
        public void OnDisable_CleansUpAllCachedActorsAndCancelsAttempt()
        {
            var manager = CreatePlayerSlot(out GameObject slotGo, out CheckpointAttemptFlow2D flow, out _, out GameObject miraPrefab, out _);
            Assert.That(manager.TrySpawn(), Is.True);
            Assert.That(manager.TrySwitch(miraPrefab), Is.True);

            slotGo.SetActive(false);

            Assert.That(manager.State, Is.EqualTo(CharacterManagerState2D.Disabled));
            Assert.That(manager.ActiveCharacter, Is.Null);
            Assert.That(manager.ActivePrefab, Is.Null);
            Assert.That(flow.IsAttemptInProgress, Is.False);
        }
    }
}
