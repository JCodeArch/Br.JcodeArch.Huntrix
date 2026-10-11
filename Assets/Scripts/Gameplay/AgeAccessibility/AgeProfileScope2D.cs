using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AgeProfileScope2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private HordeController2D horde;
        public bool TryBind(CharacterManager2D characterManager,HordeController2D encounter)
        {
            if(applying || characterManager==null) return false;
            manager=characterManager; horde=encounter; ApplyScope(); return true;
        }
        private void OnEnable() { if(profiles!=null) profiles.ProfileChanged+=OnProfileChanged; }
        private void OnDisable() { if(profiles!=null) profiles.ProfileChanged-=OnProfileChanged; }
        private void OnProfileChanged(HuntrX.Data.AgeProfileData profile)=>ApplyScope();
        private void Update()=>ApplyScope();
        private bool applying;
        private void ApplyScope()
        {
            if(applying || !isActiveAndEnabled || profiles==null || !profiles.isActiveAndEnabled || profiles.Current==null || !profiles.Current.IsValid(out _)) return;
            applying=true; profiles.BeginPropagation();
            try
            {
            var actor=manager!=null?manager.ActiveCharacter:null;
            if(actor!=null) Apply(actor.GetComponent<AgeProfileSelection2D>());
            if(horde!=null && horde.isActiveAndEnabled)
                for(int i=0;isActiveAndEnabled && profiles.isActiveAndEnabled && i<Mathf.Min(64,horde.AliveCount);i++)
                {
                    var enemy=horde.GetOwnedAt(i);
                    if(enemy!=null && enemy.isActiveAndEnabled) Apply(enemy.GetComponent<AgeProfileSelection2D>());
                }
            }
            finally { profiles.EndPropagation(); applying=false; }
        }
        private void Apply(AgeProfileSelection2D actorProfile)
        {
            if(actorProfile!=null && actorProfile.isActiveAndEnabled && actorProfile!=profiles) actorProfile.TrySelect(profiles.Current);
        }
    }
}
