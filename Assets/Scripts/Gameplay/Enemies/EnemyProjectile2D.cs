using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Bounded swept-ray projectile, preserving the existing contact damage pipeline.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyProjectile2D : MonoBehaviour
    {
        private readonly RaycastHit2D[] contacts = new RaycastHit2D[32];
        private AttackDefinition attack;
        private DamageReceiver2D owner;
        private Vector2 direction;
        private float speed, life, remainingRange;
        private LayerMask collisionMask;
        private bool launched;
        private bool completed;
        private Sprite sprite;
        public bool IsFlying => launched && isActiveAndEnabled;
        public event Action<CombatImpactEvent> ImpactOccurred;
        private void Awake()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f, 1f);
            renderer.sprite = sprite;
            renderer.color = new Color(0.85f, 0.4f, 1f);
            transform.localScale = Vector3.one * 0.18f;
        }
        public bool TryLaunch(AttackDefinition value, DamageReceiver2D source, Vector2 origin, Vector2 aim,
            float projectileSpeed, float lifetime, float range, LayerMask mask)
        {
            if (!isActiveAndEnabled || launched || completed || value == null || !value.IsValid(out _) || source == null ||
                !source.isActiveAndEnabled || !source.IsAlive || !Finite(origin.x) || !Finite(origin.y) ||
                !Finite(aim.x) || !Finite(aim.y) || !Positive(projectileSpeed) || !Positive(lifetime) || !Positive(range)) return false;
            float scale = Mathf.Max(Mathf.Abs(aim.x), Mathf.Abs(aim.y));
            if (scale <= 0f) return false;
            direction = (aim / scale).normalized;
            attack = value; owner = source; speed = projectileSpeed; life = lifetime; remainingRange = range;
            collisionMask = mask; transform.position = origin; launched = true; return true;
        }
        private void FixedUpdate()
        {
            if (!launched) return;
            if (owner == null || !owner.isActiveAndEnabled || !owner.IsAlive) { Cancel(); return; }
            float dt = Mathf.Min(Time.fixedDeltaTime, life);
            if (dt <= 0f || remainingRange <= 0f) { Cancel(); return; }
            life = Mathf.Max(0f, life - dt);
            float distance = Mathf.Min(speed * dt, remainingRange);
            Vector2 origin = transform.position;
            ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(collisionMask); filter.useTriggers = true;
            int count = Physics2D.Raycast(origin, direction, filter, contacts, distance);
            if (count == contacts.Length) { Cancel(); return; }
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = contacts[i].collider;
                if (collider == null || collider.transform.root == owner.transform.root) continue;
                Hurtbox2D hurtbox = collider.GetComponentInParent<Hurtbox2D>();
                if (hurtbox != null && hurtbox.isActiveAndEnabled && hurtbox.Receiver != null &&
                    hurtbox.Receiver.isActiveAndEnabled && hurtbox.Receiver.IsAlive)
                {
                    launched = false; completed = true; // Reserve consumption before damage callbacks.
                    DamageReceiver2D impactOwner = owner;
                    AttackDefinition impactAttack = attack;
                    CombatContactResult result = hurtbox.ResolveHit(impactAttack, impactOwner, direction.x < 0f ? -1f : 1f, 0,
                        out DamageReceiver2D receiver, out _);
                    if (result == CombatContactResult.Damaged) PublishImpact(new CombatImpactEvent(impactOwner, receiver, impactAttack, 0));
                    Cancel(); return;
                }
                if (!collider.isTrigger) { Cancel(); return; }
            }
            transform.position = origin + direction * distance;
            remainingRange = Mathf.Max(0f, remainingRange - distance);
            if (life <= 0f || remainingRange <= 0f) Cancel();
        }
        public void Cancel() { completed = true; launched = false; gameObject.SetActive(false); Destroy(gameObject); }
        private void OnDisable() { launched = false; owner = null; attack = null; }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); }
        private void PublishImpact(CombatImpactEvent value)
        {
            Action<CombatImpactEvent> handlers = ImpactOccurred;
            if (handlers == null) return;
            foreach (Action<CombatImpactEvent> handler in handlers.GetInvocationList()) { try { handler(value); } catch (Exception e) { Debug.LogException(e, this); } }
        }
        private static bool Positive(float value) => Finite(value) && value > 0f;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
