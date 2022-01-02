using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace catpi_v2_worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => 
                {
                    config
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile("appsettings.local.json", true, true)
                        .AddEnvironmentVariables();

                    var settings = config.Build();

                    var endpoint = settings["KeyVault:Endpoint"];
                    var clientId = settings["KeyVault:ClientId"];
                    var clientSecret = settings["KeyVault:ClientSecret"];

                    config.AddAzureKeyVault(endpoint, clientId, clientSecret, new DefaultKeyVaultSecretManager());
                })
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}