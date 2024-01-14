using Spectre.Console;
using Xabe.FFmpeg;

namespace AudioConverter.Prompts;

internal sealed class AudioStreamPrompt
{
    private readonly IReadOnlyList<IAudioStream> _audioStreams;
    private readonly IAnsiConsole _console;

    public AudioStreamPrompt(IAnsiConsole console, IReadOnlyList<IAudioStream> audioStreams)
    {
        _console = console;
        _audioStreams = audioStreams;
    }

    public async Task<IReadOnlyList<IAudioStream>> SelectAudioStreams(CancellationToken cancellationToken)
    {
        List<IAudioStream> selectedAudioStreams = [];

        switch (_audioStreams.Count)
        {
            case > 1:
            {
                var audioStreamsPrompt = new MultiSelectionPrompt<IAudioStream>()
                    .Title("Select audio streams to convert")
                    .PageSize(15)
                    .UseConverter(stream => stream.ToDisplayString());

                foreach (var audioStream in _audioStreams)
                {
                    var choice = audioStreamsPrompt.AddChoice(audioStream);

                    if ((audioStream.IsDts() || audioStream.IsDolby()) && audioStream.IsPreferredLanguage())
                    {
                        choice.Select();
                    }
                }

                selectedAudioStreams = await audioStreamsPrompt.ShowAsync(_console, cancellationToken);
                break;
            }
            case 1:
                // If there is only one audio stream, we don't need to ask the user to select it
                selectedAudioStreams.AddRange(_audioStreams);
                break;
        }

        if (selectedAudioStreams.Count == 0 || selectedAudioStreams.All(s => !s.IsDts()))
        {
            _console.MarkupLine("[red]No audio streams to convert - exiting.[/]");
            Environment.Exit(0);
        }

        _console.MarkupLine("Selected audio stream:");

        foreach (var audioStream in _audioStreams)
        {
            _console.MarkupInterpolated($" - [bold]{audioStream.ToDisplayString()}[/] ");

            if (selectedAudioStreams.Contains(audioStream))
            {
                if (audioStream.IsDts())
                {
                    _console.MarkupLineInterpolated($"is in DTS and will be [green underline]converted to EAC3[/].");
                    audioStream.SetCodec(AudioCodec.eac3);
                }
                else
                {
                    _console.MarkupLineInterpolated($"will be [yellow underline]copied as-is[/].");
                    audioStream.CopyStream();
                }
            }
            else
            {
                _console.MarkupLineInterpolated($"will be [red underline]removed[/].");
            }
        }

        return selectedAudioStreams;
    }
}