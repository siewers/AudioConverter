using Spectre.Console;
using Spectre.Console.Cli;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;

namespace AudioConverter;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        var cancellationTokenSource = new ConsoleAppCancellationTokenSource();
        var console = AnsiConsole.Console;
        var mediaInfo = await FFmpeg.GetMediaInfo(settings.VideoFilePath);
        var converter = new Converter(console, mediaInfo);

        try
        {
            await converter.Convert(cancellationTokenSource.Token);
        }
        catch (ConversionException ex)
        {
            console.WriteLine();
            console.MarkupLine("[red]Conversion failed![/]");
            console.MarkupLine($"[red]{ex.Message}[/]");
        }
        catch (OperationCanceledException)
        {
            console.WriteLine();
            console.MarkupLine("[red]Conversion cancelled![/]");
        }
        catch (Exception ex)
        {
            console.WriteLine();
            console.MarkupLine("[red]Conversion failed![/]");
            console.MarkupLine($"[red]{ex.Message}[/]");
            console.MarkupLine($"[red]{ex.StackTrace}[/]");
        }

        return 0;
    }
}