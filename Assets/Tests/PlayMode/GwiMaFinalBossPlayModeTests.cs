using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.BossFinal;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Narrative;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class GwiMaFinalBossPlayModeTests
    {
        private static readonly BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<GameObject> instances = new List<GameObject>();

        private GameObject Spawn(string path)
        {
            var prefab = UnityEngine.Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instances.Add(instance);
            return instance;
        }

        private static void InvokeMethod(object instance, string methodName, object[] parameters = null)
        {
            var method = instance.GetType().GetMethod(methodName, NonPublicInstance);
            Assert.That(method, Is.Not.Null, "Method not found: " + methodName);
            method.Invoke(instance, parameters);
        }

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
        public void GwiMaPrefabLoadsWithValidDefinitionAndThreeDistinctPhases()
        {
            var go = Spawn("Enemies/GwiMa_Prototype");
            var boss = go.GetComponent<GwiMaBossController2D>();
            var agent = go.GetComponent<EnemyAgent2D>();
            var pattern = go.GetComponent<EnemyPatternController2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            Assert.That(boss, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(pattern, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);

            var def = boss.Definition;
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);

            Assert.That(def.Opening, Is.Not.Null);
            Assert.That(def.Pressure, Is.Not.Null);
            Assert.That(def.Finale, Is.Not.Null);
            Assert.That(def.Opening, Is.Not.EqualTo(def.Pressure));
            Assert.That(def.Pressure, Is.Not.EqualTo(def.Finale));
            Assert.That(def.Opening, Is.Not.EqualTo(def.Finale));

            Assert.That(def.FinaleHealthFraction, Is.GreaterThan(0f));
            Assert.That(def.PressureHealthFraction, Is.GreaterThan(def.FinaleHealthFraction));
            Assert.That(def.PressureHealthFraction, Is.LessThan(1f));
            Assert.That(string.IsNullOrWhiteSpace(def.EndingCue), Is.False);
        }

        [UnityTest]
        public IEnumerator GwiMaStartsInOpeningAndAdvancesMonotonicallyThroughPhases()
        {
            var go = Spawn("Enemies/GwiMa_Prototype");
            var boss = go.GetComponent<GwiMaBossController2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            var recordedPhases = new List<GwiMaPhase2D>();
            boss.PhaseChanged += recordedPhases.Add;

            yield return null;
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Opening));

            // Set health below pressure threshold (e.g., 60% < 70%)
            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * 0.6f);
            InvokeMethod(boss, "Update");
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Pressure));

            // Set health below finale threshold (e.g., 20% < 35%)
            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * 0.2f);
            InvokeMethod(boss, "Update");
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Finale));

            // Health increase must NOT regress phase (monotonic)
            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * 0.9f);
            InvokeMethod(boss, "Update");
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Finale));

            CollectionAssert.AreEqual(
                new[] { GwiMaPhase2D.Opening, GwiMaPhase2D.Pressure, GwiMaPhase2D.Finale },
                recordedPhases);
        }

        [UnityTest]
        public IEnumerator GwiMaDeathTransitionsToDefeatedAndPublishesEndingOutcome()
        {
            var go = Spawn("Enemies/GwiMa_Prototype");
            var boss = go.GetComponent<GwiMaBossController2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            yield return null;
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Opening));

            int endingEvents = 0;
            FinalBossOutcome2D publishedOutcome = default;
            boss.EndingRequested += outcome =>
            {
                endingEvents++;
                publishedOutcome = outcome;
            };

            // Trigger death via HandleDeath
            InvokeMethod(boss, "HandleDeath", new object[] { receiver });

            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Defeated));
            Assert.That(endingEvents, Is.EqualTo(1));
            Assert.That(publishedOutcome.BossId, Is.EqualTo(boss.Definition.Id));
            Assert.That(publishedOutcome.EndingCue, Is.EqualTo(boss.Definition.EndingCue));
            Assert.That(publishedOutcome.Position, Is.EqualTo(boss.transform.position));

            Assert.That(boss.TryGetFinalOutcome(out var storedOutcome), Is.True);
            Assert.That(storedOutcome.BossId, Is.EqualTo(publishedOutcome.BossId));
            Assert.That(storedOutcome.EndingCue, Is.EqualTo(publishedOutcome.EndingCue));

            // Secondary call must be idempotent and not re-publish
            InvokeMethod(boss, "HandleDeath", new object[] { receiver });
            Assert.That(endingEvents, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator GwiMaSuspendsPatternOnDisableAndRestoresOnEnable()
        {
            var go = Spawn("Enemies/GwiMa_Prototype");
            var boss = go.GetComponent<GwiMaBossController2D>();
            var pattern = go.GetComponent<EnemyPatternController2D>();

            yield return null;
            Assert.That(pattern.enabled, Is.True);

            go.SetActive(false);
            Assert.That(pattern.enabled, Is.False);

            go.SetActive(true);
            Assert.That(pattern.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator FinalBossEndingControllerBindsBossOutcome()
        {
            var endingGo = Spawn("Narrative/GwiMaEnding_OriginalPrototype");
            var endingController = endingGo.GetComponent<FinalBossEndingController2D>();
            var boss = endingGo.GetComponent<GwiMaBossController2D>();
            var receiver = endingGo.GetComponent<DamageReceiver2D>();

            Assert.That(endingController, Is.Not.Null);
            Assert.That(boss, Is.Not.Null);

            yield return null;

            // Trigger boss death
            InvokeMethod(boss, "HandleDeath", new object[] { receiver });

            // Boss must be defeated and outcome retrieved
            Assert.That(boss.Phase, Is.EqualTo(GwiMaPhase2D.Defeated));
            Assert.That(boss.TryGetFinalOutcome(out var outcome), Is.True);
            Assert.That(outcome.EndingCue, Is.EqualTo(boss.Definition.EndingCue));
        }
    }
}
