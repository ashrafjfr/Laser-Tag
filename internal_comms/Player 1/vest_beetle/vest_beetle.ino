#include <IRremote.hpp>
#include <Adafruit_NeoPixel.h>
#include <CRC32.h>

#define irPIN 2
#define hpLEDPIN 3
#define vibPIN 4
#define NUMPIXELS 12
#define DELAYVAL 500

#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define DATA_PACKET 'D'

CRC32 crc;

int health = 100;
int ledsLit = 12;

Adafruit_NeoPixel pixels(NUMPIXELS, hpLEDPIN, NEO_GRB + NEO_KHZ800);

int seqNum = 0;
char dataReceived[20];
bool isHandshakeComplete = false;
bool isDataSending = false; 
uint8_t global_seqNum = 0;  
bool finalAckReceived = false;
bool isHandshakeInitialised = false;

float gx = 0;
float gy = 0;
float gz = 0;
float ax = 0;
float ay = 0;
float az = 0;

struct packet {
  char packetType;
  uint8_t seqNum;
  uint8_t beetleID = 2;   //imu: 0, gun: 1, vest: 2
  bool shotReceived; 
  short gyroX;
  short gyroY;
  short gyroZ;
  short accX;
  short accY;
  short accZ;
  uint32_t crcValue;
};


void setup(void) {
  Serial.begin(115200);
  pixels.begin();
  pixels.show();
  pinMode(vibPIN, OUTPUT);
  IrReceiver.begin(irPIN, ENABLE_LED_FEEDBACK);
}

void readSerial() {
  memset(dataReceived, 0, sizeof(dataReceived));
  while (Serial.available() < 20);
  Serial.readBytes(dataReceived, 20);
  //Serial.println(dataReceived);
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
      if ((dataReceived[0] == HELLO_PACKET) && (!isHandshakeInitialised)) {
        ackHandshake();
        isHandshakeInitialised = true;
        delay(250);
      } else if ((dataReceived[0] == HELLO_PACKET) && (isHandshakeInitialised)) { // set isHandshakeInitialised to false when we receive another hello packet
        isHandshakeInitialised = false;
      } else if ((dataReceived[0] == ACK_PACKET) && (!isHandshakeInitialised) && (dataReceived[2] == 'H')) { //3rd byte = H if its a handshake ack from the relay node
        isHandshakeInitialised = true;
      } else if ((dataReceived[0] == DATA_PACKET) && (isHandshakeInitialised)) {
          int hp_hundreds = (dataReceived[2] - '0') * 100;
          int hp_tens = (dataReceived[3] - '0') * 10;
          int hp_ones = dataReceived[4] - '0';    
          int health_received = hp_hundreds + hp_tens + hp_ones;

          show_hp(health_received);
      } else {
          show_hp(100);
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
    dataPacket.shotReceived = true;
    dataPacket.gyroX = 0;
    dataPacket.gyroY = 0;
    dataPacket.gyroZ = 0;
    dataPacket.accX = 0;
    dataPacket.accY = 0;
    dataPacket.accZ = 0;
    dataPacket.crcValue = calculateCRC32((byte*)&dataPacket, 16);
    Serial.write((byte*)&dataPacket, sizeof(dataPacket));
  }
}

void show_hp(int health) {
  if (health >= 91) {
      ledsLit = 10;
    } else if (health >= 81) {
      ledsLit = 9;
    } else if (health >= 71) {
      ledsLit = 8;
    } else if (health >= 61) {
      ledsLit = 7;
    } else if (health >= 51) {
      ledsLit = 6;
    } else if (health >= 41) {
      ledsLit = 5;
    } else if (health >= 31) {
      ledsLit = 4;
    } else if (health >= 21) {
      ledsLit = 3;
    } else if (health >= 11) {
      ledsLit = 2;
    } else if (health >= 1) {
      ledsLit = 1;
    } else {
      ledsLit = 0;
    }
  
    for (int i = 0; i < NUMPIXELS; i++) {
      if (ledsLit > 6) {
        for (int j = 0; j < ledsLit; j++) {
          pixels.setPixelColor(j, pixels.Color(0, 255, 0));
        } 
        for (int k = 0; k < NUMPIXELS - ledsLit; k++) {
          pixels.setPixelColor((k + ledsLit), pixels.Color(0, 0, 0));
        }
      } else if (ledsLit > 3 && ledsLit <= 6) {
        for (int j = 0; j < ledsLit; j++) {
          pixels.setPixelColor(j, pixels.Color(135, 205, 0));
        } 
        for (int k = 0; k < NUMPIXELS - ledsLit; k++) {
          pixels.setPixelColor((k + ledsLit), pixels.Color(0, 0, 0));
        }
      } else if (ledsLit >= 1 && ledsLit <= 3) {
        for (int j = 0; j < ledsLit; j++) {
          pixels.setPixelColor(j, pixels.Color(255, 0, 0));
        } 
        for (int k = 0; k < NUMPIXELS - ledsLit; k++) {
          pixels.setPixelColor((k + ledsLit), pixels.Color(0, 0, 0));
        }
      } else {
        pixels.setPixelColor(i, pixels.Color(0, 0, 0));
      }
    }
}

void run() {
  while (isHandshakeInitialised) {
    if (Serial.available() > 0) {
        readSerial();
        receive_data();
      }  
    if (IrReceiver.decode()) {
      if (IrReceiver.decodedIRData.address == 384){
        //Serial.println("Received valid code");
        send_data();    
        digitalWrite(vibPIN, HIGH);
        delay(250);
        digitalWrite(vibPIN, LOW);
      } else {
        //Serial.println("Received invalid code");
      }
      IrReceiver.resume();
    }
    
    if (IrReceiver.isIdle()) {
      pixels.show();
    }
    
//    if (health == 0) {
//      delay(5000);
//      health = 100;
//    }
  
    delay(100);
  }
}


void loop() {
  if (!isHandshakeInitialised) {
    readSerial();
    receive_data();
  }
  run();
}
