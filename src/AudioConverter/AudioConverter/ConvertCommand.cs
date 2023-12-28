using AudioConverter;
using Xabe.FFmpeg;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        
        var mediaInfo = await FFmpeg.GetMediaInfo(settings.VideoFilePath);
        var converter = new Converter(mediaInfo);
        await converter.Convert();
        return 0;
    }
}