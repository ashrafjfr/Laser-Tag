#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

Adafruit_MPU6050 mpu;
float accelX;
float accelY;
float accelZ;
float gyroX;
float gyroY;
float gyroZ;

void setup(void) {
	Serial.begin(115200);

	// Try to initialize!
	if (!mpu.begin()) {
		Serial.println("Failed to find MPU6050 chip");
		while (1) {
		  delay(10);
		}
	}

  /**
    @brief Sets the accelerometer measurement range
    @param  new_range
            The new range to set. Must be a `mpu6050_accel_range_t`
  void Adafruit_MPU6050::setAccelerometerRange(mpu6050_accel_range_t new_range){
    Adafruit_BusIO_Register accel_config =
      Adafruit_BusIO_Register(i2c_dev, MPU6050_ACCEL_CONFIG, 1);
    Adafruit_BusIO_RegisterBits accel_range =
      Adafruit_BusIO_RegisterBits(&accel_config, 2, 3);
    accel_range.write(new_range);
  }
  */
	// set accelerometer range to +-8G
  // options: 2G (default), 4G, 8G (article), 16G
	mpu.setAccelerometerRange(MPU6050_RANGE_8_G);

  /**
    @brief Sets the gyroscope measurement range
    @param  new_range
            The new range to set. Must be a `mpu6050_gyro_range_t`
  void Adafruit_MPU6050::setGyroRange(mpu6050_gyro_range_t new_range) {
    Adafruit_BusIO_Register gyro_config =
        Adafruit_BusIO_Register(i2c_dev, MPU6050_GYRO_CONFIG, 1);
    Adafruit_BusIO_RegisterBits gyro_range =
        Adafruit_BusIO_RegisterBits(&gyro_config, 2, 3);

    gyro_range.write(new_range);
  }
  */
	// set gyro range to +- 500 deg/s
  // options: 250 (default), 500 (article), 1000, 2000
	mpu.setGyroRange(MPU6050_RANGE_500_DEG);


  /**
    @brief Sets the bandwidth of the Digital Low-Pass Filter
    @param bandwidth the new `mpu6050_bandwidth_t` bandwidth
  void Adafruit_MPU6050::setFilterBandwidth(mpu6050_bandwidth_t bandwidth) {
    Adafruit_BusIO_Register config =
        Adafruit_BusIO_Register(i2c_dev, MPU6050_CONFIG, 1);

    Adafruit_BusIO_RegisterBits filter_config =
        Adafruit_BusIO_RegisterBits(&config, 3, 0);
    filter_config.write(bandwidth);
  }
  */
  // set digital low-pass filter bandwidth to 21 Hz
  // options: 260Hz (Disables filter), 184, 94, 44, 21 (article), 10, 5 Hz
	mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

	delay(100);
}

void loop() {
	/* Get new sensor events with the readings */
	sensors_event_t a, g, temp;

  /**
    @brief  Gets the most recent sensor event, Adafruit Unified Sensor format
    @param  accel
            Pointer to an Adafruit Unified sensor_event_t object to be filled
            with acceleration event data.
    @param  gyro
            Pointer to an Adafruit Unified sensor_event_t object to be filled
            with gyroscope event data.
    @param  temp
            Pointer to an Adafruit Unified sensor_event_t object to be filled
            with temperature event data.
    @return True on successful read
  bool Adafruit_MPU6050::getEvent(sensors_event_t *accel, sensors_event_t *gyro,
                                  sensors_event_t *temp) {
    uint32_t timestamp = millis();
    _read();

    fillTempEvent(temp, timestamp);
    fillAccelEvent(accel, timestamp);
    fillGyroEvent(gyro, timestamp);

    return true;
  }
  */
	mpu.getEvent(&a, &g, &temp);

  /* Get accelerometer readings */
  accelX = a.acceleration.x;
  accelY = a.acceleration.y;
  accelZ = a.acceleration.z;

  /* Get gyroscope readings */
  gyroX = g.gyro.x;
  gyroY = g.gyro.y;
  gyroZ = g.gyro.z;


	/* Print out the values */
	Serial.print(a.acceleration.x);
	Serial.print(",");
	Serial.print(a.acceleration.y);
	Serial.print(",");
	Serial.print(a.acceleration.z);
	Serial.print(", ");
	Serial.print(g.gyro.x);
	Serial.print(",");
	Serial.print(g.gyro.y);
	Serial.print(",");
	Serial.print(g.gyro.z);
	Serial.println("");

	//About 33hz
	delay(30);
}
