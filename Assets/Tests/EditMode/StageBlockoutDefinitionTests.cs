using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class StageBlockoutDefinitionTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private static void Set(object target, string name, object value)
        {
            target.GetType().GetField(name, PrivateInstance).SetValue(target, value);
        }

        private static BlockoutPlatform CreatePlatform(Vector2 position, Vector2 size)
        {
            var platform = new BlockoutPlatform();
            typeof(BlockoutPlatform).GetField("position", PrivateInstance).SetValue(platform, position);
            typeof(BlockoutPlatform).GetField("size", PrivateInstance).SetValue(platform, size);
            return platform;
        }

        private static StageBlockoutDefinition CreateValidStage(string stageId = "stage_01", string displayName = "Urban Street")
        {
            var def = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
            Set(def, "stageId", stageId);
            Set(def, "displayName", displayName);
            Set(def, "theme", StageBlockoutTheme.DayUrban);
            Set(def, "platforms", new[]
            {
                CreatePlatform(new Vector2(0f, 0f), new Vector2(10f, 1f)),
                CreatePlatform(new Vector2(15f, 3f), new Vector2(6f, 1f))
            });
            Set(def, "startPosition", new Vector2(0f, 1f));
            Set(def, "endPosition", new Vector2(20f, 3f));
            Set(def, "arenaCenter", new Vector2(10f, 5f));
            Set(def, "checkpoints", new[] { new Vector2(0f, 1f), new Vector2(12f, 2f) });
            Set(def, "enemySpawns", new[] { new Vector2(5f, 1f), new Vector2(8f, 1f) });
            Set(def, "aerialSpawns", new[] { new Vector2(5f, 6f), new Vector2(10f, 7f) });
            Set(def, "fanSpawns", new[] { new Vector2(3f, 1f), new Vector2(14f, 3f) });
            return def;
        }

        [Test]
        public void ValidationRejectsBlankIdentity()
        {
            var def = CreateValidStage();
            try
            {
                Set(def, "stageId", "  ");
                Assert.That(def.IsValid(out string error), Is.False);
                Assert.That(error, Is.Not.Empty);

                Set(def, "stageId", "valid_id");
                Set(def, "displayName", "");
                Assert.That(def.IsValid(out error), Is.False);
                Assert.That(error, Is.Not.Empty);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void ValidationRejectsEmptyOrExcessivePlatforms()
        {
            var def = CreateValidStage();
            try
            {
                Set(def, "platforms", new BlockoutPlatform[0]);
                Assert.That(def.IsValid(out _), Is.False);

                var excessive = new BlockoutPlatform[129];
                for (int i = 0; i < excessive.Length; i++)
                {
                    excessive[i] = CreatePlatform(new Vector2(i, 0), Vector2.one);
                }
                Set(def, "platforms", excessive);
                Assert.That(def.IsValid(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void ValidationRejectsInvalidPlatformSizesAndCoordinates()
        {
            var def = CreateValidStage();
            try
            {
                Set(def, "platforms", new[] { CreatePlatform(Vector2.zero, new Vector2(0f, 1f)) });
                Assert.That(def.IsValid(out _), Is.False);

                Set(def, "platforms", new[] { CreatePlatform(Vector2.zero, new Vector2(5f, -1f)) });
                Assert.That(def.IsValid(out _), Is.False);

                Set(def, "platforms", new[] { CreatePlatform(new Vector2(float.NaN, 0f), Vector2.one) });
                Assert.That(def.IsValid(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void ValidationRejectsInvalidOrExcessiveSpawnPoints()
        {
            var def = CreateValidStage();
            try
            {
                var tooManyCheckpoints = new Vector2[9];
                Set(def, "checkpoints", tooManyCheckpoints);
                Assert.That(def.IsValid(out _), Is.False);
                Set(def, "checkpoints", new Vector2[0]);

                var tooManyAerial = new Vector2[17];
                Set(def, "aerialSpawns", tooManyAerial);
                Assert.That(def.IsValid(out _), Is.False);
                Set(def, "aerialSpawns", new Vector2[0]);

                Set(def, "startPosition", new Vector2(float.PositiveInfinity, 0f));
                Assert.That(def.IsValid(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void ValidationAcceptsWellFormedBlockoutDefinition()
        {
            var def = CreateValidStage("stage_alpha", "Night Market");
            try
            {
                Assert.That(def.IsValid(out string error), Is.True);
                Assert.That(error, Is.Empty);
                Assert.That(def.StageId, Is.EqualTo("stage_alpha"));
                Assert.That(def.DisplayName, Is.EqualTo("Night Market"));
                Assert.That(def.Theme, Is.EqualTo(StageBlockoutTheme.DayUrban));
                Assert.That(def.PlatformCount, Is.EqualTo(2));
                Assert.That(def.GetPlatform(0).Size, Is.EqualTo(new Vector2(10f, 1f)));
                Assert.That(def.CheckpointCount, Is.EqualTo(2));
                Assert.That(def.EnemySpawnCount, Is.EqualTo(2));
                Assert.That(def.AerialSpawnCount, Is.EqualTo(2));
                Assert.That(def.FanSpawnCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void RouteValidationRejectsEmptyOrExcessiveStages()
        {
            var route = ScriptableObject.CreateInstance<StageRouteDefinition>();
            try
            {
                Set(route, "stages", new StageBlockoutDefinition[0]);
                Assert.That(route.IsValid(out string error), Is.False);
                Assert.That(error, Does.Contain("1..32 authored stages"));

                var excessive = new StageBlockoutDefinition[33];
                Set(route, "stages", excessive);
                Assert.That(route.IsValid(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(route);
            }
        }

        [Test]
        public void RouteValidationRejectsNullOrInvalidStages()
        {
            var route = ScriptableObject.CreateInstance<StageRouteDefinition>();
            var valid = CreateValidStage("stage_1", "Stage 1");
            try
            {
                Set(route, "stages", new StageBlockoutDefinition[] { valid, null });
                Assert.That(route.IsValid(out string error), Is.False);
                Assert.That(error, Does.Contain("contains an invalid stage"));
            }
            finally
            {
                Object.DestroyImmediate(route);
                Object.DestroyImmediate(valid);
            }
        }

        [Test]
        public void RouteValidationRejectsDuplicateStagesOrIds()
        {
            var route = ScriptableObject.CreateInstance<StageRouteDefinition>();
            var stageA = CreateValidStage("stage_repeat", "Stage A");
            var stageB = CreateValidStage("stage_repeat", "Stage B with duplicate id");
            try
            {
                Set(route, "stages", new[] { stageA, stageA });
                Assert.That(route.IsValid(out string error), Is.False);
                Assert.That(error, Does.Contain("identities must be unique"));

                Set(route, "stages", new[] { stageA, stageB });
                Assert.That(route.IsValid(out error), Is.False);
                Assert.That(error, Does.Contain("identities must be unique"));
            }
            finally
            {
                Object.DestroyImmediate(route);
                Object.DestroyImmediate(stageA);
                Object.DestroyImmediate(stageB);
            }
        }

        [Test]
        public void RouteValidationAcceptsUniqueValidStages()
        {
            var route = ScriptableObject.CreateInstance<StageRouteDefinition>();
            var s1 = CreateValidStage("stage_1", "Stage 1");
            var s2 = CreateValidStage("stage_2", "Stage 2");
            var s3 = CreateValidStage("stage_3", "Stage 3");
            try
            {
                Set(route, "stages", new[] { s1, s2, s3 });
                Assert.That(route.IsValid(out string error), Is.True);
                Assert.That(error, Is.Empty);
                Assert.That(route.StageCount, Is.EqualTo(3));
                Assert.That(route.GetStage(1), Is.SameAs(s2));
            }
            finally
            {
                Object.DestroyImmediate(route);
                Object.DestroyImmediate(s1);
                Object.DestroyImmediate(s2);
                Object.DestroyImmediate(s3);
            }
        }
    }
}
