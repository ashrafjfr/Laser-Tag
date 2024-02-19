from Beetle import Beetle
import concurrent.futures
import socket
import json
import queue
import threading
from Constants import *
import Globals


Globals.init_global()
data_to_send_queue = queue.Queue()

class RelayClient:
	def __init__(self, host, port):
		self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.server_address = (host, port)
		self.client_socket.connect(self.server_address)
		print(f"[RelayClient] Connected to the server at {self.server_address[0]}:{self.server_address[1]}")

	def send_message(self, message):
		encoded_message = message.encode('utf-8')

		# Construct packet with length prefix followed by _
		packet = f"{len(encoded_message)}_{message}".encode('utf-8')

		self.client_socket.sendall(packet)
		print(f"[RelayClient] ====>>> Sent message: {message}")
	
	def receive_message(self, is_running_event):
		while is_running_event.is_set():
			try:
				# First, receive the length prefix and underscore
				length_buffer = b''
				while True:
					byte = self.client_socket.recv(1)
					if byte == b'_':
						break
					length_buffer += byte

				# Convert length_buffer to integer
				data_length = int(length_buffer.decode())

				# Now, receive the actual message
				chunks = []
				bytes_recd = 0
				while bytes_recd < data_length:
					chunk = self.client_socket.recv(
						min(data_length - bytes_recd, 2048))
					if chunk == b'':
						raise RuntimeError("Socket connection broken")
					chunks.append(chunk)
					bytes_recd += len(chunk)
				full_message = b''.join(chunks).decode('utf-8')
				intermediate_data = json.loads(full_message)

				# Check if the decoded result is still a string, indicating double encoding
				if isinstance(intermediate_data, str):
					json_data = json.loads(intermediate_data)
				else:
					json_data = intermediate_data               

				Globals.player1_ammo = json_data["p1"]["bullets"]
				Globals.player1_hp = json_data["p1"]["hp"]

				Globals.should_update_ammo = True
				Globals.should_update_hp = True
				# Put received data from Ultra96 into the queue
				# self.received_data_queue.put(json_data)
				print(f"[RelayClient] <<<=== Received data: {full_message}")
			except Exception as e:
				print(f"[RelayClient] Error receiving message: {e}")

	def close(self):
		print("[RelayClient] Closing connection...")
		self.client_socket.close()


def run_beetle(beetle):
	while True:
		beetle.connect()
		beetle.initialise_handshake()
		beetle.collect_data()  


def sender_thread(client, data_to_send_queue, is_running_event):
	while is_running_event.is_set():
		if not data_to_send_queue.empty():
			data = data_to_send_queue.get()
			data = json.dumps(data)
			client.send_message(data)
			data_to_send_queue.task_done()


if __name__ == '__main__':
	is_running_event = threading.Event()
	is_running_event.set()
	
	# Initialize the relay client
	HOST = "127.0.0.1"
	PORT = 5000  # Update with the actual server port
	relay_client = RelayClient(HOST, PORT)
	
	send_thread = threading.Thread(target=sender_thread, args=(relay_client, data_to_send_queue, is_running_event,))
	receive_thread = threading.Thread(target=relay_client.receive_message, args=(is_running_event,))
	send_thread.start()
	receive_thread.start()

	gun_beetle = Beetle(GUN_MAC_ADDRESS, "GUN", data_to_send_queue) 
	imu_beetle = Beetle(IMU_MAC_ADDRESS, "IMU", data_to_send_queue)
	vest_beetle = Beetle(VEST_MAC_ADDRESS, "VEST", data_to_send_queue)

	Beetles = [gun_beetle, imu_beetle, vest_beetle]
	with concurrent.futures.ThreadPoolExecutor(max_workers=3) as executor:
		executor.map(run_beetle, Beetles)

	try:
		while True:
			pass
	except KeyboardInterrupt:
		print("Closing connections and threads...")
		is_running_event.clear()
		relay_client.close()
		gun_beetle.disconnect()
		imu_beetle.disconnect()
		vest_beetle.disconnect()
		send_thread.join()
		receive_thread.join()
		print("All threads closed.")