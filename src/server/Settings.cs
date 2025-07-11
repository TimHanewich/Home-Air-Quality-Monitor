using System;

namespace TestingSerial
{
    public class Settings
    {
        public string SerialID { get; set; } //i.e "/dev/ttyACM0" or "COM6"
        public string SQLConnectionString { get; set; }

        public Settings()
        {
            SerialID = "<SERIAL PORT HERE>";
            SQLConnectionString = "<SQL CONNECTION STRING HERE>";
        }
    }
}