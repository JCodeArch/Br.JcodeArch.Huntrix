using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class StageBlockoutBuilderPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject hostGo;
        private StageBlockoutBuilder2D builder;

        [SetUp]
        public void SetUp()
        {
            hostGo = new GameObject("BlockoutBuilderHost");
            builder = hostGo.AddComponent<StageBlockoutBuilder2D>();
        }

        [TearDown]
        public void TearDown()
        {
            if (hostGo != null)
            {
                Object.DestroyImmediate(hostGo);
            }
        }

        private static void Set(object target, string name, object value)
        {
            target.GetType().GetField(name, PrivateInstance).SetValue(target, value);
        }

        private static BlockoutPlatform CreatePlatform(Vector2 position, Vector2 size)
        {
            object platform = new BlockoutPlatform();
            typeof(BlockoutPlatform).GetField("position", PrivateInstance).SetValue(platform, position);
            typeof(BlockoutPlatform).GetField("size", PrivateInstance).SetValue(platform, size);
            return (BlockoutPlatform)platform;
        }

        private static StageBlockoutDefinition CreateValidStage(string stageId = "stage_test", string displayName = "Test Stage")
        {
            var def = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
            Set(def, "stageId", stageId);
            Set(def, "displayName", displayName);
            Set(def, "theme", StageBlockoutTheme.NightUrban);
            Set(def, "platforms", new[]
            {
                CreatePlatform(new Vector2(0f, 0f), new Vector2(8f, 1.5f)),
                CreatePlatform(new Vector2(12f, 4f), new Vector2(6f, 1f))
            });
            Set(def, "startPosition", new Vector2(0f, 1.5f));
            Set(def, "endPosition", new Vector2(15f, 5f));
            Set(def, "arenaCenter", new Vector2(6f, 3f));
            Set(def, "checkpoints", new[] { new Vector2(0f, 1.5f), new Vector2(12f, 4.5f) });
            Set(def, "enemySpawns", new[] { new Vector2(4f, 1.5f) });
            Set(def, "aerialSpawns", new[] { new Vector2(4f, 6f) });
            Set(def, "fanSpawns", new[] { new Vector2(2f, 1.5f) });
            return def;
        }

        [Test]
        public void TryBuildReturnsFalseOnNullOrInvalidDefinition()
        {
            Assert.That(builder.TryBuild(null), Is.False);
            Assert.That(builder.RuntimeRoot, Is.Null);

            var invalid = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
            try
            {
                Assert.That(builder.TryBuild(invalid), Is.False);
                Assert.That(builder.RuntimeRoot, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(invalid);
            }
        }

        [Test]
        public void TryBuildConstructsPlatformsWithCollidersAndVisualChildren()
        {
            var def = CreateValidStage("stage_urban", "Urban Stage");
            try
            {
                bool built = builder.TryBuild(def);
                Assert.That(built, Is.True);
                Assert.That(builder.RuntimeRoot, Is.Not.Null);
                Assert.That(builder.RuntimeRoot.name, Is.EqualTo("Blockout_stage_urban"));

                var p0 = builder.RuntimeRoot.transform.Find("Platform_0");
                Assert.That(p0, Is.Not.Null);
                var col0 = p0.GetComponent<BoxCollider2D>();
                Assert.That(col0, Is.Not.Null);
                Assert.That(col0.size, Is.EqualTo(new Vector2(8f, 1.5f)));

                var visual0 = p0.Find("Platform_Visual");
                Assert.That(visual0, Is.Not.Null);
                var renderer0 = visual0.GetComponent<SpriteRenderer>();
                Assert.That(renderer0, Is.Not.Null);
                Assert.That(renderer0.sprite, Is.Not.Null);
                Assert.That(visual0.localScale, Is.EqualTo(new Vector3(8f, 1.5f, 1f)));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TryBuildSetsStartAndEndPointsAtAuthoredCoordinates()
        {
            var def = CreateValidStage();
            try
            {
                builder.TryBuild(def);

                Assert.That(builder.StartPoint, Is.Not.Null);
                Assert.That((Vector2)builder.StartPoint.localPosition, Is.EqualTo(def.StartPosition));

                Assert.That(builder.EndPoint, Is.Not.Null);
                Assert.That((Vector2)builder.EndPoint.localPosition, Is.EqualTo(def.EndPosition));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TryBuildCreatesAndConfiguresCheckpointAnchors()
        {
            var def = CreateValidStage("stage_ckpt", "Checkpoint Stage");
            try
            {
                builder.TryBuild(def);

                Assert.That(builder.CheckpointAnchors.Count, Is.EqualTo(2));
                Assert.That(builder.CheckpointAnchors[0].CheckpointId, Is.EqualTo("stage_ckpt:checkpoint:0"));
                Assert.That(builder.CheckpointAnchors[1].CheckpointId, Is.EqualTo("stage_ckpt:checkpoint:1"));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TryClearCleansUpRuntimeObjectsAndResetsReferences()
        {
            var def = CreateValidStage();
            try
            {
                builder.TryBuild(def);
                var rootInstance = builder.RuntimeRoot;

                builder.TryClear();

                Assert.That(builder.RuntimeRoot, Is.Null);
                Assert.That(builder.Definition, Is.Null);
                Assert.That(builder.StartPoint, Is.Null);
                Assert.That(builder.EndPoint, Is.Null);
                Assert.That(builder.CheckpointAnchors.Count, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TryBuildRejectsReentrantBuildWithoutClearing()
        {
            var def1 = CreateValidStage("stage_1", "Stage 1");
            var def2 = CreateValidStage("stage_2", "Stage 2");
            try
            {
                Assert.That(builder.TryBuild(def1), Is.True);
                Assert.That(builder.TryBuild(def2), Is.False);
                Assert.That(builder.Definition, Is.SameAs(def1));
            }
            finally
            {
                Object.DestroyImmediate(def1);
                Object.DestroyImmediate(def2);
            }
        }
    }
}
