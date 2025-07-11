using System;
using System.IO.Ports;
using Spectre;
using Spectre.Console;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace TestingSerial
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Go();
		}

		public static void Go()
		{
			AnsiConsole.MarkupLine("[bold][blue]Welcome![/][/]");

			//Ask what serial port
			AnsiConsole.WriteLine("What is the serial port of the radio receiver?");
			AnsiConsole.WriteLine("On Windows this would be something like 'COM6' or 'COM8'");
			AnsiConsole.WriteLine("On Linux this would be something like '/dev/ttyACM0' or '/dev/ttyUSB0'");
			Console.WriteLine();
			string spID = AnsiConsole.Ask<string>("Serial Port > ");

			//Open
			AnsiConsole.Markup("Opening serial port " + spID + "... ");
			SerialPort sp = new SerialPort(spID, 115200);
			sp.DtrEnable = true; //must be set to true for the Pico to know to start transmiting data. If you use the serial module in Python, that module sets this automatically!
			try
			{
				sp.Open(); //Open the connection
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine("[red]Opening of serial port failed! Msg: " + ex.Message + "[/]");
				return; //exit program.
			}

			//Infinite loop of showing the data
			AnsiConsole.MarkupLine("[green]Port opened! Now displaying data below:[/]");
			while (true)
			{
				if (sp.BytesToRead > 0) //If there are bytes to read
				{
					//Read the data 
					byte[] buffer = new byte[sp.BytesToRead];
					sp.Read(buffer, 0, buffer.Length);

					//Convert to text
					string AsTxt = System.Text.Encoding.UTF8.GetString(buffer);

					//Split into lines
					string[] lines = AsTxt.Split(Environment.NewLine);

					//Handle each line separately
					foreach (string l in lines)
					{
						string line = l.Replace("\r", "").Replace("\n", ""); //Ensure new lines are stripped out at this point
						if (line != string.Empty)
						{
							if (line.StartsWith("{") && line.EndsWith("}")) //I developed the code on the Pico so where every actual data packet will be as JSON
							{
								AnsiConsole.MarkupLine("[blue][bold]" + line + "[/][/]");

								//Strip out data
								try
								{
									AnsiConsole.MarkupLine("\tParsing data...");
									JObject jo = JObject.Parse(line);
									int location = jo["location"].Value<int>();
									int temperature = jo["temperature"].Value<int>();
									int humidity = jo["humidity"].Value<int>();
									int aqi = jo["aqi"].Value<int>();
									int tvoc = jo["tvoc"].Value<int>();
									int eco2 = jo["eco2"].Value<int>();
									AnsiConsole.Markup("\tUploading data... ");
									UploadAirQualityReading(location, temperature, humidity, aqi, tvoc, eco2);
									AnsiConsole.MarkupLine("[green]Data upload successful![/]");
								}
								catch (Exception ex)
								{
									AnsiConsole.MarkupLine("\t[red]Unable to parse data! Err msg: " + ex.Message + "[/]");
								}

								AnsiConsole.MarkupLine("\tUploading data...");
							}
							else
							{
								AnsiConsole.MarkupLine("[gray][italic]" + line + "[/][/]");
							}
						}
					}
				}
				System.Threading.Tasks.Task.Delay(250).Wait();
			}

			//Should close the connection at the end but that is done automatically when Ctrl+C happenss
		}

		public static void UploadAirQualityReading(int location, int temperature, int humidity, int aqi, int tvoc, int eco2)
		{
			string query = "insert into AirQualityReading (Location, Temperature, Humidity, AQI, TVOC, ECO2) values (" + location.ToString() + ", " + temperature.ToString() + ", " + humidity.ToString() + ", " + aqi.ToString() + ", " + tvoc.ToString() + ", " + eco2.ToString() + ")";
			SqlConnection sqlcon = new SqlConnection(new Settings().SQLConnectionString);
			SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
			sqlcon.Open();
			sqlcmd.ExecuteNonQuery();
			sqlcon.Close();
		}
	}
}
