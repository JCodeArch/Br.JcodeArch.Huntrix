namespace HuntrX.Gameplay.Performance
{
    public enum PerformanceBand { Calm, Energized, Spectacular }
    public enum CrowdIntensity { Quiet, Cheering, Roaring }
    public enum PerformanceMusicIntensity { Base, Layered, Full }
    public readonly struct PerformanceSnapshot
    {
        public PerformanceSnapshot(float points,float normalized,int combo,int maximumCombo,int rescuedFans,
            float elapsedSeconds,bool noDamage,PerformanceBand band)
        {
            Points=points;Normalized=normalized;Combo=combo;MaximumCombo=maximumCombo;RescuedFans=rescuedFans;
            ElapsedSeconds=elapsedSeconds;NoDamage=noDamage;Band=band;
        }
        public float Points {get;}
        public float Normalized {get;}
        public int Combo {get;}
        public int MaximumCombo {get;}
        public int RescuedFans {get;}
        public float ElapsedSeconds {get;}
        public bool NoDamage {get;}
        public PerformanceBand Band {get;}
        public CrowdIntensity Crowd=>Band==PerformanceBand.Spectacular?CrowdIntensity.Roaring:
            Band==PerformanceBand.Energized?CrowdIntensity.Cheering:CrowdIntensity.Quiet;
        public PerformanceMusicIntensity Music=>Band==PerformanceBand.Spectacular?PerformanceMusicIntensity.Full:
            Band==PerformanceBand.Energized?PerformanceMusicIntensity.Layered:PerformanceMusicIntensity.Base;
    }
}
