SQL table creation:
```
create table AirQualityReading(ID int IDENTITY(0, 1) PRIMARY KEY, Captured DateTime, Location tinyint, Temperature tinyint, Humidity tinyint, AQI tinyint, TVOC smallint, ECO2 smallint)
```

## Location Identifiers
I use **location IDs** to specify which part of the home each reading is for. Can be 0-7, eight unique values, because it is represented by 3 bits.
- **0** = *reserved for testing*
- **1** = Master bedroom