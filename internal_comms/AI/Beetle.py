from bluepy import btle
from Delegate import Delegate
from crc import Calculator, Crc32
from timeit import default_timer as timer
from Constants import *
from termcolor import colored
import time
import csv 

class Beetle():
	def __init__(self, addr, name):
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

		if name == "IMU":
			self.color = IMU_COLOR
		if name == "GUN":
			self.color = GUN_COLOR
		if name == "VEST":
			self.color = VEST_COLOR

	def connect(self):
		while not self.is_connected:
			print(colored(f"{self.name} - Connecting to Beetle...", self.color))
			try:
				self.peripheral.connect(self.addr)
				self.delegate = Delegate(self.name, self.color)
				self.delegate.buffer = b""
				self.peripheral.withDelegate(self.delegate)
				self.is_connected = True
				print(colored(f"{self.name} - Beetle Connected", self.color))
				self.ch = self.peripheral.getServiceByUUID('dfb0').getCharacteristics('dfb1')[0]
				return
			except btle.BTLEException as err:
				print(colored(f"{self.name} - Beetle connection failed.", self.color))
				self.is_connected = False

	def initialise_handshake(self):
		while not self.handshake_initialised and self.is_connected:
			try:
				self.send_hello()
				print(colored(f'{self.name} - Initialising Handshake', self.color))
				if self.peripheral.waitForNotifications(1):
					packet_info = self.delegate.extract_data()
					print(packet_info)
					packet_type = self.delegate.get_packet_header(packet_info)
					if packet_type == 'A':
						self.send_ack(0)
						self.handshake_initialised = True
						print(colored(f"{self.name} - Three-way handshake completed", self.color))
						self.start_time = timer()
						self.packets_count = 0
					else:
						self.handshake_initialised = False
			except btle.BTLEDisconnectError as err:
				self.handshake_initialised = False
				self.set_disconnected(f"{self.name} - Beetle disconnected")

	def collect_data(self):
		while self.handshake_initialised and self.is_connected:
			if self.timer_check():
				self.print_stats()
			try:
				if self.peripheral.waitForNotifications(1.0):
					packet_info = self.delegate.extract_data()
					packet_type = self.delegate.get_packet_header(packet_info)
					seq_num = self.delegate.get_sequence_number(packet_info)
					print(colored(f"{self.name} - {packet_info}", self.color))
					if packet_info != None:
						
						if packet_type == 'D' and self.name == "IMU":
							self.save_to_csv(packet_info)
						# TCP protocol for gun and vest
						# if packet_header == 'D' and (self.name == "GUN" or self.name == "VEST"):
						# 	self.send_data_ack(seq_num)

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
				self.set_disconnected(f"{self.name} - Beetle disconnected")


	def send_hello(self):
		payload = self.encode_crc(HELLO_HEADER + ('0' * 15))
		self.ch.write(payload)

	def send_ack(self, seq_num):
		payload = self.encode_crc(ACK_HEADER + str(seq_num) + 'H' + '0' * 13)
		self.ch.write(payload)

	def send_data_ack(self, seq_num):
		payload = self.encode_crc(ACK_HEADER + str(seq_num) + 'D' + '0' * 13)
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

	def save_to_csv(self, packet_info, filename="output.csv"):
		with open(filename, 'a', newline='') as csvfile:
			fieldnames = ['AccX', 'AccY', 'AccZ', 'GyroX', 'GyroY', 'GyroZ']
			writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

			# Optional: write the header only once
			if csvfile.tell() == 0:
				writer.writeheader()

			accx, accy, accz, gyrox, gyroy, gyroz = packet_info[4:10]

			writer.writerow({
				'AccX': accx,
				'AccY': accy,
				'AccZ': accz,
				'GyroX': gyrox,
				'GyroY': gyroy,
				'GyroZ': gyroz
			})
