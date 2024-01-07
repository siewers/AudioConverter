using Spectre.Console.Cli;

namespace AudioConverter;

internal static class Program
{
    private async static Task<int> Main(string[] args)
    {
        return await new CommandApp<ConvertCommand>().RunAsync(args);
    }
}