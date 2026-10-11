using HuntrX.Gameplay.Performance;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Special;
using UnityEngine;
namespace HuntrX.Gameplay.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioStateBridge2D : MonoBehaviour
    {
        private GameAudioManager2D audioManager;
        private CombatPerformanceController2D performance;
        private HonmoonController2D honmoon;
        private HuntrXSpecialController2D special;
        private bool lastRunning;
        private void Update(){bool running=performance!=null&&performance.IsRunning;if(running!=lastRunning)Refresh();}
        public bool Configure(GameAudioManager2D audio,CombatPerformanceController2D metrics,HonmoonController2D resource,HuntrXSpecialController2D union)
        {if(audio==null||metrics==null||resource==null||union==null)return false;Unbind();audioManager=audio;performance=metrics;honmoon=resource;special=union;if(isActiveAndEnabled)Bind();return true;}
        private void Bind(){performance.SnapshotChanged+=HandleMetrics;honmoon.CrisisChanged+=HandleCrisis;special.StageChanged+=HandleStage;special.Completed+=HandleCompleted;Refresh();}
        private void Unbind(){if(performance!=null)performance.SnapshotChanged-=HandleMetrics;if(honmoon!=null)honmoon.CrisisChanged-=HandleCrisis;if(special!=null){special.StageChanged-=HandleStage;special.Completed-=HandleCompleted;}}
        private void HandleMetrics(PerformanceSnapshot value)=>Refresh();
        private void HandleCrisis(bool value)=>Refresh();
        private void HandleStage(SpecialStage2D value)=>Refresh();
        private void HandleCompleted(bool cancelled)=>Refresh();
        private void Refresh()
        {
            if(audioManager==null)return;bool crisis=honmoon!=null&&honmoon.IsInCrisis;bool active=special!=null&&special.IsActive;
            bool combat=performance!=null&&performance.IsRunning;lastRunning=combat;int combo=performance==null?0:performance.Snapshot.Combo;
            audioManager.SetLayer(AudioLayer.Exploration,combat||crisis||active ? .15f : 1f);
            audioManager.SetLayer(AudioLayer.Combat,combat&&!active? .7f:0f);
            audioManager.SetLayer(AudioLayer.Combo,combat&&!crisis&&!active?Mathf.Clamp01(combo/8f):0f);
            audioManager.SetLayer(AudioLayer.Crisis,crisis&&!active?1f:0f);audioManager.SetLayer(AudioLayer.Special,active?1f:0f);
        }
        private void OnEnable(){if(audioManager!=null)Bind();}
        private void OnDisable(){Unbind();if(audioManager!=null){audioManager.SetLayer(AudioLayer.Combat,0f);audioManager.SetLayer(AudioLayer.Combo,0f);audioManager.SetLayer(AudioLayer.Crisis,0f);audioManager.SetLayer(AudioLayer.Special,0f);audioManager.SetLayer(AudioLayer.Exploration,1f);}}
    }
}
