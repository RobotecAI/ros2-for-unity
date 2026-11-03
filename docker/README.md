Ros2 For Unity Docker
===============

Currently only building asset on Ubuntu is supported. Build windows version is not supported.

## Lyrical

From the repository root, with the committed adapted `ros2cs` checkout alongside it:

```bash
docker build -f docker/Dockerfile.lyrical -t ros2-for-unity:lyrical .
docker run --rm -it \
  -v "$PWD:/work/ros2-for-unity" \
  -v "$(realpath ../ros2cs):/work/ros2cs:ro" \
  ros2-for-unity:lyrical
```

Inside the container (the image entrypoint sources Lyrical):

```bash
python3 tools/lyrical.py fetch
python3 tools/lyrical.py build --with-tests --standalone
ROS_DOMAIN_ID=86 ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST \
  bash tests/lyrical/run_adapter_checks.sh
```

The asset is written to the host's `install/asset/Ros2ForUnity`. The image uses Ubuntu 26.04, the .NET 10 SDK and both supported DDS implementations. Docker can be used in place of Docker/Podman. Use a fresh workspace when changing build profiles, and do not reuse Windows build output in the Linux container. See [Lyrical status and runtime requirements](../README-LYRICAL.md).

The following instructions describe the legacy images.

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
