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
    public sealed class GroundEnemyPlayModeTests
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
        public void GroundDemonPrototypeLoadsWithValidComponentsAndDefinition()
        {
            var enemy = Spawn("Enemies/GroundDemon_Prototype");
            var controller = enemy.GetComponent<GroundEnemyController2D>();
            var agent = enemy.GetComponent<EnemyAgent2D>();
            var receiver = enemy.GetComponent<DamageReceiver2D>();
            var body = enemy.GetComponent<Rigidbody2D>();
            var hurtbox = enemy.GetComponent<Hurtbox2D>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(body, Is.Not.Null);
            Assert.That(hurtbox, Is.Not.Null);

            EnemyDefinition def = agent.Definition;
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);
            Assert.That(def.DetectionRange, Is.GreaterThan(0f));
            Assert.That(def.AttackRange, Is.GreaterThan(0f));
            Assert.That(def.MoveSpeed, Is.GreaterThan(0f));
            Assert.That(def.Attack, Is.Not.Null);
            Assert.That(def.Attack.IsValid(out _), Is.True);
        }

        [UnityTest]
        public IEnumerator GroundEnemyChasesTargetWhenTargetIsOutsideAttackRange()
        {
            var enemy = Spawn("Enemies/GroundDemon_Prototype");
            enemy.transform.position = Vector3.zero;
            var agent = enemy.GetComponent<EnemyAgent2D>();
            var body = enemy.GetComponent<Rigidbody2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(3f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            bool targetSet = agent.TrySetTarget(playerReceiver);
            Assert.That(targetSet, Is.True);

            yield return new WaitForFixedUpdate();

            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Chasing));
            Assert.That(body.linearVelocity.x, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator GroundEnemyEntersTelegraphWhenTargetEntersAttackRange()
        {
            var enemy = Spawn("Enemies/GroundDemon_Prototype");
            enemy.transform.position = Vector3.zero;
            var agent = enemy.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);

            yield return new WaitForFixedUpdate();

            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Telegraph));
        }

        [UnityTest]
        public IEnumerator GroundEnemyExecutesMeleeAttackAndDamagesPlayerHurtbox()
        {
            var enemy = Spawn("Enemies/GroundDemon_Prototype");
            enemy.transform.position = Vector3.zero;
            var agent = enemy.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();
            float initialPlayerHealth = playerReceiver.CurrentHealth;

            yield return null;

            agent.TrySetTarget(playerReceiver);
            InvokeNonPublic(agent, "SetState", EnemyState2D.Attacking);

            yield return new WaitForFixedUpdate();

            Assert.That(playerReceiver.CurrentHealth, Is.LessThan(initialPlayerHealth));
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Recovering));
        }

        [UnityTest]
        public IEnumerator GroundEnemyStopsMotionAndActionsOnDisable()
        {
            var enemy = Spawn("Enemies/GroundDemon_Prototype");
            enemy.transform.position = Vector3.zero;
            var agent = enemy.GetComponent<EnemyAgent2D>();
            var body = enemy.GetComponent<Rigidbody2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(3f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);
            yield return new WaitForFixedUpdate();
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Chasing));

            enemy.SetActive(false);

            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Idle));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        }
    }
}
