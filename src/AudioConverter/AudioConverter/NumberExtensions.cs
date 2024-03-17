using System.Globalization;

namespace AudioConverter;

internal static class NumberExtensions
{
    private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
    
    public static string ToFileSizeString(this long value)
    {
        double size = value;
        var index = 0;

        while (size >= 1024 && index < SizeSuffixes.Length -1)
        {
            size/= 1024;
            index++;
        }

        return $"{size.ToString("0.##", CultureInfo.InvariantCulture)} {SizeSuffixes[index]}";
    }
}
