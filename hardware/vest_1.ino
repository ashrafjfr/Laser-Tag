#include <IRremote.hpp>
#include <Adafruit_NeoPixel.h>

#define irPIN 2
#define hpLEDPIN 3
#define vibPIN 4
#define NUMPIXELS 12
#define DELAYVAL 500

int health = 100;
int ledsLit = 12;

Adafruit_NeoPixel pixels(NUMPIXELS, hpLEDPIN, NEO_GRB + NEO_KHZ800);

void setup(void) {
  Serial.begin(115200);
  pixels.begin();
  pixels.show();
  pinMode(vibPIN, OUTPUT);
  IrReceiver.begin(irPIN, ENABLE_LED_FEEDBACK);
}

void loop() {
  if (IrReceiver.decode()) {
    if (IrReceiver.decodedIRData.address == 256){
      Serial.println("Received valid code");
      health = health - 10;
      digitalWrite(vibPIN, HIGH);
      delay(250);
      digitalWrite(vibPIN, LOW);
    } else {
      Serial.println("Received invalid code");
    }
    IrReceiver.resume();
  }

  if (health >= 88) {
    ledsLit = 12;
  } else if (health >= 80) {
    ledsLit = 11;
  } else if (health >= 72) {
    ledsLit = 10;
  } else if (health >= 64) {
    ledsLit = 9;
  } else if (health >= 56) {
    ledsLit = 8;
  } else if (health >= 48) {
    ledsLit = 7;
  } else if (health >= 40) {
    ledsLit = 6;
  } else if (health >= 32) {
    ledsLit = 5;
  } else if (health >= 24) {
    ledsLit = 4;
  } else if (health >= 16) {
    ledsLit = 3;
  } else if (health >= 8) {
    ledsLit = 2;
  } else if (health >= 1) {
    ledsLit = 1;
  } else {
    ledsLit = 0;
  }

  for (int i = 0; i < NUMPIXELS; i++) {
    if (ledsLit > 8) {
      for (int j = 0; j < ledsLit; j++) {
        pixels.setPixelColor(j, pixels.Color(0, 255, 0));
      } 
      for (int k = 0; k < NUMPIXELS - ledsLit; k++) {
        pixels.setPixelColor((k + ledsLit), pixels.Color(0, 0, 0));
      }
    } else if (ledsLit > 4 && ledsLit <= 8) {
      for (int j = 0; j < ledsLit; j++) {
        pixels.setPixelColor(j, pixels.Color(135, 205, 0));
      } 
      for (int k = 0; k < NUMPIXELS - ledsLit; k++) {
        pixels.setPixelColor((k + ledsLit), pixels.Color(0, 0, 0));
      }
    } else if (ledsLit >= 1 && ledsLit <= 4) {
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
  
  if (IrReceiver.isIdle()) {
    pixels.show();
  }
  
  if (health == 0) {
    delay(5000);
    health = 100;
  }

  delay(100);
}
