import struct

HELLO_HEADER = 'H'
ACK_HEADER = 'A'
DATA_HEADER = 'D'

HEADERS = [ACK_HEADER, DATA_HEADER]

PACKET_SIZE = 20
PAYLOAD_SIZE = 13 

data_format = '=cBB?hhhhhhL'
packet_struct = struct.Struct('= c 2B ? 6h L')

IMU_COLOR = "light_green"
GUN_COLOR = "light_magenta"
VEST_COLOR = "light_cyan"

ERROR_COLOR = "red"

# Player 1
IMU_MAC_ADDRESS = "D0:39:72:E4:8E:22" 
GUN_MAC_ADDRESS = "D0:39:72:E4:83:E4" 
VEST_MAC_ADDRESS = "D0:39:72:E4:86:C5" 

# Player 2
# IMU_MAC_ADDRESS = "D0:39:72:E4:83:8C" 
# GUN_MAC_ADDRESS = "D0:39:72:E4:86:E7"
# VEST_MAC_ADDRESS = "D0:39:72:C8:53:34"