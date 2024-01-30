using Spectre.Console;
using Spectre.Console.Cli;

namespace AudioConverter;

internal sealed class ConvertCommandSettings : CommandSettings
{
    public ConvertCommandSettings(string? videoFilePath)
    {
        if (videoFilePath?.Length > 0)
        {
            VideoFilePath = Environment.ExpandEnvironmentVariables(videoFilePath);
        }
    }

    public CancellationToken CancellationToken { get; } = new ConsoleAppCancellationTokenSource().Token;

    public DirectoryInfo WorkingDirectory { get; } = new(Environment.CurrentDirectory);

    [CommandArgument(0, "[video file path]")]
    public required string? VideoFilePath { get; init; }

    public FileInfo? VideoFile => string.IsNullOrEmpty(VideoFilePath)
        ? null
        : new FileInfo(VideoFilePath);

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(VideoFilePath))
        {
            // If no video file path is provided, we'll prompt the user to select a file.
            return ValidationResult.Success();
        }

        return File.Exists(VideoFilePath)
            ? ValidationResult.Success()
            : ValidationResult.Error($"File {VideoFilePath} does not exist.");
    }
}