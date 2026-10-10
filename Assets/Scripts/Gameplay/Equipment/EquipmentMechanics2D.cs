using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using UnityEngine;
namespace HuntrX.Gameplay.Equipment
{
    [DisallowMultipleComponent,RequireComponent(typeof(DamageReceiver2D),typeof(Rigidbody2D))]
    public sealed class EquipmentMechanics2D : MonoBehaviour
    {
        [SerializeField] private EquipmentProgress2D progression;
        private EquipmentPieceDefinition piece;
        private DamageReceiver2D owner;
        private Rigidbody2D body;
        private DashController2D dash;
        private AttackController2D melee;
        private ParryController2D parry;
        private readonly RaycastHit2D[] rays=new RaycastHit2D[32];
        private readonly Collider2D[] contacts=new Collider2D[32];
        private readonly HashSet<DamageReceiver2D> hit=new HashSet<DamageReceiver2D>();
        private readonly HashSet<Collider2D> ground=new HashSet<Collider2D>();
        private bool landed,airStepUsed,usingAbility;
        private float cooldown;
        public EquipmentPieceDefinition Equipped=>piece;
        public EquipmentProgress2D Progression=>progression;
        public event Action<EquipmentPieceDefinition> AbilityUsed;
        public event Action<CombatImpactEvent> ImpactOccurred;
        private void Awake() { owner=GetComponent<DamageReceiver2D>(); body=GetComponent<Rigidbody2D>(); dash=GetComponent<DashController2D>(); melee=GetComponent<AttackController2D>(); parry=GetComponent<ParryController2D>(); }
        private void Update() => cooldown=Mathf.Max(0f,cooldown-Time.deltaTime);
        private void OnDisable() { ground.Clear(); landed=false; airStepUsed=false; }
        public bool TryBindProgression(EquipmentProgress2D value)
        {
            if(usingAbility || value==null) return false;
            progression=value; piece=null; return true;
        }
        public bool TryEquip(EquipmentPieceDefinition value)
        {
            if(usingAbility || value==null || !value.IsValid(out _) || progression==null || !progression.IsUnlocked(value)) return false;
            piece=value; return true;
        }
        private bool Ready()=>isActiveAndEnabled && owner!=null && owner.isActiveAndEnabled && owner.IsAlive && body!=null && body.bodyType==RigidbodyType2D.Dynamic && Finite(body.position.x) && Finite(body.position.y) && !usingAbility && cooldown<=0f && piece!=null && piece.IsValid(out _) && progression!=null && progression.isActiveAndEnabled && progression.IsUnlocked(piece) && (dash==null || !dash.IsDashing) && (melee==null || melee.State==AttackState2D.Idle) && (parry==null || !parry.IsWindowActive);
        public bool TryUseAbility()
        {
            if(!Ready()) return false;
            var usedPiece=piece;
            if(piece.Mechanic==EquipmentMechanic.AirStep)
            {
                if(!landed || ground.Count>0 || airStepUsed) return false;
                usingAbility=true;
                try { airStepUsed=true; cooldown=piece.Cooldown; body.linearVelocityY=piece.AirStepVelocity; PublishAbility(usedPiece); return true; }
                finally { usingAbility=false; }
            }
            ContactFilter2D filter=new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers=true;
            int count=Physics2D.OverlapCircle(body.position,piece.BurstRadius,filter,contacts);
            if(count==contacts.Length) return false;
            usingAbility=true; cooldown=piece.Cooldown; hit.Clear();
            try
            {
                for(int i=0;i<count;i++)
                {
                    if(!isActiveAndEnabled || !owner.isActiveAndEnabled || !owner.IsAlive) break;
                    var hurtbox=contacts[i]!=null?contacts[i].GetComponentInParent<Hurtbox2D>():null;
                    if(hurtbox==null || !hurtbox.isActiveAndEnabled || hurtbox.Receiver==null || !hurtbox.Receiver.isActiveAndEnabled || !hurtbox.Receiver.IsAlive || !hit.Add(hurtbox.Receiver) || !HasLineOfSight(hurtbox.Receiver.transform.position)) continue;
                    float facing=hurtbox.transform.position.x<transform.position.x?-1f:1f;
                    var result=hurtbox.ResolveHit(usedPiece.BurstAttack,owner,facing,0,out var receiver,out _);
                    if(result==CombatContactResult.Damaged) PublishImpact(new CombatImpactEvent(owner,receiver,usedPiece.BurstAttack,0));
                }
                PublishAbility(usedPiece); return true;
            }
            finally { usingAbility=false; hit.Clear(); }
        }
        private static bool Finite(float value)=>!float.IsNaN(value) && !float.IsInfinity(value);
        private bool HasLineOfSight(Vector2 destination)
        {
            if(!Finite(destination.x) || !Finite(destination.y)) return false;
            Vector2 offset=destination-body.position; ContactFilter2D filter=new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers=true;
            int count=Physics2D.Raycast(body.position,offset.normalized,filter,rays,offset.magnitude);
            if(count==rays.Length) return false;
            for(int i=0;i<count;i++)
            {
                var collider=rays[i].collider;
                if(collider!=null && collider.transform.root!=transform.root && !collider.isTrigger && collider.GetComponentInParent<Hurtbox2D>()==null) return false;
            }
            return true;
        }
        private void OnCollisionEnter2D(Collision2D collision)=>ObserveGround(collision);
        private void OnCollisionStay2D(Collision2D collision)=>ObserveGround(collision);
        private void OnCollisionExit2D(Collision2D collision)=>ground.Remove(collision.collider);
        private void ObserveGround(Collision2D collision)
        {
            bool supported=false;
            for(int i=0;i<collision.contactCount;i++) if(collision.GetContact(i).normal.y>.5f) { supported=true; break; }
            if(supported && ground.Count<32) { ground.Add(collision.collider); landed=true; airStepUsed=false; }
            else ground.Remove(collision.collider);
        }
        private void PublishAbility(EquipmentPieceDefinition value)
        {
            var handlers=AbilityUsed;
            if(handlers!=null) foreach(Action<EquipmentPieceDefinition> handler in handlers.GetInvocationList())
                try { handler(value); } catch(Exception exception) { Debug.LogException(exception,this); }
        }
        private void PublishImpact(CombatImpactEvent value)
        {
            var handlers=ImpactOccurred;
            if(handlers!=null) foreach(Action<CombatImpactEvent> handler in handlers.GetInvocationList())
                try { handler(value); } catch(Exception exception) { Debug.LogException(exception,this); }
        }
    }
}
