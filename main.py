import sys
import time

while True:
    ToSend:bytes = time.ticks_ms().to_bytes(2, "little")
    sys.stdout.buffer.write(ToSend)
    time.sleep(1.0)