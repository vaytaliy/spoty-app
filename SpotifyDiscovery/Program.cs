using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using System.IO;

namespace SpotifyDiscovery
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var c = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.MongoDBBson
                (
                    databaseUrl: $"{c["Serilog:WriteTo:Args:databaseUrl"]}/db",
                    collectionName: $"{c["Serilog:WriteTo:Args:collectionName"]}",
                    period: TimeSpan.FromSeconds(int.Parse($"{c["Serilog:WriteTo:Args:bulkSavePeriod"]}")),
                    cappedMaxSizeMb: int.Parse($"{c["Serilog:WriteTo:Args:cappedMaxSizeMb"]}")
                )
                .WriteTo.Seq(c["Serilog:SeqUrl"])
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Application couldn't start. Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog();
    }
}
