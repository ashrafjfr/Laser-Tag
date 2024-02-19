from Beetle import Beetle

IMU_MAC_ADDRESS = "D0:39:72:E4:83:8C" 
# IMU_MAC_ADDRESS = "D0:39:72:E4:8E:22" # imu

def run_beetle(beetle):
    while True:
        beetle.connect()
        beetle.initialise_handshake()
        beetle.collect_data()

if __name__ == '__main__':
    imu_beetle = Beetle(IMU_MAC_ADDRESS, "IMU")
    run_beetle(imu_beetle)