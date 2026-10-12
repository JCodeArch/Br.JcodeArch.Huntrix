using HuntrX.Data;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    [DisallowMultipleComponent]
    public sealed class StageNarrativeTransitions2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private DialoguePresenter2D presenter;
        [SerializeField] private NarrativeSequenceDefinition[] beatCues=new NarrativeSequenceDefinition[7];
        private bool[] played;
        private float fade;
        private int visualBeat=-1;
        private void Awake(){played=new bool[beatCues==null||beatCues.Length>16?0:beatCues.Length];}
        public bool Configure(VerticalSliceSession2D owner,DialoguePresenter2D dialogue)
        {if(owner==null||dialogue==null||beatCues==null||beatCues.Length>16)return false;Unbind();System.Array.Clear(played,0,played.Length);visualBeat=-1;session=owner;presenter=dialogue;if(isActiveAndEnabled)Bind();return true;}
        private void Bind(){session.BeatChanged+=HandleBeat;presenter.SequenceCancelled+=HandleCancelled;if(session.BeatIndex>=0)HandleBeat(session.BeatIndex);}
        private void Unbind(){if(session!=null)session.BeatChanged-=HandleBeat;if(presenter!=null)presenter.SequenceCancelled-=HandleCancelled;}
        private void HandleCancelled(NarrativeSequenceDefinition cue){for(int i=0;i<played.Length;i++)if(beatCues[i]==cue)played[i]=false;}
        private void HandleBeat(int index)
        {
            if(index<0||index>=played.Length||played[index])return;if(visualBeat!=index){visualBeat=index;fade=1f;}
            NarrativeSequenceDefinition cue=beatCues[index];if(cue==null){played[index]=true;return;}
            // An occupied presenter retains its current sequence. Retry the new cue on Update.
            if(presenter!=null&&presenter.TryPresent(cue))played[index]=true;
        }
        private void Update(){fade=Mathf.MoveTowards(fade,0f,Time.unscaledDeltaTime*2f);if(session!=null&&session.IsRunning&&presenter!=null&&!presenter.IsPresenting)HandleBeat(session.BeatIndex);}
        private void OnGUI(){if(fade<=0f)return;Color old=GUI.color;GUI.color=new Color(0,0,0,fade);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=old;}
        private void OnEnable(){if(session!=null&&presenter!=null)Bind();}
        private void OnDisable(){Unbind();fade=0f;}
    }
}
