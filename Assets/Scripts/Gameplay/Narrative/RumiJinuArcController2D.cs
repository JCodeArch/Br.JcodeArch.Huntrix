using System;
using HuntrX.Data;
using HuntrX.Gameplay.Rivals;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    public enum RumiJinuArcBeat { Distrust, Cooperation, Resolve }
    /// <summary>Authored original bridge scenes; progression is explicit and monotonic.</summary>
    [DisallowMultipleComponent]
    public sealed class RumiJinuArcController2D : MonoBehaviour
    {
        [SerializeField] private DialoguePresenter2D presenter;
        [SerializeField] private JinuEncounterController2D jinu;
        private int requested=-1;
        [SerializeField] private NarrativeSequenceDefinition distrust,cooperation,resolve;
        private int completed=-1;
        private int pending=-1;
        public RumiJinuArcBeat Current=>(RumiJinuArcBeat)Mathf.Max(0,completed);
        public event Action<RumiJinuArcBeat> BeatPresented;
        public bool TryPresentBeat(RumiJinuArcBeat beat)
        {
            int index=(int)beat;if(!isActiveAndEnabled||presenter==null||index<0||index>2||index!=completed+1||pending>=0)return false;
            NarrativeSequenceDefinition cue=index==0?distrust:index==1?cooperation:resolve;
            pending=index;if(!presenter.TryPresent(cue)){pending=-1;return false;}return true;
        }
        private void HandleCompleted(NarrativeSequenceDefinition cue)
        {
            if(pending<0||cue!=(pending==0?distrust:pending==1?cooperation:resolve))return;
            completed=pending;pending=-1;var handlers=BeatPresented;
            if(handlers!=null)foreach(Action<RumiJinuArcBeat> handler in handlers.GetInvocationList())try{handler((RumiJinuArcBeat)completed);}catch(Exception error){Debug.LogException(error,this);}
        }
        private void HandleCancelled(NarrativeSequenceDefinition cue){if(pending>=0&&cue==(pending==0?distrust:pending==1?cooperation:resolve))pending=-1;}
        private void HandleJinu(JinuEncounterStage stage,string cue)
        {if(stage!=JinuEncounterStage.Uninitialized)requested=Mathf.Max(requested,stage==JinuEncounterStage.Encounter?0:stage==JinuEncounterStage.Pressure?1:2);}
        private void Update(){if(requested>completed&&pending<0&&presenter!=null&&!presenter.IsPresenting)TryPresentBeat((RumiJinuArcBeat)(completed+1));}
        private void OnEnable(){if(presenter!=null){presenter.SequenceCompleted+=HandleCompleted;presenter.SequenceCancelled+=HandleCancelled;}if(jinu!=null){jinu.NarrativeCueRequested+=HandleJinu;HandleJinu(jinu.Stage,null);}}
        private void OnDisable(){if(presenter!=null){presenter.SequenceCompleted-=HandleCompleted;presenter.SequenceCancelled-=HandleCancelled;}if(jinu!=null)jinu.NarrativeCueRequested-=HandleJinu;pending=-1;}
    }
}
