using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public interface IShowArrivalAudioSink2D { void OnShowArrival(float crowdIntensity); }
    public sealed class SliceShowArrivalBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private MonoBehaviour audioSink;
        [SerializeField] private float celebrationDuration = 5f;
        private VerticalSliceSession2D session;
        private GameObject crowd;
        private Sprite sprite;
        private float elapsed;
        public override string Title => "Show: fãs salvos e equipe reunida";
        public bool TrySetAudioSink(MonoBehaviour source) { if(source!=null&&!(source is IShowArrivalAudioSink2D))return false;audioSink=source;return true; }
        public override bool TryBegin(VerticalSliceSession2D value)
        {
            if(value==null||!Positive(celebrationDuration)||!value.TryLoadStage(stage))return false;
            session=value;IsComplete=false;elapsed=0f;
            crowd=new GameObject("Original crowd silhouette");sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1f);
            for(int i=0;i<24;i++){var fan=new GameObject("Crowd "+i);fan.transform.SetParent(crowd.transform);fan.transform.position=stage.StartPosition+new Vector2(i*.6f,-1.5f);fan.transform.localScale=new Vector3(.3f,.6f,1f);var render=fan.AddComponent<SpriteRenderer>();render.sprite=sprite;render.color=i%2==0?Color.cyan:new Color(1f,.6f,.8f);}
            if(audioSink is IShowArrivalAudioSink2D audio) audio.OnShowArrival(Mathf.Clamp01(session.Performance.Snapshot.Normalized));
            return true;
        }
        public override void Tick(float deltaTime){elapsed+=deltaTime;IsComplete=elapsed>=celebrationDuration;}
        public override void Exit(){if(crowd!=null){crowd.SetActive(false);Destroy(crowd);}if(sprite!=null)Destroy(sprite);crowd=null;sprite=null;session=null;base.Exit();}
        private static bool Positive(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v)&&v>0f;
    }
}
