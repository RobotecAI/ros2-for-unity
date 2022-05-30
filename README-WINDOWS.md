# ROS2 For Unity - Windows 10

This readme contains information specific to Window 10. For general information, please see [README.md](README.md).

## Building

We assume that working directory is `C:\dev` and we are using `ROS2 galactic` (replace with `foxy` or `humble` where applicable).

### Prerequisites

It is necessary to complete all the steps for `ros2cs` [Prerequisites](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#prerequisites) and consider [Important notices](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices) sections.

### Steps

* Make sure [long paths on Windows are enabled](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* Make sure you open [`Developer PowerShell for VS` with administrator privileges](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* For `ros2 galactic` distribution, it is best to [create a `C:\ci\ws\install\include` directory](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#important-notices)
* Clone this project.
  ```powershell
  git clone git@github.com:RobotecAI/ros2-for-unity.git C:\dev\ros2-for-unity
  ```
* Source your ROS2 installation (`C:\dev\ros2_foxy\local_setup.ps1`) in the terminal before you proceed.
  ```
  C:\dev\ros2_foxy\local_setup.ps1
  ```
* Enter `Ros2ForUnity` working directory.
    ```powershell
    cd C:\dev\ros2-for-unity
    ```
* Set up you custom messages in `ros2_for_unity_custom_messages.repos`
* Import necessary and custom messages repositories.
    ```powershell
    .\pull_repositories.ps1
    ```
    > *NOTE* `pull_repositories.ps1` script doesn't update already existing repositories, you have to remove `src\ros2cs` folder to re-import new versions.
* Build `Ros2ForUnty`. You can build it in standalone or overlay mode.
    ```powershell
    # standalone mode
    ./build.ps1 -standalone
    
    # overlay mode
    ./build.ps1
    ```
  * You can build with `-clean_install` to make sure your installation directory is cleaned before deploying.
* Unity Asset is ready to import into your Unity project. You can find it in `install/asset/` directory.
* (optionally) To create `.unitypackage` in `install/unity_package`
  ```powershell
  create_unity_package.ps1
  ```
  > *NOTE* Please provide path to your Unity executable when prompted. Unity license is required. In case your Unity license has expired, the `create_unity_package.ps1` won't throw any errors but `Ros2ForUnity.unitypackage` won't be generated too.

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
```powershell
pip uninstall numpy
pip install numpy
```

**If no solution of your problem is present in the section above, please make sure to check out `ros2cs` [Troubleshooting section](https://github.com/RobotecAI/ros2cs/blob/master/README-WINDOWS.md#troubleshooting)**

## OS-Specific usage remarks

> If the Asset is built with `-standalone` flag (the default), then nothing extra needs to be done.
Otherwise, you have to source your ros distribution before launching either Unity3D Editor or Application.

> Note that after you build the Asset, you can use it on a machine that has no ros2 installation (if built with `-standalone`).

> You can simply copy over the `Ros2ForUnity` subdirectory to update your Asset.
