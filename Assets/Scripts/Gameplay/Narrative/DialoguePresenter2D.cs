using System;
using HuntrX.Data;
using HuntrX.Gameplay.Audio;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    /// <summary>Original localized text, explicit advance and prototype color portraits, without package dependencies.</summary>
    [DisallowMultipleComponent]
    public sealed class DialoguePresenter2D : MonoBehaviour
    {
        [SerializeField] private bool english;
        [SerializeField] private GameAudioManager2D audioManager;
        [SerializeField] private MonoBehaviour narrativePolicy;
        [SerializeField] private bool requiresPolicy;
        private INarrativePresentationPolicy policy;
        private bool resolvingPolicy;
        private int policyVersion;
        private NarrativeSequenceDefinition sequence;
        private int index;
        private float revealed;
        private bool publishing;
        private bool captionsEnabled = true;
        private bool uiBlocked;
        public bool IsUiBlocked => uiBlocked;
        public void SetUiBlocked(bool value) { uiBlocked = value; }
        public bool IsPresenting=>sequence!=null;
        public NarrativeSequenceDefinition Current=>sequence;
        public event Action PresentationChanged;
        private void NotifyPresentationChanged()
        {
            var handlers = PresentationChanged;
            if (handlers != null) foreach (Action handler in handlers.GetInvocationList())
                try { handler(); } catch (Exception error) { Debug.LogException(error, this); }
        }
        public event Action<NarrativeSequenceDefinition> SequenceCompleted;
        public event Action<NarrativeSequenceDefinition> SequenceCancelled;
        public bool BindPolicy(MonoBehaviour value)
        {if(resolvingPolicy||value!=null&&!(value is INarrativePresentationPolicy))return false;UnbindPolicy();narrativePolicy=value;requiresPolicy=!ReferenceEquals(value,null);policy=value as INarrativePresentationPolicy;if(isActiveAndEnabled&&policy!=null)policy.PolicyChanged+=HandlePolicyChanged;HandlePolicyChanged();return true;}
        private void HandlePolicyChanged(){policyVersion++;Cancel();}
        private void UnbindPolicy(){if(policy!=null)policy.PolicyChanged-=HandlePolicyChanged;policy=null;}
        private bool Resolve(NarrativeSequenceDefinition value,int line,out string text)
        {text=null;if(resolvingPolicy)return false;int version=policyVersion;resolvingPolicy=true;try{if(requiresPolicy){if(narrativePolicy==null||!narrativePolicy.isActiveAndEnabled||policy==null||!policy.TryResolve(value,line,english,out text))return false;}else text=value.GetLine(line).Text(english);return isActiveAndEnabled&&version==policyVersion&&!string.IsNullOrWhiteSpace(text)&&text.Length<=2000;}catch(Exception error){Debug.LogException(error,this);return false;}finally{resolvingPolicy=false;}}
        public bool TryPresent(NarrativeSequenceDefinition value)
        {
            if(!isActiveAndEnabled||publishing||resolvingPolicy||IsPresenting||value==null||!value.IsValid(out _))return false;
            int version=policyVersion;for(int i=0;i<value.Count;i++)if(!Resolve(value,i,out _))return false;
            if(version!=policyVersion||!isActiveAndEnabled||IsPresenting)return false;sequence=value;index=0;revealed=0f;PlayVoice();NotifyPresentationChanged();return true;
        }
        public void SetCaptionsEnabled(bool value) { captionsEnabled = value; }
        public void SetEnglish(bool value){english=value;}
        public void SetAudio(GameAudioManager2D value){audioManager=value;}
        public static Rect GetCutsceneInsetViewportRect(float width, float height) => new Rect(width * 0.72f, height * 0.03f, width * 0.24f, height * 0.22f);
        public static Rect GetDialoguePanelViewportRect(float width, float height) => new Rect(width * 0.08f, height * 0.72f, width * 0.84f, height * 0.23f);
        public bool Advance()
        {
            if(!isActiveAndEnabled||publishing||sequence==null)return false;
            if(!Resolve(sequence,index,out string text)){Cancel();return false;}
            if(revealed<text.Length){revealed=text.Length;return true;}
            if(++index<sequence.Count){revealed=0f;PlayVoice();return true;}
            NarrativeSequenceDefinition completed=sequence;sequence=null;audioManager?.StopVoice();
            publishing=true;
            try{var handlers=SequenceCompleted;if(handlers!=null)foreach(Action<NarrativeSequenceDefinition> handler in handlers.GetInvocationList())try{handler(completed);}catch(Exception error){Debug.LogException(error,this);}}
            finally{publishing=false;NotifyPresentationChanged();}
            return true;
        }
        public void Cancel()
        {
            policyVersion++;NarrativeSequenceDefinition old=sequence;sequence=null;revealed=0f;audioManager?.StopVoice();if(old==null)return;
            bool wasPublishing=publishing;publishing=true;try{var handlers=SequenceCancelled;if(handlers!=null)foreach(Action<NarrativeSequenceDefinition> handler in handlers.GetInvocationList())try{handler(old);}catch(Exception error){Debug.LogException(error,this);}}finally{publishing=wasPublishing;NotifyPresentationChanged();}
        }
        private void PlayVoice(){if(audioManager!=null){audioManager.StopVoice();AudioClip clip=sequence.GetLine(index).Voice;if(clip!=null&&!requiresPolicy)audioManager.TryPlayVoice(clip);}}
        private void Update(){if(uiBlocked)return;if(sequence!=null){if(!Resolve(sequence,index,out string text)){Cancel();return;}revealed=Mathf.Min(text.Length,revealed+sequence.CharactersPerSecond*Time.unscaledDeltaTime);}}
        private void OnGUI()
        {
            if(uiBlocked||sequence==null)return;NarrativeLine line=sequence.GetLine(index);if(!Resolve(sequence,index,out string text)){Cancel();return;}
            float w=Screen.width,h=Screen.height;

            // Compact Mega Man X4 Style Anime Cutscene Picture-in-Picture Card (top-right, never occluding the in-world VIP stage tableau!)
            Texture2D animeScene = ResolveAnimeCutsceneTexture(sequence.CueId, line.Speaker);
            if (animeScene != null)
            {
                Rect cinemaRect = GetCutsceneInsetViewportRect(w, h);
                Color prev = GUI.color;
                GUI.color = new Color(0.05f, 0.03f, 0.12f, 0.92f);
                GUI.DrawTexture(new Rect(cinemaRect.x - 3, cinemaRect.y - 3, cinemaRect.width + 6, cinemaRect.height + 6), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(cinemaRect, animeScene, ScaleMode.ScaleAndCrop);
                GUI.color = prev;
            }

            Rect panel = GetDialoguePanelViewportRect(w, h);
            GUI.Box(panel,GUIContent.none);
            Texture2D portraitTex = ResolveSpeakerPortrait(line.Speaker);
            if (portraitTex != null)
            {
                GUI.DrawTexture(new Rect(panel.x + 12, panel.y + 10, 88, 88), portraitTex, ScaleMode.ScaleToFit);
            }
            else
            {
                Color previous=GUI.color;GUI.color=line.PortraitColor;GUI.Box(new Rect(panel.x+12,panel.y+12,70,70),line.Speaker.Length>0?line.Speaker.Substring(0,1):"?");GUI.color=previous;
            }
            GUI.Label(new Rect(panel.x+110,panel.y+8,panel.width-126,24),line.Speaker);
            GUIStyle style=new GUIStyle(GUI.skin.label){wordWrap=true,fontSize=Mathf.Clamp(Mathf.RoundToInt(h/35f),14,26)};
            if (captionsEnabled || line.Voice == null || requiresPolicy) GUI.Label(new Rect(panel.x+110,panel.y+35,panel.width-126,panel.height-78),text.Substring(0,Mathf.Min(text.Length,Mathf.FloorToInt(revealed))),style);
            if(GUI.Button(new Rect(panel.xMax-155,panel.yMax-35,140,27),english?"Continue":"Continuar"))Advance();
        }
        private static Texture2D ResolveAnimeCutsceneTexture(string cueId, string speaker)
        {
            string s = ((cueId ?? string.Empty) + " " + (speaker ?? string.Empty)).ToLowerInvariant();
            bool isClimax = s.Contains("jinu") || s.Contains("gwi") || s.Contains("saja") || s.Contains("boss") || s.Contains("final") || s.Contains("honmoon");
            string path = isClimax ? "Art/Cutscenes/Anime_02_ClimaxShowdown" : "Art/Cutscenes/Anime_01_AirplaneFight";
            return UnityEngine.Resources.Load<Texture2D>(path);
        }
        private static Texture2D ResolveSpeakerPortrait(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return null;
            string s = speaker.ToLowerInvariant();
            string key = s.Contains("mira") ? "Mira" : s.Contains("zoey") ? "Zoey" : s.Contains("jinu") ? "Jinu" : s.Contains("gwi") ? "GwiMa" : "Rumi";
            Texture2D framedPortrait = UnityEngine.Resources.Load<Texture2D>("Art/Portraits/" + key + "_Portrait");
            if (framedPortrait != null) return framedPortrait;
            return UnityEngine.Resources.Load<Texture2D>("Art/Characters/" + key);
        }
        private void OnEnable(){requiresPolicy=requiresPolicy||!ReferenceEquals(narrativePolicy,null);policy=narrativePolicy as INarrativePresentationPolicy;if(policy!=null)policy.PolicyChanged+=HandlePolicyChanged;}
        private void OnDisable(){UnbindPolicy();Cancel();}
    }
}
