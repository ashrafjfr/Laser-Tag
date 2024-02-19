import paho.mqtt.client as mqtt
import json

CAN_SEE_TOPIC_P1 = "cg4002-b05/p1/can_see"
CAN_SEE_TOPIC_P2 = "cg4002-b05/p2/can_see"

class MQTTManager:
    def __init__(self, broker, port=1883):
        self.client = mqtt.Client()
        self.broker = broker
        self.port = port
        self.client.on_connect = self._on_connect
        self.client.on_message = self._on_message
        self.p1_can_see = None
        self.p2_can_see = None

    def _on_connect(self, client, userdata, flags, rc):
        print(f"[MQTT] Connected with result code {rc}")
        # Subscribing in on_connect ensures that if we lose the connection and
        # reconnect, subscriptions will be renewed.
        self.subscribe(CAN_SEE_TOPIC_P1)
        self.subscribe(CAN_SEE_TOPIC_P2)

    def _on_message(self, client, userdata, message):
        data = json.loads(message.payload.decode())
        if message.topic == "cg4002-b05/p1/can_see":
            self.p1_can_see = data["is_visible"]
        elif message.topic == "cg4002-b05/p2/can_see":
            self.p2_can_see = data["is_visible"]

    def connect(self):
        self.client.connect(self.broker, self.port, 60)

    def disconnect(self):
        self.client.disconnect()

    def loop_start(self):
        self.client.loop_start()

    def loop_forever(self):
        self.client.loop_forever()

    def loop_stop(self):
        self.client.loop_stop()

    def subscribe(self, topic):
        self.client.subscribe(topic)

    def publish(self, topic, data):
        message = json.dumps(data)
        self.client.publish(topic, message)
