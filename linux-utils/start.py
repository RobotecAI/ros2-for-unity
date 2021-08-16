# Copyright 2019-2021 Robotec.ai.
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
import re
import subprocess
import argparse
from pathlib import Path

def find_unity_editors(editor_path):
    unity_editor_paths = {}
    executable_name = "Unity.exe" if os.name == "nt" else "Unity"

    if not editor_path:
        potential_paths = []
        if os.name == 'nt':
            windows_default_path = Path(r'C:\Program Files\Unity\Hub\Editor')
            if windows_default_path.exists():
                potential_paths.append(windows_default_path)
        else:
            linux_default_path = Path.home().joinpath('Unity/Hub/Editor')
            if linux_default_path.exists():
                potential_paths.append(linux_default_path)

        for path in potential_paths:
            for p in path.iterdir():
                if p.joinpath('Editor', executable_name).is_file():
                    unity_editor_paths.update({p.name: p.joinpath('Editor', executable_name)})
    else:
        e_p = Path(editor_path)
        if e_p.exists() and e_p.name == executable_name:
            unity_editor_paths.update({e_p.parents[1].name: e_p})

    if unity_editor_paths == {}:
        raise Exception('Could not determine Unity Editor path.')

    return unity_editor_paths

def get_unity_version(project_path):
    version_file_path = Path(project_path).joinpath(
        'ProjectSettings', 'ProjectVersion.txt')
    print("Looking for version information in the project file: '{}'".format(version_file_path))
    try:
        with open(version_file_path, 'r') as version_file:
            contents = version_file.read()
            matches = re.search(r'm_EditorVersion: (.*)', contents)
            if matches:
                print("Found versions in project: '{}'".format(matches.groups()[0]))
                return matches.groups()[0]
    except FileNotFoundError as fnfe:
        raise Exception("Project '{}' is not a valid Unity project.".format(project_path))

def setup_environment(target_path, target_type, plugin_name):
    os_name = "Windows" if os.name == "nt" else "Linux"
    plugin_directory = ""
    if target_type == "editor":
        plugin_directory = Path(target_path).joinpath('Assets', plugin_name,  'Plugins', os_name, 'x86_64')
    else:
        plugin_directory = Path(target_path).joinpath('Plugins')

    if os_name == "Windows":
        if not (str(plugin_directory) in os.environ['PATH']):
            final_plugin_directory = plugin_directory if target_type == "editor" else plugin_directory.joinpath('x86_64')
            os.environ['PATH'] = os.environ['PATH'] + os.pathsep + str(final_plugin_directory)

    if os_name == "Linux":
        if 'LD_LIBRARY_PATH' in os.environ:
            os.environ['LD_LIBRARY_PATH'] = os.environ['LD_LIBRARY_PATH'] + os.pathsep + str(plugin_directory)
        else:
            os.environ['LD_LIBRARY_PATH'] = str(plugin_directory)

def run_app(app_path):
    application_path = str(app_path) + ".exe" if os.name == "nt" else str(app_path) + ".x86_64"
    if Path(application_path).exists():
        print('\n---------------------')
        print("Running: ")
        print("\tApplication: '{}'".format(application_path))
        print('---------------------\n')
        subprocess.call([application_path, '-logFile'])
    else:
        raise Exception("Application '{}' does not exist".format(
            application_path))

def run_editor(project_path, unity_editor):
    version = get_unity_version(project_path)
    editor = find_unity_editors(unity_editor).get(version, None)
    if editor:
        print('\n---------------------')
        print("Running: ")
        print("\tProject: '{}'".format(project_path))
        print("\tVersion: '{}'".format(version))
        print("\tEditor: '{}'".format(editor))
        print('---------------------\n')
        subprocess.call([editor, '-projectPath', project_path])
    else:
        raise Exception("No valid editor found for project '{}' version {}".format(
            project_path, version))

def setup_plugins(plugins_directory, target_type):
    # TODO - instead, search for plugins in Asset subfolders with wither a certain marker in their directory
    # (i.e. a file with a specific name), or any with a Plugins directory.
    setup_environment(plugins_directory, target_type, "ROS2")
    setup_environment(plugins_directory, target_type, "RobotecGPULidar")

def main():
    # TODO(piotr.jaroszek) Find a better way to get default project
    relative_path_levels = 3 if os.name == "nt" else 4
    default_project_name = 'ros2-for-unity'
    default_build_directory = Path(__file__).resolve().parents[relative_path_levels].joinpath(
                'src', 'ros2-for-unity', 'Builds')
    default_project_path = Path(__file__).resolve().parents[relative_path_levels].joinpath(
        'src', 'ros2-for-unity', default_project_name)

    parser = argparse.ArgumentParser(description='Run unity editor with demo scene project')
    entry_subparsers = parser.add_subparsers(help="What to start.", dest="target_subparser")

    editor_subparser = entry_subparsers.add_parser('editor', help="Starts editor.")
    editor_subparser.add_argument('-p', '--project-path', default=default_project_path,
                        help='Unity project path. Default: {}'.format(default_project_path))
    editor_subparser.add_argument('-e','--editor', help="Unity Editor executable file path.")

    application_subparser = entry_subparsers.add_parser('app', help="Starts application.")
    application_subparser.add_argument('-n','--name',
        help="Application build name. Default: {}".format(default_project_name),
        default=default_project_name
    )
    application_subparser.add_argument('-p', '--build_directory',
        help="Application build directory path. Default: {}".format(default_build_directory),
        default=default_build_directory)

    args = parser.parse_args()

    if not args.target_subparser:
        parser.print_help()
        exit()
    elif args.target_subparser == "editor":
        if not Path(args.project_path).exists():
            raise Exception("Could not find the project '{}'.".format(args.project_path))
        setup_plugins(args.project_path, args.target_subparser)
        run_editor(args.project_path, args.editor)
    elif args.target_subparser == "app":
        plugins_directory = args.build_directory.joinpath(args.name + "_Data")
        setup_plugins(plugins_directory, args.target_subparser)
        app_path = args.build_directory.joinpath(args.name)
        run_app(app_path)

if __name__ == '__main__':
    main()
