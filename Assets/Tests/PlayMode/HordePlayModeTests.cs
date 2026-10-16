using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class HordePlayModeTests
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
        public void GroundHordeEncounterPrototypeLoadsWithValidComponentsAndDefinition()
        {
            var encounter = Spawn("Enemies/GroundHordeEncounter_Prototype");
            var horde = encounter.GetComponent<HordeController2D>();
            var bootstrap = encounter.GetComponent<EnemyEncounterBootstrap2D>();

            Assert.That(horde, Is.Not.Null);
            Assert.That(bootstrap, Is.Not.Null);

            var def = Field<HordeDefinition>(horde, "definition");
            Assert.That(def, Is.Not.Null);
            Assert.That(def.IsValid(out string error), Is.True, error);
            Assert.That(def.EnemyPrefab, Is.Not.Null);
            Assert.That(def.MaximumConcurrent, Is.GreaterThan(0).And.LessThanOrEqualTo(64));
            Assert.That(def.TotalSpawns, Is.GreaterThan(0).And.LessThanOrEqualTo(256));
            Assert.That(def.SpawnInterval, Is.GreaterThan(0f));
        }

        [Test]
        public void HordeControllerTryConfigureValidatesParameters()
        {
            var encounter = Spawn("Enemies/GroundHordeEncounter_Prototype");
            var horde = encounter.GetComponent<HordeController2D>();
            var def = Field<HordeDefinition>(horde, "definition");

            var spawnPoint = new GameObject("SpawnPoint").transform;
            instances.Add(spawnPoint.gameObject);
            var points = new Transform[] { spawnPoint };

            var player = Spawn("Characters/Rumi_Prototype");
            var receiver = player.GetComponent<DamageReceiver2D>();

            Assert.That(horde.TryConfigure(null, points, null, receiver), Is.False);
            Assert.That(horde.TryConfigure(def, null, null, receiver), Is.False);
            Assert.That(horde.TryConfigure(def, new Transform[0], null, receiver), Is.False);
            Assert.That(horde.TryConfigure(def, points, null, null), Is.False);

            bool configured = horde.TryConfigure(def, points, null, receiver);
            Assert.That(configured, Is.True);
            Assert.That(horde.SpawnedCount, Is.Zero);
            Assert.That(horde.IsComplete, Is.False);
        }

        [UnityTest]
        public IEnumerator HordeControllerTryStartAndSpawnsWithinConcurrentLimit()
        {
            var encounter = Spawn("Enemies/GroundHordeEncounter_Prototype");
            var horde = encounter.GetComponent<HordeController2D>();
            var def = Field<HordeDefinition>(horde, "definition");

            var spawnPoint = new GameObject("SpawnPoint").transform;
            instances.Add(spawnPoint.gameObject);
            var points = new Transform[] { spawnPoint };

            var player = Spawn("Characters/Rumi_Prototype");
            var receiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            horde.TryConfigure(def, points, null, receiver);
            bool started = horde.TryStart();
            Assert.That(started, Is.True);
            Assert.That(horde.IsRunning, Is.True);

            yield return null;

            Assert.That(horde.SpawnedCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(horde.AliveCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(horde.AliveCount, Is.LessThanOrEqualTo(horde.EffectiveMaximumConcurrent));

            var firstActor = horde.GetOwnedAt(0);
            Assert.That(firstActor, Is.Not.Null);
            Assert.That(firstActor.HasValidTarget, Is.True);
            Assert.That(firstActor.Target, Is.EqualTo(receiver));
        }

        [Test]
        public void HordeControllerIntensityScalesCadenceAndConcurrency()
        {
            var encounter = Spawn("Enemies/GroundHordeEncounter_Prototype");
            var horde = encounter.GetComponent<HordeController2D>();
            var def = Field<HordeDefinition>(horde, "definition");

            float baseInterval = horde.EffectiveSpawnInterval;
            int baseConcurrent = horde.EffectiveMaximumConcurrent;

            Assert.That(baseInterval, Is.EqualTo(def.SpawnInterval));
            Assert.That(baseConcurrent, Is.EqualTo(def.MaximumConcurrent));

            bool intensitySet = horde.TrySetIntensity(encounter.GetComponent<EnemyEncounterBootstrap2D>(), 0.5f, 4);
            Assert.That(intensitySet, Is.True);
            Assert.That(horde.EffectiveSpawnInterval, Is.EqualTo(baseInterval * 0.5f));
            Assert.That(horde.EffectiveMaximumConcurrent, Is.EqualTo(baseConcurrent + 4));

            horde.ClearIntensity(encounter.GetComponent<EnemyEncounterBootstrap2D>());
            Assert.That(horde.EffectiveSpawnInterval, Is.EqualTo(baseInterval));
            Assert.That(horde.EffectiveMaximumConcurrent, Is.EqualTo(baseConcurrent));
        }

        [UnityTest]
        public IEnumerator HordeControllerStopAndDisableCleansUpActorsWithoutLeaking()
        {
            var encounter = Spawn("Enemies/GroundHordeEncounter_Prototype");
            var horde = encounter.GetComponent<HordeController2D>();
            var def = Field<HordeDefinition>(horde, "definition");

            var spawnPoint = new GameObject("SpawnPoint").transform;
            instances.Add(spawnPoint.gameObject);
            var points = new Transform[] { spawnPoint };

            var player = Spawn("Characters/Rumi_Prototype");
            var receiver = player.GetComponent<DamageReceiver2D>();

            yield return null;

            horde.TryConfigure(def, points, null, receiver);
            horde.TryStart();

            yield return null;
            Assert.That(horde.AliveCount, Is.GreaterThanOrEqualTo(1));

            horde.Stop();

            Assert.That(horde.IsRunning, Is.False);
            Assert.That(horde.AliveCount, Is.Zero);
            Assert.That(horde.GetOwnedAt(0), Is.Null);
        }
    }
}
