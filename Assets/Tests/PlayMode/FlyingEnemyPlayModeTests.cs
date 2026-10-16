using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FlyingEnemyPlayModeTests
    {
        private static readonly BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<GameObject> instances = new List<GameObject>();

        private GameObject Spawn(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instances.Add(instance);
            return instance;
        }

        private static T Field<T>(object instance, string name) =>
            (T)instance.GetType().GetField(name, NonPublicInstance).GetValue(instance);

        private static void SetField(object instance, string name, object value) =>
            instance.GetType().GetField(name, NonPublicInstance).SetValue(instance, value);

        private static void InvokeNonPublic(object instance, string methodName, params object[] args)
        {
            var method = instance.GetType().GetMethod(methodName, NonPublicInstance);
            Assert.That(method, Is.Not.Null, "Method not found: " + methodName);
            method.Invoke(instance, args);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in instances)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            instances.Clear();
        }

        [Test]
        public void FlyingDemonPrototypeLoadsWithValidDefinitionAndComponents()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();
            var receiver = flyer.GetComponent<DamageReceiver2D>();
            var body = flyer.GetComponent<Rigidbody2D>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(body, Is.Not.Null);

            FlyingEnemyDefinition def = controller.Definition;
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);
            Assert.That(def.HoverHeight, Is.GreaterThan(0f));
            Assert.That(def.AltitudeAmplitude, Is.GreaterThan(0f));
            Assert.That(def.AltitudeAmplitude, Is.LessThan(def.HoverHeight));
            Assert.That(def.AltitudeFrequency, Is.GreaterThan(0f));
            Assert.That(def.DiveSpeed, Is.GreaterThan(0f));
            Assert.That(def.DiveDuration, Is.GreaterThan(0f));
            Assert.That(def.ProjectileSpeed, Is.GreaterThan(0f));
            Assert.That(def.ProjectileLifetime, Is.GreaterThan(0f));
            Assert.That(def.ProjectileRange, Is.GreaterThan(0f));
            Assert.That(def.ProjectilePrefab, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator FlyingEnemyOscillatesAltitudeAndMovesTowardsTarget()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            flyer.transform.position = new Vector3(0f, 0f, 0f);
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();
            var body = flyer.GetComponent<Rigidbody2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(4f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            bool targetSet = agent.TrySetTarget(playerReceiver);
            Assert.That(targetSet, Is.True);
            Assert.That(agent.HasValidTarget, Is.True);

            float initialY = body.position.y;

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.position.y, Is.GreaterThan(initialY));
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Chasing).Or.EqualTo(EnemyState2D.Telegraph).Or.EqualTo(EnemyState2D.Idle));
        }

        [UnityTest]
        public IEnumerator FlyingEnemyDivesTowardsTargetWhenAttacking()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            flyer.transform.position = new Vector3(0f, 4f, 0f);
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();
            var body = flyer.GetComponent<Rigidbody2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);
            Assert.That(agent.HasValidTarget, Is.True);

            InvokeNonPublic(agent, "SetState", EnemyState2D.Attacking);
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Attacking));

            yield return new WaitForFixedUpdate();

            Assert.That(controller.IsDiving, Is.True);
            Assert.That(body.linearVelocity.y, Is.LessThan(0f));
        }

        [UnityTest]
        public IEnumerator FlyingEnemyFiresProjectileOnAlternateAttackCycle()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            flyer.transform.position = new Vector3(0f, 4f, 0f);
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);
            Assert.That(agent.HasValidTarget, Is.True);

            SetField(controller, "nextIsDive", false);

            InvokeNonPublic(agent, "SetState", EnemyState2D.Attacking);
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Attacking));

            yield return new WaitForFixedUpdate();

            bool nextIsDiveAfter = Field<bool>(controller, "nextIsDive");
            Assert.That(nextIsDiveAfter, Is.True);

            var projectiles = Field<List<EnemyProjectile2D>>(controller, "projectiles");
            Assert.That(projectiles.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(projectiles[0], Is.Not.Null);
            Assert.That(projectiles[0].IsFlying, Is.True);
        }

        [UnityTest]
        public IEnumerator FlyingEnemyCancelsActionsAndDivingOnDisable()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            flyer.transform.position = new Vector3(0f, 4f, 0f);
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);
            InvokeNonPublic(agent, "SetState", EnemyState2D.Attacking);

            yield return new WaitForFixedUpdate();
            Assert.That(controller.IsDiving, Is.True);

            flyer.SetActive(false);

            Assert.That(controller.IsDiving, Is.False);
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Idle));
        }

        [UnityTest]
        public IEnumerator FlyingEnemyCancelsActionsOnDeath()
        {
            var flyer = Spawn("Enemies/FlyingDemon_Prototype");
            flyer.transform.position = new Vector3(0f, 4f, 0f);
            var controller = flyer.GetComponent<FlyingEnemyController2D>();
            var agent = flyer.GetComponent<EnemyAgent2D>();
            var receiver = flyer.GetComponent<DamageReceiver2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);
            InvokeNonPublic(agent, "SetState", EnemyState2D.Attacking);

            yield return new WaitForFixedUpdate();
            Assert.That(controller.IsDiving, Is.True);

            for (int i = 0; i < 50 && receiver.IsAlive; i++)
            {
                receiver.TryReceiveHit(agent.Definition.Attack, playerReceiver, 1f);
            }

            Assert.That(receiver.IsAlive, Is.False);
            Assert.That(controller.IsDiving, Is.False);
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Dead));
        }
    }
}
