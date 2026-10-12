using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Special;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    [DisallowMultipleComponent]
    public sealed class GameplayVfxAdapter2D : MonoBehaviour
    {
        [SerializeField] private GameplayVfxPool2D pool;
        [SerializeField] private DamageReceiver2D actor;
        [SerializeField] private EnemyAgent2D enemy;
        [SerializeField] private HonmoonController2D honmoon;
        [SerializeField] private HuntrXSpecialController2D special;
        [SerializeField] private FanSoulDrainController2D drainer;
        private DamageReceiver2D boundActor;private EnemyAgent2D boundEnemy;private HonmoonController2D boundHonmoon;private HuntrXSpecialController2D boundSpecial;private FanSoulDrainController2D boundDrainer;
        public bool TryConfigure(GameplayVfxPool2D effects,DamageReceiver2D source,EnemyAgent2D telegraphSource=null,HonmoonController2D resource=null,HuntrXSpecialController2D union=null,FanSoulDrainController2D drain=null)
        { if(!isActiveAndEnabled||effects==null)return false;Unsubscribe();pool=effects;actor=source;enemy=telegraphSource;honmoon=resource;special=union;drainer=drain;Subscribe();return true; }
        public void Unbind(){Unsubscribe();actor=null;enemy=null;honmoon=null;special=null;drainer=null;}
        private void OnEnable(){Subscribe();}
        private void OnDisable(){Unsubscribe();}
        private void Subscribe()
        {
            Unsubscribe();boundActor=actor;boundEnemy=enemy;boundHonmoon=honmoon;boundSpecial=special;boundDrainer=drainer;
            if(boundActor!=null){boundActor.DamageDealt+=OnImpact;boundActor.DamageReceived+=OnImpact;}
            if(boundEnemy!=null)boundEnemy.StateChanged+=OnEnemy;
            if(boundHonmoon!=null)boundHonmoon.BandChanged+=OnBand;
            if(boundSpecial!=null){boundSpecial.StageChanged+=OnSpecial;boundSpecial.ChargeChanged+=OnEnergy;}
            if(boundDrainer!=null)boundDrainer.FanDrainAccepted+=OnDrain;
        }
        private void Unsubscribe(){if(boundActor!=null){boundActor.DamageDealt-=OnImpact;boundActor.DamageReceived-=OnImpact;}if(boundEnemy!=null)boundEnemy.StateChanged-=OnEnemy;if(boundHonmoon!=null)boundHonmoon.BandChanged-=OnBand;if(boundSpecial!=null){boundSpecial.StageChanged-=OnSpecial;boundSpecial.ChargeChanged-=OnEnergy;}if(boundDrainer!=null)boundDrainer.FanDrainAccepted-=OnDrain;boundActor=null;boundEnemy=null;boundHonmoon=null;boundSpecial=null;boundDrainer=null;}
        private Vector3 Origin=>actor!=null?actor.transform.position:transform.position;
        private void Emit(GameplayVfxKind kind,Vector3 from,Vector3 to){if(isActiveAndEnabled&&pool!=null)pool.TryEmit(kind,from,to);}
        private void OnImpact(CombatImpactEvent hit){if(hit.Receiver!=null)Emit(GameplayVfxKind.Impact,hit.Receiver.transform.position,hit.Receiver.transform.position);}
        private void OnEnemy(EnemyState2D state){if(state==EnemyState2D.Telegraph&&enemy!=null)Emit(GameplayVfxKind.Telegraph,enemy.transform.position,enemy.transform.position);}
        private void OnBand(HonmoonBand band){Emit(GameplayVfxKind.Honmoon,Origin,Origin);}
        private void OnEnergy(float charge){Emit(GameplayVfxKind.Energy,Origin,Origin+Vector3.up*.75f);}
        private void OnSpecial(SpecialStage2D stage){if(!isActiveAndEnabled)return;Emit(GameplayVfxKind.Special,Origin,Origin);if(actor!=null){CharacterGameplayAnimator2D animator=actor.GetComponent<CharacterGameplayAnimator2D>();if(animator!=null)animator.PlaySpecial();}}
        private void OnDrain(FanActor2D fan,float accepted){if(fan!=null&&drainer!=null&&accepted>0)Emit(GameplayVfxKind.SoulDrain,fan.transform.position,drainer.transform.position);}
    }
}
