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

using UnityEngine;
using ROS2;

/// <summary>
/// An example class provided for testing of basic ROS2 communication
/// </summary>
public class PublisherExample : MonoBehaviour
{
    private const string NODE_NAME = "talker_node";

    /// <summary>
    /// Topic to publish to.
    /// </summary>
    public string Topic = "chatter";

    private ROS2UnityComponent ROS;

    private ROS2Node Node;

    private IPublisher<std_msgs.msg.String> Publisher;

    private int i = 0;

    /// <summary>
    /// Create the publisher.
    /// </summary>
    void Start()
    {
        this.ROS = GetComponent<ROS2UnityComponent>();
        this.Node = this.ROS.CreateNode(NODE_NAME);
        this.Publisher = this.Node.CreatePublisher<std_msgs.msg.String>("chatter");
    }

    /// <summary>
    /// Publish a new message.
    /// </summary>
    void Update()
    {
        if (this.ROS.Ok())
        {
            this.i++;
            var msg = new std_msgs.msg.String() { Data = $"Unity ROS2 sending: hello {this.i}" };
            this.Publisher.Publish(msg);
        }
    }
}
