using AudioConverter.Prompts;
using Spectre.Console;
using Xabe.FFmpeg;

namespace AudioConverter;

internal sealed class Converter(IAnsiConsole console, IMediaInfo mediaInfo)
{
    private readonly AudioStreamPrompt _audioStreamPrompt = new(console, mediaInfo.AudioStreams.ToArray());
    private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));
    private readonly IMediaInfo _mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
    private readonly SubtitleStreamPrompt _subtitleStreamPrompt = new(console, mediaInfo.SubtitleStreams.ToArray());

    public async Task Convert(string outputFilePath, CancellationToken cancellationToken)
    {
        var conversion = FFmpeg.Conversions.New().SetOutput(outputFilePath);

        foreach (var videoStream in _mediaInfo.VideoStreams)
        {
            videoStream.SetCodec(VideoCodec.copy);
            conversion.AddStream(videoStream);
        }

        _console.MarkupLine("[green][bold]Media info[/][/]");
        _console.MarkupLineInterpolated($"[bold]Input file:[/] {_mediaInfo.Path}");
        _console.MarkupLineInterpolated($"[bold]Size:[/] {_mediaInfo.Size.ToFileSizeString()}");
        _console.MarkupLineInterpolated($"[bold]Duration:[/] {_mediaInfo.Duration.ToDurationString()}");

        var selectedAudioStreams = await _audioStreamPrompt.SelectAudioStreams(cancellationToken);
        conversion.AddStream(selectedAudioStreams);

        var selectedSubtitleStreams = await _subtitleStreamPrompt.SelectSubtitles(cancellationToken);
        conversion.AddStream(selectedSubtitleStreams);

        await ExecuteConversion(conversion, cancellationToken);
    }

    private async Task ExecuteConversion(IConversion conversion, CancellationToken cancellationToken)
    {
        var progress = new Progress(_console).AutoClear(false)
                                             .Columns(
                                                      [
                                                          new ProgressBarColumn(),
                                                          new PercentageColumn(),
                                                          new TaskDescriptionColumn(),
                                                          new RemainingTimeColumn(),
                                                      ]
                                                     );

        var fileSize = new FileInfo(_mediaInfo.Path).Length;

        await progress.StartAsync(async context =>
                                  {
                                      var conversionTask = context.AddTask(GetFileSizeProgressString(0));

                                      conversion.OnProgress += (_, args) =>
                                                               {
                                                                   conversionTask.Description = GetFileSizeProgressString(args.Percent);
                                                                   conversionTask.Value(args.Percent);
                                                               };

                                      var conversionResult = await conversion.Start(cancellationToken);
                                      _console.MarkupLineInterpolated($"Conversion completed in [green]{conversionResult.Duration.ToDurationString()}[/].");
                                  }
                                 );

        AskForOverwrite(conversion.OutputFilePath);
        return;

        string GetFileSizeProgressString(long progressPercent)
        {
            return $"[green]{(fileSize * progressPercent / 100).ToFileSizeString()}[/] of [green]{fileSize.ToFileSizeString()}[/]";
        }
    }

    private void AskForOverwrite(string outputFilePath)
    {
        if (_console.Confirm("Do you want to overwrite the original file?"))
        {
            _console.MarkupLine("Moving file, please wait...");
            File.Move(outputFilePath, _mediaInfo.Path, true);
        }
        else
        {
            _console.MarkupLineInterpolated($"Output file: [green]{outputFilePath}[/]");
        }

        _console.MarkupLine("[green]Done![/]");
    }
}
