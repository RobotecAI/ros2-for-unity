// Copyright 2019-2021 Robotec.ai.
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

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml;

namespace ROS2
{

/// <summary>
/// An internal class responsible for handling checking, proper initialization and shutdown of ROS2cs,
/// </summary>
internal class ROS2ForUnity
{
    private static bool isInitialized = false;
    private static string ros2ForUnityAssetFolderName = "Ros2ForUnity";
    private XmlDocument ros2csMetadata = new XmlDocument();
    private XmlDocument ros2ForUnityMetadata = new XmlDocument();

    public enum Platform
    {
        Windows,
        Linux
    }
    
    public static Platform GetOS()
    {
        if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            return Platform.Linux;
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return Platform.Windows;
        }
        throw new System.NotSupportedException("Only Linux and Windows are supported");
    }

    private static bool InEditor() {
        return Application.isEditor;
    }
    
    private static string GetOSName()
    {
        switch (GetOS())
        {
            case Platform.Linux:
                return "Linux";
            case Platform.Windows:
                return "Windows";
            default:
                throw new System.NotSupportedException("Only Linux and Windows are supported");
        }
    }
    
    private string GetEnvPathVariableName()
    {
      string envVariable = "LD_LIBRARY_PATH";
      if (GetOS() == Platform.Windows)
      {
          envVariable = "PATH";
      }
      return envVariable;
    }

    private string GetEnvPathVariableValue()
    {
        return Environment.GetEnvironmentVariable(GetEnvPathVariableName());
    }

    public static string GetRos2ForUnityPath()
    {
        char separator = Path.DirectorySeparatorChar;
        string appDataPath = Application.dataPath;
        string pluginPath = appDataPath;

        if (InEditor()) {
            pluginPath += separator + ros2ForUnityAssetFolderName;
        }
        return pluginPath; 
    }

    public static string GetPluginPath()
    {
        char separator = Path.DirectorySeparatorChar;
        string ros2ForUnityPath = GetRos2ForUnityPath();
        string pluginPath = ros2ForUnityPath;
        
        pluginPath += separator + "Plugins";
        
        if (InEditor()) {
            pluginPath += separator + GetOSName();
        }

        if (InEditor() || GetOS() == Platform.Windows)
        {
           pluginPath += separator + "x86_64";
        }
        
        if (GetOS() == Platform.Windows)
        {
           pluginPath = pluginPath.Replace("/", "\\");
        }

        return pluginPath;
    }

    /// <summary>
    /// Function responsible for setting up of environment paths for standalone builds
    /// </summary>
    /// <description>
    /// Note that on Linux, LD_LIBRARY_PATH as used for dlopen() is determined on process start and this change won't
    /// affect it. Ros2 looks for rmw implementation based on this variable (independently) and the change
    /// is effective for this process, however rmw implementation's dependencies itself are loaded by dynamic linker 
    /// anyway so setting it for Linux is pointless.
    /// </description>
    private void SetEnvPathVariable()
    {
        string currentPath = GetEnvPathVariableValue();
        string pluginPath = GetPluginPath();
        
        char envPathSep = ':';
        if (GetOS() == Platform.Windows)
        {
            envPathSep = ';';
        }

        Environment.SetEnvironmentVariable(GetEnvPathVariableName(), pluginPath + envPathSep + currentPath);
    }

    public bool IsStandalone() {
        return Convert.ToBoolean(Convert.ToInt16(GetMetadataValue(ros2csMetadata, "/ros2cs/standalone")));
    }

    public string GetROSVersion()
    {
        // Standalone assemblies must use the distribution they were built for.
        return IsStandalone()
            ? GetMetadataValue(ros2ForUnityMetadata, "/ros2_for_unity/ros2")
            : GetROSVersionSourced();
    }

    /// <summary>Reject mismatched native binaries before any ROS library is loaded.</summary>
    public void CheckIntegrity()
    {
        string sourced = GetROSVersionSourced();
        string binding = GetMetadataValue(ros2csMetadata, "/ros2cs/ros2");
        string unity = GetMetadataValue(ros2ForUnityMetadata, "/ros2_for_unity/ros2");
        if (binding != unity)
            throw new InvalidOperationException("ROS2 distribution mismatch: ros2cs=" + binding + ", Unity=" + unity);
        if (!IsStandalone() && sourced != binding)
            throw new InvalidOperationException("Source ROS2 " + binding + " before starting Unity; sourced=" + sourced);
        if (IsStandalone() && !string.IsNullOrEmpty(sourced) && sourced != binding)
            throw new InvalidOperationException("Standalone ROS2 " + binding + " cannot load with sourced ROS2 " + sourced);
        if (IsStandalone() && !string.IsNullOrEmpty(sourced))
            Debug.LogWarning("Start a standalone build from an unsourced shell to avoid mixing native library installations.");

        if (binding == "lyrical" && IntPtr.Size != 8)
            throw new PlatformNotSupportedException("The Lyrical native build requires an x86_64 process.");

        // Lyrical standalone packages carry exactly one selected middleware.
        XmlNode rmwNode = ros2csMetadata.SelectSingleNode("/ros2cs/rmw_implementation");
        if (IsStandalone() && binding == "lyrical")
        {
            if (rmwNode == null || string.IsNullOrWhiteSpace(rmwNode.InnerText))
                throw new InvalidOperationException("Lyrical standalone metadata is missing rmw_implementation.");
            string selected = Environment.GetEnvironmentVariable("RMW_IMPLEMENTATION");
            string packaged = rmwNode.InnerText.Trim();
            if (!string.IsNullOrEmpty(selected) && selected != packaged)
                throw new InvalidOperationException("Requested RMW " + selected + " was not packaged; available=" + packaged);
            Environment.SetEnvironmentVariable("RMW_IMPLEMENTATION", packaged);
        }
    }

    public string GetROSVersionSourced()
    {
        return Environment.GetEnvironmentVariable("ROS_DISTRO");
    }

    /// <summary>
    /// Check if the ros version is supported, only applicable to non-standalone plugin versions
    /// (i. e. without ros2 libraries included in the plugin).
    /// </summary>
    private void CheckROSSupport(string ros2Codename)
    {
        List<string> supportedVersions = new List<string>() { "foxy", "galactic", "humble", "jazzy", "lyrical", "rolling" };
        var supportedVersionsString = String.Join(", ", supportedVersions);
        if (string.IsNullOrEmpty(ros2Codename))
        {
            string errMessage = "No ROS environment sourced. You need to source your ROS2 " + supportedVersionsString
              + " environment before launching Unity (ROS_DISTRO env variable not found)";
            Debug.LogError(errMessage);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            throw new System.InvalidOperationException(errMessage);
#else
            const int ROS_NOT_SOURCED_ERROR_CODE = 33;
            Application.Quit(ROS_NOT_SOURCED_ERROR_CODE);
            throw new InvalidOperationException(errMessage);
#endif
        }

        if (!supportedVersions.Contains(ros2Codename))
        {
            string errMessage = "Currently sourced ROS version differs from supported one. Sourced: " + ros2Codename
              + ", supported: " + supportedVersionsString + ".";
            Debug.LogError(errMessage);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            throw new System.NotSupportedException(errMessage);
#else
            const int ROS_BAD_VERSION_CODE = 34;
            Application.Quit(ROS_BAD_VERSION_CODE);
            throw new NotSupportedException(errMessage);
#endif
        } else if (ros2Codename.Equals("rolling") ) {
            Debug.LogWarning("You are using ROS2 rolling version. Bleeding edge version might not work correctly.");
        }
    }

    private void RegisterCtrlCHandler()
    {
#if ENABLE_MONO
        // Il2CPP build does not support Console.CancelKeyPress currently
        Console.CancelKeyPress += (sender, eventArgs) => {
            eventArgs.Cancel = true;
            DestroyROS2ForUnity();
        };
#endif
    }

    private void ConnectLoggers()
    {
        Ros2csLogger.setCallback(LogLevel.ERROR, Debug.LogError);
        Ros2csLogger.setCallback(LogLevel.WARNING, Debug.LogWarning);
        Ros2csLogger.setCallback(LogLevel.INFO, Debug.Log);
        Ros2csLogger.setCallback(LogLevel.DEBUG, Debug.Log);
        Ros2csLogger.LogLevel = LogLevel.WARNING;
    }

    private string GetMetadataValue(XmlDocument doc, string valuePath)
    {
        XmlNode node = doc.SelectSingleNode(valuePath);
        if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
            throw new InvalidDataException("Missing or empty ROS2 metadata: " + valuePath);
        return node.InnerText.Trim();
    }

    private void LoadMetadata() 
    {
        char separator = Path.DirectorySeparatorChar;
        try
        {
            ros2csMetadata.Load(GetPluginPath() + separator + "metadata_ros2cs.xml");
            ros2ForUnityMetadata.Load(GetRos2ForUnityPath() + separator + "metadata_ros2_for_unity.xml");
        }
        catch (System.IO.FileNotFoundException)
        {
#if UNITY_EDITOR
            var errMessage = "Could not find metadata files.";
            EditorApplication.isPlaying = false;
            throw new System.IO.FileNotFoundException(errMessage);
#else
            const int NO_METADATA = 1;
            Application.Quit(NO_METADATA);
            throw;
#endif
        }
    }

    internal ROS2ForUnity()
    {
        // Load metadata
        LoadMetadata();
        string currentRos2Version = GetROSVersion();
        string standalone = IsStandalone() ? "standalone" : "non-standalone";

        // Self checks
        CheckROSSupport(currentRos2Version);
        CheckIntegrity();

        // Library loading
        // rcpputils uses this path for plugin lookup. On Linux the ELF RPATH,
        // not changing LD_LIBRARY_PATH here, resolves transitive dependencies.
        if (GetOS() == Platform.Windows || IsStandalone())
            SetEnvPathVariable();
        if (GetOS() != Platform.Windows) {
            // For foxy, it is necessary to use modified version of librcpputils to resolve custom msgs packages.
            ROS2.GlobalVariables.absolutePath = GetPluginPath() + "/";
            if (currentRos2Version == "foxy") {
                ROS2.GlobalVariables.preloadLibrary = true;
                ROS2.GlobalVariables.preloadLibraryName = "librcpputils.so";
            }
        }

        // Initialize
        ConnectLoggers();
        Ros2cs.Init();
        RegisterCtrlCHandler();

        string rmwImpl = Ros2cs.GetRMWImplementation();

        Debug.Log("ROS2 version: " + currentRos2Version + ". Build type: " + standalone + ". RMW: " + rmwImpl);

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += this.EditorPlayStateChanged;
        EditorApplication.quitting += this.DestroyROS2ForUnity;
#endif
        isInitialized = true;
    }

    private static void ThrowIfUninitialized(string callContext)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Ros2 For Unity is not initialized, can't " + callContext);
        }
    }

    /// <summary>
    /// Check if ROS2 module is properly initialized and no shutdown was called yet
    /// </summary>
    /// <returns>The state of ROS2 module. Should be checked before attempting to create or use pubs/subs</returns>
    public bool Ok()
    {
        if (!isInitialized)
        {
            return false;
        }
        return Ros2cs.Ok();
    }

    internal void DestroyROS2ForUnity()
    {
        if (isInitialized)
        {
            Debug.Log("Shutting down Ros2 For Unity");
            Ros2cs.Shutdown();
            isInitialized = false;
        }
    }

    ~ROS2ForUnity()
    {
        DestroyROS2ForUnity();
    }

#if UNITY_EDITOR
    void EditorPlayStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            DestroyROS2ForUnity();
        }
    }
#endif
}

}  // namespace ROS2
