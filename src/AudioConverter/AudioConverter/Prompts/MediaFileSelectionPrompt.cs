using Spectre.Console;

namespace AudioConverter.Prompts;

public class MediaFileSelectionPrompt(IAnsiConsole console)
{
    private readonly string[] _validExtensions = [".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm"];

    public async Task<FileInfo?> GetMediaFile(DirectoryInfo workingDirectory, CancellationToken cancellationToken)
    {
        var validFiles = GetValidFiles(workingDirectory).ToArray();

        switch (validFiles.Length)
        {
            case 0:
                return null;
            case 1:
                console.MarkupLine($"Found video file: [bold]{validFiles[0].Name}[/]");
                return validFiles[0];
            default:
            {
                console.MarkupLine("Multiple video files found");
                var prompt = new SelectionPrompt<FileInfo>()
                    .Title("Select video file to convert")
                    .AddChoices(validFiles)
                    .UseConverter(file => file.Name);

                return await prompt.ShowAsync(console, cancellationToken);
            }
        }
    }

    private IEnumerable<FileInfo> GetValidFiles(DirectoryInfo workingDirectory)
    {
        return workingDirectory.EnumerateFiles()
            .Where(IsValidFile)
            .OrderBy(file => file.Name);
    }

    private bool IsValidFile(FileSystemInfo file)
    {
        return _validExtensions.Contains(file.Extension);
    }
}