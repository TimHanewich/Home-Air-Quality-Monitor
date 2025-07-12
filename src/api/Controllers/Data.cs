using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimHanewich.Sql;
using Microsoft.Data.SqlClient;

namespace AirQualityMonitor
{
    [ApiController]
    [Route("data")]
    public class Data : ControllerBase
    {

        public async Task Get()
        {
            //Get SQL connection string
            string ConStr = "";
            try
            {
                Console.Write("Getting SQL Connection String... ");
                ConStr = Tools.GetSqlConnectionString();
                Console.WriteLine("Got it!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get SQL Connection string. Msg: " + ex.Message);
                Response.StatusCode = 500;
                await Response.WriteAsync(ex.Message);
                return;
            }


            //Determine EST time - this will be used in several places later
            TimeZoneInfo EST = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime NowEstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EST);

            //Create SQL connection
            SqlConnection sqlcon = new SqlConnection(ConStr);
            sqlcon.Open();

            //First, run query to get distinct locations within the past X minutes
            Console.Write("Checking locations that have reported data recently... ");
            List<int> UniqueLocations = new List<int>();
            try
            {
                
                string query = "select distinct Location from AirQualityReading where Captured > '" + NowEstTime.AddMinutes(-60).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                SqlDataReader sdr = sqlcmd.ExecuteReader();
                while (sdr.Read())
                {
                    byte location = sdr.GetByte(0);
                    UniqueLocations.Add(Convert.ToInt32(location));
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                sqlcon.Close();
                Console.WriteLine("Failed to check unique locations. Msg: " + ex.Message);
                Response.StatusCode = 500;
                await Response.WriteAsync(ex.Message);
                return;
            }
            Console.WriteLine("Sensors reporting data: " + JsonConvert.SerializeObject(UniqueLocations));

            //Now, for each of those, get the most recent data
            JArray ToReturn = new JArray();
            foreach (int location in UniqueLocations)
            {
                Console.Write("Getting most recent sensor data from location " + location.ToString() + "... ");
                try
                {
                    //Query, strip out data
                    string query = "select top 1 Captured, Temperature, Humidity, AQI, TVOC, ECO2 from AirQualityReading where Location = " + location.ToString() + " order by Captured desc";
                    SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                    SqlDataReader sdr = sqlcmd.ExecuteReader();
                    DateTime? Captured = null;
                    int? Temperature = null;
                    int? Humidity = null;
                    int? AQI = null;
                    int? TVOC = null;
                    int? ECO2 = null;
                    while (sdr.Read())
                    {
                        if (!sdr.IsDBNull(0))
                        {
                            Captured = sdr.GetDateTime(0);
                        }
                        if (!sdr.IsDBNull(1))
                        {
                            Temperature = Convert.ToInt32(sdr.GetByte(1));
                        }
                        if (!sdr.IsDBNull(2))
                        {
                            Humidity = Convert.ToInt32(sdr.GetByte(2));
                        }
                        if (!sdr.IsDBNull(3))
                        {
                            AQI = Convert.ToInt32(sdr.GetByte(3));
                        }
                        if (!sdr.IsDBNull(4))
                        {
                            TVOC = Convert.ToInt32(sdr.GetInt16(4));
                        }
                        if (!sdr.IsDBNull(5))
                        {
                            ECO2 = Convert.ToInt32(sdr.GetInt16(5));
                        }
                    }
                    sdr.Close();

                    //Put into object to return
                    JObject ThisRecord = new JObject();
                    ThisRecord.Add("location", location);
                    if (Captured.HasValue)
                    {
                        double SecondsAgo = (NowEstTime - Captured.Value).TotalSeconds;
                        ThisRecord.Add("secondsAgo", Convert.ToInt32(SecondsAgo));
                    }
                    if (Temperature.HasValue)
                    {
                        ThisRecord.Add("temperature", Temperature.Value);
                    }
                    if (Humidity.HasValue)
                    {
                        ThisRecord.Add("humidity", Humidity.Value);
                    }
                    if (AQI.HasValue)
                    {
                        ThisRecord.Add("aqi", AQI.Value);
                    }
                    if (TVOC.HasValue)
                    {
                        ThisRecord.Add("tvoc", TVOC.Value);
                    }
                    if (ECO2.HasValue)
                    {
                        ThisRecord.Add("eco2", ECO2.Value);
                    }
                    ToReturn.Add(ThisRecord);

                    //Print success msg
                    Console.WriteLine("Success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to collect most recent data for location " + location.ToString() + ": " + ex.Message);
                }
            }

            //Return!
            sqlcon.Close();
            Console.WriteLine("Now returning!");
            Response.StatusCode = 200;
            Response.Headers.Append("Content-Type", "application/json");
            await Response.WriteAsync(ToReturn.ToString());
        }

    }
}