using UnityEngine;
namespace HuntrX.Gameplay.Audio
{
    public enum AudioCue { Attack, Hit, Dash, Parry, Enemy, Rescue, Honmoon }
    public enum AudioLayer { Exploration, Combat, Combo, Crowd, Crisis, Special }
    /// <summary>Bounded reusable voices and independently faded synchronized music layers.</summary>
    [DisallowMultipleComponent]
    public sealed class GameAudioManager2D : MonoBehaviour
    {
        [SerializeField] private AudioClip[] cues=new AudioClip[7];
        [SerializeField] private AudioClip[] layers=new AudioClip[6];
        [SerializeField,Range(1,32)] private int voiceCount=12;
        [SerializeField,Range(0,1)] private float masterVolume=.65f;
        [SerializeField,Min(.01f)] private float fadeSpeed=2f;
        public float MasterVolume=>masterVolume;
        public bool TrySetMasterVolume(float value){if(float.IsNaN(value)||float.IsInfinity(value))return false;masterVolume=Mathf.Clamp01(value);if(dialogue!=null)dialogue.volume=masterVolume;return true;}
        private AudioSource[] voices,music;
        private AudioSource dialogue;
        private readonly float[] targets=new float[6];
        private void Awake()
        {
            voices=new AudioSource[Mathf.Clamp(voiceCount,1,32)];music=new AudioSource[6];
            for(int i=0;i<voices.Length;i++)voices[i]=Create(false);
            for(int i=0;i<6;i++){music[i]=Create(true);music[i].clip=layers!=null&&i<layers.Length?layers[i]:null;}
            dialogue=Create(false);targets[0]=1f;
        }
        private AudioSource Create(bool loop)
        {AudioSource source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.spatialBlend=0f;source.volume=0f;return source;}
        private void OnEnable()
        {if(music!=null)foreach(AudioSource source in music)if(source.clip!=null)source.Play();}
        private void Update()
        {for(int i=0;i<6;i++)music[i].volume=Mathf.MoveTowards(music[i].volume,targets[i]*masterVolume,fadeSpeed*Time.unscaledDeltaTime);}
        public bool TryPlay(AudioCue cue,Vector3 position)
        {
            int index=(int)cue;if(!isActiveAndEnabled||voices==null||cues==null||index<0||index>=cues.Length||cues[index]==null)return false;
            foreach(AudioSource source in voices)if(!source.isPlaying)
            {source.volume=masterVolume;source.clip=cues[index];source.Play();return true;}
            return false; // Saturated voices drop rather than steal or allocate.
        }
        public AudioClip GetLayerClip(AudioLayer layer){int index=(int)layer;return layers!=null&&index>=0&&index<layers.Length?layers[index]:null;}
        public bool SetLayer(AudioLayer layer,float intensity)
        {
            int i=(int)layer;if(i<0||i>=6||float.IsNaN(intensity)||float.IsInfinity(intensity))return false;
            targets[i]=Mathf.Clamp01(intensity);return true;
        }
        public bool TryPlayVoice(AudioClip clip)
        {if(!isActiveAndEnabled||dialogue==null||clip==null)return false;dialogue.Stop();dialogue.clip=clip;dialogue.volume=masterVolume;dialogue.Play();return true;}
        public void StopVoice(){if(dialogue!=null)dialogue.Stop();}
        private void OnDisable()
        {if(voices!=null)foreach(AudioSource source in voices)source.Stop();if(music!=null)foreach(AudioSource source in music)source.Stop();StopVoice();}
    }
}
