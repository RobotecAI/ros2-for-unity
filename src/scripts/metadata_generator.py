# Copyright 2019-2022 Robotec.ai.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import os
from argparse import ArgumentParser
from pathlib import Path
from xml.dom import minidom
from xml.etree import ElementTree as ET

PARSER = ArgumentParser(description="Generate metadata file for ros2-for-unity.")
PARSER.add_argument("--standalone", action="store_true", help="Whether the build is a standalone build")

METADATA_PATH = Path(__file__).parents[1] / "Ros2ForUnity" / "Resources" / "ros2-for-unity.xml"


def get_ros2_version() -> str:
    return os.environ.get("ROS_DISTRO", "unknown")


if __name__ == "__main__":
    args = PARSER.parse_args()
    metadata = ET.Element("ros2-for-unity")
    ET.SubElement(metadata, "ros").text = get_ros2_version()
    ET.SubElement(metadata, "standalone").text = "true" if args.standalone else "false"
    document = minidom.parseString(ET.tostring(metadata)).toprettyxml()
    with open(METADATA_PATH, "w") as file:
        file.write(document)
