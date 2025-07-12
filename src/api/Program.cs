using System;

namespace AirQualityMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Set up builder
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://0.0.0.0:4560");
            builder.Services.AddControllers();

            //Run the app
            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}