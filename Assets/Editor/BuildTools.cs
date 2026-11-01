using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Liangshan.EditorTools
{
    public static class BuildTools
    {
        public static void CompileCheck()
        {
            Debug.Log("[Liangshan] Compile check passed.");
        }

        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Liangshan/Build macOS Preview")]
        public static void BuildMacPreview()
        {
            BuildMac("Build/LiangshanPreview.app", true);
        }

        [MenuItem("Liangshan/Build macOS")]
        public static void BuildMacRelease()
        {
            BuildMac("Build/Liangshan.app", false);
        }

        static void BuildMac(string path, bool development)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            PlayerSettings.productName = "Liangshan 108";
            PlayerSettings.companyName = "Liangshan Studio";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.liangshanstudio.liangshan108");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = path,
                target = BuildTarget.StandaloneOSX,
                options = development ? BuildOptions.Development : BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[Liangshan] Build {summary.result}: {path} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors)");
            if (summary.result != BuildResult.Succeeded)
                throw new System.Exception("Build failed: " + summary.result);
        }
    }
}
