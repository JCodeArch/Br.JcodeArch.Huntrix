using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.Special
{
    /// <summary>Original geometric, non-colliding projections. All damage belongs to the Special core.</summary>
    [DisallowMultipleComponent,RequireComponent(typeof(HuntrXSpecialController2D))]
    public sealed class SpecialSequenceEffects2D : MonoBehaviour
    {
        [SerializeField] private HuntrXSpecialController2D special;
        private readonly SpriteRenderer[] performers=new SpriteRenderer[3];
        private readonly SpriteRenderer[] rays=new SpriteRenderer[64];
        private readonly Vector3[] impactPositions=new Vector3[64];
        private readonly bool[] impactVisible=new bool[64];
        private SpriteRenderer flourish;
        private Sprite sprite;
        private bool visible;
        private SpecialStage2D stage;
        private readonly Color[] colors={new Color(.55f,.35f,1f,.75f),new Color(.2f,.8f,1f,.75f),new Color(1f,.3f,.7f,.75f)};
        private void Awake()
        {
            if(special==null)special=GetComponent<HuntrXSpecialController2D>();
            sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1f);
            for(int i=0;i<3;i++)performers[i]=CreateVisual(i==0?"Rumi projection":i==1?"Mira projection":"Zoey projection");
            for(int i=0;i<64;i++)rays[i]=CreateVisual("Special ray "+i);
            flourish=CreateVisual("Special stage flourish");Hide();
        }
        private SpriteRenderer CreateVisual(string name)
        {
            GameObject visual=new GameObject(name);visual.transform.SetParent(transform,false);
            SpriteRenderer renderer=visual.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=5;return renderer;
        }
        private void OnEnable()
        {
            if(special==null)return;
            special.StageChanged+=ShowStage;special.Completed+=HandleCompleted;
            if(special.IsActive)ShowStage(special.Stage);
        }
        private void OnDisable()
        {
            if(special!=null){special.StageChanged-=ShowStage;special.Completed-=HandleCompleted;}Hide();
        }
        private void OnDestroy(){if(sprite!=null)Destroy(sprite);}
        private void ShowStage(SpecialStage2D next)
        {
            if(!isActiveAndEnabled||special==null||!special.IsActive)return;
            stage=next;visible=true;
            for(int i=0;i<64;i++)
            {
                DamageReceiver2D target=i<special.SelectedCount?special.GetSelectedTarget(i):null;
                impactVisible[i]=next==SpecialStage2D.Combined&&target!=null&&target.IsAlive&&
                    target.Faction==CombatFaction2D.Demon&&target.GetComponentInParent<FanActor2D>()==null&&
                    target.GetComponentInChildren<FanActor2D>(true)==null;
                if(impactVisible[i])impactPositions[i]=target.transform.position;
            }
            UpdateVisuals();
        }
        private void HandleCompleted(bool cancelled)=>Hide();
        private void Hide()
        {
            visible=false;
            for(int i=0;i<3;i++)if(performers[i]!=null)performers[i].enabled=false;
            for(int i=0;i<64;i++)if(rays[i]!=null)rays[i].enabled=false;
            if(flourish!=null)flourish.enabled=false;
        }
        private void LateUpdate(){if(visible)UpdateVisuals();}
        private void UpdateVisuals()
        {
            DamageReceiver2D owner=special==null?null:special.Owner;
            if(owner==null||!owner.isActiveAndEnabled||!owner.IsAlive||!special.IsActive){Hide();return;}
            Vector3 origin=owner.transform.position;
            for(int i=0;i<3;i++)
            {
                bool show=stage==SpecialStage2D.Combined||(int)stage==i;performers[i].enabled=show;
                performers[i].color=colors[i];performers[i].transform.position=origin+new Vector3((i-1)*1.2f,1.7f,0f);
                performers[i].transform.localScale=new Vector3(.45f,.9f,1f);
            }
            flourish.enabled=true;
            flourish.color=stage==SpecialStage2D.Combined?new Color(.95f,.8f,1f,.6f):colors[(int)stage];
            flourish.transform.position=origin+Vector3.up*.4f;
            flourish.transform.rotation=Quaternion.Euler(0,0,stage==SpecialStage2D.Rumi?-35f:stage==SpecialStage2D.Zoey?45f:0f);
            flourish.transform.localScale=stage==SpecialStage2D.Rumi?new Vector3(2.5f,.12f,1f):
                stage==SpecialStage2D.Mira?new Vector3(1.8f,1.8f,1f):stage==SpecialStage2D.Zoey?new Vector3(.65f,.65f,1f):new Vector3(2.5f,2.5f,1f);
            for(int i=0;i<64;i++)
            {
                bool show=stage==SpecialStage2D.Combined&&impactVisible[i];
                rays[i].enabled=show;if(!show)continue;
                Vector2 offset=(Vector2)impactPositions[i]-(Vector2)origin;
                rays[i].transform.position=origin+(Vector3)(offset*.5f);
                rays[i].transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(offset.y,offset.x)*Mathf.Rad2Deg);
                rays[i].transform.localScale=new Vector3(offset.magnitude,.06f,1f);
                rays[i].color=new Color(.85f,.5f,1f,.65f);
            }
        }
    }
}
