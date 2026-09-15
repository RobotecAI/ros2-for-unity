# SPDX-License-Identifier: Apache-2.0
import importlib.util
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("unity_driver", ROOT / "tools/lyrical.py")
driver = importlib.util.module_from_spec(spec)
spec.loader.exec_module(driver)


class UnityDriverTests(unittest.TestCase):
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
        self.assertIn('Environment.SetEnvironmentVariable("RMW_IMPLEMENTATION", packaged);', source)
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
