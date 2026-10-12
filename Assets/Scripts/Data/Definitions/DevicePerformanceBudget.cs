using UnityEngine;
namespace HuntrX.Data
{
    public enum DevicePerformanceTier { Entry, Standard, High }
    [CreateAssetMenu(fileName="DevicePerformanceBudget",menuName="HUNTR/X/Data/Device Performance Budget")]
    public sealed class DevicePerformanceBudget : ScriptableObject
    {
        public DevicePerformanceTier tier;
        public int targetFramesPerSecond=60;
        public float maximumCpuMilliseconds=10f;
        public float maximumGpuMilliseconds=12f;
        public float maximumFrameMilliseconds=16.67f;
        public long maximumMemoryBytes=536870912;
        public int warmupFrames=120;
        public int capturedFrames=1800;
        public bool IsValid()=>targetFramesPerSecond==60&&FinitePositive(maximumCpuMilliseconds)&&FinitePositive(maximumGpuMilliseconds)&&
            FinitePositive(maximumFrameMilliseconds)&&maximumFrameMilliseconds<=17f&&maximumMemoryBytes>0&&
            warmupFrames>=0&&warmupFrames<=600&&capturedFrames>=600&&capturedFrames<=3600;
        private static bool FinitePositive(float n)=>n>0f&&!float.IsNaN(n)&&!float.IsInfinity(n);
    }
}
