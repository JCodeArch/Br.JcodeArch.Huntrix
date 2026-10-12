using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    [DisallowMultipleComponent]
    public sealed class GameplayVfxPool2D : MonoBehaviour
    {
        [SerializeField] private GameplayVfxDefinition definition;
        private LineRenderer[] lines;
        private float[] remaining;
        private Vector3[] centers;
        private GameplayVfxKind[] kinds;
        private float[] intensities;
        private bool[] steady;
        private Material material;
        public int ActiveCount { get; private set; }
        public bool TryConfigure(GameplayVfxDefinition profile)
        {
            if(!isActiveAndEnabled||profile==null||!profile.IsValid)return false;
            Clear();definition=profile;Shader shader=Shader.Find("Sprites/Default");if(shader==null)return false;
            material=new Material(shader);lines=new LineRenderer[profile.Capacity];remaining=new float[lines.Length];centers=new Vector3[lines.Length];kinds=new GameplayVfxKind[lines.Length];intensities=new float[lines.Length];steady=new bool[lines.Length];
            for(int i=0;i<lines.Length;i++) { GameObject child=new GameObject("Original_VFX_"+i);child.transform.SetParent(transform,false);LineRenderer line=child.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.sortingOrder=20;line.startWidth=profile.LineWidth;line.endWidth=profile.LineWidth;line.enabled=false;lines[i]=line; }
            return true;
        }
        public bool TryEmit(GameplayVfxKind kind,Vector3 from,Vector3 to,float intensity=1f,bool steadyTelegraph=false)
        {
            if(!isActiveAndEnabled||definition==null||!definition.IsValid||lines==null||!Finite(from)||!Finite(to)||(int)kind<0||(int)kind>5||float.IsNaN(intensity)||float.IsInfinity(intensity)||intensity<=0||intensity>1)return false;
            int slot=-1;for(int i=0;i<remaining.Length;i++)if(remaining[i]<=0){slot=i;break;}if(slot<0)return false;
            remaining[slot]=definition.Lifetime;centers[slot]=from;kinds[slot]=kind;intensities[slot]=intensity;steady[slot]=steadyTelegraph&&kind==GameplayVfxKind.Telegraph;LineRenderer line=lines[slot];Color color=definition.ColorFor(kind);color.r*=intensity;color.g*=intensity;color.b*=intensity;line.startColor=color;line.endColor=color;line.enabled=true;ActiveCount++;
            if(kind==GameplayVfxKind.SoulDrain||kind==GameplayVfxKind.Energy){line.loop=false;line.positionCount=2;line.SetPosition(0,from);line.SetPosition(1,to);}else{line.loop=true;line.positionCount=12;Ring(slot,.1f);}return true;
        }
        private static bool Finite(Vector3 v)=>!float.IsNaN(v.x)&&!float.IsInfinity(v.x)&&!float.IsNaN(v.y)&&!float.IsInfinity(v.y)&&!float.IsNaN(v.z)&&!float.IsInfinity(v.z)&&Mathf.Abs(v.x)<=10000&&Mathf.Abs(v.y)<=10000&&Mathf.Abs(v.z)<=10000;
        private void Ring(int slot,float fraction) { float r=definition.Radius*(.5f+fraction);if(kinds[slot]==GameplayVfxKind.Special)r*=2;for(int p=0;p<12;p++){float a=p*Mathf.PI*2/12;lines[slot].SetPosition(p,centers[slot]+new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0));} }
        private void Awake(){if(definition!=null)TryConfigure(definition);}
        private void Update()
        {
            if(lines==null||definition==null||!definition.IsValid)return;
            float dt=Mathf.Min(Time.deltaTime,.1f);for(int i=0;i<lines.Length;i++)if(remaining[i]>0){remaining[i]=Mathf.Max(0,remaining[i]-dt);if(remaining[i]<=0){lines[i].enabled=false;ActiveCount--;continue;}float t=1-remaining[i]/definition.Lifetime;Color color=definition.ColorFor(kinds[i]);color.r*=intensities[i];color.g*=intensities[i];color.b*=intensities[i];if(!steady[i])color.a*=1-t;lines[i].startColor=color;lines[i].endColor=color;if(kinds[i]!=GameplayVfxKind.SoulDrain&&kinds[i]!=GameplayVfxKind.Energy)Ring(i,steady[i] ? .1f : t);}
        }
        public void ClearActive(){if(lines==null)return;for(int i=0;i<lines.Length;i++){remaining[i]=0;lines[i].enabled=false;}ActiveCount=0;}
        private void OnDisable(){ClearActive();}
        private void Clear(){if(lines!=null)foreach(LineRenderer line in lines)if(line!=null){line.enabled=false;Destroy(line.gameObject);}lines=null;remaining=null;centers=null;kinds=null;intensities=null;steady=null;ActiveCount=0;if(material!=null)Destroy(material);material=null;}
        private void OnDestroy(){Clear();}
    }
}
