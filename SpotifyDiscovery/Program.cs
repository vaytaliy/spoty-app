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
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

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
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", true)
                .AddEnvironmentVariables()
                .Build();


            Log.Logger = new LoggerConfiguration()
                
                .WriteTo.MongoDBBson(
                    databaseUrl: c["Serilog:WriteTo:Args:databaseUrl"] + "/logDb?authSource=admin",
                    collectionName: "log",
                    period: TimeSpan.FromSeconds(5)
                 )
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.Seq("http://localhost:5341")
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
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        //var configOne = config.AddEnvironmentVariables();
                        var builtConfig = config.Build();

                        foreach (var source in config.Sources)
                        {
                            Log.Logger.Information(source.ToString());
                        }
                        
                        var keyVaultUrl = $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/";
                        var clientId = builtConfig["ClientId"];
                        var clientSecret = builtConfig["ClientSecret"];

                        config.AddAzureKeyVault(vault: $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/", clientId, clientSecret);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog();
    }
}
