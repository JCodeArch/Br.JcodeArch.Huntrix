using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    /// <summary>Presentation only. State reads never authorize hits, movement or invulnerability.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterGameplayAnimator2D : MonoBehaviour
    {
        [SerializeField] private CharacterArtRenderer2D renderer2D;
        [SerializeField] private CharacterRigDefinition rig;
        private DamageReceiver2D receiver;
        private AttackController2D attack;
        private DashController2D dash;
        private ParryController2D parry;
        private Rigidbody2D body;
        private ZoeyRangedAttack2D ranged;
        private EnemyAgent2D enemy;
        private Transform cachedRoot;
        private Vector3 rootPosition;
        private float hurtRemaining,specialRemaining,rangedRemaining,clock;
        private int facing=1;
        public CharacterVisualPose Pose { get; private set; }
        public bool TryConfigure(CharacterArtRenderer2D visual,CharacterRigDefinition profile)
        {
            if(!isActiveAndEnabled||visual==null||profile==null||!profile.IsValid||visual.Definition!=profile.Character)return false;
            ResetPose();renderer2D=visual;rig=profile;CacheParts();return true;
        }
        public void PlaySpecial() { if(isActiveAndEnabled&&rig!=null&&rig.IsValid)specialRemaining=rig.SpecialSeconds; }
        private void Awake() { receiver=GetComponent<DamageReceiver2D>();attack=GetComponent<AttackController2D>();dash=GetComponent<DashController2D>();parry=GetComponent<ParryController2D>();body=GetComponent<Rigidbody2D>();ranged=GetComponent<ZoeyRangedAttack2D>();enemy=GetComponent<EnemyAgent2D>(); }
        private void OnEnable() { if(receiver!=null)receiver.DamageReceived+=OnHurt;if(ranged!=null)ranged.ShotFired+=OnShot; }
        private void OnDisable() { if(receiver!=null)receiver.DamageReceived-=OnHurt;if(ranged!=null)ranged.ShotFired-=OnShot;ResetPose();rangedRemaining=0;hurtRemaining=0;specialRemaining=0;clock=0; }
        private void OnShot(Vector2 from,Vector2 to){rangedRemaining=.3f;}
        private void OnHurt(CombatImpactEvent impact) { if(rig!=null&&rig.IsValid)hurtRemaining=rig.HurtSeconds; }
        private void CacheParts() { cachedRoot=renderer2D!=null?renderer2D.VisualRoot:null;if(cachedRoot!=null)rootPosition=cachedRoot.localPosition; }
        private void LateUpdate()
        {
            if(renderer2D==null||rig==null||!rig.IsValid||renderer2D.Definition!=rig.Character||renderer2D.VisualRoot==null)return;
            if(cachedRoot!=renderer2D.VisualRoot)CacheParts();
            float dt=Mathf.Min(Time.deltaTime,.1f);clock=(clock+dt)%1000;hurtRemaining=Mathf.Max(0,hurtRemaining-dt);specialRemaining=Mathf.Max(0,specialRemaining-dt);rangedRemaining=Mathf.Max(0,rangedRemaining-dt);
            Vector2 velocity=body!=null?body.linearVelocity:Vector2.zero;
            if(float.IsNaN(velocity.x)||float.IsInfinity(velocity.x)||float.IsNaN(velocity.y)||float.IsInfinity(velocity.y))velocity=Vector2.zero;
            if(Mathf.Abs(velocity.x)>.1f)facing=velocity.x<0?-1:1;
            Pose=receiver!=null&&!receiver.IsAlive?CharacterVisualPose.Death:specialRemaining>0?CharacterVisualPose.Special:hurtRemaining>0?CharacterVisualPose.Hurt:(parry!=null&&parry.IsWindowActive)||(enemy!=null&&enemy.State==EnemyState2D.Telegraph)?CharacterVisualPose.Parry:dash!=null&&dash.IsDashing?CharacterVisualPose.Dash:(attack!=null&&attack.State!=AttackState2D.Idle)||rangedRemaining>0||(enemy!=null&&enemy.State==EnemyState2D.Attacking)?(Mathf.Abs(velocity.y)>.15f?CharacterVisualPose.Aerial:CharacterVisualPose.Attack):Mathf.Abs(velocity.y)>.15f?CharacterVisualPose.Jump:Mathf.Abs(velocity.x)>.1f?CharacterVisualPose.Run:CharacterVisualPose.Idle;
            float wave=Mathf.Sin(clock*Mathf.PI*2/rig.RunCycleSeconds);
            ResetRotations();cachedRoot.localPosition=rootPosition;cachedRoot.localScale=new Vector3(facing,1,1);cachedRoot.localRotation=Quaternion.identity;
            switch(Pose)
            {
                case CharacterVisualPose.Idle:cachedRoot.localPosition+=Vector3.up*(.015f*Mathf.Sin(clock*Mathf.PI*2/rig.IdleCycleSeconds));break;
                case CharacterVisualPose.Run:Rotate(renderer2D.LeftLeg,wave*25);Rotate(renderer2D.RightLeg,-wave*25);Rotate(renderer2D.LeftArm,-wave*18);Rotate(renderer2D.RightArm,wave*18);cachedRoot.localPosition+=Vector3.up*Mathf.Abs(wave)*.025f;break;
                case CharacterVisualPose.Jump:Rotate(renderer2D.LeftLeg,-20);Rotate(renderer2D.RightLeg,20);Rotate(renderer2D.LeftArm,-25);Rotate(renderer2D.RightArm,25);break;
                case CharacterVisualPose.Dash:cachedRoot.localRotation=Quaternion.Euler(0,0,-facing*18);Rotate(renderer2D.LeftArm,45);Rotate(renderer2D.RightArm,45);break;
                case CharacterVisualPose.Attack:case CharacterVisualPose.Aerial:Rotate(renderer2D.RightArm,-65);Rotate(renderer2D.Weapon,-65);Rotate(renderer2D.LeftLeg,Pose==CharacterVisualPose.Aerial?-25:0);break;
                case CharacterVisualPose.Hurt:cachedRoot.localRotation=Quaternion.Euler(0,0,facing*12);break;
                case CharacterVisualPose.Parry:Rotate(renderer2D.RightArm,60);Rotate(renderer2D.Weapon,30);break;
                case CharacterVisualPose.Death:cachedRoot.localRotation=Quaternion.Euler(0,0,facing*80);cachedRoot.localPosition+=Vector3.down*.25f;break;
                case CharacterVisualPose.Special:Rotate(renderer2D.LeftArm,-70);Rotate(renderer2D.RightArm,70);Rotate(renderer2D.Weapon,Mathf.Sin(clock*6)*20);cachedRoot.localPosition+=Vector3.up*.08f;break;
            }
        }
        private static void Rotate(Transform part,float degrees) { if(part!=null)part.localRotation=Quaternion.Euler(0,0,degrees); }
        private void ResetRotations() { if(renderer2D==null)return;Rotate(renderer2D.LeftLeg,0);Rotate(renderer2D.RightLeg,0);Rotate(renderer2D.LeftArm,0);Rotate(renderer2D.RightArm,0);Rotate(renderer2D.Weapon,renderer2D.Definition!=null&&renderer2D.Definition.Weapon==CharacterWeaponShape.TwinStars?45:0); }
        private void ResetPose() { if(cachedRoot!=null){cachedRoot.localPosition=rootPosition;cachedRoot.localScale=Vector3.one;cachedRoot.localRotation=Quaternion.identity;}ResetRotations(); }
    }
}
