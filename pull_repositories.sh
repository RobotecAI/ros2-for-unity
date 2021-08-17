#!/bin/bash

SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

if [ -z "${ROS_DISTRO}" ]; then
    echo "Can't detect ROS2 version. Source your ros2 distro first. Foxy and Galactic are supported"
    exit 1
fi

vcs import < "ros2cs.repos"
vcs import < "ros2_for_unity_custom_messages.repos"
cd "$SCRIPTPATH/src/ros2cs"
./get_repos.sh
cd -
