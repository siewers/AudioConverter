using Spectre.Console;

namespace AudioConverter;

internal sealed class BinarySpinner : Spinner
{
    private BinarySpinner()
    {
    }

    public static Spinner Instance { get; } = new BinarySpinner();

    public override TimeSpan Interval => TimeSpan.FromMilliseconds(500);

    public override bool IsUnicode => false;

    /// <summary>
    ///     Generates frames consisting of 5 random 0s and 1s.
    /// </summary>
    public override IReadOnlyList<string> Frames => Enumerable.Range(0, 100)
        .Select(_ => new string(Enumerable.Range(0, 5)
                .Select(_ => Random.Shared.Next(2).ToString()[0]).ToArray()
            )
        ).ToArray();
}