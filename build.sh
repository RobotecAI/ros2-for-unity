#!/bin/bash
SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

if [ -z "${ROS_DISTRO}" ]; then
    echo "Source your ros2 distro first (Foxy and Galactic are supported)"
    exit 1
fi

TESTS=0
MSG="Build started."
if [ "$1" = "--with-tests" ]; then
    TESTS=1
    MSG="$MSG (with tests)"
elif [ "$1" = "-h" ] || [ "$1" = "--help" ]; then
    echo "Usage: "
    echo "build.sh [--with-tests]"
    echo ""
    echo "Options:"
    echo "--with-tests - build with tests."
    exit 1
fi

echo $MSG
#TODO - call ros2cs ./build.sh instead, but with this workspace directory (parametrize the script)
colcon build --merge-install --event-handlers console_direct+ --cmake-args -DCMAKE_BUILD_TYPE=Release -DBUILD_TESTING=$TESTS -DCMAKE_SHARED_LINKER_FLAGS="-Wl,-rpath=." && $SCRIPTPATH/deploy_unity_plugins.sh $SCRIPTPATH/src/Ros2ForUnity/Plugins/
