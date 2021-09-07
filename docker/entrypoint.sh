#!/bin/bash

source "/opt/ros/$ROS_DISTRO/setup.bash"

echo "######################################################################"
echo ""
echo "Cloning recent version of 'ros2-for-unity'"
echo ""
echo "######################################################################"
echo ""

git clone https://github.com/RobotecAI/ros2-for-unity.git /workdir/.ros2-for-unity

shopt -s dotglob
mkdir -p /workdir/ros2-for-unity
mv /workdir/.ros2-for-unity/* /workdir/ros2-for-unity
cd /workdir/ros2-for-unity/ && ./pull_repositories.sh
shopt -u dotglob

ln -s /workdir/custom_messages /workdir/ros2-for-unity/src/ros2cs/src/custom_messages

echo ""
echo "######################################################################"
echo ""
echo "Welcome to 'ros2-for-unity' docker container. Your ROS2 distro is $ROS_DISTRO."
echo ""
echo "Type './build.sh' to build 'ros2-for-unity'. You will find installed libs on your host machine inside 'install' directory"
echo ""
echo "######################################################################"
echo ""

exec bash
