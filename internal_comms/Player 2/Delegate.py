from bluepy import btle
from crc import Calculator, Crc32
from Constants import *
from termcolor import colored


class Delegate(btle.DefaultDelegate):
	def __init__(self, name, color):
		btle.DefaultDelegate.__init__(self)
		self.buffer = b""
		self.curr_packet = b""
		self.curr_packet_info = None
		self.packet_type = None
		self.name = name
		self.color = color
		self.is_fragmented = False
		self.prev_seq_num = None

	def handleNotification(self, cHandle, data):
		# If header is ack or data, or if there is already an existing incomplete buffer, add to buffer
		if chr(data[0]) in ['A', 'D'] or len(self.buffer) > 0 :
			self.buffer += data
			# print(f"Printing buffer: {self.buffer}")

	# If current buffer more or equal to packet size, we extract the first 20 bytes of the packet 
	def extract_data(self):
		if len(self.buffer) >= 20:
			self.is_fragmented = False
			self.curr_packet = self.buffer[:20]
			self.buffer = self.buffer[20:]
			self.curr_packet_info = self.unpack_data(self.curr_packet)
			return self.curr_packet_info
		# Buffer still not full, we continue to wait for remaining fragment
		else:
			if len(self.buffer) > 0:
				self.is_fragmented = True
				print(colored(f"{self.name} - Fragmentation occurred", self.color))
			return

	def unpack_data(self, packet):
		try:
			# return struct.unpack(data_format, packet)
			return packet_struct.unpack(packet)
		except:
			print(colored(f"{self.name} - Error unpacking data", self.color))

	def is_valid_crc(self, packet, packet_info):
		try:
			calculator = Calculator(Crc32.CRC32, optimized=True)
			expected_crc = packet_info[-1]
			return calculator.verify(packet[0:16], expected_crc)
		except:
			print(colored(f"{self.name} - CRC error", self.color))
			return

	def get_packet_header(self, packet_info):
		try:
			return packet_info[0].decode("utf-8")
		except:
			print(colored(f"{self.name} - Error getting packet header", self.color))
			return

	def get_sequence_number(self, packet_info):
		try:
			return packet_info[1]
		except:
			print(colored(f"{self.name} - Error getting sequence number", self.color))
			return

	def is_duplicate(self, seq_num):
		return seq_num == self.prev_seq_num
		
	def clear_buffer(self):
		self.buffer = b""