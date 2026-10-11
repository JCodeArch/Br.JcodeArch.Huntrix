using System;
using UnityEngine;
namespace HuntrX.Data
{
    public enum StageBlockoutTheme { DayUrban, NightUrban, Interior, Supernatural }
    [Serializable]
    public struct BlockoutPlatform
    {
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 size;
        public Vector2 Position => position;
        public Vector2 Size => size;
    }
    [CreateAssetMenu(fileName = "StageBlockoutDefinition", menuName = "HUNTR/X/Data/Stage Blockout")]
    public sealed class StageBlockoutDefinition : GameDataDefinition
    {
        [SerializeField] private string stageId;
        [SerializeField] private string displayName;
        [SerializeField] private StageBlockoutTheme theme;
        [SerializeField] private BlockoutPlatform[] platforms = new BlockoutPlatform[0];
        [SerializeField] private Vector2 startPosition;
        [SerializeField] private Vector2 endPosition;
        [SerializeField] private Vector2 arenaCenter;
        [SerializeField] private Vector2[] checkpoints = new Vector2[0];
        [SerializeField] private Vector2[] enemySpawns = new Vector2[0];
        [SerializeField] private Vector2[] aerialSpawns = new Vector2[0];
        [SerializeField] private Vector2[] fanSpawns = new Vector2[0];
        public string StageId => stageId;
        public string DisplayName => displayName;
        public StageBlockoutTheme Theme => theme;
        public int PlatformCount => platforms?.Length ?? 0;
        public BlockoutPlatform GetPlatform(int index) => platforms[index];
        public Vector2 StartPosition => startPosition;
        public Vector2 EndPosition => endPosition;
        public Vector2 ArenaCenter => arenaCenter;
        public int CheckpointCount => checkpoints?.Length ?? 0;
        public Vector2 GetCheckpoint(int index) => checkpoints[index];
        public int EnemySpawnCount => enemySpawns?.Length ?? 0;
        public Vector2 GetEnemySpawn(int index) => enemySpawns[index];
        public int AerialSpawnCount => aerialSpawns?.Length ?? 0;
        public Vector2 GetAerialSpawn(int index) => aerialSpawns[index];
        public int FanSpawnCount => fanSpawns?.Length ?? 0;
        public Vector2 GetFanSpawn(int index) => fanSpawns[index];
        public bool IsValid(out string error)
        {
            bool valid = !string.IsNullOrWhiteSpace(stageId) && !string.IsNullOrWhiteSpace(displayName) &&
                Enum.IsDefined(typeof(StageBlockoutTheme), theme) && platforms != null && platforms.Length > 0 &&
                platforms.Length <= 128 && Valid(startPosition) && Valid(endPosition) && Valid(arenaCenter) &&
                ValidPoints(checkpoints, 8) && ValidPoints(enemySpawns, 32) && ValidPoints(aerialSpawns, 16) && ValidPoints(fanSpawns, 32);
            if (valid) foreach (BlockoutPlatform platform in platforms)
                if (!Valid(platform.Position) || !Valid(platform.Size) || platform.Size.x <= 0f || platform.Size.y <= 0f) { valid = false; break; }
            error = valid ? string.Empty : "Stage requires identity, bounded finite platforms and valid start/end/arena/spawn points.";
            return valid;
        }
        private static bool ValidPoints(Vector2[] points, int maximum)
        { if (points == null || points.Length > maximum) return false; foreach (Vector2 point in points) if (!Valid(point)) return false; return true; }
        private static bool Valid(Vector2 point) => !float.IsNaN(point.x) && !float.IsInfinity(point.x) &&
            !float.IsNaN(point.y) && !float.IsInfinity(point.y) && Mathf.Abs(point.x) <= 10000f && Mathf.Abs(point.y) <= 10000f;
    }
}
