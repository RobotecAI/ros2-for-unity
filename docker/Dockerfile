ARG ROS2_DISTRO=humble
FROM ros:${ROS2_DISTRO}-ros-base

RUN apt update && apt install -y ros-${ROS_DISTRO}-test-msgs ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp ros-${ROS_DISTRO}-cyclonedds ros-${ROS_DISTRO}-rmw-cyclonedds-cpp

RUN apt update && apt install -y curl wget git

RUN curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
RUN apt update && apt install -y python3-vcstool

RUN wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt update && apt install -y apt-transport-https patchelf; if [ "$(lsb_release -rs)" = "22.04" ] ; then apt install -y dotnet-sdk-6.0 ; else apt install -y dotnet-sdk-3.1; fi
RUN apt update && apt install -y ffmpeg libsm6 libxext6 libgtk-3-0

ADD entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

RUN mkdir -p /workdir/ros2-for-unity
RUN chmod -R 777 /workdir
RUN chmod -R 777 /home

ENTRYPOINT [ "/entrypoint.sh" ]
