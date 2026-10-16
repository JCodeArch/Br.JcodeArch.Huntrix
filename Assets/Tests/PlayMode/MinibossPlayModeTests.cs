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
    public sealed class MinibossPlayModeTests
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

        private static void SetProperty(object instance, string propName, object value)
        {
            var prop = instance.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(prop, Is.Not.Null, "Property not found: " + propName);
            prop.SetValue(instance, value);
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
        public void MinibossPrototypeLoadsWithValidComponentsAndPatterns()
        {
            var miniboss = Spawn("Enemies/Miniboss_Prototype");
            var phaseController = miniboss.GetComponent<MinibossPhaseController2D>();
            var patternController = miniboss.GetComponent<EnemyPatternController2D>();
            var agent = miniboss.GetComponent<EnemyAgent2D>();
            var receiver = miniboss.GetComponent<DamageReceiver2D>();
            var body = miniboss.GetComponent<Rigidbody2D>();

            Assert.That(phaseController, Is.Not.Null);
            Assert.That(patternController, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(body, Is.Not.Null);

            var opening = Field<EnemyPatternDefinition>(phaseController, "openingPattern");
            var enraged = Field<EnemyPatternDefinition>(phaseController, "enragedPattern");
            float fraction = Field<float>(phaseController, "phaseHealthFraction");

            Assert.That(opening, Is.Not.Null);
            Assert.That(enraged, Is.Not.Null);
            Assert.That(opening.IsValid(out string errOpen), Is.True, errOpen);
            Assert.That(enraged.IsValid(out string errEnraged), Is.True, errEnraged);
            Assert.That(fraction, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator MinibossInitializesPhaseOneOnFirstUpdate()
        {
            var miniboss = Spawn("Enemies/Miniboss_Prototype");
            var phaseController = miniboss.GetComponent<MinibossPhaseController2D>();
            var patternController = miniboss.GetComponent<EnemyPatternController2D>();
            var opening = Field<EnemyPatternDefinition>(phaseController, "openingPattern");

            Assert.That(phaseController.Phase, Is.Zero);

            yield return null;

            Assert.That(phaseController.Phase, Is.EqualTo(1));
            Assert.That(patternController.Definition, Is.EqualTo(opening));
        }

        [UnityTest]
        public IEnumerator MinibossTransitionsToPhaseTwoWhenHealthReachesThreshold()
        {
            var miniboss = Spawn("Enemies/Miniboss_Prototype");
            var phaseController = miniboss.GetComponent<MinibossPhaseController2D>();
            var patternController = miniboss.GetComponent<EnemyPatternController2D>();
            var receiver = miniboss.GetComponent<DamageReceiver2D>();
            var enraged = Field<EnemyPatternDefinition>(phaseController, "enragedPattern");
            float fraction = Field<float>(phaseController, "phaseHealthFraction");

            yield return null;
            Assert.That(phaseController.Phase, Is.EqualTo(1));

            int phaseEvent = 0;
            phaseController.PhaseChanged += phase => phaseEvent = phase;

            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * (fraction - 0.05f));

            yield return null;

            Assert.That(phaseController.Phase, Is.EqualTo(2));
            Assert.That(phaseEvent, Is.EqualTo(2));
            Assert.That(patternController.Definition, Is.EqualTo(enraged));
        }

        [UnityTest]
        public IEnumerator MinibossPhaseDoesNotRegressIfHealed()
        {
            var miniboss = Spawn("Enemies/Miniboss_Prototype");
            var phaseController = miniboss.GetComponent<MinibossPhaseController2D>();
            var patternController = miniboss.GetComponent<EnemyPatternController2D>();
            var receiver = miniboss.GetComponent<DamageReceiver2D>();
            var enraged = Field<EnemyPatternDefinition>(phaseController, "enragedPattern");
            float fraction = Field<float>(phaseController, "phaseHealthFraction");

            yield return null;
            Assert.That(phaseController.Phase, Is.EqualTo(1));

            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * (fraction - 0.05f));

            yield return null;
            Assert.That(phaseController.Phase, Is.EqualTo(2));

            // Heal back to full
            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth);

            yield return null;

            Assert.That(phaseController.Phase, Is.EqualTo(2));
            Assert.That(patternController.Definition, Is.EqualTo(enraged));
        }

        [UnityTest]
        public IEnumerator MinibossDirectSpawnBelowThresholdEntersPhaseTwoDirectly()
        {
            var miniboss = Spawn("Enemies/Miniboss_Prototype");
            var phaseController = miniboss.GetComponent<MinibossPhaseController2D>();
            var patternController = miniboss.GetComponent<EnemyPatternController2D>();
            var receiver = miniboss.GetComponent<DamageReceiver2D>();
            var enraged = Field<EnemyPatternDefinition>(phaseController, "enragedPattern");
            float fraction = Field<float>(phaseController, "phaseHealthFraction");

            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * (fraction - 0.1f));

            yield return null;

            Assert.That(phaseController.Phase, Is.EqualTo(2));
            Assert.That(patternController.Definition, Is.EqualTo(enraged));
        }
    }
}
