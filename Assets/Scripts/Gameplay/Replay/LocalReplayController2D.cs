using System;
using System.IO;
using HuntrX.Gameplay.Performance;
using UnityEngine;
namespace HuntrX.Gameplay.Replay
{
    [DisallowMultipleComponent]
    public sealed class LocalReplayController2D : MonoBehaviour
    {
        [SerializeField] private CombatPerformanceController2D performance;
        private LocalReplayRecordStore store;
        private IReplayChapterAccess chapterAccess;
        private string profileId,chapterId;
        private bool recording;
        private bool transitioning;
        private LocalReplayRecord pending;
        public bool IsRecording=>recording;
        public bool HasUnsavedRecord=>pending!=null;
        public event Action<LocalReplayRecord> RunSaved;
        public event Action<string> ReplayRequested;
        public bool Configure(string opaqueProfile,string chapter,CombatPerformanceController2D metrics,
            IReplayChapterAccess eligibility,LocalReplayRecordStore persistence=null)
        {
            if(recording||transitioning||pending!=null||!LocalReplayRecord.ValidProfileId(opaqueProfile)||
                string.IsNullOrWhiteSpace(chapter)||chapter.Length>128||metrics==null)return false;
            profileId=opaqueProfile;chapterId=chapter;performance=metrics;chapterAccess=eligibility;
            store=persistence??new LocalReplayRecordStore(Path.Combine(Application.persistentDataPath,"ReplayRecords"));
            return true;
        }
        public bool TryBeginRun(bool isReplay,out string error)
        {
            error=string.Empty;
            if(!isActiveAndEnabled||recording||transitioning||pending!=null||store==null||performance==null||!performance.isActiveAndEnabled)
            {error="A configured enabled recorder with no pending run is required.";return false;}
            transitioning=true;
            try
            {
                if(isReplay&&!CanReplay(out error))return false;
                if(!isActiveAndEnabled){error="Recorder was disabled during authorization.";return false;}
                if(!performance.TryBeginRun()){error="Performance run could not begin.";return false;}
                if(!isActiveAndEnabled||!performance.IsRunning){performance.ResetRun();error="Run was cancelled during initialization.";return false;}
                recording=true;
                return recording&&isActiveAndEnabled;
            }
            finally{transitioning=false;}
        }
        public bool TryRequestReplay(out string error)
        {
            error=string.Empty;
            if(!isActiveAndEnabled||recording||transitioning||pending!=null||store==null)
            {error="A configured idle recorder is required to request replay.";return false;}
            transitioning=true;
            try
            {
                if(!CanReplay(out error))return false;
                if(!isActiveAndEnabled){error="Recorder was disabled during authorization.";return false;}
                PublishReplay();return true;
            }
            finally{transitioning=false;}
        }
        private bool CanReplay(out string error)
        {
            error=string.Empty;
            try
            {
                if(chapterAccess!=null&&chapterAccess.IsReplayAllowed(profileId,chapterId))return true;
                error="Replay requires explicit unlocked/completed chapter authorization.";return false;
            }
            catch(Exception){error="Chapter replay authorization failed.";return false;}
        }
        public bool TryCompleteRun(out string error)
        {
            error=string.Empty;
            if(!isActiveAndEnabled||!recording||transitioning||performance==null||!performance.TryFinishRun(out PerformanceSnapshot result))
            {error="No active run can be completed.";return false;}
            recording=false;pending=LocalReplayRecord.Create(profileId,chapterId,result);
            return TrySavePending(out error);
        }
        public bool TrySavePending(out string error)
        {
            error=string.Empty;
            if(!isActiveAndEnabled||transitioning||store==null||pending==null){error="No pending run summary is available.";return false;}
            transitioning=true;
            try
            {
                if(!store.TryAppend(pending,out error))return false;
                LocalReplayRecord saved=pending;pending=null;PublishSaved(saved);return true;
            }
            finally{transitioning=false;}
        }
        public bool TryReadRankings(out LocalReplayRecord[] records,out string error)
        {
            records=new LocalReplayRecord[0];error=string.Empty;
            if(store==null){error="Replay storage is not configured.";return false;}
            return store.TryReadRankings(profileId,chapterId,out records,out error);
        }
        public bool TryResetLocalRecords(out string error)
        {
            error=string.Empty;
            if(recording||transitioning||pending!=null||store==null){error="Finish or explicitly discard the current run before resetting local records.";return false;}
            return store.TryReset(profileId,out error);
        }
        public void DiscardUnsavedRun()
        {
            if(transitioning)return;
            recording=false;pending=null;if(performance!=null){performance.CancelRun();performance.ResetRun();}
        }
        private void OnDisable()
        {
            recording=false;if(performance!=null&&performance.IsRunning)performance.CancelRun();
            // Preserve a failed completed summary for an explicit save retry after re-enable.
        }
        private void PublishReplay()
        {
            Action<string> handlers=ReplayRequested;if(handlers==null)return;
            string requestedChapter=chapterId;
            foreach(Action<string> handler in handlers.GetInvocationList())
                try{handler(requestedChapter);}catch(Exception e){Debug.LogException(e,this);}
        }
        private void PublishSaved(LocalReplayRecord record)
        {
            Action<LocalReplayRecord> handlers=RunSaved;if(handlers==null)return;
            foreach(Action<LocalReplayRecord> handler in handlers.GetInvocationList())
                try{handler(record.Copy());}catch(Exception e){Debug.LogException(e,this);}
        }
    }
}
