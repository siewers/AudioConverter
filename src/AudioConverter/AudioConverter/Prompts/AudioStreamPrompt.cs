using Spectre.Console;
using Xabe.FFmpeg;

namespace AudioConverter.Prompts;

internal sealed class AudioStreamPrompt(IAnsiConsole console, IReadOnlyCollection<IAudioStream> audioStreams)
{
    public async Task<IReadOnlyList<IAudioStream>> SelectAudioStreams(CancellationToken cancellationToken)
    {
        List<IAudioStream> selectedAudioStreams = [];

        switch (audioStreams.Count)
        {
            case 0:
                break;
            case 1:
                // If there is only one audio stream, we don't need to ask the user to select it
                selectedAudioStreams.AddRange(audioStreams);
                break;
            default:
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

                selectedAudioStreams = await audioStreamsPrompt.ShowAsync(console, cancellationToken);
                break;
            }
        }

        if (selectedAudioStreams.Count == 0)
        {
            console.MarkupLine("[red]No audio streams to convert - exiting.[/]");
            Environment.Exit(0);
        }

        console.MarkupLine("Selected audio stream:");

        foreach (var audioStream in audioStreams)
        {
            console.MarkupInterpolated($" - [bold]{audioStream.ToDisplayString()}[/] ");

            if (selectedAudioStreams.Contains(audioStream))
            {
                if (audioStream.IsDts())
                {
                    console.MarkupLineInterpolated($"is in DTS and will be [green underline]converted to EAC3[/].");
                    audioStream.SetCodec(AudioCodec.eac3);
                }
                else
                {
                    console.MarkupLineInterpolated($"will be [yellow underline]copied as-is[/].");
                    audioStream.CopyStream();
                }
            }
            else
            {
                console.MarkupLineInterpolated($"will be [red underline]removed[/].");
            }
        }

        return selectedAudioStreams;
    }
}