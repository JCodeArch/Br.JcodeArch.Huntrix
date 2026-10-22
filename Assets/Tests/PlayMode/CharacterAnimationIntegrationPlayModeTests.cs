using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CharacterAnimationIntegrationPlayModeTests
    {
        private readonly List<Object> owned = new List<Object>();
        private Texture2D uniformTexture;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            uniformTexture = new Texture2D(64, 64);
            owned.Add(uniformTexture);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }
            owned.Clear();
        }

        private Sprite MakeUniformFrame(float ppu = 32f)
        {
            Sprite s = Sprite.Create(uniformTexture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0f), ppu);
            owned.Add(s);
            return s;
        }

        private CharacterSpriteSet2D CreateCatalog(
            out Sprite idle0,
            out Sprite run0,
            out Sprite jumpRise,
            out Sprite fall,
            out Sprite atk0,
            out Sprite atk1,
            out Sprite atk2,
            out Sprite death0,
            out Sprite death1)
        {
            idle0 = MakeUniformFrame();
            run0 = MakeUniformFrame();
            jumpRise = MakeUniformFrame();
            fall = MakeUniformFrame();
            atk0 = MakeUniformFrame();
            atk1 = MakeUniformFrame();
            atk2 = MakeUniformFrame();
            death0 = MakeUniformFrame();
            death1 = MakeUniformFrame();

            var set = ScriptableObject.CreateInstance<CharacterSpriteSet2D>();
            owned.Add(set);
            SetPrivate(set, "characterName", "Rumi");
            SetPrivate(set, "pixelsPerUnit", 32f);
            SetPrivate(set, "worldScale", 1.75f);
            SetPrivate(set, "footAnchor", new Vector2(0.5f, 0f));
            SetPrivate(set, "states", new[]
            {
                new SpriteAnimationState2D("Idle", new SpriteAnimationClip2D(new[] { idle0 }, new[] { 0.2f }, true)),
                new SpriteAnimationState2D("Run", new SpriteAnimationClip2D(new[] { run0 }, new[] { 0.2f }, true)),
                new SpriteAnimationState2D("Jump", new SpriteAnimationClip2D(new[] { jumpRise }, new[] { 0.2f }, false)),
                new SpriteAnimationState2D("Fall", new SpriteAnimationClip2D(new[] { fall }, new[] { 0.2f }, false)),
                new SpriteAnimationState2D("Attack", new SpriteAnimationClip2D(new[] { atk0, atk1, atk2 }, new[] { 0.08f, 0.08f, 0.08f }, false)),
                new SpriteAnimationState2D("Death", new SpriteAnimationClip2D(new[] { death0, death1 }, new[] { 0.1f, 0.1f }, false))
            });
            Assert.That(set.IsValid, Is.True, "Test CharacterSpriteSet2D must be valid.");
            return set;
        }

        private CharacterSpriteSet2D CreateCatalogWithoutIdle()
        {
            var set = CreateCatalog(out Sprite frame, out _, out _, out _, out _, out _, out _, out _, out _);
            SetPrivate(set, "worldScale", 0.5f);
            SetPrivate(set, "states", new[] {
                new SpriteAnimationState2D("Run", new SpriteAnimationClip2D(new[] { frame }, new[] { 0.2f }, true))
            });
            Assert.That(set.IsValid, Is.True);
            return set;
        }

        [Test]
        public void RejectedCatalogPreservesArtRendererCatalogFrameAndScale()
        {
            var catalog = CreateCatalog(out Sprite idle, out _, out _, out _, out _, out _, out _, out _, out _);
            CreateConfiguredActor(catalog, out _, out _, out _, out _, out CharacterArtRenderer2D renderer, out _);
            Vector3 priorScale = renderer.SpriteRenderer.transform.localScale;
            Assert.That(renderer.TryBindSpriteSet(CreateCatalogWithoutIdle()), Is.False);
            Assert.That(renderer.SpriteSet, Is.SameAs(catalog));
            Assert.That(renderer.SpriteRenderer.sprite, Is.SameAs(idle));
            Assert.That(renderer.SpriteRenderer.transform.localScale, Is.EqualTo(priorScale));
        }

        private AttackDefinition CreateAttackDefinition(string id, float damage, float startup, float active, float recovery)
        {
            var atk = ScriptableObject.CreateInstance<AttackDefinition>();
            owned.Add(atk);
            SetBasePrivate(atk, "id", id);
            SetPrivate(atk, "damage", damage);
            SetPrivate(atk, "startupDuration", startup);
            SetPrivate(atk, "activeDuration", active);
            SetPrivate(atk, "recoveryDuration", recovery);
            SetPrivate(atk, "hitboxSize", new Vector2(1.2f, 1.0f));
            SetPrivate(atk, "hitboxOffset", new Vector2(0.8f, 0f));
            SetPrivate(atk, "horizontalKnockbackImpulse", 2.5f);
            SetPrivate(atk, "upwardKnockbackImpulse", 0f);
            Assert.That(atk.IsValid(out string error), Is.True, error);
            return atk;
        }

        private CombatComboDefinition CreateVariableDurationTwoStepCombo()
        {
            // Step 0: fast 0.24s total (0.08s startup, 0.08s active, 0.08s recovery)
            AttackDefinition step0Atk = CreateAttackDefinition("slash_fast", 10f, 0.08f, 0.08f, 0.08f);
            // Step 1: heavy 0.48s total (0.16s startup, 0.16s active, 0.16s recovery) - 2x longer than Step 0
            AttackDefinition step1Atk = CreateAttackDefinition("slash_heavy", 18f, 0.16f, 0.16f, 0.16f);

            var combo = ScriptableObject.CreateInstance<CombatComboDefinition>();
            owned.Add(combo);
            SetBasePrivate(combo, "id", "combo_test");
            var steps = new[]
            {
                new ComboStep(step0Atk, 0.04f, 0.23f, 0f),
                new ComboStep(step1Atk, 0.04f, 0.44f, 0f)
            };
            SetPrivate(combo, "groundedSteps", steps);
            SetPrivate(combo, "aerialSteps", steps);
            Assert.That(combo.IsValid(out string reason), Is.True, reason);
            return combo;
        }

        private GameObject CreateConfiguredActor(
            CharacterSpriteSet2D assignedCatalog,
            out DamageReceiver2D receiver,
            out AttackController2D attack,
            out JumpController2D jump,
            out Rigidbody2D body,
            out CharacterArtRenderer2D artRenderer,
            out CharacterGameplayAnimator2D animator)
        {
            var actor = new GameObject("Rumi_TestActor");
            owned.Add(actor);
            actor.SetActive(false);

            body = actor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var bodyCol = actor.AddComponent<BoxCollider2D>();
            bodyCol.size = new Vector2(0.8f, 1.8f);

            receiver = actor.AddComponent<DamageReceiver2D>();
            SetPrivate(receiver, "faction", CombatFaction2D.HuntrX);
            SetPrivate(receiver, "maximumHealth", 100f);

            var hitboxGo = new GameObject("Hitbox");
            hitboxGo.transform.SetParent(actor.transform, false);
            var hitboxCol = hitboxGo.AddComponent<BoxCollider2D>();
            hitboxCol.isTrigger = true;
            var hitbox = actor.AddComponent<AttackHitbox2D>();
            SetPrivate(hitbox, "hitboxCollider", hitboxCol);

            attack = actor.AddComponent<AttackController2D>();
            SetPrivate(attack, "comboDefinition", CreateVariableDurationTwoStepCombo());

            jump = actor.AddComponent<JumpController2D>();
            SetPrivate(jump, "jumpVelocity", 10f);
            SetPrivate(jump, "wallJumpHorizontalSpeed", 6f);
            SetPrivate(jump, "gravityScale", 2.5f);
            SetPrivate(jump, "coyoteTime", 0.1f);
            SetPrivate(jump, "jumpBufferTime", 0.1f);
            SetPrivate(jump, "jumpCutMultiplier", 0.5f);

            var artDef = ScriptableObject.CreateInstance<CharacterArtDefinition>();
            owned.Add(artDef);
            SetBasePrivate(artDef, "id", "rumi_art");
            SetPrivate(artDef, "characterName", "Rumi");
            SetPrivate(artDef, "height", 1.8f);
            SetPrivate(artDef, "width", 0.55f);

            var rigDef = ScriptableObject.CreateInstance<CharacterRigDefinition>();
            owned.Add(rigDef);
            SetBasePrivate(rigDef, "id", "rumi_rig");
            SetPrivate(rigDef, "character", artDef);
            SetPrivate(rigDef, "idleCycleSeconds", 0.6f);
            SetPrivate(rigDef, "runCycleSeconds", 0.4f);
            SetPrivate(rigDef, "hurtSeconds", 0.2f);
            SetPrivate(rigDef, "specialSeconds", 0.5f);

            artRenderer = actor.AddComponent<CharacterArtRenderer2D>();
            SetPrivate(artRenderer, "definition", artDef);
            SetPrivate(artRenderer, "spriteSet", assignedCatalog);

            animator = actor.AddComponent<CharacterGameplayAnimator2D>();
            SetPrivate(animator, "renderer2D", artRenderer);
            SetPrivate(animator, "rig", rigDef);

            actor.SetActive(true);
            jump.SetGrounded(true);

            Assert.That(receiver.IsConfigurationValid, Is.True);
            Assert.That(hitbox.IsConfigurationValid, Is.True);
            Assert.That(animator.TryConfigure(artRenderer, rigDef), Is.True);

            return actor;
        }

        private DamageReceiver2D CreateDemonAttacker()
        {
            var go = new GameObject("Demon_Attacker");
            owned.Add(go);
            go.SetActive(false);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            var receiver = go.AddComponent<DamageReceiver2D>();
            SetPrivate(receiver, "faction", CombatFaction2D.Demon);
            SetPrivate(receiver, "maximumHealth", 50f);
            go.SetActive(true);
            Assert.That(receiver.IsConfigurationValid, Is.True);
            return receiver;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing field " + fieldName + " on " + target.GetType().Name);
            field.SetValue(target, value);
        }

        private static void SetBasePrivate(object target, string fieldName, object value)
        {
            System.Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Assert.Fail("Missing base field " + fieldName + " on " + target.GetType().Name);
        }

        private static void InvokeNonPublic(object target, string methodName)
        {
            MethodInfo m = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(m, Is.Not.Null, "Missing " + methodName + " on " + target.GetType().Name);
            m.Invoke(target, null);
        }

        private static void StepAttackFixedTime(AttackController2D attack, float totalSeconds)
        {
            float step = Time.fixedDeltaTime;
            int count = Mathf.RoundToInt(totalSeconds / step);
            for (int i = 0; i < count; i++)
            {
                InvokeNonPublic(attack, "FixedUpdate");
            }
        }

        [Test]
        public void AttackSynchronizesWithStartupActiveRecoveryAcrossVariableStepDurationsAndFreezesWhenPaused()
        {
            CharacterSpriteSet2D catalog = CreateCatalog(out _, out _, out _, out _, out Sprite atk0, out Sprite atk1, out Sprite atk2, out _, out _);
            CreateConfiguredActor(catalog, out _, out AttackController2D attack, out _, out _, out CharacterArtRenderer2D artRenderer, out CharacterGameplayAnimator2D animator);

            SpriteRenderer sr = artRenderer.VisualRoot.GetComponentInChildren<SpriteRenderer>();
            var playback = artRenderer.VisualRoot.GetComponentInChildren<CharacterSpritePlayback2D>();
            Assert.That(playback, Is.Not.Null, "CharacterSpritePlayback2D must be bound automatically from assigned spriteSet.");

            // Start Step 0 (0.06s startup, 0.06s active, 0.06s recovery = 0.18s total)
            Assert.That(attack.TryStartAttack(1f, true), Is.True);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Startup));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk0), "Startup of Step 0 must present frame 0 (atk0).");

            // Pause / HitStop (Time.timeScale == 0): must not drift even if Update/LateUpdate run repeatedly
            float elapsedBeforePause = playback.Elapsed;
            Time.timeScale = 0f;
            InvokeNonPublic(playback, "Update");
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(playback.Elapsed, Is.EqualTo(elapsedBeforePause).Within(0.0001f), "Attack playback must not advance or drift during pause/hitstop.");
            Assert.That(sr.sprite, Is.SameAs(atk0));
            Time.timeScale = 1f;

            // Advance AttackController2D into Active of Step 0 (5 FixedUpdate ticks = 0.10s -> Active)
            StepAttackFixedTime(attack, 0.10f);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Active));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk1), "Active phase of Step 0 must advance to atk1 instead of freezing on combo step index 0.");

            // Advance into Recovery of Step 0 (+0.10s -> Recovery)
            StepAttackFixedTime(attack, 0.10f);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Recovery));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk2), "Recovery phase of Step 0 must present atk2.");

            // Chain into Combo Step 1 (0.16s startup, 0.16s active, 0.16s recovery = 0.48s total, 2x slower than Step 0)
            Assert.That(attack.TryStartAttack(1f, true), Is.True);
            Assert.That(attack.CurrentComboStepIndex, Is.EqualTo(1));
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Startup));

            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(playback.Elapsed, Is.Zero, "Advancing to combo step 1 must restart the attack clip clock.");
            Assert.That(sr.sprite, Is.SameAs(atk0), "Combo step 1 must start at frame 0 (atk0).");

            // Advance 0.12s into Step 1: still in Startup (0.12s < 0.16s, 25% of Step 1 -> 0.06s in 0.24s clip -> still atk0, NO premature drift into atk1!)
            StepAttackFixedTime(attack, 0.12f);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Startup));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk0), "Longer Step 1 Startup (0.12s of 0.48s) must not drift early into atk1.");

            // Advance +0.08s into Step 1 Active (0.20s -> Active)
            StepAttackFixedTime(attack, 0.08f);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Active));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk1), "Step 1 Active phase must present atk1 without drift.");

            // Advance +0.18s into Step 1 Recovery (0.38s -> Recovery)
            StepAttackFixedTime(attack, 0.18f);
            Assert.That(attack.State, Is.EqualTo(AttackState2D.Recovery));
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(atk2), "Step 1 Recovery phase must present atk2 without drift.");
        }

        [Test]
        public void JumpAndFallCoverAirborneApexWithoutIdleFlashAndRealGroundContact()
        {
            // Create real ground platform collider at y = -1.0 (top surface at y = -0.5)
            var floor = new GameObject("GroundFloor");
            owned.Add(floor);
            floor.transform.position = new Vector3(0f, -1.0f, 0f);
            var floorCol = floor.AddComponent<BoxCollider2D>();
            floorCol.size = new Vector2(10f, 1f);

            CharacterSpriteSet2D catalog = CreateCatalog(out Sprite idle0, out _, out Sprite jumpRise, out Sprite fall, out _, out _, out _, out _, out _);
            GameObject actor = CreateConfiguredActor(catalog, out _, out _, out JumpController2D jump, out Rigidbody2D body, out CharacterArtRenderer2D artRenderer, out CharacterGameplayAnimator2D animator);

            SpriteRenderer sr = artRenderer.VisualRoot.GetComponentInChildren<SpriteRenderer>();

            // 1. Standing on ground floor (actor bottom at 0.4 - 0.9 = -0.5, touching GroundFloor top at -0.5)
            actor.transform.position = new Vector3(0f, 0.4f, 0f);
            Physics2D.SyncTransforms();
            jump.SetGrounded(true);
            body.linearVelocity = Vector2.zero;
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(animator.Pose, Is.EqualTo(CharacterVisualPose.Idle));
            Assert.That(sr.sprite, Is.SameAs(idle0));

            // 2. Rising Jump in mid-air
            actor.transform.position = new Vector3(0f, 3.0f, 0f);
            Physics2D.SyncTransforms();
            jump.SetGrounded(false);
            body.linearVelocity = new Vector2(0f, 4.5f);
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(animator.Pose, Is.EqualTo(CharacterVisualPose.Jump));
            Assert.That(sr.sprite, Is.SameAs(jumpRise), "Positive vertical velocity in air must select Jump state.");

            // 3. Airborne Apex (velocity.y == 0f while airborne / not grounded): MUST NOT flash Idle!
            actor.transform.position = new Vector3(0f, 4.5f, 0f);
            Physics2D.SyncTransforms();
            jump.SetGrounded(false);
            body.linearVelocity = Vector2.zero;
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(animator.Pose, Is.EqualTo(CharacterVisualPose.Jump), "Airborne apex (velocity == 0 while ungrounded) must remain Jump and never flash Idle.");
            Assert.That(sr.sprite, Is.SameAs(jumpRise), "Airborne apex must keep presenting Jump sprite without flashing Idle.");

            // 4. Descending Fall in mid-air (velocity.y < -0.15f while ungrounded)
            actor.transform.position = new Vector3(0f, 2.5f, 0f);
            Physics2D.SyncTransforms();
            jump.SetGrounded(false);
            body.linearVelocity = new Vector2(0f, -4.5f);
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(fall), "Negative vertical velocity while ungrounded must select Fall state.");

            // 5. Real Ground Contact on Landing
            actor.transform.position = new Vector3(0f, 0.4f, 0f);
            Physics2D.SyncTransforms();
            jump.SetGrounded(true);
            body.linearVelocity = Vector2.zero;
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(animator.Pose, Is.EqualTo(CharacterVisualPose.Idle));
            Assert.That(sr.sprite, Is.SameAs(idle0), "Touching real ground with zero vertical velocity must restore Idle state.");
        }

        [Test]
        public void PresentationPreservesCatalogWorldScaleAndNeverRotatesRootOnDeath()
        {
            CharacterSpriteSet2D catalog = CreateCatalog(out _, out _, out _, out _, out _, out _, out _, out Sprite death0, out Sprite death1);
            CreateConfiguredActor(catalog, out DamageReceiver2D receiver, out _, out _, out _, out CharacterArtRenderer2D artRenderer, out CharacterGameplayAnimator2D animator);

            SpriteRenderer sr = artRenderer.VisualRoot.GetComponentInChildren<SpriteRenderer>();
            var playback = artRenderer.VisualRoot.GetComponentInChildren<CharacterSpritePlayback2D>();

            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.transform.localScale, Is.EqualTo(new Vector3(1.75f, 1.75f, 1f)), "Sprite scale must match catalog WorldScale without bounds.size.y normalization.");

            // Inflict lethal damage via real combat API
            DamageReceiver2D demonAttacker = CreateDemonAttacker();
            AttackDefinition lethalHit = CreateAttackDefinition("lethal_hit", 250f, 0.05f, 0.05f, 0.05f);
            Assert.That(receiver.TryReceiveHit(lethalHit, demonAttacker, 1f), Is.True);
            Assert.That(receiver.IsAlive, Is.False);

            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(artRenderer.VisualRoot.localRotation, Is.EqualTo(Quaternion.identity), "Death must not tilt VisualRoot by 80 degrees.");
            Assert.That(sr.sprite, Is.SameAs(death0));

            playback.Tick(0.25f);
            InvokeNonPublic(animator, "LateUpdate");
            Assert.That(sr.sprite, Is.SameAs(death1), "Death clip must hold final frame without looping.");
        }

        [Test]
        public void SlicePlayerPresentationUsesSinglePlaybackOwnerInRealSlotAndRestoresPriorPresenterOnSwitchAndUnbind()
        {
            GameObject slotPrefab = Resources.Load<GameObject>("Characters/HuntrXPlayerSlot_Prototype");
            GameObject rumiPrefab = Resources.Load<GameObject>("Characters/Rumi_Prototype");
            GameObject miraPrefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            GameObject zoeyPrefab = Resources.Load<GameObject>("Characters/Zoey_Prototype");
            Assert.That(slotPrefab, Is.Not.Null);
            Assert.That(rumiPrefab, Is.Not.Null);
            Assert.That(miraPrefab, Is.Not.Null);
            Assert.That(zoeyPrefab, Is.Not.Null);

            GameObject slotGo = Object.Instantiate(slotPrefab);
            owned.Add(slotGo);
            var manager = slotGo.GetComponent<CharacterManager2D>();
            var switchController = slotGo.GetComponent<CharacterSwitchController2D>();
            var flow = slotGo.GetComponent<CheckpointAttemptFlow2D>();
            Transform stageStart = slotGo.transform.Find("StageStart");

            Assert.That(manager.Configure(rumiPrefab, flow, stageStart, "start-01"), Is.True);
            Assert.That(switchController.Configure(manager, new[] { rumiPrefab, miraPrefab, zoeyPrefab }, out string error), Is.True, error);
            Assert.That(manager.TrySpawn(), Is.True);

            DamageReceiver2D rumiActor = manager.ActiveCharacter;
            Assert.That(rumiActor, Is.Not.Null);

            // Attach CharacterArtRenderer2D + CharacterGameplayAnimator2D to Rumi actor to verify single-owner suppression & restoration
            var artDef = ScriptableObject.CreateInstance<CharacterArtDefinition>();
            owned.Add(artDef);
            SetBasePrivate(artDef, "id", "rumi_art_slot");
            SetPrivate(artDef, "characterName", "Rumi");
            SetPrivate(artDef, "height", 1.8f);
            SetPrivate(artDef, "width", 0.55f);

            var rigDef = ScriptableObject.CreateInstance<CharacterRigDefinition>();
            owned.Add(rigDef);
            SetBasePrivate(rigDef, "id", "rumi_rig_slot");
            SetPrivate(rigDef, "character", artDef);
            SetPrivate(rigDef, "idleCycleSeconds", 0.6f);
            SetPrivate(rigDef, "runCycleSeconds", 0.4f);
            SetPrivate(rigDef, "hurtSeconds", 0.2f);
            SetPrivate(rigDef, "specialSeconds", 0.5f);

            var rumiArtRenderer = rumiActor.gameObject.AddComponent<CharacterArtRenderer2D>();
            Assert.That(rumiArtRenderer.TryConfigure(artDef), Is.True);
            var rumiAnimator = rumiActor.gameObject.AddComponent<CharacterGameplayAnimator2D>();
            Assert.That(rumiAnimator.TryConfigure(rumiArtRenderer, rigDef), Is.True);

            var camGo = new GameObject("FollowCam");
            owned.Add(camGo);
            var cam = camGo.AddComponent<Camera>();

            CharacterSpriteSet2D catalog = CreateCatalog(out Sprite idle0, out Sprite run0, out _, out _, out _, out _, out _, out _, out _);

            var presGo = new GameObject("SlicePresentationHost");
            owned.Add(presGo);
            presGo.SetActive(false);
            var presentation = presGo.AddComponent<SlicePlayerPresentation2D>();
            SetPrivate(presentation, "characterSpriteSets", new[] { catalog });
            presGo.SetActive(true);
            Assert.That(presentation.TryBind(manager, cam), Is.True);

            // 1. Automatically bound assigned CharacterSpriteSet2D & CharacterSpritePlayback2D in real slot on TryBind!
            var playback = rumiActor.GetComponentInChildren<CharacterSpritePlayback2D>();
            Assert.That(playback, Is.Not.Null, "SlicePlayerPresentation2D must automatically bind CharacterSpritePlayback2D when bound to a real character slot.");
            Assert.That(playback.CurrentState, Is.EqualTo("Idle"));
            Assert.That(playback.GetComponent<SpriteRenderer>().sprite, Is.SameAs(idle0), "SlicePlayerPresentation2D must automatically present Idle from the assigned CharacterSpriteSet2D.");

            Vector3 boundScale = playback.transform.localScale;
            Assert.That(presentation.TryBindSpriteSet(CreateCatalogWithoutIdle()), Is.False);
            Assert.That(playback.transform.localScale, Is.EqualTo(boundScale), "Rejected catalog must preserve the active visual scale.");
            Assert.That(playback.GetComponent<SpriteRenderer>().sprite, Is.SameAs(idle0));

            // 2. Single presentation owner: Rumi's duplicate animator/artRenderer must be suppressed and only ONE character SpriteRenderer active on the actor
            Assert.That(rumiAnimator.enabled, Is.False, "Duplicate CharacterGameplayAnimator2D must be disabled while SlicePlayerPresentation2D owns the actor.");
            Assert.That(rumiArtRenderer.enabled, Is.False, "Duplicate CharacterArtRenderer2D must be disabled while SlicePlayerPresentation2D owns the actor.");

            SpriteRenderer[] allRenderersOnActor = rumiActor.GetComponentsInChildren<SpriteRenderer>(false);
            int enabledCharacterRenderers = 0;
            for (int i = 0; i < allRenderersOnActor.Length; i++)
            {
                if (allRenderersOnActor[i].enabled && allRenderersOnActor[i].gameObject.name != "Capcom_WeaponSlash_VFX")
                {
                    enabledCharacterRenderers++;
                }
            }
            Assert.That(enabledCharacterRenderers, Is.EqualTo(1), "Exactly one active character SpriteRenderer must be enabled on the actor.");

            // 3. Movement playback verification without procedural squash/tilt
            var body = rumiActor.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(3.5f, 0f);
            InvokeNonPublic(presentation, "LateUpdate");

            Transform visualTransform = playback.transform;
            Assert.That(visualTransform.localRotation, Is.EqualTo(Quaternion.identity), "Running must not tilt the sprite transform.");
            Assert.That(visualTransform.localScale, Is.EqualTo(new Vector3(1.75f, 1.75f, 1f)), "Running must preserve catalog WorldScale without procedural stretchX/stretchY.");
            Assert.That(playback.GetComponent<SpriteRenderer>().sprite, Is.SameAs(run0));

            body.linearVelocity = Vector2.zero;

            // 4. Disable SlicePlayerPresentation2D (Unbind/Stop): must restore Rumi's prior presenter state (enabled == true)
            presentation.enabled = false;
            Assert.That(rumiAnimator.enabled, Is.True, "Unbinding/disabling SlicePlayerPresentation2D must restore prior CharacterGameplayAnimator2D.enabled state.");
            Assert.That(rumiArtRenderer.enabled, Is.True, "Unbinding/disabling SlicePlayerPresentation2D must restore prior CharacterArtRenderer2D.enabled state.");
            Assert.That(rumiArtRenderer.VisualRoot.gameObject.activeSelf, Is.True, "Unbinding/disabling must restore VisualRoot active state.");

            // 5. Verify that if animator.enabled was false BEFORE binding, unbinding restores false (not blindly true)
            rumiAnimator.enabled = false;
            presentation.enabled = true;
            Assert.That(rumiAnimator.enabled, Is.False);

            // Switch from Rumi to Mira via CharacterSwitchController2D -> Rumi's prior state (animator=false, artRenderer=true) must be restored!
            Assert.That(switchController.TrySwitchNext(out string switchErr), Is.True, switchErr);
            Assert.That(rumiAnimator.enabled, Is.False, "Switching away from Rumi must restore her exact prior animator.enabled state (false).");
            Assert.That(rumiArtRenderer.enabled, Is.True, "Switching away from Rumi must restore her exact prior artRenderer.enabled state (true).");
        }
    }
}
