using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Pooling;
using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Bounded swept-ray projectile, preserving the existing contact damage pipeline.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyProjectile2D : MonoBehaviour, IPooledLifecycle
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
        private PooledInstance poolLease;
        private long generation;
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
            if (scale <= 0f) { aim = Vector2.down; scale = 1f; }
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
                    long impactGeneration=generation;
                    launched = false; completed = true; // Reserve consumption before damage callbacks.
                    DamageReceiver2D impactOwner = owner;
                    AttackDefinition impactAttack = attack;
                    CombatContactResult result = hurtbox.ResolveHit(impactAttack, impactOwner, direction.x < 0f ? -1f : 1f, 0,
                        out DamageReceiver2D receiver, out _);
                    if (result == CombatContactResult.Damaged && generation == impactGeneration)
                        PublishImpact(new CombatImpactEvent(impactOwner, receiver, impactAttack, 0));
                    if(generation==impactGeneration)Cancel(); return;
                }
                if (!collider.isTrigger) { Cancel(); return; }
            }
            transform.position = origin + direction * distance;
            remainingRange = Mathf.Max(0f, remainingRange - distance);
            if (life <= 0f || remainingRange <= 0f) Cancel();
        }
        public void Cancel()
        {
            completed = true; launched = false;
            if(poolLease!=null&&poolLease.IsPooled){poolLease.TryReturn();return;}
            gameObject.SetActive(false);Destroy(gameObject);
        }
        public void PrepareForRent()
        {
            if(generation<long.MaxValue)generation++;
            poolLease=GetComponent<PooledInstance>();completed=false;launched=false;
            owner=null;attack=null;direction=Vector2.zero;speed=life=remainingRange=0f;
        }
        public void PrepareForReturn()
        {
            completed=true;launched=false;owner=null;attack=null;ImpactOccurred=null;
        }
        private void OnDisable() { launched = false; owner = null; attack = null; }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); }
        private void PublishImpact(CombatImpactEvent value)
        {
            Action<CombatImpactEvent> handlers = ImpactOccurred;
            if (handlers == null) return;
            long impactGeneration=generation;
            foreach (Action<CombatImpactEvent> handler in handlers.GetInvocationList())
            {
                // A callback may return and rent this object for another shot. Retire the old dispatch.
                if(generation!=impactGeneration)return;
                try { handler(value); } catch (Exception e) { Debug.LogException(e, this); }
            }
        }
        private static bool Positive(float value) => Finite(value) && value > 0f;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
