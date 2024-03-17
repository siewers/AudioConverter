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

    private async Task<FileInfo?> TryGetVideoFilePath(ConvertCommandSettings settings)
    {
        try
        {
            return settings.VideoFile ?? await _mediaFileSelectionPrompt.GetMediaFile(settings.WorkingDirectory, settings.CancellationToken);
        }
        catch (TaskCanceledException)
        {
        }

        return null;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        var videoFile = await TryGetVideoFilePath(settings);

        if (videoFile is null)
        {
            _console.MarkupLine("[red]No video file selected - exiting.[/]");
            return -1;
        }

        var outputVideoFilePath = GetOutputVideoFilePath(videoFile.FullName);

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(videoFile.FullName);
            var converter = new Converter(_console, mediaInfo);
            await converter.Convert(outputVideoFilePath, settings.CancellationToken);
            return 0;
        }
        catch (Exception ex)
        {
            _console.WriteLine();

            switch (ex)
            {
                case TaskCanceledException:
                case OperationCanceledException:
                    _console.MarkupLine("[yellow]Conversion cancelled![/]");
                    break;
                case ConversionException:
                    _console.MarkupLine("[red]Conversion failed![/]");
                    _console.WriteException(ex);
                    break;
                default:
                    _console.MarkupLine("[red]Unknown error during conversion![/]");
                    _console.WriteException(ex);
                    break;
            }

            await DeleteFile(outputVideoFilePath);
            return -1;
        }
    }

    private static string GetOutputVideoFilePath(string videoFilePath)
    {
        var outputFileExtension = Path.GetExtension(videoFilePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}-converted-{timestamp}{outputFileExtension}";
        return Path.Combine(Path.GetDirectoryName(videoFilePath)!, outputFileName);
    }

    private async static Task DeleteFile(string temporaryVideoFilePath)
    {
        while (File.Exists(temporaryVideoFilePath))
        {
            try
            {
                File.Delete(temporaryVideoFilePath);
            }
            catch
            {
                // Ignored: File is still in use
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }
    }
}
