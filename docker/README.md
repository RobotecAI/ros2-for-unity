Ros2 For Unity Docker
===============

Currently only building asset on Ubuntu is supported. Build windows version is not supported.

## Build docker image

1. Source ROS2 (foxy or galactic):

```bash
. /opt/ros/<ROS_DISTRO>/setup.bash
```

2. Build image - image will be based on sourced ROS2 version:

```bash
./build_image.sh
```

## Using docker container

1. Run docker container. Container will fetch `master` version of `ros2-for-unity`:

```bash
./run_container.sh
```

2. Build asset. `./run_container.sh` script mounts `install` host directory inside docker, so you can find install results on host machine:

```bash
./build.sh --with-tests
```

## Adding custom messages

You can add custom messages by putting them inside `docker/custom_messages` folder or just simply `git clone` them inside docker containers `/workdir/ros2-for-unity/src/ros2cs/src/custom_messages`