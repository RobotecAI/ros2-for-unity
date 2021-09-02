#!/bin/bash

SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

display_usage() {
  echo "This script creates a temporary Unity project in '/tmp' directory, copy input asset and makes an unity package out of it. Valid Unity license is required."
  echo ""
  echo "Usage:" 
  echo "create_unity_package.sh -u <UNITY_PATH> -i [INPUT_ASSET] -p [PACKAGE_NAME] -o [OUTPUT_DIR]"
  echo ""
  echo "UNITY_PATH - Unity editor executable path"
  echo "INPUT_ASSET - input asset to pack into unity package, default = 'install/asset/Ros2ForUnity'"
  echo "PACKAGE_NAME - unity package name, default = 'Ros2ForUnity'"
  echo "OUTPUT_DIR - output file directory, default = 'install/unity_package'"
}

UNITY_PATH=""
INPUT_ASSET="install/asset/Ros2ForUnity"
PACKAGE_NAME="Ros2ForUnity"
OUTPUT_DIR="$SCRIPTPATH/install/unity_package"

while [[ $# -gt 0 ]]; do
  key="$1"

  case $key in
    -u|--unity-path)
      UNITY_PATH="$2"
      shift # past argument
      shift # past value
      ;;
    -p|--package_name)
      PACKAGE_NAME="$2"
      shift # past argument
      shift # past value
      ;;
    -i|--input-directory)
      INPUT_ASSET="$2"
      shift # past argument
      shift # past value
      ;;
    -o|--output-directory)
      OUTPUT_DIR="$2"
      shift # past argument
      shift # past value
      ;;
    -h|--help)
      display_usage
      exit 0
      shift # past argument
      ;;
    *)    # unknown option
      shift # past argument
      ;;
  esac
done

if [ -z "$UNITY_PATH" ] || [ -z "$PACKAGE_NAME" ] || [ -z "$INPUT_ASSET" ] || [ -z "$OUTPUT_DIR" ]; then
    echo -e "\nMissing arguments!"
    echo ""
    display_usage
    exit 1
fi

if [ ! -d "$INPUT_ASSET" ]; then
    echo "Input asset '$INPUT_ASSET' doesn't exist!  Use 'build.sh' to build project first."
    exit 1
fi

UNITY_VERSION=`$UNITY_PATH -version`

# Test if unity editor is valid
if [[ $UNITY_VERSION =~ ^[0-9]{4}\.[0-9]*\.[0-9]*[f]?[0-9]*$ ]]; then
    echo "Unity editor confirmed..."
else
    echo "Can't confirm Unity editor. Exiting."
    exit 1
fi

TMP_PROJECT_PATH=/tmp/ros2cs_unity_project/$UNITY_VERSION
# Create temp project
if [ -d "$TMP_PROJECT_PATH" ]; then
    echo "Found existing temporary project for Unity $UNITY_VERSION."
    rm -rf $TMP_PROJECT_PATH/Assets/*
else
  rm -rf $TMP_PROJECT_PATH
  echo "Creating Unity temporary project for Unity $UNITY_VERSION..."
  $UNITY_PATH -createProject $TMP_PROJECT_PATH -batchmode -quit
fi

# Copy asset
echo "Copying asset to export..."
cp -r "$INPUT_ASSET" "$TMP_PROJECT_PATH/Assets/$PACKAGE_NAME"

# Creating asset
echo "Saving unitypackage '$OUTPUT_DIR/$PACKAGE_NAME.unitypackage'..."
mkdir -p $OUTPUT_DIR
$UNITY_PATH -projectPath "$TMP_PROJECT_PATH" -exportPackage "Assets/$PACKAGE_NAME" "$OUTPUT_DIR/$PACKAGE_NAME.unitypackage" -batchmode -quit

# Cleaning up
echo "Cleaning up temporary project..."
rm -rf $TMP_PROJECT_PATH/Assets/*

echo "Done!"

