# SPDX-License-Identifier: Apache-2.0
import importlib.util
import os
import shutil
from pathlib import Path
import tempfile
import unittest
from unittest.mock import Mock, patch
from types import SimpleNamespace
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("unity_driver", ROOT / "tools/lyrical.py")
driver = importlib.util.module_from_spec(spec)
spec.loader.exec_module(driver)


class UnityDriverTests(unittest.TestCase):
    def make_install(self, root):
        source = root / "src/Ros2ForUnity/Scripts"
        source.mkdir(parents=True)
        (source / "ROS2UnityComponent.cs").write_text("// fixture")
        install = root / "install"
        (install / "lib/dotnet").mkdir(parents=True)
        (install / "lib/dotnet/ros2cs_core.dll").write_bytes(b"assembly")
        runtime = install / "standalone"
        runtime.mkdir()
        (runtime / "libros2cs_native.so").write_bytes(b"patched library")
        resource = runtime / "share/ament_index/resource_index/packages/rosidl_buffer_backend"
        resource.parent.mkdir(parents=True)
        resource.touch()
        (runtime / "runtime-manifest.json").write_text('{"ros_distro":"lyrical"}')
        binding = SimpleNamespace(
            native_libraries=lambda folder: list(folder.glob("*.so")),
            copy_unique=lambda source, dest: shutil.copy2(source, dest),
            find_library=lambda name, folders: next(folders[0].glob("*.so")))
        return install, binding

    def test_standalone_deploy_preserves_resources_and_replaces_stale_asset(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            install, binding = self.make_install(root)
            old = install / "asset/Ros2ForUnity"
            old.mkdir(parents=True)
            (old / "stale.so").touch()
            with patch.object(driver, "ROOT", root), patch.object(driver.platform, "system", return_value="Linux"), patch.object(driver, "git_metadata", return_value={}):
                asset = driver.deploy(binding, install, True, "rmw_fastrtps_cpp")
            native = asset / "Plugins/Linux/x86_64"
            self.assertTrue((native / "share/ament_index/resource_index/packages/rosidl_buffer_backend").is_file())
            self.assertTrue((native / "runtime-manifest.json").is_file())
            self.assertEqual((native / "libros2cs_native.so").read_bytes(), b"patched library")
            self.assertFalse((asset / "stale.so").exists())

    def test_failed_deploy_preserves_previous_asset(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            install, binding = self.make_install(root)
            (install / "standalone/share/ament_index/resource_index/packages/rosidl_buffer_backend").unlink()
            old = install / "asset/Ros2ForUnity"
            old.mkdir(parents=True)
            (old / "previous").write_text("usable")
            with patch.object(driver, "ROOT", root), patch.object(driver.platform, "system", return_value="Linux"):
                with self.assertRaisesRegex(RuntimeError, "resource is missing"):
                    driver.deploy(binding, install, True, "rmw_fastrtps_cpp")
            self.assertEqual((old / "previous").read_text(), "usable")

    def test_clean_install_retains_asset_when_build_fails(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            asset = root / "install/asset"
            asset.mkdir(parents=True)
            binding = Mock()
            binding.build.side_effect = RuntimeError("profile changed")
            with patch.object(driver, "ROOT", root), patch.object(driver, "load_binding", return_value=binding):
                with self.assertRaisesRegex(RuntimeError, "profile changed"):
                    driver.build(clean_install=True)
            self.assertTrue(asset.is_dir())

    def test_generator_forwarded_to_binding(self):
        binding = Mock()
        binding.check_environment.return_value = "rmw_cyclonedds_cpp"
        with patch.object(driver, "load_binding", return_value=binding), patch.object(driver, "deploy"):
            driver.build(True, True, False, "rmw_cyclonedds_cpp", "Ninja")
        binding.build.assert_called_once_with(ROOT, True, True, "rmw_cyclonedds_cpp", "Ninja")

    def test_metadata_records_lyrical_rmw_platform_and_commits(self):
        with tempfile.TemporaryDirectory() as temp:
            asset = Path(temp) / "Ros2ForUnity"
            version = {"sha": "a" * 40, "desc": "test", "date": "2026-09-14"}
            driver.write_metadata(asset, True, "rmw_cyclonedds_cpp", "Windows", version, version)
            root = ET.parse(asset / "Plugins/Windows/x86_64/metadata_ros2cs.xml").getroot()
            self.assertEqual(root.findtext("ros2"), "lyrical")
            self.assertEqual(root.findtext("standalone"), "1")
            self.assertEqual(root.findtext("rmw_implementation"), "rmw_cyclonedds_cpp")
            self.assertEqual(root.findtext("version/sha"), "a" * 40)
            self.assertEqual(root.findtext("architecture"), "x86_64")
            self.assertEqual((asset / "metadata_ros2cs.xml").read_bytes(), (asset / "Plugins/metadata_ros2cs.xml").read_bytes())

    def test_nonstandalone_metadata(self):
        with tempfile.TemporaryDirectory() as temp:
            asset = Path(temp)
            driver.write_metadata(asset, False, "rmw_fastrtps_cpp", "Linux", {}, {})
            self.assertEqual(ET.parse(asset / "Plugins/Linux/x86_64/metadata_ros2cs.xml").getroot().findtext("standalone"), "0")

    def test_binding_missing_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            with self.assertRaisesRegex(RuntimeError, "Adapted ros2cs"):
                driver.load_binding(Path(temp))

    def test_wrong_distro_does_not_clone(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "jazzy"}, clear=True), patch.object(driver, "run") as run:
            with self.assertRaises(RuntimeError):
                driver.fetch()
            run.assert_not_called()

    def test_missing_local_source_does_not_fetch_upstream_jazzy(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            with patch.object(driver, "ROOT", root), patch.dict(os.environ, {"ROS_DISTRO": "lyrical"}, clear=True), patch.object(driver, "run") as run:
                with self.assertRaisesRegex(RuntimeError, "adapted ros2cs"):
                    driver.fetch(root / "does-not-exist")
                run.assert_not_called()

    def test_csharp_safety_checks_are_before_init(self):
        # Source-level assertion, NOT a C# compilation or Unity runtime test.
        source = (ROOT / "src/Ros2ForUnity/Scripts/ROS2ForUnity.cs").read_text()
        self.assertIn('#if UNITY_EDITOR\nusing UnityEditor;\n#endif', source)
        self.assertIn('"lyrical"', source)
        constructor = source[source.index('    internal ROS2ForUnity()'):]
        self.assertLess(constructor.index("CheckIntegrity();"), constructor.index("Ros2cs.Init();"))
        self.assertIn('throw new InvalidOperationException("ROS2 distribution mismatch:', source)
        self.assertIn('SetRuntimeEnvironment("RMW_IMPLEMENTATION", packaged);', source)
        self.assertIn('Application.Quit(ROS_BAD_VERSION_CODE);\n            throw', source)

    def test_bash_entrypoint_parses(self):
        import subprocess
        import shutil
        if not shutil.which("bash"):
            self.skipTest("bash not installed")
        for name in ("build.sh", "pull_repositories.sh"):
            subprocess.run(["bash", "-n", ROOT / name], check=True)


if __name__ == "__main__":
    unittest.main()
