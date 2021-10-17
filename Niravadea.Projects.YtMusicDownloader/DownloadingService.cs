using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class DownloadingService : IHostedService
    {
        private readonly ILogger<DownloadingService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly RuntimeConfiguration _configuration;
        private readonly IMediator _mediator;

        public DownloadingService(
            ILogger<DownloadingService> logger,
            IHostApplicationLifetime lifetime,
            IOptions<RuntimeConfiguration> configuration,
            IMediator mediator
        )
        {
            _logger = logger;
            _lifetime = lifetime;
            _configuration = configuration.Value;
            _mediator = mediator;
        }

        async void MainBody()
        {
            // make initial youtube-dl call
            _logger.LogInformation("Downloading playlist metadata.");
            PlaylistMetadata metadata = await _mediator.Send(new PlaylistMetadataCollectionRequest(runtimeConfiguration: _configuration));
            _logger.LogInformation("Successfully downloaded playlist metadata.");


            // now perform the actual retrievals in a parallel fashion
            var entries = metadata.PlaylistEntries
                                  .Select(selector: (x, i) => new DownloadItem
                                  {
                                      TrackNumber = i + 1,
                                      DownloadLocation = x.Url
                                  })
                                  .AsParallel()
                                  .Select(x => _mediator.Send(x));

            _logger.LogInformation("Beginning worker tasks");
            Task.WaitAll(tasks: entries.ToArray());
            _logger.LogInformation("Completed worker tasks");

            await Task.Delay(1000);

            _lifetime.StopApplication();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(MainBody);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
