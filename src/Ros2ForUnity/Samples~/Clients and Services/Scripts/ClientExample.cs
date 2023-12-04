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

using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using ROS2;

using Request = example_interfaces.srv.AddTwoInts_Request;
using Response = example_interfaces.srv.AddTwoInts_Response;


/// <summary>
/// An example class provided for testing of basic ROS2 client
/// </summary>
public class ClientExample : MonoBehaviour
{
    private const string NODE_NAME = "client_node";

    /// <summary>
    /// Service topic.
    /// </summary>
    public string Topic = "add_two_ints";

    /// <summary>
    /// Timeout for requests.
    /// </summary>
    public float Timeout = 1;

    private ROS2UnityComponent ROS;

    private ROS2Node Node;

    private IClient<Request, Response> Client;

    /// <summary>
    /// Create the client.
    /// </summary>
    void Start()
    {
        this.ROS = GetComponent<ROS2UnityComponent>();
        this.Node = this.ROS.CreateNode(NODE_NAME);
        this.Client = this.Node.CreateClient<Request, Response>(this.Topic);
        this.StartCoroutine(this.RequestAnswers());
    }

    /// <summary>
    /// Wait for the service to become available
    /// and send random requests.
    /// </summary>
    private IEnumerator RequestAnswers()
    {
        while (!this.Client.IsServiceAvailable())
        {
            Debug.Log("Waiting for Service");
            yield return new WaitForSecondsRealtime(0.25f);
        }

        while (this.ROS.Ok())
        {
            var request = new Request() { A = Random.Range(0, 100), B = Random.Range(0, 100) };

            Debug.Log($"Request answer for {request.A} + {request.B}");
            using (Task<Response> task = this.Client.CallAsync(request))
            {
                float deadline = Time.time + this.Timeout;
                yield return new WaitUntil(() => task.IsCompleted || Time.time >= deadline);

                if (task.IsCompleted)
                {
                    Debug.Log($"Received answer {task.Result.Sum}");
                    Debug.Assert(task.Result.Sum == request.A + request.B, "Received invalid answer");
                }
                else
                {
                    Debug.LogError($"Service call timed out");
                    this.Client.Cancel(task);
                }
            }
        }
    }
}
