# Lyrical adapter checks

Run driver regression tests on Windows or Linux:

```bash
python3 -m unittest discover -s tests/lyrical -v
```

After a standalone Linux build, in the sourced Lyrical environment with .NET 10:

```bash
export ROS_DOMAIN_ID=86 ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST
bash tests/lyrical/run_adapter_checks.sh
```

The script accepts optional asset and output directory arguments. It compiles all
Unity scripts against real ros2cs assemblies in Editor and Player preprocessor
modes, with `ENABLE_MONO` defined. `harness/UnityStubs.cs` supplies the small Unity
API surface used by those scripts; **the Unity engine and Mono runtime are not
executed**. The checks exercise the actual adapter, post-build hook and ROS native
libraries under .NET 10. They cover manifest hashes, versioned ELF files, runtime
resources, managed/native environment synchronization, middleware selection,
three lifecycle/pub/sub/service cycles, failure diagnostics and a separate rclpy
round trip.

To include a custom message with a string and variable-length integer array,
copy the fixture into the source workspace **before building**:

```bash
cp -R tests/lyrical/custom_interfaces src/unity_lyrical_test_interfaces
python3 tools/lyrical.py build --with-tests --standalone
HarnessCustom=true bash tests/lyrical/run_adapter_checks.sh
```

The `tests/COLCON_IGNORE` marker keeps test fixtures and the harness out of normal
colcon discovery. The copied package is deliberately opt-in.

To test the generated Player-style directory on Ubuntu 26.04 without any ROS
installation:

```bash
docker build -f tests/lyrical/Dockerfile.runtime -t ros2-for-unity:lyrical-runtime .
docker run --rm \
  -v "$PWD/install/adapter-checks:/checks:ro" \
  -e ROS_DOMAIN_ID=86 -e ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST \
  ros2-for-unity:lyrical-runtime \
  /checks/player/Harness.dll runtime /checks/App_Data
```

This still uses the test harness. Real Unity Editor/Player import, actual
Play/Stop callbacks and application shutdown, Windows DLL loading, and IL2CPP
require their own acceptance runs.
