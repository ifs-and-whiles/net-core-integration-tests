using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreIntegrationTestsSample.Infrastructure;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    public class HostFixture : IDisposable
    {
        public IHost Host { get; set; }
        public int ApiHostPort { get; } = 5000;
        public string HostAddress { get; }
        public IConfiguration AppConfiguration { get; }

        public HostFixture()
        {
            HostAddress = $"http://localhost:{ApiHostPort}";

            AppConfiguration = LoadConfig("appsettings-integration-tests.json");

            var dbConfig = AppConfiguration.GetSection(ConfigKeys.Database).Get<DatabaseConfig>();
            DatabaseCreator.CreateIfNotExists(dbConfig).Wait();

            Host = ApiHost.CreateHost(AppConfiguration, ApiHostPort);

            Host.Start();
        }

        private IConfiguration LoadConfig(string appSettings)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettings, optional: false, false)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        public void Dispose()
        {
            Host?.StopAsync().Wait(millisecondsTimeout: 2000);
            Host?.Dispose();
        }
    }
}