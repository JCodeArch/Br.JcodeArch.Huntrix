using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AerialStageEncounterPlayModeTests
    {
        private GameObject hostGo;
        private AerialStageEncounter2D encounter;
        private StageBlockoutBuilder2D builder;
        private CharacterManager2D manager;
        private GameObject validFlyerPrefab;

        [SetUp]
        public void SetUp()
        {
            hostGo = new GameObject("AerialEncounterHost");
            encounter = hostGo.AddComponent<AerialStageEncounter2D>();
            builder = hostGo.AddComponent<StageBlockoutBuilder2D>();
            manager = hostGo.AddComponent<CharacterManager2D>();
            validFlyerPrefab = Resources.Load<GameObject>("Enemies/FlyingDemon_Prototype");
        }

        [TearDown]
        public void TearDown()
        {
            if (hostGo != null)
            {
                Object.DestroyImmediate(hostGo);
            }
        }

        [Test]
        public void ConfigureRejectsNullParameters()
        {
            Assert.That(encounter.Configure(null, manager, validFlyerPrefab), Is.False);
            Assert.That(encounter.Configure(builder, null, validFlyerPrefab), Is.False);
            Assert.That(encounter.Configure(builder, manager, null), Is.False);
        }

        [Test]
        public void ConfigureRejectsSceneInstancePrefab()
        {
            var sceneInstance = new GameObject("FlyerSceneInstance");
            try
            {
                Assert.That(encounter.Configure(builder, manager, sceneInstance), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(sceneInstance);
            }
        }

        [Test]
        public void ConfigureAcceptsValidResourcesPrefab()
        {
            Assert.That(validFlyerPrefab, Is.Not.Null);
            Assert.That(validFlyerPrefab.scene.IsValid(), Is.False);

            bool configured = encounter.Configure(builder, manager, validFlyerPrefab);
            Assert.That(configured, Is.True);
            Assert.That(encounter.Count, Is.Zero);
        }

        [Test]
        public void TrySpawnReturnsFalseWhenBuilderHasNoRuntimeRoot()
        {
            encounter.Configure(builder, manager, validFlyerPrefab);
            Assert.That(builder.RuntimeRoot, Is.Null);

            bool spawned = encounter.TrySpawn();
            Assert.That(spawned, Is.False);
            Assert.That(encounter.Count, Is.Zero);
        }

        [Test]
        public void ClearResetsEncounterCountWithoutLeaking()
        {
            encounter.Configure(builder, manager, validFlyerPrefab);
            encounter.Clear();
            Assert.That(encounter.Count, Is.Zero);
            Assert.That(encounter.GetAt(0), Is.Null);
        }

        [Test]
        public void OnDisableCleansUpEncounter()
        {
            encounter.Configure(builder, manager, validFlyerPrefab);
            encounter.gameObject.SetActive(false);
            Assert.That(encounter.Count, Is.Zero);
        }
    }
}
