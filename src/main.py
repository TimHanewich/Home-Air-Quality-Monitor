print("Hello! Home Air Quality Monitor system booting...")

# load dependencies
print("Loading dependencies...")
import machine
import time
import HC12
import ENS160
import dht

# set up LED with fatal sequences
print("Setting up Pico LED...")
led = machine.Pin("LED", machine.Pin.OUT)
led.on()
def FATAL() -> None:
    while True:
        led.on()
        time.sleep(1.0)
        led.off()
        time.sleep(1.0)

# Set up HC-12
print("Setting up HC-12 transceiver...")
uart = machine.UART(0, tx=machine.Pin(12), rx=machine.Pin(13), baudrate=9600)
hc12 = HC12.HC12(uart, 11) # with set pin (GP11)
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

# Configure the HC-12: Transmit power
time.sleep(1.0)
try:
    print("Setting HC-12 transmit power...")
    hc12.power = 4
    print("HC-12 transmit power set successfully!")
except Exception as ex:
    print("HC-12 transmit power set failed! Failing. Msg: " + str(ex))
    FATAL()

# Set up DHT22
dht22 = dht.DHT22(machine.Pin(22))
try:
    dht22.measure()
    print("DHT22 temperature: " + str(dht22.temperature()))
    print("DHT22 Humidity: " + str(dht22.humidity()))
except Exception as ex:
    print("DHT-22 connection failed! Msg: " + str(ex))
    FATAL()

# next, set up ENS160
i2c = machine.I2C(0, sda=machine.Pin(16), scl=machine.Pin(17))
i2c_scan:list[int] = i2c.scan()
print("I2C scan: " + str(i2c_scan))
if 83 not in i2c_scan:
    print("ENS160 (0x53) not detected on I2C bus! Is it connected?")
    FATAL()
ens = ENS160.ENS160(i2c)
print("Starting up ENS160...")
ens.operating_mode = 2 # standard gas sensing
time.sleep(2.0) # wait a moment for it to boot
try:
    print("AQI: " + str(ens.AQI))
    time.sleep(0.25)
    print("TVOC: " + str(ens.TVOC))
    time.sleep(0.25)
    print("ECO2: " + str(ens.ECO2))
except Exception as ex:
    print("Failed reading data from ENS-160! Failing. Msg: " + str(ex))
    FATAL()

