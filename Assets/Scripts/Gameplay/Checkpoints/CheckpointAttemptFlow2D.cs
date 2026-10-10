using System;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Checkpoints
{
    /// <summary>Tracks scene-local checkpoint selection and publishes respawn requests for ended attempts.</summary>
    [DisallowMultipleComponent]
    public sealed class CheckpointAttemptFlow2D : MonoBehaviour
    {
        private string startCheckpointId;
        private Transform startPoint;
        private CheckpointAnchor2D activeCheckpoint;
        private DamageReceiver2D actor;
        private int attemptNumber;
        private bool startConfigured;
        private bool attemptInProgress;
        private bool missingDestinationReported;

        public event Action<RespawnRequest> RespawnRequested;
        public event Action<string> CheckpointActivated;

        public int AttemptNumber => attemptNumber;
        public bool IsAttemptInProgress => attemptInProgress;
        public DamageReceiver2D BoundActor => actor;
        public string ActiveCheckpointId => TryGetActiveCheckpoint(out string id, out _, out _) ? id : string.Empty;
        public Vector3 ActiveCheckpointPosition => TryGetActiveCheckpoint(out _, out Vector3 position, out _)
            ? position
            : Vector3.zero;

        public bool ConfigureStart(string checkpointId, Transform point)
        {
            if (!isActiveAndEnabled || attemptInProgress || string.IsNullOrWhiteSpace(checkpointId) ||
                !IsValidDestination(point))
            {
                return false;
            }

            startCheckpointId = checkpointId;
            startPoint = point;
            activeCheckpoint = null;
            startConfigured = true;
            missingDestinationReported = false;
            return true;
        }

        public bool BindActor(DamageReceiver2D newActor)
        {
            if (!isActiveAndEnabled || newActor == null || !newActor.IsConfigurationValid || !newActor.IsAlive)
            {
                return false;
            }

            if (actor == newActor)
            {
                return true;
            }

            UnbindActor();
            actor = newActor;
            actor.Died += HandleActorDied;
            return true;
        }

        public bool StartAttempt()
        {
            if (!isActiveAndEnabled || attemptInProgress || !startConfigured || actor == null ||
                !actor.IsConfigurationValid || !actor.IsAlive || attemptNumber == int.MaxValue ||
                !TryGetActiveCheckpoint(out _, out _, out _))
            {
                return false;
            }

            attemptNumber++;
            attemptInProgress = true;
            return true;
        }

        /// <summary>Ends ownership of this attempt without changing checkpoint selection or attempt number.</summary>
        public void CancelAttempt()
        {
            attemptInProgress = false;
            UnbindActor();
        }

        public bool ActivateCheckpoint(CheckpointAnchor2D checkpoint)
        {
            if (!isActiveAndEnabled || !attemptInProgress || checkpoint == null ||
                !checkpoint.TryGetCheckpoint(out string checkpointId, out _, out _))
            {
                return false;
            }

            activeCheckpoint = checkpoint;
            PublishCheckpointActivated(checkpointId);

            return true;
        }

        private void OnDisable()
        {
            UnbindActor();
            attemptInProgress = false;
        }

        private void HandleActorDied(DamageReceiver2D deadActor)
        {
            if (!isActiveAndEnabled || !attemptInProgress || actor != deadActor)
            {
                return;
            }

            attemptInProgress = false;
            if (!TryGetActiveCheckpoint(out string checkpointId, out Vector3 position, out Quaternion rotation))
            {
                ReportMissingDestination();
                return;
            }

            PublishRespawnRequested(new RespawnRequest(attemptNumber, checkpointId, position, rotation));
        }

        private void PublishCheckpointActivated(string checkpointId)
        {
            Action<string> handlers = CheckpointActivated;
            if (handlers == null)
            {
                return;
            }

            foreach (Action<string> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(checkpointId);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void PublishRespawnRequested(RespawnRequest request)
        {
            Action<RespawnRequest> handlers = RespawnRequested;
            if (handlers == null)
            {
                return;
            }

            foreach (Action<RespawnRequest> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(request);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
        private bool TryGetActiveCheckpoint(out string checkpointId, out Vector3 position, out Quaternion rotation)
        {
            if (activeCheckpoint != null &&
                activeCheckpoint.TryGetCheckpoint(out checkpointId, out position, out rotation))
            {
                return true;
            }

            checkpointId = string.Empty;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            if (!startConfigured || !IsValidDestination(startPoint))
            {
                return false;
            }

            checkpointId = startCheckpointId;
            position = startPoint.position;
            rotation = startPoint.rotation;
            return true;
        }

        private static bool IsValidDestination(Transform point)
        {
            if (point == null || !point.gameObject.activeInHierarchy)
            {
                return false;
            }

            Vector3 position = point.position;
            Quaternion rotation = point.rotation;
            return IsFinite(position.x) && IsFinite(position.y) && IsFinite(position.z) &&
                IsFinite(rotation.x) && IsFinite(rotation.y) && IsFinite(rotation.z) &&
                IsFinite(rotation.w) && !IsZeroRotation(rotation);
        }

        private void ReportMissingDestination()
        {
            if (missingDestinationReported)
            {
                return;
            }

            missingDestinationReported = true;
            Debug.LogError("CheckpointAttemptFlow2D could not resolve an active checkpoint or stage start.", this);
        }

        private void UnbindActor()
        {
            if (actor != null)
            {
                actor.Died -= HandleActorDied;
                actor = null;
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsZeroRotation(Quaternion value) =>
            value.x == 0f && value.y == 0f && value.z == 0f && value.w == 0f;
    }
}
