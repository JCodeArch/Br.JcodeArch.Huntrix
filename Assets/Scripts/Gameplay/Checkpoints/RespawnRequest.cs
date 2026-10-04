using UnityEngine;

namespace HuntrX.Gameplay.Checkpoints
{
    /// <summary>Immutable checkpoint and attempt data for a respawn consumer.</summary>
    public readonly struct RespawnRequest
    {
        public RespawnRequest(int attemptNumber, string checkpointId, Vector3 position, Quaternion rotation)
        {
            AttemptNumber = attemptNumber;
            CheckpointId = checkpointId;
            Position = position;
            Rotation = rotation;
        }

        public int AttemptNumber { get; }
        public string CheckpointId { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}
