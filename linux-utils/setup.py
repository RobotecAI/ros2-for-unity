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

from setuptools import setup
from setuptools import find_packages

package_name = 'ros2-for-unity'
setup(
    name=package_name,
    version='1.0.0',
    packages=find_packages(),
    data_files=[
        ('share/ament_index/resource_index/packages',
            ['resource/' + package_name]),
        ('share/' + package_name, ['package.xml']),
    ],
    py_modules =[
        'start',
        'teleop'
    ],
    install_requires=['setuptools'],
    zip_safe=True,
    maintainer='Adam Dabrowski',
    maintainer_email='adam.dabrowski@robotec.ai',
    description=(
        'Scripts for running Unity demo scene (Editor and App) in ROS2 context'
    ),
    license='Apache Licence 2.0',
    tests_require=['pytest'],
    entry_points={
        'console_scripts': [
            'start=start:main',
            'teleop=teleop:main'
        ],
    },
)
