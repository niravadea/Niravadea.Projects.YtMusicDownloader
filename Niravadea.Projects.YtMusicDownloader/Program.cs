using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Niravadea.Projects.YtMusicDownloader
{
    class Program
    {
        static async Task Main(string[] args) =>
            await Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configure => {
                configure.AddCommandLine(
                    args: args,
                    switchMappings: new Dictionary<string, string> {
                        { "--ffmpeg-location","CmdLine:FfmpegLocation" },
                        { "--ytdl-location","CmdLine:YouTubeDownloadLocation" },
                        { "--output-directory", "CmdLine:OutputDirectory" },
                        { "--playlist","CmdLine:OriginalPlaylist" }
                    }
                );
            })
            .ConfigureServices((ctx, services) => {
                services.Configure<RuntimeConfiguration>(ctx.Configuration.GetSection("CmdLine"));
                services.AddHostedService<DownloadingService>();
                services.AddMediatR(Assembly.GetExecutingAssembly());
            })
            .Build()
            .RunAsync();
    }
}
