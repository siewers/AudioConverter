using Spectre.Console;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace AudioConverter;

internal sealed class Converter
{
    private readonly IAnsiConsole _console;
    private readonly IMediaInfo _mediaInfo;

    public Converter(IAnsiConsole console, IMediaInfo mediaInfo)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
    }

    public async Task Convert(CancellationToken cancellationToken = default)
    {
        var conversion = FFmpeg.Conversions.New();

        foreach (var videoStream in _mediaInfo.VideoStreams)
        {
            videoStream.SetCodec(VideoCodec.copy);
            conversion.AddStream(videoStream);
        }

        var audioStreams = _mediaInfo.AudioStreams.ToArray();
        List<IAudioStream> selectedAudioStreams;

        if (audioStreams.Length > 0)
        {
            var audioStreamsPrompt = new MultiSelectionPrompt<IAudioStream>()
                .Title("Select audio streams to convert")
                .PageSize(15)
                .UseConverter(stream => stream.ToDisplayString());

            foreach (var audioStream in audioStreams)
            {
                var choice = audioStreamsPrompt.AddChoice(audioStream);

                if ((audioStream.IsDts() || audioStream.IsDolby()) && audioStream.IsPreferredLanguage())
                {
                    choice.Select();
                }
            }

            selectedAudioStreams = await audioStreamsPrompt.ShowAsync(_console, cancellationToken);
        }
        else
        {
            _console.MarkupLine("[red]No audio streams found in file - exiting.[/]");
            return;
        }

        _console.MarkupLine("Audio stream:");

        foreach (var audioStream in audioStreams)
        {
            _console.MarkupInterpolated($" - [bold]{audioStream.ToDisplayString()}[/] ");

            if (selectedAudioStreams.Contains(audioStream))
            {
                conversion.AddStream(audioStream);

                if (audioStream.IsDts())
                {
                    _console.MarkupLineInterpolated($"is in DTS and will be [green underline]converted to EAC3[/].");
                    audioStream.SetCodec(AudioCodec.eac3);
                }
                else
                {
                    _console.MarkupLineInterpolated($"will be [yellow underline]copied as-is[/].");
                    audioStream.CopyStream();
                }
            }
            else
            {
                _console.MarkupLineInterpolated($"will be [red underline]removed[/].");
            }
        }

        var subtitleStreams = _mediaInfo.SubtitleStreams.ToArray();
        List<ISubtitleStream> selectedSubtitleStreams = [];

        if (subtitleStreams.Length > 0)
        {
            var prompt = new MultiSelectionPrompt<ISubtitleStream>()
                .Title("Select subtitle streams to keep")
                .NotRequired()
                .PageSize(15)
                .UseConverter(s => s.ToDisplayString());

            foreach (var subtitleStream in subtitleStreams)
            {
                var choice = prompt.AddChoice(subtitleStream);

                if (subtitleStream.IsPreferredLanguage())
                {
                    choice.Select();
                }
            }

            selectedSubtitleStreams = await prompt.ShowAsync(_console, cancellationToken);
        }

        if (subtitleStreams.Length > 0)
        {
            _console.MarkupLine("Subtitle streams:");
        }

        foreach (var subtitleStream in subtitleStreams)
        {
            _console.MarkupInterpolated($" - [bold]{subtitleStream.ToDisplayString()}[/] ");

            if (selectedSubtitleStreams.Contains(subtitleStream))
            {
                conversion.AddStream(subtitleStream.SetCodec(SubtitleCodec.copy));
                _console.MarkupLine("will be [green underline]kept[/].");
            }
            else
            {
                _console.MarkupLine("will be [red underline]removed[/].");
            }
        }

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
                    _console.MarkupLineInterpolated($"Conversion completed in [green]{conversionResult.Duration.ToHumanTimeString()}[/].");
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