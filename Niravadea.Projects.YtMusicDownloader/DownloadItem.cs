using System;
using MediatR;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class DownloadItem : IRequest
    {
        public int TrackNumber { get; init; }
        public Uri DownloadLocation { get; set; }
    }
}
