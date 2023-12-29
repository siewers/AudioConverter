using Spectre.Console;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace AudioConverter;

internal sealed class Converter
{
    private HashSet<string> _acceptedSubtitleLanguages = ["dan", "eng"];
    private readonly IMediaInfo _mediaInfo;

    public Converter(IMediaInfo mediaInfo)
    {
        _mediaInfo = mediaInfo;
    }

    public async Task Convert()
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
                .UseConverter(stream => $"({stream.Index}) {stream.ToDisplayString()}");

            foreach (var audioStream in audioStreams)
            {
                var choice = audioStreamsPrompt.AddChoice(audioStream);
                if (audioStream.IsDts())
                {
                    choice.Select();
                }
            }

            selectedAudioStreams = AnsiConsole.Prompt(audioStreamsPrompt);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No audio streams found in file.[/]");
            return;
        }

        foreach (var audioStream in selectedAudioStreams)
        {
            if (audioStream.IsDts())
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[green]Audio stream {audioStream.ToDisplayString()} is in DTS and will be converted to EAC3.[/]");
                audioStream.SetCodec(AudioCodec.eac3);
                conversion.AddStream(audioStream);
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow bold]Audio stream {audioStream.ToDisplayString()} is not in DTS and will be kept as-is.[/]");
                audioStream.CopyStream();
            }
        }

        var subtitleStreams = _mediaInfo.SubtitleStreams.ToArray();
        List<ISubtitleStream> selectedSubtitleStreams = [];
        if (subtitleStreams.Length > 0)
        {
            var prompt = new MultiSelectionPrompt<ISubtitleStream>()
                .Title("Select subtitle streams to keep")
                .UseConverter(s => "Language: " + s.Language);

            foreach (var subtitleStream in subtitleStreams)
            {
                var choice = prompt.AddChoice(subtitleStream);
                if (_acceptedSubtitleLanguages.Contains(subtitleStream.Language))
                {
                    choice.Select();
                }
            }

            selectedSubtitleStreams = AnsiConsole.Prompt(prompt);
        }

        conversion.AddStream(selectedSubtitleStreams);
        foreach (var subtitleStream in subtitleStreams)
        {
            if (selectedSubtitleStreams.Contains(subtitleStream))
            {
                subtitleStream.SetCodec(SubtitleCodec.copy);
            }
        }
        var outputFileExtension = Path.GetExtension(_mediaInfo.Path);
        var outputFileName = $"{Path.GetFileNameWithoutExtension(_mediaInfo.Path)}-converted-{DateTime.Now.Ticks}{outputFileExtension}";

        var outputFilePath = Path.Combine(Path.GetDirectoryName(_mediaInfo.Path)!, outputFileName);
        conversion.SetOutput(outputFilePath);

        var progress = AnsiConsole.Progress()
            .AutoClear(true)
            .Columns([
                new ProgressBarColumn(),
                new PercentageColumn(),
                new TaskDescriptionColumn(),
                new RemainingTimeColumn()
            ]);
        await progress.StartAsync(async context =>
        {
            var conversionTask = context.AddTask("[green]Converting...[/]")
                .IsIndeterminate(false)
                .MaxValue(100);

            conversion.OnProgress += (_, args) => { conversionTask.Value(args.Percent); };

            try
            {
                var conversionResult = await conversion.Start();
                AnsiConsole.MarkupLineInterpolated($"Conversion completed in [green]{conversionResult.Duration.ToHumanTimeString()}[/].");
                AnsiConsole.MarkupLineInterpolated($"Output file: [green]{outputFilePath}[/]");
            }
            catch (ConversionException ex)
            {
                AnsiConsole.MarkupLine("[red]Conversion failed![/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Conversion failed![/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                AnsiConsole.MarkupLine($"[red]{ex.StackTrace}[/]");
            }
        });
    }
}