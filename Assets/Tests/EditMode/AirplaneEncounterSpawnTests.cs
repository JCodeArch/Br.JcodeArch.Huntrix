using HuntrX.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class AirplaneEncounterSpawnTests
    {
        private const float CameraHalfWidthAtStart = 3.7f * 16f / 9f;
        private const float MinimumCheckpointClearance = 5f;

        private static StageBlockoutDefinition Stage() =>
            AssetDatabase.LoadAssetAtPath<StageBlockoutDefinition>("Assets/Data/Stages/Aviao_Blockout.asset");

        [Test] public void NoAuthoredSpawnIsInsideTheOpeningViewport()
        {
            var stage = Stage();
            for (int i = 0; i < stage.EnemySpawnCount; i++)
                Assert.That(stage.GetEnemySpawn(i).x - stage.StartPosition.x, Is.GreaterThan(CameraHalfWidthAtStart + 1f), "ground " + i);
            for (int i = 0; i < stage.AerialSpawnCount; i++)
                Assert.That(stage.GetAerialSpawn(i).x - stage.StartPosition.x, Is.GreaterThan(CameraHalfWidthAtStart + 1f), "aerial " + i);
        }

        [Test] public void NoAuthoredSpawnIsWithinStrikingDistanceOfACheckpoint()
        {
            var stage = Stage();
            for (int c = 0; c < stage.CheckpointCount; c++)
            {
                Vector2 checkpoint = stage.GetCheckpoint(c);
                for (int i = 0; i < stage.EnemySpawnCount; i++)
                    Assert.That(Vector2.Distance(checkpoint, stage.GetEnemySpawn(i)), Is.GreaterThanOrEqualTo(MinimumCheckpointClearance), $"ground {i} vs checkpoint {c}");
                for (int i = 0; i < stage.AerialSpawnCount; i++)
                    Assert.That(Vector2.Distance(checkpoint, stage.GetAerialSpawn(i)), Is.GreaterThanOrEqualTo(MinimumCheckpointClearance), $"aerial {i} vs checkpoint {c}");
            }
        }
    }
}
