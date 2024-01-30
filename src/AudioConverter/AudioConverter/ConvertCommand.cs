using AudioConverter.Prompts;
using Spectre.Console;
using Spectre.Console.Cli;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;

namespace AudioConverter;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    private readonly IAnsiConsole _console;
    private readonly MediaFileSelectionPrompt _mediaFileSelectionPrompt;

    public ConvertCommand()
    {
        _console = AnsiConsole.Console;
        _mediaFileSelectionPrompt = new MediaFileSelectionPrompt(_console);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        var videoFile = settings.VideoFile is null
            ? await _mediaFileSelectionPrompt.GetMediaFile(settings.WorkingDirectory, settings.CancellationToken)
            : null;

        if (videoFile is null)
        {
            _console.MarkupLine("[red]No video file selected - exiting.[/]");
            return -1;
        }

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoFile.FullName);
            var converter = new Converter(_console, mediaInfo);
            await converter.Convert(settings.CancellationToken);
        }
        catch (ConversionException ex)
        {
            _console.WriteLine();
            _console.MarkupLine("[red]Conversion failed![/]");
            _console.MarkupLine($"[red]{ex.Message}[/]");
        }
        catch (OperationCanceledException)
        {
            _console.WriteLine();
            _console.MarkupLine("[red]Conversion cancelled![/]");
        }
        catch (Exception ex)
        {
            _console.WriteLine();
            _console.MarkupLine("[red]Conversion failed![/]");
            _console.WriteException(ex);
        }

        return 0;
    }
}