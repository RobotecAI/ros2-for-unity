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

    internal ROS2ForUnity()
    {
        Ros2cs.Init();
        RegisterCtrlCHandler();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += this.EditorPlayStateChanged;
        EditorApplication.quitting += this.DestroyROS2ForUnity;
#endif
        isInitialized = true;
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
