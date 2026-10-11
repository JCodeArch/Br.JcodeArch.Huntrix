using System;
using HuntrX.Data;
using HuntrX.Gameplay.BossFinal;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    /// <summary>Independent scene-owned handoff survives destruction of the defeated combat actor.</summary>
    [DisallowMultipleComponent,RequireComponent(typeof(DialoguePresenter2D))]
    public sealed class EndingPresentation2D : MonoBehaviour
    {
        private DialoguePresenter2D presenter;
        private NarrativeSequenceDefinition sequence;
        private FinalBossOutcome2D outcome;
        private bool complete;
        public event Action<FinalBossOutcome2D> Completed;
        public bool TryBegin(NarrativeSequenceDefinition value,FinalBossOutcome2D result,MonoBehaviour policy)
        {
            if(sequence!=null||value==null||!value.IsValid(out _)||value.CueId!=result.EndingCue)return false;
            presenter=GetComponent<DialoguePresenter2D>();if(!ReferenceEquals(policy,null)&&!presenter.BindPolicy(policy))return false;
            sequence=value;outcome=result;presenter.SequenceCompleted+=HandleCompleted;return true;
        }
        private void Update(){if(!complete&&sequence!=null&&presenter!=null&&!presenter.IsPresenting)presenter.TryPresent(sequence);}
        private void HandleCompleted(NarrativeSequenceDefinition value)
        {
            if(complete||value!=sequence)return;complete=true;
            var handlers=Completed;if(handlers!=null)foreach(Action<FinalBossOutcome2D> handler in handlers.GetInvocationList())try{handler(outcome);}catch(Exception error){Debug.LogException(error,this);}
        }
        private void OnGUI()
        {
            if(!complete)return;
            GUI.Box(new Rect(Screen.width*.2f,Screen.height*.25f,Screen.width*.6f,Screen.height*.28f),"Fim do protótipo\nHUNTR/X — união, coragem e um novo começo\nTexto e sons originais de protótipo. Produção e direitos pendentes.");
            if(GUI.Button(new Rect(Screen.width*.4f,Screen.height*.57f,Screen.width*.2f,35),"Fechar créditos"))Destroy(gameObject);
        }
        private void OnDisable(){if(presenter!=null){presenter.SequenceCompleted-=HandleCompleted;if(!complete&&presenter.Current==sequence)presenter.Cancel();}}
        private void OnEnable(){if(presenter!=null)presenter.SequenceCompleted+=HandleCompleted;}
    }
}
