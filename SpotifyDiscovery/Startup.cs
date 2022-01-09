using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SpotifyDiscovery.Data;
using SpotifyDiscovery.Filters;
using SpotifyDiscovery.Hubs;
using SpotifyDiscovery.Models;
using SpotifyDiscovery.Realtime;
using SpotifyDiscovery.Services;
using SpotifyDiscovery.Utilities;
using System.Net.Http;
using Serilog;
using Serilog.Events;
using System;
using Microsoft.AspNetCore.SignalR;

namespace SpotifyDiscovery
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddInMemoryRateLimiting();

            services.Configure<SpotifyDiscoveryDatabaseSettings>(
                    Configuration.GetSection(nameof(SpotifyDiscoveryDatabaseSettings))
            );
            services.AddCors(options =>
            {
                options.AddPolicy("cors_policy", builder =>
                {
                    builder.WithOrigins(Configuration["Hosting:BaseURL"]);
                });
            });
            services.AddSignalR().AddStackExchangeRedis(Configuration["SpotifyDiscoveryDatabaseSettings:InMemoryDatabaseConnection"], options => {
                options.Configuration.ChannelPrefix = "MyApp";
            });
            services.AddStackExchangeRedisCache(opts => {
                opts.Configuration = Configuration["SpotifyDiscoveryDatabaseSettings:InMemoryDatabaseConnection"];
                opts.InstanceName = "CacheInstance";
                }
            );
            
            services.AddSingleton<ISpotifyDiscoveryDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<SpotifyDiscoveryDatabaseSettings>>().Value);

            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddScoped<IInMemoryDb, RedisInMemoryDb>();

            services.AddScoped<Db>();
            services.AddScoped<ISpotiRepository, DbSpotiRepository>();
            services.AddAutoMapper(typeof(Startup));

            services.AddScoped<HttpClient>();
            services.AddScoped<SongTrackerService>();
            services.AddScoped<AccountService>();
            services.AddScoped<SharedPlayerService>();
            services.AddScoped<IRoomManager, RoomManager>();

            services.AddScoped<SpotifyAuthFilter>();
            services.AddAuthentication();
            services.AddControllers();
            services.AddLogging();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseIpRateLimiting();

            if (env.IsDevelopment() && Environment.GetEnvironmentVariable("LAUNCH_DEV_NODE_SERVER") == null)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SharedPlayerHub>("/playerHub");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
