using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.IO;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class DownloadItemRequestHandler : IRequestHandler<DownloadItem>
    {
        private readonly ILogger<DownloadItemRequestHandler> _logger;
        private readonly RuntimeConfiguration _runtimeConfiguration;
        private const int MaximumRetries = 5;

        public DownloadItemRequestHandler(
            ILogger<DownloadItemRequestHandler> logger,
            IOptions<RuntimeConfiguration> runtimeConfiguration
        )
        {
            _logger = logger;
            _runtimeConfiguration = runtimeConfiguration.Value;
        }

        public async Task<Unit> Handle(DownloadItem request, CancellationToken cancellationToken)
        {
            int currentThreadId = Environment.CurrentManagedThreadId;
            _logger.LogInformation($"Beginning handler for track #{request.TrackNumber}.  Thread ID is {currentThreadId}");

            // yeah, this doesn't do padded zeros
            // TODO: count number of playlist entries and then pass that through, so we can calculate how many zeros to pad
            // also, we have to manually seed the track number because individual downloads have no playlist context
            string filePath = string.Format(@"%(album)s\%(artist)s\%(album)s\{0} - %(title)s.%(ext)s", request.TrackNumber);

            _logger.LogInformation($"q: {_runtimeConfiguration.OutputDirectory}");
            _logger.LogInformation($"w: {filePath}");

            string outputPath = Path.Combine(
                _runtimeConfiguration.OutputDirectory,
                filePath
            );

            string[] args = new string[]
            {
                "--format 140",
                $"--output \"{outputPath}\"",
                $"--ffmpeg-location \"{_runtimeConfiguration.FfmpegLocation}\"",
                request.DownloadLocation.AbsoluteUri
            };

            string finalArgumentsString = string.Join(' ', args);

            bool successfulDownload = false;
            int retries = 0;
            do
            {
                // TODO: make a generic process execution item
                Process downloadItemProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _runtimeConfiguration.YouTubeDownloadLocation,
                        Arguments = finalArgumentsString,

                        CreateNoWindow = true,
                        UseShellExecute = false,

                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                if (!downloadItemProcess.Start())
                {
                    throw new Exception("Unable to start download item process");
                }

                string stdOut = downloadItemProcess.StandardOutput.ReadToEnd();
                string stdErr = downloadItemProcess.StandardError.ReadToEnd();

                await downloadItemProcess.WaitForExitAsync();

                if (string.IsNullOrWhiteSpace(stdErr))
                {
                    successfulDownload = true;
                    _logger.LogInformation("Successfully downloaded item.");
                    _logger.LogTrace("STDOUT is: {stdOut}", stdOut);
                }
                else
                {
                    _logger.LogWarning($"Error while downloading item.  Error output was: {stdErr}");
                    _logger.LogTrace("STDERR is: {stdErr}", stdErr);
                    retries++;
                    if (retries > MaximumRetries)
                    {
                        // not sure how to gracefully handle this, so I guess just explode or something
                        throw new Exception("Exceeded maximum number of retries while attempting to download item");
                    }
                }
            }
            while (!successfulDownload);
            _logger.LogInformation($"Completed handler for track #{request.TrackNumber}.");
            return Unit.Value;
        }
    }
}
