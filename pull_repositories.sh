#!/bin/bash

# Lyrical uses the checked, cross-platform build driver.
if [ "${ROS_DISTRO:-}" = "lyrical" ]; then
  SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
  exec python3 "$SCRIPT_ROOT/tools/lyrical.py" fetch "$@"
fi

SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

if [ -z "${ROS_DISTRO}" ]; then
    echo "Can't detect ROS2 version. Source your ros2 distro first. Foxy, Galactic, Humble, Jazzy are supported"
    exit 1
fi

echo "========================================="
echo "* Pulling ros2cs repository:"
vcs import < "ros2cs.repos"

echo ""
echo "========================================="
echo "Pulling custom repositories:"
vcs import < "ros2_for_unity_custom_messages.repos"

echo ""
echo "========================================="
echo "Pulling ros2cs dependencies:"
cd "$SCRIPTPATH/src/ros2cs"
./get_repos.sh
cd -
