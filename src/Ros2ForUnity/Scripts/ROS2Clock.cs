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

namespace ROS2
{

/// <summary>
/// A ros2 clock wrapper class that can acquire ros2 time on demand. It does not include a publisher.
/// (Not every clock needs to be associated with a publisher)
/// </summary>
public class ROS2Clock
{
    private int seconds;
    private uint nanoseconds;
    private ROS2.Clock clock;

    public ROS2Clock()
    {
        clock = new ROS2.Clock();
    }

    ~ROS2Clock()
    {
        clock.Dispose();
    }

    public void UpdateClockMessage(ref rosgraph_msgs.msg.Clock clockMessage)
    {
        GetRosTime();
        clockMessage.Clock_.Sec = seconds;
        clockMessage.Clock_.Nanosec = nanoseconds;
    }

    public void UpdateROSClockTime(builtin_interfaces.msg.Time time)
    {
        GetRosTime();
        time.Nanosec = nanoseconds;
        time.Sec = seconds;
    }

    public void UpdateROSTimestamp(ref ROS2.MessageWithHeader message)
    {
        GetRosTime();
        message.UpdateHeaderTime(seconds, nanoseconds);
    }

    private void GetRosTime()
    {
        if (!ROS2.Ros2cs.Ok())
        {
          Debug.LogWarning("Cannot update ros time, ros2 either not initialized or shut down already");
          return;
        }

        long nanosec = (long)(clock.Now.Seconds * 1e9);
        seconds = (int)(nanosec / 1000000000);
        nanoseconds = (uint)(nanosec % 1000000000);
    }
}

}  // namespace ROS2
