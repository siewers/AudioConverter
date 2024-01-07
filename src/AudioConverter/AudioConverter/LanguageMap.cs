using System.Reflection;
using System.Text;

namespace AudioConverter;

internal static class LanguageMap
{
    private static readonly Dictionary<string, string> Cache = new();
    private static readonly Stream LanguageMapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AudioConverter.LanguageMap.txt")!;

    public static string GetLanguageName(string languageCode)
    {
        if (Cache.TryGetValue(languageCode, out var languageName))
        {
            return languageName;
        }

        languageName = FindLanguageName(languageCode);
        Cache[languageCode] = languageName;
        return languageName;
    }

    private static string FindLanguageName(string languageCode)
    {
        LanguageMapStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(LanguageMapStream, Encoding.UTF8, false, 64, true);

        while (streamReader.ReadLine() is { } entry)
        {
            if (entry.StartsWith(languageCode, StringComparison.InvariantCultureIgnoreCase))
            {
                return entry[(languageCode.Length + 1)..];
            }
        }

        return languageCode;
    }
}