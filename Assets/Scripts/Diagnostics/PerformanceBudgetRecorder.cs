using System;
using System.IO;
using HuntrX.Data;
using Unity.Profiling;
using UnityEngine;
namespace HuntrX.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class PerformanceBudgetRecorder : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public string tier,platform,editorVersion,version,capturedUtc,buildGuid,deviceModel,operatingSystem,processorType,graphicsDeviceName,scene,scenario;
            public int processorCount,systemMemoryMegabytes,graphicsMemoryMegabytes;
            public bool measured,cpuAvailable,gpuAvailable,memoryAvailable,withinBudget,isEditor;
            public int samples;
            public float frameP95Milliseconds,cpuP95Milliseconds,gpuP95Milliseconds;
            public long peakMemoryBytes;
        }
        [SerializeField] private DevicePerformanceBudget budget;
        [SerializeField] private string scenario;
        private ProfilerRecorder cpu,gpu,memory;
        private float[] frames,cpus,gpus;
        private int count,warmup;
        private long peakMemory;
        private bool recording;
        private bool allCountersAvailable;
        private string capturedTier,capturedScenario,capturedScene;
        private float frameLimit,cpuLimit,gpuLimit;
        private long memoryLimit;
        public Report LastReport{get;private set;}
        public bool TryBegin()
        {
            if(!isActiveAndEnabled||recording||budget==null||!budget.IsValid()||string.IsNullOrWhiteSpace(scenario)||scenario.Length>128)return false;
            capturedScenario=scenario.Trim();capturedScene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            capturedTier=budget.tier.ToString();frameLimit=budget.maximumFrameMilliseconds;cpuLimit=budget.maximumCpuMilliseconds;gpuLimit=budget.maximumGpuMilliseconds;memoryLimit=budget.maximumMemoryBytes;
            frames=new float[budget.capturedFrames];cpus=new float[frames.Length];gpus=new float[frames.Length];
            try
            {
                cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
                gpu=ProfilerRecorder.StartNew(ProfilerCategory.Render,"GPU Frame Time",1);
                memory=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory",1);
            }
            catch(Exception){Dispose();return false;}
            count=0;warmup=budget.warmupFrames;peakMemory=0;LastReport=null;allCountersAvailable=true;recording=true;return true;
        }
        private void LateUpdate()
        {
            if(!recording)return;if(warmup>0){warmup--;return;}
            frames[count]=Time.unscaledDeltaTime*1000f;
            cpus[count]=cpu.Valid?cpu.LastValue/1000000f:-1f;
            gpus[count]=gpu.Valid?gpu.LastValue/1000000f:-1f;
            allCountersAvailable&=cpu.Valid&&gpu.Valid&&memory.Valid&&cpu.LastValue>0&&gpu.LastValue>0&&memory.LastValue>0;
            if(memory.Valid)peakMemory=Math.Max(peakMemory,memory.LastValue);
            count++;if(count>=frames.Length)Finish();
        }
        public bool TryFinish(out Report report)
        {
            report=null;if(!recording||count<600)return false;Finish();report=LastReport;return true;
        }
        public bool TryWriteReport(string path)
        {
            if(LastReport==null||string.IsNullOrWhiteSpace(path))return false;
            try{string parent=Path.GetDirectoryName(Path.GetFullPath(path));Directory.CreateDirectory(parent);File.WriteAllText(path,JsonUtility.ToJson(LastReport,true));return true;}
            catch(IOException){return false;}catch(UnauthorizedAccessException){return false;}
        }
        private void Finish()
        {
            Report r=new Report
            {
                tier=capturedTier,platform=Application.platform.ToString(),editorVersion=Application.unityVersion,
                scenario=capturedScenario,scene=capturedScene,deviceModel=SystemInfo.deviceModel,operatingSystem=SystemInfo.operatingSystem,
                processorType=SystemInfo.processorType,graphicsDeviceName=SystemInfo.graphicsDeviceName,
                processorCount=SystemInfo.processorCount,systemMemoryMegabytes=SystemInfo.systemMemorySize,graphicsMemoryMegabytes=SystemInfo.graphicsMemorySize,
                buildGuid=Application.buildGUID,version=Application.version,capturedUtc=DateTime.UtcNow.ToString("O"),samples=count,
                cpuAvailable=cpu.Valid,gpuAvailable=gpu.Valid,memoryAvailable=memory.Valid,isEditor=Application.isEditor,
                frameP95Milliseconds=Percentile(frames,count),cpuP95Milliseconds=Percentile(cpus,count),
                gpuP95Milliseconds=Percentile(gpus,count),peakMemoryBytes=peakMemory
            };
            r.measured=!r.isEditor&&allCountersAvailable&&count>=600&&r.cpuAvailable&&r.gpuAvailable&&r.memoryAvailable&&
                r.cpuP95Milliseconds>0f&&r.gpuP95Milliseconds>0f&&r.peakMemoryBytes>0;
            r.withinBudget=r.measured&&r.frameP95Milliseconds<=frameLimit&&
                r.cpuP95Milliseconds<=cpuLimit&&r.gpuP95Milliseconds<=gpuLimit&&r.peakMemoryBytes<=memoryLimit;
            LastReport=r;recording=false;Dispose();
        }
        private static float Percentile(float[] values,int n)
        {
            if(n==0)return -1f;float[] copy=new float[n];Array.Copy(values,copy,n);Array.Sort(copy);return copy[Mathf.Clamp(Mathf.CeilToInt(n*.95f)-1,0,n-1)];
        }
        private void Dispose(){cpu.Dispose();gpu.Dispose();memory.Dispose();}
        private void OnDisable(){recording=false;Dispose();}
    }
}
