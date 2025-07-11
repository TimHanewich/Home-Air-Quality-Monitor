print("Hello! Home Air Quality Monitor (Receiver) booting...")

print("Loading dependencies...")
import machine
import DataPackets
import time
import HC12

# Set up LED
led = machine.Pin("LED", machine.Pin.OUT)
led.on() # turn on while loading
def FATAL() -> None:
    while True:
        led.on()
        time.sleep(1.0)
        led.off()
        time.sleep(1.0)

# Set up HC-12
print("Setting up HC-12 transceiver...")
uart = machine.UART(0, tx=machine.Pin(17), rx=machine.Pin(16), baudrate=9600)
hc12 = HC12.HC12(uart, 15) # with set pin (GP15)
hc12_pulsed:bool = False
for a in range(3): # try 3 times
    time.sleep(1.0)
    print("Validating HC-12 connection attempt # " + str(a + 1) + "... ")
    if hc12.pulse:
        hc12_pulsed = True
        print("HC-12 connection confirmed!")
        break
    else:
        time.sleep(1.0)
if not hc12_pulsed:
    print("Connection to HC-12 unsuccessful. A pulse was not received successfully after several attempts. Failing.")
    FATAL()

# Configure the HC-12: operating mode
time.sleep(1.0)
try:
    print("Setting HC-12 operating mode to FU3...")
    hc12.mode = 3 # Set to FU3 (default), general-purpose mode
    print("HC-12 operating mode set to FU3!")
except Exception as ex:
    print("Failure while setting HC-12 operating mode to FU3! Failing. Msg: " + str(ex))
    FATAL()

# Configure the HC-12: channel
time.sleep(1.0)
try:
    print("Setting HC-12 channel to 1...")
    hc12.channel = 1
    print("HC-12 channel set to channel 1!")
except Exception as ex:
    print("HC-12 channel setting to 1 unsuccesful. Failing. Msg: " + str(ex))
    FATAL()

# Infinite receiver loop
print("Entering infinite receive loop...")
MsgsReceived:int = 0
led.off() # turn LED off now... from now on, it will only be on when receiving a message
time.sleep(1.0)
while True:

    # receive
    print("Checking for message...")
    NewData:bytes = hc12.receive()
    if len(NewData) == 0:
        print("No new data!")
    else:
        print(str(len(NewData)) + " new bytes received!")

        # is it a standard packet?
        if DataPackets.is_standard_packet(NewData):
            print("It is a StandardPacket!")

            # decode it
            sp:DataPackets.StandardPacket = DataPackets.StandardPacket()
            sp.decode(NewData)

            # print it
            print("Temperature: " + str(sp.temperature) + ", Humidity: " + str(sp.humidity) + ", AQI: " + str(sp.AQI) + ", TVOC: " + str(sp.TVOC) + ", ECO2: " + str(sp.ECO2))
    
        else:
            print("Data payload of " + str(len(NewData)) + " not recognized as an understood packet type!")
            print("Data received: " + str(NewData))

    # wait
    WaitTime:int = 3
    print("Waiting for " + str(WaitTime) + " seconds before checking for new data...")
    time.sleep(WaitTime)