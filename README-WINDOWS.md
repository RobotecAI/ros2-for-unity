# ROS2 For Unity - Windows 10

This readme contains information specific to Window 10. For general information, please see README.md

## Building

Example for setting up `ros2cs` standalone with `Unity` editor on Windows (powershell with git). Let's assume working directory is `C:\dev` and we are using `ROS2 foxy`.

### Prerequisites

*  ROS2 installed on the system (additionally you should go to [Building ROS2 section](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html) and check if all `pip` [Install dependencies](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html#install-dependencies) and [Developer tools](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html#install-developer-tools) are installed)
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET 5.0 sdk - [see here](https://dotnet.microsoft.com/download/dotnet/5.0)
*  For tests only: xUnit testing framework - [see here](https://xunit.net/)

### Important notices

> For **Windows**, [path length is limited to 260 characters](https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation). Clone your repo to `C:\dev` into `r2fu` folder or a similar shallow path to avoid this issue during build. **Cloning into longer path will cause compilation errors!**

> For **Windows**, a Visual Studio preconfigured powershell terminal must be used. Standard powershell prompt might not be configured properly to be used with MSVC compiler and Windows SDKs.  You should have Visual Studio already installed (ROS2 dependency) and you can find shortcut for `Developer PowerShell for VS` here: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Visual Studio 2019\Visual Studio Tools`.

> A powershell terminal with administrator privileges is required for **Windows** and **ros2 galactic**. This is because python packages installation requires a privilage for creating symlinks. More about this issue: [github issue](https://github.com/ament/ament_cmake/issues/350).

> There is a bug with hardcoded include exports in some **ros2 galactic** packages on **Windows**. Easiest workaround is to create a `C:\ci\ws\install\include` directory in your system. More about this bug and proposed workarounds: [github issue](https://github.com/ros2/rclcpp/issues/1688#issuecomment-858467147).

> Sometimes it is required to set NuGet package feed to nuget.org: `dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json` in order to resolve some missing packages for `ros2cs` project.

### Steps

*  Clone this project and change name to `r2fu` shortcut with `git clone git@gitlab.com:robotec.ai/tieriv/ros2-for-unity.git r2fu` command. Anything longer can cause compilation errors due to Windows path lenght limitations.
*  You need to source your ROS2 installation (`C:\dev\ros2_foxy\local_setup.ps1`) before you proceed, for each new terminal.
*  Run `pull_repositories.ps1`. This will pull `ros2cs` as well as your custom messages. You will be asked for gitlab credentials, so please fill your information.
*  Run `build.ps1` script.
    * You can build tests by adding `--with-tests` argument to `build` command.
    * It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
    * It deploys built plugins into the Asset directory. Note that only plugins built for the current platform will be deployed (there is no cross-compilation).
    * It prepares Unity Asset that is ready to import into your Unity project.
    * By default, build process generates standalone libraries (on Windows only).
      You can disable this feature by setting CMake option `STANDALONE_BUILD` to `OFF` (e.g. through editing `build.ps1`).
* In order to generate `Ros2ForUnity.unitypackage` please run `create_unity_asset.ps1`. Please provide path to your Unity executable when prompted.
    * Asset can be found under `C:\dev\r2fu\install\unity_package` directory
    * In case your Unity license has expired, the `create_unity_asset.ps1` won't throw any errors but `Ros2ForUnity.unitypackage` won't be generated too.
* At this moment you have two valid forms of the Asset.
    * One is available as `C:\dev\r2fu\src\Ros2ForUnity` folder.
    * Second one is `Ros2ForUnity.unitypackage`

## Build troubleshooting

- If you see one of the following errors:
><script_name> is not digitally signed

><script_name> cannot be loaded because running scripts is disabled on this system

Please execute `Set-ExecutionPolicy Bypass -Scope Process` in PS shell session to enable third party scripts execution only for this session. Otherwise please refer to official [Execution Policies](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies?view=powershell-7.1).

- If you see the following error:
>     [4.437s] Traceback (most recent call last):
>     [4.437s]   File "<string>", line 1, in <module>
>     [4.437s]   File "C:\Python38\lib\site-packages\numpy\__init__.py", line 148, in <module>
>     [4.437s]     from . import _distributor_init
>     [4.437s]   File "C:\Python38\lib\site-packages\numpy\_distributor_init.py", line 26, in <module>
>     [4.437s]     WinDLL(os.path.abspath(filename))
>     [4.437s]   File "C:\Python38\lib\ctypes\__init__.py", line 373, in __init__
>     [4.453s]     self._handle = _dlopen(self._name, mode)
>     [4.453s] OSError: [WinError 193] %1 is not a valid Win32 application
>     [4.469s] CMake Error at C:/dev/ros2_foxy/share/rosidl_generator_py/cmake/rosidl_generator_py_generate_interfaces.cmake:213 (message)
>     [4.469s]   execute_process(C:/Python38/python.exe -c 'import
>     [4.469s]   numpy;print(numpy.get_include())') returned error code 1
>     [4.469s] Call Stack (most recent call first):
>     [4.469s]   C:/dev/ros2_foxy/share/ament_cmake_core/cmake/core/ament_execute_extensions.cmake:48 (include)
>     [4.469s]   C:/dev/ros2_foxy/share/rosidl_cmake/cmake/rosidl_generate_interfaces.cmake:286 (ament_execute_extensions)
>     [4.484s]   CMakeLists.txt:16 (rosidl_generate_interfaces)
Please reinstall `numpy` package from python by typing:
```bash
pip uninstall numpy
pip install numpy
```

## OS-Specific usage remarks

> If the Asset is built with `STANDALONE_BUILD` option set to `1` (the default), then nothing extra needs to be done.
Otherwise, you have to source your ros distribution before launching either Unity3D Editor or Application.

> Note that after you build the Asset, you can use it on a machine that has no ros2 installation.

> You can simply copy over the `Ros2ForUnity` subdirectory to update your Asset.
