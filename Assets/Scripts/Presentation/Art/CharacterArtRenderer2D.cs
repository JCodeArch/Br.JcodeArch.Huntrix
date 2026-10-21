using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using UnityEngine;

namespace HuntrX.Presentation.Art
{
    /// <summary>Presentation-only character visual host backed by CharacterSpritePlayback2D.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterArtRenderer2D : MonoBehaviour
    {
        [SerializeField] private CharacterArtDefinition definition;
        [SerializeField] private CharacterSpriteSet2D spriteSet;

        private Texture2D texture;
        private Sprite sprite;
        private bool ownsSprite;
        private SpriteRenderer hdSpriteRenderer;
        private CharacterSpritePlayback2D spritePlayback;
        private CharacterSpriteSet2D activeSpriteSet;
        private readonly List<Object> ownedRuntimeAssets = new List<Object>(32);

        public CharacterArtDefinition Definition => definition;
        public CharacterSpriteSet2D SpriteSet => activeSpriteSet != null ? activeSpriteSet : spriteSet;
        public SpriteRenderer SpriteRenderer => hdSpriteRenderer;
        public CharacterSpritePlayback2D Playback => spritePlayback;
        public Transform VisualRoot { get; private set; }
        public Transform LeftLeg { get; private set; }
        public Transform RightLeg { get; private set; }
        public Transform LeftArm { get; private set; }
        public Transform RightArm { get; private set; }
        public Transform Weapon { get; private set; }

        public bool TryConfigure(CharacterArtDefinition profile)
        {
            if (!isActiveAndEnabled || profile == null || !profile.IsValid) return false;
            Clear();
            definition = profile;
            sprite = Resources.Load<Sprite>("Art/SharedQuad");
            ownsSprite = false;
            if (sprite == null)
            {
                texture = new Texture2D(4, 4);
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        texture.SetPixel(x, y, Color.white);
                texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0f), 4f);
                ownsSprite = true;
            }

            VisualRoot = new GameObject("Original_" + profile.CharacterName + "_Rig").transform;
            VisualRoot.SetParent(transform, false);
            float h = profile.Height, w = profile.Width;
            Part("Torso", new Vector2(0f, 0.04f * h), new Vector2(w, 0.42f * h), profile.Primary, 11);
            Part("HairBack", new Vector2(-0.03f, 0.34f * h - profile.HairLength * 0.5f), new Vector2(w * 0.8f, 0.2f * h + profile.HairLength), profile.Accent, 10);
            Part("Head", new Vector2(0f, 0.34f * h), new Vector2(w * 0.64f, 0.24f * h), profile.Skin, 12);
            Part("HairTop", new Vector2(-0.03f, 0.46f * h), new Vector2(w * 0.8f, 0.07f * h), profile.Accent, 13);
            Part("Eyes", new Vector2(w * 0.15f, 0.36f * h), new Vector2(w * 0.2f, 0.025f * h), Color.white, 14);
            Part("Belt", new Vector2(0f, -0.16f * h), new Vector2(w * 1.06f, 0.04f * h), profile.Accent, 13);
            LeftLeg = Part("Leg_L", new Vector2(-w * 0.25f, -0.32f * h), new Vector2(w * 0.34f, 0.34f * h), profile.Primary * 0.7f, 11);
            RightLeg = Part("Leg_R", new Vector2(w * 0.25f, -0.32f * h), new Vector2(w * 0.34f, 0.34f * h), profile.Primary * 0.85f, 12);
            LeftArm = Part("Arm_L", new Vector2(-w * 0.66f, 0.03f * h), new Vector2(w * 0.25f, 0.35f * h), profile.Skin, 11);
            RightArm = Part("Arm_R", new Vector2(w * 0.66f, 0.03f * h), new Vector2(w * 0.25f, 0.35f * h), profile.Skin, 13);

            Vector2 weaponSize = new Vector2(0.08f, 0.65f * h);
            if (profile.Weapon == CharacterWeaponShape.Polearm) weaponSize = new Vector2(0.06f, 1.25f * h);
            if (profile.Weapon == CharacterWeaponShape.TwinStars) weaponSize = new Vector2(0.25f, 0.25f);
            if (profile.Weapon == CharacterWeaponShape.Microphone) weaponSize = new Vector2(0.13f, 0.28f);
            if (profile.Weapon == CharacterWeaponShape.Staff) weaponSize = new Vector2(0.09f, 0.9f * h);
            if (profile.Weapon == CharacterWeaponShape.Fans) weaponSize = new Vector2(0.38f, 0.18f);
            Weapon = Part("Weapon", new Vector2(w * 0.95f, 0.04f * h), weaponSize, profile.Accent, 14);
            if (profile.Weapon == CharacterWeaponShape.TwinStars)
            {
                Weapon.localRotation = Quaternion.Euler(0f, 0f, 45f);
                Part("TwinStar", new Vector2(-w * 0.9f, 0.05f * h), weaponSize, profile.Accent, 14).localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            if (profile.Weapon == CharacterWeaponShape.Horns)
            {
                Weapon.gameObject.SetActive(false);
                Part("Horn_L", new Vector2(-w * 0.32f, 0.56f * h), new Vector2(0.12f, 0.28f), profile.Accent, 14).localRotation = Quaternion.Euler(0f, 0f, 25f);
                Part("Horn_R", new Vector2(w * 0.32f, 0.56f * h), new Vector2(0.12f, 0.28f), profile.Accent, 14).localRotation = Quaternion.Euler(0f, 0f, -25f);
            }

            EnsureSpriteHost();
            CharacterSpriteSet2D resolved = ResolveOrBuildSpriteSet(profile.CharacterName, h);
            if (resolved != null)
            {
                TryBindSpriteSet(resolved);
            }
            return true;
        }

        public bool TryBindSpriteSet(CharacterSpriteSet2D set)
        {
            if (!isActiveAndEnabled || set == null || !set.IsValid) return false;
            if (VisualRoot == null)
            {
                VisualRoot = new GameObject("Original_" + set.CharacterName + "_Rig").transform;
                VisualRoot.SetParent(transform, false);
            }
            EnsureSpriteHost();
            if (!spritePlayback.TryBind(hdSpriteRenderer, set)) return false;
            activeSpriteSet = set;
            hdSpriteRenderer.transform.localPosition = CharacterGameplayAnimator2D.FootLocalPosition(hdSpriteRenderer.transform, CharacterGameplayAnimator2D.FindRootBodyCollider(transform));
            hdSpriteRenderer.transform.localRotation = Quaternion.identity;
            hdSpriteRenderer.transform.localScale = new Vector3(set.WorldScale, set.WorldScale, 1f);
            return true;
        }

        public void UpdatePoseFrame(CharacterVisualPose pose, float clock, float cycleSeconds, int forcedFrameIndex = -1)
        {
            if (spritePlayback == null || activeSpriteSet == null) return;
            string desired = MapPoseToStateId(pose, activeSpriteSet);
            spritePlayback.SetState(desired, false);
        }

        internal static string MapPoseToStateId(CharacterVisualPose pose, CharacterSpriteSet2D set)
        {
            if (set == null) return "Idle";
            string primary = pose.ToString();
            if (set.TryGetClip(primary, out _)) return primary;

            switch (pose)
            {
                case CharacterVisualPose.Aerial:
                    if (set.TryGetClip("Attack", out _)) return "Attack";
                    if (set.TryGetClip("Jump", out _)) return "Jump";
                    break;
                case CharacterVisualPose.Dash:
                    if (set.TryGetClip("Run", out _)) return "Run";
                    if (set.TryGetClip("Jump", out _)) return "Jump";
                    break;
                case CharacterVisualPose.Hurt:
                case CharacterVisualPose.Parry:
                case CharacterVisualPose.Special:
                    if (set.TryGetClip("Attack", out _)) return "Attack";
                    break;
            }
            return "Idle";
        }

        private void EnsureSpriteHost()
        {
            if (hdSpriteRenderer != null && spritePlayback != null) return;
            GameObject artObj = new GameObject("HD_CharacterSprite");
            artObj.transform.SetParent(VisualRoot, false);
            artObj.transform.localPosition = Vector3.zero;
            artObj.transform.localRotation = Quaternion.identity;
            artObj.transform.localScale = Vector3.one;
            hdSpriteRenderer = artObj.AddComponent<SpriteRenderer>();
            hdSpriteRenderer.sortingOrder = 15;
            spritePlayback = artObj.AddComponent<CharacterSpritePlayback2D>();
        }

        private CharacterSpriteSet2D ResolveOrBuildSpriteSet(string characterName, float targetHeight)
        {
            if (spriteSet != null && spriteSet.IsValid) return spriteSet;
            return BuildRuntimeSpriteSet(characterName, Mathf.Max(1f, targetHeight * 1.15f), ownedRuntimeAssets);
        }

        internal static CharacterSpriteSet2D BuildRuntimeSpriteSet(string characterName, float targetWorldHeight, List<Object> ownedAssets)
        {
            string safeName = string.IsNullOrWhiteSpace(characterName) ? "Rumi" : characterName;
            string key = safeName == "Gwi-Ma" ? "GwiMa" : safeName;

            CharacterSpriteSet2D loadedSet = Resources.Load<CharacterSpriteSet2D>("Art/Characters/" + key + "SpriteSet");
            if (loadedSet == null) loadedSet = Resources.Load<CharacterSpriteSet2D>("Art/SpriteSets/" + key);
            if (loadedSet != null && loadedSet.IsValid) return loadedSet;

            Texture2D baseTex = Resources.Load<Texture2D>("Art/Animations/" + key + "/Idle/0");
            if (baseTex == null) baseTex = Resources.Load<Texture2D>("Art/Characters/" + key);

            int canvasW = baseTex != null ? baseTex.width : 64;
            int canvasH = baseTex != null ? baseTex.height : 64;
            float ppu = Mathf.Clamp(canvasH / Mathf.Max(0.5f, targetWorldHeight), 1f, 4096f);
            Vector2 footAnchor = new Vector2(0.5f, 0f);

            if (baseTex == null)
            {
                baseTex = new Texture2D(canvasW, canvasH);
                ownedAssets.Add(baseTex);
            }

            Sprite fallbackSprite = Sprite.Create(baseTex, new Rect(0f, 0f, canvasW, canvasH), footAnchor, ppu);
            ownedAssets.Add(fallbackSprite);

            var states = new List<SpriteAnimationState2D>(6);
            states.Add(new SpriteAnimationState2D("Idle", LoadClipOrFallback(key, "Idle", canvasW, canvasH, footAnchor, ppu, 0.16f, true, fallbackSprite, ownedAssets)));
            states.Add(new SpriteAnimationState2D("Run", LoadClipOrFallback(key, "Run", canvasW, canvasH, footAnchor, ppu, 0.10f, true, fallbackSprite, ownedAssets)));
            states.Add(new SpriteAnimationState2D("Jump", LoadClipOrFallback(key, "Jump", canvasW, canvasH, footAnchor, ppu, 0.12f, false, fallbackSprite, ownedAssets)));
            states.Add(new SpriteAnimationState2D("Fall", LoadClipOrFallback(key, "Fall", canvasW, canvasH, footAnchor, ppu, 0.12f, false, fallbackSprite, ownedAssets)));
            states.Add(new SpriteAnimationState2D("Attack", LoadClipOrFallback(key, "Attack", canvasW, canvasH, footAnchor, ppu, 0.08f, false, fallbackSprite, ownedAssets)));
            states.Add(new SpriteAnimationState2D("Death", LoadClipOrFallback(key, "Death", canvasW, canvasH, footAnchor, ppu, 0.12f, false, fallbackSprite, ownedAssets)));

            var runtimeSet = ScriptableObject.CreateInstance<CharacterSpriteSet2D>();
            ownedAssets.Add(runtimeSet);
            SetPrivateField(runtimeSet, "characterName", safeName);
            SetPrivateField(runtimeSet, "pixelsPerUnit", ppu);
            SetPrivateField(runtimeSet, "worldScale", 1f);
            SetPrivateField(runtimeSet, "footAnchor", footAnchor);
            SetPrivateField(runtimeSet, "states", states.ToArray());
            return runtimeSet;
        }

        private static SpriteAnimationClip2D LoadClipOrFallback(
            string key,
            string stateName,
            int canvasW,
            int canvasH,
            Vector2 footAnchor,
            float ppu,
            float frameDuration,
            bool loop,
            Sprite fallbackSprite,
            List<Object> ownedAssets)
        {
            var frames = new List<Sprite>(4);
            var durations = new List<float>(4);
            for (int i = 0; i < 8; i++)
            {
                Texture2D tex = Resources.Load<Texture2D>("Art/Animations/" + key + "/" + stateName + "/" + i);
                if (tex == null) break;
                if (tex.width < canvasW || tex.height < canvasH) continue;
                Sprite s = Sprite.Create(tex, new Rect(0f, 0f, canvasW, canvasH), footAnchor, ppu);
                ownedAssets.Add(s);
                frames.Add(s);
                durations.Add(frameDuration);
            }

            if (frames.Count == 0)
            {
                frames.Add(fallbackSprite);
                durations.Add(frameDuration);
            }

            return new SpriteAnimationClip2D(frames.ToArray(), durations.ToArray(), loop);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null) field.SetValue(target, value);
        }

        private Transform Part(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(VisualRoot, false);
            part.transform.localPosition = position;
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            return part.transform;
        }

        private void Awake()
        {
            if (definition != null) TryConfigure(definition);
        }

        private void OnEnable()
        {
            if (VisualRoot != null) VisualRoot.gameObject.SetActive(true);
            if (spritePlayback != null && activeSpriteSet != null && hdSpriteRenderer != null && string.IsNullOrEmpty(spritePlayback.CurrentState))
            {
                spritePlayback.TryBind(hdSpriteRenderer, activeSpriteSet);
            }
        }

        private void OnDisable()
        {
            if (VisualRoot != null) VisualRoot.gameObject.SetActive(false);
        }

        private void Clear()
        {
            if (spritePlayback != null) spritePlayback.Unbind();
            if (VisualRoot != null)
            {
                VisualRoot.gameObject.SetActive(false);
                Destroy(VisualRoot.gameObject);
            }
            VisualRoot = null;
            LeftLeg = null;
            RightLeg = null;
            LeftArm = null;
            RightArm = null;
            Weapon = null;
            hdSpriteRenderer = null;
            spritePlayback = null;
            activeSpriteSet = null;
            for (int i = 0; i < ownedRuntimeAssets.Count; i++)
            {
                if (ownedRuntimeAssets[i] != null) Destroy(ownedRuntimeAssets[i]);
            }
            ownedRuntimeAssets.Clear();
            if (ownsSprite && sprite != null) Destroy(sprite);
            if (texture != null) Destroy(texture);
            sprite = null;
            texture = null;
            ownsSprite = false;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
