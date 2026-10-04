using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Protection
{
    /// <summary>Owns one temporary field, its scaled lifetime, and its receiver registrations.</summary>
    [DisallowMultipleComponent]
    public sealed class MiraProtectionField2D : MonoBehaviour
    {
        [SerializeField] private MiraProtectionDefinition definition;
        [SerializeField] private CircleCollider2D fieldCollider;

        private readonly List<DamageProtection2D> members = new List<DamageProtection2D>();
        private readonly List<Collider2D> overlaps = new List<Collider2D>();
        private float remainingDuration;

        public MiraProtectionDefinition Definition => definition;
        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (fieldCollider != null)
            {
                fieldCollider.enabled = false;
            }
        }

        public bool TryActivate()
        {
            DamageReceiver2D owner = GetComponent<DamageReceiver2D>();
            if (IsActive || !isActiveAndEnabled || definition == null || !definition.IsValid(out _) ||
                owner == null || !owner.isActiveAndEnabled || !owner.IsConfigurationValid || !owner.IsAlive ||
                transform != transform.root || fieldCollider == null || !fieldCollider.isTrigger ||
                !fieldCollider.gameObject.activeInHierarchy || fieldCollider.transform == transform ||
                !fieldCollider.transform.IsChildOf(transform) || fieldCollider.offset != Vector2.zero ||
                fieldCollider.transform.localPosition != Vector3.zero ||
                fieldCollider.transform.position != transform.position ||
                fieldCollider.attachedRigidbody != owner.GetComponent<Rigidbody2D>())
            {
                return false;
            }

            fieldCollider.radius = definition.Radius;
            remainingDuration = definition.DurationSeconds;
            IsActive = true;
            fieldCollider.enabled = true;

            // One synchronous query includes the owner's own colliders, unlike Collider2D.OverlapCollider.
            Physics2D.SyncTransforms();
            overlaps.Clear();
            float worldRadius = fieldCollider.radius * Mathf.Max(
                Mathf.Abs(fieldCollider.transform.lossyScale.x), Mathf.Abs(fieldCollider.transform.lossyScale.y));
            Physics2D.OverlapCircle(fieldCollider.transform.TransformPoint(fieldCollider.offset), worldRadius,
                ContactFilter2D.noFilter, overlaps);
            foreach (Collider2D overlap in overlaps)
            {
                RegisterOverlap(overlap);
            }
            overlaps.Clear();
            return true;
        }

        internal bool ContainsOverlap(Collider2D overlap)
        {
            if (!IsActive || !isActiveAndEnabled || fieldCollider == null || !fieldCollider.enabled ||
                !fieldCollider.gameObject.activeInHierarchy || overlap == null || overlap == fieldCollider ||
                !overlap.enabled || !overlap.gameObject.activeInHierarchy)
            {
                return false;
            }

            Rigidbody2D fieldBody = fieldCollider.attachedRigidbody;
            Rigidbody2D targetBody = overlap.attachedRigidbody;
            if (fieldBody == null || targetBody == null)
            {
                return false;
            }

            // The posed overload uses attached-body poses and retains authored collider offsets.
            // Read current Transforms so pre-physics movement is immediate, without a global transform sync.
            ColliderDistance2D distance = fieldCollider.Distance(fieldBody.transform.position,
                fieldBody.transform.eulerAngles.z, overlap, targetBody.transform.position,
                targetBody.transform.eulerAngles.z);
            return distance.isValid && distance.isOverlapped;
        }

        internal void Track(DamageProtection2D receiver)
        {
            if (!members.Contains(receiver))
            {
                members.Add(receiver);
            }
        }

        internal void Untrack(DamageProtection2D receiver) => members.Remove(receiver);

        private void RegisterOverlap(Collider2D overlap)
        {
            if (!IsActive || overlap == null || overlap == fieldCollider || !overlap.enabled ||
                !overlap.gameObject.activeInHierarchy)
            {
                return;
            }

            DamageProtection2D protection = overlap.GetComponentInParent<DamageProtection2D>();
            DamageReceiver2D receiver = protection != null ? protection.GetComponent<DamageReceiver2D>() : null;
            if (receiver != null && receiver.isActiveAndEnabled && receiver.IsConfigurationValid && receiver.IsAlive)
            {
                protection.Register(this, overlap);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => UpdateOverlap(other);
        private void OnTriggerStay2D(Collider2D other) => UpdateOverlap(other);

        private void UpdateOverlap(Collider2D other)
        {
            // Root callbacks can also come from the owner's hurtboxes; only this circle grants protection.
            if (ContainsOverlap(other))
            {
                RegisterOverlap(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsActive || other == null || ContainsOverlap(other))
            {
                return;
            }

            DamageProtection2D protection = other.GetComponentInParent<DamageProtection2D>();
            if (protection != null)
            {
                protection.Unregister(this, other);
            }
        }

        private void FixedUpdate()
        {
            if (!IsActive)
            {
                return;
            }

            remainingDuration -= Time.fixedDeltaTime;
            if (remainingDuration <= 0f)
            {
                EndField();
            }
        }

        private void EndField()
        {
            IsActive = false;
            remainingDuration = 0f;
            if (fieldCollider != null)
            {
                fieldCollider.enabled = false;
            }

            while (members.Count > 0)
            {
                DamageProtection2D receiver = members[members.Count - 1];
                members.RemoveAt(members.Count - 1);
                if (receiver != null)
                {
                    receiver.UnregisterSource(this);
                }
            }
        }

        private void OnDisable() => EndField();
        private void OnDestroy() => EndField();
    }
}
