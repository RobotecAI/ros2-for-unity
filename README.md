Ros2 For Unity
=============

ROS2 For Unity is a high-performance communication solution to connect Unity3D and ros2 ecosystem in a ros2 "native" way. Communication is not bridged as in several other solutions, but instead uses ros2 (rcl layer and below) middleware stack, which means your can have ros2 nodes in your simulation.
Advantages of this module include:
- High performance, as in supporting higher throughputs and having considerably lower latencies than bridging solutions.
- Your simulation entities are real ros2 nodes / publishers / subscribers and will behave correctly with e.g. command line tooling such as `ros2 topic`. They will respect QoS settings and can use ros2 native time.
- The module supplies abstractions and tools to use in your Unity project, including transformations, sensor interface, a clock, spinning loop wrapped in a Monobehavior, handling initialization and shutdown.
- Suppots all standard ros2 messages
- Custom messages are generated automatically with build, using standard ros2 way. It is straightforward to generate and use them without having to define `.cs` equivalents by hand.
- The module is wrapped as Unity asset. 

### Platforms

Supported OSes: 
- Ubuntu 20.04  (bash)
- Windows 10 (powershell)

Supported ROS2 distributions:
- Foxy
- Galactic

For Windows only, this asset can be prepared in two flavors:
- standalone (no ROS2 installation required on target machine, e.g. your Unity3D simulation server). All required dependencies are installed and can be used e.g. as a complete set of Unity3D plugins.
- overlay (assuming existing (supported) ROS2 installation on target machine). Only asset libraries and generated messages are installed.

#### Platform considerations

On Linux, you can run Editor and App in any way as long as ros2 is sourced in your environment. The best way to ensure that system-wide is to add `source /opt/ros/foxy/setup.bash` to your `~/.profile`
file. This way running by clicking (Hub, Editor, or App executable) is supported as well as command line execution. Note that you need to re-log for changes in `~/.profile` to take place.

### Releases

You can download pre-built releases of the Asset that support both platforms and specific ros2 and Unity3D versions. (TODO - add links).

### Prerequisites

#### Ubuntu

*  ROS2 installed on the system, along with `test-msgs` and `fastrtps` packages
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core 3.1 sdk - [see here](https://www.microsoft.com/net/learn/get-started)

The following script can be used to install the aforementioned prerequisites on Ubuntu 20.04:

```bash
# Install tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp

# Install vcstool package
curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
sudo apt-get update
sudo apt-get install -y python3-vcstool

# Install .NET core
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
```

#### Windows

*  ROS2 installed on the system
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET 5.0 sdk - [see here](https://dotnet.microsoft.com/download/dotnet/5.0)
*  xUnit testing framework (for tests only) - [see here](https://xunit.net/)


### Note on project composition

The project will pull `ros2cs` into the workspace, which also functions independently as it is a more general project aimed at any `C# / .Net` environment. It has its own README and scripting, but for building the Unity Asset, please use instructions and scripting in this document instead, unless you also wish to run tests or examples for `ros2cs`.

### Building

#### Windows considerations

> For **Windows**, [path length is limited to 260 characters](https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation). Clone your repo to `C:\dev` or a similar shallow path to avoid this issue during build.

> For **Windows**, a Visual Studio preconfigured powershell terminal must be used. Standard powershell prompt might not be configured properly to be used with MSVC compiler and Windows SDKs.  You should have Visual Studio already installed (ROS2 dependency) and you can find shortcut for `Developer PowerShell for VS` here: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Visual Studio 2019\Visual Studio Tools`. 

> A powershell terminal with administrator privileges is required for **Windows** and **ros2 galactic**. This is because python packages installation requires a privilage for creating symlinks. More about this issue: [github issue](https://github.com/ament/ament_cmake/issues/350).

> There is a bug with hardcoded include exports in some **ros2 galactic** packages on **Windows**. Easiest workaround is to create a `C:\ci\ws\install\include` directory in your system. More about this bug and proposed workarounds: [github issue](https://github.com/ros2/rclcpp/issues/1688#issuecomment-858467147).

> Sometimes it is required to set NuGet package feed to nuget.org: `dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json` in order to resolve some missing packages for `ros2cs` project.

#### Build instructions

*  Clone this project.
*  If you wish to include custom messages in your build, make sure to put them into `ros2_for_unity_custom_messages.repos` file. You can change this file in your fork or change `custom_messages.repos` in the ros2cs repository fork, it will work either way as the scripts will pull both sources. 
   As an alternative, you can also add your custom messages package directly by copying it to `src/ros2cs/custom_messages` folder after the next step. Any message package in the build tree will be subjected to `.cs` file generator during the build.
*  You need to source your ROS2 installation (e.g. `source /opt/ros/foxy/setup.bash` on Ubuntu or `C:\dev\ros2_foxy\local_setup.ps1` on Windows) before you proceed, for each new open terminal. On Ubuntu, it is most convenient to include this command in your `~/.profile` file.
*  Run `pull_repositories.sh`. This will pull `ros2cs` as well as your custom messages.
*  Run `build.sh` (Ubuntu) or `build.ps1` (Windows) script.
   * You can build tests by adding `--with-tests` argument to `build` command.
   * It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
   * It deploys built plugins into the Asset directory. Note that only plugins built for the current platform will be deployed (there is no cross-compilation).
   * It prepares Unity Asset that is ready to import into your Unity project.

#### Standalone version (Windows)

By default, build process generates standalone libraries (on Windows only).
You can disable this feature by setting CMake option `STANDALONE_BUILD` to `OFF` (e.g. through editing `build.ps1`).

## Running with Unity

## Standalone mode

If the Asset is built with `STANDALONE_BUILD` option set to `1` (the default), then nothing extra needs to be done. Otherwise, you have to source your ros distribution before launching either Unity3D Editor or Application.

## Running the Editor

Open your Unity3D project and import Ros2ForUnity Asset which is built by this project. 

#### Additional considerations for Linux

You need to have ros2 sourced in environment where you run Unity3D. See Platform Considerations for details.

### Building Unity3D application

When there are no errors in the Editor, you can proceed with an application build.
You can do this standard way through `Build->Build Settings...`.

## Running application 

You can run your application in as standard way by clicking it or executing from command line.

## Full example (Windows)

Example for setting up `ros2cs` standalone with `Unity` editor on Windows (powershell with git). Let's assume working directory is `C:\dev` and we are using `ROS2 foxy`:

1. Install ros2 distribution, either Foxy or Galactic. We assume standard directory, e.g. `C:\dev\ros2_foxy`.
   You can find instructions [here (ros2 binary)](https://docs.ros.org/en/foxy/Installation/Windows-Install-Binary.html) and [here (development tools)](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html). 
2. Make sure you are running `Developer PowerShell for Visual Studio` (see **Building** section in `ros2cs` README.md).
3. Source ROS2
```
C:\dev\ros2_foxy\local_setup.ps1
```
4. Follow Build Instructions
5. Launch your Unity3D Project. The following command assumes that you have Unity3D installed in `C:\Program Files\Unity\Hub\Editor\2021.1.7f1\Editor\Unity.exe`:
```
& "C:\Program Files\Unity\Hub\Editor\2021.1.7f1\Editor\Unity.exe" -projectPath C:\dev\<UnityProjectPath>
```
6. Import `Ros2ForUnity` Asset into your project.
10. You should be able to use `Ros2 For Unity` with the module now. You can test if everything works through `Ros2 For Unity` test publisher.
> Note that after you build the Asset, you can use it on a machine that has no ros2 installation.
> You can simply copy over the `Ros2ForUnity` subdirectory to update your Asset. 
