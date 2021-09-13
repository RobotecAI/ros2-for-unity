ARG ROS2_DISTRO=foxy
FROM ros:${ROS2_DISTRO}-ros-base

RUN apt update && apt install -y ros-foxy-test-msgs ros-foxy-fastrtps ros-foxy-rmw-fastrtps-cpp ros-foxy-cyclonedds ros-foxy-rmw-cyclonedds-cpp

RUN apt update && apt install -y curl wget git

RUN curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
RUN apt update && apt install -y python3-vcstool

RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb 
RUN dpkg -i packages-microsoft-prod.deb
RUN apt update && apt install -y apt-transport-https dotnet-sdk-3.1

ADD entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

RUN mkdir -p /workdir/ros2-for-unity
RUN chmod -R 777 /workdir
RUN chmod -R 777 /home

ENTRYPOINT [ "/entrypoint.sh" ]
