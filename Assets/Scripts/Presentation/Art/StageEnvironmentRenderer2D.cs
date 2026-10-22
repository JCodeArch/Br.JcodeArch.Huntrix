using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    /// <summary>Bounded, original layered backgrounds. No gameplay collision or scene discovery.</summary>
    [DisallowMultipleComponent]
    public sealed class StageEnvironmentRenderer2D : MonoBehaviour
    {
        [SerializeField] private StageEnvironmentDefinition[] profiles;
        private Texture2D texture;private Sprite sprite;private Sprite stageArtSprite;private bool ownsStageArtSprite;private Transform root,farLayer,nearLayer;private Camera view;private Vector3 cameraStart;
        public string CurrentStageId { get; private set; }
        public bool TryBuild(StageBlockoutDefinition stage,Camera camera)
        {
            if(!isActiveAndEnabled||stage==null||!stage.IsValid(out _)||camera==null||profiles==null||profiles.Length<1||profiles.Length>32)return false;
            StageEnvironmentDefinition profile=null;for(int i=0;i<profiles.Length;i++){if(profiles[i]==null||!profiles[i].IsValid)return false;for(int j=0;j<i;j++)if(profiles[i].StageId==profiles[j].StageId)return false;if(profiles[i].StageId==stage.StageId)profile=profiles[i];}
            if(profile==null||profile.Theme!=stage.Theme)return false;
            Clear();view=camera;cameraStart=camera.transform.position;CurrentStageId=stage.StageId;
            texture=new Texture2D(1,1);texture.SetPixel(0,0,Color.white);texture.Apply();sprite=Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
            root=new GameObject("Original_Environment_"+stage.StageId).transform;root.SetParent(transform,false);root.position=Vector3.zero;
            farLayer=Layer("Far",root);nearLayer=Layer("Near",root);
            Quad(root,"Sky",new Vector2(24,8),new Vector2(100,30),profile.Sky,-30);
            TryAttachStageArtBackdrop(stage.StageId);
            for(int i=0;i<12;i++)
            {
                float x=i*5-5;float h=3+(i%4)*1.1f;
                Quad(farLayer,"Skyline_"+i,new Vector2(x,h*.5f),new Vector2(3.8f,h),profile.Distant,-25);
                if(profile.Theme==StageBlockoutTheme.Interior)
                {Quad(nearLayer,"CabinPanel_"+i,new Vector2(x,3.2f),new Vector2(4.5f,3),profile.Near,-20);Quad(nearLayer,"Window_"+i,new Vector2(x,3.5f),new Vector2(2.5f,1.3f),profile.Accent,-19);}
                else if(profile.Theme==StageBlockoutTheme.Supernatural)
                {Quad(nearLayer,"ArchL_"+i,new Vector2(x-1.4f,2.1f),new Vector2(.35f,4.2f),profile.Near,-20);Quad(nearLayer,"ArchR_"+i,new Vector2(x+1.4f,2.1f),new Vector2(.35f,4.2f),profile.Near,-20);Quad(nearLayer,"ArchTop_"+i,new Vector2(x,4.2f),new Vector2(3.2f,.3f),profile.Accent,-19);}
                else
                {Quad(nearLayer,"Building_"+i,new Vector2(x+1,1.2f),new Vector2(2.8f,2.4f),profile.Near,-20);for(int n=0;n<3;n++)Quad(nearLayer,"Window_"+i+"_"+n,new Vector2(x+.1f+n*.8f,1.6f),new Vector2(.35f,.5f),profile.Accent,-19);}
            }
            if(profile.Theme==StageBlockoutTheme.DayUrban)Quad(farLayer,"Sun",new Vector2(16,7),new Vector2(1.2f,1.2f),profile.Accent,-24);
            if(profile.Theme==StageBlockoutTheme.NightUrban)Quad(farLayer,"Moon",new Vector2(18,7),new Vector2(.85f,.85f),profile.Accent,-24);
            return true;
        }
        private void TryAttachStageArtBackdrop(string stageId)
        {
            stageArtSprite = Resources.Load<Sprite>("Art/Stages/" + stageId);
            ownsStageArtSprite = false;
            if (stageArtSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("Art/Stages/" + stageId);
                if (tex != null)
                {
                    stageArtSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    ownsStageArtSprite = true;
                }
            }
            // StageBlockoutBuilder2D owns the undistorted 16:9 tiled HD_StageBackdrop_<key> and its single LivingStageBackdropAnimator2D.
            // Keep stageArtSprite resolved here so procedural foreground blocks remain suppressed when HD stage art is present.
        }
        private Transform Layer(string name,Transform parent){Transform layer=new GameObject(name).transform;layer.SetParent(parent,false);return layer;}
        private void Quad(Transform parent,string name,Vector2 position,Vector2 size,Color color,int sorting){GameObject item=new GameObject(name);item.transform.SetParent(parent,false);item.transform.localPosition=position;item.transform.localScale=new Vector3(size.x,size.y,1);SpriteRenderer renderer=item.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingOrder=sorting;if(stageArtSprite!=null)renderer.enabled=false;}
        private void LateUpdate(){if(view==null||root==null)return;Vector3 delta=view.transform.position-cameraStart;if(float.IsNaN(delta.x)||float.IsInfinity(delta.x)||float.IsNaN(delta.y)||float.IsInfinity(delta.y))return;delta.z=0;if(farLayer!=null)farLayer.localPosition=delta*.15f;if(nearLayer!=null)nearLayer.localPosition=delta*.06f;}
        public void Clear(){if(root!=null){root.gameObject.SetActive(false);Destroy(root.gameObject);}root=null;farLayer=null;nearLayer=null;view=null;CurrentStageId=null;if(ownsStageArtSprite&&stageArtSprite!=null)Destroy(stageArtSprite);stageArtSprite=null;ownsStageArtSprite=false;if(sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);sprite=null;texture=null;}
        private void OnDisable(){Clear();}
    }
}
