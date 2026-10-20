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
        private Sprite backdropSprite;
        private bool ownsBackdropSprite;
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
                texture = CreateNeonDeckTexture(profile.Theme);
                tile = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 32f);

                TryCreateHdStageBackdrop(candidate.transform, profile);

                for (int i = 0; i < profile.PlatformCount; i++)
                {
                    BlockoutPlatform authored = profile.GetPlatform(i);
                    GameObject platform = new GameObject("Platform_" + i);
                    platform.transform.SetParent(candidate.transform, false);
                    platform.transform.localPosition = authored.Position;
                    BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
                    collider.size = authored.Size;
                    GameObject visual = new GameObject("Platform_Visual");
                    visual.transform.SetParent(platform.transform, false);
                    // Keep collider world dimensions explicit; render a sleek Capcom metallic top catwalk deck.
                    SpriteRenderer display = visual.AddComponent<SpriteRenderer>();
                    display.sprite = tile;
                    display.color = Color.white;
                    display.sortingOrder = 2;
                    float visualHeight = Mathf.Min(authored.Size.y, 0.36f);
                    visual.transform.localPosition = new Vector3(0f, (authored.Size.y - visualHeight) * 0.5f, 0f);
                    visual.transform.localScale = new Vector3(authored.Size.x, visualHeight, 1f);
                }

                StartPoint = Point(candidate.transform, "Stage_Start", profile.StartPosition);
                EndPoint = Point(candidate.transform, "Stage_End", profile.EndPosition);
                for (int i = 0; i < profile.CheckpointCount; i++)
                {
                    Transform point = Point(candidate.transform, "Checkpoint_" + i, profile.GetCheckpoint(i));
                    CheckpointAnchor2D anchor = point.gameObject.AddComponent<CheckpointAnchor2D>();
                    checkpoints.Add(anchor);
                }
                candidate.SetActive(true);
                for (int i = 0; i < checkpoints.Count; i++)
                    if (!checkpoints[i].TryConfigure(profile.StageId + ":checkpoint:" + i, checkpoints[i].transform))
                        throw new InvalidOperationException("Checkpoint configuration failed.");
                if (!isActiveAndEnabled) throw new InvalidOperationException("Stage owner disabled during construction.");
                Definition = profile;
                RuntimeRoot = candidate;
                return true;
            }
            catch (Exception exception)
            {
                candidate.SetActive(false);
                Destroy(candidate);
                ClearReferences();
                DestroyTiles();
                Debug.LogException(exception, this);
                return false;
            }
            finally { building = false; }
        }

        private void TryCreateHdStageBackdrop(Transform parent, StageBlockoutDefinition profile)
        {
            string key = ResolveStageArtKey(profile.StageId, profile.DisplayName);
            backdropSprite = UnityEngine.Resources.Load<Sprite>("Art/Stages/" + key);
            ownsBackdropSprite = false;
            if (backdropSprite == null)
            {
                Texture2D tex = UnityEngine.Resources.Load<Texture2D>("Art/Stages/" + key);
                if (tex != null)
                {
                    backdropSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    ownsBackdropSprite = true;
                }
            }
            if (backdropSprite == null) return;

            float minX = profile.StartPosition.x;
            float maxX = profile.EndPosition.x;
            float minY = profile.StartPosition.y;
            for (int i = 0; i < profile.PlatformCount; i++)
            {
                var p = profile.GetPlatform(i);
                minX = Mathf.Min(minX, p.Position.x - p.Size.x * 0.5f);
                maxX = Mathf.Max(maxX, p.Position.x + p.Size.x * 0.5f);
                minY = Mathf.Min(minY, p.Position.y + p.Size.y * 0.5f);
            }

            float w = backdropSprite.bounds.size.x;
            float h = backdropSprite.bounds.size.y;
            float aspect = (w > 0.01f && h > 0.01f) ? (w / h) : (16f / 9f);

            // Human-scale Capcom Mega Man X corridor height (9.6 world units tall, tiled horizontally across the whole stage!)
            float panelHeight = 9.6f;
            float panelWidth = panelHeight * aspect;
            float totalWidth = Mathf.Max(panelWidth * 4f, (maxX - minX) + 48f);
            int tileCount = Mathf.Clamp(Mathf.CeilToInt(totalWidth / Mathf.Max(1f, panelWidth)), 3, 12);
            float centerX = (minX + maxX) * 0.5f;
            float centerY = minY + (panelHeight * 0.34f);

            GameObject bgRoot = new GameObject("HD_StageBackdrop_" + key);
            bgRoot.transform.SetParent(parent, false);
            bgRoot.transform.localPosition = new Vector3(centerX, centerY, 0f);
            bgRoot.transform.localScale = Vector3.one;

            SpriteRenderer primaryRenderer = null;
            float startOffset = -(tileCount - 1) * 0.5f * panelWidth;
            for (int t = 0; t < tileCount; t++)
            {
                GameObject panel = new GameObject("Backdrop_Tile_" + t);
                panel.transform.SetParent(bgRoot.transform, false);
                panel.transform.localPosition = new Vector3(startOffset + t * panelWidth, 0f, 0f);
                if (w > 0.01f && h > 0.01f)
                {
                    // Alternate horizontal flip on odd panels only if not airplane corridor (airplane corridor is already symmetrical)
                    panel.transform.localScale = new Vector3(panelWidth / w, panelHeight / h, 1f);
                }
                SpriteRenderer sr = panel.AddComponent<SpriteRenderer>();
                sr.sprite = backdropSprite;
                sr.sortingOrder = -25;
                if (primaryRenderer == null) primaryRenderer = sr;
            }

            var anim = bgRoot.AddComponent<LivingStageBackdropAnimator2D>();
            anim.Configure(key, primaryRenderer);
        }

        private static string ResolveStageArtKey(string stageId, string displayName)
        {
            string s = ((stageId ?? string.Empty) + " " + (displayName ?? string.Empty)).ToLowerInvariant();
            if (s.Contains("airplane") || s.Contains("aviao") || s.Contains("avião") || s.Contains("cabin")) return "airplane";
            if (s.Contains("show") || s.Contains("concert") || s.Contains("palco") || s.Contains("parachute")) return "show";
            if (s.Contains("hanok") || s.Contains("bukchon")) return "hanok";
            if (s.Contains("cheongdam")) return "cheongdam";
            if (s.Contains("myeongdong") || s.Contains("ramen")) return "myeongdong";
            if (s.Contains("coex") || s.Contains("gangnam")) return "coex";
            if (s.Contains("naksan")) return "naksan";
            if (s.Contains("namsan") || s.Contains("tower")) return "namsan";
            if (s.Contains("award") || s.Contains("final") || s.Contains("gwima")) return "awards";
            return "show";
        }

        private static Texture2D CreateNeonDeckTexture(StageBlockoutTheme theme)
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color neonTop = theme == StageBlockoutTheme.DayUrban ? new Color(0.15f, 0.92f, 1f, 0.98f) :
                            theme == StageBlockoutTheme.NightUrban ? new Color(0.95f, 0.25f, 0.85f, 0.98f) :
                            theme == StageBlockoutTheme.Interior ? new Color(0.18f, 0.92f, 1f, 0.98f) :
                            new Color(0.45f, 0.95f, 0.75f, 0.98f);
            Color bodyDark = new Color(0.08f, 0.09f, 0.16f, 0.95f);
            Color steelMid = new Color(0.18f, 0.21f, 0.32f, 0.95f);
            Color hazardGold = new Color(0.95f, 0.68f, 0.15f, 0.92f);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color c;
                    if (y >= 28) c = neonTop; // Glowing Capcom cyan/magenta top energy rail
                    else if (y >= 23 && y <= 26) c = ((x + y) % 8 < 4) ? hazardGold : bodyDark; // Capcom industrial hazard stripe
                    else if (y == 27 || y <= 2 || x == 0 || x == 31) c = steelMid;
                    else if ((x % 4 == 0) && y >= 6 && y <= 20) c = new Color(0.03f, 0.04f, 0.08f, 0.95f); // Grated floor slots
                    else c = steelMid;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        public void TryClear()
        {
            if (RuntimeRoot != null) { RuntimeRoot.SetActive(false); Destroy(RuntimeRoot); }
            ClearReferences();
            DestroyTiles();
        }

        private void ClearReferences() { RuntimeRoot = null; Definition = null; StartPoint = null; EndPoint = null; checkpoints.Clear(); }
        private void DestroyTiles()
        {
            if (ownsBackdropSprite && backdropSprite != null) Destroy(backdropSprite);
            backdropSprite = null;
            ownsBackdropSprite = false;
            if (tile != null) Destroy(tile);
            if (texture != null) Destroy(texture);
            tile = null;
            texture = null;
        }
        private static Transform Point(Transform root, string name, Vector2 position)
        { GameObject point = new GameObject(name); point.transform.SetParent(root, false); point.transform.localPosition = position; return point.transform; }
        private void OnDisable() => TryClear();
        private void OnDestroy() => TryClear();
    }
}
