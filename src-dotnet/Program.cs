using System;
using System.IO.Ports;

namespace TestingSerial
{
	public class Program
	{
		public static void Main(string[] args)
		{
			SerialPort sp = new SerialPort("/dev/ttyACM0", 115200); //"dev/ttyACM0" for example for Linux. Windows would be "COM3" or "COM8" for example
			sp.DtrEnable = true; //must be set to true for the Pico to know to start transmiting data. If you use the serial module in Python, that module sets this automatically!
			sp.Open(); //Open the connection

			while (true)
			{
				if (sp.BytesToRead > 0)
				{
					byte[] buffer = new byte[sp.BytesToRead];
					sp.Read(buffer, 0, buffer.Length);
					foreach (byte b in buffer)
					{
						Console.Write(b.ToString() + ", ");
					}
					Console.WriteLine();
				}
				else
				{
					Console.WriteLine("No bytes available!");
				}

				System.Threading.Tasks.Task.Delay(1000).Wait();
			}
			
			//Should close the connection at the end but that is done automatically when Ctrl+C happenss

		}
	}
}
