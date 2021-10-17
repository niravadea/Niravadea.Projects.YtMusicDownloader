using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Text.Json;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class PlaylistMetadataCollectionRequestHandler : IRequestHandler<PlaylistMetadataCollectionRequest, PlaylistMetadata>
    {
        public async Task<PlaylistMetadata> Handle(PlaylistMetadataCollectionRequest request, CancellationToken cancellationToken)
        {
            Process metadataCollectionProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = request.RuntimeConfiguration.YouTubeDownloadLocation,
                    Arguments = $"--dump-single-json {request.RuntimeConfiguration.OriginalPlaylist.AbsoluteUri}",

                    CreateNoWindow = true,
                    UseShellExecute = false,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            if (!metadataCollectionProcess.Start())
            {
                throw new Exception("Unable to start metadata collection process");
            }

            // magic is happening

            string stdOut = metadataCollectionProcess.StandardOutput.ReadToEnd();
            string stdErr = metadataCollectionProcess.StandardError.ReadToEnd();

            await metadataCollectionProcess.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(stdErr))
            {
                throw new InvalidOperationException($"An error occurred while retrieving the playlist metadata: {stdErr}");
            }

            PlaylistMetadata metadata = JsonSerializer.Deserialize<PlaylistMetadata>(
                json: stdOut,
                options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            return metadata;
        }
    }
}
