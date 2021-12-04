using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;

namespace SpotifyDiscovery.Utilities
{
    public class LoggerBuilder
    {

        private readonly ILogger log;

        public LoggerBuilder(IConfiguration configuration)
        {

            var c = configuration;
            log = new LoggerConfiguration().WriteTo.MongoDBBson(
                databaseUrl: $"{c["Serilog:WriteTo:databaseUrl"]}",
                collectionName: $"{c["Serilog:WriteTo:Args:collectionName"]}",
                period: TimeSpan.FromSeconds(int.Parse($"{c["Serilog:WriteTo:Args:bulkSavePeriod"]}")),
                cappedMaxSizeMb: int.Parse($"{c["Serilog:WriteTo:Args:cappedMaxSizeMb"]}"))
            .CreateLogger();
        }

        public ILogger GetLogger(string error, string stackTrace)
        {
            return log;
        }
    }
}
