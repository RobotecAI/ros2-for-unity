// Minimal API surface for compilation and adapter tests. This is NOT Unity.
using System;
namespace UnityEngine
{
    public enum RuntimePlatform { LinuxEditor, LinuxPlayer, WindowsEditor, WindowsPlayer }
    public static class Application
    {
        public static RuntimePlatform platform;
        public static bool isEditor;
        public static string dataPath;
        public static void Quit(int code) { Console.WriteLine("Quit: " + code); }
    }
    public static class Debug
    {
        public static void Log(object value) { Console.WriteLine(value); }
        public static void LogError(object value) { Console.Error.WriteLine(value); }
        public static void LogWarning(object value) { Console.WriteLine(value); }
    }
    public class MonoBehaviour
    {
        public T GetComponent<T>() where T : new() { return new T(); }
        public void StartCoroutine(System.Collections.IEnumerator routine) { }
    }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float seconds) { } }
    public static class Random { public static int Range(int min, int max) { return min; } }
    public static class Time { public static double timeAsDouble; public static float timeScale = 1, fixedDeltaTime = .02f; }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
    }
    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x=x; this.y=y; this.z=z; this.w=w; }
    }
    public struct Vector4 { public Vector4(float x, float y, float z, float w) { } }
    public struct Matrix4x4
    {
        public Matrix4x4(Vector4 a, Vector4 b, Vector4 c, Vector4 d) { }
        public Matrix4x4 transpose { get { return this; } }
    }
}
namespace UnityEngine.Profiling { }
#if UNITY_EDITOR
namespace UnityEditor
{
    public enum BuildTarget { StandaloneLinux64, StandaloneWindows64, Android }
    public enum PlayModeStateChange { ExitingPlayMode }
    public static class EditorApplication
    {
        public static bool isPlaying;
        public static event Action<PlayModeStateChange> playModeStateChanged;
        public static event Action quitting;
        public static void Stop() { playModeStateChanged?.Invoke(PlayModeStateChange.ExitingPlayMode); }
        public static void Quit() { quitting?.Invoke(); }
    }
}
namespace UnityEditor.Build
{
    public interface IPostprocessBuildWithReport
    {
        int callbackOrder { get; }
        void OnPostprocessBuild(Reporting.BuildReport report);
    }
    public class BuildFailedException : Exception { public BuildFailedException(string message) : base(message) { } }
}
namespace UnityEditor.Build.Reporting
{
    public class BuildReport { public BuildSummary summary; }
    public struct BuildSummary { public string outputPath; public UnityEditor.BuildTarget platform; }
}
#endif
