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
using System.IO;
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
        var r2fuMetadataName = "metadata_ros2_for_unity.xml";
        var r2csMetadataName = "metadata_ros2cs.xml";

        var r2fuMeta = Path.Combine(ROS2ForUnity.rootPath, r2fuMetadataName);
        var r2csMeta = Path.Combine(ROS2ForUnity.pluginPath, r2csMetadataName);

        var outputDir = Directory.GetParent(report.summary.outputPath).ToString();
        var execFilename = Path.GetFileNameWithoutExtension(report.summary.outputPath);

        FileUtil.CopyFileOrDirectory(
            r2fuMeta, Path.Combine(outputDir, execFilename + "_Data", r2fuMetadataName)
        );

        string r2csMetaTarget;
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64) {
            r2csMetaTarget = Path.Combine(outputDir, execFilename + "_Data", "Plugins", r2csMetadataName);
        } else {
            r2csMetaTarget = Path.Combine(outputDir, execFilename + "_Data", "Plugins", "x86_64", r2csMetadataName);
        }

        FileUtil.CopyFileOrDirectory(r2csMeta, r2csMetaTarget);
    }

}

}
#endif
