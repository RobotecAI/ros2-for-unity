Ros2 For Unity
===============

ROS2 For Unity is a high-performance communication solution to connect Unity3D and ROS2 ecosystem in a ROS2 "native" way. Communication is not bridged as in several other solutions, but instead it uses ROS2 middleware stack (rcl layer and below), which means you can have ROS2 nodes in your simulation.
Advantages of this module include:
- High performance - higher throughput and considerably lower latencies comparing to bridging solutions.
- Your simulation entities are real ROS2 nodes / publishers / subscribers. They will behave correctly with e.g. command line tools such as `ros2 topic`. They will respect QoS settings and can use ROS2 native time.
- The module supplies abstractions and tools to use in your Unity project, including transformations, sensor interface, a clock, spinning loop wrapped in a MonoBehavior, handling initialization and shutdown.
- Supports all standard ROS2 messages.
- Custom messages are generated automatically with build, using standard ROS2 way. It is straightforward to generate and use them without having to define `.cs` equivalents by hand.
- The module is wrapped as a Unity asset.

## Platforms

Supported OSes:
- Ubuntu 22.04 (bash)
- Ubuntu 20.04 (bash)
- Windows 10 (powershell)
- Windows 11* (powershel)

> \* ROS2 Galactic and Humble support only Windows 10 ([ROS 2 Windows system requirements](https://docs.ros.org/en/humble/Installation/Windows-Install-Binary.html#system-requirements)), but it is proven that it also works fine on Windows 11.


Supported ROS2 distributions:
- Galactic
- Humble

Supported Unity3d:
- 2020+

Older versions of Unity3d may work, but the editor executable most probably won't be detected properly by deployment script. This would require user confirmation for using unsupported version.

This asset can be prepared in two flavours:

- standalone mode, where no ROS2 installation is required on target machine, e.g., your Unity3D simulation server. All required dependencies are installed and can be used e.g. as a complete set of Unity3D plugins.
- overlay mode, where the ROS2 installation is required on target machine. Only asset libraries and generated messages are installed therefore ROS2 instance must be sourced.

## Releases

> **Note:** The releases are built in overlay mode.

The best way to start quickly is to use our releases.

You can download pre-built [releases](https://github.com/RobotecAI/ros2-for-unity/releases) of the Asset that support both platforms and specific ros2 and Unity3D versions.

## Building

> **Note:** The project will pull `ros2cs` into the workspace, which also functions independently as it is a more general project aimed at any `C# / .Net` environment.
It has its own README and scripting, but for building the Unity Asset, please use instructions and scripting in this document instead, unless you also wish to run tests or examples for `ros2cs`.

Please see OS-specific instructions:
- [Instructions for Ubuntu](README-UBUNTU.md)
- [Instructions for Windows](README-WINDOWS.md)

## Custom messages

Custom messages can be included in the build by either:
* listing them in `ros2_for_unity_custom_messages.repos` file, or
* manually inserting them in `src/ros2cs` directory. If the folder doesn't exist, you must pull repositories first (see building steps for each OS).

## Installation

1. Perform building steps described in the OS-specific readme or download pre-built Unity package. Do not source `ros2-for-unity` nor `ros2cs` project into ROS2 workspace.
1. Open or create Unity project.
1. Import asset into project:
    1. copy `install/asset/Ros2ForUnity` into your project `Assets` folder, or
    1. if you have deployed an `.unitypackage` - import it in Unity Editor by selecting `Import Package` → `Custom Package`

## Usage

**Prerequisites**

* If your build was prepared with `--standalone` flag then you are fine, and all you have to do is run the editor

otherwise

* source ROS2 which matches the `Ros2ForUnity` version, then run the editor from within the very same terminal/console.

**Initializing Ros2ForUnity**

1. Initialize `Ros2ForUnity` by creating a "hook" object which will be your wrapper around ROS2. You have two options:
    1. `ROS2UnityComponent` based on `MonoBehaviour` which must be attached to a `GameObject` somewhere in the scene, then:
        ```c#
        using ROS2;
        ...
        // Example method of getting component, if ROS2UnityComponent lives in different GameObject, just use different get component methods.
        ROS2UnityComponent ros2Unity = GetComponent<ROS2UnityComponent>();
        ```
    1. or `ROS2UnityCore` which is a standard class that can be created anywhere
        ```c#
        using ROS2;
        ...
        ROS2UnityCore ros2Unity = new ROS2UnityCore();
        ```
1. Create a node. You must first check if `Ros2ForUnity` is initialized correctly:
    ```c#
    private ROS2Node ros2Node;
    ...
    if (ros2Unity.Ok()) {
        ros2Node = ros2Unity.CreateNode("ROS2UnityListenerNode");
    }
    ```

**Publishing messages:**

1. Create publisher
    ```c#
    private IPublisher<std_msgs.msg.String> chatter_pub;
    ...
    if (ros2Unity.Ok()){
        chatter_pub = ros2Node.CreatePublisher<std_msgs.msg.String>("chatter"); 
    }
    ```
1. Send messages
    ```c#
    std_msgs.msg.String msg = new std_msgs.msg.String();
    msg.Data = "Hello Ros2ForUnity!";
    chatter_pub.Publish(msg);
    ```

**Subscribing to a topic**

1. Create subscriber:
    ```c#
    private ISubscription<std_msgs.msg.String> chatter_sub;
    ...
    if (ros2Unity.Ok()) {
        chatter_sub = ros2Node.CreateSubscription<std_msgs.msg.String>(
            "chatter", msg => Debug.Log("Unity listener heard: [" + msg.Data + "]"));
    }
    ```

### Examples

1. Create a top-level object containing `ROS2UnityComponent.cs`. This is the central `Monobehavior` for `Ros2ForUnity` that manages all the nodes. Refer to class documentation for details.
1. Add `ROS2TalkerExample.cs` script to the very same game object.
1. Add `ROS2ListenerExample.cs` script to the very same game object.
    > **Note:** Each example script looks for `ROS2UnityComponent` in its own game object. However, this is not a requirement, just example implementation.
1. Once you start the project in Unity, you should be able to see two nodes talking with each other in  Unity Editor's console or use `ros2 node list` and `ros2 topic echo /chatter` to verify ros2 communication.
## Acknowledgements 

Open-source release of ROS2 For Unity was made possible through cooperation with [Tier IV](https://tier4.jp). Thanks to encouragement, support and requirements driven by Tier IV the project was significantly improved in terms of portability, stability, core structure and user-friendliness.
