using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace HuntrX.Editor.Build
{
    /// <summary>Invoked through -executeMethod only; never fabricates an artifact after a failed build.</summary>
    public static class PrototypeBuildPipeline
    {
        public const string RequiredEditorVersion="6000.6.4f1";
        [Serializable] private sealed class Evidence
        {
            public string editorVersion,target,result,outputPath,artifactRoot,artifactSha256,completedUtc,sourceCommit,version,buildGuid;
            public string[] scenes;
            public ulong totalBytes;
            public int errors,warnings;
            public double durationSeconds;
        }
        public static void AndroidDevelopment()=>Run(BuildTarget.Android,false);
        public static void AndroidRelease()=>Run(BuildTarget.Android,true);
        public static void IOSProject()=>Run(BuildTarget.iOS,false);
        public static void WindowsDevelopment()=>Run(BuildTarget.StandaloneWindows64,false);
        public static void WindowsRelease()=>Run(BuildTarget.StandaloneWindows64,true);
        private static void Run(BuildTarget target,bool release)
        {
            if(Application.unityVersion!=RequiredEditorVersion)throw new InvalidOperationException("Use the project's pinned Unity editor version.");
            if(EditorUserBuildSettings.activeBuildTarget!=target)
                throw new InvalidOperationException("Launch Unity with -buildTarget matching the build method before compilation.");
            if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target))
                throw new InvalidOperationException("The required platform build module is unavailable.");
            string output=Arg("-huntrxOutput");
            if(string.IsNullOrWhiteSpace(output))throw new InvalidOperationException("Provide -huntrxOutput with a new build destination.");
            output=Path.GetFullPath(output);
            if(File.Exists(output)||Directory.Exists(output))throw new InvalidOperationException("Build output must not already exist; stale artifacts cannot masquerade as this build.");
            string[] scenes=Arg("-huntrxScenes")?.Split(';');
            if(scenes==null||scenes.Length==0||scenes.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Provide explicit -huntrxScenes Assets/Scenes/...unity entries; no sample-scene default.");
            foreach(string scene in scenes)
                if(!scene.StartsWith("Assets/Scenes/",StringComparison.Ordinal)||scene.Contains("..")||
                    !scene.EndsWith(".unity",StringComparison.Ordinal)||AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)==null)
                    throw new InvalidOperationException("The build scene list contains an invalid or missing scene asset.");
            string parent=Path.GetDirectoryName(output);if(!string.IsNullOrEmpty(parent))Directory.CreateDirectory(parent);
            if(target==BuildTarget.StandaloneWindows64&&Directory.EnumerateFiles(parent,"*",SearchOption.AllDirectories).Any(file=>file!=output+".unity.log"))
                throw new InvalidOperationException("Windows artifact folder must be fresh, apart from this invocation's Unity log.");
            string version=File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"VERSION")).Trim();
            Match semver=Regex.Match(version,@"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$");
            if(!semver.Success||semver.Groups[4].Value.Split('.').Any(part=>part.Length>1&&part[0]=='0'&&part.All(c=>c>='0'&&c<='9')))throw new InvalidOperationException("VERSION must contain semantic versioning.");
            PlayerSettings.bundleVersion=target==BuildTarget.Android?version:semver.Groups[1].Value+"."+semver.Groups[2].Value+"."+semver.Groups[3].Value;
            if(target==BuildTarget.Android||target==BuildTarget.iOS)
            {
                if(!int.TryParse(Environment.GetEnvironmentVariable("HUNTRX_BUILD_NUMBER"),out int buildNumber)||buildNumber<=0||buildNumber>2100000000)
                    throw new InvalidOperationException("Provide a positive explicit HUNTRX_BUILD_NUMBER for platform build codes.");
                if(target==BuildTarget.Android)PlayerSettings.Android.bundleVersionCode=buildNumber;
                else PlayerSettings.iOS.buildNumber=buildNumber.ToString();
            }
            if(target==BuildTarget.Android)
            {
                EditorUserBuildSettings.buildAppBundle=release;
                if(release)ConfigureAndroidSigning();
                else PlayerSettings.Android.useCustomKeystore=false;
            }
            HuntrX.Editor.Optimization.SpriteAtlasPipeline.Build();
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=scenes,locationPathName=output,target=target,
                options=release?BuildOptions.None:BuildOptions.Development
            });
            Evidence evidence=new Evidence
            {
                buildGuid=report.summary.guid.ToString(),editorVersion=Application.unityVersion,version=version,target=target.ToString(),result=report.summary.result.ToString(),
                outputPath=output,completedUtc=DateTime.UtcNow.ToString("O"),sourceCommit=Environment.GetEnvironmentVariable("GITHUB_SHA")??string.Empty,
                totalBytes=report.summary.totalSize,errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,
                durationSeconds=report.summary.totalTime.TotalSeconds,scenes=scenes,
                artifactRoot=target==BuildTarget.StandaloneWindows64?parent:output
            };
            if(report.summary.result==BuildResult.Succeeded&&(File.Exists(evidence.artifactRoot)||Directory.Exists(evidence.artifactRoot)))
                evidence.artifactSha256=ArtifactHash(evidence.artifactRoot);
            string evidencePath=Arg("-huntrxEvidence");
            if(!string.IsNullOrWhiteSpace(evidencePath))
            {
                string folder=Path.GetDirectoryName(Path.GetFullPath(evidencePath));if(!string.IsNullOrEmpty(folder))Directory.CreateDirectory(folder);
                File.WriteAllText(evidencePath,JsonUtility.ToJson(evidence,true));
            }
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0||
                (!File.Exists(output)&&!Directory.Exists(output)))throw new InvalidOperationException("Unity build failed or did not produce its expected output.");
        }
        private static string ArtifactHash(string root)
        {
            bool directory=Directory.Exists(root);
            string[] files=directory?Directory.GetFiles(root,"*",SearchOption.AllDirectories):new[]{root};
            string prefix=directory?root.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar:Path.GetDirectoryName(root)+Path.DirectorySeparatorChar;
            StringBuilder manifest=new StringBuilder();
            foreach(string file in files.Where(x=>!x.EndsWith(".unity.log",StringComparison.Ordinal)&&!x.EndsWith(".evidence.json",StringComparison.Ordinal)).OrderBy(x=>x,StringComparer.Ordinal))
            {
                string hash;
                using(SHA256 sha=SHA256.Create())using(FileStream stream=File.OpenRead(file))hash=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-",string.Empty).ToLowerInvariant();
                manifest.Append(file.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar,'/')).Append('\0').Append(hash).Append('\n');
            }
            using(SHA256 sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(manifest.ToString()))).Replace("-",string.Empty).ToLowerInvariant();
        }
        private static void ConfigureAndroidSigning()
        {
            // Values come from the runner's secure environment; never logged or committed.
            string path=Environment.GetEnvironmentVariable("HUNTRX_ANDROID_KEYSTORE_PATH");
            string storePassword=Environment.GetEnvironmentVariable("HUNTRX_ANDROID_KEYSTORE_PASSWORD");
            string alias=Environment.GetEnvironmentVariable("HUNTRX_ANDROID_KEY_ALIAS");
            string aliasPassword=Environment.GetEnvironmentVariable("HUNTRX_ANDROID_KEY_ALIAS_PASSWORD");
            if(string.IsNullOrWhiteSpace(path)||!File.Exists(path)||string.IsNullOrEmpty(storePassword)||
                string.IsNullOrWhiteSpace(alias)||string.IsNullOrEmpty(aliasPassword))
                throw new InvalidOperationException("Release Android signing requires runner-managed keystore and signing secrets.");
            PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=path;
            PlayerSettings.Android.keystorePass=storePassword;PlayerSettings.Android.keyaliasName=alias;PlayerSettings.Android.keyaliasPass=aliasPassword;
        }
        private static string Arg(string name)
        {
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i+1<args.Length;i++)if(args[i]==name)return args[i+1];
            return null;
        }
    }
}
