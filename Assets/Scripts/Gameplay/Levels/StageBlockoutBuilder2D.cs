using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Checkpoints;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    [DisallowMultipleComponent]
    public sealed class StageBlockoutBuilder2D : MonoBehaviour
    {
        private readonly List<CheckpointAnchor2D> checkpoints = new List<CheckpointAnchor2D>(8);
        private Sprite tile;
        private Texture2D texture;
        private bool building;
        private IReadOnlyList<CheckpointAnchor2D> checkpointView;
        public StageBlockoutDefinition Definition { get; private set; }
        public GameObject RuntimeRoot { get; private set; }
        public Transform StartPoint { get; private set; }
        public Transform EndPoint { get; private set; }
        public IReadOnlyList<CheckpointAnchor2D> CheckpointAnchors => checkpointView ?? (checkpointView = checkpoints.AsReadOnly());
        public bool TryBuild(StageBlockoutDefinition profile)
        {
            if (!isActiveAndEnabled || building || RuntimeRoot != null || profile == null || !profile.IsValid(out _)) return false;
            building = true;
            GameObject candidate = new GameObject("Blockout_" + profile.StageId);
            candidate.SetActive(false);
            try
            {
                texture = new Texture2D(1, 1); texture.SetPixel(0, 0, Color.white); texture.Apply();
                tile = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f,.5f), 1f);
                for (int i = 0; i < profile.PlatformCount; i++)
                {
                    BlockoutPlatform authored = profile.GetPlatform(i);
                    GameObject platform = new GameObject("Platform_" + i);
                    platform.transform.SetParent(candidate.transform, false);
                    platform.transform.localPosition = authored.Position;
                    BoxCollider2D collider = platform.AddComponent<BoxCollider2D>(); collider.size = authored.Size;
                    GameObject visual = new GameObject("Platform_Visual"); visual.transform.SetParent(platform.transform, false);
                    // Keep collider world dimensions explicit; only the visual child is scaled.
                    SpriteRenderer display = visual.AddComponent<SpriteRenderer>();
                    display.sprite = tile; display.color = PlatformColor(profile.Theme);
                    visual.transform.localScale = new Vector3(authored.Size.x, authored.Size.y, 1f);
                }
                StartPoint = Point(candidate.transform, "Stage_Start", profile.StartPosition);
                EndPoint = Point(candidate.transform, "Stage_End", profile.EndPosition);
                for (int i = 0; i < profile.CheckpointCount; i++)
                {
                    Transform point = Point(candidate.transform, "Checkpoint_" + i, profile.GetCheckpoint(i));
                    CheckpointAnchor2D anchor = point.gameObject.AddComponent<CheckpointAnchor2D>();
                    // Candidate is activated before configuring the anchors' validated destinations.
                    checkpoints.Add(anchor);
                }
                candidate.SetActive(true);
                for (int i = 0; i < checkpoints.Count; i++)
                    if (!checkpoints[i].TryConfigure(profile.StageId + ":checkpoint:" + i, checkpoints[i].transform))
                        throw new InvalidOperationException("Checkpoint configuration failed.");
                if (!isActiveAndEnabled) throw new InvalidOperationException("Stage owner disabled during construction.");
                Definition = profile; RuntimeRoot = candidate;
                return true;
            }
            catch (Exception exception)
            {
                candidate.SetActive(false); Destroy(candidate); ClearReferences(); DestroyTiles(); Debug.LogException(exception, this); return false;
            }
            finally { building = false; }
        }
        public void TryClear()
        {
            if (RuntimeRoot != null) { RuntimeRoot.SetActive(false); Destroy(RuntimeRoot); }
            ClearReferences(); DestroyTiles();
        }
        private void ClearReferences() { RuntimeRoot = null; Definition = null; StartPoint = null; EndPoint = null; checkpoints.Clear(); }
        private void DestroyTiles() { if (tile != null) Destroy(tile); if (texture != null) Destroy(texture); tile = null; texture = null; }
        private static Transform Point(Transform root, string name, Vector2 position)
        { GameObject point = new GameObject(name); point.transform.SetParent(root, false); point.transform.localPosition = position; return point.transform; }
        private static Color PlatformColor(StageBlockoutTheme theme) => theme == StageBlockoutTheme.DayUrban ? new Color(.3f,.6f,.75f) :
            theme == StageBlockoutTheme.NightUrban ? new Color(.4f,.3f,.65f) : theme == StageBlockoutTheme.Interior ? new Color(.55f,.65f,.72f) : new Color(.45f,.7f,.55f);
        private void OnDisable() => TryClear();
        private void OnDestroy() => TryClear();
    }
}
