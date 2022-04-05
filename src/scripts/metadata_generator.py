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

import argparse
import xml.etree.ElementTree as ET
from xml.dom import minidom
import subprocess
import pathlib
import os

parser = argparse.ArgumentParser(description='Generate metadata file for ros2-for-unity.')
parser.add_argument('--standalone', action='store_true', help='is a standalone build')
args = parser.parse_args()

def get_git_commit(working_directory) -> str:
    return subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=working_directory).decode('ascii').strip()

def get_git_description(working_directory) -> str:
    return subprocess.check_output(['git', 'describe', '--tags', '--always'], cwd=working_directory).decode('ascii').strip()

def get_commit_date(working_directory) -> str:
    return subprocess.check_output(['git', 'show', '-s', '--format=%ci'], cwd=working_directory).decode('ascii').strip()

def get_git_abbrev(working_directory) -> str:
    return subprocess.check_output(['git', 'rev-parse', '--abbrev-ref', 'HEAD'], cwd=working_directory).decode('ascii').strip()

def get_ros2_for_unity_root_path() -> pathlib.Path:
    return pathlib.Path(__file__).parents[2]

def get_ros2_for_unity_path() -> pathlib.Path:
    return pathlib.Path(__file__).parents[1].joinpath("Ros2ForUnity")

def get_ros2cs_path() -> pathlib.Path:
    return pathlib.Path(__file__).parents[1].joinpath("ros2cs")

def get_ros2_path() -> pathlib.Path:
    return get_ros2cs_path().joinpath("src").joinpath("ros2").joinpath("rcl_interfaces")

def get_ros2_version() -> str:
    return os.environ.get("ROS_DISTRO", "unknown")

ros2_for_unity = ET.Element("ros2_for_unity")
ET.SubElement(ros2_for_unity, "ros2").text = get_ros2_version()
ros2_for_unity_version = ET.SubElement(ros2_for_unity, "version")
ET.SubElement(ros2_for_unity_version, "sha").text = get_git_commit(get_ros2_for_unity_root_path())
ET.SubElement(ros2_for_unity_version, "desc").text = get_git_description(get_ros2_for_unity_root_path())
ET.SubElement(ros2_for_unity_version, "date").text = get_commit_date(get_ros2_for_unity_root_path())

ros2_cs = ET.Element("ros2cs")
ET.SubElement(ros2_cs, "ros2").text = get_ros2_version()
ros2_cs_version = ET.SubElement(ros2_cs, "version")
ET.SubElement(ros2_cs_version, "sha").text = get_git_commit(get_ros2cs_path())
ET.SubElement(ros2_cs_version, "desc").text = get_git_description(get_ros2cs_path())
ET.SubElement(ros2_cs_version, "date").text = get_commit_date(get_ros2cs_path())
ET.SubElement(ros2_cs, "standalone").text = str(int(args.standalone))

rf2u_xmlstr = minidom.parseString(ET.tostring(ros2_for_unity)).toprettyxml(indent="   ")
metadata_rf2u_file = get_ros2_for_unity_path().joinpath("metadata_ros2_for_unity.xml")
with open(str(metadata_rf2u_file), "w") as f:
    f.write(rf2u_xmlstr)
    
r2cs_xmlstr = minidom.parseString(ET.tostring(ros2_cs)).toprettyxml(indent="   ")
metadata_r2cs_file = get_ros2_for_unity_path().joinpath("metadata_ros2cs.xml")
with open(str(metadata_r2cs_file), "w") as f:
    f.write(r2cs_xmlstr)
