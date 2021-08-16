#!/bin/bash

SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

if [ $# -eq 0 ] || [ $1 = "-h" ] || [ $1 = "--help" ]; then
  echo "Usage:" 
  echo "deploy_unity_plugins.ps1 <PLUGINS_DIR>"
  echo ""
  echo "PLUGINS_DIR - Assets/ROS2/Plugins/ directory of Unity project."
  exit 1
fi

pluginDir=$1

cp --verbose $SCRIPTPATH/install/lib/dotnet/* ${pluginDir}
mkdir -p  ${pluginDir}/Linux/x86_64/
cp --verbose $SCRIPTPATH/install/standalone/* ${pluginDir}/Linux/x86_64/
cp --verbose $SCRIPTPATH/install/lib/*.so ${pluginDir}/Linux/x86_64/
