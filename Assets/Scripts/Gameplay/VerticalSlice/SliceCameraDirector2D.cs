using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.VerticalSlice
{
    /// <summary>Pure camera math: clamps a focus so the viewport stays inside stage bounds on each axis.</summary>
    public static class SliceCameraSolver2D
    {
        /// <summary>
        /// Returns the camera center. On an axis where the stage is not larger than the viewport,
        /// the camera is centered on the stage instead of following (no empty space on one side).
        /// </summary>
        public static Vector2 Solve(Rect bounds, Vector2 focus, float orthographicSize, float aspect)
        {
            float halfH = Mathf.Max(0.01f, orthographicSize);
            float halfW = halfH * Mathf.Max(0.01f, aspect);
            return new Vector2(Axis(bounds.xMin, bounds.xMax, focus.x, halfW), Axis(bounds.yMin, bounds.yMax, focus.y, halfH));
        }

        private static float Axis(float min, float max, float value, float half)
        {
            if (max - min <= 2f * half) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + half, max - half);
        }
    }

    /// <summary>
    /// Single writer of the slice camera pose. Reads the presentation's desired focus, clamps it to the active
    /// stage bounds and smooths it. It never writes orthographicSize; parallax layers only read the pose.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(100)]
    public sealed class SliceCameraDirector2D : MonoBehaviour
    {
        private const float Smoothing = 8f;
        private VerticalSliceSession2D session;
        private CharacterManager2D manager;
        private SlicePlayerPresentation2D presentation;
        private Camera view;
        private StageBlockoutDefinition lastStage;
        private DamageReceiver2D lastActor;
        private Vector2 pose;
        private bool hasPose;
        private Rect boundsRect;
        public Camera View => view;
        public Vector2 Pose => pose;

        public bool TryBind(VerticalSliceSession2D owner, CharacterManager2D characters, SlicePlayerPresentation2D focusSource, Camera camera)
        {
            if (owner == null || characters == null || focusSource == null || camera == null) return false;
            session = owner; manager = characters; presentation = focusSource; view = camera;
            Snap();
            return true;
        }

        /// <summary>Forget the previous pose so the next frame places the camera directly (restart, stage/actor change).</summary>
        public void Snap() { hasPose = false; lastStage = null; lastActor = null; }

        private void LateUpdate()
        {
            if (session == null || manager == null || presentation == null || view == null) return;
            var actor = manager.ActiveCharacter;
            if (actor == null) return;
            StageBlockoutDefinition stage = session.Stage != null ? session.Stage.Definition : null;
            if (stage != lastStage || lastActor != actor) { hasPose = false; lastStage = stage; lastActor = actor; }

            Vector2 target = presentation.CameraFocus;
            bool bounded = stage != null && stage.TryGetWorldBounds(out boundsRect);
            if (bounded) target = SliceCameraSolver2D.Solve(boundsRect, target, view.orthographicSize, view.aspect);

            if (!hasPose) { pose = target; hasPose = true; }
            else
            {
                float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                pose = Vector2.Lerp(pose, target, 1f - Mathf.Exp(-Smoothing * dt));
            }
            // Final clamp: a smoothed pose from a smaller zoom/aspect may violate the bounds of the current viewport.
            if (bounded) pose = SliceCameraSolver2D.Solve(boundsRect, pose, view.orthographicSize, view.aspect);
            view.transform.position = new Vector3(pose.x, pose.y, -10f);
        }
    }
}
