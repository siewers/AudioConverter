using Xabe.FFmpeg;

namespace AudioConverter;

internal static class AudioStreamExtensions
{
    public static string ToDisplayString(this IAudioStream stream)
    {
        return $"{stream.Language}, {stream.Bitrate} kb/s, {stream.SampleRate}.0 kHz, {stream.Channels} channels, {stream.Codec}";
    }

    public static bool IsDts(this IAudioStream stream)
    {
        return stream.Codec.Contains(AudioCodec.dts.ToString(), StringComparison.InvariantCultureIgnoreCase);
    }
}