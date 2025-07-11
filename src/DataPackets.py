import binary

def is_standard_packet(packet:bytes) -> bool:
    if len(packet) == 0:
        return False
    bits:list[bool] = binary.byte_to_bits(packet[0])
    return not bits[0] # if the first bit is 0, it is a standard packet. If it is 1, it is a special (unstructured) packet

class StandardPacket:
    """Standard packet containing sensor readings"""

    def __init__(self):
        self.temperature:int = 0 # temperature, 0-255 (8 bits)
        self.humidity:int = 0 # relative humidity, 0-255 (8 bits)
        self.AQI:int = 0 # air quality index, 1-5 (3 bits)
        self.TVOC:int = 0 # Total voltatile organic compounds, 0-65,535 (16 bits)
        self.ECO2:int = 0 # estimated CO2 content, 0-65,535 (16 bits)

    def encode(self) -> bytes:
        """Encodes sensor reading package as a raw data package."""

        # create a list of bits
        AllBits:list[bool] = []

        # populate it
        AllBits.append(False) # first bit is 0 for standard packet, which this is

        # populate temperature bits
        TempToUse:int = max(min(self.temperature, 255), 0) # force btween 0-255
        AllBits.extend(binary.byte_to_bits(TempToUse))

        # populate humidity bits
        HumidityToUse:int = max(min(self.humidity, 255), 0) # force btween 0-255
        AllBits.extend(binary.byte_to_bits(HumidityToUse))

        # populate AQI, only adding the last 3 bits
        AQIToUse:int = max(min(self.AQI, 5), 0)
        AQI_bits:list[bool] = binary.byte_to_bits(AQIToUse)
        AllBits.append(AQI_bits[5])
        AllBits.append(AQI_bits[6])
        AllBits.append(AQI_bits[7])

        # populate TVOC
        TVOC_bytes:bytes = max(min(self.TVOC, 65535), 0).to_bytes(2, "little")
        AllBits.extend(binary.byte_to_bits(TVOC_bytes[0]))
        AllBits.extend(binary.byte_to_bits(TVOC_bytes[1]))

        # populate ECO2
        ECO2_bytes:bytes = max(min(self.ECO2, 65535), 0).to_bytes(2, "little")
        print("ECO2 bytes: " + str(ECO2_bytes))
        AllBits.extend(binary.byte_to_bits(ECO2_bytes[0]))
        AllBits.extend(binary.byte_to_bits(ECO2_bytes[1]))
        
        # convert all bits to bytes
        ToReturn:bytearray = bytearray()
        BitBuffer:list[bool] = []
        while len(AllBits) > 0:
            BitBuffer.append(AllBits.pop(0)) # remove and add
            if len(BitBuffer) == 8:
                ToReturn.append(binary.bits_to_byte(BitBuffer))
                BitBuffer.clear()
        if len(BitBuffer) > 0:
            while len(BitBuffer) < 8:
                BitBuffer.append(False)
            ToReturn.append(binary.bits_to_byte(BitBuffer))

        return ToReturn
    
    def decode(self, data:bytes) -> None:
        """Decode a data packet."""

        if len(data) != 7: # standard packet is 7 bytes
            raise Exception("Provided data '" + str(data) + "' is not a valid byte packet.")
        
        if is_standard_packet(data) == False:
            raise Exception("Provided data '" + str(data) + " is not a valid StandardPacket.")
        
        # Convert all bytes to bits
        AllBits:list[bool] = []
        for byte in data:
            AllBits.extend(binary.byte_to_bits(byte))
        
        # First bit is a packet identifier - get rid of it
        AllBits.pop(0)

        BitBuffer:list[bool] = []

        # Next 8 are temperature
        for _ in range(8):
            BitBuffer.append(AllBits.pop(0))
        self.temperature = binary.bits_to_byte(BitBuffer)
        BitBuffer.clear()

        # Next 8 are humidity
        for _ in range(8):
            BitBuffer.append(AllBits.pop(0))
        self.humidity = binary.bits_to_byte(BitBuffer)
        BitBuffer.clear()

        # Next 3 are AQI
        for _ in range(3):
            BitBuffer.append(AllBits.pop(0))
        while len(BitBuffer) < 8: # build it out until it is a full byte
            BitBuffer.insert(0, False)
        self.AQI = binary.bits_to_byte(BitBuffer)
        self.AQI = max(min(self.AQI, 5), 0)
        BitBuffer.clear()

        # next 16 are TVOC
        for _ in range(16):
            BitBuffer.append(AllBits.pop(0))
        b1 = binary.bits_to_byte(BitBuffer[0:8])
        b2 = binary.bits_to_byte(BitBuffer[8:16])
        self.TVOC = int.from_bytes(bytes([b1, b2]), "little")
        BitBuffer.clear()

        # next 16 are ECO2
        for _ in range(16):
            BitBuffer.append(AllBits.pop(0))
        b1 = binary.bits_to_byte(BitBuffer[0:8])
        b2 = binary.bits_to_byte(BitBuffer[8:16])
        self.ECO2 = int.from_bytes(bytes([b1, b2]), "little")
        BitBuffer.clear()