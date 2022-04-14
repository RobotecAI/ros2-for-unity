#!/bin/bash
SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

display_usage() {
    echo "Usage: "
    echo ""
    echo "build.sh -u <UNITY_PATH> [-i INPUT_ASSET] [-p PACKAGE_NAME]  [--with-tests] [--standalone] [--clean-install]"
    echo ""
    echo "Options:"
    echo "UNITY_PATH - Unity editor executable path"
    echo "INPUT_ASSET - input asset to pack into unity package, default = 'install/asset/Ros2ForUnity'"
    echo "PACKAGE_NAME - unity package name, default = 'Ros2ForUnity'"
    echo "--with-tests - build with tests"
    echo "--standalone - standalone version"
    echo "--clean-install - makes a clean installation, removes install directory before deploying"
}

if [ ! -d "$SCRIPTPATH/src/ros2cs" ]; then
    echo "Pull repositories with 'pull_repositories.sh' first."
    exit 1
fi

UNITY_PATH=""
INPUT_ASSET="install/asset/Ros2ForUnity"
PACKAGE_NAME="Ros2ForUnity"
OPTIONS=""
STANDALONE=0
TESTS=0
CLEAN_INSTALL=0

while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    -t|--with-tests)
      OPTIONS="$OPTIONS --with-tests"
      TESTS=1
      shift # past argument
      ;;
    -s|--standalone)
      OPTIONS="$OPTIONS --standalone"
      STANDALONE=1
      shift # past argument
      ;;
    -c|--clean-install)
      CLEAN_INSTALL=1
      shift # past argument
      ;;
    -h|--help)
      display_usage
      exit 0
      shift # past argument
      ;;
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
    *)    # unknown option
      shift # past argument
      ;;
  esac
done

if [ -z "$UNITY_PATH" ] || [ -z "$PACKAGE_NAME" ] || [ -z "$INPUT_ASSET" ]; then
    echo -e "\nMissing unity path argument!"
    echo ""
    display_usage
    exit 1
fi

# Test if unity editor is valid
UNITY_VERSION=`$UNITY_PATH -version`
if [[ $UNITY_VERSION =~ ^[0-9]{4}\.[0-9]*\.[0-9]*[f]?[0-9]*$ ]]; then
    echo "Unity editor confirmed."
else
    while true; do
      read -p "Can't confirm Unity editor. Do you want to force \"$UNITY_PATH\" as an Unity editor executable? [y]es or [N]o: " yn
      yn=${yn:-"n"}
      case $yn in
          [Yy]* ) break;;
          [Nn]* ) exit 1;;
          * ) echo "Please answer [y]es or [n]o.";;
      esac
    done
fi

if [ $CLEAN_INSTALL == 1 ]; then
    echo "Cleaning install directory..."
    rm -rf $SCRIPTPATH/install/*
fi

if [ $STANDALONE == 1 ]; then
  python3 $SCRIPTPATH/src/scripts/metadata_generator.py --standalone
else
  python3 $SCRIPTPATH/src/scripts/metadata_generator.py
fi

if $SCRIPTPATH/src/ros2cs/build.sh $OPTIONS; then
    mkdir -p $SCRIPTPATH/install/asset && cp -R $SCRIPTPATH/src/Ros2ForUnity $SCRIPTPATH/install/asset/
    $SCRIPTPATH/deploy_unity_plugins.sh $SCRIPTPATH/install/asset/Ros2ForUnity/Plugins/
    cp $SCRIPTPATH/src/Ros2ForUnity/metadata_ros2cs.xml $SCRIPTPATH/install/asset/Ros2ForUnity/Plugins/Linux/x86_64/metadata_ros2cs.xml
    cp $SCRIPTPATH/src/Ros2ForUnity/metadata_ros2cs.xml $SCRIPTPATH/install/asset/Ros2ForUnity/Plugins/metadata_ros2cs.xml
else
    echo "Ros2cs build failed!"
    exit 1
fi

echo "Testing generated files with \"${UNITY_PATH}\" editor."

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
echo "Compiling asset's C# scripts"
if ! $UNITY_PATH -projectPath "$TMP_PROJECT_PATH" -batchmode -quit &>/dev/null; then
  echo
  echo "Ros2ForUnity scripts compilation errors detected. Please check ~/.config/unity3d/Editor.log for further details."
  echo "Build failed. Exiting."
  exit 1
fi

# Cleaning up
echo "Cleaning up temporary project..."
rm -rf $TMP_PROJECT_PATH/Assets/*

echo "Done!"
