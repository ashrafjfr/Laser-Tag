#include <IRremote.hpp>
#include <ezButton.h>
#include <TM1637Display.h>

#define IR_SEND_PIN 2
#define LASER_PIN A0
#define CLK_PIN 4
#define DIO_PIN 5

int ammo = 6;
int i = 0;
int state = LOW;
int prevState = LOW;

ezButton limitSwitch(3);
TM1637Display display = TM1637Display(CLK_PIN, DIO_PIN);

void setup() {
    Serial.begin(115200);
    pinMode(LASER_PIN, OUTPUT);
    IrSender.begin(IR_SEND_PIN);
    limitSwitch.setDebounceTime(50);
    digitalWrite(LASER_PIN, HIGH);
    display.setBrightness(3);
    display.clear();
}

void loop() {
    limitSwitch.loop();
    display.showNumberDec(ammo, false, 1, 2);
    int state = limitSwitch.getState();   
    if(ammo != 0 && state == HIGH && prevState == LOW) {
      Serial.println("Sent " + String(i));
      i++;
      ammo--;
      IrSender.sendSony(0x30, 20); //384
      delay(500);
    }
    
    if (ammo == 0) {
      for (int j = 0; j < 20; j++) {
        display.showNumberDec(ammo, false, 1, 2);
        delay(50);
        display.clear();
        delay(50);
      }
      ammo = 6;
    }
    prevState = state;
    delay(50);
}
