using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace AudioConverter;

internal sealed class Converter
{
    private readonly IMediaInfo _mediaInfo;

    public Converter(IMediaInfo mediaInfo)
    {
        _mediaInfo = mediaInfo;
    }

    public async Task Convert()
    {
        var audioStreams = _mediaInfo.AudioStreams.ToArray();

        List<IAudioStream> selectedAudioStreams = [];
        if (audioStreams.Length > 0)
        {
            var audioStreamsPrompt = new MultiSelectionPrompt<IAudioStream>()
                    .Title("Select audio streams to convert")
                    .UseConverter(stream => $"({stream.Index}) {stream.GetDisplayString()}");
            foreach (var audioStream in audioStreams)
            {
                var choice = audioStreamsPrompt.AddChoice(audioStream);
                if (audioStream.IsDts())
                {
                    choice.Select();
                }
            }

            AnsiConsole.Prompt(audioStreamsPrompt);
        }

        foreach (var audioStream in selectedAudioStreams)
        {
            if (audioStream.IsDts())
            {
                AnsiConsole.MarkupLineInterpolated($"Audio stream {audioStream.GetDisplayString()} is in DTS and will be converted to EAC3.");
                audioStream.SetCodec(AudioCodec.eac3);
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"Audio stream {audioStream.GetDisplayString()} is [yellow bold]not[/] in DTS and will be kept as-is.");
                audioStream.CopyStream();
            }
        }

        var subtitleStreams = _mediaInfo.SubtitleStreams.ToArray();
        List<ISubtitleStream> selectedSubtitleStreams = [];
        if (subtitleStreams.Length > 0)
        {
            selectedSubtitleStreams = AnsiConsole.Prompt(
                new MultiSelectionPrompt<ISubtitleStream>()
                    .Title("Select subtitle streams to keep")
                    .AddChoices(_mediaInfo.SubtitleStreams)
                    .UseConverter(s => "Language: " + s.Language)
            );
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Video file does not contain any subtitle streams.[/]");
        }

        foreach (var subtitleStream in subtitleStreams)
        {
            if (selectedSubtitleStreams.Contains(subtitleStream))
            {
                subtitleStream.SetCodec(SubtitleCodec.copy);
            }
        }
        
        foreach (var subtitleStream in selectedSubtitleStreams)
        {
            Console.WriteLine($"Subtitle stream {subtitleStream.Index} is in {subtitleStream.Language} and will be kept.");
        }

        var acceptedSubtitleLanguages = new[] {"dan", "eng"};
        
    }
}


file static class AudioStreamExtensions
{
    public static string GetDisplayString(this IAudioStream stream)
    {
        return $"{stream.Language}, {stream.Bitrate} kb/s, {stream.SampleRate}.0 kHz, {stream.Channels} channels, {stream.Codec}";
    }
    
    public static bool IsDts(this IAudioStream stream)
    {
        return stream.Codec.Contains(AudioCodec.dts.ToString(), StringComparison.InvariantCultureIgnoreCase);
    }
}