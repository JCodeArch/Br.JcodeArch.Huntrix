using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationBinder2D : MonoBehaviour
    {
        [SerializeField] private CharacterArtBinder2D artBinder;
        [SerializeField] private CharacterRigDefinition[] rigs;
        private CharacterArtRenderer2D current;
        public bool TryConfigure(CharacterArtBinder2D binder,CharacterRigDefinition[] profiles)
        {
            if(!isActiveAndEnabled||binder==null||profiles==null||profiles.Length<1||profiles.Length>16)return false;
            for(int i=0;i<profiles.Length;i++){if(profiles[i]==null||!profiles[i].IsValid)return false;for(int j=0;j<i;j++)if(profiles[i].Character==profiles[j].Character)return false;}
            artBinder=binder;rigs=(CharacterRigDefinition[])profiles.Clone();current=null;return true;
        }
        private void LateUpdate()
        {
            if(artBinder==null||rigs==null||rigs.Length<1||rigs.Length>16)return;
            CharacterArtRenderer2D visual=artBinder.CurrentRenderer;if(visual==null||visual==current)return;
            for(int i=0;i<rigs.Length;i++)if(rigs[i]!=null&&rigs[i].IsValid&&rigs[i].Character==visual.Definition)
            {
                CharacterGameplayAnimator2D animator=visual.GetComponent<CharacterGameplayAnimator2D>();if(animator==null)animator=visual.gameObject.AddComponent<CharacterGameplayAnimator2D>();animator.enabled=true;
                if(animator.TryConfigure(visual,rigs[i]))current=visual;return;
            }
        }
        private void OnDisable(){current=null;}
    }
}
