using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.FileProviders;

namespace PhysicalFilesWatcherInContainer
{
    public class PhysicalFilesWatcherService
    {
        public static string DefaultDirectory = ".";
        public static string DefaultFilename = "Watch.Me";

        private readonly ILogger<PhysicalFilesWatcherService> _logger;
        private readonly string _directory;
        private readonly string _filename;

        public PhysicalFilesWatcherService(IConfiguration configuration, ILogger<PhysicalFilesWatcherService> logger)
        {
            _directory = configuration.GetValue<string>("dir") ?? DefaultDirectory;
            _filename = configuration.GetValue<string>("fn") ?? DefaultFilename;
            _logger = logger;

            if (File.Exists(Path.Combine(_directory, _filename)))
            {
                _logger.LogDebug("File found!");
            }
            else
            {
                _logger.LogWarning("File doesn't exist!");
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_directory, _filename);
            _logger.LogDebug("Started watching file: {filename}", fullPath);

            using var watcher = new PhysicalFileProvider(_directory);

            try
            {
                watcher.UsePollingFileWatcher = true;
                watcher.UseActivePolling = true;

                var fileChangeToken = watcher.Watch(_filename);
                fileChangeToken.RegisterChangeCallback(FileWatcherNotification, new FileInfo(fullPath));

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (fileChangeToken.HasChanged)
                    {
                        fileChangeToken = watcher.Watch(_filename);
                        fileChangeToken.RegisterChangeCallback(FileWatcherNotification, new FileInfo(fullPath));
                    }

                    await Task.Delay(10);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation cancelled.");
            }
            catch(Exception exception)
            {
                _logger.LogWarning(exception, "Exception occured while watching.");
            }
            finally
            {
                _logger.LogDebug("Stopped watching file: {filename}", Path.Combine(_directory, _filename));
            }
        }

        private void FileWatcherNotification(object state)
        {
            _logger.LogDebug("File changed: {state}", state);
        }
    }
}
