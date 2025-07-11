using System;
using System.IO.Ports;

namespace TestingSerial
{
	public class Program
	{
		public static void Main(string[] args)
		{
			SerialPort sp = new SerialPort("/dev/ttyACM0", 115200);
			sp.DtrEnable = true;
			sp.Open();

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

		}
	}
}
