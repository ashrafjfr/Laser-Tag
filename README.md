# Laser Tag AR System

## Links to Individual Subcomponents README
* [Hardware Sensors](hardware/README.md) (done by Ashraf Jaffar)
* [Hardware AI](hardware-ai/README.md) (done by Nigel Ng)
* [Internal Communications](internal_comms/README.md) (done by Wang Zhihuang)
* [External Communications](external_comms/README.md) (done by Jia Yixuan)
* [Visualiser](visualizer/README.md) (done by Lee Qi An)

## Full System Setup
Ensure that the prerequisites and set-up are done for each individual subsystem, and start them in the order below.

### Hardware 
Guns:
* Upload Gun 1’s beetle with this code(internal_comms/Player 1/gun_beetle/gun_beetle.ino)
* Upload Gun 2’s beetle with this code(internal_comms/Player 2/gun_beetle/gun_beetle.ino)
* Insert 2 AA batteries each into Gun 1 and Gun 2 battery holders

Vests:
* Upload Vest 1’s beetle with this code(internal_comms/Player 1/vest_beetle/vest_beetle.ino)
* Upload Vest 2’s beetle with this code(internal_comms/Player 2/vest_beetle/vest_beetle.ino)
* Insert 2 AA batteries each into Vest 1 and Vest 2 battery holders

Gloves:
* Upload Glove 1’s beetle with this code(internal_comms/Player 1/imu_beetle/imu_beetle.ino)
* Upload Glove 2’s beetle with this code(internal_comms/Player 2/imu_beetle/imu_beetle.ino)
* Insert 2 AAA batteries each into Glove 1 and Glove 2 battery holders

### Eval Server
Ensure that the eval server is awaiting a TCP connection from the Ultra96. The WebSocket client should be connected, with a successful handshake, and the eval port should be visible.
Password key is 1111111111111111

### Ultra96
```sh
ssh -L 5000:127.0.0.1:5000 -L 5001:127.0.0.1:5001 xilinx@makerslab-fpga-17.d2.comp.nus.edu.sg
~$ <key in ultra96 password: xilinx>
~$ su
~$ <key in password: xilinx>
~$ source /etc/profile.d/pynq_venv.sh
~$ cd ultra96
~/ultra96$ python main.py
~/ultra96$ <Enter port number>
```

#### Relay Node 1
```sh 
~$ cd internal_comms/Player\ 1
~/internal_comms/Player 1$ python3 main.py
```
#### Relay Node 2
```sh 
~$ cd internal_comms/Player\ 2
~/internal_comms/Player 2$ python3 main.py
```
