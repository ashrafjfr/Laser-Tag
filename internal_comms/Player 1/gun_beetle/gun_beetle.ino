#include <IRremote.hpp>
#include <ezButton.h>
#include <TM1637Display.h>
#include <CRC32.h>

#define IR_SEND_PIN 2
#define LASER_PIN A0
#define CLK_PIN 4
#define DIO_PIN 5

#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define DATA_PACKET 'D'

CRC32 crc;
int ammo = 6;
int i = 0;
int state = LOW;
int prevState = LOW;

ezButton limitSwitch(3);
TM1637Display display = TM1637Display(CLK_PIN, DIO_PIN);

int seqNum = 0;
char dataReceived[20];
bool isHandshakeComplete = false;
bool isDataSending = false; 
uint8_t global_seqNum = 0;  
bool finalAckReceived = false;
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
  uint8_t beetleID = 1;   //imu: 0, gun: 1, vest: 2
  bool shotFired; 
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
    pinMode(LASER_PIN, OUTPUT);
    IrSender.begin(IR_SEND_PIN);
    limitSwitch.setDebounceTime(50);
    digitalWrite(LASER_PIN, HIGH);
    display.setBrightness(3);
    display.clear();
    
    state = limitSwitch.getState();
    prevState = state;
}

void readSerial() {
  memset(dataReceived, 0, sizeof(dataReceived));
  while (Serial.available() < 20);
  Serial.readBytes(dataReceived, 20);
  //Serial.println(dataReceived);
}

bool isValidCRC() {
    uint32_t expectedCRC = calculateCRC32((byte*)dataReceived, 16);
    //Serial.println(expectedCRC);
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
      if ((dataReceived[0] == 'H') && (!isHandshakeInitialised)) {
        ackHandshake();
        isHandshakeInitialised = true;
        delay(250);
        display.showNumberDec(6, false, 1, 2); // show 6 after handshake initialized
      } else if ((dataReceived[0] == 'H') && (isHandshakeInitialised)) { // set isHandshakeInitialised to false when we receive another hello packet
        isHandshakeInitialised = false;
      } else if ((dataReceived[0] == 'A') && (!isHandshakeInitialised) && (dataReceived[2] == 'H')) { //3rd byte = H if its a handshake ack from the relay node
        isHandshakeInitialised = true;
        display.showNumberDec(6, false, 1, 2); // show 6 after handshake initialized
      } else if ((dataReceived[0] == 'A') && (isHandshakeInitialised) && (dataReceived[2] == 'D') && (dataReceived[1] - '0' == seqNum)) {
        isPacketAck = true;
        seqNum = 1 - seqNum;
      } else if ((dataReceived[0] == 'D') && (isHandshakeInitialised)) {
          ammo = dataReceived[2] - '0';
          display.showNumberDec(ammo, false, 1, 2);
      } else {
          display.showNumberDec(6, false, 1, 2);
      }
    }
}

void ackHandshake() {
  packet ackPacket;
  ackPacket.packetType = ACK_PACKET;
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
    dataPacket.packetType = DATA_PACKET;
    dataPacket.seqNum = seqNum;
    dataPacket.shotFired = true;
    dataPacket.gyroX = 0;
    dataPacket.gyroY = 0;
    dataPacket.gyroZ = 0;
    dataPacket.accX = 0;
    dataPacket.accY = 0;
    dataPacket.accZ = 0;
    dataPacket.crcValue = calculateCRC32((byte*)&dataPacket, 16);
    Serial.write((byte*)&dataPacket, sizeof(dataPacket));
    delay(20);
  }
}

void run() {
  while (isHandshakeInitialised) {
    int counter = 0;
    if (Serial.available() > 0) {
      readSerial();
      receive_data();
    }  
    
    limitSwitch.loop();
    int state = limitSwitch.getState();   
    if(state == HIGH && prevState == LOW) { // for 1 player game 
//    if(ammo != 0 && state == HIGH && prevState == LOW) {
      
      //Serial.println("Sent " + String(i));
      i++;
      //ammo--;
      IrSender.sendSony(0x30, 20); 
      //IrSender.sendSony(0x1, 0x30, 0); 

      send_data();
      //display.showNumberDec(7, false, 1, 2);
      if (Serial.available() > 0) {
        display.showNumberDec(7, false, 1, 2);
        readSerial();
        receive_data();
        
      }  
      if (isPacketAck) {
        break;
      }
      isPacketAck = false; 
 
      delay(500);
    }
    
//    if (ammo == 0) {
//      for (int j = 0; j < 20; j++) {
//        display.showNumberDec(ammo, false, 1, 2);
//        delay(50);
//        display.clear();
//        delay(50);
//      }
//      ammo = 6;
//    }
    prevState = state;
    delay(50);
    

  }
}

void loop() {
  if (!isHandshakeInitialised) {
    readSerial();
    receive_data();
  }
  run();
}
