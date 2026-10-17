using System.Collections;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FanRescuePlayModeTests
    {
        private readonly List<UnityEngine.Object> disposables = new List<UnityEngine.Object>();

        private GameObject Spawn(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, "Resource not found at: " + path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            disposables.Add(instance);
            return instance;
        }

        private DamageReceiver2D CreateActor(string name, CombatFaction2D faction, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
            var receiver = go.AddComponent<DamageReceiver2D>();
            disposables.Add(go);
            return receiver;
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
        public void RescuableFan_HasFanRescueControllerAndValidDefinition()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var rescue = fanGo.GetComponent<FanRescueController2D>();
            var fan = fanGo.GetComponent<FanActor2D>();

            Assert.That(rescue, Is.Not.Null);
            Assert.That(fan, Is.Not.Null);
            Assert.That(rescue.IsRecovering, Is.False);
            Assert.That(rescue.CompletedRescueCount, Is.EqualTo(0));
        }

        [Test]
        public void TryBeginRescue_ValidatesEligibilityAndInteractionRange()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            fanGo.transform.position = Vector3.zero;
            var fan = fanGo.GetComponent<FanActor2D>();
            var rescue = fanGo.GetComponent<FanRescueController2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = Vector3.zero;
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            // Normal fan does not need rescue
            Assert.That(fan.State, Is.EqualTo(FanState.Normal));
            Assert.That(rescue.TryBeginRescue(playerReceiver), Is.False);

            // Drain fan so it needs rescue
            Assert.That(fan.TryDrain(50f), Is.True);
            Assert.That(fan.State, Is.Not.EqualTo(FanState.Normal));

            // Null rescuer
            Assert.That(rescue.TryBeginRescue(null), Is.False);

            // Rescuer far away (> 2m default range)
            player.transform.position = new Vector3(10f, 0f, 0f);
            Assert.That(rescue.TryBeginRescue(playerReceiver), Is.False);

            // Rescuer close (< 2m range)
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            bool rescueStarted = rescue.TryBeginRescue(playerReceiver);
            Assert.That(rescueStarted, Is.True);
            Assert.That(rescue.IsRecovering, Is.True);
            Assert.That(fan.IsProtected, Is.True);
        }

        [UnityTest]
        public IEnumerator RescueProcess_GraduallyRecoversSoulAndFiresRescueCompletedEvent()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            fanGo.transform.position = Vector3.zero;
            var fan = fanGo.GetComponent<FanActor2D>();
            var rescue = fanGo.GetComponent<FanRescueController2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerBody = player.GetComponent<Rigidbody2D>();
            if (playerBody != null)
            {
                playerBody.gravityScale = 0f;
                playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            Assert.That(fan.TryDrain(10f), Is.True);
            float initialDrainedSoul = fan.Soul;

            FanActor2D completedFan = null;
            int receivedReward = 0;
            rescue.RescueCompleted += (actor, reward) =>
            {
                completedFan = actor;
                receivedReward = reward;
            };

            Assert.That(rescue.TryBeginRescue(playerReceiver), Is.True);

            // Wait until full recovery (small drain recovers quickly)
            float timeout = 4f;
            while (rescue.IsRecovering && timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.That(fan.State, Is.EqualTo(FanState.Normal));
            Assert.That(rescue.CompletedRescueCount, Is.EqualTo(1));
            Assert.That(completedFan, Is.EqualTo(fan));
            Assert.That(receivedReward, Is.GreaterThan(0));
            Assert.That(rescue.IsRecovering, Is.False);
        }

        [UnityTest]
        public IEnumerator RescueCancelsWhenRescuerMovesOutOfRange()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            fanGo.transform.position = Vector3.zero;
            var fan = fanGo.GetComponent<FanActor2D>();
            var rescue = fanGo.GetComponent<FanRescueController2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            Assert.That(fan.TryDrain(60f), Is.True);
            Assert.That(rescue.TryBeginRescue(playerReceiver), Is.True);
            Assert.That(rescue.IsRecovering, Is.True);

            // Move player far away
            player.transform.position = new Vector3(25f, 0f, 0f);

            yield return null;

            Assert.That(rescue.IsRecovering, Is.False);
            Assert.That(fan.IsProtected, Is.False);
            Assert.That(fan.State, Is.Not.EqualTo(FanState.Normal));
        }

        [Test]
        public void CancelAndOnDisable_CleansUpProtectionAndResetsState()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            fanGo.transform.position = Vector3.zero;
            var fan = fanGo.GetComponent<FanActor2D>();
            var rescue = fanGo.GetComponent<FanRescueController2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            Assert.That(fan.TryDrain(50f), Is.True);
            Assert.That(rescue.TryBeginRescue(playerReceiver), Is.True);
            Assert.That(rescue.IsRecovering, Is.True);
            Assert.That(fan.IsProtected, Is.True);

            rescue.Cancel();

            Assert.That(rescue.IsRecovering, Is.False);
            Assert.That(fan.IsProtected, Is.False);
        }
    }
}
