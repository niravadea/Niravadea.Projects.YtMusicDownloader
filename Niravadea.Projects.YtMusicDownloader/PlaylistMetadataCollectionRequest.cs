using MediatR;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class PlaylistMetadataCollectionRequest : IRequest<PlaylistMetadata>
    {
        public RuntimeConfiguration RuntimeConfiguration { get; private init; }
        public PlaylistMetadataCollectionRequest(RuntimeConfiguration runtimeConfiguration)
        {
            RuntimeConfiguration = runtimeConfiguration;
        }
    }
}
