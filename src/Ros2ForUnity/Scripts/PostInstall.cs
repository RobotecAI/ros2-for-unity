// Copyright 2019-2022 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ROS2
{

/// <summary>
/// An internal class responsible for installing ros2-for-unity metadata files 
/// </summary>
internal class PostInstall : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        var target = report.summary.platform;
        if (target != BuildTarget.StandaloneLinux64 && target != BuildTarget.StandaloneWindows64)
            throw new BuildFailedException("ROS2ForUnity supports x86_64 Linux and Windows Players only.");

        string osName = target == BuildTarget.StandaloneLinux64 ? "Linux" : "Windows";
        string asset = ROS2ForUnity.GetRos2ForUnityPath();
        string source = Path.Combine(asset, "Plugins", osName, "x86_64");
        string data = Path.Combine(Path.GetDirectoryName(report.summary.outputPath),
            Path.GetFileNameWithoutExtension(report.summary.outputPath) + "_Data");
        string native = target == BuildTarget.StandaloneLinux64
            ? Path.Combine(data, "Plugins") : Path.Combine(data, "Plugins", "x86_64");
        Directory.CreateDirectory(native);
        File.Copy(Path.Combine(asset, "metadata_ros2_for_unity.xml"),
            Path.Combine(data, "metadata_ros2_for_unity.xml"), true);
        File.Copy(Path.Combine(source, "metadata_ros2cs.xml"),
            Path.Combine(native, "metadata_ros2cs.xml"), true);

        // Unity skips versioned ELF libraries, including multi-digit SONAMEs.
        if (target == BuildTarget.StandaloneLinux64)
            foreach (string library in Directory.GetFiles(source))
                if (Regex.IsMatch(library, @"\.so(\.[0-9]+)+$"))
                    File.Copy(library, Path.Combine(native, Path.GetFileName(library)), true);

        // Non-plugin files are not automatically included in a Player build.
        string share = Path.Combine(source, "share");
        if (Directory.Exists(share))
            CopyResources(share, Path.Combine(native, "share"));
        string manifest = Path.Combine(source, "runtime-manifest.json");
        if (File.Exists(manifest))
            File.Copy(manifest, Path.Combine(native, "runtime-manifest.json"), true);
    }

    private static void CopyResources(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.GetFiles(source))
            if (!file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        foreach (string directory in Directory.GetDirectories(source))
            CopyResources(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

}

}
#endif
