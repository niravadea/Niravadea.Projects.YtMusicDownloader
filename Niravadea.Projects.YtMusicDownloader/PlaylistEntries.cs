using System;
using System.Text.Json.Serialization;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class PlaylistEntries
    {
        public string Title { get; set; }
        public string Track { get; set; }
        public string Artist { get; set; }
        [JsonPropertyName("webpage_url")]
        public Uri Url { get; set; }
    }
}
