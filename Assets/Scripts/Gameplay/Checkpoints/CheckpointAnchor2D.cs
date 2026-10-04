using UnityEngine;

namespace HuntrX.Gameplay.Checkpoints
{
    /// <summary>A scene-authored checkpoint identity and its respawn pose.</summary>
    [DisallowMultipleComponent]
    public sealed class CheckpointAnchor2D : MonoBehaviour
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private Transform respawnPoint;

        public bool TryGetCheckpoint(out string id, out Vector3 position, out Quaternion rotation)
        {
            id = string.Empty;
            position = Vector3.zero;
            rotation = Quaternion.identity;

            if (!isActiveAndEnabled || string.IsNullOrWhiteSpace(checkpointId))
            {
                return false;
            }

            Transform destination = respawnPoint != null ? respawnPoint : transform;
            if (destination == null || !destination.gameObject.activeInHierarchy)
            {
                return false;
            }

            Vector3 candidatePosition = destination.position;
            Quaternion candidateRotation = destination.rotation;
            if (!IsFinite(candidatePosition.x) || !IsFinite(candidatePosition.y) ||
                !IsFinite(candidatePosition.z) || !IsFinite(candidateRotation.x) ||
                !IsFinite(candidateRotation.y) || !IsFinite(candidateRotation.z) ||
                !IsFinite(candidateRotation.w) || IsZeroRotation(candidateRotation))
            {
                return false;
            }

            id = checkpointId;
            position = candidatePosition;
            rotation = candidateRotation;
            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsZeroRotation(Quaternion value) =>
            value.x == 0f && value.y == 0f && value.z == 0f && value.w == 0f;
    }
}
