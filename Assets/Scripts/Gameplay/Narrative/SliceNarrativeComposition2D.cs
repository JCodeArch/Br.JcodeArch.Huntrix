using HuntrX.Gameplay.Audio;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Gameplay.Narrative
{
    [DefaultExecutionOrder(-200), DisallowMultipleComponent]
    public sealed class SliceNarrativeComposition2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private GameObject narrativePrefab;
        [SerializeField] private SliceAudioComposition2D audio;
        [SerializeField] private MonoBehaviour narrativePolicy;
        [SerializeField] private bool requiresPolicy;
        private GameObject owned;
        private bool creating;
        public event System.Action PresentationChanged;
        private void NotifyPresentationChanged()
        {
            var handlers = PresentationChanged;
            if (handlers != null) foreach (System.Action handler in handlers.GetInvocationList())
                try { handler(); } catch (System.Exception error) { Debug.LogException(error, this); }
        }
        public DialoguePresenter2D Presenter => isActiveAndEnabled && owned != null && owned.activeInHierarchy ? owned.GetComponent<DialoguePresenter2D>() : null;
        public bool Configure(VerticalSliceSession2D owner,GameObject prefab,SliceAudioComposition2D sound=null,MonoBehaviour policy=null)
        {if(!ReferenceEquals(policy,null)&&!(policy is INarrativePresentationPolicy)||creating||owned!=null||owner==null||prefab==null||prefab.scene.IsValid()||prefab.GetComponent<DialoguePresenter2D>()==null||prefab.GetComponent<StageNarrativeTransitions2D>()==null)return false;session=owner;narrativePrefab=prefab;audio=sound;narrativePolicy=policy;requiresPolicy=!ReferenceEquals(policy,null);return true;}
        private void Update()
        {
            if(session==null||!session.IsRunning){Clear();return;}
            if(owned==null&&!creating&&narrativePrefab!=null)
            {
                creating=true;
                try
                {
                    GameObject candidate=Instantiate(narrativePrefab);candidate.SetActive(true);
                    var presenter=candidate.GetComponent<DialoguePresenter2D>();
                    bool policyValid=!requiresPolicy||narrativePolicy!=null&&presenter.BindPolicy(narrativePolicy);
                    if(!isActiveAndEnabled||session==null||!session.IsRunning||!policyValid||!candidate.GetComponent<StageNarrativeTransitions2D>().Configure(session,presenter))
                    {candidate.SetActive(false);Destroy(candidate);return;}
                    if(!isActiveAndEnabled||session==null||!session.IsRunning){candidate.SetActive(false);Destroy(candidate);return;}
                    owned=candidate;
                    presenter.PresentationChanged += NotifyPresentationChanged;
                    NotifyPresentationChanged();
                }
                finally{creating=false;}
            }
            if(owned!=null&&audio!=null)owned.GetComponent<DialoguePresenter2D>().SetAudio(audio.AudioManager);
        }
        public void Clear(){GameObject value=owned;owned=null;if(value!=null){value.GetComponent<DialoguePresenter2D>().PresentationChanged-=NotifyPresentationChanged;value.SetActive(false);Destroy(value);NotifyPresentationChanged();}}
        private void OnEnable(){requiresPolicy=requiresPolicy||!ReferenceEquals(narrativePolicy,null);}
        private void OnDisable(){Clear();}
    }
}
