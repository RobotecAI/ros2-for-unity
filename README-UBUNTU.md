# ROS2 For Unity - Ubuntu 20.04

This readme contains information specific to Ubuntu 20.04. For general information, please see README.md 

## Building

### Prerequisites

*  ROS2 installed on the system, along with `test-msgs`, `cyclonedds` and `fastrtps` packages
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core 3.1 sdk - [see here](https://www.microsoft.com/net/learn/get-started)

The following script can be used to install the aforementioned prerequisites:

```bash
# Install rmw and tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs 
apt install -y ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp
apt install -y ros-${ROS_DISTRO}-cyclonedds ros-${ROS_DISTRO}-rmw-cyclonedds-cpp

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
