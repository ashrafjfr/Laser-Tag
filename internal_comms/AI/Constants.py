import struct

HELLO_HEADER = 'H'
ACK_HEADER = 'A'
DATA_HEADER = 'D'

PACKET_SIZE = 20
PAYLOAD_SIZE = 13 

data_format = '=cBB?hhhhhhL'
packet_struct = struct.Struct('= c 2B ? 6h L')

IMU_COLOR = "light_green"
GUN_COLOR = "light_magenta"
VEST_COLOR = "light_cyan"

ERROR_COLOR = "red"