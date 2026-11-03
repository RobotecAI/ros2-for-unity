using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ROS2;
using UnityEngine;

internal static class Program
{
    [System.Runtime.InteropServices.DllImport("libc.so.6", EntryPoint = "getenv")]
    private static extern IntPtr GetNativeEnvironment(string name);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static int Main(string[] args)
    {
        Application.dataPath = Path.GetFullPath(args[1]);
#if UNITY_EDITOR
        Application.isEditor = true;
        Application.platform = RuntimePlatform.LinuxEditor;
        if (args[0] == "postinstall")
        {
            var report = new UnityEditor.Build.Reporting.BuildReport {
                summary = new UnityEditor.Build.Reporting.BuildSummary {
                    outputPath = Path.GetFullPath(args[2]),
                    platform = UnityEditor.BuildTarget.StandaloneLinux64 } };
            new PostInstall().OnPostprocessBuild(report);
            new PostInstall().OnPostprocessBuild(report); // Rebuilding must overwrite safely.
            string plugins = Path.Combine(Path.GetDirectoryName(report.summary.outputPath),
                Path.GetFileNameWithoutExtension(report.summary.outputPath) + "_Data", "Plugins");
            Require(File.Exists(Path.Combine(plugins, "share/ament_index/resource_index/packages/rosidl_buffer_backend")), "Player resource missing");
            foreach (string file in Directory.GetFiles(ROS2ForUnity.GetPluginPath()))
                if (System.Text.RegularExpressions.Regex.IsMatch(file, @"\.so(\.[0-9]+)+$"))
                    Require(File.Exists(Path.Combine(plugins, Path.GetFileName(file))), "Player SONAME missing: " + file);
            Console.WriteLine("PASS postinstall resources, versioned libraries, repeat build");
            return 0;
        }
#else
        Application.isEditor = false;
        Application.platform = RuntimePlatform.LinuxPlayer;
#endif
        if (args[0] == "reject")
        {
            try { new ROS2ForUnity(); }
            catch (Exception e) when (e is InvalidOperationException || e is InvalidDataException)
            {
                Require(e.Message.Contains(args[2]), "Wrong rejection: " + e.Message);
                Console.WriteLine("PASS rejected: " + e.Message);
                return 0;
            }
            throw new Exception("Invalid artifact/environment was accepted");
        }
        ROS2ForUnity previous = null;
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var runtime = new ROS2ForUnity();
            Require(runtime.Ok(), "Initialization failed");
            string prefix = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(GetNativeEnvironment("AMENT_PREFIX_PATH"));
            Require(prefix == Environment.GetEnvironmentVariable("AMENT_PREFIX_PATH"), "Native ament environment differs from managed environment");
            Require(Ros2cs.GetRMWImplementation() == Environment.GetEnvironmentVariable("RMW_IMPLEMENTATION"), "Native ROS loaded the wrong RMW");
            previous?.DestroyROS2ForUnity();
            Require(runtime.Ok(), "A stopped owner shut down the new runtime");
            var node = new ROS2Node("unity_adapter_" + cycle);
            string received = null;
            var sub = node.CreateSubscription<std_msgs.msg.String>("unity_adapter_loop", msg => received = msg.Data);
            var pub = node.CreatePublisher<std_msgs.msg.String>("unity_adapter_loop");
            var message = new std_msgs.msg.String { Data = "Lyrical Unity " + cycle };
            var timer = Stopwatch.StartNew();
            while (received != message.Data && timer.Elapsed.TotalSeconds < 15)
            {
                pub.Publish(message);
                Ros2cs.SpinOnce(node.node, .02);
                Thread.Sleep(20);
            }
            Require(received == message.Data, "Pub/sub timeout");
            var service = node.CreateService<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response>(
                "unity_adapter_add", request => new example_interfaces.srv.AddTwoInts_Response { Sum = request.A + request.B });
            var client = node.CreateClient<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response>("unity_adapter_add");
            var task = client.CallAsync(new example_interfaces.srv.AddTwoInts_Request { A = 19, B = 23 });
            timer.Restart();
            while (!task.IsCompleted && timer.Elapsed.TotalSeconds < 15) Ros2cs.SpinOnce(node.node, .02);
            Require(task.IsCompleted && task.Result.Sum == 42, "Service timeout/wrong result");
#if CUSTOM_INTERFACES
            bool customReceived = false;
            var customSub = node.CreateSubscription<unity_lyrical_test_interfaces.msg.Payload>(
                "unity_adapter_custom", msg => customReceived = msg.Label == "custom" &&
                    msg.Values.Length == 3 && msg.Values[2] == 42);
            var customPub = node.CreatePublisher<unity_lyrical_test_interfaces.msg.Payload>("unity_adapter_custom");
            timer.Restart();
            while (!customReceived && timer.Elapsed.TotalSeconds < 15)
            {
                customPub.Publish(new unity_lyrical_test_interfaces.msg.Payload {
                    Label = "custom", Values = new int[] { -1, 0, 42 } });
                Ros2cs.SpinOnce(node.node, .02);
                Thread.Sleep(20);
            }
            Require(customReceived, "Custom string/sequence message round trip failed");
            Console.WriteLine("PASS custom string/sequence message");
#endif
            if (args[0] == "interop" && cycle == 0)
            {
                received = null;
                var peerSub = node.CreateSubscription<std_msgs.msg.String>("ros_to_unity", msg => received = msg.Data);
                var peerPub = node.CreatePublisher<std_msgs.msg.String>("unity_to_ros");
                timer.Restart();
                while (received != "ros-ack" && timer.Elapsed.TotalSeconds < 30)
                {
                    peerPub.Publish(new std_msgs.msg.String { Data = "unity-ping" });
                    Ros2cs.SpinOnce(node.node, .05);
                    Thread.Sleep(50);
                }
                Require(received == "ros-ack", "External rclpy round trip failed");
                Console.WriteLine("PASS external rclpy round trip");
            }
            Ros2cs.RemoveNode(node.node);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Stop();
            Require(!runtime.Ok(), "Editor stop callback failed");
#endif
            runtime.DestroyROS2ForUnity();
            previous = runtime;
            Require(!runtime.Ok(), "Shutdown failed");
            Console.WriteLine("PASS cycle " + cycle + " pub/sub, service, shutdown");
        }
        return 0;
    }
}
