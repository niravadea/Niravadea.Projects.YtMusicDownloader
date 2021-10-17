using System;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class RuntimeConfiguration
    {
        public string FfmpegLocation { get; init; }
        public string YouTubeDownloadLocation { get; init; }
        public string OutputDirectory { get; init; }
        public Uri OriginalPlaylist { get; init; }
    }
}
