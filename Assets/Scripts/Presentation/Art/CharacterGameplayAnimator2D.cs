using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Jump;
using UnityEngine;

namespace HuntrX.Presentation.Art
{
    /// <summary>Presentation only. State reads never authorize hits, movement or invulnerability.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterGameplayAnimator2D : MonoBehaviour
    {
        private static readonly RaycastHit2D[] GroundHits = new RaycastHit2D[16];

        [SerializeField] private CharacterArtRenderer2D renderer2D;
        [SerializeField] private CharacterRigDefinition rig;

        private DamageReceiver2D receiver;
        private AttackController2D attack;
        private DashController2D dash;
        private JumpController2D jump;
        private ParryController2D parry;
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private ZoeyRangedAttack2D ranged;
        private EnemyAgent2D enemy;
        private Transform cachedRoot;
        private Vector3 rootPosition;
        private float hurtRemaining;
        private float specialRemaining;
        private float rangedRemaining;
        private int facing = 1;
        private bool wasInAttack;
        private int lastAttackStepIndex = -1;
        private bool airborneLatched;

        public CharacterVisualPose Pose { get; private set; }

        public bool TryConfigure(CharacterArtRenderer2D visual, CharacterRigDefinition profile)
        {
            if (!isActiveAndEnabled || visual == null || profile == null || !profile.IsValid || visual.Definition != profile.Character)
                return false;
            ResetPose();
            renderer2D = visual;
            rig = profile;
            CacheParts();
            return true;
        }

        public void PlaySpecial()
        {
            if (isActiveAndEnabled && rig != null && rig.IsValid)
                specialRemaining = rig.SpecialSeconds;
        }

        private void Awake()
        {
            receiver = GetComponent<DamageReceiver2D>();
            attack = GetComponent<AttackController2D>();
            dash = GetComponent<DashController2D>();
            jump = GetComponent<JumpController2D>();
            parry = GetComponent<ParryController2D>();
            body = GetComponent<Rigidbody2D>();
            bodyCollider = FindRootBodyCollider(transform);
            ranged = GetComponent<ZoeyRangedAttack2D>();
            enemy = GetComponent<EnemyAgent2D>();
        }

        private void OnEnable()
        {
            if (receiver != null) receiver.DamageReceived += OnHurt;
            if (ranged != null) ranged.ShotFired += OnShot;
        }

        private void OnDisable()
        {
            if (receiver != null) receiver.DamageReceived -= OnHurt;
            if (ranged != null) ranged.ShotFired -= OnShot;
            ResetPose();
            rangedRemaining = 0f;
            hurtRemaining = 0f;
            specialRemaining = 0f;
            wasInAttack = false;
            lastAttackStepIndex = -1;
            airborneLatched = false;
        }

        private void OnShot(Vector2 from, Vector2 to)
        {
            rangedRemaining = 0.3f;
        }

        private void OnHurt(CombatImpactEvent impact)
        {
            if (rig != null && rig.IsValid)
                hurtRemaining = rig.HurtSeconds;
        }

        private void CacheParts()
        {
            cachedRoot = renderer2D != null ? renderer2D.VisualRoot : null;
            if (cachedRoot != null)
                rootPosition = cachedRoot.localPosition;
        }

        private void LateUpdate()
        {
            if (renderer2D == null || rig == null || !rig.IsValid || renderer2D.Definition != rig.Character || renderer2D.VisualRoot == null)
                return;
            if (cachedRoot != renderer2D.VisualRoot)
                CacheParts();

            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            hurtRemaining = Mathf.Max(0f, hurtRemaining - dt);
            specialRemaining = Mathf.Max(0f, specialRemaining - dt);
            rangedRemaining = Mathf.Max(0f, rangedRemaining - dt);

            Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
            if (float.IsNaN(velocity.x) || float.IsInfinity(velocity.x) || float.IsNaN(velocity.y) || float.IsInfinity(velocity.y))
                velocity = Vector2.zero;

            if (attack != null && attack.State != AttackState2D.Idle)
                facing = attack.LastFacingDirection >= 0f ? 1 : -1;
            else if (Mathf.Abs(velocity.x) > 0.1f)
                facing = velocity.x < 0f ? -1 : 1;

            bool isGrounded = EvaluateGrounded(transform, bodyCollider, jump, dash, velocity, ref airborneLatched);
            bool isAirborne = !isGrounded;
            bool isAttacking = (attack != null && attack.State != AttackState2D.Idle) ||
                               rangedRemaining > 0f ||
                               (enemy != null && enemy.State == EnemyState2D.Attacking);

            if (receiver != null && !receiver.IsAlive)
                Pose = CharacterVisualPose.Death;
            else if (specialRemaining > 0f)
                Pose = CharacterVisualPose.Special;
            else if (hurtRemaining > 0f)
                Pose = CharacterVisualPose.Hurt;
            else if ((parry != null && parry.IsWindowActive) || (enemy != null && enemy.State == EnemyState2D.Telegraph))
                Pose = CharacterVisualPose.Parry;
            else if (dash != null && dash.IsDashing)
                Pose = CharacterVisualPose.Dash;
            else if (isAttacking)
                Pose = isAirborne ? CharacterVisualPose.Aerial : CharacterVisualPose.Attack;
            else if (isAirborne)
                Pose = CharacterVisualPose.Jump;
            else if (Mathf.Abs(velocity.x) > 0.1f)
                Pose = CharacterVisualPose.Run;
            else
                Pose = CharacterVisualPose.Idle;

            ResetRotations();
            cachedRoot.localPosition = rootPosition;
            cachedRoot.localScale = new Vector3(facing, 1f, 1f);
            cachedRoot.localRotation = Quaternion.identity;

            if (renderer2D.SpriteRenderer != null && renderer2D.SpriteSet != null)
            {
                float worldScale = renderer2D.SpriteSet.WorldScale;
                renderer2D.SpriteRenderer.transform.localPosition = FootLocalPosition(renderer2D.SpriteRenderer.transform, bodyCollider);
                renderer2D.SpriteRenderer.transform.localRotation = Quaternion.identity;
                renderer2D.SpriteRenderer.transform.localScale = new Vector3(worldScale, worldScale, 1f);
            }

            DrivePlayback(renderer2D.Playback, renderer2D.SpriteSet, Pose, velocity, isAirborne, attack, ref wasInAttack, ref lastAttackStepIndex);
        }

        internal static Vector3 FootLocalPosition(Transform visual, Collider2D collider)
        {
            if (visual == null || collider == null || !collider.enabled || collider.isTrigger)
                return Vector3.zero;
            Bounds bounds = collider.bounds;
            Vector3 foot = new Vector3(bounds.center.x, bounds.min.y, visual.position.z);
            return visual.parent != null ? visual.parent.InverseTransformPoint(foot) : foot;
        }
        internal static Collider2D FindRootBodyCollider(Transform actorTransform)
        {
            if (actorTransform == null) return null;
            Collider2D[] colliders = actorTransform.GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger)
                    return colliders[i];
            }
            return null;
        }

        internal static bool EvaluateGrounded(
            Transform actorTransform,
            Collider2D bodyCollider,
            JumpController2D jump,
            DashController2D dash,
            Vector2 velocity,
            ref bool airborneLatched)
        {
            if (Mathf.Abs(velocity.y) > 0.15f)
            {
                airborneLatched = true;
                return false;
            }

            bool hasPhysicsGround = HasGroundContact(actorTransform, bodyCollider);
            bool hasJumpComponent = jump != null;
            bool jumpGrounded = hasJumpComponent && jump.IsGrounded;
            bool hasDashComponent = dash != null;
            bool dashGrounded = hasDashComponent && dash.IsGrounded;

            if (hasPhysicsGround)
            {
                airborneLatched = false;
                return true;
            }

            if (!airborneLatched)
            {
                return true;
            }

            if (hasJumpComponent)
            {
                if (jumpGrounded)
                {
                    airborneLatched = false;
                    return true;
                }
                return false;
            }

            if (hasDashComponent)
            {
                if (dashGrounded)
                {
                    airborneLatched = false;
                    return true;
                }
                return false;
            }

            if (bodyCollider != null && bodyCollider.enabled && !bodyCollider.isTrigger)
            {
                return false;
            }

            airborneLatched = false;
            return true;
        }

        private static bool HasGroundContact(Transform actorTransform, Collider2D collider)
        {
            if (actorTransform == null || collider == null || !collider.enabled || collider.isTrigger)
                return false;

            Bounds bounds = collider.bounds;
            if (bounds.extents.y <= 0.001f)
                return false;

            var filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(~0);

            int count = Physics2D.Raycast(bounds.center, Vector2.down, filter, GroundHits, bounds.extents.y + 0.08f);
            Transform root = actorTransform.root;
            for (int i = 0; i < count; i++)
            {
                Collider2D hitCol = GroundHits[i].collider;
                if (hitCol == null || hitCol.transform.root == root)
                    continue;
                if (GroundHits[i].normal.y >= 0.5f)
                    return true;
            }
            return false;
        }

        internal static void DrivePlayback(
            CharacterSpritePlayback2D playback,
            CharacterSpriteSet2D spriteSet,
            CharacterVisualPose pose,
            Vector2 velocity,
            bool isAirborne,
            AttackController2D attack,
            ref bool wasInAttack,
            ref int lastAttackStepIndex)
        {
            if (playback == null || spriteSet == null)
                return;

            bool inAttackControllerState = (pose == CharacterVisualPose.Attack || pose == CharacterVisualPose.Aerial) &&
                                           attack != null &&
                                           attack.State != AttackState2D.Idle;

            if (inAttackControllerState)
            {
                int currentStep = attack.CurrentComboStepIndex;
                bool restart = !wasInAttack || currentStep != lastAttackStepIndex;
                wasInAttack = true;
                lastAttackStepIndex = currentStep;

                string attackStateId = CharacterArtRenderer2D.MapPoseToStateId(pose, spriteSet);
                SyncAttackPlaybackToController(playback, spriteSet, attackStateId, attack, restart);
                return;
            }

            wasInAttack = false;
            lastAttackStepIndex = -1;

            string desiredStateId;
            if (pose == CharacterVisualPose.Jump && isAirborne && velocity.y < -0.15f)
            {
                desiredStateId = spriteSet.TryGetClip("Fall", out _) ? "Fall" : "Jump";
            }
            else
            {
                desiredStateId = CharacterArtRenderer2D.MapPoseToStateId(pose, spriteSet);
            }

            playback.SetState(desiredStateId, false);
        }

        private static void SyncAttackPlaybackToController(
            CharacterSpritePlayback2D playback,
            CharacterSpriteSet2D spriteSet,
            string stateId,
            AttackController2D attack,
            bool restart)
        {
            if (!spriteSet.TryGetClip(stateId, out SpriteAnimationClip2D clip) || clip == null || clip.Duration <= 0f)
            {
                playback.SetState(stateId, restart);
                return;
            }

            float progress = GetNormalizedAttackProgress(attack);
            float maxSampleTime = Mathf.Max(0f, clip.Duration - 0.0001f);
            float targetClipTime = Mathf.Clamp(progress * clip.Duration, 0f, maxSampleTime);

            playback.SampleStateAt(stateId, targetClipTime);
        }

        private static float GetNormalizedAttackProgress(AttackController2D attack)
        {
            if (attack == null || attack.State == AttackState2D.Idle)
                return 0f;

            AttackDefinition def = attack.CurrentAttackDefinition;
            if (def == null)
                return 0f;

            float startup = Mathf.Max(0f, def.StartupDuration);
            float active = Mathf.Max(0f, def.ActiveDuration);
            float recovery = Mathf.Max(0f, def.RecoveryDuration);
            float total = startup + active + recovery;
            if (total <= 0.0001f)
                return 0f;

            float remaining = attack.RemainingPhaseDuration;
            float elapsedByPhase;
            switch (attack.State)
            {
                case AttackState2D.Startup:
                    elapsedByPhase = Mathf.Clamp(startup - remaining, 0f, startup);
                    break;
                case AttackState2D.Active:
                    elapsedByPhase = startup + Mathf.Clamp(active - remaining, 0f, active);
                    break;
                case AttackState2D.Recovery:
                    elapsedByPhase = startup + active + Mathf.Clamp(recovery - remaining, 0f, recovery);
                    break;
                default:
                    elapsedByPhase = 0f;
                    break;
            }

            return Mathf.Clamp01(elapsedByPhase / total);
        }

        private static void Rotate(Transform part, float degrees)
        {
            if (part != null) part.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }

        private void ResetRotations()
        {
            if (renderer2D == null) return;
            Rotate(renderer2D.LeftLeg, 0f);
            Rotate(renderer2D.RightLeg, 0f);
            Rotate(renderer2D.LeftArm, 0f);
            Rotate(renderer2D.RightArm, 0f);
            Rotate(renderer2D.Weapon, renderer2D.Definition != null && renderer2D.Definition.Weapon == CharacterWeaponShape.TwinStars ? 45f : 0f);
        }

        private void ResetPose()
        {
            if (cachedRoot != null)
            {
                cachedRoot.localPosition = rootPosition;
                cachedRoot.localScale = Vector3.one;
                cachedRoot.localRotation = Quaternion.identity;
            }
            ResetRotations();
        }
    }
}
