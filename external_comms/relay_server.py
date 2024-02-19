import socket
import json
import queue
import logging
from helper import recv_exact

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class RelayServer:
    def __init__(self, host, port, data_queue, action_queue, game_to_visualizer_queue):
        self.server_address = (host, port)
        self.data_queue = data_queue
        self.action_queue = action_queue
        self.outgoing_queue = queue.Queue()
        self.game_to_visualizer_queue = game_to_visualizer_queue
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.bind(self.server_address)
        self.server_socket.listen(3)
        self.client_socket = None
        self.client_address = None
        print(f"[RelayServer] Starting server on {host}:{port}")

    def receive(self, is_running_event):
        try:
            while is_running_event.is_set():
                if self.client_socket:
                    data = recv_exact(self.client_socket)
                    if data:
                        # Put the received data into the queue for processing
                        decoded_data = json.loads(data.decode('utf-8'))
                        # If from gun, put to action queue instead of data queue
                        if isinstance(decoded_data["data"], bool):
                            self.game_to_visualizer_queue.put({
                                "type": "message",
                                "player_id": decoded_data["player"],
                                "data": {
                                    "device": decoded_data["device"],
                                    "is_connected": decoded_data["data"]
                                }
                            })
                        elif decoded_data["device"] == "imu":
                            if self.data_queue.full():
                                try:
                                    # Remove the oldest item from the queue to make space
                                    self.data_queue.get_nowait()
                                except self.data_queue.Empty:
                                    pass
                            try:
                                self.data_queue.put_nowait(decoded_data)
                            except self.data_queue.Full:
                                pass
                        else:
                            self.action_queue.put({
                                "player_id": decoded_data["player"],
                                "action": "shoot" if decoded_data["device"] == "gun" else "shot_recv"
                            })
                            # print(f"[RelayServer] ====>>> Put data into action queue: {decoded_data}")
                    else:
                        print(
                            f"[RelayServer] No more data from {self.client_address}")
                        break
        finally:
            # Clean up the connection
            if self.client_socket:
                self.client_socket.close()
            if self.server_socket:
                self.server_socket.close()

    def start(self):
        print(f"[RelayServer] Server started on {self.server_address[0]}:{self.server_address[1]}")
        self.client_socket, self.client_address = self.server_socket.accept()
        print(f"[RelayServer] Accepted connection from {self.client_address}")
        

    def send(self, is_running_event):
        while is_running_event.is_set():
            if self.client_socket and not self.outgoing_queue.empty():
                message = self.outgoing_queue.get()
                encoded_message = json.dumps(message).encode('utf-8')
                # Construct the packet with length prefix followed by _
                packet = f"{len(encoded_message)}_".encode('utf-8') + encoded_message
                self.client_socket.sendall(packet)
                # self.outgoing_queue.task_done()

    def stop(self):
        # This method is added if you wish to shut down the server gracefully in the future.
        if self.client_socket:
            self.client_socket.close()
        if self.server_socket:
            self.server_socket.close()

