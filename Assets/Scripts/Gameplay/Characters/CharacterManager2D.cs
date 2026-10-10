using System;
using System.Collections.Generic;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.Movement;
using HuntrX.Gameplay.Protection;
using UnityEngine;

namespace HuntrX.Gameplay.Characters
{
    /// <summary>Owns the runtime actors of one local player slot; no singleton or network ownership.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterManager2D : MonoBehaviour
    {
        private readonly Dictionary<GameObject, DamageReceiver2D> actors = new Dictionary<GameObject, DamageReceiver2D>();
        private GameObject initialPrefab;
        private CheckpointAttemptFlow2D flow;
        private Transform stageStart;
        private bool configured;
        private bool transitioning;
        private bool subscribed;
        private RespawnRequest pendingRespawn;
        private bool hasPendingRespawn;
        private bool deferredRespawn;
        private int lastConsumedAttempt;

        public DamageReceiver2D ActiveCharacter { get; private set; }
        public GameObject ActivePrefab { get; private set; }
        public CharacterManagerState2D State { get; private set; } = CharacterManagerState2D.Uninitialized;
        public event Action<DamageReceiver2D, DamageReceiver2D> ActiveCharacterChanged;

        public bool Configure(GameObject prefab, CheckpointAttemptFlow2D attemptFlow, Transform start, string startId)
        {
            if (!isActiveAndEnabled || transitioning || ActiveCharacter != null ||
                !ValidPrefab(prefab) || attemptFlow == null || !attemptFlow.isActiveAndEnabled ||
                start == null || !start.gameObject.activeInHierarchy || !ValidPose(start.position, start.rotation) ||
                string.IsNullOrWhiteSpace(startId) || !attemptFlow.ConfigureStart(startId, start)) return false;
            Unsubscribe();
            initialPrefab = prefab;
            flow = attemptFlow;
            stageStart = start;
            configured = true;
            ActivePrefab = null;
            hasPendingRespawn = false;
            deferredRespawn = false;
            lastConsumedAttempt = 0;
            State = CharacterManagerState2D.Uninitialized;
            Subscribe();
            return true;
        }

        public bool TrySpawn()
        {
            if (!isActiveAndEnabled || transitioning || !configured || flow == null || !flow.isActiveAndEnabled ||
                State == CharacterManagerState2D.Alive ||
                (!hasPendingRespawn && (stageStart == null || !stageStart.gameObject.activeInHierarchy)) ||
                (hasPendingRespawn && pendingRespawn.AttemptNumber != flow.AttemptNumber)) return false;
            Vector3 position = hasPendingRespawn ? pendingRespawn.Position : stageStart.position;
            Quaternion rotation = hasPendingRespawn ? pendingRespawn.Rotation : stageStart.rotation;
            GameObject prefab = hasPendingRespawn && ActivePrefab != null ? ActivePrefab : initialPrefab;
            if (!ValidPose(position, rotation) || !ValidPrefab(prefab)) return false;
            transitioning = true;
            DamageReceiver2D replacement = null;
            try
            {
                replacement = CreateActor(prefab, position, rotation);
                if (replacement == null || !flow.BindActor(replacement) || !flow.StartAttempt())
                {
                    if (replacement != null) DestroyActor(replacement);
                    flow.CancelAttempt();
                    return false;
                }
                DamageReceiver2D previous = ActiveCharacter;
                DestroyCache();
                actors.Add(prefab, replacement);
                ActiveCharacter = replacement;
                ActivePrefab = prefab;
                if (hasPendingRespawn) lastConsumedAttempt = pendingRespawn.AttemptNumber;
                hasPendingRespawn = false;
                deferredRespawn = false;
                State = CharacterManagerState2D.Alive;
                PublishChanged(previous, replacement);
                return true;
            }
            finally { transitioning = false; }
        }

        /// <summary>Switches between cached actors, preserving individual health and the current attempt.</summary>
        public bool TrySwitch(GameObject prefab)
        {
            if (!isActiveAndEnabled || transitioning || State != CharacterManagerState2D.Alive ||
                ActiveCharacter == null || !ValidRuntimeActor(ActiveCharacter) ||
                flow == null || !flow.isActiveAndEnabled || !flow.IsAttemptInProgress ||
                flow.BoundActor != ActiveCharacter || !ValidPrefab(prefab) || Busy(ActiveCharacter)) return false;
            if (prefab == ActivePrefab) return true;
            DamageReceiver2D previous = ActiveCharacter;
            Vector3 position = previous.transform.position;
            Quaternion rotation = previous.transform.rotation;
            Rigidbody2D previousBody = previous.GetComponent<Rigidbody2D>();
            Vector2 velocity = previousBody.linearVelocity;
            float angularVelocity = previousBody.angularVelocity;
            if (!ValidPose(position, rotation) || !Finite(velocity.x) || !Finite(velocity.y) || !Finite(angularVelocity)) return false;
            transitioning = true;
            bool created = false;
            DamageReceiver2D next = null;
            try
            {
                if (!actors.TryGetValue(prefab, out next) || next == null)
                {
                    next = CreateActor(prefab, position, rotation);
                    created = true;
                }
                if (next == null || !next.IsConfigurationValid || !next.IsAlive)
                {
                    if (created && next != null) DestroyActor(next);
                    return false;
                }
                // Keep each actor a scene root: existing combat/protection ownership uses transform.root.
                ResetInput(next);
                next.transform.SetPositionAndRotation(position, rotation);
                next.gameObject.SetActive(true);
                if (!ValidRuntimeActor(next))
                {
                    if (created) DestroyActor(next); else next.gameObject.SetActive(false);
                    return false;
                }
                Rigidbody2D body = next.GetComponent<Rigidbody2D>();
                body.linearVelocity = velocity;
                body.angularVelocity = angularVelocity;
                if (!flow.BindActor(next))
                {
                    if (created) DestroyActor(next); else next.gameObject.SetActive(false);
                    return false;
                }
                ResetInput(previous);
                previous.gameObject.SetActive(false);
                if (created) actors[prefab] = next;
                ActiveCharacter = next;
                ActivePrefab = prefab;
                PublishChanged(previous, next);
                return true;
            }
            finally { transitioning = false; }
        }

        private void HandleRespawn(RespawnRequest request)
        {
            // A presentation subscriber may cause real damage while a transition event is being published.
            // Capture that death instead of discarding it; deferred recovery prevents recursive respawn chains.
            if (!isActiveAndEnabled || State != CharacterManagerState2D.Alive ||
                ActiveCharacter == null || ActiveCharacter.IsAlive || request.AttemptNumber <= lastConsumedAttempt ||
                flow == null || flow.BoundActor != ActiveCharacter || request.AttemptNumber != flow.AttemptNumber ||
                string.IsNullOrWhiteSpace(request.CheckpointId) || !ValidPose(request.Position, request.Rotation)) return;
            deferredRespawn = transitioning;
            pendingRespawn = request;
            hasPendingRespawn = true;
            State = CharacterManagerState2D.Respawning;
            foreach (DamageReceiver2D actor in actors.Values)
            {
                if (actor == null) continue;
                ResetInput(actor);
                actor.gameObject.SetActive(false);
            }
            if (!transitioning && !TrySpawn())
                Debug.LogError("CharacterManager2D could not recover the ended attempt; TrySpawn may retry.", this);
        }

        private void LateUpdate()
        {
            // One deferred attempt per frame only for a death captured while publishing a transition.
            if (deferredRespawn && !transitioning && hasPendingRespawn && State == CharacterManagerState2D.Respawning)
            {
                deferredRespawn = false;
                if (!TrySpawn()) Debug.LogError("CharacterManager2D deferred recovery failed; TrySpawn may retry.", this);
            }
        }

        private void OnEnable()
        {
            State = CharacterManagerState2D.Uninitialized;
            Subscribe();
        }
        private void OnDisable()
        {
            Unsubscribe();
            if (configured && flow != null) flow.CancelAttempt();
            DamageReceiver2D previous = ActiveCharacter;
            DestroyCache();
            ActiveCharacter = null;
            ActivePrefab = null;
            hasPendingRespawn = false;
            deferredRespawn = false;
            State = CharacterManagerState2D.Disabled;
            PublishChanged(previous, null);
        }
        private void Subscribe()
        {
            if (subscribed || !configured || flow == null || !isActiveAndEnabled) return;
            flow.RespawnRequested += HandleRespawn;
            subscribed = true;
        }
        private void Unsubscribe()
        {
            if (subscribed && flow != null) flow.RespawnRequested -= HandleRespawn;
            subscribed = false;
        }
        private void DestroyCache()
        {
            foreach (DamageReceiver2D actor in actors.Values) if (actor != null) DestroyActor(actor);
            actors.Clear();
        }
        private static void DestroyActor(DamageReceiver2D actor)
        {
            ResetInput(actor);
            actor.gameObject.SetActive(false);
            Destroy(actor.gameObject);
        }
        private static DamageReceiver2D CreateActor(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.SetActive(true);
            DamageReceiver2D actor = instance.GetComponent<DamageReceiver2D>();
            if (actor == null || !ValidRuntimeActor(actor))
            {
                instance.SetActive(false);
                Destroy(instance);
                return null;
            }
            ResetInput(actor);
            Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            return actor;
        }
        private static bool ValidRuntimeActor(DamageReceiver2D actor)
        {
            if (!actor.isActiveAndEnabled || !actor.IsConfigurationValid || !actor.IsAlive ||
                actor.transform != actor.transform.root) return false;
            Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
            if (body == null || body.bodyType != RigidbodyType2D.Dynamic) return false;
            AttackHitbox2D hitbox = actor.GetComponent<AttackHitbox2D>();
            Hurtbox2D hurtbox = actor.GetComponent<Hurtbox2D>();
            HorizontalMovement2D movement = actor.GetComponent<HorizontalMovement2D>();
            JumpController2D jump = actor.GetComponent<JumpController2D>();
            DashController2D dash = actor.GetComponent<DashController2D>();
            AttackController2D attack = actor.GetComponent<AttackController2D>();
            ParryController2D parry = actor.GetComponent<ParryController2D>();
            CharacterDefinitionApplier2D applier = actor.GetComponent<CharacterDefinitionApplier2D>();
            if (hitbox == null || !hitbox.isActiveAndEnabled || !hitbox.IsConfigurationValid ||
                hurtbox == null || !hurtbox.isActiveAndEnabled || hurtbox.Receiver != actor ||
                movement == null || !movement.isActiveAndEnabled || jump == null || !jump.isActiveAndEnabled ||
                dash == null || !dash.isActiveAndEnabled || attack == null || !attack.isActiveAndEnabled ||
                parry == null || !parry.isActiveAndEnabled || applier == null || !applier.isActiveAndEnabled ||
                applier.Definition == null || !applier.Definition.IsValid(out _)) return false;
            ZoeyRangedAttack2D ranged = actor.GetComponent<ZoeyRangedAttack2D>();
            MiraProtectionField2D field = actor.GetComponent<MiraProtectionField2D>();
            return (ranged == null || (ranged.isActiveAndEnabled && ranged.IsConfigurationValid)) &&
                (field == null || (field.isActiveAndEnabled && field.Definition != null && field.Definition.IsValid(out _)));
        }

        private static bool ValidPrefab(GameObject prefab)
        {
            if (prefab == null || prefab.scene.IsValid()) return false;
            DamageReceiver2D receiver = prefab.GetComponent<DamageReceiver2D>();
            CharacterDefinitionApplier2D applier = prefab.GetComponent<CharacterDefinitionApplier2D>();
            return receiver != null && Finite(receiver.MaximumHealth) && receiver.MaximumHealth > 0f &&
                prefab.GetComponent<Rigidbody2D>() != null && applier != null && applier.Definition != null &&
                applier.Definition.IsValid(out _) && prefab.GetComponent<HorizontalMovement2D>() != null &&
                prefab.GetComponent<JumpController2D>() != null && prefab.GetComponent<DashController2D>() != null &&
                prefab.GetComponent<AttackController2D>() != null && prefab.GetComponent<ParryController2D>() != null;
        }
        private static bool Busy(DamageReceiver2D actor)
        {
            DashController2D dash = actor.GetComponent<DashController2D>();
            AttackController2D attack = actor.GetComponent<AttackController2D>();
            ParryController2D parry = actor.GetComponent<ParryController2D>();
            MiraProtectionField2D field = actor.GetComponent<MiraProtectionField2D>();
            ZoeyRangedAttack2D ranged = actor.GetComponent<ZoeyRangedAttack2D>();
            return (dash != null && dash.IsDashing) || (attack != null && attack.State != AttackState2D.Idle) ||
                (parry != null && parry.IsWindowActive) || (field != null && field.IsActive) ||
                (ranged != null && ranged.CooldownRemaining > 0f);
        }
        private static void ResetInput(DamageReceiver2D actor)
        {
            HorizontalMovement2D movement = actor.GetComponent<HorizontalMovement2D>();
            if (movement != null) movement.SetMovementInput(Vector2.zero);
            JumpController2D jump = actor.GetComponent<JumpController2D>();
            if (jump != null)
            {
                jump.SetJumpSuppressed(true);
                jump.SetGrounded(false);
                jump.SetWallContact(false, Vector2.zero);
                jump.SetJumpSuppressed(false);
            }
            DashController2D dash = actor.GetComponent<DashController2D>();
            if (dash != null) dash.SetGrounded(false);
        }
        private void PublishChanged(DamageReceiver2D previous, DamageReceiver2D current)
        {
            Action<DamageReceiver2D, DamageReceiver2D> handlers = ActiveCharacterChanged;
            if (handlers == null) return;
            foreach (Action<DamageReceiver2D, DamageReceiver2D> handler in handlers.GetInvocationList())
            {
                try { handler(previous, current); }
                catch (Exception exception) { Debug.LogException(exception, this); }
            }
        }
        private static bool ValidPose(Vector3 position, Quaternion rotation) => Finite(position.x) &&
            Finite(position.y) && Finite(position.z) && Finite(rotation.x) && Finite(rotation.y) &&
            Finite(rotation.z) && Finite(rotation.w) &&
            (rotation.x != 0f || rotation.y != 0f || rotation.z != 0f || rotation.w != 0f);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
