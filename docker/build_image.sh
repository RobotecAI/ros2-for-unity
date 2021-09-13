#!/bin/bash

if [ -z "$ROS_DISTRO" ]; then
    echo "Source your ros2 distro first."
    exit 1
fi

docker build . --build-arg ROS2_DISTRO=$ROS_DISTRO --tag ros2-for-unity
