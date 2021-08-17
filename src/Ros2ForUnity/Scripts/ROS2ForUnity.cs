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
using UnityEditor;

namespace ROS2
{

/// <summary>
/// An internal class responsible for handling checking, proper initialization and shutdown of ROS2cs,
/// </summary>
internal class ROS2ForUnity
{
    private static bool isInitialized = false;
    private static string ros2ForUnityAssetFolderName = "Ros2ForUnity";

    enum Platform
    {
        Windows,
        Linux
    }
    
    private Platform GetOS()
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

    private bool InEditor() {
        return Application.isEditor;
    }
    
    private string GetOSName()
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

    private string GetPluginPath()
    {
        char separator = Path.DirectorySeparatorChar;
        string appDataPath = Application.dataPath;
        string pluginPath = appDataPath;

        if (InEditor()) {
            pluginPath += separator + ros2ForUnityAssetFolderName;
        }
        
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
    /// anyway so setting  it for Linux is pointless.
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

    /// <summary>
    /// Check if the ros version is supported, only applicable to non-standalone plugin versions
    /// (i. e. without ros2 libraries included in the plugin).
    /// </summary>
    private void CheckROSVersionSourced()
    {
        string currentVersion = Environment.GetEnvironmentVariable("ROS_DISTRO");
        List<string> supportedVersions = new List<string>() { "foxy", "galactic" };
        var supportedVersionsString = String.Join(", ", supportedVersions);
        if (string.IsNullOrEmpty(currentVersion))
        {
            string errMessage = "No ROS environment sourced. You need to source your ROS2 " + supportedVersionsString
              + " environment before launching Unity. Make sure you launch with the start app/editor script";
            Debug.LogError(errMessage);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            throw new System.InvalidOperationException(errMessage);
#else
            const int ROS_NOT_SOURCED_ERROR_CODE = 33;
            Application.Quit(ROS_NOT_SOURCED_ERROR_CODE);
#endif
        }

        if (!supportedVersions.Contains(currentVersion))
        {
            string errMessage = "Currently sourced ROS version differs from supported one. Sourced: " + currentVersion
              + ", supported: " + supportedVersionsString + ".";
            Debug.LogError(errMessage);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            throw new System.NotSupportedException(errMessage);
#else
            const int ROS_BAD_VERSION_CODE = 34;
            Application.Quit(ROS_BAD_VERSION_CODE);
#endif
        }
        Debug.Log("Running with a supported ROS 2 version: " + currentVersion);
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

    internal ROS2ForUnity()
    {
        // TODO: Find a way to determine whether we run standalone build
        if (GetOS() == Platform.Windows) {
            // Windows version can run standalone, modifies PATH to ensure all plugins visibility
            SetEnvPathVariable();
        } else {
            // Linux version needs to have ros2 sourced, which is checked here. It also loads plugins by absolute path
            // since LD_LIBRARY_PATH cannot be set dynamically within the process for dlopen() which is used under the hood.
            // Since libraries are built with -rpath=".", dependencies will be correcly located within plugins directory
            CheckROSVersionSourced();
            ROS2.GlobalVariables.absolutePath = GetPluginPath() + "/";
        }
        ConnectLoggers();
        Ros2cs.Init();
        RegisterCtrlCHandler();

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
