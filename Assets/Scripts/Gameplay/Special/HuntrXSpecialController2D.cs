using System;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Protection;
using UnityEngine;
namespace HuntrX.Gameplay.Special
{
    [DisallowMultipleComponent]
    public sealed class HuntrXSpecialController2D : MonoBehaviour,ITeamUnionAction2D
    {
        [SerializeField] private HuntrXSpecialDefinition definition;
        [SerializeField] private CharacterManager2D ownerSlot;
        [SerializeField] private DamageReceiver2D owner;
        [SerializeField] private DamageReceiver2D[] targets=Array.Empty<DamageReceiver2D>();
        private readonly DamageReceiver2D[] selected=new DamageReceiver2D[64];
        private DamageReceiver2D activationOwner;
        private float remaining;
        private bool subscribed,resolving,configuring,dispatching;
        public HuntrXSpecialDefinition Definition=>definition;
        public DamageReceiver2D Owner=>ownerSlot!=null?ownerSlot.ActiveCharacter:owner;
        public float Charge {get;private set;}
        public bool IsReady=>definition!=null&&definition.IsValid(out _)&&Charge>=definition.MaximumCharge;
        public bool IsActive {get;private set;}
        public SpecialStage2D Stage {get;private set;}
        public int SelectedCount {get;private set;}
        public event Action<float> ChargeChanged;
        public event Action<SpecialStage2D> StageChanged;
        public event Action<bool> Completed;
        public DamageReceiver2D GetSelectedTarget(int index)=>index>=0&&index<SelectedCount?selected[index]:null;
        private void OnEnable()=>Subscribe();
        private void OnDisable(){Unsubscribe();Cancel();}
        public bool TryConfigure(HuntrXSpecialDefinition next,DamageReceiver2D source,DamageReceiver2D[] enemies,out string error)
        {
            error=string.Empty;
            if(IsActive||resolving||configuring||dispatching||next==null||!next.IsValid(out error)||!ValidOwner(source))
            {if(string.IsNullOrEmpty(error))error="Special configuration requires an idle controller and live HUNTR/X owner.";return false;}
            if(!ValidRoster(enemies,out error))return false;
            configuring=true;
            try
            {
                Unsubscribe();definition=next;owner=source;ownerSlot=null;targets=(DamageReceiver2D[])enemies.Clone();
                Charge=Mathf.Min(Charge,next.MaximumCharge);Subscribe();PublishCharge();return true;
            }
            finally{configuring=false;}
        }
        public bool TrySetTargets(DamageReceiver2D[] enemies,out string error)
        {
            if(IsActive||resolving||configuring||dispatching){error="Cannot change Special targets during activation.";return false;}
            if(!ValidRoster(enemies,out error))return false;
            Unsubscribe();targets=(DamageReceiver2D[])enemies.Clone();Subscribe();return true;
        }
        public bool TryAddCharge(float value)
        {
            if(!isActiveAndEnabled||configuring||dispatching||resolving||IsActive||definition==null||!definition.IsValid(out _)||!Positive(value))return false;
            Charge=Mathf.Min(definition.MaximumCharge,Charge+value);PublishCharge();return true;
        }
        public bool TryExecuteTeamUnion()=>TryActivate();
        public bool TryActivate()
        {
            DamageReceiver2D source=Owner;
            if(!isActiveAndEnabled||IsActive||resolving||configuring||dispatching||targets==null||targets.Length>64||!IsReady||!ValidOwner(source)||Busy(source))return false;
            SelectedCount=0;
            for(int i=0;i<targets.Length;i++)
            {
                DamageReceiver2D candidate=targets[i];
                if(!Eligible(candidate,source))continue;
                bool duplicate=false;
                for(int j=0;j<SelectedCount;j++)if(selected[j]==candidate){duplicate=true;break;}
                if(duplicate)continue;
                int insert=SelectedCount;
                float distance=((Vector2)candidate.transform.position-(Vector2)source.transform.position).sqrMagnitude;
                while(insert>0&&((Vector2)selected[insert-1].transform.position-(Vector2)source.transform.position).sqrMagnitude>distance)
                {selected[insert]=selected[insert-1];insert--;}
                selected[insert]=candidate;SelectedCount++;
            }
            if(SelectedCount==0)return false;
            int eligibleCount=SelectedCount;
            SelectedCount=Mathf.Clamp(Mathf.CeilToInt(eligibleCount*definition.TargetFraction),1,eligibleCount);
            for(int i=SelectedCount;i<selected.Length;i++)selected[i]=null;
            activationOwner=source;activationOwner.Died+=HandleOwnerDeath;
            IsActive=true;Stage=SpecialStage2D.Rumi;remaining=definition.StageDuration;
            Charge=0f;PublishCharge();
            if(!ActivationValid()){Cancel();return false;}
            ExecuteStage();return IsActive;
        }
        private void Update()
        {
            if(!IsActive)return;
            if(!ActivationValid()){Cancel();return;}
            remaining=Mathf.Max(0f,remaining-Time.deltaTime);
            if(remaining>0f)return;
            if(Stage==SpecialStage2D.Combined){Finish(false);return;}
            Stage=(SpecialStage2D)((int)Stage+1);remaining=definition.StageDuration;ExecuteStage();
        }
        private void ExecuteStage()
        {
            SpecialStage2D published=Stage;
            var handlers=StageChanged;
            bool wasDispatching=dispatching;dispatching=true;
            try
            {
                if(handlers!=null)foreach(Action<SpecialStage2D> handler in handlers.GetInvocationList())
                    try{handler(published);}catch(Exception error){Debug.LogException(error,this);}
            }
            finally{dispatching=wasDispatching;}
            if(!ActivationValid()){Cancel();return;}
            AttackDefinition attack=Stage==SpecialStage2D.Rumi?definition.RumiAttack:Stage==SpecialStage2D.Mira?definition.MiraAttack:
                Stage==SpecialStage2D.Zoey?definition.ZoeyAttack:definition.CombinedAttack;
            resolving=true;
            try
            {
                for(int i=0;i<SelectedCount;i++)
                {
                    if(!ActivationValid())break;
                    DamageReceiver2D target=selected[i];
                    if(!Eligible(target,activationOwner))continue;
                    Hurtbox2D hurtbox=target.GetComponentInChildren<Hurtbox2D>();
                    if(hurtbox==null||!hurtbox.isActiveAndEnabled||hurtbox.Receiver!=target)continue;
                    float facing=target.transform.position.x<activationOwner.transform.position.x?-1f:1f;
                    hurtbox.ResolveHit(attack,activationOwner,facing,0,out _,out _);
                }
            }
            finally{resolving=false;}
            if(IsActive&&!ActivationValid())Cancel();
        }
        public void Cancel(){if(IsActive)Finish(true);}
        private void Finish(bool cancelled)
        {
            if(!IsActive)return;
            IsActive=false;
            if(activationOwner!=null)activationOwner.Died-=HandleOwnerDeath;
            activationOwner=null;remaining=0f;
            Array.Clear(selected,0,selected.Length);SelectedCount=0;
            var handlers=Completed;
            bool wasDispatching=dispatching;dispatching=true;
            try
            {
                if(handlers!=null)foreach(Action<bool> handler in handlers.GetInvocationList())
                    try{handler(cancelled);}catch(Exception error){Debug.LogException(error,this);}
            }
            finally{dispatching=wasDispatching;}
        }
        private void HandleOwnerDeath(DamageReceiver2D receiver)=>Cancel();
        private bool ActivationValid()=>isActiveAndEnabled&&IsActive&&definition!=null&&definition.IsValid(out _)&&
            ValidOwner(activationOwner)&&Owner==activationOwner;
        private bool Eligible(DamageReceiver2D receiver,DamageReceiver2D source)
        {
            if(receiver==null||!receiver.isActiveAndEnabled||!receiver.IsAlive||receiver.Faction!=CombatFaction2D.Demon||
                receiver.transform.root==source.transform.root||receiver.GetComponentInParent<FanActor2D>()!=null||
                receiver.GetComponentInChildren<FanActor2D>(true)!=null||!Finite(receiver.transform.position.x)||!Finite(receiver.transform.position.y))return false;
            return Vector2.Distance(receiver.transform.position,source.transform.position)<=definition.Range;
        }
        private static bool ValidOwner(DamageReceiver2D receiver)=>receiver!=null&&receiver.isActiveAndEnabled&&receiver.IsAlive&&
            receiver.Faction==CombatFaction2D.HuntrX&&receiver.GetComponentInParent<FanActor2D>()==null&&
            Finite(receiver.transform.position.x)&&Finite(receiver.transform.position.y);
        private static bool Busy(DamageReceiver2D receiver)
        {
            DashController2D dash=receiver.GetComponent<DashController2D>();AttackController2D attack=receiver.GetComponent<AttackController2D>();
            ParryController2D parry=receiver.GetComponent<ParryController2D>();MiraProtectionField2D field=receiver.GetComponent<MiraProtectionField2D>();
            ZoeyRangedAttack2D ranged=receiver.GetComponent<ZoeyRangedAttack2D>();
            return(dash!=null&&dash.IsDashing)||(attack!=null&&attack.State!=AttackState2D.Idle)||(parry!=null&&parry.IsWindowActive)||
                (field!=null&&field.IsActive)||(ranged!=null&&ranged.CooldownRemaining>0f);
        }
        private static bool ValidRoster(DamageReceiver2D[] values,out string error)
        {
            error="Special target roster must contain at most 64 distinct, non-null receivers.";
            if(values==null||values.Length>64)return false;
            for(int i=0;i<values.Length;i++){if(values[i]==null)return false;for(int j=0;j<i;j++)if(values[i]==values[j])return false;}
            error=string.Empty;return true;
        }
        private void Subscribe()
        {
            if(subscribed||!isActiveAndEnabled||targets==null||targets.Length>64)return;
            for(int i=0;i<targets.Length;i++)
            {
                if(targets[i]==null)continue;bool duplicate=false;
                for(int j=0;j<i;j++)if(targets[j]==targets[i]){duplicate=true;break;}
                if(!duplicate)targets[i].Died+=HandleEnemyDefeat;
            }
            subscribed=true;
        }
        private void Unsubscribe()
        {
            if(!subscribed)return;
            for(int i=0;i<targets.Length;i++)if(targets[i]!=null)targets[i].Died-=HandleEnemyDefeat;
            subscribed=false;
        }
        private void HandleEnemyDefeat(DamageReceiver2D receiver)
        {
            if(receiver==null||receiver.Faction!=CombatFaction2D.Demon||receiver.GetComponentInParent<FanActor2D>()!=null||
                receiver.GetComponentInChildren<FanActor2D>(true)!=null||definition==null)return;
            TryAddCharge(definition.ChargePerDefeat);
        }
        private void PublishCharge()
        {
            float published=Charge;var handlers=ChargeChanged;
            bool wasDispatching=dispatching;dispatching=true;
            try
            {
                if(handlers!=null)foreach(Action<float> handler in handlers.GetInvocationList())
                    try{handler(published);}catch(Exception error){Debug.LogException(error,this);}
            }
            finally{dispatching=wasDispatching;}
        }
        private static bool Positive(float value)=>Finite(value)&&value>0f;
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
