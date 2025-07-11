using System;
using System.IO.Ports;

namespace TestingSerial
{
	public class Program
	{
		public static void Main(string[] args)
		{
			SerialPort sp = new SerialPort("COM6", 115200); //"dev/ttyACM0" for example for Linux. Windows would be "COM3" or "COM8" for example
			sp.DtrEnable = true; //must be set to true for the Pico to know to start transmiting data. If you use the serial module in Python, that module sets this automatically!
			sp.Open(); //Open the connection

			while (true)
			{
				if (sp.BytesToRead > 0) //If there are bytes to read
				{
					//Read the data 
					byte[] buffer = new byte[sp.BytesToRead];
					sp.Read(buffer, 0, buffer.Length);

					//Print as text
					string AsTxt = System.Text.Encoding.UTF8.GetString(buffer);
					Console.Write(AsTxt);
				}
				else
				{
					//Can write a message here but I commented out to ensure ONLY received data is being printed to the terminal
					//Console.WriteLine("No bytes available!");
				}

				System.Threading.Tasks.Task.Delay(1000).Wait();
			}
			
			//Should close the connection at the end but that is done automatically when Ctrl+C happenss

		}
	}
}
