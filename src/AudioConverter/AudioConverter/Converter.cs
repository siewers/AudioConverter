using AudioConverter.Prompts;
using Spectre.Console;
using Xabe.FFmpeg;

namespace AudioConverter;

internal sealed class Converter
{
    private readonly AudioStreamPrompt _audioStreamPrompt;
    private readonly IAnsiConsole _console;
    private readonly IMediaInfo _mediaInfo;
    private readonly SubtitleStreamPrompt _subtitleStreamPrompt;

    public Converter(IAnsiConsole console, IMediaInfo mediaInfo)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
        _audioStreamPrompt = new AudioStreamPrompt(console, mediaInfo.AudioStreams.ToArray());
        _subtitleStreamPrompt = new SubtitleStreamPrompt(console, mediaInfo.SubtitleStreams.ToArray());
    }

    public async Task Convert(CancellationToken cancellationToken = default)
    {
        var conversion = FFmpeg.Conversions.New();

        foreach (var videoStream in _mediaInfo.VideoStreams)
        {
            videoStream.SetCodec(VideoCodec.copy);
            conversion.AddStream(videoStream);
        }

        var selectedAudioStreams = await _audioStreamPrompt.SelectAudioStreams(cancellationToken);
        conversion.AddStream(selectedAudioStreams);

        var selectedSubtitleStreams = await _subtitleStreamPrompt.SelectSubtitles(cancellationToken);
        conversion.AddStream(selectedSubtitleStreams);

        await ExecuteConversion(conversion, cancellationToken);
    }

    private async Task ExecuteConversion(IConversion conversion, CancellationToken cancellationToken)
    {
        var outputFilePath = GetOutputFilePath();
        conversion.SetOutput(outputFilePath);
        var progress = new Progress(_console)
            .AutoClear(true)
            .AutoRefresh(true)
            .Columns(
                [
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn(BinarySpinner.Instance),
                    new TaskDescriptionColumn(),
                    new RemainingTimeColumn(),
                ]
            );

        await progress
            .StartAsync(
                async context =>
                {
                    var conversionTask = context.AddTask("[green]Converting... [/][grey](press Ctrl+C to cancel)[/]")
                        .IsIndeterminate(false)
                        .MaxValue(100);

                    conversion.OnProgress += (_, args) => { conversionTask.Value(args.Percent); };
                    var conversionResult = await conversion.Start(cancellationToken);
                    _console.MarkupLineInterpolated($"Conversion completed in [green]{conversionResult.Duration.ToDurationString()}[/].");
                    _console.MarkupLineInterpolated($"Output file: [green]{outputFilePath}[/]");
                }
            );
    }

    private string GetOutputFilePath()
    {
        var outputFileExtension = Path.GetExtension(_mediaInfo.Path);
        var outputFileName = $"{Path.GetFileNameWithoutExtension(_mediaInfo.Path)}-converted-{DateTime.Now.Ticks}{outputFileExtension}";
        return Path.Combine(Path.GetDirectoryName(_mediaInfo.Path)!, outputFileName);
    }
}