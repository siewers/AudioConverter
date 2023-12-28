namespace AudioConverter;

internal sealed class ConvertCommandSettings : CommandSettings
{
    public ConvertCommandSettings(string videoFilePath)
    {
        VideoFilePath = Environment.ExpandEnvironmentVariables(videoFilePath);
    }

    [CommandArgument(0, "<video file path>")]
    public required string VideoFilePath { get; init; }

    public override ValidationResult Validate()
    {
        return File.Exists(VideoFilePath) 
            ? ValidationResult.Success() 
            : ValidationResult.Error($"File {VideoFilePath} does not exist.");
    }
}