import socket
from Crypto.Cipher import AES
from Crypto.Random import get_random_bytes
from Crypto.Util.Padding import pad
import base64
import json
from helper import recv_exact

class EvalClient:
    def __init__(self, server_address, server_port, secret_key, game_engine_instance, game_to_visualizer_queue, game_to_relay_queue):
        self.server_address = server_address
        self.server_port = server_port
        self.secret_key = secret_key
        self.client_socket = None
        # self.eval_to_game_queue = eval_to_game_queue
        self.game_engine_instance = game_engine_instance
        self.game_to_visualizer_queue = game_to_visualizer_queue
        self.game_to_relay_queue = game_to_relay_queue
        print("[EvalClient] Initialized")

    def connect(self):
        # Create a socket object
        self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # Connect to the server
        self.client_socket.connect((self.server_address, self.server_port))
        print(f"[EvalClient] Connected to the eval server on {self.server_address}:{self.server_port}")

    def send_message(self, message):
        encrypted_message = self.encrypt_message(message)
        self.client_socket.sendall(str(len(encrypted_message)).encode() + b'_' + encrypted_message)
        # print("[EvalClient] ====>>> Sent game state to eval server")

    def send_game_state(self, game_state):
        game_state_str = json.dumps(game_state)
        self.send_message(game_state_str)
        # Receive the expected game state from eval server

    def receive_game_state(self, is_running_event):
        while is_running_event.is_set():
            if self.client_socket:
                response = recv_exact(self.client_socket)
                correct_game_state = json.loads(response.decode())
                # self.eval_to_game_queue.put(json.loads(response.decode()))
                # Give updated game state to game engine and relay clients
                self.game_engine_instance.update_game_state(correct_game_state)
                self.game_to_visualizer_queue.put({
                    "type": "game_state",
                    "data": {
                        "game_state": correct_game_state
                    }
                })
                self.game_to_relay_queue.put(correct_game_state)
    
    def encrypt_message(self, message):
        secret_key = bytes(self.secret_key, encoding="utf8")
        iv = get_random_bytes(AES.block_size)

        cipher = AES.new(secret_key, AES.MODE_CBC, iv)
        encrypted_message = cipher.encrypt(pad(message.encode('utf-8'), AES.block_size))

        return base64.b64encode(iv + encrypted_message)

    def close(self):
        self.client_socket.close()
