using System;
using HuntrX.Data;
using HuntrX.Gameplay.BossFinal;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    [DisallowMultipleComponent]
    public sealed class FinalBossEndingController2D : MonoBehaviour
    {
        [SerializeField] private GwiMaBossController2D boss;
        [SerializeField] private NarrativeSequenceDefinition ending;
        [SerializeField] private MonoBehaviour narrativePolicy;
        [SerializeField] private bool requiresPolicy;
        public bool BindPolicy(MonoBehaviour value){if(received||!ReferenceEquals(value,null)&&!(value is INarrativePresentationPolicy))return false;narrativePolicy=value;requiresPolicy=!ReferenceEquals(value,null);return true;}
        private bool received,complete;
        private EndingPresentation2D presentation;
        public bool IsComplete=>complete;
        public event Action<FinalBossOutcome2D> EndingCompleted;
        private void HandleOutcome(FinalBossOutcome2D value)
        {
            if(!isActiveAndEnabled||received||complete||boss==null||!boss.TryGetFinalOutcome(out var accepted)||
                value.BossId!=accepted.BossId||value.EndingCue!=accepted.EndingCue||value.Position!=accepted.Position||
                requiresPolicy&&narrativePolicy==null||ending==null||!ending.IsValid(out _)||value.EndingCue!=ending.CueId)return;
            // Reserve before allocation and callbacks. The narrative root intentionally outlives this actor.
            received=true;GameObject holder=new GameObject("Final outcome presentation");
            holder.AddComponent<DialoguePresenter2D>();presentation=holder.AddComponent<EndingPresentation2D>();
            if(!presentation.TryBegin(ending,accepted,narrativePolicy)){holder.SetActive(false);Destroy(holder);presentation=null;received=false;return;}
            presentation.Completed+=HandleCompleted;
        }
        private void HandleCompleted(FinalBossOutcome2D value)
        {
            if(this==null||complete)return;complete=true;
            var handlers=EndingCompleted;if(handlers!=null)foreach(Action<FinalBossOutcome2D> handler in handlers.GetInvocationList())try{handler(value);}catch(Exception error){Debug.LogException(error,this);}
        }
        private void OnEnable(){requiresPolicy=requiresPolicy||!ReferenceEquals(narrativePolicy,null);if(boss!=null){boss.EndingRequested+=HandleOutcome;if(boss.TryGetFinalOutcome(out var outcome))HandleOutcome(outcome);}}
        private void OnDisable(){if(boss!=null)boss.EndingRequested-=HandleOutcome;}
        private void OnDestroy(){if(presentation!=null)presentation.Completed-=HandleCompleted;}
    }
}
