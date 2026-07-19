using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CommonFramework.EditorTools.Build
{
    public static class BuildScript
    {
        public static void BuildMac()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray(),
                locationPathName = "Builds/macOS/HarnessProject.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (BuildResult.Succeeded != report.summary.result)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
