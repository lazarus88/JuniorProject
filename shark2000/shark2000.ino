#include <Wire.h>
#include <BleMouse.h>
#include <WiFi.h>
#include <HTTPClient.h>

uint8_t data[6];
int16_t gyroX, gyroY, gyroZ;
int16_t gyroX0;
int Sensitivity = 300;
int delayi = 20;
bool ind = false;
BleMouse bleMouse;
uint32_t timer;
uint8_t i2cData[14];

const uint8_t IMUAddress = 0x68;
const uint16_t I2C_TIMEOUT = 1000;

uint8_t i2cWrite(uint8_t registerAddress, uint8_t* data, uint8_t length, bool sendStop)
{
    Wire.beginTransmission(IMUAddress);
    Wire.write(registerAddress);
    Wire.write(data, length);
    return Wire.endTransmission(sendStop);
}

uint8_t i2cWrite2(uint8_t registerAddress, uint8_t data, bool sendStop)
{
    return i2cWrite(registerAddress, &data, 1, sendStop);
}

uint8_t i2cRead(uint8_t registerAddress, uint8_t* data, uint8_t nbytes)
{
    uint32_t timeOutTimer;
    Wire.beginTransmission(IMUAddress);
    Wire.write(registerAddress);
    if (Wire.endTransmission(false))
        return 1;
    Wire.requestFrom(IMUAddress, nbytes, (uint8_t)true);
    for (uint8_t i = 0; i < nbytes; i++)
    {
        if (Wire.available())
            data[i] = Wire.read();
        else
        {
            timeOutTimer = micros();
            while (((micros() - timeOutTimer) < I2C_TIMEOUT) && !Wire.available()) ;
            if (Wire.available())
                data[i] = Wire.read();
            else
                return 2;
        }
    }
    return 0;
}

void setup()
{
    char* ssid = "ABS";
    char* password = "chungachanga";
    Wire.begin(8, 9);
    pinMode(D3, INPUT);
    pinMode(D1, INPUT);
    i2cData[0] = 7;
    i2cData[1] = 0x00;
    i2cData[3] = 0x00;

   while (i2cWrite(0x19, i2cData, 4, false)) ;
   while (i2cWrite2(0x6B, 0x01, true)) ;
   while (i2cRead(0x75, i2cData, 1)) ;
   delay(100);
   while (i2cRead(0x3B, i2cData, 6)) ;

    timer = micros();
    Serial.begin(115200);
    bleMouse.begin();
    WiFi.begin(ssid, password);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(1000);
        Serial.println("Connecting to WiFi...");
    }

    Serial.println("Connected to WiFi");
    delay(1000);
}

void loop()
{
    
    char* serverName1 = "http://54.93.212.178/AdminRestaurant/set-print-input?message=https://sketchpad.app/";
    char* serverName2 = "http://54.93.212.178/AdminRestaurant/set-print-input?message=https://poki.com/en/g/fruit-ninja";
    while (i2cRead(0x3B, i2cData, 14)) ;

    gyroX = ((i2cData[8] << 8) | i2cData[9]);
    gyroY = ((i2cData[10] << 8) | i2cData[11]);
    gyroZ = ((i2cData[12] << 8) | i2cData[13]);
    Serial.println(abs(gyroX - gyroX0));
    if (digitalRead(D1) == HIGH)
    {
        delay(1000);
        if (digitalRead(D1) == HIGH)
        {
            if (WiFi.status() == WL_CONNECTED)
            {
                HTTPClient http;
                http.begin(serverName2);
                int httpResponseCode = http.GET(); 

                if (httpResponseCode > 0)
                {
                    String response = http.getString(); 
                    Serial.println(httpResponseCode);   
                    Serial.println(response);     
                }
                else
                {
                    Serial.print("Error on sending request: ");
                    Serial.println(httpResponseCode);
                    Serial.println(WiFi.status());
                    Serial.println(http.errorToString(httpResponseCode));
                }
                http.end(); 
            }
        }
            else
            {
                if (WiFi.status() == WL_CONNECTED)
                {
                    HTTPClient http;
                    http.begin(serverName1);
                    int httpResponseCode = http.GET(); 

                    if (httpResponseCode > 0)
                    {
                        String response = http.getString();
                       Serial.println(httpResponseCode);   
                       Serial.println(response);     
                    }
                    else
                    {
                        Serial.print("Error on sending request: ");
                        Serial.println(httpResponseCode);
                        Serial.println(WiFi.status());
                        Serial.println(http.errorToString(httpResponseCode));
                    }
                    http.end();  
                }

                
            }
            delay(1000);
        
    }
        if (abs(gyroX - gyroX0) < 1100)
        {
            gyroX = gyroX0;
            ind = false;
        }
        gyroX0 = gyroX;
        if (ind)
            gyroX = (gyroX + 1000) / Sensitivity / -1.2;
        else
            gyroX = (gyroX + 600) / Sensitivity / -1.2;
        gyroY = gyroY / Sensitivity * -1;
        gyroZ = gyroZ / Sensitivity * -1;
        ind = true;
        if (bleMouse.isConnected())
        {
            bleMouse.move(gyroZ, gyroX);
        }
        if (digitalRead(D3) == HIGH && !bleMouse.isPressed())
        {
            bleMouse.press();
        }
        delay(delayi);
        if (digitalRead(D3) == LOW && bleMouse.isPressed())
        {
            bleMouse.release();
        }
}