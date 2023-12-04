#!/bin/sh -e
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

display_usage() {
    echo "Usage: "
    echo ""
    echo "build.sh [--with-tests] [--standalone] [--clean-install]"
    echo ""
    echo "Options:"
    echo "--with-tests - build with tests"
    echo "--standalone - standalone version"
    echo "--clean-install - makes a clean installation, removes install directory before deploying"
}

if [ ! -d "$SCRIPTPATH/src/ros2cs" ]; then
    echo "Pull repositories with 'pull_repositories.sh' first."
    exit 1
fi

OPTIONS=""
STANDALONE=0
TESTS=0
CLEAN_INSTALL=0

while [ $# -gt 0 ]; do
  key="$1"
  case $key in
    -t|--with-tests)
      OPTIONS="$OPTIONS --with-tests"
      TESTS=1
      shift # past argument
      ;;
    -s|--standalone)
      if ! hash patchelf 2>/dev/null ; then
        echo "Patchelf missing. Standalone build requires patchelf. Install it via apt 'sudo apt install patchelf'."
        exit 1
      fi
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
      ;;
    *)    # unknown option
      shift # past argument
      ;;
  esac
done

if [ $CLEAN_INSTALL = 1 ]; then
    echo "Cleaning install directory..."
    rm -rf "${SCRIPTPATH}/install/"*
fi

if [ $STANDALONE = 1 ]; then
  python3 "${SCRIPTPATH}/src/scripts/metadata_generator.py" --standalone
else
  python3 "${SCRIPTPATH}/src/scripts/metadata_generator.py"
fi

if "${SCRIPTPATH}/src/ros2cs/build.sh" $OPTIONS; then
    mkdir -p "${SCRIPTPATH}/install/package" && cp -R "${SCRIPTPATH}/src/Ros2ForUnity" "${SCRIPTPATH}/install/package/"
    "${SCRIPTPATH}/deploy_unity_plugins.sh" "${SCRIPTPATH}/install/package/Ros2ForUnity/Plugins/"
else
    echo "Ros2cs build failed!"
    exit 1
fi
