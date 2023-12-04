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
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ROS2
{
    /// <summary>
    /// An internal class responsible for installing versioned shared objects
    /// </summary>
    internal class PostInstall : IPostprocessBuildWithReport
    {
        public int callbackOrder { get => 0; }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.StandaloneLinux64)
            {
                // Copy versioned libraries (Unity skips them)
                Regex soWithVersionReg = new Regex(@".*\.so(\.[0-9])+$");
                var versionedLibs = Directory.GetFiles(Setup.PluginPath).Where(path => soWithVersionReg.IsMatch(path));
                var outputDir = Directory.GetParent(report.summary.outputPath);
                string execFilename = Path.GetFileNameWithoutExtension(report.summary.outputPath);
                foreach (string libPath in versionedLibs)
                {
                    FileUtil.CopyFileOrDirectory(libPath, $"{outputDir}/{execFilename}_Data/Plugins/{Path.GetFileName(libPath)}");
                }
            }
        }
    }
}
#endif
