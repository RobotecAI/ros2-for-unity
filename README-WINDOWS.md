# ROS2 For Unity - Windows 10

This readme contains information specific to Window 10. For general information, please see README.md

## Building

Example for setting up `ros2cs` standalone with `Unity` editor on Windows (powershell with git). Let's assume working directory is `C:\dev` and we are using `ROS2 foxy`.

### Prerequisites

To prepare your machine for `ROS2 For Unity` build please follow step by step the `ros2cs` [Prequisites](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#prequisites) and [Important notices](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices) sections.
Prepairing environment with `ros2cs` instruction is ***crucial*** for failure-free Unity Asset generation.

**Ommitting any of the steps mentioned above will result in a build failure!**

### Steps

* Make sure [long paths on Windows are enabled](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* Please open [`Developer PowerShell for VS` with administrator privileges](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* When using `ros2 galactic` distribution, for simplicity's's sake please [create a `C:\ci\ws\install\include` directory](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* Clone this repository.
* You need to source your ROS2 installation (`C:\dev\ros2_foxy\local_setup.ps1`) before you proceed, for each new terminal.
* Run `pull_repositories.ps1`. This will pull `ros2cs` as well as your custom messages. You will be asked for github credentials, so please fill your information.
* Run `build.ps1` script.
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

**If no solution of your problem is present in the section above, please make sure to check out `ros2cs` [Troubleshooting section](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#troubleshooting)**

## OS-Specific usage remarks

> If the Asset is built with `STANDALONE_BUILD` option set to `1` (the default), then nothing extra needs to be done.
Otherwise, you have to source your ros distribution before launching either Unity3D Editor or Application.

> Note that after you build the Asset, you can use it on a machine that has no ros2 installation.

> You can simply copy over the `Ros2ForUnity` subdirectory to update your Asset.
