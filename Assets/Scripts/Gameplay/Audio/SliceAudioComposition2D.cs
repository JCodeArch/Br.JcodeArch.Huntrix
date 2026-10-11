using System;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Gameplay.Audio
{
    [DisallowMultipleComponent]
    public sealed class SliceAudioComposition2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private GameObject audioPrefab;
        private GameObject owned;
        private bool creating;
        public GameAudioManager2D AudioManager {get;private set;}
        public bool Configure(VerticalSliceSession2D owner,GameObject prefab)
        {if(creating||owned!=null||owner==null||prefab==null||prefab.scene.IsValid()||prefab.GetComponent<GameAudioManager2D>()==null)return false;session=owner;audioPrefab=prefab;return true;}
        private void Update()
        {
            if(session==null||!session.IsRunning){Clear();return;}
            if(owned!=null||creating||session.Manager==null||session.Performance==null||session.Honmoon==null||session.Special==null||audioPrefab==null)return;
            creating=true;GameObject candidate=null;
            try
            {
                candidate=Instantiate(audioPrefab);candidate.SetActive(true);
                if(!isActiveAndEnabled||session==null||!session.IsRunning){candidate.SetActive(false);Destroy(candidate);return;}
                AudioManager=candidate.GetComponent<GameAudioManager2D>();
                bool valid=candidate.AddComponent<AudioStateBridge2D>().Configure(AudioManager,session.Performance,session.Honmoon,session.Special);
                var combat=candidate.AddComponent<CombatAudioBridge2D>();valid&=combat.Configure(AudioManager,session.Manager,session.Honmoon,Array.Empty<FanRescueController2D>());combat.BindEncounter(session.Fans);
                var crowd=candidate.AddComponent<CrowdAudioBridge2D>();valid&=crowd.Configure(AudioManager,session.Performance);
                if(!valid){AudioManager=null;candidate.SetActive(false);Destroy(candidate);return;}
                owned=candidate;session.BeatChanged+=HandleBeat;HandleBeat(session.BeatIndex);
            }
            finally{creating=false;}
        }
        private void HandleBeat(int index){if(owned!=null&&index==6)owned.GetComponent<CrowdAudioBridge2D>().OnShowArrival(.65f);}
        public void Clear(){if(session!=null)session.BeatChanged-=HandleBeat;GameObject value=owned;owned=null;AudioManager=null;if(value!=null){value.SetActive(false);Destroy(value);}}
        private void OnDisable(){Clear();}
    }
}
