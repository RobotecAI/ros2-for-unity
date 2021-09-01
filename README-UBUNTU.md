# ROS2 For Unity - Ubuntu 20.04

This readme contains information specific to Ubuntu 20.04. For general information, please see [README.md](README.md)

## Building

### Prerequisites

Start with installation of dependencies. Make sure to complete each step of `ros2cs` [Prerequisites section](https://github.com/RobotecAI/ros2cs/blob/master/README-UBUNTU.md#prerequisites).

### Steps

*  Clone this project.
*  You need to source your ROS2 installation (`source /opt/ros/foxy/setup.bash`) before you proceed, for each new open terminal. It is convenient to include this command in your `~/.profile` file.
*  Run `pull_repositories.sh`. This will pull `ros2cs` as well as your custom messages. You might be asked for gitlab credentials. Remember to **pull this repository with each update** (e.g. with `vcs pull`).
*  Run `build.sh` script.
    * You can build tests by adding `--with-tests` argument to `build` command.
    * It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
    * It deploys built plugins into the Asset directory. Note that only plugins built for the current platform will be deployed (there is no cross-compilation).
    * It prepares Unity Asset that is ready to import into your Unity project.
* Run `create_unity_asset.sh -u <your-path-to-unity-editor-executable>` to generate .unitypackage file in `install/unity_package`

## OS-Specific usage remarks

You can run Unity Editor or App executable from GUI (clicking) or from terminal as long as ROS2 is sourced in your environment.
The best way to ensure that system-wide is to add `source /opt/ros/foxy/setup.bash` to your `~/.profile` file.
Note that you need to re-log for changes in `~/.profile` to take place.
Running Unity Editor through Unity Hub is also supported.

## Usage troubleshooting

* If you see `"No ROS environment sourced. You need to source your ROS2 (..)"` message in Unity3D Editor, it means your environment was not sourced properly. This could happen if you run Unity but it redirects to Hub and ignores your console environment variables (this behavior can depend on Unity3D version). In such case, run project directly with `-projectPath` or add ros2 sourcing to your `~/.profile` file (you need to re-log for it to take effect).
* Keep in mind that UnityHub works the whole time in the background after it's first launch. This means that environment variables are inherited and cached when process is launched for the first time. To change between Unity Assets compiled with different ros2 distributions, please kill existent UnityHub process and run it with the correct ros2 distribution sourced.
