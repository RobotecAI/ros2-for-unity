#!/bin/bash
SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

mkdir -p $SCRIPTPATH/../install
docker run \
--rm \
-it \
--name ros2-for-unity \
--user $(id -u):$(id -g) \
-v /etc/passwd:/etc/passwd:ro \
-v /etc/group:/etc/group:ro \
-v /etc/shadow:/etc/shadow:ro \
-v $(pwd)/../install:/workdir/ros2-for-unity/install:rw \
-v $(pwd)/custom_messages:/workdir/custom_messages \
ros2-for-unity \
bash
