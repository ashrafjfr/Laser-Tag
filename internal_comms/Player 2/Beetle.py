from bluepy import btle
from Delegate import Delegate
from crc import Calculator, Crc32
from timeit import default_timer as timer
from Constants import *
from termcolor import colored
import time
import Globals
import pandas as pd 
import numpy as np
import logging

# logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class Beetle():
	def __init__(self, addr, name, data_to_send_queue, client_socket=None):
		self.addr = addr
		self.name = name
		self.peripheral = btle.Peripheral()
		self.is_connected = False
		self.handshake_initialised = False
		self.delegate = None
		self.ch = None
		self.start_time = None
		self.prev_time = None
		self.transmission_rate = 0
		self.packets_count = 0
		self.fragmented_packets_count = 0
		self.is_prev_packet_frag = False
		self.seq_num = 0
		self.data_to_send_queue = data_to_send_queue
		self.sliding_window = list()
		self.columns = ['Acc_X', 'Acc_Y', 'Acc_Z', 'Gyr_X', 'Gyr_Y', 'Gyr_Z']
		self.threshold_hit = False
		self.cooldown_period = 3
		self.last_threshold_time = 0
		self.prev_thres = 0

		if name == "IMU":
			self.color = IMU_COLOR
		if name == "GUN":
			self.color = GUN_COLOR
		if name == "VEST":
			self.color = VEST_COLOR

		self.data = {
			"player": 2,
			"device": self.name.lower(),
			"data": []
		}

	def connect(self):
		while not self.is_connected:
			print(colored(f"{self.name} - Connecting to Beetle...", self.color))
			try:
				self.peripheral.connect(self.addr)
				self.delegate = Delegate(self.name, self.color)
				self.delegate.buffer = b""
				self.peripheral.withDelegate(self.delegate)
				self.is_connected = True
				self.send_connect_message()
				print(colored(f"{self.name} - Beetle Connected", self.color))
				self.ch = self.peripheral.getServiceByUUID('dfb0').getCharacteristics('dfb1')[0]
				return
			except btle.BTLEException as err:
				print(colored(f"{self.name} - Beetle connection failed.", self.color))
				self.is_connected = False
				self.send_error_message()

	def initialise_handshake(self):
		while not self.handshake_initialised and self.is_connected:
			try:
				self.send_hello()
				print(colored(f'{self.name} - Initialising Handshake', self.color))
				if self.peripheral.waitForNotifications(1):
					packet_info = self.delegate.extract_data()
					print(packet_info)
					packet_header = self.delegate.get_packet_header(packet_info)
					if packet_header == 'A':
						self.send_ack(0, True)
						self.handshake_initialised = True
						print(colored(f"{self.name} - Three-way handshake completed", self.color))
						self.start_time = timer()
						self.packets_count = 0
					else:
						self.handshake_initialised = False
			except btle.BTLEDisconnectError as err:
				self.handshake_initialised = False
				self.set_disconnected(f"{self.name} - Beetle disconnected")
				self.send_error_message()

	def send_error_message(self):
		self.data_to_send_queue.put({
			"player": 2,
			"device": self.name.lower(),
			"data": False
		})

	def send_connect_message(self):
		self.data_to_send_queue.put({
			"player": 2, 
			"device": self.name.lower(),
			"data": True
		})		

	def get_threshold(self, sequence):
		max_acc_x = np.max(np.abs(np.diff(sequence['Acc_X'])))
		max_acc_y = np.max(np.abs(np.diff(sequence['Acc_Y'])))
		max_acc_z = np.max(np.abs(np.diff(sequence['Acc_Z'])))

		return sum([max_acc_x, max_acc_y, max_acc_z])

	def detect_send(self, packet):
		current_time = time.time()
		# Append packet to the sliding window
		self.sliding_window.append(packet)

		window_length = len(self.sliding_window)
		
		# If we've hit our threshold, we just wait until we've collected 35 packets
		if self.threshold_hit:
			print("Threshold hit. Enlarging window now.")
			if window_length == 35:
				print("Window size 35 achieved")
				self.threshold_hit = False  # Reset for the next time we hit 25 packets
				self.last_threshold_time = current_time
				return True  # We've collected 35 packets, signal that we're ready to send
			return False  # Keep collecting until we hit 35

		if current_time - self.last_threshold_time < self.cooldown_period:
			if window_length >= 25:
				self.sliding_window.pop(0)
			return False

		# If we've reached 25 packets, check the threshold
		if window_length == 25:
			df = pd.DataFrame(self.sliding_window, columns=self.columns)
			threshold = self.get_threshold(df)
			if threshold >= 10000 and abs(threshold - self.prev_thres) > 2000:
				self.prev_thres = threshold
				self.threshold_hit = True  # We've hit our threshold, start collecting additional packets
				return False  # Signal to keep the sliding window open for more packets
			else:
				self.sliding_window.pop(0)

		# If we go over 25 packets without hitting the threshold, we slide the window
		elif window_length > 25:
			self.sliding_window.pop(0)  # Remove the oldest packet

		return False  # Default return value indicating the window is not ready to be sent yet


	def collect_data(self):
		while self.handshake_initialised and self.is_connected:
			if self.timer_check():
				self.print_stats()

			self.send_ammo_update()
			self.send_hp_update()
			try:
				if self.peripheral.waitForNotifications(1.0):
						
					packet_info = self.delegate.extract_data()
					packet_header = self.delegate.get_packet_header(packet_info)
					# print(f"[DEBUG] {self.name} - Packet Header: {packet_header}")
					print("Packet received")
					# seq_num = self.delegate.get_sequence_number(packet_info)
					print(colored(f"{self.name} - {packet_info}", self.color))
					if packet_info != None:
						if packet_header == 'D':
							if self.name == "GUN":
								self.data_to_send_queue.put(self.data)
								print("DEBUG: PLAYER 2 SHOT RECEIVED ON LAPTOP")

							elif self.name == "VEST":
								self.data_to_send_queue.put(self.data)

							elif self.name == "IMU":
								imu_data = (packet_info[4], packet_info[5], packet_info[6], packet_info[7], packet_info[8], packet_info[9])
								can_send = self.detect_send(imu_data)
								if can_send:
									self.data["data"] = self.sliding_window
									self.data_to_send_queue.put(self.data)
									self.sliding_window = list()


						self.is_prev_packet_frag = False
						self.packets_count += 1
						self.transmission_rate = self.get_transmission_rate()

					else:	
						if self.delegate.is_fragmented and not self.is_prev_packet_frag:
							self.fragmented_packets_count += 1
							self.is_prev_packet_frag = True
				
					# self.delegate.prev_seq_num = seq_num
			except btle.BTLEDisconnectError as err:
				self.handshake_initialised = False
				self.set_disconnected(f"{self.name} - Beetle disconnected.")
				self.send_error_message()

	def send_hello(self):
		payload = self.encode_crc(HELLO_HEADER + ('0' * 15))
		self.ch.write(payload)

	def send_ack(self, seq_num, is_handshake_ack):
		if is_handshake_ack:
			payload = self.encode_crc(ACK_HEADER + str(seq_num) + 'H' + '0' * 13)
			self.ch.write(payload)
		else:
			payload = self.encode_crc(ACK_HEADER + str(seq_num) + '0' * 14)
			self.ch.write(payload)

	def set_disconnected(self, msg):
		self.is_connected = False
		print(colored('*'*100, ERROR_COLOR))
		print(colored(msg, ERROR_COLOR))
		print(colored('*'*100, ERROR_COLOR))

	def encode_crc(self, message):
		calculator = Calculator(Crc32.CRC32, optimized=True)
		payload = bytes(message, encoding='utf-8')
		crc = calculator.checksum(payload)
		payload += crc.to_bytes(4, 'big')
		return payload

	def timer_check(self):
		curr_time = time.time()
		if self.prev_time is None:
			self.prev_time = curr_time
			return True

		time_interval = curr_time - self.prev_time
		# Print every second
		if time_interval >= 1:
			self.prev_time = curr_time
			return True
		return False	

	def get_transmission_rate(self):
		curr_time = timer()
		transmission_rate = (PAYLOAD_SIZE * 8 * self.packets_count/1000)/(curr_time-self.start_time)
		return transmission_rate
	
	def print_stats(self):
		print(colored(f"{'#' * 30 } {self.name} - Transmission rate: {self.transmission_rate:.3f}kbps {'#' * 30 }", self.color))
		print(colored(f"{'#' * 30 } {self.name} - Number of Fragmented Packets: {self.fragmented_packets_count} {'#' * 30 }", self.color))

	def format_message(self, message):
		encoded_message = message.encode('utf-8')

		# Construct packet with length prefix followed by _
		packet = f"{len(encoded_message)}_{message}".encode('utf-8')
		return packet

	def send_ammo_update(self):
		if self.name == "GUN" and Globals.should_update_ammo:
			payload = self.encode_crc(DATA_HEADER + str(self.seq_num) + str(Globals.player2_ammo) + '0' * 13) 
			self.ch.write(payload)
			Globals.should_update_ammo = False

	def send_hp_update(self):
		if self.name == "VEST" and Globals.should_update_hp:
			hp_hundreds = (Globals.player2_hp // 100) % 10
			hp_tens = (Globals.player2_hp // 10) % 10
			hp_ones = Globals.player2_hp % 10
			hp_str = str(hp_hundreds) + str(hp_tens) + str(hp_ones)
			payload = self.encode_crc(DATA_HEADER + str(self.seq_num) + hp_str + '0' * 11)
			self.ch.write(payload)
			Globals.should_update_hp = False