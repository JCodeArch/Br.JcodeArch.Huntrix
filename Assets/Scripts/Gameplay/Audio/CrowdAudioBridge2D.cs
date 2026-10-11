using HuntrX.Gameplay.Performance;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Gameplay.Audio
{
    [DisallowMultipleComponent]
    public sealed class CrowdAudioBridge2D : MonoBehaviour,IShowArrivalAudioSink2D
    {
        private GameAudioManager2D audioManager;
        private CombatPerformanceController2D performance;
        private float arrival;
        private AudioSource cheering,roaring;
        private void Awake(){cheering=gameObject.AddComponent<AudioSource>();roaring=gameObject.AddComponent<AudioSource>();foreach(var source in new[]{cheering,roaring}){source.playOnAwake=false;source.loop=true;source.volume=0f;source.spatialBlend=0f;}cheering.pitch=.85f;roaring.pitch=1.12f;}
        public bool Configure(GameAudioManager2D audio,CombatPerformanceController2D metrics)
        {if(audio==null||metrics==null)return false;Unbind();audioManager=audio;performance=metrics;if(isActiveAndEnabled)Bind();return true;}
        public void OnShowArrival(float intensity)
        {if(float.IsNaN(intensity)||float.IsInfinity(intensity))return;arrival=Mathf.Clamp01(intensity);Refresh();}
        private void Bind(){performance.SnapshotChanged+=Handle;AudioClip clip=audioManager.GetLayerClip(AudioLayer.Crowd);cheering.clip=clip;roaring.clip=clip;if(clip!=null){cheering.Play();roaring.Play();}Refresh();}
        private void Unbind(){if(performance!=null)performance.SnapshotChanged-=Handle;}
        private void Handle(PerformanceSnapshot value)=>Refresh();
        private void Refresh()
        {
            float intensity=Mathf.Max(arrival,performance==null?0f:performance.Snapshot.Normalized);
            if(audioManager!=null)audioManager.SetLayer(AudioLayer.Crowd,intensity*.25f);
            if(cheering!=null)cheering.volume=intensity*.22f*(audioManager==null||!audioManager.isActiveAndEnabled?0f:audioManager.MasterVolume);
            if(roaring!=null)roaring.volume=Mathf.Clamp01((intensity-.5f)*2f)*.25f*(audioManager==null||!audioManager.isActiveAndEnabled?0f:audioManager.MasterVolume);
        }
        private void Update(){Refresh();}
        private void OnEnable(){if(audioManager!=null)Bind();}
        private void OnDisable(){Unbind();arrival=0f;if(cheering!=null)cheering.Stop();if(roaring!=null)roaring.Stop();if(audioManager!=null)audioManager.SetLayer(AudioLayer.Crowd,0f);}
    }
}
