using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace FileWatcherInContainer
{
    public class FileWatcherService
    {
        public static string DefaultDirectory = ".";
        public static string DefaultFilename = "Watch.Me";

        private readonly ILogger<FileWatcherService> _logger;
        private readonly string _directory;
        private readonly string _filename;

        public FileWatcherService(IConfiguration configuration, ILogger<FileWatcherService> logger)
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
            _logger.LogDebug("Started watching file: {filename}", Path.Combine(_directory, _filename));

            using var watcher = new FileSystemWatcher(_directory, _filename);

            try
            {
                watcher.Changed += Watcher_Changed;
                watcher.Created += Watcher_Changed;
                watcher.Deleted += Watcher_Changed;
                watcher.Renamed += Watcher_Changed;
                watcher.Error += Watcher_Error;

                watcher.EnableRaisingEvents = true;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(10);
                }
            }
            catch(Exception exception)
            {
                _logger.LogWarning(exception, "Exception occured while watching.");
            }
            finally
            {
                watcher.EnableRaisingEvents = false;

                watcher.Changed -= Watcher_Changed;
                watcher.Created -= Watcher_Changed;
                watcher.Deleted -= Watcher_Changed;
                watcher.Renamed -= Watcher_Changed;
                watcher.Error -= Watcher_Error;
                _logger.LogDebug("Stopped watching file: {filename}", Path.Combine(_directory, _filename));
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "Error occured while watching {filePath}", Path.Combine(_directory, _filename));
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            _logger.LogDebug("{filePath} has been changed: {changeType}", e.FullPath, e.ChangeType);
        }
    }
}
