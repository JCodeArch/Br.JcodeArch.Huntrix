using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Levels;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    [DisallowMultipleComponent]
    public sealed class CharacterArtBinder2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D player;
        [SerializeField] private GameObject[] characterPrefabs;
        [SerializeField] private CharacterArtDefinition[] profiles;
        private CharacterManager2D subscribed;
        private DamageReceiver2D actor;
        public CharacterArtRenderer2D CurrentRenderer { get; private set; }
        public bool TryConfigure(CharacterManager2D manager,GameObject[] prefabs,CharacterArtDefinition[] definitions)
        {
            if(!isActiveAndEnabled || manager==null || prefabs==null || definitions==null || prefabs.Length==0 || prefabs.Length>16 || prefabs.Length!=definitions.Length) return false;
            for(int i=0;i<prefabs.Length;i++) { if(prefabs[i]==null || definitions[i]==null || !definitions[i].IsValid) return false; for(int j=0;j<i;j++)if(prefabs[i]==prefabs[j])return false; }
            Unsubscribe(); player=manager; characterPrefabs=(GameObject[])prefabs.Clone();profiles=(CharacterArtDefinition[])definitions.Clone();Subscribe();Bind(null,player.ActiveCharacter); return true;
        }
        private void OnEnable() { Subscribe(); if(player!=null)Bind(null,player.ActiveCharacter); }
        private void Subscribe() { if(player==null||subscribed==player)return;Unsubscribe();subscribed=player;subscribed.ActiveCharacterChanged+=Bind; }
        private void Unsubscribe() { if(subscribed!=null)subscribed.ActiveCharacterChanged-=Bind;subscribed=null; }
        private void OnDisable() { Unsubscribe();actor=null;CurrentRenderer=null; }
        private void Bind(DamageReceiver2D previous,DamageReceiver2D current)
        {
            actor=current;CurrentRenderer=null;
            if(!isActiveAndEnabled || player==null || current==null || characterPrefabs==null || profiles==null || characterPrefabs.Length!=profiles.Length || characterPrefabs.Length==0 || characterPrefabs.Length>16)return;
            for(int i=0;i<characterPrefabs.Length;i++){if(characterPrefabs[i]==null||profiles[i]==null||!profiles[i].IsValid)return;for(int j=0;j<i;j++)if(characterPrefabs[i]==characterPrefabs[j])return;}
            for(int i=0;i<characterPrefabs.Length;i++) if(characterPrefabs[i]==player.ActivePrefab && profiles[i]!=null && profiles[i].IsValid)
            {
                CurrentRenderer=current.GetComponent<CharacterArtRenderer2D>();if(CurrentRenderer==null)CurrentRenderer=current.gameObject.AddComponent<CharacterArtRenderer2D>();
                CurrentRenderer.enabled=true;CurrentRenderer.TryConfigure(profiles[i]);HideFallback();return;
            }
        }
        private void LateUpdate() { HideFallback(); }
        private void HideFallback() { if(actor==null||CurrentRenderer==null||CurrentRenderer.VisualRoot==null)return;BlockoutPlayerSilhouette2D fallback=actor.GetComponent<BlockoutPlayerSilhouette2D>();if(fallback!=null&&fallback.enabled)fallback.enabled=false; }
    }
}
