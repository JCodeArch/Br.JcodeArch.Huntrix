using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.FanEncounters;
using HuntrX.Gameplay.Honmoon;
using UnityEngine;
namespace HuntrX.Gameplay.Audio
{
    [DisallowMultipleComponent]
    public sealed class CombatAudioBridge2D : MonoBehaviour
    {
        private GameAudioManager2D audioManager;
        private CharacterManager2D manager;
        private HonmoonController2D honmoon;
        private FanRescueController2D[] rescues;
        private FanEncounterController2D encounter;
        private DamageReceiver2D actor;
        private AttackController2D attack;
        private DashController2D dash;
        private ZoeyRangedAttack2D ranged;
        private ParryController2D parry;
        private AttackState2D previousAttack;
        private bool previousParry;
        public bool Configure(GameAudioManager2D audio,CharacterManager2D slot,HonmoonController2D resource,FanRescueController2D[] sources)
        {
            if(audio==null||slot==null||resource==null||sources==null||sources.Length>64)return false;
            for(int i=0;i<sources.Length;i++){if(sources[i]==null)return false;for(int j=0;j<i;j++)if(sources[i]==sources[j])return false;}
            Unbind();audioManager=audio;manager=slot;honmoon=resource;rescues=(FanRescueController2D[])sources.Clone();if(isActiveAndEnabled)Bind();return true;
        }
        public void BindEncounter(FanEncounterController2D value){if(encounter!=null)encounter.RescueCompleted-=HandleRescue;encounter=value;if(isActiveAndEnabled&&encounter!=null)encounter.RescueCompleted+=HandleRescue;}
        private void Bind(){if(encounter!=null)encounter.RescueCompleted+=HandleRescue;manager.ActiveCharacterChanged+=HandleActor;honmoon.BandChanged+=HandleBand;foreach(var rescue in rescues)rescue.RescueCompleted+=HandleRescue;HandleActor(null,manager.ActiveCharacter);}
        private void Unbind(){if(encounter!=null)encounter.RescueCompleted-=HandleRescue;DetachActor();if(manager!=null)manager.ActiveCharacterChanged-=HandleActor;if(honmoon!=null)honmoon.BandChanged-=HandleBand;if(rescues!=null)foreach(var rescue in rescues)if(rescue!=null)rescue.RescueCompleted-=HandleRescue;}
        private void DetachActor(){if(actor!=null){actor.DamageDealt-=HandleHit;actor.DamageReceived-=HandleReceived;}if(dash!=null)dash.DashStateChanged-=HandleDash;if(ranged!=null)ranged.ShotFired-=HandleShot;actor=null;attack=null;dash=null;ranged=null;parry=null;}
        private void HandleActor(DamageReceiver2D previous,DamageReceiver2D current)
        {DetachActor();actor=current;if(actor==null)return;actor.DamageDealt+=HandleHit;actor.DamageReceived+=HandleReceived;attack=actor.GetComponent<AttackController2D>();dash=actor.GetComponent<DashController2D>();ranged=actor.GetComponent<ZoeyRangedAttack2D>();parry=actor.GetComponent<ParryController2D>();previousAttack=attack==null?AttackState2D.Idle:attack.State;previousParry=parry!=null&&parry.IsWindowActive;if(dash!=null)dash.DashStateChanged+=HandleDash;if(ranged!=null)ranged.ShotFired+=HandleShot;}
        private void Play(AudioCue cue){if(audioManager!=null)audioManager.TryPlay(cue,actor==null?transform.position:actor.transform.position);}
        private void HandleHit(CombatImpactEvent value)=>Play(AudioCue.Hit);
        private void HandleReceived(CombatImpactEvent value)=>Play(AudioCue.Enemy);
        private void HandleDash(bool active){if(active)Play(AudioCue.Dash);}
        private void HandleShot(Vector2 origin,Vector2 direction)=>Play(AudioCue.Attack);
        private void HandleBand(HonmoonBand value)=>Play(AudioCue.Honmoon);
        private void HandleRescue(FanActor2D fan,int amount)=>Play(AudioCue.Rescue);
        private void Update()
        {
            if(attack!=null){if(attack.State!=previousAttack&&attack.State==AttackState2D.Startup)Play(AudioCue.Attack);previousAttack=attack.State;}
            bool window=parry!=null&&parry.IsWindowActive;if(window&&!previousParry)Play(AudioCue.Parry);previousParry=window;
        }
        private void OnEnable(){if(audioManager!=null)Bind();}
        private void OnDisable(){Unbind();}
    }
}
