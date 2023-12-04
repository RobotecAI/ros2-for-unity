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

using Request = example_interfaces.srv.AddTwoInts_Request;
using Response = example_interfaces.srv.AddTwoInts_Response;

/// <summary>
/// An example class provided for testing of basic ROS2 service
/// </summary>
public class ServiceExample : MonoBehaviour
{
    private const string NODE_NAME = "service_node";

    /// <summary>
    /// Topic of the service.
    /// </summary>
    public string Topic = "add_two_ints";

    private ROS2UnityComponent ROS;

    private ROS2Node Node;

    private IService<Request, Response> Service;

    /// <summary>
    /// Create the service.
    /// </summary>
    void Start()
    {
        this.ROS = GetComponent<ROS2UnityComponent>();
        this.Node = this.ROS.CreateNode(NODE_NAME);
        this.Service = this.Node.CreateService<Request, Response>(this.Topic, this.OnRequest);
    }

    private Response OnRequest(Request msg)
    {
        Debug.Log($"Incoming Service Request A={msg.A} B={msg.B}");
        return new Response() { Sum = msg.A + msg.B };
    }
}
