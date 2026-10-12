using System;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AgeNarrativePresenter2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private AgeNarrativeDefinition initialCue;
        [SerializeField] private bool autoPresent;
        public string CurrentText { get; private set; }
        public float Remaining { get; private set; }
        private bool publishing;
        public event Action<string,float> Presented;
        private void OnEnable() { if(profiles!=null) profiles.ProfileChanged+=OnProfileChanged; }
        private void OnProfileChanged(AgeProfileData profile) { CurrentText=null; Remaining=0f; }
        private void Start() { if(autoPresent) TryPresent(initialCue); }
        private void Update() { Remaining=Mathf.Max(0f,Remaining-Time.deltaTime); if(Remaining<=0f) CurrentText=null; }
        private void OnDisable() { if(profiles!=null) profiles.ProfileChanged-=OnProfileChanged; CurrentText=null; Remaining=0f; }
        public bool TryPresent(AgeNarrativeDefinition cue)
        {
            if(!isActiveAndEnabled || publishing || profiles==null || !profiles.isActiveAndEnabled || cue==null || !cue.TryChoose(profiles.Current,out var text)) return false;
            var selectedProfile=profiles.Current; CurrentText=text; Remaining=cue.Duration; publishing=true;
            try
            {
                var handlers=Presented;
                if(handlers!=null) foreach(Action<string,float> handler in handlers.GetInvocationList())
                    {
                        if(!isActiveAndEnabled || !profiles.isActiveAndEnabled || profiles.Current!=selectedProfile || CurrentText!=text) break;
                        try { handler(text,cue.Duration); } catch(Exception exception) { Debug.LogException(exception,this); }
                    }
                return true;
            }
            finally { publishing=false; }
        }
    }
}
