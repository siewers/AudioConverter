using Spectre.Console;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace AudioConverter.Prompts;

internal sealed class SubtitleStreamPrompt
{
    private readonly IAnsiConsole _console;
    private readonly IReadOnlyCollection<ISubtitleStream> _subtitleStreams;

    public SubtitleStreamPrompt(IAnsiConsole console, IReadOnlyCollection<ISubtitleStream> subtitleStreams)
    {
        _console = console;
        _subtitleStreams = subtitleStreams;
    }

    public async Task<IReadOnlyList<ISubtitleStream>> SelectSubtitles(CancellationToken cancellationToken)
    {
        var subtitleStreams = _subtitleStreams.ToArray();
        List<ISubtitleStream> selectedSubtitleStreams = [];

        if (subtitleStreams.Length > 0)
        {
            var prompt = new MultiSelectionPrompt<ISubtitleStream>()
                .Title("Select subtitle streams to keep")
                .NotRequired()
                .PageSize(15)
                .UseConverter(s => s.ToDisplayString());

            foreach (var subtitleStream in subtitleStreams)
            {
                var choice = prompt.AddChoice(subtitleStream);

                if (subtitleStream.IsPreferredLanguage())
                {
                    choice.Select();
                }
            }

            selectedSubtitleStreams = await prompt.ShowAsync(_console, cancellationToken);
        }

        if (subtitleStreams.Length > 0)
        {
            _console.MarkupLine("Subtitle streams:");
        }

        foreach (var subtitleStream in subtitleStreams)
        {
            _console.MarkupInterpolated($" - [bold]{subtitleStream.ToDisplayString()}[/] ");

            if (selectedSubtitleStreams.Contains(subtitleStream))
            {
                subtitleStream.SetCodec(SubtitleCodec.copy);
                _console.MarkupLine("will be [green underline]kept[/].");
            }
            else
            {
                _console.MarkupLine("will be [red underline]removed[/].");
            }
        }

        return selectedSubtitleStreams;
    }
}