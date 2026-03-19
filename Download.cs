using System.Globalization;
using System.Text.RegularExpressions;
using Serilog;
using Xabe.FFmpeg;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeSegmentDownloader.Extension;
using YtdlpVideoData = YoutubeSegmentDownloader.Models.YtdlpVideoData.ytdlpVideoData;

namespace YoutubeSegmentDownloader;

internal partial class Download(string id,
                                float start,
                                float end,
                                DirectoryInfo outputDirectory,
                                string format,
                                string browser,
                                string namingTemplate,
                                string freeText)
{
    public bool Finished;
    public string? OutputFilePath;
    public bool Succeeded;

    private string Link =>
        id.Contains('/')
            ? id
            : @$"https://youtu.be/{id}";

    public async Task StartAsync(CancellationToken? cancellationToken = default)
    {
        cancellationToken ??= CancellationToken.None;
        Log.Information("Start the download process...");
        Succeeded = false;
        Finished = false;

        string tempFilePath1 = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
        string tempFilePath2 = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");
        Log.Debug("Create temporary files:");
        Log.Debug("{TempFilePath}", tempFilePath1);
        Log.Debug("{TempFilePath}", tempFilePath2);

        try
        {
            OptionSet optionSet = CreateOptionSet();

            YoutubeDL ytdl = new()
            {
                YoutubeDLPath = Path.Combine(ExternalProgram.YtdlpPath ?? "./", "yt-dlp.exe"),
                FFmpegPath = Path.Combine(ExternalProgram.FFmpegPath ?? "./", "ffmpeg.exe"),
                //OutputFolder = outputDirectory.FullName,
                OutputFileTemplate = tempFilePath1,
                OverwriteFiles = true,
                IgnoreDownloadErrors = true
            };

            YtdlpVideoData? videoData = await FetchVideoInfoAsync(ytdl, optionSet, cancellationToken);
            if (null == videoData) return;

            OutputFilePath = CalculatePath(videoData);

            bool downloadSuccess = await DownloadVideoAsync(ytdl, optionSet, cancellationToken);
            if (!downloadSuccess) return;

            if (end == 0)
            {
                _ = await EmbedMetadataAsync(tempFilePath1, tempFilePath2, videoData, cancellationToken);
                File.Move(tempFilePath2, OutputFilePath, true);
            }
            else
            {
                _ = await CutWithFFmpegAsync(tempFilePath1, tempFilePath2, videoData, cancellationToken);
                File.Move(tempFilePath2, OutputFilePath, true);
            }

            Log.Information("Download completed:");
            Log.Information(OutputFilePath);
            Succeeded = true;
        }
        catch (Exception e)
        {
            if (e is TaskCanceledException or OperationCanceledException)
                throw;

            Log.Error("vvvvvvvvvvvvvvvvvvvvv");
            Log.Error(e.Message);
            Log.Error("^^^^^^^^^^^^^^^^^^^^^");
        }
        finally
        {
            // Wait 500 ms to ensure the file is released
            await Task.Delay(500);

            File.Delete(tempFilePath1);
            File.Delete(tempFilePath2);
            File.Delete(Path.ChangeExtension(tempFilePath1, "tmp"));
            File.Delete(Path.ChangeExtension(tempFilePath2, "tmp"));
            Log.Information("Clean up temporary files.");
            Log.Information("Process ends.");
            Finished = true;
        }
    }

    private OptionSet CreateOptionSet()
    {
        OptionSet optionSet = new()
        {
            NoCheckCertificates = true,
            ExtractorArgs = "youtube:skip=dash",
            Color = "never"
        };

        if (!string.IsNullOrEmpty(format))
        {
            optionSet.Format = format;
        }
        else
        {
            // Workaround for FFmpeg sometimes uses 251 as bestvideo
            optionSet.AddCustomOption("-S", "res");
        }

        if (!string.IsNullOrEmpty(browser))
        {
            optionSet.AddCustomOption("--cookies-from-browser", browser);
        }

        if (end != 0)
        {
            optionSet.Downloader = "ffmpeg";
            optionSet.DownloaderArgs = $"ffmpeg_i:-ss {start} -to {end}";
        }

        return optionSet;
    }

    /// <summary>
    ///     取得影片資訊
    /// </summary>
    /// <param name="ytdl"></param>
    /// <param name="optionSet"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<YtdlpVideoData?> FetchVideoInfoAsync(YoutubeDL ytdl, OptionSet optionSet, CancellationToken? cancellationToken = default)
    {
        Log.Information("Start getting video information...");
        RunResult<YtdlpVideoData> result =
            await ytdl.RunVideoDataFetch_Alt(Link, overrideOptions: optionSet, ct: cancellationToken ?? CancellationToken.None);

        if (!result.Success)
        {
            Log.Error("vvvvvvvvvvvvvvvvvvvvv");
            Log.Error(string.Join("\n", result.ErrorOutput));
            Log.Error("^^^^^^^^^^^^^^^^^^^^^");

            Log.Error("Failed to get video information! VideoId: {id}", id);
            Log.Error("Please ensure that your network connection is stable and that you have the necessary permissions to access the video.");
            Log.Error("Additionally, if you are using the 'Cookies from browser' feature, please close your browser.");
            return null;
        }

        float duration = result.Data.Duration ?? 0;

        Log.Information("{title}", result.Data.Title);
        Log.Information("{duration}", duration);

        if (result.Data.Duration < start)
        {
            Log.Error("Segment input invalid!");
            Log.Error("Start, End time should be smaller then video duration.");
            return null;
        }

        return result.Data;
    }

    /// <summary>
    ///     下載影片
    /// </summary>
    /// <param name="ytdl"></param>
    /// <param name="optionSet"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> DownloadVideoAsync(YoutubeDL ytdl, OptionSet optionSet, CancellationToken? cancellationToken = default)
    {
        Log.Information("Start downloading video...");
        var lastProgress = 0.0f;

        RunResult<string>? result =
            await ytdl.RunVideoDownload(Link,
                                        mergeFormat: DownloadMergeFormat.Mp4,
                                        progress: new Progress<DownloadProgress>(s => Log.Verbose(s.Data)),
                                        output: new Progress<string>(rawProgress =>
                                        {
                                            Match m = DownloadPercentage().Match(rawProgress);
                                            if (!m.Success)
                                            {
                                                Log.Verbose(rawProgress);
                                                return;
                                            }

                                            var currentProgress = float.Parse(m.Groups[1].Value);

                                            if (isProgressEqualOrMinorChange(currentProgress, lastProgress))
                                                return;

                                            lastProgress = currentProgress;
                                            Log.Verbose(rawProgress);
                                        }),
                                        overrideOptions: optionSet,
                                        ct: cancellationToken ?? CancellationToken.None);

        if (!result.Success)
        {
            Log.Error("Failed to download video! Please try again later.");
            foreach (string? str in result.ErrorOutput)
            {
                Log.Information(str);
            }

            return false;
        }

        Log.Information("Video downloaded.");
        return true;

        static bool isProgressEqualOrMinorChange(float currentProgress, float lastProgress)
            => Math.Abs(currentProgress - lastProgress) < float.Epsilon
               || (currentProgress < lastProgress && lastProgress - currentProgress < 1);
    }

    /// <summary>
    ///     剪切影片
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IConversionResult> CutWithFFmpegAsync(string inputPath, string outputPath, YtdlpVideoData videoData, CancellationToken? cancellationToken = default)
    {
        Log.Information("Start cutting video with FFmpeg...");

        float duration = end - start;

        FFmpeg.SetExecutablesPath(ExternalProgram.FFmpegPath);
        IMediaInfo? mediaInfo = await FFmpeg.GetMediaInfo(inputPath);
        // How to Encode Videos for YouTube, Facebook, Vimeo, twitch, and other Video Sharing Sites
        // https://trac.ffmpeg.org/wiki/Encode/YouTube
        IConversion? conversion = FFmpeg.Conversions.New()
                                        .AddParameter($"-sseof -{duration}", ParameterPosition.PreInput)
                                        .AddStream(mediaInfo.Streams)
                                        .AddParameter("-c:v libx264 -preset slow -crf 18 -c:a aac -b:a 192k -pix_fmt yuv420p")
                                        .AddParameter(BuildMetadataParameters(videoData))
                                        .AddParameter("-movflags +faststart")
                                        .SetOutput(outputPath)
                                        .SetOverwriteOutput(true);

        conversion.OnDataReceived += (_, e) =>
        {
            if (e.Data != null) Log.Verbose(e.Data);
        };

        Log.Debug("FFmpeg arguments: {arguments}", conversion.Build());
        return await conversion.Start(cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    ///     Embed metadata without re-encoding (for full video downloads)
    /// </summary>
    private async Task<IConversionResult> EmbedMetadataAsync(string inputPath, string outputPath, YtdlpVideoData videoData, CancellationToken? cancellationToken = default)
    {
        Log.Information("Embedding metadata with FFmpeg...");

        FFmpeg.SetExecutablesPath(ExternalProgram.FFmpegPath);
        IMediaInfo? mediaInfo = await FFmpeg.GetMediaInfo(inputPath);

        IConversion? conversion = FFmpeg.Conversions.New()
                                        .AddStream(mediaInfo.Streams)
                                        .AddParameter("-c copy")
                                        .AddParameter(BuildMetadataParameters(videoData))
                                        .AddParameter("-movflags +faststart")
                                        .SetOutput(outputPath)
                                        .SetOverwriteOutput(true);

        conversion.OnDataReceived += (_, e) =>
        {
            if (e.Data != null) Log.Verbose(e.Data);
        };

        Log.Debug("FFmpeg arguments: {arguments}", conversion.Build());
        return await conversion.Start(cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    ///     Build FFmpeg metadata parameters from YouTube video data.
    ///     Maps: video URL → purl (Author URL), channel URL → episode_id (Promotion URL),
    ///     upload date → creation_time (Media created), title, tags, and comment.
    /// </summary>
    private static string BuildMetadataParameters(YtdlpVideoData videoData)
    {
        var parts = new List<string>();

        // Title
        if (!string.IsNullOrEmpty(videoData.Title))
            parts.Add($"-metadata title={EscapeMetadataValue(videoData.Title)}");

        // Artist (channel name)
        string? creator = videoData.Uploader ?? videoData.Channel;
        if (!string.IsNullOrEmpty(creator))
            parts.Add($"-metadata artist={EscapeMetadataValue(creator)}");

        // Media created (ISO 8601 format for creation_time)
        if (!string.IsNullOrEmpty(videoData.UploadDate)
            && DateTime.TryParseExact(videoData.UploadDate, "yyyyMMdd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime uploadDate))
        {
            parts.Add($"-metadata creation_time={uploadDate:yyyy-MM-ddTHH:mm:ssZ}");
            parts.Add($"-metadata date={uploadDate:yyyy-MM-dd}");
        }

        // Author URL → YouTube video URL
        if (!string.IsNullOrEmpty(videoData.WebpageUrl))
            parts.Add($"-metadata purl={EscapeMetadataValue(videoData.WebpageUrl)}");

        // Comment: include video URL, channel URL, and description excerpt
        var commentLines = new List<string>();
        if (!string.IsNullOrEmpty(videoData.WebpageUrl))
            commentLines.Add($"Source: {videoData.WebpageUrl}");
        string? channelUrl = videoData.ChannelUrl ?? videoData.UploaderUrl;
        if (!string.IsNullOrEmpty(channelUrl))
            commentLines.Add($"Channel: {channelUrl}");
        if (!string.IsNullOrEmpty(videoData.Description))
        {
            string descExcerpt = videoData.Description.Length > 200
                ? videoData.Description[..200] + "..."
                : videoData.Description;
            commentLines.Add(descExcerpt);
        }
        if (commentLines.Count > 0)
            parts.Add($"-metadata comment={EscapeMetadataValue(string.Join("\n", commentLines))}");

        // Tags/keywords
        if (videoData.Tags is { Count: > 0 })
        {
            string tagsStr = string.Join(", ", videoData.Tags);
            parts.Add($"-metadata keywords={EscapeMetadataValue(tagsStr)}");
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    ///     Escape a metadata value for FFmpeg's -metadata parameter.
    ///     FFmpeg metadata values with special characters need quoting.
    /// </summary>
    private static string EscapeMetadataValue(string value)
    {
        // Replace backslashes, then quotes
        string escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    private string CalculatePath(YtdlpVideoData videoData)
    {
        DateTime? uploadDate = DateTime.TryParseExact(videoData.UploadDate ?? "19700101",
                                                       "yyyyMMdd",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out DateTime parsed)
                                   ? parsed
                                   : DateTime.Now;

        string? resolution = videoData.Height != null ? $"{videoData.Height}p" : videoData.Resolution;

        string fileName = FileNameTemplate.BuildFileName(
            template: namingTemplate,
            title: videoData.Title,
            uploadDate: uploadDate,
            videoId: videoData.Id ?? id,
            uploader: videoData.Uploader ?? videoData.Channel,
            start: start,
            end: end,
            resolution: resolution,
            fullDurationSeconds: videoData.Duration,
            freeText: freeText);

        string newPath = Path.Combine(outputDirectory.FullName, fileName);
        Log.Debug("Calculate output file path as {newPath}", newPath);
        return newPath;
    }

    [GeneratedRegex(@"^\[download\]\s+(\d+\.\d+)%")]
    private static partial Regex DownloadPercentage();
}