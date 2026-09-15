"""External Lyrical rclpy peer; exits nonzero if no adapter message arrives."""
import time
import rclpy
from std_msgs.msg import String

rclpy.init()
node = rclpy.create_node("unity_adapter_peer")
publisher = node.create_publisher(String, "ros_to_unity", 10)
received = False


def on_message(message):
    global received
    if message.data == "unity-ping":
        received = True
        publisher.publish(String(data="ros-ack"))


subscription = node.create_subscription(String, "unity_to_ros", on_message, 10)
deadline = time.monotonic() + 40
while time.monotonic() < deadline:
    rclpy.spin_once(node, timeout_sec=0.1)
node.destroy_node()
rclpy.shutdown()
if not received:
    raise SystemExit("No message from Unity adapter")
print("PASS rclpy received unity-ping and published ros-ack")
