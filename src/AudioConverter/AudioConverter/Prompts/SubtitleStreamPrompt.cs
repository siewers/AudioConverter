using Spectre.Console;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace AudioConverter.Prompts;

internal sealed class SubtitleStreamPrompt(IAnsiConsole console, IReadOnlyCollection<ISubtitleStream> streams)
{
    public async ValueTask<IReadOnlyList<ISubtitleStream>> SelectSubtitles(CancellationToken cancellationToken)
    {
        var subtitleStreams = streams.ToArray();
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

            selectedSubtitleStreams = await prompt.ShowAsync(console, cancellationToken);
        }

        if (subtitleStreams.Length > 0)
        {
            console.MarkupLine("Selected subtitle streams:");
        }

        foreach (var subtitleStream in subtitleStreams)
        {
            console.MarkupInterpolated($" - [bold]{subtitleStream.ToDisplayString()}[/] will be ");

            if (selectedSubtitleStreams.Contains(subtitleStream))
            {
                if (subtitleStream.Codec != SubtitleCodec.srt.ToString())
                {
                    subtitleStream.SetCodec(SubtitleCodec.srt);
                    console.MarkupLine("[yellow underline]converted to SRT[/].");
                }
                else
                {
                    console.MarkupLine("[green underline]kept[/].");
                }
            }
            else
            {
                console.MarkupLine("[red underline]removed[/].");
            }
        }

        return selectedSubtitleStreams;
    }
}