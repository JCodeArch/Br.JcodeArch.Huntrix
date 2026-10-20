using System.Collections.Generic;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using UnityEngine;

namespace HuntrX.Gameplay.VerticalSlice
{
    [DisallowMultipleComponent]
    public sealed class SlicePlayerPresentation2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private Camera followCamera;
        private Sprite sprite;
        private Sprite customSprite;
        private bool ownsCustomSprite;
        private DamageReceiver2D current;
        private GameObject visual;
        private SpriteRenderer mainRenderer;
        private GameObject vfxSlashObject;
        private SpriteRenderer vfxSlashRenderer;
        private bool characterVisible = true;

        private string activeHeroKey = "Rumi";
        private readonly Dictionary<string, Sprite[]> poseCache = new Dictionary<string, Sprite[]>();
        private readonly List<Sprite> ownedRuntimeSprites = new List<Sprite>(64);
        private Sprite saberCyanSprite;
        private Sprite saberTurquoiseSprite;
        private Sprite coffeePotSmashSprite;
        private Sprite miraCoffeePotEquippedSprite;

        private AttackController2D attackController;
        private DashController2D dashController;
        private ZoeyRangedAttack2D rangedController;
        private HuntrX.Gameplay.Resources.TemporaryPropWeapon2D propWeapon;
        private Rigidbody2D body2D;
        private float animClock;
        private float rangedVfxTimer;
        private int facing = 1;
        private float baseScale = 1f;

        private readonly SpriteRenderer[] ghostRenderers = new SpriteRenderer[3];
        private readonly Vector3[] historyPositions = new Vector3[3];
        private readonly Sprite[] historySprites = new Sprite[3];
        private readonly int[] historyFacing = new int[3];
        private float ghostSampleTimer;

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

        private void Awake()
        {
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
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
            if (visual != null) Destroy(visual);
            if (vfxSlashObject != null) Destroy(vfxSlashObject);
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                if (ghostRenderers[i] != null) Destroy(ghostRenderers[i].gameObject);
                ghostRenderers[i] = null;
            }
            if (ownsCustomSprite && customSprite != null) Destroy(customSprite);
            customSprite = null;
            ownsCustomSprite = false;
            current = after;
            mainRenderer = null;
            vfxSlashRenderer = null;
            attackController = null;
            dashController = null;
            rangedController = null;
            propWeapon = null;
            body2D = null;

            if (after == null)
            {
                visual = null;
                return;
            }

            // Ensure any legacy blockout rectangle on the character is hidden
            var blockout = after.GetComponent<HuntrX.Gameplay.Levels.BlockoutPlayerSilhouette2D>();
            if (blockout != null) blockout.enabled = false;

            attackController = after.GetComponent<AttackController2D>();
            dashController = after.GetComponent<DashController2D>();
            rangedController = after.GetComponent<ZoeyRangedAttack2D>();
            propWeapon = after.GetComponent<HuntrX.Gameplay.Resources.TemporaryPropWeapon2D>();
            body2D = after.GetComponent<Rigidbody2D>();
            if (rangedController != null) rangedController.ShotFired += OnRangedShot;

            visual = new GameObject("Player prototype silhouette");
            visual.transform.SetParent(after.transform, false);
            mainRenderer = visual.AddComponent<SpriteRenderer>();
            mainRenderer.enabled = characterVisible;
            mainRenderer.sortingOrder = 15;

            string identity = manager.ActivePrefab != null ? manager.ActivePrefab.name : after.name;
            activeHeroKey = identity.Contains("Mira") ? "Mira" : identity.Contains("Zoey") ? "Zoey" : "Rumi";

            Sprite[] idleFrames = GetPoseFrames(activeHeroKey, "Idle");
            if (idleFrames.Length > 0)
            {
                customSprite = idleFrames[0];
            }
            else
            {
                customSprite = LoadOrCreateSprite("Art/Characters/" + activeHeroKey);
            }

            if (customSprite != null && customSprite.bounds.size.y > 0.01f)
            {
                mainRenderer.sprite = customSprite;
                mainRenderer.color = Color.white;
                baseScale = 2.25f / customSprite.bounds.size.y;
                visual.transform.localScale = new Vector3(baseScale * facing, baseScale, 1f);
            }
            else
            {
                baseScale = 1f;
                visual.transform.localScale = new Vector3(0.6f, 1.2f, 1f);
                mainRenderer.sprite = sprite;
                mainRenderer.color = Color.white;
            }

            // Create Capcom Zero-style Z-Saber / Coffee Pot VFX child
            vfxSlashObject = new GameObject("Capcom_WeaponSlash_VFX");
            vfxSlashObject.transform.SetParent(after.transform, false);
            vfxSlashRenderer = vfxSlashObject.AddComponent<SpriteRenderer>();
            vfxSlashRenderer.sortingOrder = 22;
            vfxSlashRenderer.enabled = false;

            // Create 3 Capcom Zero-style Neon Afterimage Ghost Trails
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

        private void OnRangedShot(Vector2 from, Vector2 to)
        {
            rangedVfxTimer = 0.22f;
            if (Mathf.Abs(to.x - from.x) > 0.01f) facing = to.x >= from.x ? 1 : -1;
        }

        private void LateUpdate()
        {
            if (current == null || followCamera == null) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
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

            Vector3 point = current.transform.position + new Vector3(1.55f * facing, 0.95f, -10f);
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, point, 1f - Mathf.Exp(-8f * dt));
        }

        private void UpdateAnimatedHeroSprite(Vector2 vel, float dt)
        {
            if (visual == null || mainRenderer == null) return;
            if (propWeapon == null && current != null)
            {
                propWeapon = current.GetComponent<HuntrX.Gameplay.Resources.TemporaryPropWeapon2D>();
            }

            bool isAttacking = (attackController != null && attackController.State != AttackState2D.Idle) || rangedVfxTimer > 0f;
            bool isDashing = dashController != null && dashController.IsDashing;
            bool isAirborne = Mathf.Abs(vel.y) > 0.2f;
            bool isRunning = Mathf.Abs(vel.x) > 0.15f;
            int step = attackController != null ? attackController.CurrentComboStepIndex : 0;
            bool hasEquippedProp = propWeapon != null && propWeapon.HasEquippedProp;

            // Rumi's Aerial Red-Sole Downward Dive Kick when attacking in the air!
            bool isAerialAttack = isAttacking && isAirborne;
            string pose = isAerialAttack ? "Jump" : isAttacking ? "Attack" : (isAirborne || isDashing) ? "Jump" : isRunning ? "Run" : "Idle";
            Sprite[] frames = GetPoseFrames(activeHeroKey, pose);

            Sprite selectedSprite = null;
            if (isAttacking && activeHeroKey == "Mira" && hasEquippedProp && miraCoffeePotEquippedSprite != null)
            {
                selectedSprite = miraCoffeePotEquippedSprite;
            }
            else if (frames.Length > 0)
            {
                int idx;
                if (isAerialAttack)
                {
                    // Frame 3 of Jump is Rumi's Red-Sole Downward Dive Kick / Zoey's Aerial Kunai Storm / Mira's Aerial Cleave
                    idx = frames.Length - 1;
                }
                else if (isAttacking && attackController != null && attackController.State != AttackState2D.Idle)
                {
                    // 0 = Slash 1 / Mira Brawler Punch, 1 = Slash 2 / Zoey Thrown Daggers / Mira Gokdo Sweep,
                    // 2 = Rumi Kick / Zoey Leg Chokehold / Mira Spinning-Blade Shoryuken, 3 = Finisher
                    idx = Mathf.Clamp(step, 0, frames.Length - 1);
                }
                else
                {
                    float cycleDuration = isRunning ? 0.36f : isAirborne ? 0.45f : 0.68f;
                    idx = Mathf.Abs(Mathf.FloorToInt((animClock / cycleDuration) * frames.Length)) % frames.Length;
                }
                selectedSprite = frames[idx];
            }

            if (selectedSprite != null)
            {
                mainRenderer.sprite = selectedSprite;
                if (selectedSprite.bounds.size.y > 0.01f)
                {
                    baseScale = 2.25f / selectedSprite.bounds.size.y;
                }
            }

            // Natural 60 FPS breathing, running stride bounce, and martial arts squash-and-stretch
            float tiltZ = 0f;
            Vector3 offset = Vector3.zero;
            float stretchX = 1f;
            float stretchY = 1f;

            if (isDashing)
            {
                tiltZ = -facing * 14f;
                stretchX = 1.12f;
                stretchY = 0.92f;
                mainRenderer.color = new Color(0.8f, 0.96f, 1f, 1f);
            }
            else if (isAttacking)
            {
                float attackPulse = Mathf.Sin(animClock * 28f) * 0.05f;
                stretchX = 1.06f + attackPulse;
                stretchY = 0.96f - attackPulse * 0.5f;
                if (isAerialAttack && activeHeroKey == "Rumi")
                {
                    // Rumi's Aerial Red-Sole Downward Dive Kick lunge
                    offset = new Vector3(facing * 0.25f, -0.18f, 0f);
                    tiltZ = -facing * 10f;
                }
                else if (step >= 2)
                {
                    if (activeHeroKey == "Zoey")
                    {
                        offset = new Vector3(facing * 0.22f, 0.25f, 0f);
                        tiltZ = -facing * 8f;
                    }
                    else if (activeHeroKey == "Rumi")
                    {
                        offset = new Vector3(facing * 0.2f, 0.14f, 0f);
                        tiltZ = facing * 6f;
                    }
                    else
                    {
                        // Mira's Spinning-Blade Shoryuken rising vertical uppercut!
                        offset = new Vector3(facing * 0.22f, 0.38f, 0f);
                        tiltZ = facing * 8f;
                    }
                }
                mainRenderer.color = Color.white;
            }
            else if (isAirborne)
            {
                float airStretch = Mathf.Clamp(vel.y * 0.018f, -0.08f, 0.10f);
                stretchX = 1f - airStretch * 0.6f;
                stretchY = 1f + airStretch;
                tiltZ = -facing * Mathf.Clamp(vel.y * 1.4f, -10f, 10f);
                mainRenderer.color = Color.white;
            }
            else if (isRunning)
            {
                float runWave = Mathf.Sin(animClock * 18f);
                offset = new Vector3(0f, Mathf.Abs(runWave) * 0.07f, 0f);
                stretchX = 1f + runWave * 0.045f;
                stretchY = 1f - Mathf.Abs(runWave) * 0.04f;
                tiltZ = -facing * (5f + runWave * 3f);
                mainRenderer.color = Color.white;
            }
            else
            {
                // Subtle natural idle breathing & rhythm sway
                float breath = Mathf.Sin(animClock * 4.2f);
                offset = new Vector3(0f, breath * 0.025f, 0f);
                stretchX = 1f - breath * 0.018f;
                stretchY = 1f + breath * 0.024f;
                mainRenderer.color = Color.white;
            }

            visual.transform.localPosition = offset;
            visual.transform.localScale = new Vector3(baseScale * stretchX * facing, baseScale * stretchY, 1f);
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, tiltZ);

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
                historyPositions[0] = current.transform.position + visual.transform.localPosition;
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
                float h = gr.sprite.bounds.size.y > 0.01f ? (2.25f / gr.sprite.bounds.size.y) : baseScale;
                gr.transform.position = historyPositions[i] - new Vector3(historyFacing[i] * (i + 1) * 0.22f, 0f, 0f);
                gr.transform.localScale = new Vector3(h * historyFacing[i], h, 1f);
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
                // Temporary Equippable Stage Prop (e.g. Airline Coffee Pot Smash!)
                chosenVfx = coffeePotSmashSprite;
                vfxHeight = 1.75f;
                localPos = new Vector3(facing * 1.25f, 0.25f, 0f);
                vfxSlashRenderer.color = Color.white;
            }
            else if (activeHeroKey == "Mira")
            {
                chosenVfx = saberCyanSprite;
                vfxHeight = step >= 2 ? 2.55f : 1.95f;
                rotZ = step >= 2 ? (animClock * 720f) % 360f : 0f; // 360-degree Spinning-Blade Shoryuken vortex
                localPos = step >= 2 ? new Vector3(facing * 0.55f, 0.45f, 0f) : new Vector3(facing * 1.05f, 0.15f, 0f);
                vfxSlashRenderer.color = new Color(1f, 0.35f, 0.55f, 0.95f); // Crimson-gold Gokdo arc
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
                // Rumi's Saingeom Z-Saber Arc, Taekwondo High Kick Wave & Red-Sole Dive Kick
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

        private Sprite[] GetPoseFrames(string heroKey, string poseName)
        {
            string cacheKey = heroKey + ":" + poseName;
            if (poseCache.TryGetValue(cacheKey, out Sprite[] cached)) return cached;

            var list = new List<Sprite>(4);
            for (int i = 0; i < 4; i++)
            {
                Sprite s = LoadOrCreateSprite("Art/Animations/" + heroKey + "/" + poseName + "/" + i);
                if (s == null) break;
                list.Add(s);
            }
            Sprite[] arr = list.ToArray();
            poseCache[cacheKey] = arr;
            return arr;
        }

        private Sprite LoadOrCreateSprite(string resourcePath)
        {
            Sprite s = UnityEngine.Resources.Load<Sprite>(resourcePath);
            if (s != null) return s;
            Texture2D tex = UnityEngine.Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                ownedRuntimeSprites.Add(s);
                return s;
            }
            return null;
        }

        private void Unbind()
        {
            if (rangedController != null) rangedController.ShotFired -= OnRangedShot;
            if (manager != null) manager.ActiveCharacterChanged -= OnActor;
            if (visual != null) Destroy(visual);
            if (vfxSlashObject != null) Destroy(vfxSlashObject);
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                if (ghostRenderers[i] != null) Destroy(ghostRenderers[i].gameObject);
                ghostRenderers[i] = null;
            }
            if (ownsCustomSprite && customSprite != null) Destroy(customSprite);
            customSprite = null;
            ownsCustomSprite = false;
            current = null;
            visual = null;
            vfxSlashObject = null;
            mainRenderer = null;
            vfxSlashRenderer = null;
        }

        private void OnDisable() => Unbind();

        private void OnDestroy()
        {
            Unbind();
            if (sprite != null) Destroy(sprite);
            for (int i = 0; i < ownedRuntimeSprites.Count; i++)
            {
                if (ownedRuntimeSprites[i] != null) Destroy(ownedRuntimeSprites[i]);
            }
            ownedRuntimeSprites.Clear();
        }
    }
}
