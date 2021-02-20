using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace FileSystemWatcherTrigger
{
    public class FileSystemWatcherTrigger
    {
        public static string DefaultDirectory = ".";
        public static string DefaultFilename = "Watch.Me";

        private readonly ILogger<FileSystemWatcherTrigger> _logger;
        private readonly string _directory;
        private readonly string _filename;

        public FileSystemWatcherTrigger(IConfiguration configuration, ILogger<FileSystemWatcherTrigger> logger)
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

            using var checksumCalculator = SHA256.Create();
            byte[] lastCheckSum = null;
            byte[] currentCheckSum = null;

            using (var stream = File.OpenRead(fullPath))
            {
                lastCheckSum = checksumCalculator.ComputeHash(stream);
            }

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var stream = File.OpenRead(fullPath))
                    {
                        currentCheckSum = checksumCalculator.ComputeHash(stream);
                    }

                    if (!lastCheckSum.SequenceEqual(currentCheckSum))
                    {
                        File.SetLastWriteTime(fullPath, DateTime.UtcNow);
                        _logger.LogDebug("File last write time updated: {filename} - {newTime}", fullPath, DateTime.UtcNow);
                    }

                    lastCheckSum = currentCheckSum;

                    await Task.Delay(1000);
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Exception occured while watching.");
            }
        }
    }
}
