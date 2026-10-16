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
    public sealed class EliteEnemyPlayModeTests
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
        public void EliteDemonPrototypeLoadsWithValidPatternAndComponents()
        {
            var elite = Spawn("Enemies/EliteDemon_Prototype");
            var pattern = elite.GetComponent<EnemyPatternController2D>();
            var agent = elite.GetComponent<EnemyAgent2D>();
            var receiver = elite.GetComponent<DamageReceiver2D>();
            var body = elite.GetComponent<Rigidbody2D>();

            Assert.That(pattern, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(body, Is.Not.Null);

            EnemyPatternDefinition def = pattern.Definition;
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);
            Assert.That(def.BurstCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(def.AlternateBurstCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(def.RecoveryDuration, Is.GreaterThan(0f));
            Assert.That(def.AlternateRecoveryDuration, Is.GreaterThan(0f));
            Assert.That(def.PrimaryBehavior, Is.Not.Null);
            Assert.That(def.AlternateBehavior, Is.Not.Null);
            Assert.That(def.PrimaryBehavior.IsValid(out _), Is.True);
            Assert.That(def.AlternateBehavior.IsValid(out _), Is.True);
        }

        [UnityTest]
        public IEnumerator EliteDemonPatternStartsAndEmitsPatternStartedEvent()
        {
            var elite = Spawn("Enemies/EliteDemon_Prototype");
            elite.transform.position = Vector3.zero;
            var pattern = elite.GetComponent<EnemyPatternController2D>();
            var agent = elite.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            bool targetSet = agent.TrySetTarget(playerReceiver);
            Assert.That(targetSet, Is.True);

            int reportedBurst = 0;
            int eventFiredCount = 0;
            pattern.PatternStarted += count =>
            {
                eventFiredCount++;
                reportedBurst = count;
            };

            yield return null;

            Assert.That(eventFiredCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(reportedBurst, Is.GreaterThanOrEqualTo(1));
            Assert.That(Field<bool>(pattern, "cycleStarted"), Is.True);
        }

        [UnityTest]
        public IEnumerator EliteDemonAlternatesPrimaryAndAlternateBehaviors()
        {
            var elite = Spawn("Enemies/EliteDemon_Prototype");
            elite.transform.position = Vector3.zero;
            var pattern = elite.GetComponent<EnemyPatternController2D>();
            var agent = elite.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);

            yield return null;

            EnemyPatternDefinition def = pattern.Definition;
            Assert.That(Field<bool>(pattern, "alternate"), Is.False);
            Assert.That(agent.Definition, Is.EqualTo(def.PrimaryBehavior));

            // Force burst depletion to trigger alternating transition
            SetField(pattern, "attacksRemaining", 0);

            yield return null;

            Assert.That(Field<bool>(pattern, "alternate"), Is.True);
            Assert.That(Field<float>(pattern, "rest"), Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator EliteDemonPatternResetsOnDisable()
        {
            var elite = Spawn("Enemies/EliteDemon_Prototype");
            elite.transform.position = Vector3.zero;
            var pattern = elite.GetComponent<EnemyPatternController2D>();
            var agent = elite.GetComponent<EnemyAgent2D>();

            var player = Spawn("Characters/Rumi_Prototype");
            player.transform.position = new Vector3(0.5f, 0f, 0f);
            var playerReceiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            agent.TrySetTarget(playerReceiver);

            yield return null;

            Assert.That(Field<bool>(pattern, "cycleStarted"), Is.True);

            elite.SetActive(false);

            Assert.That(Field<bool>(pattern, "cycleStarted"), Is.False);
            Assert.That(Field<int>(pattern, "attacksRemaining"), Is.Zero);
            Assert.That(Field<bool>(pattern, "alternate"), Is.False);
            Assert.That(Field<float>(pattern, "rest"), Is.Zero);
            Assert.That(agent.State, Is.EqualTo(EnemyState2D.Idle));
        }

        [Test]
        public void EliteDemonTrySetDefinitionValidatesInput()
        {
            var elite = Spawn("Enemies/EliteDemon_Prototype");
            var pattern = elite.GetComponent<EnemyPatternController2D>();

            Assert.That(pattern.TrySetDefinition(null, out string errNull), Is.False);
            Assert.That(errNull, Is.Not.Empty);

            var originalDef = pattern.Definition;
            Assert.That(pattern.TrySetDefinition(originalDef, out string errValid), Is.True, errValid);
            Assert.That(errValid, Is.Empty);
        }
    }
}
