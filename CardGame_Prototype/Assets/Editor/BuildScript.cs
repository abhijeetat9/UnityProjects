using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// CI entry points, invoked by GitHub Actions (game-ci/unity-builder) via
// `-executeMethod BuildScript.<MethodName>`. Mirrors exactly what the
// matching Build Profile in Assets/Settings/Build Profiles does, so a CI
// build and an Editor "Build" click produce the same output:
//   - BuildWebGLClient  <-> "Web - Desktop - Release" profile
//     (WebGL target, Client subtarget, scenes: Auth, Instruction, Lobby)
// The Linux dedicated server is intentionally NOT built here - the Oracle
// VM is ARM64 and a standard x86_64 CI runner would produce an
// incompatible x86_64 binary, so that build stays a local step (see
// GameServer-Oracle/README or the deploy-gameserver workflow, which only
// automates shipping an already-built ARM64 binary to the VM).
public static class BuildScript
{
    public static void BuildWebGLClient()
    {
        var scenes = new[]
        {
            "Assets/Scenes/Auth.unity",
            "Assets/Scenes/Instruction.unity",
            "Assets/Scenes/Lobby.unity",
        };

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            // Matches the game-ci/unity-builder workflow's buildsPath ("build")
            // + targetPlatform ("WebGL") convention, so the action's own
            // artifact handling and our explicit upload-artifact step agree
            // on where the output actually is.
            locationPathName = "build/WebGL",
            target = BuildTarget.WebGL,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = BuildOptions.None,
        };

        Debug.Log("BuildScript: starting WebGL client build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException($"WebGL build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
        }

        Debug.Log($"BuildScript: WebGL build succeeded, {report.summary.totalSize} bytes, output at {buildPlayerOptions.locationPathName}");
    }
}

