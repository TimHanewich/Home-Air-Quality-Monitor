SQL table creation:
```
create table AirQualityReading(ID int IDENTITY(0, 1) PRIMARY KEY, Captured DateTime, Location tinyint, Temperature tinyint, Humidity tinyint, AQI tinyint, TVOC smallint, ECO2 smallint)
```