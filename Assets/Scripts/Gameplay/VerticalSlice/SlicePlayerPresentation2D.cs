using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using HuntrX.Presentation.Art;
using UnityEngine;

namespace HuntrX.Gameplay.VerticalSlice
{
    [DisallowMultipleComponent]
    public sealed class SlicePlayerPresentation2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private Camera followCamera;
        [SerializeField] private CharacterSpriteSet2D spriteSetOverride;
        [SerializeField] private CharacterSpriteSet2D[] characterSpriteSets;

        private Sprite sprite;
        private DamageReceiver2D current;
        private GameObject visual;
        private SpriteRenderer mainRenderer;
        private CharacterSpritePlayback2D spritePlayback;
        private CharacterSpriteSet2D activeSpriteSet;
        private GameObject vfxSlashObject;
        private SpriteRenderer vfxSlashRenderer;
        private bool characterVisible = true;

        private string activeHeroKey = "Rumi";
        // VFX survive actor switches; generated sprite sets belong to one actor binding only.
        private readonly List<Object> ownedRuntimeAssets = new List<Object>(64);
        private readonly List<Object> ownedActorRuntimeAssets = new List<Object>(64);
        private Sprite saberCyanSprite;
        private Sprite saberTurquoiseSprite;
        private Sprite coffeePotSmashSprite;
        private Sprite miraCoffeePotEquippedSprite;

        private AttackController2D attackController;
        private DashController2D dashController;
        private JumpController2D jumpController;
        private Collider2D bodyCollider;
        private ZoeyRangedAttack2D rangedController;
        private HuntrX.Gameplay.Resources.TemporaryPropWeapon2D propWeapon;
        private Rigidbody2D body2D;
        private float animClock;
        private float rangedVfxTimer;
        private int facing = 1;
        private float worldScale = 1f;
        private bool wasInAttack;
        private int lastAttackStepIndex = -1;
        private bool airborneLatched;

        private CharacterGameplayAnimator2D suppressedAnimator;
        private bool priorAnimatorEnabled;
        private CharacterArtRenderer2D suppressedArtRenderer;
        private bool priorArtRendererEnabled;
        private GameObject suppressedVisualRoot;
        private bool priorVisualRootActive;
        private HuntrX.Gameplay.Levels.BlockoutPlayerSilhouette2D suppressedBlockout;
        private bool priorBlockoutEnabled;

        private readonly SpriteRenderer[] ghostRenderers = new SpriteRenderer[3];
        private readonly Vector3[] historyPositions = new Vector3[3];
        private readonly Sprite[] historySprites = new Sprite[3];
        private readonly int[] historyFacing = new int[3];
        private float ghostSampleTimer;

        public CharacterSpriteSet2D SpriteSet => activeSpriteSet;
        public CharacterSpritePlayback2D Playback => spritePlayback;
        public Vector2 CameraFocus => current != null
            ? (Vector2)current.transform.position + new Vector2(1.55f * facing, 0.95f)
            : Vector2.zero;

        public void SetCharacterVisible(bool visible)
        {
            characterVisible = visible;
            if (mainRenderer != null) mainRenderer.enabled = visible;
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                if (ghostRenderers[i] != null && !visible) ghostRenderers[i].enabled = false;
            }
        }

        public bool TryBind(CharacterManager2D owner, Camera camera)
        {
            if (owner == null || camera == null) return false;
            Unbind();
            manager = owner;
            followCamera = camera;
            if (isActiveAndEnabled)
            {
                manager.ActiveCharacterChanged += OnActor;
                OnActor(null, manager.ActiveCharacter);
            }
            return true;
        }

        public bool TryBindSpriteSet(CharacterSpriteSet2D set)
        {
            if (!isActiveAndEnabled || set == null || !set.IsValid || mainRenderer == null || spritePlayback == null || visual == null)
                return false;

            if (!spritePlayback.TryBind(mainRenderer, set)) return false;
            activeSpriteSet = set;
            worldScale = set.WorldScale;
            visual.transform.localPosition = CharacterGameplayAnimator2D.FootLocalPosition(visual.transform, bodyCollider);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(worldScale * facing, worldScale, 1f);
            wasInAttack = false;
            lastAttackStepIndex = -1;
            return true;
        }

        private void Awake()
        {
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0f), 1f);
            saberCyanSprite = LoadOrCreateSprite("Art/Vfx/ZSaberArc_Cyan");
            saberTurquoiseSprite = LoadOrCreateSprite("Art/Vfx/ZSaberArc_Turquoise");
            coffeePotSmashSprite = LoadOrCreateSprite("Art/Vfx/CoffeePot_Smash");
            miraCoffeePotEquippedSprite = LoadOrCreateSprite("Art/Vfx/Mira_CoffeePot_Equipped");
        }

        private void OnEnable()
        {
            if (manager != null)
            {
                manager.ActiveCharacterChanged -= OnActor;
                manager.ActiveCharacterChanged += OnActor;
                OnActor(null, manager.ActiveCharacter);
            }
        }

        private void OnActor(DamageReceiver2D before, DamageReceiver2D after)
        {
            if (rangedController != null) rangedController.ShotFired -= OnRangedShot;
            RestoreSuppressedActorPresenters();
            if (spritePlayback != null) spritePlayback.Unbind();
            if (visual != null) Destroy(visual);
            if (vfxSlashObject != null) Destroy(vfxSlashObject);
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                if (ghostRenderers[i] != null) Destroy(ghostRenderers[i].gameObject);
                ghostRenderers[i] = null;
                historySprites[i] = null;
            }
            ReleaseActorRuntimeAssets();

            current = after;
            mainRenderer = null;
            spritePlayback = null;
            activeSpriteSet = null;
            vfxSlashRenderer = null;
            attackController = null;
            dashController = null;
            jumpController = null;
            bodyCollider = null;
            rangedController = null;
            propWeapon = null;
            body2D = null;
            wasInAttack = false;
            lastAttackStepIndex = -1;
            airborneLatched = false;

            if (after == null)
            {
                visual = null;
                return;
            }

            SuppressDuplicateActorPresenters(after);

            attackController = after.GetComponent<AttackController2D>();
            dashController = after.GetComponent<DashController2D>();
            jumpController = after.GetComponent<JumpController2D>();
            bodyCollider = CharacterGameplayAnimator2D.FindRootBodyCollider(after.transform);
            rangedController = after.GetComponent<ZoeyRangedAttack2D>();
            propWeapon = after.GetComponent<HuntrX.Gameplay.Resources.TemporaryPropWeapon2D>();
            body2D = after.GetComponent<Rigidbody2D>();
            if (rangedController != null) rangedController.ShotFired += OnRangedShot;

            visual = new GameObject("Player prototype silhouette");
            visual.transform.SetParent(after.transform, false);
            visual.transform.localPosition = CharacterGameplayAnimator2D.FootLocalPosition(visual.transform, bodyCollider);
            visual.transform.localRotation = Quaternion.identity;
            mainRenderer = visual.AddComponent<SpriteRenderer>();
            mainRenderer.enabled = characterVisible;
            mainRenderer.sortingOrder = 15;
            mainRenderer.color = Color.white;
            spritePlayback = visual.AddComponent<CharacterSpritePlayback2D>();

            string identity = manager != null && manager.ActivePrefab != null ? manager.ActivePrefab.name : after.name;
            activeHeroKey = identity.Contains("Mira") ? "Mira" : identity.Contains("Zoey") ? "Zoey" : "Rumi";

            CharacterSpriteSet2D resolvedSet = ResolveAssignedSpriteSet(activeHeroKey);
            if (resolvedSet == null)
            {
                resolvedSet = CharacterArtRenderer2D.BuildRuntimeSpriteSet(activeHeroKey, 2.25f, ownedActorRuntimeAssets);
            }

            if (resolvedSet == null || !TryBindSpriteSet(resolvedSet))
            {
                worldScale = 1f;
                visual.transform.localScale = new Vector3(facing, 1f, 1f);
                mainRenderer.sprite = sprite;
            }

            vfxSlashObject = new GameObject("Capcom_WeaponSlash_VFX");
            vfxSlashObject.transform.SetParent(after.transform, false);
            vfxSlashRenderer = vfxSlashObject.AddComponent<SpriteRenderer>();
            // Keep the weapon arc visible over enemies without covering the active heroine.
            vfxSlashRenderer.sortingOrder = mainRenderer.sortingOrder - 1;
            vfxSlashRenderer.enabled = false;

            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                GameObject ghostGo = new GameObject("Capcom_GhostTrail_" + i);
                ghostGo.transform.SetParent(transform, true);
                SpriteRenderer gr = ghostGo.AddComponent<SpriteRenderer>();
                gr.sortingOrder = 12 - i;
                gr.enabled = false;
                ghostRenderers[i] = gr;
                historyPositions[i] = after.transform.position;
                historySprites[i] = mainRenderer.sprite;
                historyFacing[i] = facing;
            }
        }

        private CharacterSpriteSet2D ResolveAssignedSpriteSet(string heroKey)
        {
            if (characterSpriteSets != null)
            {
                for (int i = 0; i < characterSpriteSets.Length; i++)
                {
                    CharacterSpriteSet2D candidate = characterSpriteSets[i];
                    if (candidate != null && candidate.IsValid &&
                        string.Equals(candidate.CharacterName, heroKey, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return candidate;
                    }
                }
            }

            if (spriteSetOverride != null && spriteSetOverride.IsValid)
            {
                return spriteSetOverride;
            }

            return null;
        }

        private void SuppressDuplicateActorPresenters(DamageReceiver2D actor)
        {
            if (actor == null) return;

            suppressedBlockout = actor.GetComponent<HuntrX.Gameplay.Levels.BlockoutPlayerSilhouette2D>();
            if (suppressedBlockout != null)
            {
                priorBlockoutEnabled = suppressedBlockout.enabled;
                suppressedBlockout.enabled = false;
            }

            suppressedAnimator = actor.GetComponent<CharacterGameplayAnimator2D>();
            if (suppressedAnimator != null)
            {
                priorAnimatorEnabled = suppressedAnimator.enabled;
                suppressedAnimator.enabled = false;
            }

            suppressedArtRenderer = actor.GetComponent<CharacterArtRenderer2D>();
            if (suppressedArtRenderer != null)
            {
                priorArtRendererEnabled = suppressedArtRenderer.enabled;
                if (suppressedArtRenderer.VisualRoot != null)
                {
                    suppressedVisualRoot = suppressedArtRenderer.VisualRoot.gameObject;
                    priorVisualRootActive = suppressedVisualRoot.activeSelf;
                    suppressedVisualRoot.SetActive(false);
                }
                suppressedArtRenderer.enabled = false;
            }
        }

        private void RestoreSuppressedActorPresenters()
        {
            if (suppressedArtRenderer != null)
            {
                suppressedArtRenderer.enabled = priorArtRendererEnabled;
            }
            if (suppressedVisualRoot != null)
            {
                suppressedVisualRoot.SetActive(priorVisualRootActive);
            }
            if (suppressedAnimator != null)
            {
                suppressedAnimator.enabled = priorAnimatorEnabled;
            }
            if (suppressedBlockout != null)
            {
                suppressedBlockout.enabled = priorBlockoutEnabled;
            }

            suppressedArtRenderer = null;
            suppressedVisualRoot = null;
            suppressedAnimator = null;
            suppressedBlockout = null;
        }

        private void OnRangedShot(Vector2 from, Vector2 to)
        {
            rangedVfxTimer = 0.22f;
            if (Mathf.Abs(to.x - from.x) > 0.01f) facing = to.x >= from.x ? 1 : -1;
        }

        private void LateUpdate()
        {
            if (current == null || followCamera == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            animClock = (animClock + dt) % 1000f;
            if (rangedVfxTimer > 0f) rangedVfxTimer = Mathf.Max(0f, rangedVfxTimer - dt);

            Vector2 vel = body2D != null ? body2D.linearVelocity : Vector2.zero;
            if (float.IsNaN(vel.x) || float.IsInfinity(vel.x)) vel.x = 0f;
            if (float.IsNaN(vel.y) || float.IsInfinity(vel.y)) vel.y = 0f;

            if (attackController != null && attackController.State != AttackState2D.Idle)
            {
                facing = attackController.LastFacingDirection >= 0f ? 1 : -1;
            }
            else if (Mathf.Abs(vel.x) > 0.12f)
            {
                facing = vel.x >= 0f ? 1 : -1;
            }

            UpdateAnimatedHeroSprite(vel, dt);
            UpdateCapcomSlashAndMartialVfx();

        }

        private void UpdateAnimatedHeroSprite(Vector2 vel, float dt)
        {
            if (visual == null || mainRenderer == null) return;
            if (propWeapon == null && current != null)
            {
                propWeapon = current.GetComponent<HuntrX.Gameplay.Resources.TemporaryPropWeapon2D>();
            }

            bool isDead = !current.IsAlive;
            bool isAttacking = (attackController != null && attackController.State != AttackState2D.Idle) || rangedVfxTimer > 0f;
            bool isDashing = dashController != null && dashController.IsDashing;
            bool isGrounded = CharacterGameplayAnimator2D.EvaluateGrounded(
                current.transform,
                bodyCollider,
                jumpController,
                dashController,
                vel,
                ref airborneLatched);
            bool isAirborne = !isGrounded;
            bool isRunning = Mathf.Abs(vel.x) > 0.15f;
            int step = attackController != null ? attackController.CurrentComboStepIndex : 0;

            CharacterVisualPose pose;
            if (isDead)
                pose = CharacterVisualPose.Death;
            else if (isDashing)
                pose = CharacterVisualPose.Dash;
            else if (isAttacking)
                pose = isAirborne ? CharacterVisualPose.Aerial : CharacterVisualPose.Attack;
            else if (isAirborne)
                pose = CharacterVisualPose.Jump;
            else if (isRunning)
                pose = CharacterVisualPose.Run;
            else
                pose = CharacterVisualPose.Idle;

            if (spritePlayback != null && activeSpriteSet != null)
            {
                worldScale = activeSpriteSet.WorldScale;
                CharacterGameplayAnimator2D.DrivePlayback(
                    spritePlayback,
                    activeSpriteSet,
                    pose,
                    vel,
                    isAirborne,
                    attackController,
                    ref wasInAttack,
                    ref lastAttackStepIndex);
            }

            mainRenderer.color = isDashing ? new Color(0.8f, 0.96f, 1f, 1f) : Color.white;
            visual.transform.localPosition = CharacterGameplayAnimator2D.FootLocalPosition(visual.transform, bodyCollider);
            visual.transform.localScale = new Vector3(worldScale * facing, worldScale, 1f);
            visual.transform.localRotation = Quaternion.identity;

            bool isAerialAttack = isAttacking && isAirborne;
            UpdateGhostTrails(dt, isDashing || isAerialAttack || (isAttacking && step >= 1));
        }

        private void UpdateGhostTrails(float dt, bool emitGhosts)
        {
            if (current == null || mainRenderer == null) return;
            ghostSampleTimer += dt;
            if (ghostSampleTimer >= 0.035f)
            {
                ghostSampleTimer = 0f;
                for (int i = ghostRenderers.Length - 1; i > 0; i--)
                {
                    historyPositions[i] = historyPositions[i - 1];
                    historySprites[i] = historySprites[i - 1];
                    historyFacing[i] = historyFacing[i - 1];
                }
                historyPositions[0] = current.transform.position;
                historySprites[0] = mainRenderer.sprite;
                historyFacing[0] = facing;
            }

            Color ghostBase = activeHeroKey == "Mira"
                ? new Color(1f, 0.28f, 0.55f, 0.42f)
                : activeHeroKey == "Zoey"
                    ? new Color(0.15f, 0.98f, 0.85f, 0.42f)
                    : new Color(0.25f, 0.88f, 1f, 0.42f);

            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                SpriteRenderer gr = ghostRenderers[i];
                if (gr == null) continue;
                if (!emitGhosts || !characterVisible || historySprites[i] == null)
                {
                    gr.enabled = false;
                    continue;
                }
                gr.enabled = true;
                gr.sprite = historySprites[i];
                gr.transform.position = historyPositions[i] - new Vector3(historyFacing[i] * (i + 1) * 0.22f, 0f, 0f);
                gr.transform.localScale = new Vector3(worldScale * historyFacing[i], worldScale, 1f);
                Color c = ghostBase;
                c.a = ghostBase.a * (1f - i * 0.28f);
                gr.color = c;
            }
        }

        private void UpdateCapcomSlashAndMartialVfx()
        {
            if (vfxSlashObject == null || vfxSlashRenderer == null) return;

            bool activeMelee = attackController != null && (attackController.State == AttackState2D.Startup || attackController.State == AttackState2D.Active);
            bool activeRanged = rangedVfxTimer > 0f;

            if (!activeMelee && !activeRanged)
            {
                vfxSlashRenderer.enabled = false;
                return;
            }

            int step = attackController != null ? attackController.CurrentComboStepIndex : 0;
            bool hasEquippedProp = propWeapon != null && propWeapon.HasEquippedProp;
            Sprite chosenVfx = saberCyanSprite;
            float vfxHeight = 1.95f;
            Vector3 localPos = new Vector3(facing * 1.05f, 0.15f, 0f);
            float rotZ = 0f;

            if (hasEquippedProp && coffeePotSmashSprite != null)
            {
                chosenVfx = coffeePotSmashSprite;
                vfxHeight = 1.75f;
                localPos = new Vector3(facing * 1.25f, 0.25f, 0f);
                vfxSlashRenderer.color = Color.white;
            }
            else if (activeHeroKey == "Mira")
            {
                chosenVfx = saberCyanSprite;
                vfxHeight = step >= 2 ? 2.55f : 1.95f;
                rotZ = step >= 2 ? (animClock * 720f) % 360f : 0f;
                localPos = step >= 2 ? new Vector3(facing * 0.55f, 0.45f, 0f) : new Vector3(facing * 1.05f, 0.15f, 0f);
                vfxSlashRenderer.color = new Color(1f, 0.35f, 0.55f, 0.95f);
            }
            else if (activeHeroKey == "Zoey")
            {
                chosenVfx = saberTurquoiseSprite != null ? saberTurquoiseSprite : saberCyanSprite;
                vfxHeight = step >= 2 ? 2.25f : 1.80f;
                rotZ = step >= 2 ? (animClock * 540f) % 360f : 0f;
                vfxSlashRenderer.color = Color.white;
            }
            else
            {
                chosenVfx = saberCyanSprite;
                bool isAirborne = body2D != null && Mathf.Abs(body2D.linearVelocity.y) > 0.2f;
                vfxHeight = step >= 2 || isAirborne ? 2.45f : 1.95f;
                rotZ = isAirborne ? -45f * facing : (step == 1 ? 35f * facing : (step >= 2 ? -25f * facing : 0f));
                vfxSlashRenderer.color = isAirborne ? new Color(1f, 0.22f, 0.28f, 0.96f) : Color.white;
            }

            if (chosenVfx == null)
            {
                vfxSlashRenderer.enabled = false;
                return;
            }

            vfxSlashRenderer.sprite = chosenVfx;
            vfxSlashRenderer.enabled = characterVisible;
            float h = chosenVfx.bounds.size.y > 0.01f ? chosenVfx.bounds.size.y : 1f;
            float s = vfxHeight / h;
            vfxSlashObject.transform.localPosition = localPos;
            vfxSlashObject.transform.localScale = new Vector3(s * facing, s, 1f);
            vfxSlashObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        }

        private Sprite LoadOrCreateSprite(string resourcePath)
        {
            Sprite s = UnityEngine.Resources.Load<Sprite>(resourcePath);
            if (s != null) return s;
            Texture2D tex = UnityEngine.Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                ownedRuntimeAssets.Add(s);
                return s;
            }
            return null;
        }

        private void Unbind()
        {
            if (rangedController != null) rangedController.ShotFired -= OnRangedShot;
            if (manager != null) manager.ActiveCharacterChanged -= OnActor;
            RestoreSuppressedActorPresenters();
            if (spritePlayback != null) spritePlayback.Unbind();
            if (visual != null) Destroy(visual);
            if (vfxSlashObject != null) Destroy(vfxSlashObject);
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                if (ghostRenderers[i] != null) Destroy(ghostRenderers[i].gameObject);
                ghostRenderers[i] = null;
                historySprites[i] = null;
            }
            ReleaseActorRuntimeAssets();
            current = null;
            visual = null;
            vfxSlashObject = null;
            mainRenderer = null;
            spritePlayback = null;
            activeSpriteSet = null;
            vfxSlashRenderer = null;
            wasInAttack = false;
            lastAttackStepIndex = -1;
            airborneLatched = false;
        }

        private void OnDisable() => Unbind();

        private void ReleaseActorRuntimeAssets()
        {
            for (int i = ownedActorRuntimeAssets.Count - 1; i >= 0; i--)
            {
                if (ownedActorRuntimeAssets[i] != null) Destroy(ownedActorRuntimeAssets[i]);
            }
            ownedActorRuntimeAssets.Clear();
        }

        private void OnDestroy()
        {
            Unbind();
            if (sprite != null) Destroy(sprite);
            for (int i = 0; i < ownedRuntimeAssets.Count; i++)
            {
                if (ownedRuntimeAssets[i] != null) Destroy(ownedRuntimeAssets[i]);
            }
            ownedRuntimeAssets.Clear();
        }
    }
}
