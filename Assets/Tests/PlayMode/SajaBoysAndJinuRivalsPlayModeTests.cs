using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Rivals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SajaBoysAndJinuRivalsPlayModeTests
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

        [UnityTest]
        public IEnumerator ReconfiguredRivalArtworkFollowsAuthoredIdentityInsteadOfInstanceHash()
        {
            var go = Spawn("Rivals/Abby_Prototype");
            go.name = "Rival with no character name";
            var rival = go.GetComponent<SajaRivalController2D>();
            var visual = go.GetComponent<EnemyPrototypeVisual2D>();
            if (visual == null) visual = go.AddComponent<EnemyPrototypeVisual2D>();
            foreach (string key in new[] { "Abby", "Baby", "Mystery", "Romance" })
            {
                var template = UnityEngine.Resources.Load<GameObject>("Rivals/" + key + "_Prototype");
                var definition = template.GetComponent<SajaRivalController2D>().Definition;
                Assert.That(rival.TrySetDefinition(definition, out string error), Is.True, error);
                yield return null;
                var rendered = go.transform.Find("Prototype silhouette").GetComponent<SpriteRenderer>();
                var expected = UnityEngine.Resources.Load<Sprite>("Art/Characters/" + key);
                Texture2D expectedTexture = expected != null ? expected.texture :
                    UnityEngine.Resources.Load<Texture2D>("Art/Characters/" + key);
                Assert.That(expectedTexture, Is.Not.Null, key + " artwork");
                if (expected != null)
                    Assert.That(rendered.sprite, Is.SameAs(expected), key + " must keep its authored sprite identity.");
                else
                    Assert.That(rendered.sprite.texture, Is.SameAs(expectedTexture),
                        key + " must retain the visual identity of its configured combat profile.");
            }
        }

        [Test]
        public void AllFourSajaBoysPrototypesLoadAndHaveDistinctIdentitiesAndTactics()
        {
            string[] paths =
            {
                "Rivals/Abby_Prototype",
                "Rivals/Baby_Prototype",
                "Rivals/Mystery_Prototype",
                "Rivals/Romance_Prototype"
            };

            var seenIdentities = new HashSet<SajaIdentity>();
            var seenTactics = new HashSet<SajaTactic>();

            foreach (string path in paths)
            {
                var go = Spawn(path);
                var rival = go.GetComponent<SajaRivalController2D>();
                var agent = go.GetComponent<EnemyAgent2D>();
                var receiver = go.GetComponent<DamageReceiver2D>();

                Assert.That(rival, Is.Not.Null, path);
                Assert.That(agent, Is.Not.Null, path);
                Assert.That(receiver, Is.Not.Null, path);

                var def = rival.Definition;
                Assert.That(def, Is.Not.Null, path + " definition");
                Assert.That(def.IsValid(out string error), Is.True, error);
                Assert.That(def.BurstCount, Is.GreaterThanOrEqualTo(1));
                Assert.That(def.PauseBetweenBursts, Is.GreaterThanOrEqualTo(0f));
                Assert.That(def.PreferredDistance, Is.GreaterThan(0f));

                seenIdentities.Add(def.Identity);
                seenTactics.Add(def.Tactic);
            }

            Assert.That(seenIdentities.Count, Is.EqualTo(4), "All 4 Saja Boys must have distinct identities.");
            Assert.That(seenTactics.Count, Is.GreaterThanOrEqualTo(3), "Saja Boys must have distinct tactical roles.");
        }

        [UnityTest]
        public IEnumerator SajaRivalControllerKeepDistanceMovesAwayFromCloseTarget()
        {
            var romanceGo = Spawn("Rivals/Romance_Prototype");
            var rival = romanceGo.GetComponent<SajaRivalController2D>();
            var agent = romanceGo.GetComponent<EnemyAgent2D>();
            var body = romanceGo.GetComponent<Rigidbody2D>();

            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var targetReceiver = rumiGo.GetComponent<DamageReceiver2D>();

            // Place Romance at origin and Rumi close to the right
            romanceGo.transform.position = Vector3.zero;
            rumiGo.transform.position = new Vector3(rival.Definition.PreferredDistance * 0.5f, 0f, 0f);

            Assert.That(agent.TrySetTarget(targetReceiver), Is.True);
            yield return null;

            InvokeMethod(rival, "FixedUpdate");

            // Since target is to the right (+x) within PreferredDistance, rival should retreat left (-x)
            Assert.That(body.linearVelocityX, Is.LessThan(0f));
        }

        [UnityTest]
        public IEnumerator JinuEncounterPrefabLoadsWithValidDefinitionAndTriggersEncounterCueOnStart()
        {
            var go = Spawn("Rivals/Jinu_Prototype");
            var encounter = go.GetComponent<JinuEncounterController2D>();
            var rival = go.GetComponent<SajaRivalController2D>();
            var agent = go.GetComponent<EnemyAgent2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            Assert.That(encounter, Is.Not.Null);
            Assert.That(rival, Is.Not.Null);
            Assert.That(agent, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);

            var definitionField = typeof(JinuEncounterController2D).GetField("definition", NonPublicInstance);
            var def = (JinuEncounterDefinition)definitionField.GetValue(encounter);
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);
            Assert.That(def.Opening.Identity, Is.EqualTo(SajaIdentity.Jinu));
            Assert.That(def.Pressure.Identity, Is.EqualTo(SajaIdentity.Jinu));

            int cueCount = 0;
            JinuEncounterStage reportedStage = JinuEncounterStage.Uninitialized;
            string reportedCue = null;

            encounter.NarrativeCueRequested += (stage, cue) =>
            {
                cueCount++;
                reportedStage = stage;
                reportedCue = cue;
            };

            yield return null; // Executes Start()

            Assert.That(encounter.Stage, Is.EqualTo(JinuEncounterStage.Encounter));
            Assert.That(cueCount, Is.EqualTo(1));
            Assert.That(reportedStage, Is.EqualTo(JinuEncounterStage.Encounter));
            Assert.That(reportedCue, Is.EqualTo(def.EncounterCue));
        }

        [UnityTest]
        public IEnumerator JinuEncounterAdvancesToPressureAtHealthThreshold()
        {
            var go = Spawn("Rivals/Jinu_Prototype");
            var encounter = go.GetComponent<JinuEncounterController2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            var cues = new List<string>();
            encounter.NarrativeCueRequested += (stage, cue) => cues.Add(cue);

            yield return null; // Start
            Assert.That(encounter.Stage, Is.EqualTo(JinuEncounterStage.Encounter));

            var definitionField = typeof(JinuEncounterController2D).GetField("definition", NonPublicInstance);
            var def = (JinuEncounterDefinition)definitionField.GetValue(encounter);

            // Drop health below PressureHealthFraction (0.5f)
            SetProperty(receiver, "CurrentHealth", receiver.MaximumHealth * (def.PressureHealthFraction - 0.1f));
            InvokeMethod(encounter, "Update");

            Assert.That(encounter.Stage, Is.EqualTo(JinuEncounterStage.Pressure));
            Assert.That(cues.Count, Is.EqualTo(2));
            Assert.That(cues[1], Is.EqualTo(def.PressureCue));
        }

        [UnityTest]
        public IEnumerator JinuEncounterTransitionsToDefeatedOnDeath()
        {
            var go = Spawn("Rivals/Jinu_Prototype");
            var encounter = go.GetComponent<JinuEncounterController2D>();
            var receiver = go.GetComponent<DamageReceiver2D>();

            var cues = new List<string>();
            encounter.NarrativeCueRequested += (stage, cue) => cues.Add(cue);

            yield return null; // Start

            var definitionField = typeof(JinuEncounterController2D).GetField("definition", NonPublicInstance);
            var def = (JinuEncounterDefinition)definitionField.GetValue(encounter);

            // Trigger death via HandleDeath
            InvokeMethod(encounter, "HandleDeath", new object[] { receiver });

            Assert.That(encounter.Stage, Is.EqualTo(JinuEncounterStage.Defeated));
            Assert.That(cues.Count, Is.EqualTo(2));
            Assert.That(cues[1], Is.EqualTo(def.DefeatedCue));

            // Secondary call must be idempotent
            InvokeMethod(encounter, "HandleDeath", new object[] { receiver });
            Assert.That(cues.Count, Is.EqualTo(2));
        }
    }
}
