#!/usr/bin/env bash
# Run in the Linux Lyrical build container after a standalone asset build.
# Uses real ROS libraries with test-only Unity API stubs, not the Unity engine.
set -euo pipefail
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
ASSET="${1:-$ROOT/install/asset/Ros2ForUnity}"
OUTPUT="${2:-$ROOT/install/adapter-checks}"
mkdir -p "$OUTPUT"
ASSET="$(realpath "$ASSET")"
OUTPUT="$(realpath "$OUTPUT")"
export RosAssemblies="$ASSET/Plugins"
unset HarnessEditor
dotnet build "$ROOT/tests/lyrical/harness/Harness.csproj" -o "$OUTPUT/player"
export HarnessEditor=true
dotnet build "$ROOT/tests/lyrical/harness/Harness.csproj" -o "$OUTPUT/editor"
unset HarnessEditor

# Simulate Unity's copy of unversioned native plugins, then exercise our actual
# post-build hook for metadata, SONAME libraries and non-plugin resources.
mkdir -p "$OUTPUT/App_Data/Plugins"
cp "$ASSET"/Plugins/Linux/x86_64/*.so "$OUTPUT/App_Data/Plugins/"
dotnet "$OUTPUT/editor/Harness.dll" postinstall "$(dirname "$ASSET")" "$OUTPUT/App.x86_64"

python3 - "$ASSET/Plugins/Linux/x86_64" "$OUTPUT/App_Data/Plugins" <<'PY'
import hashlib, json, pathlib, sys
for folder in map(pathlib.Path, sys.argv[1:]):
    manifest = json.loads((folder / 'runtime-manifest.json').read_text())
    for entry in manifest['files']:
        path = folder / entry['name']
        assert path.is_file(), path
        assert hashlib.sha256(path.read_bytes()).hexdigest() == entry['sha256'], path
    print('PASS manifest:', folder, len(manifest['files']), 'files')
PY

# Do not source ROS or preconfigure native/ament lookup: initialization must
# establish its own paths and RMW selection from the packaged metadata.
clean_env=(env -u ROS_DISTRO -u RMW_IMPLEMENTATION -u AMENT_PREFIX_PATH
    -u CMAKE_PREFIX_PATH -u COLCON_PREFIX_PATH -u LD_LIBRARY_PATH
    -u ROS_PACKAGE_PATH -u PYTHONPATH)
"${clean_env[@]}" dotnet "$OUTPUT/editor/Harness.dll" runtime "$(dirname "$ASSET")"
"${clean_env[@]}" dotnet "$OUTPUT/player/Harness.dll" runtime "$OUTPUT/App_Data"
"${clean_env[@]}" ROS_DISTRO=jazzy dotnet "$OUTPUT/player/Harness.dll" reject "$OUTPUT/App_Data" "cannot load with sourced ROS2"
"${clean_env[@]}" RMW_IMPLEMENTATION=unpackaged_rmw dotnet "$OUTPUT/player/Harness.dll" reject "$OUTPUT/App_Data" "was not packaged"
mkdir -p "$OUTPUT/Missing_Data/Plugins"
cp "$OUTPUT/App_Data/metadata_ros2_for_unity.xml" "$OUTPUT/Missing_Data/"
cp "$OUTPUT/App_Data/Plugins/metadata_ros2cs.xml" "$OUTPUT/Missing_Data/Plugins/"
"${clean_env[@]}" dotnet "$OUTPUT/player/Harness.dll" reject "$OUTPUT/Missing_Data" "Missing Lyrical runtime resource"

# Use a separate sourced rclpy process for the round trip.
python3 "$ROOT/tests/lyrical/harness/peer.py" > "$OUTPUT/peer.log" 2>&1 &
peer=$!
trap 'kill "$peer" 2>/dev/null || true' EXIT
"${clean_env[@]}" dotnet "$OUTPUT/player/Harness.dll" interop "$OUTPUT/App_Data"
wait "$peer"
trap - EXIT
cat "$OUTPUT/peer.log"
