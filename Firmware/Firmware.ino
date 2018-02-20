#include <DigiUSB.h>

void setup() {
  DigiUSB.begin();
}

void loop() {
  //poll usb 
  DigiUSB.refresh();
  
  int value = analogRead(1); //This is Pin3
  if(value>1020)
    value = 255;
  else if(value<2)
    value = 0;
  else
    value = value/4;
  
  value = round(byte(value));
  DigiUSB.write(value);
}
