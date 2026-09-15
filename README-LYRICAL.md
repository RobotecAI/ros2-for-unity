# ROS 2 Lyrical / Unity integration

## Status

This is a **source adaptation candidate**, not a validated binary release. Linux driver/ELF-fixture tests have been run in the delivery environment. Full ROS builds, Windows DLL loading, Unity Editor/Player execution and DDS interoperability have not been run there. The source preserves the upstream programming model; no TCP bridge, new action API, or IL2CPP certification is introduced.

A full checkout of this repository and a committed Lyrical-adapted `ros2cs` checkout are required. The default directory arrangement is:

```text
workspace/
  ros2cs/            # Adapted full dependency checkout
  ros2-for-unity/    # This full checkout
```

## Dependencies and build

Use a sourced x86_64 ROS 2 Lyrical environment and the toolchain described in `ros2cs/README-LYRICAL.md`. Build and run native binaries on matching OS/runtime platforms. The Linux ROS target is Ubuntu 26.04; Unity Editor compatibility with that OS remains a separate validation item. Windows uses Windows 11 and VS 2022 x64 native tools.

From this repository's root, Linux:

```bash
source /opt/ros/lyrical/setup.bash
python3 tools/lyrical.py fetch
python3 tools/lyrical.py build --with-tests --standalone
```

Windows, from a ROS-sourced VS 2022 x64 environment:

```powershell
python tools/lyrical.py fetch
python tools/lyrical.py build --with-tests --standalone
```

`fetch` clones the adapted sibling `../ros2cs` into `src/ros2cs` and fetches its Lyrical dependencies. It will not substitute the upstream Jazzy dependency for a missing adapted repository. To choose another adapted source checkout:

```bash
python3 tools/lyrical.py fetch --ros2cs-source /path/to/ros2cs
```

`ROS2CS_SOURCE` is the environment-variable alternative. The source repository must be committed and clean when cloned. An existing dependency checkout is preserved rather than reset or overwritten. Add `--get-custom-messages` to import `ros2_for_unity_custom_messages.repos` explicitly.

The `pull_repositories.sh/.ps1` and `build.sh/.ps1` entrypoints dispatch to the Lyrical driver. Bash flags are `--with-tests`, `--standalone`, and `--clean-install`; PowerShell flags are `-with_tests`, `-standalone`, and `-clean_install`. The Python entrypoint additionally accepts `--rmw rmw_fastrtps_cpp` or `--rmw rmw_cyclonedds_cpp`. RMW selection creates a distinct build profile. Use a fresh build/install tree when changing profiles.

`--clean-install` clears the generated Unity asset before deployment, not the dependency repository or ROS underlay. It does not waive build-profile mismatch checks.

## Build output

A successful target-environment build deploys:

```text
install/asset/Ros2ForUnity/
  Scripts/
  Plugins/
    ros2cs_common.dll
    ros2cs_core.dll
    ...generated message assemblies...
    Linux/x86_64/           # or Windows/x86_64
      ...native libraries...
      metadata_ros2cs.xml
      runtime-manifest.json  # standalone only
  metadata_ros2_for_unity.xml
  metadata_ros2cs.xml
```

Copy that generated `Ros2ForUnity` directory into the Unity project's `Assets`. Metadata is generated after a successful build and records the two repository commits, Lyrical, platform, x86_64, build type and selected RMW. Staging avoids overlaying stale native binaries from an earlier asset build.

## Runtime initialization

The runtime checks metadata and the sourced distribution before loading ROS libraries. A Unity/ros2cs distribution mismatch throws an exception. A non-standalone build requires the matching sourced ROS environment. A standalone build gets its distribution from the artifact metadata, rejects a different sourced ROS distribution, and validates `RMW_IMPLEMENTATION` against the packaged implementation.

Start standalone validation from a fresh unsourced shell. No `ROS_DISTRO` environment variable is needed for a standalone asset. Native-library paths from another ROS installation can still contaminate a process even without that variable; use an uncontaminated environment rather than merely unsetting `ROS_DISTRO`.

The editor-only namespace is behind `UNITY_EDITOR`. A failed Player initialization throws after requesting application exit, so native initialization does not continue. Linux plugin lookup and ELF dependency lookup are distinct: the former uses the advertised plugin path and the latter uses library RPATH. A bundled Fast DDS artifact cannot switch to Cyclone DDS merely by changing an environment variable; build and deploy the corresponding RMW profile.

## Verification

Local driver checks:

```bash
python3 -m unittest discover -s tests/lyrical -v
```

Target-environment acceptance must cover Unity Editor and a built Player, repeated Play/Stop, application shutdown, publication/subscription, services, standard/custom messages, and two-way communication with an external Lyrical node. Use the upstream example components and inspect their actual topic/service names before issuing ROS CLI commands. Check a freshly started standalone process separately from a ROS-sourced process. Verify both RMW variants in separate artifacts. The delivery does not establish Unity 6/IL2CPP or mixed ROS-distribution interoperability.

## References

- Unity baseline: https://github.com/RobotecAI/ros2-for-unity/tree/0e84e2b10db5e5cd7bf36d55c4987f422f1ece98
- Binding baseline: https://github.com/RobotecAI/ros2cs/tree/e08651b64fe9868c0df9d98d81e77fc40b8d5c92
- Lyrical installation/platforms: https://docs.ros.org/en/lyrical/Installation.html
