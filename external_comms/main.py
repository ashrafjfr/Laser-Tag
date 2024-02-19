import sys
import threading
import queue
import time
import signal
from relay_server import RelayServer
from eval_client import EvalClient
from mqtt_manager import MQTTManager
from game_engine import GameEngine
import logging

# AI imports
import pandas as pd
import numpy as np
import pynq
import os
import numpy as np
from pynq import Overlay, allocate
import pickle

import numpy as np

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def get_threshold(sequence):
	max_acc_x = np.max(np.abs(np.diff(sequence['Acc_X'])))
	max_acc_y = np.max(np.abs(np.diff(sequence['Acc_Y'])))
	max_acc_z = np.max(np.abs(np.diff(sequence['Acc_Z'])))

	return sum([max_acc_x, max_acc_y, max_acc_z])

def infer(dma, input_data, input_buffer, output_buffer):
    """
    returns an inference given input data
    
    @ param input_data: a list of 300 floats, representing 50 samples of [Acc_x, Acc_y, Acc_z, Gyro_x, Gyro_y, Gyro_z]
    @ param input_buffer: allocated memory for input data
    @ param output_buffer: allocated memory for output data
    """
    for i, x in enumerate(input_data):
        input_buffer[i] = x
    # call predict
    dma.sendchannel.transfer(input_buffer) 
    dma.recvchannel.transfer(output_buffer)
    
    dma.sendchannel.wait()
    dma.recvchannel.wait()
    
    return float(output_buffer)

# Queues for data communication between different components
data_queue = queue.Queue(maxsize=50)  # Queue for data from relay clients
action_queue = queue.Queue()  # Queue for predicted actions from ML model
game_to_visualizer_queue = queue.Queue()  # Queue for updated game state from the game engine
game_to_eval_queue = queue.Queue()  # Queue for game state to be sent to the eval server
game_to_relay_queue = queue.Queue()  # Queue for game state to be sent to relay client 1

# Constants
RELAY_SERVER_ADDRESS = "127.0.0.1"
RELAY_SERVER_PORT_1 = 5000
RELAY_SERVER_PORT_2 = 5001  # Second instance
EVAL_SERVER_ADDRESS = "localhost"
EVAL_SERVER_PORT = 8888
SECRET_KEY = "1111111111111111"
MQTT_BROKER_URL = "172.26.190.101"
MQTT_BROKER_PORT = 1883
# Topics
GAME_STATE_TOPIC = "cg4002-b05/game/game_state"
ACTION_TOPIC_P1 = "cg4002-b05/p1/action"
ACTION_TOPIC_P2 = "cg4002-b05/p2/action"
MESSAGE_TOPIC_P1 = "cg4002-b05/p1/message"
MESSAGE_TOPIC_P2 = "cg4002-b05/p2/message"

# # Overlay instantiation
bitstream_path = 'run2.bit' # Path to bitstream (should include 'infer.hwh' hardware handoff file)
overlay = Overlay(bitstream_path) # Initialise overlay
dma = overlay.axi_dma_0 # Get DMA from overlay
input_buffer = allocate(shape=(210,), dtype=np.float32) # Allocate memory for input data
output_buffer = allocate(shape=(1,), dtype=np.float32) # Allocate memory for output data

action_map = {
	0: "grenade",
	1: "web",
	2: "portal",
	3: "punch",
	4: "hammer",
	5: "spear",
	6: "shield",
	7: "reload",
	8: "logout"
}

def ml_model_processor(data_queue, action_queue, is_running_event):
	"""
	@ param data_queue: queue from internal comms. assume packet not None.
	@ param action_queue: Queue for predicted actions from ML model
	@ param is_running_event: Event to indicate whether the program is running
	"""

	columns = ['Acc_X', 'Acc_Y', 'Acc_Z', 'Gyr_X', 'Gyr_Y', 'Gyr_Z']
	while is_running_event.is_set():
		if not data_queue.empty():
			data = data_queue.get()
			data_df = data["data"]
			player_id = data["player"]
			df = pd.DataFrame(data_df, columns=columns, dtype=float)     
			features = df.values.flatten().tolist()
			prediction = infer(dma, features, input_buffer, output_buffer)	
			action_queue.put({
				"player_id": player_id,
				"action": action_map.get(int(prediction))
			})

def game_engine(action_queue, game_to_visualizer_queue, game_engine_instance, mqtt_manager, is_running_event):
	while is_running_event.is_set():
		if not action_queue.empty():
			action = action_queue.get()
			player_id = action["player_id"]
			actual_action = action["action"]
			print(f'\nPlayer {player_id} - {actual_action}')
			mqtt_manager.p1_can_see = None
			mqtt_manager.p2_can_see = None
			if action["action"] not in ["logout", "shield", "reload", "shoot" , "shot_recv"]:
				game_to_visualizer_queue.put({
					"type": "action",
					"data": action
				})
				# Block until can_see is received from visualizer
				timer = time.time()
				while mqtt_manager.p1_can_see is None and mqtt_manager.p2_can_see is None:
					# Timeout in case visualizer is not responding
					if time.time() - timer > 1:
						print(f"Player {player_id} : Visibility failed")
						if action["player_id"] == 1:
							mqtt_manager.p1_can_see = True
						else:
							mqtt_manager.p2_can_see = True
						break
				if mqtt_manager.p1_can_see is not None:
					game_engine_instance.handle_action(action["action"], 1, mqtt_manager.p1_can_see)
					mqtt_manager.p1_can_see = None
				elif mqtt_manager.p2_can_see is not None:
					game_engine_instance.handle_action(action["action"], 2, mqtt_manager.p2_can_see)
			else:
				if action["action"] != "reload":
					game_to_visualizer_queue.put({
						"type": "action",
						"data": action
					})
				game_engine_instance.handle_action(action["action"], action["player_id"], None)


def evaluation_sender(eval_client, game_to_eval_queue, is_running_event):
	while is_running_event.is_set():
		if not game_to_eval_queue.empty():
			game_state = game_to_eval_queue.get()
			# Send game state to eval server
			eval_client.send_game_state(game_state)
			
def mqtt_publisher(mqtt_manager, is_running_event):
	mqtt_manager.connect()
	mqtt_manager.loop_start()
	while is_running_event.is_set():
		if not game_to_visualizer_queue.empty():
			to_publish = game_to_visualizer_queue.get()
			if to_publish["type"] == "game_state":
				mqtt_manager.publish(GAME_STATE_TOPIC, to_publish["data"])
				action_topic = ACTION_TOPIC_P1 if to_publish["data"]["player_id"] == 1 else ACTION_TOPIC_P2
				mqtt_manager.publish(action_topic, to_publish["data"])
			elif to_publish["type"] == "message":
				message_topic = MESSAGE_TOPIC_P1 if to_publish["player_id"] == 1 else MESSAGE_TOPIC_P2
				mqtt_manager.publish(message_topic, to_publish["data"])

def relay_server_sender(relay_server1, relay_server2, is_running_event):
	while is_running_event.is_set():
		if not game_to_relay_queue.empty():
			game_state = game_to_relay_queue.get()
			relay_server1.outgoing_queue.put(game_state)
			relay_server2.outgoing_queue.put(game_state)

def interrupt_handle(relay_server1, relay_server2, eval_client, mqtt_manager, is_running_event):
		relay_server1.stop()
		relay_server2.stop()
		eval_client.close()
		mqtt_manager.disconnect()
		is_running_event.clear()


if __name__ == "__main__":
	eval_port = int(input("Enter eval server port: "))
	is_running_event = threading.Event()
	is_running_event.set()

	mqtt_manager = MQTTManager(MQTT_BROKER_URL, MQTT_BROKER_PORT)
	game_engine_instance = GameEngine(game_to_visualizer_queue, game_to_eval_queue, game_to_relay_queue, mqtt_manager)
	relay_server1 = RelayServer(RELAY_SERVER_ADDRESS, RELAY_SERVER_PORT_1, data_queue, action_queue, game_to_visualizer_queue)
	relay_server2 = RelayServer(RELAY_SERVER_ADDRESS, RELAY_SERVER_PORT_2, data_queue, action_queue, game_to_visualizer_queue)
	eval_client = EvalClient(EVAL_SERVER_ADDRESS, eval_port, SECRET_KEY, game_engine_instance, game_to_visualizer_queue, game_to_relay_queue)
	eval_client.connect()
	eval_client.send_message("hello")

	start_thread1 = threading.Thread(target=relay_server1.start)
	start_thread2 = threading.Thread(target=relay_server2.start)
	receive_thread1 = threading.Thread(target=relay_server1.receive, args=(is_running_event,))
	send_thread1 = threading.Thread(target=relay_server1.send, args=(is_running_event,))
	receive_thread2 = threading.Thread(target=relay_server2.receive, args=(is_running_event,))
	send_thread2 = threading.Thread(target=relay_server2.send, args=(is_running_event,))
	
	ml_model_thread = threading.Thread(target=ml_model_processor, args=(data_queue, action_queue, is_running_event))
	game_engine_thread = threading.Thread(target=game_engine, args=(action_queue, game_to_visualizer_queue, game_engine_instance, mqtt_manager, is_running_event))
	game_state_publish_thread = threading.Thread(target=mqtt_publisher, args=(mqtt_manager, is_running_event))
	eval_send_thread = threading.Thread(target=evaluation_sender, args=(eval_client, game_to_eval_queue, is_running_event))
	eval_client_receive_thread = threading.Thread(target=eval_client.receive_game_state, args=(is_running_event,))
	
	relay_server_publish_thread = threading.Thread(target=relay_server_sender, args=(relay_server1, relay_server2, is_running_event))

	start_thread1.start()
	start_thread2.start()
	receive_thread1.start()
	send_thread1.start()
	receive_thread2.start()
	send_thread2.start()
	eval_send_thread.start()
	eval_client_receive_thread.start()
	ml_model_thread.start()
	game_engine_thread.start()
	game_state_publish_thread.start()
	relay_server_publish_thread.start()

	signal.signal(signal.SIGINT, lambda signum, frame: interrupt_handle(relay_server1, relay_server2, eval_client, mqtt_manager, is_running_event))

	start_thread1.join()
	start_thread2.join()
	receive_thread1.join()
	send_thread1.join()
	receive_thread2.join()
	send_thread2.join()
	eval_send_thread.join()
	eval_client_receive_thread.join()
	game_engine_thread.join()
	ml_model_thread.join()
	game_state_publish_thread.join()
	relay_server_publish_thread.join()
	print("Threads closed.")
	# Exit
	sys.exit(0)
