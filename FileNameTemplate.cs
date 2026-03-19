using System.Globalization;
using Serilog;

namespace YoutubeSegmentDownloader;

internal static class FileNameTemplate
{
    public const string DefaultTemplate = "[video creator]-[upload date]-[video title]-[video ID]-[video segment]";

    public static readonly string[] AvailableTags =
    [
        "[free text]",
        "[upload date]",
        "[video creator]",
        "[video ID]",
        "[video title]",
        "[video segment]",
        "[video resolution]",
        "[full video duration]",
        "[video segment duration]",
        "[download date]"
    ];

    public static string BuildFileName(
        string template,
        string? title,
        DateTime? uploadDate,
        string? videoId,
        string? uploader,
        float start,
        float end,
        string? resolution,
        long? fullDurationSeconds,
        string? freeText)
    {
        // Sanitize components
        title = SanitizeComponent(title ?? "", 80);
        uploader = SanitizeComponent(uploader ?? "Unknown", 50);
        videoId ??= "unknown";
        string uploadDateStr = (uploadDate ?? DateTime.Now).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        string downloadDateStr = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        string result = template;

        // Replace tags
        result = result.Replace("[video creator]", uploader);
        result = result.Replace("[upload date]", uploadDateStr);
        result = result.Replace("[video title]", title);
        result = result.Replace("[video ID]", videoId);
        result = result.Replace("[download date]", downloadDateStr);
        result = result.Replace("[video resolution]", resolution ?? "");
        result = result.Replace("[full video duration]", fullDurationSeconds?.ToString() ?? "");

        // Segment tag: only include if it's a segment (end > 0), otherwise remove tag and surrounding dash
        bool isSegment = end > 0;
        if (isSegment)
        {
            string segmentStr = $"{start}_{end}";
            result = result.Replace("[video segment]", segmentStr);

            float segmentDuration = end - start;
            result = result.Replace("[video segment duration]", segmentDuration.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            // Remove the tag and any adjacent dash
            result = result.Replace("-[video segment]", "");
            result = result.Replace("[video segment]-", "");
            result = result.Replace("[video segment]", "");

            result = result.Replace("-[video segment duration]", "");
            result = result.Replace("[video segment duration]-", "");
            result = result.Replace("[video segment duration]", "");
        }

        // Free text: if empty, remove tag and adjacent dash
        if (!string.IsNullOrWhiteSpace(freeText))
        {
            result = result.Replace("[free text]", SanitizeComponent(freeText, 50));
        }
        else
        {
            result = result.Replace("-[free text]", "");
            result = result.Replace("[free text]-", "");
            result = result.Replace("[free text]", "");
        }

        // Clean up any double dashes left from removed tags
        while (result.Contains("--"))
            result = result.Replace("--", "-");

        result = result.Trim('-');

        if (string.IsNullOrWhiteSpace(result))
            result = $"{uploadDateStr}-{videoId}";

        Log.Debug("Built filename from template: {fileName}", result);
        return result + ".mp4";
    }

    private static string SanitizeComponent(string value, int maxLength)
    {
        value = string.Join(string.Empty, value.Split(Path.GetInvalidFileNameChars()))
                      .Replace(".", string.Empty);

        if (value.Length > maxLength)
        {
            Log.Warning("Component '{value}' is too long, truncating to {maxLength} characters.", value, maxLength);
            value = value[..maxLength];
        }

        return value;
    }
}
