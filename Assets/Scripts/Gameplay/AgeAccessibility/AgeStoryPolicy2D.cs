using System;
using HuntrX.Data;
using HuntrX.Gameplay.Narrative;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [Serializable]
    public sealed class AgeStoryLine
    {
        [SerializeField] private NarrativeSequenceDefinition sequence;
        [SerializeField] private int line;
        [SerializeField] private AgeNarrativeDefinition portuguese;
        [SerializeField] private AgeNarrativeDefinition english;
        public bool Matches(NarrativeSequenceDefinition value,int index)=>sequence==value && line==index;
        public bool TryResolve(AgeProfileData profile,bool useEnglish,out string text)
        {
            var cue=useEnglish?english:portuguese;
            text=null; return cue!=null && cue.TryChoose(profile,out text);
        }
    }
    /// <summary>Unclassified story lines are rejected; changing the profile cancels the currently displayed sequence.</summary>
    [DisallowMultipleComponent]
    public sealed class AgeStoryPolicy2D : MonoBehaviour,INarrativePresentationPolicy
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private DialoguePresenter2D presenter;
        [SerializeField] private NarrativeSequenceDefinition initialSequence;
        [SerializeField] private bool autoPresent;
        [SerializeField] private AgeStoryLine[] lines=new AgeStoryLine[0];
        public event Action PolicyChanged;
        private void OnEnable() { if(profiles!=null) profiles.ProfileChanged+=Changed; }
        private void Start() { if(presenter!=null && presenter.BindPolicy(this) && autoPresent) presenter.TryPresent(initialSequence); }
        private void OnDisable() { if(profiles!=null) profiles.ProfileChanged-=Changed; PublishChanged(); }
        private void Changed(AgeProfileData profile)=>PublishChanged();
        private void PublishChanged()
        {
            var handlers=PolicyChanged;
            if(handlers!=null) foreach(Action handler in handlers.GetInvocationList())
                try { handler(); } catch(Exception error) { Debug.LogException(error,this); }
        }
        public bool TryResolve(NarrativeSequenceDefinition sequence,int line,bool english,out string text)
        {
            text=null;
            if(!isActiveAndEnabled || profiles==null || !profiles.isActiveAndEnabled || profiles.Current==null || !profiles.Current.IsValid(out _) || lines==null || lines.Length>64) return false;
            foreach(var entry in lines) if(entry!=null && entry.Matches(sequence,line)) return entry.TryResolve(profiles.Current,english,out text);
            return false;
        }
    }
}
