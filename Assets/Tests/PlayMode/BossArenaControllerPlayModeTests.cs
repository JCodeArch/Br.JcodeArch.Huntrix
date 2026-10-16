using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Levels;
using HuntrX.Gameplay.Performance;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class BossArenaControllerPlayModeTests
    {
        private GameObject hostGo;
        private BossArenaController2D arena;
        private StageBlockoutBuilder2D builder;
        private CharacterManager2D manager;
        private FanRegistry2D fans;
        private HonmoonController2D honmoon;
        private CombatPerformanceController2D performance;
        private GameObject validBossPrefab;

        [SetUp]
        public void SetUp()
        {
            hostGo = new GameObject("BossArenaHost");
            arena = hostGo.AddComponent<BossArenaController2D>();
            builder = hostGo.AddComponent<StageBlockoutBuilder2D>();
            manager = hostGo.AddComponent<CharacterManager2D>();
            fans = hostGo.AddComponent<FanRegistry2D>();
            honmoon = hostGo.AddComponent<HonmoonController2D>();
            performance = hostGo.AddComponent<CombatPerformanceController2D>();
            validBossPrefab = Resources.Load<GameObject>("Enemies/Miniboss_Prototype");
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
            Assert.That(arena.Configure(null, manager, validBossPrefab, fans, honmoon, performance), Is.False);
            Assert.That(arena.Configure(builder, null, validBossPrefab, fans, honmoon, performance), Is.False);
            Assert.That(arena.Configure(builder, manager, null, fans, honmoon, performance), Is.False);
            Assert.That(arena.Configure(builder, manager, validBossPrefab, null, honmoon, performance), Is.False);
            Assert.That(arena.Configure(builder, manager, validBossPrefab, fans, null, performance), Is.False);
            Assert.That(arena.Configure(builder, manager, validBossPrefab, fans, honmoon, null), Is.False);
        }

        [Test]
        public void ConfigureRejectsSceneInstancePrefab()
        {
            var sceneInstance = new GameObject("BossSceneInstance");
            try
            {
                Assert.That(arena.Configure(builder, manager, sceneInstance, fans, honmoon, performance), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(sceneInstance);
            }
        }

        [Test]
        public void ConfigureAcceptsValidResourcesPrefab()
        {
            Assert.That(validBossPrefab, Is.Not.Null);
            Assert.That(validBossPrefab.scene.IsValid(), Is.False);

            bool configured = arena.Configure(builder, manager, validBossPrefab, fans, honmoon, performance);
            Assert.That(configured, Is.True);
            Assert.That(arena.Boss, Is.Null);
            Assert.That(arena.IsComplete, Is.False);
        }

        [Test]
        public void TrySpawnFailsWhenRuntimeRootIsNotBuilt()
        {
            arena.Configure(builder, manager, validBossPrefab, fans, honmoon, performance);
            Assert.That(builder.RuntimeRoot, Is.Null);

            bool spawned = arena.TrySpawn();
            Assert.That(spawned, Is.False);
            Assert.That(arena.Boss, Is.Null);
        }

        [Test]
        public void ClearResetsBossStateAndCompletion()
        {
            arena.Configure(builder, manager, validBossPrefab, fans, honmoon, performance);
            arena.Clear();
            Assert.That(arena.Boss, Is.Null);
            Assert.That(arena.IsComplete, Is.False);
        }

        [Test]
        public void OnDisableCleansUpArenaState()
        {
            arena.Configure(builder, manager, validBossPrefab, fans, honmoon, performance);
            arena.gameObject.SetActive(false);
            Assert.That(arena.Boss, Is.Null);
            Assert.That(arena.IsComplete, Is.False);
        }
    }
}
