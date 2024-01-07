using Xabe.FFmpeg;

namespace AudioConverter;

internal static class FFmpegStreamExtensions
{
    private static readonly HashSet<string> PreferredLanguages = ["dan", "eng"];

    public static string ToDisplayString(this IAudioStream stream)
    {
        return $"{LanguageMap.GetLanguageName(stream.Language)}, {stream.Bitrate / 1000} kb/s, {stream.SampleRate / 1000}.0 kHz, {stream.Channels} channels, {stream.Codec}";
    }

    public static bool IsDts(this IAudioStream stream)
    {
        return stream.Codec.Contains("dts", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsPreferredLanguage(this IStream stream)
    {
        var streamLanguage = stream switch
        {
            IAudioStream audioStream => audioStream.Language,
            ISubtitleStream subtitleStream => subtitleStream.Language,
            _ => null,
        };

        return streamLanguage != null && PreferredLanguages.Contains(streamLanguage);
    }

    public static string ToDisplayString(this ISubtitleStream stream)
    {
        return $"{LanguageMap.GetLanguageName(stream.Language)}, {stream.Title} ({stream.Codec})";
    }
}