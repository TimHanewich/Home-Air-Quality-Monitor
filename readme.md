# Home Air Quality Monitor
This is a project for monitoring temperature, humidity, and air quality (TVOC) for rooms in a household. The project has three separate code bases:
- [pi](./src/pi/) - code that can be loaded onto a Raspberry Pi Pico or other micropython microcontroller and act as either a **sender** or a **receiver**
    - **sender** = the microcontroller will be equipped with a temperature, humidity, and air quality sensor and will continuously poll for data and send it to a receiver via radio communication (HC-12).
    - **receiver** = a microcontroller that only has a radio receiver (HC-12) and listens for sensor data packets from senders. A reciever can be plugged into a computer via its USB port and a lightweight program can be ran on that computer to access those readings over serial communication.
- [server](./src/server/) - the program that runs on a computer that is connected to a **receiver**. Lightweight .NET program for receiving data via the microcontroller over USB, parsing that data, and uploading it to a cloud-based SQL server (i.e. Azure SQL)
- [api](./src/api/) - ASP.NET API for accessing and reporting most recent air quality data on the cloud-based SQL server.

## Location Identifiers
I use **location IDs** to specify which part of the home each reading is for. Can be 0-7, eight unique values, because it is represented by 3 bits.
- **0** = *reserved for testing*
- **1** = Master bedroom

## SQL table creation
```
create table AirQualityReading(ID int IDENTITY(0, 1) PRIMARY KEY, Captured DateTime, Location tinyint, Temperature tinyint, Humidity tinyint, AQI tinyint, TVOC smallint, ECO2 smallint)
```