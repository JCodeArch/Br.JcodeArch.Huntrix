using System;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class BlockoutLevelControllerPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<Object> disposables = new List<Object>();

        private GameObject hostGo;
        private BlockoutLevelController2D controller;
        private StageBlockoutBuilder2D builder;
        private CharacterManager2D player;
        private CheckpointAttemptFlow2D flow;
        private GameObject playerPrefab;
        private StageRouteDefinition route;
        private StageBlockoutDefinition stageA;
        private StageBlockoutDefinition stageB;

        [SetUp]
        public void SetUp()
        {
            playerPrefab = Resources.Load<GameObject>("Characters/Rumi_Prototype");
            Assert.That(playerPrefab, Is.Not.Null, "The Rumi prototype prefab must exist in Resources.");

            hostGo = new GameObject("BlockoutLevelHost");
            hostGo.SetActive(false);
            disposables.Add(hostGo);

            controller = hostGo.AddComponent<BlockoutLevelController2D>();
            builder = hostGo.AddComponent<StageBlockoutBuilder2D>();
            player = hostGo.AddComponent<CharacterManager2D>();
            flow = hostGo.AddComponent<CheckpointAttemptFlow2D>();

            stageA = CreateValidStage("stage_alpha", "Alpha Blockout Stage");
            stageB = CreateValidStage("stage_beta", "Beta Blockout Stage");
            disposables.Add(stageA);
            disposables.Add(stageB);

            route = ScriptableObject.CreateInstance<StageRouteDefinition>();
            Set(route, "stages", new[] { stageA, stageB });
            disposables.Add(route);

            Set(controller, "builder", builder);
            Set(controller, "route", route);
            Set(controller, "player", player);
            Set(controller, "flow", flow);
            Set(controller, "playerPrefab", playerPrefab);
            Set(controller, "autoStart", false);

            hostGo.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                {
                    Object.DestroyImmediate(disposables[i]);
                }
            }
            disposables.Clear();
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Field '{name}' must exist on '{target.GetType().Name}'.");
            field.SetValue(target, value);
        }

        private static BlockoutPlatform CreatePlatform(Vector2 position, Vector2 size)
        {
            object platform = new BlockoutPlatform();
            typeof(BlockoutPlatform).GetField("position", PrivateInstance).SetValue(platform, position);
            typeof(BlockoutPlatform).GetField("size", PrivateInstance).SetValue(platform, size);
            return (BlockoutPlatform)platform;
        }

        private static StageBlockoutDefinition CreateValidStage(string stageId, string displayName)
        {
            var def = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
            Set(def, "stageId", stageId);
            Set(def, "displayName", displayName);
            Set(def, "theme", StageBlockoutTheme.NightUrban);
            Set(def, "platforms", new[]
            {
                CreatePlatform(new Vector2(0f, 0f), new Vector2(10f, 1.5f)),
                CreatePlatform(new Vector2(15f, 3f), new Vector2(8f, 1f))
            });
            Set(def, "startPosition", new Vector2(0f, 1.5f));
            Set(def, "endPosition", new Vector2(18f, 4f));
            Set(def, "arenaCenter", new Vector2(8f, 2f));
            Set(def, "checkpoints", new[] { new Vector2(0f, 1.5f), new Vector2(14f, 3.5f) });
            Set(def, "enemySpawns", new[] { new Vector2(5f, 1.5f) });
            Set(def, "aerialSpawns", new[] { new Vector2(5f, 5f) });
            Set(def, "fanSpawns", new[] { new Vector2(2f, 1.5f) });
            return def;
        }

        [Test]
        public void InitialStageIndexDefaultsToNegativeOneWhenAutoStartIsDisabled()
        {
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(-1));
        }

        [Test]
        public void TryLoadReturnsFalseOnMissingDependenciesOrInvalidIndex()
        {
            Assert.That(controller.TryLoad(-1), Is.False);
            Assert.That(controller.TryLoad(2), Is.False);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(-1));

            Set(controller, "builder", null);
            Assert.That(controller.TryLoad(0), Is.False);
            Set(controller, "builder", builder);

            Set(controller, "route", null);
            Assert.That(controller.TryLoad(0), Is.False);
            Set(controller, "route", route);

            Set(controller, "player", null);
            Assert.That(controller.TryLoad(0), Is.False);
            Set(controller, "player", player);

            Set(controller, "flow", null);
            Assert.That(controller.TryLoad(0), Is.False);
            Set(controller, "flow", flow);

            Set(controller, "playerPrefab", null);
            Assert.That(controller.TryLoad(0), Is.False);
            Set(controller, "playerPrefab", playerPrefab);
        }

        [Test]
        public void TryLoadRejectsInvalidRoute()
        {
            var invalidRoute = ScriptableObject.CreateInstance<StageRouteDefinition>();
            disposables.Add(invalidRoute);
            Set(controller, "route", invalidRoute);

            Assert.That(controller.TryLoad(0), Is.False);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(-1));
        }

        [Test]
        public void TryLoadConstructsStageSpawnsPlayerWiresCheckpointsAndFiresEvent()
        {
            StageBlockoutDefinition loadedStage = null;
            controller.StageLoaded += stage => loadedStage = stage;

            bool loaded = controller.TryLoad(0);

            Assert.That(loaded, Is.True);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(0));
            Assert.That(loadedStage, Is.SameAs(stageA));
            Assert.That(builder.RuntimeRoot, Is.Not.Null);
            Assert.That(builder.RuntimeRoot.name, Is.EqualTo("Blockout_stage_alpha"));
            Assert.That(player.ActiveCharacter, Is.Not.Null);
            Assert.That(player.ActiveCharacter.IsAlive, Is.True);

            Assert.That(builder.CheckpointAnchors.Count, Is.EqualTo(2));
            foreach (CheckpointAnchor2D checkpoint in builder.CheckpointAnchors)
            {
                BoxCollider2D collider = checkpoint.GetComponent<BoxCollider2D>();
                Assert.That(collider, Is.Not.Null);
                Assert.That(collider.isTrigger, Is.True);
                Assert.That(collider.size, Is.EqualTo(new Vector2(1.5f, 2f)));

                BlockoutCheckpointRegion2D region = checkpoint.GetComponent<BlockoutCheckpointRegion2D>();
                Assert.That(region, Is.Not.Null);
            }
        }

        [Test]
        public void TryLoadAdvancesToNextStageAndClearsPreviousStage()
        {
            Assert.That(controller.TryLoad(0), Is.True);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(0));
            Assert.That(builder.RuntimeRoot.name, Is.EqualTo("Blockout_stage_alpha"));

            Assert.That(controller.TryLoad(1), Is.True);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(1));
            Assert.That(builder.RuntimeRoot.name, Is.EqualTo("Blockout_stage_beta"));
        }

        [Test]
        public void OnDisableClearsBuilderAndResetsStageIndex()
        {
            Assert.That(controller.TryLoad(0), Is.True);
            Assert.That(controller.CurrentStageIndex, Is.EqualTo(0));
            Assert.That(builder.RuntimeRoot, Is.Not.Null);

            controller.enabled = false;

            Assert.That(controller.CurrentStageIndex, Is.EqualTo(-1));
            Assert.That(builder.RuntimeRoot, Is.Null);
            Assert.That(player.enabled, Is.False);
        }

        [Test]
        public void BlockoutCheckpointRegionActivatesCheckpointWhenMatchingPlayerEnters()
        {
            Assert.That(controller.TryLoad(0), Is.True);
            CheckpointAnchor2D anchor = builder.CheckpointAnchors[1];
            BlockoutCheckpointRegion2D region = anchor.GetComponent<BlockoutCheckpointRegion2D>();
            Assert.That(region, Is.Not.Null);

            MethodInfo onTriggerEnter = typeof(BlockoutCheckpointRegion2D)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(onTriggerEnter, Is.Not.Null);

            GameObject foreignGo = new GameObject("ForeignReceiver");
            disposables.Add(foreignGo);
            foreignGo.AddComponent<BoxCollider2D>();
            LogAssert.Expect(LogType.Error, "DamageReceiver2D requires finite positive maximum health.");
            DamageReceiver2D foreignReceiver = foreignGo.AddComponent<DamageReceiver2D>();
            foreignReceiver.Configure(100f, CombatFaction2D.Demon, 1f, null, 1f, 1f);

            onTriggerEnter.Invoke(region, new object[] { foreignGo.GetComponent<Collider2D>() });
            Assert.That(flow.ActiveCheckpoint, Is.Null);

            Collider2D playerCollider = player.ActiveCharacter.GetComponentInChildren<Collider2D>();
            Assert.That(playerCollider, Is.Not.Null);

            onTriggerEnter.Invoke(region, new object[] { playerCollider });
            Assert.That(flow.ActiveCheckpoint, Is.SameAs(anchor));
        }
    }
}
