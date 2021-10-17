using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Niravadea.Projects.YtMusicDownloader
{
    public class PlaylistMetadata
    {
        public string Title { get; set; }
        [JsonPropertyName("entries")]
        public IEnumerable<PlaylistEntries> PlaylistEntries { get; set; }
    }
}
