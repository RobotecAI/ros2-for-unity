#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Build a Lyrical Unity asset from this checkout and an adapted ros2cs checkout."""
from __future__ import annotations
import argparse
import importlib.util
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
RMWS = ("rmw_fastrtps_cpp", "rmw_cyclonedds_cpp")


def run(args, cwd=None, capture=False):
    print("+ " + subprocess.list2cmdline([str(a) for a in args]), flush=True)
    return subprocess.run([str(a) for a in args], cwd=cwd, check=True, text=True,
                          stdout=subprocess.PIPE if capture else None).stdout


def load_binding(root=None):
    root = ROOT if root is None else root
    path = root / "src/ros2cs/tools/lyrical.py"
    if not path.is_file():
        raise RuntimeError("Adapted ros2cs is missing. Run pull_repositories.sh/.ps1 first.")
    spec = importlib.util.spec_from_file_location("ros2cs_lyrical_driver", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def fetch(source=None, custom=False):
    if os.environ.get("ROS_DISTRO") != "lyrical":
        raise RuntimeError("Source ROS 2 Lyrical before fetching dependencies.")
    target = ROOT / "src/ros2cs"
    candidate = source or os.environ.get("ROS2CS_SOURCE") or ROOT.parent / "ros2cs"
    candidate = Path(candidate).resolve()
    if not target.exists():
        if not (candidate / "tools/lyrical.py").is_file() or not (candidate / ".git").exists():
            raise RuntimeError("Provide the adapted ros2cs repository via --ros2cs-source or ROS2CS_SOURCE; the sibling ../ros2cs is the default. No upstream Jazzy checkout will be substituted.")
        if run(["git", "status", "--porcelain"], cwd=candidate, capture=True).strip():
            raise RuntimeError("Commit or stash changes in the source ros2cs checkout before cloning it.")
        target.parent.mkdir(parents=True, exist_ok=True)
        run(["git", "clone", "--no-hardlinks", candidate, target])
    # Never checkout/reset/overwrite an existing repository implicitly.
    binding = load_binding()
    binding.full_checkout()
    binding.fetch(custom=False)
    if custom:
        run(["vcs", "import", "--input", ROOT / "ros2_for_unity_custom_messages.repos", ROOT], cwd=ROOT)


def git_metadata(repository):
    def git(*args):
        return run(["git", *args], cwd=repository, capture=True).strip()
    return {"sha": git("rev-parse", "HEAD"), "desc": git("describe", "--tags", "--always"),
            "date": git("show", "-s", "--format=%ci"),
            "dirty": str(bool(git("status", "--porcelain"))).lower()}


def write_metadata(asset, standalone, rmw, os_name, unity_meta, binding_meta):
    def document(tag, version):
        root = ET.Element(tag)
        ET.SubElement(root, "ros2").text = "lyrical"
        v = ET.SubElement(root, "version")
        for key, value in version.items():
            ET.SubElement(v, key).text = value
        ET.SubElement(root, "platform").text = os_name
        ET.SubElement(root, "architecture").text = "x86_64"
        return root
    unity = document("ros2_for_unity", unity_meta)
    binding = document("ros2cs", binding_meta)
    ET.SubElement(binding, "standalone").text = str(int(standalone))
    ET.SubElement(binding, "rmw_implementation").text = rmw
    native = asset / "Plugins" / os_name / "x86_64"
    for file, doc in ((asset / "metadata_ros2_for_unity.xml", unity),
                      (asset / "metadata_ros2cs.xml", binding),
                      (asset / "Plugins/metadata_ros2cs.xml", binding),
                      (native / "metadata_ros2cs.xml", binding)):
        file.parent.mkdir(parents=True, exist_ok=True)
        ET.ElementTree(doc).write(file, encoding="utf-8", xml_declaration=True)


def deploy(binding, install, standalone, rmw):
    source = ROOT / "src/Ros2ForUnity"
    # A buildable full checkout, not just the patched startup file, is required.
    if not (source / "Scripts/ROS2UnityComponent.cs").is_file():
        raise RuntimeError("Unity source tree is incomplete. Restore the full repository before building.")
    os_name = platform.system()
    with tempfile.TemporaryDirectory(prefix="lyrical-asset-", dir=install) as temporary:
        asset = Path(temporary) / "Ros2ForUnity"
        shutil.copytree(source, asset)
        plugins = asset / "Plugins"
        # Do not carry native binaries from an earlier build or another OS.
        if plugins.exists():
            shutil.rmtree(plugins)
        native = plugins / os_name / "x86_64"
        native.mkdir(parents=True)
        assemblies = sorted((install / "lib/dotnet").glob("*.dll"))
        if not assemblies:
            raise RuntimeError("No managed ROS assemblies were built.")
        for assembly in assemblies:
            shutil.copy2(assembly, plugins / assembly.name)
        natives = install / ("bin" if os_name == "Windows" else "lib")
        # Bundled libraries already have their ELF RPATH patched; don't replace them
        # afterwards with their unpatched install/lib counterparts.
        libraries = binding.native_libraries(install / "standalone" if standalone else natives)
        for library in libraries:
            binding.copy_unique(library, native)
        binding.find_library("ros2cs_native", [native])
        if standalone:
            manifest = install / "standalone/runtime-manifest.json"
            if not manifest.is_file():
                raise RuntimeError("Standalone runtime manifest is missing.")
            # The Lyrical buffer registry queries the ament package index at
            # node initialization, even when no custom buffer plugin is used.
            resources = install / "standalone/share"
            if not (resources / "ament_index/resource_index/packages/rosidl_buffer_backend").is_file():
                raise RuntimeError("Standalone rosidl_buffer_backend resource is missing.")
            shutil.copytree(resources, native / "share")
            shutil.copy2(manifest, native / manifest.name)
        write_metadata(asset, standalone, rmw, os_name, git_metadata(ROOT), git_metadata(ROOT / "src/ros2cs"))
        destination = install / "asset/Ros2ForUnity"
        destination.parent.mkdir(parents=True, exist_ok=True)
        if destination.exists():
            shutil.rmtree(destination)
        shutil.move(str(asset), destination)
    return destination


def build(standalone=False, with_tests=False, clean_install=False, rmw=None, cmake_generator=None):
    binding = load_binding()
    selected = binding.check_environment(rmw)
    # Deployment always replaces the asset after successful staging. Retain the
    # last usable asset on compilation, profile validation or packaging failure,
    # including when --clean-install is supplied.
    install = binding.build(ROOT, standalone, with_tests, selected, cmake_generator)
    asset = deploy(binding, install, standalone, selected)
    print("Unity asset: " + str(asset))


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    p = commands.add_parser("fetch")
    p.add_argument("--ros2cs-source", type=Path)
    p.add_argument("--get-custom-messages", action="store_true")
    p = commands.add_parser("build")
    p.add_argument("--standalone", "-s", action="store_true")
    p.add_argument("--with-tests", "-t", action="store_true")
    p.add_argument("--clean-install", "-c", action="store_true")
    p.add_argument("--rmw", choices=RMWS)
    p.add_argument("--cmake-generator", help="Explicit CMake generator, e.g. Ninja")
    args = parser.parse_args(argv)
    try:
        if args.command == "fetch":
            fetch(args.ros2cs_source, args.get_custom_messages)
        else:
            build(args.standalone, args.with_tests, args.clean_install, args.rmw, args.cmake_generator)
    except (RuntimeError, subprocess.CalledProcessError, OSError, ValueError) as exc:
        print("Lyrical Unity build failed: " + str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
