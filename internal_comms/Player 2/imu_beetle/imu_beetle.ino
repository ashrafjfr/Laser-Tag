#include <CRC32.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

Adafruit_MPU6050 mpu;

CRC32 crc;

int seqNum = 0;
char dataReceived[20];
bool isHandshakeInitialised = false;
bool isPacketAck = false;

float gx = 0;
float gy = 0;
float gz = 0;
float ax = 0;
float ay = 0;
float az = 0;

struct packet {
  char packetType;
  uint8_t seqNum;
  uint8_t beetleID = 0;   //imu: 0, gun: 1, vest: 2
  bool shotReceived; 
  short gyroX;
  short gyroY;
  short gyroZ;
  short accX;
  short accY;
  short accZ;
  uint32_t crcValue;
};

void setup() {
    Serial.begin(115200);
    reinitializeSensor();
    delay(100);    
}

void readSerial() {
  memset(dataReceived, 0, sizeof(dataReceived));
  while (Serial.available() < 20);
  Serial.readBytes(dataReceived, 20);
  //Serial.println(dataReceived);
}

void read_sensor() {
    sensors_event_t a, g, temp;

    int readAttempts = 3;
    bool readSuccess = false;

    while (readAttempts > 0) {
        if (mpu.getEvent(&a, &g, &temp)) {
            gx = g.gyro.x;
            gy = g.gyro.y;
            gz = g.gyro.z;
            ax = a.acceleration.x;
            ay = a.acceleration.y;
            az = a.acceleration.z;  
            readSuccess = true;
            break;  // Exit the loop if reading was successful
        } else {
            readAttempts--;
            delay(10);  // Small delay before trying to read again
        }
    }

    if (!readSuccess) {
        // If reading failed even after multiple attempts
        reinitializeSensor();
    }
}

void reinitializeSensor() {
    if (!mpu.begin()) {
        Serial.println("Failed to initialize MPU6050!");
        while (1) {
            delay(10);  
        }
    }

    mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
    mpu.setGyroRange(MPU6050_RANGE_500_DEG);
    mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);
}

bool isValidCRC() {
    uint32_t expectedCRC = calculateCRC32((byte*)dataReceived, 16);
    for (int i = 0; i < 4; i++) {
        uint8_t expectedByte = (expectedCRC >> (i*8)) & 0xFF;
        if (expectedByte != dataReceived[19-i]) {
            return false;
        }
    }
    return true;
}

void receive_data() {
    if (isValidCRC()) {
      if ((dataReceived[0] == 'H') && (!isHandshakeInitialised)) { //first hello packet received
        ackHandshake();
        isHandshakeInitialised = true;
        delay(250);
      } else if ((dataReceived[0] == 'H') && (isHandshakeInitialised)) { // set isHandshakeInitialised to false when we receive another hello packet
        isHandshakeInitialised = false;
      } else if ((dataReceived[0] == 'A') && (!isHandshakeInitialised) && (dataReceived[2] == 'H')) { //3rd byte = H if its a handshake ack from the relay node
        isHandshakeInitialised = true;
      } else if ((dataReceived[0] = 'A') && (isHandshakeInitialised) && (dataReceived[2] == 'H') && (dataReceived[1] - '0' == seqNum)) {
        isPacketAck = true;
        seqNum = 1 - seqNum;
      }
  
    }
}

void ackHandshake() {
  packet ackPacket;
  ackPacket.packetType = 'A';
  ackPacket.crcValue = calculateCRC32((byte*)&ackPacket, 16);
  Serial.write((byte*)&ackPacket, sizeof(ackPacket));
}

uint32_t calculateCRC32(byte *data, size_t length) {
    crc.reset();
    for (size_t i = 0; i < length; i++) {
        crc.update(data[i]);
    }
    return crc.finalize();
}

void send_data() {
  if (isHandshakeInitialised) {
    packet dataPacket;
    dataPacket.packetType = 'D';
    dataPacket.seqNum = seqNum;
    dataPacket.gyroX = (short)(gx * 1000);
    dataPacket.gyroY = (short)(gy * 1000);
    dataPacket.gyroZ = (short)(gz * 1000);
    dataPacket.accX = (short)(ax * 1000);
    dataPacket.accY = (short)(ay * 1000);
    dataPacket.accZ = (short)(az * 1000);
    dataPacket.crcValue = calculateCRC32((byte*)&dataPacket, 16);
    Serial.write((byte*)&dataPacket, sizeof(dataPacket));
    delay(50);
  }
}

void run() {
  while (isHandshakeInitialised) {
    read_sensor();
    send_data();
    seqNum = 1 - seqNum;    
    if (Serial.available() > 0) {
      readSerial();
      receive_data();
    }
    if (!isPacketAck) {
      break;
    }
    isPacketAck = false;
  }
}

void loop() {
  if (!isHandshakeInitialised) {
    readSerial();
    receive_data();
  }
  run();
}