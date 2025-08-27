using LightlessSyncServices.Discord;
using LightlessSyncShared.Data;
using LightlessSyncShared.Metrics;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using LightlessSyncShared.Utils;
using LightlessSyncShared.Services;
using StackExchange.Redis;
using LightlessSyncShared.Utils.Configuration;

namespace LightlessSyncServices;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var config = app.ApplicationServices.GetRequiredService<IConfigurationService<LightlessConfigurationBase>>();

        var metricServer = new KestrelMetricServer(config.GetValueOrDefault<int>(nameof(LightlessConfigurationBase.MetricsPort), 4982));
        metricServer.Start();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var lightlessConfig = Configuration.GetSection("LightlessSync");

        services.AddDbContextPool<LightlessDbContext>(options =>
        {
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), builder =>
            {
                builder.MigrationsHistoryTable("_efmigrationshistory", "public");
            }).UseSnakeCaseNamingConvention();
            options.EnableThreadSafetyChecks(false);
        }, Configuration.GetValue(nameof(LightlessConfigurationBase.DbContextPoolSize), 1024));
        services.AddDbContextFactory<LightlessDbContext>(options =>
        {
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), builder =>
            {
                builder.MigrationsHistoryTable("_efmigrationshistory", "public");
                builder.MigrationsAssembly("LightlessSyncShared");
            }).UseSnakeCaseNamingConvention();
            options.EnableThreadSafetyChecks(false);
        });

        services.AddSingleton(m => new LightlessMetrics(m.GetService<ILogger<LightlessMetrics>>(), new List<string> { },
        new List<string> { }));

        var redis = lightlessConfig.GetValue(nameof(ServerConfiguration.RedisConnectionString), string.Empty);
        var options = ConfigurationOptions.Parse(redis);
        options.ClientName = "Lightless";
        options.ChannelPrefix = "UserData";
        ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        services.Configure<ServicesConfiguration>(Configuration.GetRequiredSection("LightlessSync"));
        services.Configure<ServerConfiguration>(Configuration.GetRequiredSection("LightlessSync"));
        services.Configure<LightlessConfigurationBase>(Configuration.GetRequiredSection("LightlessSync"));
        services.AddSingleton(Configuration);
        services.AddSingleton<ServerTokenGenerator>();
        services.AddSingleton<DiscordBotServices>();
        services.AddHostedService<DiscordBot>();
        services.AddSingleton<IConfigurationService<ServicesConfiguration>, LightlessConfigurationServiceServer<ServicesConfiguration>>();
        services.AddSingleton<IConfigurationService<ServerConfiguration>, LightlessConfigurationServiceClient<ServerConfiguration>>();
        services.AddSingleton<IConfigurationService<LightlessConfigurationBase>, LightlessConfigurationServiceClient<LightlessConfigurationBase>>();

        services.AddHostedService(p => (LightlessConfigurationServiceClient<LightlessConfigurationBase>)p.GetService<IConfigurationService<LightlessConfigurationBase>>());
        services.AddHostedService(p => (LightlessConfigurationServiceClient<ServerConfiguration>)p.GetService<IConfigurationService<ServerConfiguration>>());
    }
}