using System;
using HuntrX.Gameplay.Performance;
namespace HuntrX.Gameplay.Replay
{
    [Serializable]
    public sealed class LocalReplayRecord
    {
        public string recordId;
        public string profileId;
        public string chapterId;
        public int rescuedFans;
        public float performancePoints;
        public float performanceNormalized;
        public float elapsedSeconds;
        public int maximumCombo;
        public bool noDamage;
        public long completedUtcTicks;
        public static LocalReplayRecord Create(string profile,string chapter,PerformanceSnapshot snapshot)=>new LocalReplayRecord
        {
            recordId=Guid.NewGuid().ToString("N"),profileId=profile,chapterId=chapter,
            rescuedFans=snapshot.RescuedFans,performancePoints=snapshot.Points,performanceNormalized=snapshot.Normalized,
            elapsedSeconds=snapshot.ElapsedSeconds,maximumCombo=snapshot.MaximumCombo,noDamage=snapshot.NoDamage,
            completedUtcTicks=DateTime.UtcNow.Ticks
        };
        public bool IsValid(string profile)=>ValidProfileId(profile)&&profileId==profile&&ValidProfileId(recordId)&&
            !string.IsNullOrWhiteSpace(chapterId)&&chapterId.Length<=128&&rescuedFans>=0&&rescuedFans<=1024&&
            Finite(performancePoints)&&performancePoints>=0f&&performancePoints<=1000000f&&
            Finite(performanceNormalized)&&performanceNormalized>=0f&&performanceNormalized<=1f&&
            Finite(elapsedSeconds)&&elapsedSeconds>=0f&&elapsedSeconds<=86400f&&maximumCombo>=0&&maximumCombo<=10000&&
            completedUtcTicks>=DateTime.MinValue.Ticks&&completedUtcTicks<=DateTime.MaxValue.Ticks;
        public static bool ValidProfileId(string id)
        {
            if(id==null||id.Length!=32)return false;
            for(int i=0;i<id.Length;i++)if(!(id[i]>='0'&&id[i]<='9')&&!(id[i]>='a'&&id[i]<='f'))return false;
            return true;
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        public LocalReplayRecord Copy()=>(LocalReplayRecord)MemberwiseClone();
    }
    public interface IReplayChapterAccess
    {
        bool IsReplayAllowed(string opaqueProfileId,string chapterId);
    }
}
