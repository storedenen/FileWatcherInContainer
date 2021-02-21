using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhysicalFilesWatcherInContainer
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static CancellationToken _cancellationToken;
        private static ILogger<Program> _logger;

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, config);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                var watcherService = serviceProvider.GetService<PhysicalFilesWatcherService>();
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                var watcherServiceTask = watcherService.Run(_cancellationToken);

                while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
                {
                    await Task.Delay(100);
                }

                _cancellationTokenSource.Cancel();

                await watcherServiceTask;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An exception occured during running!");
            }

            _logger.LogDebug("Application exiting.");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfiguration>(_ => configuration);
            services
                .AddLogging(configure => configure.AddConsole().AddConfiguration(configuration.GetSection("Logging")))
                .AddSingleton<PhysicalFilesWatcherService>();
        }
    }
}
