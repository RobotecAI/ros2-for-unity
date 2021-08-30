# ROS2 For Unity - Ubuntu 20.04

This readme contains information specific to Ubuntu 20.04. For general information, please see [README.md](README.md)

## Building

### Prerequisites

To prepare your machine for `ROS2 For Unity` build please follow step by step the `ros2cs` [Prequisites section](https://github.com/RobotecAI/ros2cs/blob/master/README-UBUNTU.md#prequisites).
Prepairing environment with `ros2cs` instruction is ***crucial*** for failure-free Unity Asset generation.

**Ommitting any of the steps mentioned above will result in a build failure!**

### Steps

*  Clone this project.
*  You need to source your ROS2 installation (`source /opt/ros/foxy/setup.bash`) before you proceed, for each new open terminal. It is convenient to include this command in your `~/.profile` file.
*  Run `pull_repositories.sh`. This will pull `ros2cs` as well as your custom messages. You will be asked for gitlab credentials, so please fill your information.
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
