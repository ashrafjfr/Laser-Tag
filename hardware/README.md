# Hardware Sensors

The glove, gun and vest code in this folder is not integrated with the game engine. The code here is only meant to test the hardware components individually.

## Prerequisites
Glove:
- [Adafruit MPU6050 by Adafruit](https://github.com/adafruit/Adafruit_MPU6050)
- [Adafruit Sensor by Adafruit](https://github.com/adafruit/Adafruit_Sensor)
- [Wire by Arduino](https://github.com/arduino/ArduinoCore-avr/blob/master/libraries/Wire/src/Wire.h)
  
Gun:
- [IRremote by Arduino](https://github.com/Arduino-IRremote/Arduino-IRremote)
- [ezButton by ArduinoGetStarted, bmcdonnell](https://github.com/ArduinoGetStarted/button)
- [TM1637Display by avishorp](https://github.com/avishorp/TM1637)

Vest:
- [IRremote by Arduino](https://github.com/Arduino-IRremote/Arduino-IRremote)
- [Adafruit NeoPixel by Adafruit](https://github.com/adafruit/Adafruit_NeoPixel)

## Setup
- Upload glove.ino into the Glove Beetle
- Upload gun_1.ino into Gun 1's Beetle and gun_2.ino into Gun 2's Beetle
- Upload vest_1.ino into Vest 1's Beetle and vest_2.ino into Vest 2's Beetle
- Gun 1's shot (IR pulse) can only be detected by Vest 2 and vice-versa
- The Beetles can be connected to the laptop or using batteries to test the components (Glove - 2 AAA batteries; Gun, Vest - 2 AA batteries)
